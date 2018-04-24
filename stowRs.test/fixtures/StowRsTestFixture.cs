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
        //private Mock<HttpClient> _mockApiHttpClient;

        public StowRsTestFixture()
        {
            //_mockApiHttpClient = new Mock<HttpClient>();
            //_mockApiHttpClient.Setup(x => x);
            httpClient = new HttpClient(new CustomHandler());
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

        public class CustomHandler : HttpClientHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                switch (request.RequestUri.AbsolutePath)
                {
                    case "/api/condition/search/patient/PUNA9910163/registrationdate/2015-07-20":
                        return await Task.FromResult(CreateResponse(new List<ConditionRecord>
                        {
                            new ConditionRecord
                            {
                                Patient = new Reference {Id = "c2d49f1a-7888-455c-a88b-58e318a02ce9"},
                                Id = "b982953f-0958-4bc2-97f1-5c2f94e890b3"
                            }
                        }));
                    case "/api/condition/search/patient/PUNA9910158/registrationdate/2015-07-11":
                        return await Task.FromResult(CreateResponse(new List<ConditionRecord>
                        {
                            new ConditionRecord
                            {
                                Patient = new Reference {Id = "4228bce9-c02b-4e20-b758-52200d492a15"},
                                Id = "4b6f5632-46a2-4eb5-ad1d-00da0faa721b"
                            }
                        }));
                    case "/api/condition/search/patient/PUNA9910158/registrationdate/2018-04-24":
                        return await Task.FromResult(CreateResponse(new List<ConditionRecord>
                        {
                            new ConditionRecord
                            {
                                Patient = new Reference {Id = "4228bce9-c02b-4e20-b758-52200d492a15"},
                                Id = "59b1146d-e2f7-4922-a602-306fca97c355"
                            }
                        }));
                    default:
                        return await base.SendAsync(request, cancellationToken);

                }
            }

            private HttpResponseMessage CreateResponse(IEnumerable<ConditionRecord> records)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(records), Encoding.UTF8, "application/json")
                };
            }
        }
    }
}