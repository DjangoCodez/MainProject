namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysEdiMessageRaw")]
    public partial class SysEdiMessageRaw : SysEntity
    {
        public int SysEdiMessageRawId { get; set; }

        public int? SysWholesellerId { get; set; }

        public int? SysCompanyId { get; set; }

        public DateTime? Time { get; set; }

        [StringLength(500)]
        public string Filename { get; set; }

        [StringLength(50)]
        public string Number { get; set; }

        [StringLength(128)]
        public string Name { get; set; }

        public string FileString { get; set; }

        public int Status { get; set; }

        public Guid? Guid { get; set; }

        public string ErrorMessage { get; set; }

        public long Size { get; set; }

        public int Rows { get; set; }

        [StringLength(128)]
        public string Checksum { get; set; }

        public string XDocument { get; set; }

        public int State { get; set; }
    }
}
