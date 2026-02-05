using System;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models
{
    public class UserItem
    {
        public string LoginName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string EmployeeNr { get; set; }
        public string Roles { get; set; }
        public string AttestRoles { get; set; }
        public string AttestRoleAccount { get; set; }
        public DateTime? DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? DateModified { get; set; }
        public string ModifiedBy { get; set; }
        public int State { get; set; }
        public bool IsActive { get; set; }
        public bool IsMobileUser { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? RoleDateFrom { get; set; }
        public DateTime? RoleDateTo { get; set; }
        public DateTime? AttestRoleDateFrom { get; set; }
        public DateTime? AttestRoleDateTo { get; set; }
        public bool ShowAllCategories { get; set; }
        public bool ShowUncategorized { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }

        public string GroupOn(List<TermGroup_UserMatrixColumns> columns)
        {
            StringBuilder sb = new StringBuilder(string.Empty);

            foreach (var column in columns)
            {
                switch (column)
                {
                    case TermGroup_UserMatrixColumns.EmployeeNr:
                        sb.Append($"#{this.EmployeeNr}");
                        break;
                    case TermGroup_UserMatrixColumns.LoginName:
                        sb.Append($"#{this.LoginName}");
                        break;
                    case TermGroup_UserMatrixColumns.AttestRoles:
                        sb.Append($"#{this.AttestRoles}");
                        break;
                    case TermGroup_UserMatrixColumns.AttestRoleAccount:
                        sb.Append($"#{this.AttestRoleAccount}");
                        break;
                    case TermGroup_UserMatrixColumns.AttestRoleDateFrom:
                        sb.Append($"#{this.AttestRoleDateFrom}");
                        break;
                    case TermGroup_UserMatrixColumns.AttestRoleDateTo:
                        sb.Append($"#{this.AttestRoleDateTo}");
                        break;
                    case TermGroup_UserMatrixColumns.Roles:
                        sb.Append($"#{this.Roles}");
                        break;
                    case TermGroup_UserMatrixColumns.RoleDateFrom:
                        sb.Append($"#{this.RoleDateFrom}");
                        break;
                    case TermGroup_UserMatrixColumns.RoleDateTo:
                        sb.Append($"#{this.RoleDateTo}");
                        break;
                    case TermGroup_UserMatrixColumns.AttestRoleAccountName:
                        sb.Append($"#{this.AccountId}");
                        break;
                    case TermGroup_UserMatrixColumns.ShowAllCategories:
                    case TermGroup_UserMatrixColumns.ShowUncategorized:
                        break;
                    default:
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
