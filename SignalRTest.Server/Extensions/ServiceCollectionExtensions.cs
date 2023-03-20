using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SignalRTest.Server.Services;

namespace SignalRTest.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = new JwtSettings();
        var section = config.GetSection(nameof(JwtSettings));
        section.Bind(jwtSettings);
        services.Configure<JwtSettings>(section);

        var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        services.AddScoped<AuthTokenService>();

        return services;
    }
}
