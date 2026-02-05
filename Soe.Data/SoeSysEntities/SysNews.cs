namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class SysNews : SysEntity
    {
        public int SysNewsId { get; set; }

        public int SysXEArticleId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(512)]
        public string Title { get; set; }

        [StringLength(512)]
        public string Link { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Description { get; set; }

        public DateTime PubDate { get; set; }

        [Column(TypeName = "image")]
        public byte[] Image { get; set; }

        [Column(TypeName = "image")]
        public byte[] Attachment { get; set; }

        [StringLength(512)]
        public string AttachmentFileName { get; set; }

        [StringLength(100)]
        public string AttachmentImageSrc { get; set; }

        public bool IsPublic { get; set; }

        [StringLength(255)]
        public string Author { get; set; }

        public int AttachmentExportType { get; set; }

        public int State { get; set; }

        [StringLength(512)]
        public string Preview { get; set; }

        public int SysLanguageId { get; set; }

        public int DisplayType { get; set; }

        public virtual SysLanguage SysLanguage { get; set; }
    }
}
