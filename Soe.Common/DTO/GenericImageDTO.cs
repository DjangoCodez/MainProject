using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class GenericImageDTO
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        public List<byte[]> Images { get; set; }
        public SoeDataStorageRecordType ImageFormatType { get; set; }
        public string Filename { get; set; }
        public string Description { get; set; }
        public string ConnectedTypeName { get; set; }
        public List<InvoiceAttachmentDTO> InvoiceAttachments { get; set; }
        public InvoiceAttachmentSourceType SourceType { get; set; }
        public ImageFormatType Format { get; set; }
    }
}
