using System.Text;
using BalochiAcademy.API.Authorization;
using BalochiAcademy.API.Services;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Application.Common.Mappings;
using BalochiAcademy.Infrastructure.Identity;
using BalochiAcademy.Infrastructure.Persistence;
using BalochiAcademy.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
      .WriteTo.Console()
      .WriteTo.File(
          Path.Combine(AppContext.BaseDirectory, "logs", "app-.log"),
          rollingInterval: Serilog.RollingInterval.Day,
          retainedFileCountLimit: 7,
          outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

    // SQL Server sink — only in Production, wrapped so a bad connection string
    // or missing SQL login does not crash the process on startup.
    if (ctx.HostingEnvironment.IsDevelopment())
    {
        var connStr = ctx.Configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            try
            {
                lc.WriteTo.MSSqlServer(connStr,
                    sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
                    {
                        TableName          = "AppLogs",
                        AutoCreateSqlTable = true,
                    });
            }
            catch (Exception ex)
            {
                // Bootstrap logger is still active here — this surfaces in the IIS stdout log.
                Log.Warning(ex, "SQL Server log sink failed to initialize — file sink is active");
            }
        }
    }
});

// ── EF Core ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddScoped<IApplicationDbContext>(sp =>
    sp.GetRequiredService<ApplicationDbContext>());

// ── Repository / Unit of Work ─────────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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
        // SignalR passes the token as ?access_token= because WebSocket/SSE can't send headers.
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
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

// ── AutoMapper ───────────────────────────────────────────────────────────────
// Scan both assemblies: API (future profiles) + Application (MappingProfile)
builder.Services.AddAutoMapper(cfg => {
    cfg.AddMaps(typeof(Program));       // API assembly
    cfg.AddMaps(typeof(MappingProfile)); // Application assembly — contains User→UserDto etc.
});

// ── FluentValidation ─────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<MappingProfile>();

// ── Memory Cache (for permission service) ────────────────────────────────────
builder.Services.AddMemoryCache();

// ── Permission-Based Authorization ───────────────────────────────────────────
builder.Services.AddScoped<IPermissionService, PermissionService>();
// Wraps the default policy provider so built-in policies (AdminOnly, etc.) still resolve
builder.Services.AddSingleton<IAuthorizationPolicyProvider>(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
    var defaultProvider = new DefaultAuthorizationPolicyProvider(options);
    return new PermissionPolicyProvider(defaultProvider);
});
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// ── System Settings (DB-backed, singleton cache) ─────────────────────────────
builder.Services.AddSingleton<ISystemSettingsService, SystemSettingsService>();

// ── AI Service (Ollama → Gemini fallback) ────────────────────────────────────
// Named "ollama" client — base URL comes from appsettings Ai:OllamaUrl
builder.Services.AddHttpClient("ollama", (sp, client) =>
{
    var url = sp.GetRequiredService<IConfiguration>()["Ai:OllamaUrl"] ?? "http://localhost:11434";
    client.BaseAddress = new Uri(url);
});
builder.Services.AddHttpClient(); // default factory for Gemini (absolute URLs)
builder.Services.AddScoped<AiService>();

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
        Title       = "Balochistan Academy Portal API",
        Version     = "v1",
        Description = "REST API for the Balochistan Academy Portal — student learning platform.\n\n" +
                      "Authenticate via **POST /api/auth/login**, copy the `token` field, " +
                      "and click **Authorize** → paste `Bearer {token}`.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name  = "USS — Ultra Soft System",
            Email = "support@ultrasoftsystem.com",
        },
    });

    // Include XML documentation comments from the API project
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description  = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name         = "Authorization",
        In           = ParameterLocation.Header,
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
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

// Swagger enabled in all environments for API documentation (Phase 8)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Balochistan Academy API v1");
    c.RoutePrefix = "docs";
    c.DocumentTitle = "Balochistan Academy Portal — API Docs";
});

app.UseCors();
app.UseStaticFiles(); // serves wwwroot/uploads/* publicly (auth handled at upload endpoint)
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BalochiAcademy.API.Middleware.ExceptionMiddleware>();
app.MapControllers();
app.MapHub<BalochiAcademy.API.Hubs.NotificationHub>("/hubs/notifications");

// ── Migrate database + Seed initial data ────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    try
    {
        // MigrateAsync creates the DB if missing and applies any pending migrations.
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var passwords = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        await BalochiAcademy.API.Infrastructure.DatabaseSeeder.SeedAsync(context, passwords);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database migration/seeding failed — fix the connection string and restart");
    }
}

app.Run();
