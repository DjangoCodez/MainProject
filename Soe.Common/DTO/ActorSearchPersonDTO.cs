using SoftOne.Soe.Common.Util;
using System;


namespace SoftOne.Soe.Common.DTO
{
    public class ActorSearchPersonDTO
    {
        public int RecordId { get; set; }

        public string Number { get; set; }
        public string Name { get; set; }
        public string OrgNr { get; set; }
        public bool IsPrivatePerson { get; set; }
        public SoeEntityType EntityType { get; set; }
        public string EntityTypeName { get; set; }
        public bool HasConsent { get; set; }
        public string HasConsentString { get; set; }
        public DateTime? ConsentDate { get; set; }
    }
}
