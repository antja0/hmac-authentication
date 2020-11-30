using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Antja.Authentication.HMAC;
using HMACAuthentication.TestApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HMACAuthentication.Tests.Integration
{
    internal abstract class IntegrationTestBase
    {
        protected readonly HttpClient TestClient;
        private readonly Dictionary<string, HMACSignatureOptions> _authOptions;

        protected IntegrationTestBase()
        {
            const string hostUrl = "https://localhost:1338";

            var appFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseUrls(hostUrl);
                    builder.ConfigureLogging(logging => logging.ClearProviders());
                });

            TestClient = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                // Note: Must specify HTTPS here because default is HTTP.
                BaseAddress = new Uri(hostUrl),
            });

            _authOptions = appFactory.Services.GetService<IOptions<Dictionary<string, HMACSignatureOptions>>>().Value;
        }

        protected Tuple<string, string> GetSignatureHeader<TBody>(TBody body, string schema)
        {
            var bodyData = JsonConvert.SerializeObject(body);
            var bodyAsBytes = Encoding.ASCII.GetBytes(bodyData);

            var options = _authOptions[schema];

            var hash = HMACUtilities.ComputeHash(options.HashFunction, options.Secret, bodyAsBytes);
            var hashString = HMACUtilities.ToHexString(hash);

            return new Tuple<string, string>(options.Header, HMACUtilities.GetSignaturePrefix(options.HashFunction) + hashString);
        }
    }
}
