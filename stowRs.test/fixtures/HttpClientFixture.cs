using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;


namespace stowRs.test.fixtures
{
    public class HttpClientFixture : IDisposable
    {
        public HttpClientFixture()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build();

            if (string.IsNullOrWhiteSpace(config["BaseUri"]))
            {
                throw new Exception("Value of configuration parameter 'BaseUri' is missing!");
            }

            if (string.IsNullOrWhiteSpace(config["BearerToken"]))
            {
                throw new Exception("Value of configuration parameter 'BearerToken' is missing!");
            }

            HttpClient = new HttpClient
            {
                BaseAddress = new Uri(config["BaseUri"]),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", config["BearerToken"])
                }
            };
        }

        public HttpClient HttpClient { get; }

        public string RequestUri => "/api/stowrs/studies";

        public void Dispose()
        {
            HttpClient?.Dispose();
        }
    }
}