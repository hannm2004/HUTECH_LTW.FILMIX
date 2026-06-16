using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using untitled1.Data;
using untitled1.Models.Entities;
using untitled1.Models.Settings;
using untitled1.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddMvcOptions(options =>
    {
        // Không coi các thuộc tính string non-nullable là bắt buộc ngầm định.
        // Các trường như ImageUrl, Director, Cast... là tùy chọn (controller tự gán mặc định).
        // Nếu không tắt, khi upload ảnh JS xóa trắng ô ImageUrl -> ModelState invalid -> không lưu được.
        // Validation thật sự bắt buộc vẫn do [Required] tường minh trên các DTO đảm nhiệm.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var response = untitled1.Models.DTOs.ApiResponse<object>.ErrorResponse("Dữ liệu đầu vào không hợp lệ.", errors);
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

// ── M-02 CORS Policy — restrict to known origins ─────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("FilmixPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: allow all localhost ports
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? Array.Empty<string>();
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // Auth API group
    c.SwaggerDoc("auth", new OpenApiInfo
    {
        Title        = "FILMIX – Auth API",
        Version      = "v1",
        Description  = "RESTful API xác thực và quản lý tài khoản bằng JWT cho FILMIX.",
        Contact      = new OpenApiContact { Name = "FILMIX Dev Team" }
    });

    // Cart API group
    c.SwaggerDoc("cart", new OpenApiInfo
    {
        Title        = "FILMIX – Cart API",
        Version      = "v1",
        Description  = "RESTful API quản lý giỏ hàng (Session-based) cho FILMIX.",
        Contact      = new OpenApiContact { Name = "FILMIX Dev Team" }
    });

    // Products API group
    c.SwaggerDoc("products", new OpenApiInfo
    {
        Title        = "FILMIX – Products API",
        Version      = "v1",
        Description  = "RESTful CRUD API quản lý phim/sản phẩm dành cho Admin.",
        Contact      = new OpenApiContact { Name = "FILMIX Dev Team" }
    });

    // Cookie-based auth annotation
    c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
    {
        Type   = SecuritySchemeType.ApiKey,
        In     = ParameterLocation.Cookie,
        Name   = ".AspNetCore.Identity.Application",
        Description = "ASP.NET Core Identity cookie (đăng nhập trước tại /Account/Auth, sau đó cookie sẽ được tự động gửi kèm)."
    });

    // JWT Bearer auth annotation
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập 'Bearer [space] <your token>' bên dưới.\nVí dụ: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "cookieAuth" }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // Route the controllers into correct doc groups
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out var mi)) return false;
        var controllerName = mi.DeclaringType?.Name ?? string.Empty;
        return docName switch
        {
            "auth"     => controllerName.Contains("AuthApi"),
            "cart"     => controllerName.Contains("CartApi"),
            "products" => controllerName.Contains("ProductsApi"),
            _          => false
        };
    });
});

// Register HttpContextAccessor & Session support
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Custom Repositories & Services
builder.Services.AddScoped<untitled1.Repositories.IOrderRepository, untitled1.Repositories.OrderRepository>();
builder.Services.AddScoped<untitled1.Repositories.ISubscriptionPlanRepository, untitled1.Repositories.SubscriptionPlanRepository>();
builder.Services.AddScoped<untitled1.Repositories.IUserRepository, untitled1.Repositories.UserRepository>();
builder.Services.AddScoped<untitled1.Repositories.ISubscriptionRepository, untitled1.Repositories.SubscriptionRepository>();
builder.Services.AddScoped<untitled1.Repositories.IViewingHistoryRepository, untitled1.Repositories.ViewingHistoryRepository>();
builder.Services.AddScoped<untitled1.Repositories.ILogRepository, untitled1.Repositories.LogRepository>();
builder.Services.AddScoped<untitled1.Services.ICartService, untitled1.Services.CartService>();
builder.Services.AddScoped<untitled1.Services.IOrderService, untitled1.Services.OrderService>();
builder.Services.AddScoped<untitled1.Services.IAdminService, untitled1.Services.AdminService>();
builder.Services.AddScoped<untitled1.Services.IRecommendationService, untitled1.Services.RecommendationService>();
builder.Services.AddScoped<untitled1.Services.ILogService, untitled1.Services.LogService>();

