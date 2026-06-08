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

// Register JWT configuration and services
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
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
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("Cấu hình JwtSettings chưa được khai báo trong appsettings.json");
}

builder.Services.AddAuthentication()
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
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        DbSeeder.SeedAsync(roleManager, userManager).GetAwaiter().GetResult();
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
