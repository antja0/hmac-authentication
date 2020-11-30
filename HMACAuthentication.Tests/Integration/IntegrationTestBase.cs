using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Antja.Authentication.HMAC;
using Antja.Authentication.HMAC.Utilities;
using HMACAuthentication.TestApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HMACAuthentication.Tests.Integration
{
    internal abstract class IntegrationTestBase
    {
        protected readonly HttpClient TestClient;
        private Dictionary<string, HMACSignatureOptions> _authOptions;

        protected IntegrationTestBase()
        {
            const string hostUrl = "https://localhost:1338";

            var appFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseUrls(hostUrl);
                    builder.ConfigureAppConfiguration((webHostBuilderContext, configurationBuilder) =>
                    {
                        var config = configurationBuilder.Build();
                        _authOptions = config.GetValue("AuthOptions", new Dictionary<string, HMACSignatureOptions>());
                    });
                    builder.ConfigureLogging(logging => logging.ClearProviders());
                });

            TestClient = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                // Note: Must specify HTTPS here because default is HTTP.
                BaseAddress = new Uri(hostUrl),
            });
        }

        protected void AddSignatureHeaders<TBody>(TBody body, string schema)
        {
            var bodyData = JsonConvert.SerializeObject(body);
            var bodyAsBytes = Encoding.ASCII.GetBytes(bodyData);

            var options = _authOptions[schema];

            var hash = HMACUtilities.ComputeHash(options.HashFunction, options.Secret, bodyAsBytes);
            var hashString = HMACUtilities.ToHexString(hash);
            TestClient.DefaultRequestHeaders.Add(options.Header, new[] { HMACUtilities.GetSignaturePrefix(options.HashFunction) + hashString });
        }
    }
}
