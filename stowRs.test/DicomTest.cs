using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using stowRs.test.fixtures;
using Xunit;
using Xunit.Abstractions;

namespace stowRs.test
{
    [Collection("Http client collection")]
    public class DicomTest
    {
        public DicomTest(HttpClientFixture httpClientFixture, ITestOutputHelper output)
        {
            _httpClientFixture = httpClientFixture;
            _output = output;
        }

        private readonly HttpClientFixture _httpClientFixture;
        private readonly ITestOutputHelper _output;

        [Fact]
        public async Task StoreMultipleDicoms()
        {
            //Arrange
            //Arrange
            var uuid1 = Guid.NewGuid().ToString();
            var uuid2 = Guid.NewGuid().ToString();

            var metadata = $@"
            [
                {{
                    ""00200010"": {{
                        ""vr"": ""SH"",
                        ""Value"": [ ""ef98238b-016a-40bb-8790-ea8576d83d5c"" ]
                    }},
                    ""00100020"": {{
                        ""vr"": ""LO"",
                        ""Value"": [ ""1a481da2-6022-4d7f-b9cc-4ef799132e3f"" ]
                    }},
                    ""7FE00010"": {{
                        ""vr"": ""OW"",
                        ""BulkDataURI"": ""urn:uuid:{uuid1}""

                    }}
                }},
                {{
                     ""00200010"": {{
                        ""vr"": ""SH"",
                        ""Value"": [ ""ef98238b-016a-40bb-8790-ea8576d83d5c"" ]
                    }},
                    ""00100020"": {{
                        ""vr"": ""LO"",
                     ""Value"": [ ""1a481da2-6022-4d7f-b9cc-4ef799132e3f"" ]
                    }},
                    ""7FE00010"": {{
                        ""vr"": ""OW"",
                        ""BulkDataURI"": ""urn:uuid:{uuid2}""

                    }}
                }}
            ]";

            var files = new List<FileToStore>
            {
                new FileToStore
                {
                    BlobDataUri = $"urn:uuid:{uuid1}",
                    File = Path.GetFullPath("resources/dicoms/1.3.6.1.4.1.25403.207732457674374.13668.20141127075926.10.dcm")
                },
                new FileToStore
                {
                    BlobDataUri = $"urn:uuid:{uuid2}",
                    File = Path.GetFullPath("resources/dicoms/1.3.6.1.4.1.25403.207732457674374.13668.20141127075926.11.dcm")
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _httpClientFixture.RequestUri)
            {
                Content = TestHelper.CreateMultipartContent(TestHelper.DicomMimeType, metadata, files)
            };

            //Act
            var result = await _httpClientFixture.HttpClient.SendAsync(request);

            //Assert
            _output.WriteLine(result.Content.ReadAsAsync<string>().Result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task StoreSinleDicom()
        {
            //Arrange
            var uuid = Guid.NewGuid().ToString();

            var metadata = $@"
            {{
                ""00200010"": {{
                    ""vr"": ""SH"",
                    ""Value"": [ ""ef98238b-016a-40bb-8790-ea8576d83d5c"" ]
                }},
                ""00100020"": {{
                    ""vr"": ""LO"",
                    ""Value"": [ ""1a481da2-6022-4d7f-b9cc-4ef799132e3f"" ]
                }},
                ""7FE00010"": {{
                    ""vr"": ""OW"",
                    ""BulkDataURI"": ""urn:uuid:{uuid}""

                }}
            }}";

            var file = new FileToStore
            {
                BlobDataUri = $"urn:uuid:{uuid}",
                File = Path.GetFullPath("resources/dicoms/1.3.6.1.4.1.25403.207732457674374.13668.20141127075926.10.dcm")
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _httpClientFixture.RequestUri)
            {
                Content = TestHelper.CreateMultipartContent(TestHelper.DicomMimeType, metadata, new[] {file})
            };

            //Act
            var result = await _httpClientFixture.HttpClient.SendAsync(request);

            //Assert
            _output.WriteLine(await result.Content.ReadAsAsync<string>());
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}