// Register Email service with environment variables override and fallback warnings (H-01)
builder.Services.Configure<untitled1.Models.Settings.EmailSettings>(options =>
{
    builder.Configuration.GetSection("EmailSettings").Bind(options);
    // Ưu tiên biến môi trường (an toàn, không commit secret); nếu không có thì GIỮ NGUYÊN
    // Password đã bind từ appsettings.json. Không ghi đè bằng giá trị dummy nữa — việc ghi đè
    // trước đây khiến password thật trong appsettings.json bị vô hiệu hoá và SMTP luôn auth lỗi.
    var envSmtpPassword = Environment.GetEnvironmentVariable("FILMIX_SMTP_PASSWORD");
    if (!string.IsNullOrEmpty(envSmtpPassword))
    {
        options.Password = envSmtpPassword;
    }
    else if (string.IsNullOrEmpty(options.Password))
    {
        Console.WriteLine("[WARN] Chưa cấu hình SMTP Password (cả biến môi trường FILMIX_SMTP_PASSWORD lẫn EmailSettings:Password trong appsettings.json đều trống). Email sẽ không gửi được.");
    }
});
builder.Services.AddScoped<untitled1.Services.IEmailService, untitled1.Services.EmailService>();

// Register JWT configuration with environment variables override and fallback warnings (H-01)
builder.Services.Configure<JwtSettings>(options =>
{
    builder.Configuration.GetSection("JwtSettings").Bind(options);
    var envJwtSecret = Environment.GetEnvironmentVariable("FILMIX_JWT_SECRET");
    if (!string.IsNullOrEmpty(envJwtSecret))
    {
        options.Secret = envJwtSecret;
    }
    else if (options.Secret == "YOUR_JWT_SECRET_PLACEHOLDER_MIN_32_CHARS_LONG_2026!")
    {
        // Fallback for local development
        options.Secret = "SuperSecretKeyForFilmixRESTApiAuthBearerTokens2026!";
        Console.WriteLine("[WARN] FILMIX_JWT_SECRET environment variable not set. Using development fallback secret key.");
    }
});
builder.Services.AddScoped<IJwtService, JwtService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbProvider = builder.Configuration["DbProvider"] ?? "MySql";

if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}

// Register Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Auth";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Configure JWT Authentication (H-01)
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("Cấu hình JwtSettings chưa được khai báo trong appsettings.json");
}

var envJwtSecretForAuth = Environment.GetEnvironmentVariable("FILMIX_JWT_SECRET");
if (!string.IsNullOrEmpty(envJwtSecretForAuth))
{
    jwtSettings.Secret = envJwtSecretForAuth;
}
else if (jwtSettings.Secret == "YOUR_JWT_SECRET_PLACEHOLDER_MIN_32_CHARS_LONG_2026!")
{
    jwtSettings.Secret = "SuperSecretKeyForFilmixRESTApiAuthBearerTokens2026!";
    Console.WriteLine("[WARN] AddJwtBearer configuration is using the fallback development JWT Secret key.");
}

if (string.IsNullOrEmpty(jwtSettings.Secret))
{
    throw new InvalidOperationException("Khóa bí mật JWT (JWT Secret) chưa được cấu hình.");
}

var authBuilder = builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero
        };

        // ── Trả JSON thay vì HTML khi JWT xác thực thất bại ──────────────
        options.Events = new JwtBearerEvents
        {
            // 401 — Token không có hoặc không hợp lệ
            OnChallenge = async context =>
            {
                context.HandleResponse(); // Ngăn handler mặc định chạy
                context.Response.StatusCode  = 401;
                context.Response.ContentType = "application/json; charset=utf-8";
                var body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    success    = false,
                    message    = "Unauthorized. Vui lòng cung cấp JWT Token hợp lệ.",
                    statusCode = 401
                });
                await context.Response.WriteAsync(body);
            },

            // 403 — Token hợp lệ nhưng không đủ quyền (Role)
            OnForbidden = async context =>
            {
                context.Response.StatusCode  = 403;
                context.Response.ContentType = "application/json; charset=utf-8";
                var body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    success    = false,
                    message    = "Forbidden. Bạn không có quyền truy cập tài nguyên này.",
                    statusCode = 403
                });
                await context.Response.WriteAsync(body);
            }
        };
    });

