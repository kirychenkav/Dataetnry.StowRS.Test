using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dicom;
using stowRs.test.fixtures;
using Xunit;
using Xunit.Abstractions;

namespace stowRs.test
{
    [Collection("Http client collection")]
    public class StowRsTest
    {
        private const string EmptyBaseUriTemplate = "{INSERT BASE URI HERE}";
        private const string EmptyBearerTokenTemplate = "{INSERT BEARER TOKEN HERE}";
        private const string EmptyPatientIdTemplate = "{INSERT PATIENT ID HERE}";
        private const string EmptyCaseIdTemplate = "{INSERT CASE ID HERE}";

        private readonly ITestOutputHelper _output;
        private readonly StowRsTestFixture _stowRsTestFixture;

        public StowRsTest(StowRsTestFixture stowRsTestFixture, ITestOutputHelper output)
        {
            _stowRsTestFixture = stowRsTestFixture;
            _output = output;
        }

        [Theory]
        [InlineData(EmptyBaseUriTemplate,
            EmptyBearerTokenTemplate,
            EmptyPatientIdTemplate,
            EmptyCaseIdTemplate,
            "resources/dicoms/1.3.6.1.4.1.25403.207732457674374.13668.20141127075926.10.dcm",
            "resources/dicoms/1.3.6.1.4.1.25403.207732457674374.13668.20141127075926.11.dcm")]
        public async Task StoreDicoms(string baseUri, string bearerToken, string patientId, string studyId,
            params string[] files)
        {
            //Arrange
            Assert.True(ValidateParameters(baseUri, bearerToken, patientId, studyId));
            var httpClient = _stowRsTestFixture
                .UseBaseUri(baseUri)
                .UseBearerToken(bearerToken)
                .Build();

            var dataset = new DicomDataset();

            dataset.AddOrUpdate(DicomTag.PatientID, patientId);
            dataset.AddOrUpdate(DicomTag.StudyID, studyId);

            var dataToStore = TestHelper.FillDataWithBlobDataUris(dataset, files.Select(Path.GetFullPath));

            var request = new HttpRequestMessage(HttpMethod.Post, _stowRsTestFixture.RequestUri)
            {
                Content = TestHelper.CreateMultipartContent(dataToStore)
            };

            //Act
            var result = await httpClient.SendAsync(request);

            //Assert
            if (result.IsSuccessStatusCode == false)
            {
                _output.WriteLine(result.ReasonPhrase);
                _output.WriteLine(await result.Content.ReadAsStringAsync());
            }

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Theory]
        [InlineData(EmptyBaseUriTemplate,
            EmptyBearerTokenTemplate,
            EmptyPatientIdTemplate,
            EmptyCaseIdTemplate,
            "CR",
            "20180101",
            "resources/jpegs/1.jpg")]
        public async Task StoreJpegs(string baseUri, string bearerToken, string patientId, string studyId,
            string modality, string studyDate, params string[] files)
        {
            //Arrange
            Assert.True(ValidateParameters(baseUri, bearerToken, patientId, studyId));

            var httpClient = _stowRsTestFixture
                .UseBaseUri(baseUri)
                .UseBearerToken(bearerToken)
                .Build();

            var dataset = new DicomDataset();

            dataset.AddOrUpdate(DicomTag.PatientID, patientId);
            dataset.AddOrUpdate(DicomTag.StudyID, studyId);

            //for JPEG you MUST define "Modality" and "StudyDate" tags
            dataset.AddOrUpdate(DicomTag.Modality, modality);
            dataset.AddOrUpdate(DicomTag.StudyDate, studyDate);

            var dataToStore = TestHelper.FillDataWithBlobDataUris(dataset, files.Select(Path.GetFullPath));

            var request = new HttpRequestMessage(HttpMethod.Post, _stowRsTestFixture.RequestUri)
            {
                Content = TestHelper.CreateMultipartContent(dataToStore)
            };

            //Act
            var result = await httpClient.SendAsync(request);

            //Assert
            if (result.IsSuccessStatusCode == false)
            {
                _output.WriteLine(result.ReasonPhrase);
                _output.WriteLine(await result.Content.ReadAsStringAsync());
            }

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Theory]
        [MemberData(nameof(BatchTestData))]
        public async Task StoreJpegsBatch(IEnumerable<BatchTestModel> testData)
        {
            //Arrange
            const string baseUri = "http://localhost:49799/";
            const string bearerToken =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImEzck1VZ01Gdjl0UGNsTGE2eUYzekFrZnF1RSIsImtpZCI6ImEzck1VZ01Gdjl0UGNsTGE2eUYzekFrZnF1RSJ9.eyJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjQ5Nzk5L2lkZW50aXR5IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo0OTc5OS9pZGVudGl0eS9yZXNvdXJjZXMiLCJleHAiOjE1MjQ1NjY2MzAsIm5iZiI6MTUyNDU2MzAzMCwiY2xpZW50X2lkIjoibmloLW5pYWlkLWNsaSIsInNjb3BlIjpbIm9mZmxpbmVfYWNjZXNzIiwicmVhZDpwYXRpZW50Iiwid3JpdGU6cGF0aWVudCIsImRlbGV0ZTpwYXRpZW50IiwicmVhZDpjb25kaXRpb24iLCJ3cml0ZTpjb25kaXRpb24iLCJkZWxldGU6Y29uZGl0aW9uIiwiYXBwcm92ZTpjb25kaXRpb24iXSwic3ViIjoidmFsZXJ5LmtpcnljaGVua2EuYnkiLCJhdXRoX3RpbWUiOjE1MjM3MTg2OTQsImlkcCI6Imlkc3J2IiwiYW1yIjpbIjJmYSJdfQ.WEzBEZt_uKm2Y9Ym1T-Et81J3-YvCmHAK1XyO-pemcMUY4cNpj4fST74pyYGi0YMXzk0WjrGM294I_Mp7Zxf2EBS8-r2YmBx_HjYgCmC68oUqw5Xu7LKg8TYmxeg19Jr_E2tjZl17FMxaThbAlfehVI8rTnwBBPSn7iyPAsLGHUfDeF-au0Ts0PfxFW-C9UH91cI_jJ5i1w0MeXpkSZwMbNQfXDU-XkeUdIfpjzHVheN5gsXcw6cO_2GlSzdP8hqOU_2oTMcEFAFOkGH0YLIfJhCUXS6NelBSpvzc8zrAzj8mO65ZS_xRozXibo9v34Hdv_kRcxDR1b6Y_pkudwS8A";

            var httpClient = _stowRsTestFixture
                .UseBaseUri(baseUri)
                .UseBearerToken(bearerToken)
                .Build();


            var dataToStore = new List<FileToStore>();
            foreach (var batchTestModel in testData)
            {
                var dataset = new DicomDataset();
                dataset.AddOrUpdate(DicomTag.PatientID, batchTestModel.PatientId);
                dataset.AddOrUpdate(DicomTag.StudyID, batchTestModel.CaseId);

                dataset.AddOrUpdate(DicomTag.Modality, "CR");
                dataset.AddOrUpdate(DicomTag.StudyDate, "20180101");

                dataToStore.AddRange(TestHelper.FillDataWithBlobDataUris(dataset,
                    new[] {batchTestModel.File}.Select(Path.GetFullPath)));
            }

            var request = new HttpRequestMessage(HttpMethod.Post, _stowRsTestFixture.RequestUri)
            {
                Content = TestHelper.CreateMultipartContent(dataToStore)
            };

            //Act
            var result = await httpClient.SendAsync(request);

            //Assert
            if (result.IsSuccessStatusCode == false)
            {
                _output.WriteLine(result.ReasonPhrase);
                _output.WriteLine(await result.Content.ReadAsStringAsync());
            }

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }


