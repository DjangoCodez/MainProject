using System;

namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface IUserCompanyRole
    {
        int UserId { get; }
        int RoleId { get; }
        int ActorCompanyId { get; }
        bool Default { get; }
        DateTime? DateFrom { get; }
        DateTime? DateTo { get; }
        int StateId { get; }
    }
}
