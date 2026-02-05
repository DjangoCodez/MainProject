using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class Import : ICreatedModified, IState
    {
        public int? ImportHeadType { get; set; }
        public string HeadName { get; set; }
        public TermGroup_SysImportDefinitionType Type { get; set; }
        public string TypeText { get; set; }
        public string IsStandardText { get; set; }
        public string SpecialFunctionality { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region Import

        public static ImportDTO ToDTO(this Import e)
        {
            if (e == null)
                return null;

            return new ImportDTO()
            {
                ImportId = e.ImportId,
                ActorCompanyId = e.ActorCompanyId,
                ImportDefinitionId = e.ImportDefinitionId,
                Module = e.Module,
                Name = e.Name,
                HeadName = e.HeadName,
                IsStandard = e.Standard,
                IsStandardText = e.IsStandardText,
                State = (SoeEntityState)e.State,
                ImportHeadType = e.ImportHeadType != null ? (TermGroup_IOImportHeadType)e.ImportHeadType : TermGroup_IOImportHeadType.Unknown,
                Type = e.Type,
                TypeText = e.TypeText,
                UseAccountDistribution = e.UseAccountDistribution,
                UseAccountDimensions = e.UseAccountDims,
                UpdateExistingInvoice = e.UpdateExistingInvoice,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                Guid = e.Guid,
                AccountYearId = e.AccountYearId,
                VoucherSeriesId = e.VoucherSeriesId,
                SpecialFunctionality = e.SpecialFunctionality,
                Dim1AccountId = e.Dim1AccountId,
                Dim2AccountId = e.Dim2AccountId,
                Dim3AccountId = e.Dim3AccountId,
                Dim4AccountId = e.Dim4AccountId,
                Dim5AccountId = e.Dim5AccountId,
                Dim6AccountId = e.Dim6AccountId
            };
        }

        public static IEnumerable<ImportDTO> ToDTOs(this IEnumerable<Import> l)
        {
            var dtos = new List<ImportDTO>();
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
    }
}
