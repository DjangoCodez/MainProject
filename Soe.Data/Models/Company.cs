using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Company : ICreatedModified, IState
    {
        public bool ShowSupportLogin { get; set; }
    }

    public partial class CompanyLogo : ICompanyLogo
    {
        public string FileName
        {
            get
            {
                string fileName = "";
                if (this.Company != null)
                    fileName = "logo_company_" + this.Company.ActorCompanyId + "_id_" + this.ImageId;
                return fileName;
            }
        }
        public string FileNameWithExtension
        {
            get
            {
                string fileName = "";
                if (this.Company != null)
                    fileName = "logo_company_" + this.Company.ActorCompanyId + "_id_" + this.ImageId + this.Extension;
                return fileName;
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region Company

        public static CompanyDTO ToCompanyDTO(this Company e)
        {
            if (e == null)
                return null;

            return new CompanyDTO
            {
                ActorCompanyId = e.ActorCompanyId,
                _companyGuid = e.CompanyGuid,
                Number = e.CompanyNr,
                Name = e.Name,
                ShortName = e.ShortName,
                OrgNr = e.OrgNr,
                VatNr = e.VatNr,
                Language = e.SysCountryId.HasValue ? (TermGroup_Languages)e.SysCountryId.Value : TermGroup_Languages.Unknown,
                AllowSupportLogin = e.AllowSupportLogin == true,
                AllowSupportLoginTo = e.AllowSupportLoginTo ?? DateTime.MinValue,
                LicenseId = e.LicenseId,
                LicenseNr = e.License?.LicenseNr ?? string.Empty,
                _licenseGuid = e.License?.LicenseGuid ?? Guid.Empty,
                LicenseSupport = e.License?.Support ?? false,
                Template = e.Template,
                Global = e.Global,
                SysCountryId = e.SysCountryId,
                TimeSpotId = e.TimeSpotId,
                Demo = e.Demo,
            };
        }

        public static CompanyDTO Clone(this CompanyDTO e)
        {
            if (e == null)
                return null;

            return new CompanyDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                Number = e.Number,
                Name = e.Name,
                ShortName = e.ShortName,
                OrgNr = e.OrgNr,
                VatNr = e.VatNr,
                Language = e.Language,
                AllowSupportLogin = e.AllowSupportLogin,
                AllowSupportLoginTo = e.AllowSupportLoginTo,
                LicenseId = e.LicenseId,
                LicenseNr = e.LicenseNr,
                LicenseSupport = e.LicenseSupport,
                Template = e.Template,
                Global = e.Global,
                SysCountryId = e.SysCountryId,
                TimeSpotId = e.TimeSpotId,
                Demo = e.Demo,
            };
        }

        public static IEnumerable<CompanyDTO> ToCompanyDTOs(this IEnumerable<Company> e)
        {
            return e.Select(s => s.ToCompanyDTO()).ToList();
        }

        public static SmallGenericType ToSmallGenericType(this Company e)
        {
            if (e == null)
                return null;

            return new SmallGenericType()
            {
                Id = e.ActorCompanyId,
                Name = e.Name,
            };
        }

        public static IEnumerable<SmallGenericType> ToSmallGenericTypes(this IEnumerable<Company> e)
        {
            return e.Select(s => s.ToSmallGenericType()).ToList();
        }

        public static bool IsSupportLoginAllowed(this Company e)
        {
            bool allowSupportLogin = false;
            if (e.AllowSupportLogin.HasValue && e.AllowSupportLogin.Value)
                allowSupportLogin = e.AllowSupportLoginTo.HasValue && e.AllowSupportLoginTo.Value >= DateTime.Now;

#if DEBUG
            // Always allow when debugging
            allowSupportLogin = true;
#endif

            return allowSupportLogin;
        }

        public static string GetName(this IEnumerable<Company> companies, int actorCompanyId)
        {
            return companies?.FirstOrDefault(c => c.ActorCompanyId == actorCompanyId)?.Name ?? string.Empty;
        }

        #endregion

        #region CompanyExternalCode

        public static CompanyExternalCodeDTO ToDTO(this CompanyExternalCode e)
        {
            if (e == null)
                return null;

            return new CompanyExternalCodeDTO()
            {
                CompanyExternalCodeId = e.CompanyExternalCodeId,
                ActorCompanyId = e.ActorCompanyId,
                ExternalCode = e.ExternalCode,
                RecordId = e.RecordId,
                Entity = (TermGroup_CompanyExternalCodeEntity)e.Entity
            };
        }

        public static List<CompanyExternalCodeDTO> ToDTOs(this IEnumerable<CompanyExternalCode> l)
        {
            List<CompanyExternalCodeDTO> dtos = new List<CompanyExternalCodeDTO>();
            if (l != null)
            {
                foreach (CompanyExternalCode e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static CompanyExternalCodeGridDTO ToGridDTO(this CompanyExternalCode e)
        {
            if (e == null)
                return null;

            return new CompanyExternalCodeGridDTO()
            {
                CompanyExternalCodeId = e.CompanyExternalCodeId,
                ExternalCode = e.ExternalCode,
                RecordId = e.RecordId,
                Entity = (TermGroup_CompanyExternalCodeEntity)e.Entity
            };
        }

        public static List<CompanyExternalCodeGridDTO> ToGridDTOs(this IEnumerable<CompanyExternalCode> l)
        {
            List<CompanyExternalCodeGridDTO> dtos = new List<CompanyExternalCodeGridDTO>();
            if (l != null)
            {
                foreach (CompanyExternalCode e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static List<CompanyExternalCode> Filter(this List<CompanyExternalCode> l, TermGroup_CompanyExternalCodeEntity entity)
        {
            return l?.Where(i => i.Entity == (int)entity).ToList() ?? new List<CompanyExternalCode>();
        }

        public static CompanyExternalCode Get(this List<CompanyExternalCode> l, TermGroup_CompanyExternalCodeEntity entity, int recordId)
        {
            return l?.FirstOrDefault(i => i.Entity == (int)entity && i.RecordId == recordId);
        }

        #endregion

        #region CompanyGroupAdministration

        public static CompanyGroupAdministrationGridDTO ToGridDTO(this CompanyGroupAdministration e)
        {
            if (e == null)
                return null;

            try
            {
                if (!e.CompanyReference.IsLoaded)
                {
                    e.CompanyReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Company.cs e.CompanyReference");
                }
                if (!e.CompanyGroupMappingHeadReference.IsLoaded)
                {
                    e.CompanyGroupMappingHeadReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Company.cs e.CompanyGroupMappingHeadReference");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            return new CompanyGroupAdministrationGridDTO()
            {
                CompanyGroupAdministrationId = e.CompanyGroupAdministrationId,
                GroupCompanyActorCompanyId = e.GroupCompanyActorCompanyId,
                ChildActorCompanyId = e.ChildActorCompanyId,
                ChildCompanyName = e.ChildActorCompanyName,
                ChildCompanyNr = e.ChildActorCompanyNr,
                CompanyGroupMappingHeadId = e.CompanyGroupMappingHeadId,
                MappingHeadName = e.CompanyGroupMappingHead?.Name ?? string.Empty,
                AccountId = e.AccountId,
                Conversionfactor = e.Conversionfactor,
                Note = e.Note,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<CompanyGroupAdministrationGridDTO> ToGridDTOs(this IEnumerable<CompanyGroupAdministration> l)
        {
            var dtos = new List<CompanyGroupAdministrationGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static CompanyGroupAdministrationDTO ToDTO(this CompanyGroupAdministration e)
        {
            if (e == null)
                return null;

            return new CompanyGroupAdministrationDTO()
            {
                CompanyGroupAdministrationId = e.CompanyGroupAdministrationId,
                GroupCompanyActorCompanyId = e.GroupCompanyActorCompanyId,
                ChildActorCompanyId = e.ChildActorCompanyId,
                ChildActorCompanyName = e.ChildActorCompanyName,
                ChildActorCompanyNr = e.ChildActorCompanyNr,
                CompanyGroupMappingHeadId = e.CompanyGroupMappingHeadId,
                AccountId = e.AccountId,
                Conversionfactor = e.Conversionfactor,
                MatchInternalAccountOnNr = e.MatchInternalAccountsOnNr,
                Note = e.Note,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static List<CompanyGroupAdministrationDTO> ToDTOs(this List<CompanyGroupAdministration> l)
        {
            var dtos = new List<CompanyGroupAdministrationDTO>();
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

        #region CompanyGroupMappingHead

        public static CompanyGroupMappingHeadDTO ToDTO(this CompanyGroupMappingHead e, bool includeRows)
        {
            if (e == null)
                return null;

            CompanyGroupMappingHeadDTO dto = new CompanyGroupMappingHeadDTO()
            {
                CompanyGroupMappingHeadId = e.CompanyGroupMappingHeadId,
                ActorCompanyId = e.ActorCompanyId,
                Number = e.Number,
                Name = e.Name,
                Description = e.Description,
                Type = e.Type,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (includeRows)
                dto.Rows = e.CompanyGroupMappingRow?.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<CompanyGroupMappingRowDTO>();

            return dto;
        }

        public static List<CompanyGroupMappingHeadDTO> ToDTOs(this List<CompanyGroupMappingHead> l, bool includeRows)
        {
            var dtos = new List<CompanyGroupMappingHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows));
                }
            }
            return dtos;
        }

        public static CompanyGroupMappingRowDTO ToDTO(this CompanyGroupMappingRow e)
        {
            if (e == null)
                return null;

            return new CompanyGroupMappingRowDTO()
            {
                CompanyGroupMappingRowId = e.CompanyGroupMappingRowId,
                CompanyGroupMappingHeadId = e.CompanyGroupMappingHeadId,
                ChildAccountFrom = e.ChildAccountFrom,
                ChildAccountTo = e.ChildAccountTo,
                GroupCompanyAccount = e.GroupCompanyAccount,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<CompanyGroupMappingRowDTO> ToDTOs(this IEnumerable<CompanyGroupMappingRow> l)
        {
            var dtos = new List<CompanyGroupMappingRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<CompanyGroupMappingRow> GetCompanyGroupMappingRows(this CompanyGroupMappingHead e)
        {
            return e?.CompanyGroupMappingRow?.Where(vr => vr.State == (int)SoeEntityState.Active).ToList();
        }

        #endregion
    }
}
