using System;
using System.Net.Http;
using System.Net.Http.Headers;


namespace stowRs.test.fixtures
{
    public class StowRsTestFixture : IDisposable
    {
        public StowRsTestFixture()
        {
            HttpClient = new HttpClient();
        }

        public void SetHttpClientParams(string baseUri, string bearerToken)
        {
            HttpClient.BaseAddress = new Uri(baseUri);
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        public HttpClient HttpClient { get; }

        public string RequestUri => "/api/stowrs/studies";

        public void Dispose()
        {
            HttpClient?.Dispose();
        }
    }
}