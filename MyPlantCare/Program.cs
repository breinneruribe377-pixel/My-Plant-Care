using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyPlantCare.Data;
using MyPlantCare.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Configuración obligatoria (viene de variables de entorno, user-secrets o appsettings) ──
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'ConnectionStrings:Default' (variable de entorno ConnectionStrings__Default).");

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException(
        "Falta 'Jwt:Key' o es demasiado corta (mínimo 32 caracteres). Defínela con la variable de entorno Jwt__Key.");

// ── JSON: el enum TipoCuidado viaja como texto ("riego", "revision_plagas", ...) ──
static void ConfigurarJson(JsonSerializerOptions o) =>
    o.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));

builder.Services.AddControllers().AddJsonOptions(o => ConfigurarJson(o.JsonSerializerOptions));
builder.Services.ConfigureHttpJsonOptions(o => ConfigurarJson(o.SerializerOptions));

// ── Base de datos (SQL Server) ──
builder.Services.AddDbContext<PlantCareDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

// ── Servicios propios ──
builder.Services.AddScoped<TokenService>();

// ── Autenticación JWT ──
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // conservamos el claim "sub" tal cual
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ── OpenAPI ──
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// ── Documentación: OpenAPI + Swagger UI (siempre disponible, es parte de la entrega) ──
app.MapOpenApi(); // /openapi/v1.json
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "MyPlantCare API v1");
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Agrega el esquema Bearer a Swagger y marca como protegidos todos los endpoints salvo /auth/*
sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "MyPlantCare API",
            Version = "v1",
            Description = "API REST para gestionar plantas y sus cuidados periódicos (riego, fertilización, poda, etc.)."
        };

        document.Servers = new List<OpenApiServer> { new OpenApiServer { Url = "/" } };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Pega aquí el accessToken devuelto por /auth/login (sin la palabra 'Bearer')."
        };

        foreach (var (ruta, pathItem) in document.Paths)
        {
            if (ruta.StartsWith("/auth", StringComparison.OrdinalIgnoreCase)) continue;

            foreach (var operation in pathItem.Operations.Values)
            {
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    }] = Array.Empty<string>()
                });
            }
        }

        return Task.CompletedTask;
    }
}
