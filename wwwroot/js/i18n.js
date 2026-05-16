/* ─────────────────────────────────────────────────────
   FILMIX — i18n (Internationalization)
   ───────────────────────────────────────────────────── */

const TRANSLATIONS = {
  vi: {
    /* ── Navbar ── */
    'nav.home':      'Trang Chủ',
    'nav.tvShows':   'TV Shows',
    'nav.movies':    'Phim Lẻ',
    'nav.newHot':    'Mới & Hot',
    'nav.watchlist': 'Danh Sách',
    'nav.login':     'Đăng Nhập',
    'nav.logout':    'Đăng Xuất',

    /* ── Hero ── */
    'hero.badge':    'Phim Mới Mỗi Tuần',
    'hero.thin':     'Trải Nghiệm',
    'hero.title':    'XEM PHIM',
    'hero.red':      'KHÔNG GIỚI HẠN',
    'hero.sub':      'Hàng nghìn bộ phim, series nổi tiếng, anime & tài liệu hấp dẫn — xem mọi lúc, mọi nơi, trên mọi thiết bị. Hủy bất cứ lúc nào.',
    'hero.cta1':     'Đăng Ký Ngay',
    'hero.cta2':     'Khám Phá Phim',
    'hero.stat1.n':  '10K+',
    'hero.stat1.l':  'Nội Dung',
    'hero.stat2.n':  '190+',
    'hero.stat2.l':  'Quốc Gia',
    'hero.stat3.n':  '4K',
    'hero.stat3.l':  'Ultra HD',
    'hero.stat4.n':  '30',
    'hero.stat4.l':  'Ngày Miễn Phí',

    /* ── Trending ── */
    'trending.title':    'Đang Hot Nhất',
    'trending.badge':    '🔥 Top 10 Hôm Nay',
    'trending.seeAll':   'Xem Tất Cả',
    'trending.collapse': 'Thu Gọn',

    /* ── Features ── */
    'feat.eyebrow':    'Tại Sao Chọn FILMIX?',
    'feat.title':      'Trải Nghiệm Không Thể So Sánh',
    'feat.sub':        'Mọi thứ bạn cần, tất cả ở một nơi.',
    'feat.tv.title':   'Xem Trên TV',
    'feat.tv.desc':    'Tương thích Smart TV, PlayStation, Xbox, Chromecast, Apple TV và hơn 1000 thiết bị khác với chất lượng 4K HDR.',
    'feat.dl.title':   'Tải Về Offline',
    'feat.dl.desc':    'Lưu phim yêu thích và xem khi không có mạng. Hoàn hảo cho chuyến đi máy bay, tàu hỏa hay mọi nơi.',
    'feat.dev.title':  'Mọi Thiết Bị',
    'feat.dev.desc':   'Stream trên điện thoại, tablet, laptop và TV cùng lúc mà không tốn thêm phí — một gói, mọi màn hình.',
    'feat.kids.title': 'Hồ Sơ Trẻ Em',
    'feat.kids.desc':  'Không gian riêng an toàn cho trẻ với nội dung được kiểm duyệt. Cha mẹ kiểm soát hoàn toàn qua PIN.',

    /* ── FAQ ── */
    'faq.eyebrow': 'Hỗ Trợ',
    'faq.title':   'Câu Hỏi Thường Gặp',
    'faq.q1': 'FILMIX là gì?',
    'faq.a1': 'FILMIX là dịch vụ streaming cung cấp hàng nghìn bộ phim, series, anime và tài liệu đoạt giải thưởng. Xem bao nhiêu tùy thích, bất cứ lúc nào — chỉ với một mức phí hàng tháng cố định, không quảng cáo, không hợp đồng ràng buộc.',
    'faq.q2': 'FILMIX có giá bao nhiêu?',
    'faq.a2': 'Chúng tôi có ba gói: Cơ Bản 79.000đ/tháng, Tiêu Chuẩn Full HD 149.000đ/tháng, và Premium 4K UHD 249.000đ/tháng (4 màn hình đồng thời). Tất cả đều có 30 ngày dùng thử miễn phí, hủy bất cứ lúc nào.',
    'faq.q3': 'Tôi có thể xem ở đâu?',
    'faq.a3': 'Xem mọi lúc, mọi nơi trên bất kỳ thiết bị nào — Smart TV, điện thoại, máy tính bảng, laptop, Apple TV hay máy chơi game. Tải ứng dụng iOS, Android hoặc Windows để xem offline khi không có mạng.',
    'faq.q4': 'Làm thế nào để hủy đăng ký?',
    'faq.a4': 'FILMIX hoàn toàn linh hoạt. Bạn có thể hủy tài khoản trực tuyến trong vài giây từ trang cài đặt tài khoản. Không có phí hủy — bắt đầu hoặc dừng bất cứ lúc nào.',
    'faq.q5': 'Nội dung có phụ đề tiếng Việt không?',
    'faq.a5': 'Có! Hơn 90% nội dung có phụ đề tiếng Việt và nhiều ngôn ngữ khác. Nhiều bộ phim còn có lồng tiếng Việt chất lượng cao. Bạn có thể chuyển đổi ngôn ngữ ngay trong khi xem.',
    'faq.q6': 'FILMIX có phù hợp cho trẻ em không?',
    'faq.a6': 'Hoàn toàn phù hợp! Hồ sơ Kids với giao diện thân thiện và nội dung được kiểm duyệt kỹ theo độ tuổi. Cha mẹ cài đặt PIN bảo vệ và giới hạn nội dung theo rating để đảm bảo an toàn cho trẻ.',

    /* ── Final CTA ── */
    'cta.eyebrow': 'Sẵn Sàng Chưa?',
    'cta.title':   'Tham Gia FILMIX Ngay Hôm Nay',
    'cta.sub':     'Hàng nghìn bộ phim, series và nhiều hơn nữa. Khám phá tất cả miễn phí trong 30 ngày đầu — không cần thẻ tín dụng.',
    'cta.btn1':    'Bắt Đầu Xem Ngay',
    'cta.btn2':    'Xem Bảng Giá',

    /* ── Welcome bar ── */
    'welcome.greeting': 'Chào mừng trở lại,',

    /* ── Auth page ── */
    'auth.back':       'Về Trang Chủ',
    'auth.login':      'Đăng Nhập',
    'auth.signup':     'Đăng Ký',
    'auth.email':      'Email',
    'auth.password':   'Mật Khẩu',
    'auth.name':       'Họ & Tên',
    'auth.confirmPw':  'Xác Nhận Mật Khẩu',
    'auth.forgot':     'Quên mật khẩu?',
    'auth.remember':   'Ghi nhớ đăng nhập',
    'auth.loginBtn':   'Đăng Nhập',
    'auth.signupBtn':  'Tạo Tài Khoản Miễn Phí',
    'auth.noAccount':  'Chưa có tài khoản?',
    'auth.signupFree': 'Đăng ký miễn phí',
    'auth.hasAccount': 'Đã có tài khoản?',
    'auth.loginNow':   'Đăng nhập ngay',
    'auth.orEmail':    'hoặc tiếp tục với email',
    'auth.orNew':      'hoặc tạo tài khoản mới',
    'auth.terms':      'Tôi đồng ý với',
    'auth.termsLink':  'Điều khoản',
    'auth.policyLink': 'Chính sách',
    'auth.namePh':     'Nguyễn Văn A',
    'auth.emailPh':    'ten@email.com',
    'auth.pwPh':       '••••••••',
    'auth.pwMinPh':    'Tối thiểu 8 ký tự',
    'auth.confirmPwPh':'Nhập lại mật khẩu',

    /* ── Toast messages ── */
    'toast.loginOk':    'Đăng nhập thành công! Chào mừng trở lại 👋',
    'toast.signupOk':   'Tài khoản đã được tạo thành công! 🎉',
    'toast.emptyFields':'Vui lòng điền đầy đủ thông tin',
    'toast.badEmail':   'Email không hợp lệ',
    'toast.pwShort':    'Mật khẩu phải có ít nhất 6 ký tự',
    'toast.pwShort8':   'Mật khẩu phải có ít nhất 8 ký tự',
    'toast.pwMismatch': 'Mật khẩu không khớp',
    'toast.needTerms':  'Bạn cần đồng ý với điều khoản sử dụng',
    'toast.social':     'Đang kết nối với {provider}...',
    'toast.saved':      'Đã lưu vào danh sách',
    'toast.removed':    'Đã xóa khỏi danh sách',

    /* ── Inline validation ── */
    'val.emailOk':     '✓ Email hợp lệ',
    'val.emailBad':    'Email không hợp lệ',
    'val.pwMatch':     '✓ Mật khẩu khớp',
    'val.pwMismatch':  'Mật khẩu không khớp',
    'pw.weak':   '⚠ Yếu',
    'pw.medium': '● Trung bình',
    'pw.good':   '◑ Khá',
    'pw.strong': '✓ Mạnh',

    /* ── Content pages ── */
    'page.tvShows':            'TV Shows',
    'page.tvShows.sub':        'Series nổi tiếng, drama và anime từ khắp thế giới',
    'page.movies':             'Phim Lẻ',
    'page.movies.sub':         'Phim điện ảnh chất lượng cao — hành động, tình cảm, kinh dị và hơn thế',
    'page.newHot':             'Mới & Hot',
    'page.newHot.sub':         'Nội dung mới nhất và đang được xem nhiều nhất tuần này',
    'page.newRelease':         'Mới Phát Hành',
    'page.trending':           'Đang Thịnh Hành',
    'page.watchlist':          'Danh Sách Của Tôi',
    'page.watchlist.sub':      'Phim và series bạn đã lưu để xem sau',
    'page.watchlist.empty':    'Danh sách của bạn đang trống',
    'page.watchlist.emptyDesc':'Lưu phim và series yêu thích để xem sau.',
    'page.watchlist.browse':   'Khám Phá Ngay',
    'page.watchlist.login':    'Đăng nhập để xem danh sách của bạn',
    'page.watchlist.loginBtn': 'Đăng Nhập',

    /* ── Shared UI ── */
    'ui.save':     'Lưu',
    'ui.saved':    'Đã lưu',
    'ui.play':     'Xem ngay',
    'ui.playBtn':  'Phát',
    'ui.details':  'Chi tiết',
    'ui.recommended':'Đề xuất cho bạn',
    'ui.popular':  'Phổ biến',
    'ui.new':      'MỚI',
    'ui.hot':      'HOT',
    'ui.filterAll':'Tất cả',
    'ui.seasons':  'Mùa',
    'ui.scroll':   'Scroll',
    'ui.myList':   'Danh sách của tôi',
  },

  en: {
    'nav.home':      'Home',
    'nav.tvShows':   'TV Shows',
    'nav.movies':    'Movies',
    'nav.newHot':    'New & Hot',
    'nav.watchlist': 'My List',
    'nav.login':     'Sign In',
    'nav.logout':    'Sign Out',
    'hero.badge':    'New Films Every Week',
    'hero.thin':     'Experience',
    'hero.title':    'UNLIMITED',
    'hero.red':      'STREAMING',
    'hero.sub':      'Thousands of movies, top series, anime & documentaries — watch anytime, anywhere, on any device. Cancel anytime.',
    'hero.cta1':     'Get Started',
    'hero.cta2':     'Explore Films',
    'hero.stat1.n':  '10K+',
    'hero.stat1.l':  'Titles',
    'hero.stat2.n':  '190+',
    'hero.stat2.l':  'Countries',
    'hero.stat3.n':  '4K',
    'hero.stat3.l':  'Ultra HD',
    'hero.stat4.n':  '30',
    'hero.stat4.l':  'Days Free',
    'trending.title':    'Trending Now',
    'trending.badge':    '🔥 Top 10 Today',
    'trending.seeAll':   'See All',
    'trending.collapse': 'Collapse',
    'feat.eyebrow':    'Why Choose FILMIX?',
    'feat.title':      'An Unmatched Experience',
    'feat.sub':        'Everything you need, all in one place.',
    'feat.tv.title':   'Watch on TV',
    'feat.tv.desc':    'Compatible with Smart TVs, PlayStation, Xbox, Chromecast, Apple TV and 1000+ devices in 4K HDR.',
    'feat.dl.title':   'Download & Watch Offline',
    'feat.dl.desc':    'Save your favorites and watch without internet. Perfect for flights, trains, or anywhere.',
    'feat.dev.title':  'All Devices',
    'feat.dev.desc':   'Stream on phone, tablet, laptop and TV simultaneously at no extra cost — one plan, every screen.',
    'feat.kids.title': 'Kids Profile',
    'feat.kids.desc':  'A safe space for kids with age-appropriate content. Parents control everything with a PIN.',
    'faq.eyebrow': 'Support',
    'faq.title':   'Frequently Asked Questions',
    'faq.q1': 'What is FILMIX?',
    'faq.a1': 'FILMIX is a streaming service with thousands of award-winning movies, series, anime and documentaries. Watch as much as you want, anytime — one fixed monthly fee, no ads, no binding contracts.',
    'faq.q2': 'How much does FILMIX cost?',
    'faq.a2': 'Three plans: Basic $3.99/month, Standard Full HD $7.99/month, and Premium 4K UHD $12.99/month (4 screens). All include a 30-day free trial, cancel anytime.',
    'faq.q3': 'Where can I watch?',
    'faq.a3': 'Anywhere — Smart TV, phone, tablet, laptop, Apple TV or gaming console. Download the iOS, Android or Windows app to watch offline.',
    'faq.q4': 'How do I cancel?',
    'faq.a4': 'Cancel online in seconds from your account settings. No cancellation fees — start or stop anytime.',
    'faq.q5': 'Is content subtitled?',
    'faq.a5': 'Yes! Over 90% of content has English subtitles and many more languages. Many films are fully dubbed. Switch languages while watching.',
    'faq.q6': 'Is FILMIX suitable for children?',
    'faq.a6': "Absolutely! The Kids profile has a friendly interface and age-appropriate content. Parents can set a PIN and restrict content by rating.",
    'cta.eyebrow': 'Ready?',
    'cta.title':   'Join FILMIX Today',
    'cta.sub':     'Thousands of movies, series and more. Explore everything free for the first 30 days — no credit card needed.',
    'cta.btn1':    'Start Watching',
    'cta.btn2':    'View Plans',
    'welcome.greeting': 'Welcome back,',
    'auth.back':       'Back to Home',
    'auth.login':      'Sign In',
    'auth.signup':     'Sign Up',
    'auth.email':      'Email',
    'auth.password':   'Password',
    'auth.name':       'Full Name',
    'auth.confirmPw':  'Confirm Password',
    'auth.forgot':     'Forgot password?',
    'auth.remember':   'Remember me',
    'auth.loginBtn':   'Sign In',
    'auth.signupBtn':  'Create Free Account',
    'auth.noAccount':  "Don't have an account?",
    'auth.signupFree': 'Sign up for free',
    'auth.hasAccount': 'Already have an account?',
    'auth.loginNow':   'Sign in now',
    'auth.orEmail':    'or continue with email',
    'auth.orNew':      'or create a new account',
    'auth.terms':      'I agree to the',
    'auth.termsLink':  'Terms',
    'auth.policyLink': 'Policy',
    'auth.namePh':     'John Smith',
    'auth.emailPh':    'you@email.com',
    'auth.pwPh':       '••••••••',
    'auth.pwMinPh':    'Minimum 8 characters',
    'auth.confirmPwPh':'Repeat password',
    'toast.loginOk':    'Signed in successfully! Welcome back 👋',
    'toast.signupOk':   'Account created successfully! 🎉',
    'toast.emptyFields':'Please fill in all fields',
    'toast.badEmail':   'Invalid email address',
    'toast.pwShort':    'Password must be at least 6 characters',
    'toast.pwShort8':   'Password must be at least 8 characters',
    'toast.pwMismatch': "Passwords don't match",
    'toast.needTerms':  'You must agree to the terms of service',
    'toast.social':     'Connecting with {provider}...',
    'toast.saved':      'Saved to list',
    'toast.removed':    'Removed from list',
    'val.emailOk':     '✓ Valid email',
    'val.emailBad':    'Invalid email',
    'val.pwMatch':     '✓ Passwords match',
    'val.pwMismatch':  "Passwords don't match",
    'pw.weak':   '⚠ Weak',
    'pw.medium': '● Fair',
    'pw.good':   '◑ Good',
    'pw.strong': '✓ Strong',
    'page.tvShows':            'TV Shows',
    'page.tvShows.sub':        'Popular series, dramas and anime from around the world',
    'page.movies':             'Movies',
    'page.movies.sub':         'High-quality films — action, romance, horror and more',
    'page.newHot':             'New & Hot',
    'page.newHot.sub':         'The latest and most-watched content this week',
    'page.newRelease':         'New Releases',
    'page.trending':           'Trending',
    'page.watchlist':          'My List',
    'page.watchlist.sub':      'Films and series you saved to watch later',
    'page.watchlist.empty':    'Your list is empty',
    'page.watchlist.emptyDesc':'Add movies and series you love to watch them later.',
    'page.watchlist.browse':   'Explore Now',
    'page.watchlist.login':    'Sign in to see your list',
    'page.watchlist.loginBtn': 'Sign In',
    'ui.save':     'Save',
    'ui.saved':    'Saved',
    'ui.play':     'Watch now',
    'ui.playBtn':  'Play',
    'ui.details':  'Details',
    'ui.recommended':'Recommended for you',
    'ui.popular':  'Popular',
    'ui.new':      'NEW',
    'ui.hot':      'HOT',
    'ui.filterAll':'All',
    'ui.seasons':  'Seasons',
    'ui.scroll':   'Scroll',
    'ui.myList':   'My List',
  },
};

