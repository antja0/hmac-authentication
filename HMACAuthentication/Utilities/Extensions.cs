using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Antja.Authentication.HMAC.Utilities
{
    public static class Extensions
    {
        public static AuthenticationBuilder AddScheme<THandler>(this AuthenticationBuilder builder, string authenticationScheme)
            where THandler : AuthenticationHandler<AuthenticationSchemeOptions> =>
            builder.AddScheme<AuthenticationSchemeOptions, THandler>(authenticationScheme, o => { });

        public static IServiceCollection ConfigureDictionary<TDictValue>(this IServiceCollection services, IConfigurationSection section)
            where TDictValue : class, new()
        {
            var values = section
                .GetChildren()
                .ToList();

            services.Configure<Dictionary<string, TDictValue>>(dict =>
            {
                foreach (var value in values)
                {
                    var obj = new TDictValue();
                    section.Bind(value.Key, obj);
                    dict.Add(value.Key, obj);
                }
            });

            return services;
        }
    }
}
