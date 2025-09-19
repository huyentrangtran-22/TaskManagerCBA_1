using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Services;
using TaskManager.Identity.Entities;
using TaskManager.Identity.Services;

using TaskManager.Projects.Data;
using TaskManager.Projects.Interfaces;
using TaskManager.Projects.Services;
using TaskManager.Shared.Data;
using TaskManager.Tasks.Services;

using TaskManager.Notifications.Core;
using TaskManager.Notifications.Email.Smtp;
using TaskManager.Notifications.Templating;
using TaskManager.Notifications.Persistence.EFCore;
using TaskManager.Notifications.Abstractions;
using TaskManager.Notifications.Services;

var builder = WebApplication.CreateBuilder(args);
//hg

//ProjectDbContext 
// ===== Add DbContext =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// DbContext dành cho Notifications (dùng lại ConnectionStrings:DefaultConnection)
builder.Services.AddDbContext<NotificationsDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// ===== Add Services (DI) =====
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddNotificationsCore(builder.Configuration);
builder.Services.AddScoped<TaskManager.Notifications.Services.IUserNotificationService, NotificationTemplateAdapter>();

// Cấu hình SMTP + Render template (đọc từ DB Templates)
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<ITemplateRenderer, ScribanTemplateRenderer>();
builder.Services.AddScoped<ProjectMembershipService>();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // chuyển hướng nếu chưa đăng nhập
    options.AccessDeniedPath = "/Account/AccessDenied"; // nếu không đủ quyền
});


// ===== Add MVC =====
builder.Services.AddControllersWithViews();
// Đăng ký Notifications (Service + Worker nền)
builder.Services.AddNotificationsCore(builder.Configuration);

var app = builder.Build();

// ===== Configure Middleware =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication phải đặt trước Authorization
app.UseAuthentication();
app.UseAuthorization();

// ===== Map routes =====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
