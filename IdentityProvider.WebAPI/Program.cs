using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Azure.Identity;
using Extensions;
using Extensions.Mapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Services;
using Services.Authentication;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

var builder = WebApplication.CreateBuilder(args);

var azureKeyVaultUri = new Uri(builder.Configuration["AzureKeyVaultUri"]);
builder.Configuration.AddAzureKeyVault(azureKeyVaultUri, new DefaultAzureCredential());

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])),
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var securityToken = context.SecurityToken as JsonWebToken;
            if (securityToken == null)
            {
                context.Fail("Invalid token");
                return;
            }

            var bearer = context.Request.Headers.Authorization.ToString();
            var jwtToken = bearer.Replace("Bearer ", string.Empty).Trim();
            var authenticationService = context.HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
            var isRevokedToken = await authenticationService.IsRevokedTokenAsync(jwtToken);
            if (isRevokedToken)
            {
                context.Fail("Invalid token");
                return;
            }

            var userId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value?.ToGuid();
            var sessionId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sid)?.Value?.ToGuid();
            if (!userId.HasValue || !sessionId.HasValue)
            {
                context.Fail("Invalid token claims");
                return;
            }
            
            var isValidLoginSession = await authenticationService.IsValidLoginSessionAsync(userId.Value, sessionId.Value);
            if (!isValidLoginSession)
            {
                context.Fail("Invalid login session");
            }
        },
        OnChallenge = context =>
        {
            if (context.Response.HasStarted)
            {
                return Task.CompletedTask;
            }
            
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "text/plain";
            return context.Response.WriteAsync("Unauthorized");
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Response.HasStarted)
            {
                return Task.CompletedTask;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "text/plain";
            return context.Response.WriteAsync(context.Exception.Message);
        },
        OnForbidden = context =>
        {
            if (context.Response.HasStarted)
            {
                return Task.CompletedTask;
            }

            context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "text/plain";
            return context.Response.WriteAsync("Forbidden");
        },
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.RegisterAuthenticationWebApiServices();
builder.Services.AddScoped<JwtSecurityTokenHandler>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Authentication API",
            Version = "v1"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Description = "Enter: {your JWT token}"
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                []
            }
        });
    });
}

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

MapperFactory.InitializeServiceProvider(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.Run();