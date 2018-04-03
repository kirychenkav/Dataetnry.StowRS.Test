using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Dicom;
using Dicom.IO.Buffer;
using Dicom.Serialization;
using Newtonsoft.Json;

namespace stowRs.test
{
    public class TestHelper
    {
        private static Dictionary<string, string> _mimeDict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {".dcm", "application/dicom"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"}
        };

        /// <summary>
        ///     Get a valid multipart content.
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        private static MultipartContent GetMultipartContent(string mimeType)
        {
            var multiContent = new MultipartContent("related", "DICOM DATA BOUNDARY");

            multiContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("type", "\"" + mimeType + "\""));
            return multiContent;
        }

        /// <summary>
        ///     Get a Stream from string content
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static MultipartContent CreateMultipartContent(IEnumerable<FileToStore> files)
        {
            var multipartContent = GetMultipartContent("application/dicom+json");

            var metadataString = JsonConvert.SerializeObject(files.Select(f => f.Metadata), Formatting.Indented,
                new JsonDicomConverter());

            var jsonContent = new StreamContent(GenerateStreamFromString(metadataString));
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/dicom+json");
            multipartContent.Add(jsonContent);

            foreach (var file in files)
            {
                var sContent = new StreamContent(File.OpenRead(file.FilePath));

                string mime;
                mime = _mimeDict.TryGetValue(Path.GetExtension(file.FilePath), out mime)
                    ? mime
                    : "application/octet-stream";


                sContent.Headers.ContentType = new MediaTypeHeaderValue(mime);
                sContent.Headers.ContentLocation = new Uri(file.ContentLocaltionHeader);

                multipartContent.Add(sContent);
            }

            return multipartContent;
        }

        public static IEnumerable<FileToStore> FillDataWithBlobDataUris(DicomDataset dataset, IEnumerable<string> files)
        {
            var result = new List<FileToStore>();
            foreach (var enumerateFile in files)
            {
                var uri = $"urn:uuid:{Guid.NewGuid()}";

                var bulkDataUri = new BulkDataUriByteBuffer(uri);
                var pixelData = new DicomOtherWord(DicomTag.PixelData, bulkDataUri);
                dataset.AddOrUpdate(pixelData);

                result.Add(new FileToStore
                {
                    ContentLocaltionHeader = uri,
                    FilePath = enumerateFile,
                    Metadata = new DicomDataset(dataset)
                });
            }

            return result;
        }
    }
}