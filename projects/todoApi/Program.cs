using NSwag.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.NoClobber().Load(Path.Combine(builder.Environment.ContentRootPath, ".env"));
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddSingleton<JwtTokenGenerator>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "TodoAPI";
    config.Title = "TodoAPI v1";
    config.Version = "v1";
});
var secret = builder.Configuration["Jwt:Secret"]
      ?? throw new InvalidOperationException("Missing Secret.");
var keyStr = Convert.FromBase64String(secret);
var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyStr);

var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = "TodoApi",
    ValidateAudience = true,
    ValidAudience = "TodoApiUsers",
    ValidateLifetime = true,
    RequireExpirationTime = true,
    RequireSignedTokens = true,
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = securityKey
};
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = validationParameters;
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.MapGet("/", () => "Hello World!").RequireAuthorization();

app.MapPost("/login", (LoginRequest request, JwtTokenGenerator jwt) =>
{
    if (request.Username != "admin" || request.Password != "demo123")
        return Results.Unauthorized();

    var token = jwt.GenerateToken(request.Username);
    return Results.Ok(new { token });
}).AllowAnonymous();

app.Run();

record LoginRequest(string Username, string Password);
