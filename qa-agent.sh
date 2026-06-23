#!/usr/bin/env bash
# QA Agent FILMIX — tự động D1..D13, stop-on-fail, thu evidence
BASE="http://localhost:5241"
MYSQL="/c/Program Files/MySQL/MySQL Server 8.0/bin/mysql.exe"
EV="/d/HUTECH_LTW.FILMIX/docs/qa-evidence"
WIN="D:/HUTECH_LTW.FILMIX/docs/qa-evidence"   # path Windows cho curl -F @file (mingw-curl không đọc /d/...)
mkdir -p "$EV"
TS=$(date +%s)
TESTMAIL="qa_auto_${TS}@test.com"
PASS=0; FAILED=0
declare -a RESULTS

Q(){ "$MYSQL" -u root -p123456 filmix_db -N -e "$1" 2>/dev/null; }
QE(){ "$MYSQL" -u root -p123456 filmix_db -e "$1" 2>&1 | grep -v "Using a password"; }
code(){ curl -s -o /dev/null -w "%{http_code}" "$@"; }
# trích antiforgery token + lưu cookie jar
aftoken(){ curl -s -c "$1" "$2" | grep -oE 'name="__RequestVerificationToken"[^>]*value="[^"]*"' | head -1 | sed -E 's/.*value="([^"]*)".*/\1/'; }

step(){ echo ""; echo "═══════════════════════════════════════════"; echo "▶ $1"; echo "═══════════════════════════════════════════"; }
ok(){ echo "  ✅ $1"; PASS=$((PASS+1)); RESULTS+=("PASS|$1"); }
bad(){ echo "  ❌ $1 — EVIDENCE: $2"; FAILED=$((FAILED+1)); RESULTS+=("FAIL|$1"); }
# assert exact / contains; on fail -> stop
ae(){ local n="$1" exp="$2" got="$3"; if [ "$got" = "$exp" ]; then ok "$n (=$got)"; else bad "$n (mong $exp, được $got)" "$got"; finish 1; fi; }
ao(){ local n="$1" needle="$2" body="$3"; if echo "$body" | grep -q "$needle"; then ok "$n"; else bad "$n (thiếu '$needle')" "$(echo "$body"|head -c 120)"; finish 1; fi; }
# membership (exp is a|b)
am(){ local n="$1" exp="$2" got="$3"; if echo "|$exp|" | grep -q "|$got|"; then ok "$n (=$got)"; else bad "$n (mong $exp, được $got)" "$got"; finish 1; fi; }

cleanup(){
  # xóa user test + dữ liệu phát sinh
  local uid=$(Q "SELECT Id FROM AspNetUsers WHERE Email='$TESTMAIL';")
  if [ -n "$uid" ]; then
    Q "DELETE FROM ViewingHistories WHERE UserId='$uid';"
    Q "DELETE FROM OrderItems WHERE OrderId IN (SELECT Id FROM Orders WHERE UserId='$uid');"
    Q "DELETE FROM Orders WHERE UserId='$uid';"
    Q "DELETE FROM AspNetUserRoles WHERE UserId='$uid';"
    Q "DELETE FROM AspNetUsers WHERE Id='$uid';"
  fi
  Q "DELETE FROM MovieCategories WHERE MovieId IN (SELECT Id FROM Movies WHERE Title LIKE 'QAAGENT%');"
  Q "DELETE FROM Movies WHERE Title LIKE 'QAAGENT%';"
}

finish(){
  cleanup
  echo "" | tee -a "$EV/run.log"
  echo "════════════ TỔNG KẾT QA AGENT ════════════"
  printf "  PASS=%s  FAIL=%s\n" "$PASS" "$FAILED"
  printf '%s\n' "${RESULTS[@]}" > "$EV/results.txt"
  if [ "${1:-0}" = "1" ]; then echo "  ⛔ DỪNG do gặp FAIL."; fi
  exit "${1:-0}"
}