// ── Social login (Google / Facebook) — chỉ bật nếu đã cấu hình credentials trong appsettings hoặc env. ──
var googleClientId = Environment.GetEnvironmentVariable("FILMIX_GOOGLE_CLIENT_ID")
    ?? builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = Environment.GetEnvironmentVariable("FILMIX_GOOGLE_CLIENT_SECRET")
    ?? builder.Configuration["Authentication:Google:ClientSecret"];

// Ignore placeholder settings (H-01)
if (string.IsNullOrWhiteSpace(googleClientId) || googleClientId.Contains("PLACEHOLDER"))
{
    Console.WriteLine("[WARN] FILMIX_GOOGLE_CLIENT_ID not configured or is a placeholder. Google Auth will be disabled.");
    googleClientId = null;
}
if (string.IsNullOrWhiteSpace(googleClientSecret) || googleClientSecret.Contains("PLACEHOLDER"))
{
    Console.WriteLine("[WARN] FILMIX_GOOGLE_CLIENT_SECRET not configured or is a placeholder. Google Auth will be disabled.");
    googleClientSecret = null;
}

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        // CallbackPath mặc định: /signin-google (đăng ký trong Google Console)
        options.SaveTokens = true;

        // Tránh lỗi "Correlation failed" khi chạy trên môi trường phát triển HTTP (localhost)
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.CorrelationCookie.HttpOnly = true;
    });
}

var facebookAppId = Environment.GetEnvironmentVariable("FILMIX_FACEBOOK_APP_ID")
    ?? builder.Configuration["Authentication:Facebook:AppId"];
var facebookAppSecret = Environment.GetEnvironmentVariable("FILMIX_FACEBOOK_APP_SECRET")
    ?? builder.Configuration["Authentication:Facebook:AppSecret"];

// Bỏ qua giá trị placeholder/trống (H-01)
if (string.IsNullOrWhiteSpace(facebookAppId) || facebookAppId.Contains("PLACEHOLDER"))
{
    Console.WriteLine("[WARN] Facebook AppId chưa cấu hình. Đăng nhập Facebook sẽ bị tắt — điền 'Authentication:Facebook:AppId' trong appsettings.json để bật.");
    facebookAppId = null;
}
if (string.IsNullOrWhiteSpace(facebookAppSecret) || facebookAppSecret.Contains("PLACEHOLDER"))
{
    Console.WriteLine("[WARN] Facebook AppSecret chưa cấu hình. Đăng nhập Facebook sẽ bị tắt.");
    facebookAppSecret = null;
}

if (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret))
{
    authBuilder.AddFacebook(options =>
    {
        options.AppId = facebookAppId;
        options.AppSecret = facebookAppSecret;
        // CallbackPath mặc định: /signin-facebook (đăng ký trong Facebook App)
        options.Fields.Add("email");
        options.Fields.Add("name");
        options.SaveTokens = true;
    });
    Console.WriteLine("[INFO] Đăng nhập Facebook đã được kích hoạt.");
}

var app = builder.Build();

// Auto-migrate/create database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "=========================================================================\n" +
                            "ERROR: Không thể kết nối hoặc khởi tạo cơ sở dữ liệu!\n" +
                            "Vui lòng đảm bảo dịch vụ cơ sở dữ liệu đang chạy và thông tin kết nối chính xác.\n" +
                            "========================================================================");
    }

    // Call DbSeeder
    try
    {
        var roleManager  = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager  = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var seederLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        DbSeeder.SeedAsync(roleManager, userManager, seederLogger).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi chạy DbSeeder");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // ── Swagger UI (chỉ hiện trong môi trường Development) ────────────────
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/auth/swagger.json",     "FILMIX Auth API v1");
        c.SwaggerEndpoint("/swagger/cart/swagger.json",     "FILMIX Cart API v1");
        c.SwaggerEndpoint("/swagger/products/swagger.json", "FILMIX Products API v1");
        c.RoutePrefix = "swagger"; // truy cập tại /swagger
        c.DocumentTitle = "FILMIX API Explorer";
        c.DefaultModelsExpandDepth(-1); // ẩn schema models mặc định
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// Custom status code pages (404, 500, etc.) — works in all environments
// Bỏ qua redirect HTML cho các API request — trả thẳng status code
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    appBuilder => appBuilder.UseStatusCodePagesWithReExecute("/Error/{0}")
);

app.UseRouting();
app.UseSession();
app.UseCors("FilmixPolicy"); // M-02: Apply named CORS policy

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
