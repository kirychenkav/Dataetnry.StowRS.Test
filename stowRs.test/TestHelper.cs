using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace stowRs.test
{
    public class TestHelper
    {
        public static string DicomMimeType = "application/dicom";
        public static string JpegMimeType = "image/jpeg";

        /// <summary>
        ///     Get a valid multipart content.
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public static MultipartContent GetMultipartContent(string mimeType)
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
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static MultipartContent CreateMultipartContent(string mimeType, string metadata,
            IEnumerable<FileToStore> files)
        {
            var multipartContent = GetMultipartContent("application/dicom+json");

            var jsonContent = new StreamContent(GenerateStreamFromString(metadata));
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/dicom+json");
            multipartContent.Add(jsonContent);

            foreach (var file in files)
            {
                var sContent = new StreamContent(File.OpenRead(file.File));

                sContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                sContent.Headers.ContentLocation = new Uri(file.BlobDataUri);

                multipartContent.Add(sContent);
            }

            return multipartContent;
        }
    }
}