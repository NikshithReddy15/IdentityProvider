using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace IdentityProvider.Auth;

public static class ServiceCollectionExtensions
{
    private const string HttpClientName = "IdentityProvider.Auth";

    public static IServiceCollection AddIdentityProviderAuthentication(
        this IServiceCollection services,
        Action<IdentityProviderAuthOptions> configure)
    {
        var options = new IdentityProviderAuthOptions
        {
            Key = string.Empty,
            Issuer = string.Empty,
            Audience = string.Empty,
        };
        configure(options);

        if (string.IsNullOrEmpty(options.Key))
            throw new InvalidOperationException("IdentityProviderAuthOptions.Key is required.");
        if (string.IsNullOrEmpty(options.Issuer))
            throw new InvalidOperationException("IdentityProviderAuthOptions.Issuer is required.");
        if (string.IsNullOrEmpty(options.Audience))
            throw new InvalidOperationException("IdentityProviderAuthOptions.Audience is required.");
        if (options.EnableRemoteRevocationCheck && string.IsNullOrEmpty(options.IdentityProviderBaseUrl))
            throw new InvalidOperationException("IdentityProviderAuthOptions.IdentityProviderBaseUrl is required when EnableRemoteRevocationCheck is true.");

        if (options.EnableRemoteRevocationCheck)
        {
            services.AddHttpClient(HttpClientName, client =>
            {
                client.BaseAddress = new Uri(options.IdentityProviderBaseUrl!.TrimEnd('/') + "/");
            });
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.MapInboundClaims = false;
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = options.Issuer,
                    ValidAudience = options.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
                };

                jwtOptions.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        if (!options.EnableRemoteRevocationCheck) return;

                        var factory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var client = factory.CreateClient(HttpClientName);

                        var bearer = context.Request.Headers.Authorization.ToString();
                        var token = bearer.Replace("Bearer ", string.Empty).Trim();

                        var request = new HttpRequestMessage(HttpMethod.Get, "api/authentication");
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        var response = await client.SendAsync(request);
                        if (!response.IsSuccessStatusCode)
                            context.Fail("Invalid token");
                    },
                    OnChallenge = context =>
                    {
                        if (context.Response.HasStarted) return Task.CompletedTask;
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "text/plain";
                        return context.Response.WriteAsync("Unauthorized");
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Response.HasStarted) return Task.CompletedTask;
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "text/plain";
                        return context.Response.WriteAsync(context.Exception.Message);
                    },
                    OnForbidden = context =>
                    {
                        if (context.Response.HasStarted) return Task.CompletedTask;
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "text/plain";
                        return context.Response.WriteAsync("Forbidden");
                    },
                };
            });

        return services;
    }
}
