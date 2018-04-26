using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace stowRs.test.fixtures
{
    public class StowRsTestFixture : IDisposable
    {
        private static HttpClient httpClient;

        public StowRsTestFixture()
        {
            httpClient = new HttpClient();
        }

        public string RequestUri => "/api/stowrs/studies";

        public void Dispose()
        {
            httpClient?.Dispose();
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
    }
}