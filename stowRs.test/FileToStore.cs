using Dicom;

namespace stowRs.test
{
    public class FileToStore
    {
        public string FilePath { get; set; }
        public string ContentLocaltionHeader { get; set; }
        public DicomDataset Metadata { get; set; }
    }
}