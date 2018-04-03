using System;
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
        private readonly StowRsTestFixture _stowRsTestFixture;
        private readonly ITestOutputHelper _output;

        public StowRsTest(StowRsTestFixture stowRsTestFixture, ITestOutputHelper output)
        {
            _stowRsTestFixture = stowRsTestFixture;
            _output = output;
        }

        [Theory]
        [InlineData("application/dicom", "resources/dicoms/1.3.6.1.4.1.25403.207732457674374.13668.20141127075926.10.dcm",
            "resources/dicoms/1.3.6.1.4.1.25403.207732457674374.13668.20141127075926.11.dcm")]
        public async Task StoreDicoms(string mimeType, params string[] files)
        {
            //Arrange
            var dataset = new DicomDataset();

            dataset.AddOrUpdate(DicomTag.PatientID, _stowRsTestFixture.PatientId);
            dataset.AddOrUpdate(DicomTag.StudyID, _stowRsTestFixture.ConditionId);

            var dataToStore = TestHelper.FillDataWithBlobDataUris(dataset, files.Select(Path.GetFullPath));

            var request = new HttpRequestMessage(HttpMethod.Post, _stowRsTestFixture.RequestUri)
            {
                Content = TestHelper.CreateMultipartContent(mimeType, dataToStore)
            };

            //Act
            var result = await _stowRsTestFixture.HttpClient.SendAsync(request);

            //Assert
            _output.WriteLine(result.ReasonPhrase);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Theory]
        [InlineData("image/jpeg", "resources/jpegs/1.jpg")]
        public async Task StoreJpegs(string mimeType, params string[] files)
        {
            //Arrange
            var dataset = new DicomDataset();

            dataset.AddOrUpdate(DicomTag.PatientID, _stowRsTestFixture.PatientId);
            dataset.AddOrUpdate(DicomTag.StudyID, _stowRsTestFixture.ConditionId);
            
            //for JPEG you MUST define "Modality" and "StudyDate" tags
            dataset.AddOrUpdate(DicomTag.Modality, "CR");
            dataset.AddOrUpdate(DicomTag.StudyDate, "20180101");

            var dataToStore = TestHelper.FillDataWithBlobDataUris(dataset, files.Select(Path.GetFullPath));

            var request = new HttpRequestMessage(HttpMethod.Post, _stowRsTestFixture.RequestUri)
            {
                Content = TestHelper.CreateMultipartContent(mimeType, dataToStore)
            };

            //Act
            var result = await _stowRsTestFixture.HttpClient.SendAsync(request);

            //Assert
            _output.WriteLine(result.ReasonPhrase);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}