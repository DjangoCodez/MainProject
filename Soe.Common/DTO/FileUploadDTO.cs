using SoftOne.Soe.Common.Attributes;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class TagDTO
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class UploadFileDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public List<TagDTO> Tags { get; set; }
    }

    [TSInclude]
    public class DownloadFileDTO
    {
        public bool Success { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
        public string FileType { get; set; }
        public string ErrorMessage { get; set; }
        public byte[] BinaryData { get; set; }
    }
}
