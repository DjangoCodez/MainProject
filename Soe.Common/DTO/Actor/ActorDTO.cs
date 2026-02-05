using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO
{
    public class ActorConsentGridDTO
    {
        public int ActorId { get; set; }
        public string ActorName { get; set; }
        public bool HasConsent { get; set; }
        public int ConsentType { get; set; }
        public SoeActorType ActorType { get; set; }
        public string ActorTypeName { get; set; }
        public string Email { get; set; }
        public bool HasConnectedInvoices { get; set; }
        public string HasConnectedInvoicesName { get; set; }
    }
}
