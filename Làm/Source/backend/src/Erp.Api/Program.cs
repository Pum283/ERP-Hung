using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Erp.Api.Middlewares;
using Erp.Application;
using Erp.Infrastructure;
using Erp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Load .env trước CreateBuilder để Configuration đọc được biến môi trường
var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile)) DotNetEnv.Env.Load(envFile);

var builder = WebApplication.CreateBuilder(args);

// Map CONNECTION_STRING → ConnectionStrings:Default
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
if (!string.IsNullOrWhiteSpace(connectionString))
    builder.Configuration["ConnectionStrings:Default"] = connectionString;

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

var key = builder.Configuration["Jwt:SecretKey"] ?? "PumsErp_DevSecretKey_ChangeMe_AtLeast32Chars!";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "pums-erp-api";
var audience = builder.Configuration["Jwt:Audience"] ?? "pums-erp-app";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role
        };
        // SignalR: JWT qua query ?access_token=
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddSingleton<Erp.Application.Interfaces.Realtime.IWfRealtimeNotifier, Erp.Api.Realtime.WfRealtimeNotifier>();
builder.Services.AddSingleton<Erp.Application.Interfaces.Realtime.IMsgRealtimeNotifier, Erp.Api.Realtime.MsgRealtimeNotifier>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFrontend", policy =>
        policy.WithOrigins(
                "http://localhost:2222",
                "http://127.0.0.1:2222")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pum's ERP API", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DevFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<LicenseModuleMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();
app.MapControllers();
app.MapHub<Erp.Api.Hubs.WfHub>("/hubs/wf");
app.MapHub<Erp.Api.Hubs.MsgHub>("/hubs/msg");

if (app.Configuration.GetValue("SeedOnStartup", true))
{
    try
    {
        await DbSeeder.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Seed/DB chưa sẵn sàng — API vẫn chạy; cấu hình ConnectionStrings:Default.");
    }
}

app.Run();

public partial class Program;
