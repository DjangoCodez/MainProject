using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO.SoftOneId
{
    public class IdSuperKeyDTO
    {
        public int IdSuperKeyId { get; set; }
        public string SuperKey { get; set; }
        public Guid SuperKeyGuid { get; set; }
        public IdSuperKeyType Type { get; set; }
        public bool Used { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime? ValidatedTime { get; set; }
    }
}