['ko','ja','zh','fr','es','th'].forEach(code => {
  TRANSLATIONS[code] = { ...TRANSLATIONS.en };
});

function getCurrentLang() {
  return localStorage.getItem('filmix_lang') || 'vi';
}

function t(key, vars) {
  const lang = getCurrentLang();
  const dict = TRANSLATIONS[lang] || TRANSLATIONS.vi;
  let str = (dict[key] !== undefined) ? dict[key] : (TRANSLATIONS.vi[key] || key);
  if (vars) {
    Object.keys(vars).forEach(k => { str = str.replace('{' + k + '}', vars[k]); });
  }
  return str;
}

function applyTranslations() {
  document.querySelectorAll('[data-i18n]').forEach(el => {
    el.textContent = t(el.dataset.i18n);
  });
  document.querySelectorAll('[data-i18n-html]').forEach(el => {
    el.innerHTML = t(el.dataset.i18nHtml);
  });
  document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
    el.placeholder = t(el.dataset.i18nPlaceholder);
  });
  document.documentElement.lang = getCurrentLang();
}

function toggleLang() {
  const btn = document.getElementById('langBtn');
  const dropdown = document.getElementById('langDropdown');
  if (btn && dropdown) {
    const isOpen = dropdown.classList.contains('open');
    dropdown.classList.toggle('open', !isOpen);
    btn.classList.toggle('open', !isOpen);
  }
}