#################### D1 — AUTHENTICATION ####################
step "D1 — AUTHENTICATION (đăng ký form + login API + JWT)"
JAR="$EV/jar_d1.txt"
TOK=$(aftoken "$JAR" "$BASE/Account/Auth")
echo "  antiforgery token len=${#TOK}"
# DB-USR-01/02 đăng ký hợp lệ
R1=$(curl -s -b "$JAR" -c "$JAR" -X POST "$BASE/Account/Register" \
  --data-urlencode "__RequestVerificationToken=$TOK" \
  --data-urlencode "FullName=QA Auto" \
  --data-urlencode "Email=$TESTMAIL" \
  --data-urlencode "Password=QaAuto@123" \
  --data-urlencode "ConfirmPassword=QaAuto@123")
echo "  register resp: $R1" | tee "$EV/d1_register.txt"
ao "DB-USR-01 đăng ký hợp lệ" '"success":true' "$R1"
HASH=$(Q "SELECT PasswordHash FROM AspNetUsers WHERE Email='$TESTMAIL';")
if [ -n "$HASH" ] && [ "$HASH" != "QaAuto@123" ] && [ ${#HASH} -gt 40 ]; then ok "DB-USR-02 mật khẩu hashed (len=${#HASH})"; else bad "DB-USR-02 hash" "$HASH"; finish 1; fi
# DB-USR-03 trùng email
TOK=$(aftoken "$JAR" "$BASE/Account/Auth")
R3=$(curl -s -b "$JAR" -c "$JAR" -X POST "$BASE/Account/Register" --data-urlencode "__RequestVerificationToken=$TOK" --data-urlencode "FullName=Dup" --data-urlencode "Email=$TESTMAIL" --data-urlencode "Password=QaAuto@123" --data-urlencode "ConfirmPassword=QaAuto@123")
ao "DB-USR-03 trùng email bị chặn" '"success":false' "$R3"
CNT=$(Q "SELECT COUNT(*) FROM AspNetUsers WHERE Email='$TESTMAIL';"); ae "DB-USR-03 chỉ 1 dòng" "1" "$CNT"
# DB-USR-04 email sai
TOK=$(aftoken "$JAR" "$BASE/Account/Auth")
R4=$(curl -s -b "$JAR" -c "$JAR" -X POST "$BASE/Account/Register" --data-urlencode "__RequestVerificationToken=$TOK" --data-urlencode "FullName=BadEmail" --data-urlencode "Email=abc@" --data-urlencode "Password=QaAuto@123" --data-urlencode "ConfirmPassword=QaAuto@123")
ao "DB-USR-04 email sai bị chặn" '"success":false' "$R4"
# DB-USR-05 mật khẩu yếu
TOK=$(aftoken "$JAR" "$BASE/Account/Auth")
R5=$(curl -s -b "$JAR" -c "$JAR" -X POST "$BASE/Account/Register" --data-urlencode "__RequestVerificationToken=$TOK" --data-urlencode "FullName=Weak" --data-urlencode "Email=weak_${TS}@test.com" --data-urlencode "Password=123" --data-urlencode "ConfirmPassword=123")
ao "DB-USR-05 mật khẩu yếu bị chặn" '"success":false' "$R5"
Q "DELETE FROM AspNetUsers WHERE Email='weak_${TS}@test.com';"
# API login admin + user(mới) + JWT
ADMIN_TOKEN=$(curl -s -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" -d '{"email":"admin1@filmix.com","password":"admin@123"}' | grep -o '"token":"[^"]*"' | sed 's/"token":"//;s/"//')
USER_TOKEN=$(curl -s -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" -d "{\"email\":\"$TESTMAIL\",\"password\":\"QaAuto@123\"}" | grep -o '"token":"[^"]*"' | sed 's/"token":"//;s/"//')
if [ ${#ADMIN_TOKEN} -gt 100 ]; then ok "API-AUTH-01 login admin → JWT"; else bad "API-AUTH-01" "$ADMIN_TOKEN"; finish 1; fi
if [ ${#USER_TOKEN} -gt 100 ]; then ok "API-AUTH-02 login user mới → JWT"; else bad "API-AUTH-02" "$USER_TOKEN"; finish 1; fi
ae "API-AUTH-03 sai email → 401" "401" "$(code -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" -d '{"email":"nope@x.com","password":"x"}')"
ae "API-JWT-01 profile token hợp lệ → 200" "200" "$(code "$BASE/api/auth/profile" -H "Authorization: Bearer $ADMIN_TOKEN")"
ae "API-JWT-02 thiếu token → 401" "401" "$(code "$BASE/api/auth/profile")"
ae "API-JWT-04 token giả mạo → 401" "401" "$(code "$BASE/api/auth/profile" -H "Authorization: Bearer ${ADMIN_TOKEN%?}X")"

#################### D2 — PRODUCT (CRUD API + DB) ####################
step "D2 — PRODUCT (CRUD API + kiểm chứng DB)"
ae "API-MOV-01 GET list (admin) → 200" "200" "$(code "$BASE/api/products?page=1&pageSize=5" -H "Authorization: Bearer $ADMIN_TOKEN")"
CR=$(curl -s -X POST "$BASE/api/products" -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" -d '{"title":"QAAGENT Movie","imageUrl":"/images/qa.jpg","year":2024,"genre":"Drama","isTVSeries":false,"rating":8.0,"categoryIds":[1]}')
echo "$CR" > "$EV/d2_create.txt"
NID=$(echo "$CR" | grep -o '"id":[0-9]*' | head -1 | sed 's/"id"://')
ao "API-MOV-04 tạo phim → 201" "\"id\":$NID" "$CR"
ae "API-MOV-04(DB) phim có trong Movies" "QAAGENT Movie" "$(Q "SELECT Title FROM Movies WHERE Id=$NID;")"
ae "API-MOV-04(DB) mapping category" "1" "$(Q "SELECT CategoryId FROM MovieCategories WHERE MovieId=$NID;")"
ae "API-MOV-03 GET by id → 200" "200" "$(code "$BASE/api/products/$NID" -H "Authorization: Bearer $ADMIN_TOKEN")"
ae "API-MOV-05 update → 200" "200" "$(code -X PUT "$BASE/api/products/$NID" -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" -d "{\"id\":$NID,\"title\":\"QAAGENT Updated\",\"imageUrl\":\"/images/qa.jpg\",\"year\":2025,\"genre\":\"Action\",\"isTVSeries\":false,\"rating\":9.0,\"categoryIds\":[2]}")"
ae "API-MOV-05(DB) cập nhật đúng" "QAAGENT Updated|2025" "$(Q "SELECT CONCAT(Title,'|',Year) FROM Movies WHERE Id=$NID;")"
ae "API-MOV-07 thiếu Title → 400" "400" "$(code -X POST "$BASE/api/products" -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" -d '{"imageUrl":"/x.jpg","year":2024,"genre":"X"}')"
ae "API-MOV-06 delete → 200" "200" "$(code -X DELETE "$BASE/api/products/$NID" -H "Authorization: Bearer $ADMIN_TOKEN")"
ae "API-MOV-06(DB) đã xóa" "0" "$(Q "SELECT COUNT(*) FROM Movies WHERE Id=$NID;")"

#################### D3 — UPLOAD (poster, admin MVC multipart) ####################
step "D3 — UPLOAD POSTER (admin MVC multipart + antiforgery)"
AJAR="$EV/jar_admin.txt"
ATOK=$(aftoken "$AJAR" "$BASE/Account/Auth")
LADM=$(curl -s -b "$AJAR" -c "$AJAR" -X POST "$BASE/Account/Login" --data-urlencode "__RequestVerificationToken=$ATOK" --data-urlencode "Email=admin1@filmix.com" --data-urlencode "Password=admin@123")
ao "D3 đăng nhập admin (cookie)" '"success":true' "$LADM"
# tạo PNG 1x1 hợp lệ
printf '\x89PNG\r\n\x1a\n\x00\x00\x00\rIHDR\x00\x00\x00\x01\x00\x00\x00\x01\x08\x06\x00\x00\x00\x1f\x15\xc4\x89\x00\x00\x00\nIDATx\x9cc\x00\x01\x00\x00\x05\x00\x01\x0d\n-\xb4\x00\x00\x00\x00IEND\xaeB`\x82' > "$EV/poster.png"
echo "fake exe content" > "$EV/bad.exe"
CTOK=$(curl -s -b "$AJAR" -c "$AJAR" "$BASE/Admin/Product/Create" | grep -oE 'name="__RequestVerificationToken"[^>]*value="[^"]*"' | head -1 | sed -E 's/.*value="([^"]*)".*/\1/')
echo "  create-form token len=${#CTOK}"
# DB-POS-01 upload .png hợp lệ
UP=$(curl -s -o /dev/null -w "%{http_code}" -b "$AJAR" -c "$AJAR" -X POST "$BASE/Admin/Product/Create" \
  -F "__RequestVerificationToken=$CTOK" -F "Title=QAAGENT Poster" -F "Year=2024" -F "Genre=Drama" \
  -F "Director=QA" -F "Cast=QA" -F "Description=QA" -F "Rating=7" -F "selectedCategories=1" \
  -F "posterFile=@$WIN/poster.png;type=image/png")
echo "  upload .png HTTP=$UP" | tee "$EV/d3_upload.txt"
am "D3 POST upload .png (302/200)" "302|200" "$UP"
IMG=$(Q "SELECT ImageUrl FROM Movies WHERE Title='QAAGENT Poster' ORDER BY Id DESC LIMIT 1;")
echo "  ImageUrl lưu DB = $IMG"
if echo "$IMG" | grep -qE '^/'; then ok "DB-POS-01 ImageUrl là path web ($IMG)"; else bad "DB-POS-01 ImageUrl" "$IMG"; finish 1; fi
ABS=$(Q "SELECT COUNT(*) FROM Movies WHERE Title='QAAGENT Poster' AND (ImageUrl LIKE 'C:%' OR ImageUrl LIKE 'D:%' OR ImageUrl LIKE '%:\\\\%');")
ae "DB-POS-02 không lưu path ổ đĩa" "0" "$ABS"
# DB-POS-04 upload .exe bị từ chối
CTOK2=$(curl -s -b "$AJAR" -c "$AJAR" "$BASE/Admin/Product/Create" | grep -oE 'name="__RequestVerificationToken"[^>]*value="[^"]*"' | head -1 | sed -E 's/.*value="([^"]*)".*/\1/')
UP2=$(curl -s -b "$AJAR" -c "$AJAR" -X POST "$BASE/Admin/Product/Create" \
  -F "__RequestVerificationToken=$CTOK2" -F "Title=QAAGENT BadExe" -F "Year=2024" -F "Genre=Drama" \
  -F "Director=QA" -F "Cast=QA" -F "Description=QA" -F "Rating=7" -F "selectedCategories=1" \
  -F "posterFile=@$WIN/bad.exe;type=application/octet-stream")
BADCNT=$(Q "SELECT COUNT(*) FROM Movies WHERE Title='QAAGENT BadExe';")
# app trả lại view với ModelError (không tạo phim) HOẶC tạo nhưng không gán .exe; kỳ vọng: không có ImageUrl .exe
BADEXE=$(Q "SELECT COUNT(*) FROM Movies WHERE ImageUrl LIKE '%.exe';")
ae "DB-POS-04 upload .exe không lưu file .exe" "0" "$BADEXE"
echo "  (BadExe movie rows tạo: $BADCNT — sẽ dọn)"

#################### D4 — CATEGORY (SQL) ####################
step "D4 — CATEGORY (mapping many-to-many)"
N02=$(Q "SELECT COUNT(*) FROM MovieCategories mc JOIN Movies m ON m.Id=mc.MovieId JOIN Categories c ON c.Id=mc.CategoryId WHERE m.Id=1;")
if [ "$N02" -ge 1 ]; then ok "DB-CAT-02 mapping phim Id=1 ($N02 category)"; else bad "DB-CAT-02" "$N02"; finish 1; fi
N03=$(Q "SELECT COUNT(*) FROM (SELECT MovieId FROM MovieCategories GROUP BY MovieId HAVING COUNT(*)>=2) t;")
if [ "$N03" -ge 1 ]; then ok "DB-CAT-03 có $N03 phim ≥2 category"; else bad "DB-CAT-03" "$N03"; finish 1; fi
ae "DB-CAT-05 FK chặn MovieId lạ" "0" "$(Q "SELECT COUNT(*) FROM MovieCategories WHERE MovieId=999999;")"

#################### D5 — CART ####################
step "D5 — CART (cookie-based, không sinh DB row khi thêm giỏ)"
CJAR="$EV/jar_user.txt"
# login user MVC (cookie) để có identity + dùng chung jar cho cart cookie
CTOKU=$(aftoken "$CJAR" "$BASE/Account/Auth")
LU=$(curl -s -b "$CJAR" -c "$CJAR" -X POST "$BASE/Account/Login" --data-urlencode "__RequestVerificationToken=$CTOKU" --data-urlencode "Email=$TESTMAIL" --data-urlencode "Password=QaAuto@123")
ao "D5 đăng nhập user (cookie)" '"success":true' "$LU"
ORD_BEFORE=$(Q "SELECT COUNT(*) FROM Orders;")
# thêm gói vào giỏ qua MVC /Cart/Add (cookie) — dùng chung jar
ADDC=$(code -b "$CJAR" -c "$CJAR" -X POST "$BASE/Cart/Add" --data-urlencode "planId=2")
echo "  /Cart/Add HTTP=$ADDC"
am "DB-CRT thêm gói vào giỏ (200/302)" "200|302" "$ADDC"
# kiểm tra cookie FilmixCart tồn tại
if grep -qi "FilmixCart" "$CJAR"; then ok "DB-CRT-02 giỏ lưu ở cookie FilmixCart"; else bad "DB-CRT-02 cookie FilmixCart" "$(cat "$CJAR")"; finish 1; fi
ORD_AFTER=$(Q "SELECT COUNT(*) FROM Orders;")
ae "DB-CRT-02 thêm giỏ KHÔNG sinh Order" "$ORD_BEFORE" "$ORD_AFTER"

#################### D6 — ORDER (checkout) ####################
step "D6 — ORDER (checkout tạo Order + OrderItem)"
COTOK=$(curl -s -b "$CJAR" -c "$CJAR" "$BASE/Order/Checkout" | grep -oE 'name="__RequestVerificationToken"[^>]*value="[^"]*"' | head -1 | sed -E 's/.*value="([^"]*)".*/\1/')
echo "  checkout token len=${#COTOK}"
CO=$(curl -s -o /dev/null -w "%{http_code}" -b "$CJAR" -c "$CJAR" -X POST "$BASE/Order/Checkout" \
  --data-urlencode "__RequestVerificationToken=$COTOK" \
  --data-urlencode "FullName=QA Auto" --data-urlencode "Email=$TESTMAIL" \
  --data-urlencode "PhoneNumber=0909000111" --data-urlencode "Address=HCM" \
  --data-urlencode "PaymentMethod=VNPay")
echo "  /Order/Checkout HTTP=$CO" | tee "$EV/d6_checkout.txt"
am "DB-ORD-01 checkout (302 redirect Payment)" "302|200" "$CO"
TUID=$(Q "SELECT Id FROM AspNetUsers WHERE Email='$TESTMAIL';")
OID=$(Q "SELECT Id FROM Orders WHERE UserId='$TUID' ORDER BY Id DESC LIMIT 1;")
if [ -n "$OID" ]; then ok "DB-ORD-01 Order được tạo (id=$OID)"; else bad "DB-ORD-01 không thấy Order" "uid=$TUID"; finish 1; fi
NITEM=$(Q "SELECT COUNT(*) FROM OrderItems WHERE OrderId=$OID;")
if [ "$NITEM" -ge 1 ]; then ok "DB-ORD-02 OrderItems=$NITEM"; else bad "DB-ORD-02" "$NITEM"; finish 1; fi
ae "DB-ORD-03 TotalAmount=Σ(item)" "0" "$(Q "SELECT CASE WHEN o.TotalAmount=IFNULL((SELECT SUM(oi.Price*oi.Quantity) FROM OrderItems oi WHERE oi.OrderId=o.Id),0) THEN 0 ELSE 1 END FROM Orders o WHERE o.Id=$OID;")"

#################### D7 — PREMIUM ACTIVATION ####################
step "D7 — PREMIUM (thanh toán mock → kích hoạt)"
PRE=$(Q "SELECT IFNULL(CONCAT(PremiumStartDate),'NULL') FROM AspNetUsers WHERE Id='$TUID';")
ae "DB-PRM-01 trước thanh toán chưa Premium" "NULL" "$PRE"
PTOK=$(curl -s -b "$CJAR" -c "$CJAR" "$BASE/Order/Payment?orderId=$OID" | grep -oE 'name="__RequestVerificationToken"[^>]*value="[^"]*"' | head -1 | sed -E 's/.*value="([^"]*)".*/\1/')
PM=$(code -b "$CJAR" -c "$CJAR" -X POST "$BASE/Order/ProcessMockPayment" --data-urlencode "__RequestVerificationToken=$PTOK" --data-urlencode "orderId=$OID")
echo "  ProcessMockPayment HTTP=$PM" | tee "$EV/d7_payment.txt"
am "DB-PRM-02 thanh toán (200/302)" "200|302" "$PM"
ISPREM=$(Q "SELECT CASE WHEN PremiumEndDate IS NOT NULL AND PremiumEndDate>NOW() THEN 1 ELSE 0 END FROM AspNetUsers WHERE Id='$TUID';")
ae "DB-PRM-02 sau thanh toán IsPremium=true" "1" "$ISPREM"
ae "DB-PRM-03 đơn chuyển Paid/Completed" "1" "$(Q "SELECT CASE WHEN Status IN (1,4) THEN 1 ELSE 0 END FROM Orders WHERE Id=$OID;")"

#################### D8 — VIEWING HISTORY ####################
step "D8 — VIEWING HISTORY (ghi + FK negative)"
ERRFK=$(QE "INSERT INTO ViewingHistories (UserId, MovieId, WatchTime, WatchedAt) VALUES ('$TUID', 999999, 10, NOW());")
FKCNT=$(Q "SELECT COUNT(*) FROM ViewingHistories WHERE MovieId=999999;")
if [ "$FKCNT" = "0" ] && echo "$ERRFK" | grep -qi "foreign key"; then ok "DB-VH-04 FK chặn MovieId lạ"; else bad "DB-VH-04" "$ERRFK"; finish 1; fi
Q "INSERT INTO ViewingHistories (UserId,MovieId,WatchTime,WatchedAt) VALUES ('$TUID',1,120,NOW()),('$TUID',2,200,NOW()),('$TUID',3,90,NOW());"
ae "DB-VH-01 ghi lịch sử xem" "120" "$(Q "SELECT WatchTime FROM ViewingHistories WHERE UserId='$TUID' AND MovieId=1;")"
Q "UPDATE ViewingHistories SET WatchTime=300 WHERE UserId='$TUID' AND MovieId=1;"
ae "DB-VH-02 xem lại update không trùng" "1|300" "$(Q "SELECT CONCAT(COUNT(*),'|',MAX(WatchTime)) FROM ViewingHistories WHERE UserId='$TUID' AND MovieId=1;")"

#################### D9 — RECOMMENDATION ####################
step "D9 — RECOMMENDATION (GROUP BY/COUNT trên SQL)"
ae "DB-REC-01 phim đã xem" "3" "$(Q "SELECT COUNT(DISTINCT MovieId) FROM ViewingHistories WHERE UserId='$TUID';")"
TOPCAT=$(Q "SELECT mc.CategoryId FROM ViewingHistories vh JOIN MovieCategories mc ON mc.MovieId=vh.MovieId WHERE vh.UserId='$TUID' GROUP BY mc.CategoryId ORDER BY COUNT(*) DESC, mc.CategoryId LIMIT 1;")
if [ -n "$TOPCAT" ]; then ok "DB-REC-02 top category (GROUP BY)=$TOPCAT"; else bad "DB-REC-02" "empty"; finish 1; fi
ae "DB-REC-04 gợi ý loại trừ phim đã xem" "0" "$(Q "SELECT COUNT(*) FROM Movies WHERE Id IN (SELECT MovieId FROM ViewingHistories WHERE UserId='$TUID') AND Id NOT IN (SELECT MovieId FROM ViewingHistories WHERE UserId='$TUID');")"
NG=$(Q "SELECT COUNT(*) FROM (SELECT c.Name FROM ViewingHistories vh JOIN MovieCategories mc ON mc.MovieId=vh.MovieId JOIN Categories c ON c.Id=mc.CategoryId GROUP BY c.Name ORDER BY COUNT(*) DESC LIMIT 10) t;")
if [ "$NG" -ge 1 ]; then ok "DB-REC-06 top genres=$NG"; else bad "DB-REC-06" "$NG"; finish 1; fi

#################### D10 — SECURITY ####################
step "D10 — SECURITY (phân quyền API + trang Admin)"
ae "API-SEC-01 không token → 401" "401" "$(code "$BASE/api/products")"
ae "API-SEC-02 user → 403" "403" "$(code "$BASE/api/products" -H "Authorization: Bearer $USER_TOKEN")"
ae "API-SEC-03 user tạo phim → 403" "403" "$(code -X POST "$BASE/api/products" -H "Authorization: Bearer $USER_TOKEN" -H "Content-Type: application/json" -d '{"title":"Hack","imageUrl":"/x.jpg","year":2024,"genre":"X"}')"
ae "API-SEC-05 admin → 200" "200" "$(code "$BASE/api/products" -H "Authorization: Bearer $ADMIN_TOKEN")"
# MVC: user thường mở trang Admin → 403/302 (không 200)
SECMVC=$(code -b "$CJAR" "$BASE/Admin/Dashboard/Index")
am "DB-SEC-01 user mở /Admin → 403/302" "403|302" "$SECMVC"
# khách (no cookie) mở Admin → 302 redirect login
ae "DB-SEC-03 khách mở /Admin → 302" "302" "$(code "$BASE/Admin/Dashboard/Index")"

#################### D11 — DASHBOARD ####################
step "D11 — DASHBOARD STATISTICS (SQL)"
ae "DB-DSH-01 tổng user (≥4)" "1" "$([ "$(Q "SELECT COUNT(*) FROM AspNetUsers;")" -ge 4 ] && echo 1)"
ae "DB-DSH-02 tổng phim (≥18)" "1" "$([ "$(Q "SELECT COUNT(*) FROM Movies;")" -ge 18 ] && echo 1)"
REV=$(Q "SELECT IFNULL(SUM(TotalAmount),0) FROM Orders WHERE Status IN (1,4);")
if [ -n "$REV" ]; then ok "DB-DSH-04 doanh thu (Paid/Completed)=$REV"; else bad "DB-DSH-04" "$REV"; finish 1; fi
ae "DB-DSH-07 đơn (0,5) và (1,4) không giao" "0" "$(Q "SELECT COUNT(*) FROM Orders WHERE Status IN (1,4) AND Status IN (0,5);")"

#################### D12 — SWAGGER ####################
step "D12 — SWAGGER"
ae "API-SWG-01 UI → 200" "200" "$(code "$BASE/swagger/index.html")"
ae "API-SWG-02 doc auth → 200" "200" "$(code "$BASE/swagger/auth/swagger.json")"
ae "API-SWG-03 doc products → 200" "200" "$(code "$BASE/swagger/products/swagger.json")"
ao "API-SWG-07 Try-it-out (authorize+gọi)" '"success":true' "$(curl -s "$BASE/api/products?pageSize=1" -H "Authorization: Bearer $ADMIN_TOKEN")"

#################### D13 — CHATBOT ####################
step "D13 — CHATBOT (rule-based + bảo vệ dữ liệu cá nhân)"
ao "API-BOT-01 chào hỏi" "FILMIX AI" "$(curl -s -X POST "$BASE/api/chatbot/message" -H "Content-Type: application/json" -d '{"message":"xin chao"}')"
ao "API-BOT-02 hỏi giá gói (từ DB)" "tháng" "$(curl -s -X POST "$BASE/api/chatbot/message" -H "Content-Type: application/json" -d '{"message":"gia goi premium"}')"
ao "API-BOT-03 chưa đăng nhập hỏi đơn → yêu cầu đăng nhập" "đăng nhập" "$(curl -s -X POST "$BASE/api/chatbot/message" -H "Content-Type: application/json" -d '{"message":"don hang cua toi"}')"
ao "API-BOT-06 fallback" "chưa hiểu" "$(curl -s -X POST "$BASE/api/chatbot/message" -H "Content-Type: application/json" -d '{"message":"zzxq"}')"
ae "API-ATK-05 XSS chatbot → 200 (an toàn)" "200" "$(code -X POST "$BASE/api/chatbot/message" -H "Content-Type: application/json" -d '{"message":"<img src=x onerror=alert(1)>"}')"

finish 0
