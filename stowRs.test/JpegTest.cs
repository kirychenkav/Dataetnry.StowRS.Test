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
    public class JpegTest
    {
        public JpegTest(StowRsTestFixture stowRsTestFixture, ITestOutputHelper output)
        {
            _stowRsTestFixture = stowRsTestFixture;
            _output = output;
        }

        private readonly StowRsTestFixture _stowRsTestFixture;
        private readonly ITestOutputHelper _output;

        [Fact]
        public async Task StoreMultipleJpegs()
        {
            //Arrange
            var uuid1 = Guid.NewGuid().ToString();
            var uuid2 = Guid.NewGuid().ToString();

            var metadata =
                $@"
                [
                    {{
                        ""00200010"": {{
                            ""vr"": ""SH"",
                            ""Value"": [ ""{_stowRsTestFixture.ConditionId}"" ]
                        }},
                        ""00100020"": {{
                            ""vr"": ""LO"",
                            ""Value"": [ ""{_stowRsTestFixture.PatientId}"" ]
                        }},
                        ""7FE00010"": {{
                            ""vr"": ""OW"",
                            ""BulkDataURI"": ""urn:uuid:{uuid1}""
                        }},
                        ""00080020"": {{
                            ""vr"": ""DA"",
                            ""Value"": [ ""20170101"" ]
                        }},
                        ""00080060"": {{
                            ""vr"": ""CS"",
                            ""Value"": [ ""CR"" ]
                        }}
                    }},
                    {{
                        ""00200010"": {{
                            ""vr"": ""SH"",
                            ""Value"": [ ""{_stowRsTestFixture.ConditionId}"" ]
                        }},
                        ""00100020"": {{
                            ""vr"": ""LO"",
                            ""Value"": [ ""{_stowRsTestFixture.PatientId}"" ]
                        }},
                        ""7FE00010"": {{
                            ""vr"": ""OW"",
                            ""BulkDataURI"": ""urn:uuid:{uuid2}""
                        }},
                        ""00080020"": {{
                            ""vr"": ""DA"",
                            ""Value"": [ ""20180101"" ]
                        }},
                        ""00080060"": {{
                            ""vr"": ""CS"",
                            ""Value"": [ ""CR"" ]
                        }}
                    }}
                ]";

            var files = new List<FileToStore>
            {
                new FileToStore
                {
                    BlobDataUri = $"urn:uuid:{uuid1}",
                    File = Path.GetFullPath("resources/jpegs/1.jpg")
                },
                new FileToStore
                {
                    BlobDataUri = $"urn:uuid:{uuid2}",
                    File = Path.GetFullPath("resources/jpegs/2.jpg")
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _stowRsTestFixture.RequestUri)
            {
                Content = TestHelper.CreateMultipartContent(TestHelper.JpegMimeType, metadata, files)
            };

            //Act
            var result = await _stowRsTestFixture.HttpClient.SendAsync(request);

            //Assert
            _output.WriteLine(result.Content.ReadAsAsync<string>().Result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task StoreSinleJpeg()
        {
            //Arrange
            var uuid = Guid.NewGuid().ToString();

            var metadata =
                $@"{{
                        ""00200010"": {{
                            ""vr"": ""SH"",
                            ""Value"": [ ""{_stowRsTestFixture.ConditionId}"" ]
                        }},
                        ""00100020"": {{
                            ""vr"": ""LO"",
                            ""Value"": [ ""{_stowRsTestFixture.PatientId}"" ]
                        }},
                        ""7FE00010"": {{
                            ""vr"": ""OW"",
                            ""BulkDataURI"": ""urn:uuid:{uuid}""
                        }},
                        ""00080020"": {{
                            ""vr"": ""DA"",
                            ""Value"": [ ""20170101"" ]
                        }},
                        ""00080060"": {{
                            ""vr"": ""CS"",
                            ""Value"": [ ""CR"" ]
                        }}
                    }}";

            var file = new FileToStore
            {
                BlobDataUri = $"urn:uuid:{uuid}",
                File = Path.GetFullPath("resources/jpegs/1.jpg")
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _stowRsTestFixture.RequestUri)
            {
                Content = TestHelper.CreateMultipartContent(TestHelper.JpegMimeType, metadata, new[] {file})
            };

            //Act
            var result = await _stowRsTestFixture.HttpClient.SendAsync(request);

            //Assert
            _output.WriteLine(await result.Content.ReadAsAsync<string>());
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}