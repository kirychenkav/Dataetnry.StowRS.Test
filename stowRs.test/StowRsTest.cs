using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dicom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using stowRs.test.fixtures;
using Xunit;
using Xunit.Abstractions;

namespace stowRs.test
{
    [Collection("Http client collection")]
    public class StowRsTest
    {
        private readonly ITestOutputHelper _output;
        private readonly StowRsTestFixture _stowRsTestFixture;

        public StowRsTest(StowRsTestFixture stowRsTestFixture, ITestOutputHelper output)
        {
            _stowRsTestFixture = stowRsTestFixture;
            _output = output;
        }

        [Theory]
        [InlineData(Constants.EmptyBaseUriTemplate,
            Constants.EmptyBearerTokenTemplate,
            Constants.EmptyPatientIdTemplate,
            Constants.EmptyCaseIdTemplate,
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
        [InlineData(Constants.EmptyBaseUriTemplate,
            Constants.EmptyBearerTokenTemplate,
            Constants.EmptyPatientIdTemplate,
            Constants.EmptyCaseIdTemplate,
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
        [InlineData(
            Constants.EmptyBaseUriTemplate,
            Constants.EmptyBearerTokenTemplate,
            "resources/batch",
            BatchType.RequestPerPatient)]
        [InlineData(Constants.EmptyBaseUriTemplate,
            Constants.EmptyBearerTokenTemplate,
            "resources/batch",
            BatchType.RequestAllData)]
        public async Task StoreJpegsBatch(string baseUri, string bearerToken, string dir, BatchType type)
        {
            //Arrange
            var httpClient = _stowRsTestFixture
                .UseBaseUri(baseUri)
                .UseBearerToken(bearerToken)
                .Build();

            var dataToStore = new List<FileToStore>();

            foreach (var metadataFile in Directory.EnumerateFiles(dir, "metadata.json", SearchOption.AllDirectories))
            {
                var metadata = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(metadataFile));
                foreach (var metadataCase in metadata.Cases)
                {
                    var caseResponse = await httpClient.GetAsync(
                        $"api/condition/search?patient={metadata.Identifier}&registrationdate={metadataCase.RegistrationDate}");

                    var condition = (await caseResponse.Content.ReadAsAsync<IEnumerable<ConditionRecord>>()).FirstOrDefault();

                    foreach (var imagingstudy in metadataCase.ImagingStudies)
                    {
                        var dataset = new DicomDataset();
                        dataset.AddOrUpdate(DicomTag.PatientID, condition.Patient.Id);
                        dataset.AddOrUpdate(DicomTag.StudyID, condition.Id);

                        dataset.AddOrUpdate(DicomTag.Modality, imagingstudy.Modality.ToString());
                        dataset.AddOrUpdate(DicomTag.StudyDate, imagingstudy.StudyDate.ToString());

                        var fileDirectory = Path.GetFullPath(Path.GetDirectoryName(metadataFile));

                        dataToStore.AddRange(TestHelper.FillDataWithBlobDataUris(dataset,
                            ((JArray)imagingstudy.Files).ToObject<string[]>().Select(f => Path.Combine(fileDirectory, f))));
                    }
                }

                if (type == BatchType.RequestPerPatient)
                {
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

                    dataToStore = new List<FileToStore>();
                }
            }

            if (type == BatchType.RequestAllData)
            {
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

        }


        private bool ValidateParameters(string baseUri, string bearerToken, string patientId, string studyId)
        {
            var result = true;
            if (baseUri == Constants.EmptyBaseUriTemplate)
            {
                _output.WriteLine(
                    "You must specify the valid base uri for STAGING or PRODUCTION. See https://github.com/ibrsp/dataentry-api-postman-collection#getting-started-by-cloning-repository");
                result = false;
            }

            if (bearerToken == Constants.EmptyBearerTokenTemplate)
            {
                _output.WriteLine(
                    "You must specify the valid bearer token. To find out how to get it, see https://github.com/ibrsp/dataentry-api-postman-collection#getting-the-authorization-token-via-postman");
                result = false;
            }

            if (patientId == Constants.EmptyPatientIdTemplate)
            {
                _output.WriteLine("You must specify the valid PATIENT ID from dataentry portal");
                result = false;
            }

            if (studyId == Constants.EmptyCaseIdTemplate)
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