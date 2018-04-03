using System;
using System.Net.Http;
using System.Net.Http.Headers;


namespace stowRs.test.fixtures
{
    public class StowRsTestFixture : IDisposable
    {
        private HttpClient httpClient;
        public StowRsTestFixture()
        {
            httpClient = new HttpClient();
        }

        public StowRsTestFixture UseBaseUri(string baseUri)
        {
            httpClient.BaseAddress = new Uri(baseUri);
            return this;
        }

        public StowRsTestFixture UseBearerToken(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return this;
        }

        public HttpClient Build()
        {
            return httpClient;
        }

        public string RequestUri => "/api/stowrs/studies";

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}