        private bool ValidateParameters(string baseUri, string bearerToken, string patientId, string studyId)
        {
            var result = true;
            if (baseUri == EmptyBaseUriTemplate)
            {
                _output.WriteLine(
                    "You must specify the valid base uri for STAGING or PRODUCTION. See https://github.com/ibrsp/dataentry-api-postman-collection#getting-started-by-cloning-repository");
                result = false;
            }

            if (bearerToken == EmptyBearerTokenTemplate)
            {
                _output.WriteLine(
                    "You must specify the valid bearer token. To find out how to get it, see https://github.com/ibrsp/dataentry-api-postman-collection#getting-the-authorization-token-via-postman");
                result = false;
            }

            if (patientId == EmptyPatientIdTemplate)
            {
                _output.WriteLine("You must specify the valid PATIENT ID from dataentry portal");
                result = false;
            }

            if (studyId == EmptyCaseIdTemplate)
            {
                _output.WriteLine("You must specify the valid CASE ID from dataentry portal");
                result = false;
            }

            return result;
        }

        public static IEnumerable<object[]> BatchTestData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    new List<BatchTestModel>
                    {
                        new BatchTestModel
                        {
                            PatientId = "1302f327-6c62-4935-a776-74629f285150",
                            CaseId = "7ad96241-71ac-4df2-93e0-c82d0e1e4972",
                            File = "resources/jpegs/1.jpg"
                        },
                        new BatchTestModel
                        {
                            PatientId = "1302f327-6c62-4935-a776-74629f285150",
                            CaseId = "7ad96241-71ac-4df2-93e0-c82d0e1e4972",
                            File = "resources/jpegs/2.jpg"
                        },
                        new BatchTestModel
                        {
                            PatientId = "fadab6f2-c79c-411d-b26a-2011dad3ee0b",
                            CaseId = "0e868ea3-8c6a-407f-bedc-431290dea837",
                            File = "resources/jpegs/1.jpg"
                        },
                        new BatchTestModel
                        {
                            PatientId = "4228bce9-c02b-4e20-b758-52200d492a15",
                            CaseId = "59b1146d-e2f7-4922-a602-306fca97c355",
                            File = "resources/jpegs/1.jpg"
                        },
                        new BatchTestModel
                        {
                            PatientId = "4228bce9-c02b-4e20-b758-52200d492a15",
                            CaseId = "4b6f5632-46a2-4eb5-ad1d-00da0faa721b",
                            File = "resources/jpegs/2.jpg"
                        }
                    }
                }
            };
        }
    }
}