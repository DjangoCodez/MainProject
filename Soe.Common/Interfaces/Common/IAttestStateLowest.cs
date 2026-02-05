
namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface IAttestStateLowest
    {
        int AttestStateId { get; set; }
        int AttestStateSort { get; set; }
        string AttestStateColor { get; set; }
        string AttestStateName { get; set; }
    }
}
