using SoftOne.Soe.Common.Attributes;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class FileImportHeadGridDTO
    {
        public int FileImportHeadId { get; set; }
        public string FileName { get; set; }
        public string SystemMessage { get; set; }
        public string Comment { get; set; }
        public int Status { get; set; }
        public string StatusStr { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class FileImportHeadDTO
    {
        public int FileImportHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public Guid? BatchId { get; set; }
        public int EntityType { get; set; }
        public string FileName { get; set; }
        public string SystemMessage { get; set; }
        public string Comment { get; set; }
        public int Status { get; set; }
    }
}
