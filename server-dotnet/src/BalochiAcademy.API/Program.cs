using System.Text;
using BalochiAcademy.API.Services;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Infrastructure.Identity;
using BalochiAcademy.Infrastructure.Persistence;
using BalochiAcademy.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// Bootstrap logger — always succeeds; replaced once the host is built.
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console();

    // Only add SQL Server structured logging in non-Development environments
    // where the sink will reliably connect. In Development, console is enough.
    if (!ctx.HostingEnvironment.IsDevelopment())
    {
        var connStr = ctx.Configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            lc.WriteTo.MSSqlServer(connStr,
                sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
                {
                    TableName = "AppLogs",
                    AutoCreateSqlTable = true,
                });
        }
    }
});

// ── EF Core ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddScoped<IApplicationDbContext>(sp =>
    sp.GetRequiredService<ApplicationDbContext>());

// ── Auth Services ────────────────────────────────────────────────────────────
builder.Services.AddScoped<ITokenService,    TokenService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuditService,    AuditService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ── JWT Authentication ───────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer           = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidateAudience         = true,
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminOnly",   p => p.RequireRole("admin"));
    opts.AddPolicy("Authenticated", p => p.RequireAuthenticatedUser());
});

// ── CORS (allow Vite dev server) ─────────────────────────────────────────────
builder.Services.AddCors(opts => opts.AddDefaultPolicy(p =>
    p.WithOrigins(
        "http://localhost:5173",
        "http://localhost:5174",
        builder.Configuration["App:ClientUrl"] ?? "http://localhost:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

// ── Controllers + SignalR ────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Use full type name to avoid conflicts between same-named DTOs in different namespaces
    c.CustomSchemaIds(t => t.FullName?.Replace("+", "_") ?? t.Name);

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "Balochistan Academy Portal API",
        Version = "v1",
        Description = "REST API for the Balochistan Academy Portal — student learning platform",
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Balochistan Academy API v1");
        c.RoutePrefix = "docs";
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BalochiAcademy.API.Middleware.ExceptionMiddleware>();
app.MapControllers();
app.MapHub<BalochiAcademy.API.Hubs.NotificationHub>("/hubs/notifications");

// ── Seed initial data ────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db        = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    var passwords = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    await BalochiAcademy.API.Infrastructure.DatabaseSeeder.SeedAsync(db, passwords);
}

app.Run();
