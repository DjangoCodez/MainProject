using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Core
{
    public class UserCopyItem
    {
        public string LoginName { get; set; }
        public Guid IdLoginGuid { get; set; }
        public string Name { get; set; }
        public string LicenseName { get; set; }
        public string DefaultCompanyName { get; set; }
        public string DefaultRoleName { get; set; }
        public List<UserRoleCopyItem> UserRoleCopyItems { get; set; }
        public string EstatusLoginId { get;  set; }
    }

    public class UserRoleCopyItem
    {
        public RoleCopyItem RoleCopyItem { get; set; }
        public UserCopyItem UserCopyItem { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
