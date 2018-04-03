using Dicom;

namespace stowRs.test
{
    public class FileToStore
    {
        public string File { get; set; }
        public string BlobDataUri { get; set; }
        public DicomDataset Metadata { get; set; }
    }
}