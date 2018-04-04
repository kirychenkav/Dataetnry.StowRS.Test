using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dicom;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
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

        private bool ValidateParameters(string baseUri, string bearerToken, string patientId, string studyId)
        {
            var result = true;
            if (baseUri == EmptyBaseUriTemplate)
            {
                _output.WriteLine("You must specify the valid base uri for STAGING or PRODUCTION. See https://github.com/ibrsp/dataentry-api-postman-collection#getting-started-by-cloning-repository");
                result = false;
            }

            if (bearerToken == EmptyBearerTokenTemplate)
            {
                _output.WriteLine("You must specify the valid bearer token. To find out how to get it, see https://github.com/ibrsp/dataentry-api-postman-collection#getting-the-authorization-token-via-postman");
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
    }
}