function setLanguage(code, flag, label) {
  localStorage.setItem('filmix_lang', code);
  localStorage.setItem('filmix_lang_flag', flag);
  localStorage.setItem('filmix_lang_label', label);

  const flagEl  = document.getElementById('langFlag');
  const labelEl = document.getElementById('langLabel');
  if (flagEl)  flagEl.textContent  = flag;
  if (labelEl) labelEl.textContent = label;

  document.querySelectorAll('.lang-dropdown a[data-lang]').forEach(a => {
    a.classList.toggle('active', a.dataset.lang === code);
  });

  // Close dropdown after selection
  const btn = document.getElementById('langBtn');
  const dropdown = document.getElementById('langDropdown');
  if (btn) btn.classList.remove('open');
  if (dropdown) dropdown.classList.remove('open');

  applyTranslations();
  return false;
}

// Close dropdown when clicking outside
document.addEventListener('click', (e) => {
  const switcher = document.getElementById('langSwitcher');
  if (switcher && !switcher.contains(e.target)) {
    const btn = document.getElementById('langBtn');
    const dropdown = document.getElementById('langDropdown');
    if (btn) btn.classList.remove('open');
    if (dropdown) dropdown.classList.remove('open');
  }
});

(function () {
  const code  = localStorage.getItem('filmix_lang')       || 'vi';
  const flag  = localStorage.getItem('filmix_lang_flag')  || '🇻🇳';
  const label = localStorage.getItem('filmix_lang_label') || 'Tiếng Việt';

  document.addEventListener('DOMContentLoaded', function () {
    applyTranslations();
    const flagEl  = document.getElementById('langFlag');
    const labelEl = document.getElementById('langLabel');
    if (flagEl)  flagEl.textContent  = flag;
    if (labelEl) labelEl.textContent = label;

    document.querySelectorAll('.lang-dropdown a[data-lang]').forEach(a => {
      a.classList.toggle('active', a.dataset.lang === code);
    });
  });
})();
