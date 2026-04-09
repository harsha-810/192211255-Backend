using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PetClinicAPI.Models;
using PetClinicAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddHttpClient();

builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IEmailService, BrevoEmailService>();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.30-mysql")));

// JWT Settings
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SecretKey";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Diagnostic Middleware for 403 Errors
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 403)
    {
        var user = context.User;
        var roles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
        var otherRoles = user.FindAll("role").Select(c => c.Value);
        Console.WriteLine($"[AUTH-DEBUG] 403 Forbidden: Path={context.Request.Path}, Roles(Schema)={string.Join("|", roles)}, Roles(Simple)={string.Join("|", otherRoles)}");
    }
});

app.MapControllers();

app.Run();
