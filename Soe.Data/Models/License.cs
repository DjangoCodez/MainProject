using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class License : ICreatedModified, IState
    {
        public string SysServerUrl { get; set; }
        public int CurrentConcurrentUsers { get; set; }
        public bool OnOtherServer { get; set; }
        public string EditUrl { get; set; }
        public string CompaniesUrl { get; set; }
        public string UsersUrl { get; set; }
        public string LicenseNrSort
        {
            get { return LicenseNr.PadLeft(15, '0'); }
        }
    }

    public static partial class EntityExtensions
    {
        #region License

        public static LicenseDTO ToDTO(this License e)
        {
            if (e == null)
                return null;

            LicenseDTO licenseDTO = new LicenseDTO()
            {
                LicenseId = e.LicenseId,
                LicenseNr = e.LicenseNr,
                Name = e.Name,
                OrgNr = e.OrgNr,
                Support = e.Support,
                NrOfCompanies = e.NrOfCompanies,
                MaxNrOfUsers = e.MaxNrOfUsers,
                MaxNrOfEmployees = e.MaxNrOfEmployees,
                MaxNrOfMobileUsers = e.MaxNrOfMobileUsers,
                ConcurrentUsers = e.ConcurrentUsers,
                TerminationDate = e.TerminationDate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = e.State,
                AllowDuplicateUserLogin = e.AllowDuplicateUserLogin,
                LegalName = e.LegalName,
                IsAccountingOffice = e.IsAccountingOffice,
                AccountingOfficeId = e.AccountingOfficeId,
                AccountingOfficeName = e.AccountingOfficeName,
                SysServerId = e.SysServerId,
                LicenseGuid = e.LicenseGuid,
            };
            return licenseDTO;

        }

        public static List<LicenseDTO> ToDTOs(this List<License> l)
        {
            var dtos = new List<LicenseDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;

        }

        #endregion

        #region LicenseArticle

        public static List<LicenseArticleDTO> ToDTOs(this List<LicenseArticle> e)
        {
            List<LicenseArticleDTO> dtos = new List<LicenseArticleDTO>();

            foreach (var l in e)
                dtos.Add(l.ToDTO());

            return dtos;
        }

        public static LicenseArticleDTO ToDTO(this LicenseArticle e)
        {
            if (e == null)
                return null;

            return new LicenseArticleDTO()
            {
                LicenseArticleId = e.LicenseArticleId,
                LicenseId = e.License.LicenseId,
                SysXEArticleId = e.SysXEArticleId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };
        }

        #endregion
    }
}
