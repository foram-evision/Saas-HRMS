using System.Text;
using HRMS.API.Middleware;
using HRMS.Application;
using HRMS.Infrastructure;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using HRMS.Application.Validators;
using System.Text.Json.Serialization;

// Top-level statements used for ASP.NET Core minimal Program pattern.
// This file configures services and middleware for the HRMS API.

var builder = WebApplication.CreateBuilder(args);

#region Controllers

// Register MVC controllers and configure JSON + model validation behavior.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enforces string conversions for enum keys inside Swagger UI and API payloads.
        // Using JsonStringEnumConverter ensures enums are serialized/deserialized as strings.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Customize the default response for invalid model state to return BadRequest with the ModelState.
        options.InvalidModelStateResponseFactory = context =>
        {
            return new BadRequestObjectResult(context.ModelState);
        };
    });

// Enable automatic FluentValidation integration with ASP.NET Core model binding.
builder.Services.AddFluentValidationAutoValidation();

// Register all validators from the assembly that contains RegisterValidator.
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

#endregion

#region CORS

// Configure Cross-Origin Resource Sharing (CORS).
// "AllowAll" policy is permissive and intended for development or internal APIs.
// Consider tightening origins/headers/methods in production.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

#endregion

#region Application & Infrastructure

// Register application-level services (CQRS, business logic, etc.)
builder.Services.AddApplication();

// Register infrastructure services (EF Core DbContext, identity, repositories, email, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

#endregion

#region HTTP Context Accessor

// Register IHttpContextAccessor to allow services to access the current HttpContext when needed.
builder.Services.AddHttpContextAccessor();

#endregion

#region JWT Authentication

// Read JWT settings from configuration (appsettings.json or environment).
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// Ensure a secret key exists; if not, throw an explicit error to fail fast.
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

// Convert the secret into a symmetric key for token signing.
var key = Encoding.UTF8.GetBytes(secretKey);

// Configure authentication to use JWT Bearer tokens.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Do not require HTTPS metadata during development; consider enabling in production.
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    // Configure token validation parameters to ensure tokens are valid.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        // Issuer and Audience should match values in configuration and the tokens issued by your auth service.
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(key),

        // Remove default clock skew to have strict expiry behavior.
        ClockSkew = TimeSpan.Zero
    };
});

#endregion

#region Swagger

// Enable API explorer for endpoints (required by Swagger generation).
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI generation and include a bearer token security definition.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HRMS POC API",
        Version = "v1",
        Description = "Human Resource Management System API — Enterprise Authentication"
    });

    // Add a security definition so consumers can provide a Bearer token in the Authorization header.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT Token like: Bearer eyJhbGciOiJIUzI1Ni...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    // Require the defined security scheme globally (applies to all operations).
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

var app = builder.Build();

#region Middleware

// Global exception handling middleware should be registered early so it can catch exceptions from later middleware.
app.UseMiddleware<ExceptionMiddleware>();

// Apply CORS policy.
app.UseCors("AllowAll");

// Enable Swagger middleware to serve generated JSON and UI.
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRMS API V1");
    // Serve Swagger UI at application root ("/").
    c.RoutePrefix = string.Empty;
});

// Redirect HTTP to HTTPS.
app.UseHttpsRedirection();

// Register authentication/authorization middleware in correct order.
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes for API endpoints.
app.MapControllers();

#endregion

#region Migration & Seeding

// On startup, create a scope to apply EF Core migrations and perform initial data seeding.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Apply any pending migrations. This ensures database schema is up-to-date.
        await context.Database.MigrateAsync();

        // Optional: run raw SQL from a StoredProcedures.sql file if present.
        // This is commented out by default to avoid accidental execution.
        //var sqlPath = Path.Combine(builder.Environment.ContentRootPath, "..", "StoredProcedures.sql");
        //if (File.Exists(sqlPath))
        //{
        //    var sql = await File.ReadAllTextAsync(sqlPath);
        //    await context.Database.ExecuteSqlRawAsync(sql);
        //}

        // Seed identity roles and any other initial identity data.
        await IdentitySeed.SeedRolesAsync(services);
    }
    catch (Exception ex)
    {
        // If migration or seeding fails, log the error. The application continues to start, but it's useful to surface failures.
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogError(ex,
            "An error occurred while migrating or seeding the database.");
    }
}

#endregion

// Start the web application and begin listening for requests.
app.Run();                              