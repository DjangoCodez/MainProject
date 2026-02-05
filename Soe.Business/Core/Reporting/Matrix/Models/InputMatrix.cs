using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class InputMatrix
    {
        public List<GenericType> Terms { get; set; }
        public PermissionParameterObject PermissionParam { get; set; }
        public PermissionCacheRepository PermissionRepository { get; set; }
        public MatrixColumnsSelectionDTO MatrixColumnsSelection { get; set; }
        public List<AccountDimDTO> AccountDims { get; set; }
        public List<AccountDTO> AccountInternals { get; set; }
        public List<ExtraFieldDTO> ExtraFields { get; set; }
        public TermGroup_ReportExportType ExportType { get; set; }

        public InputMatrix(List<GenericType> terms, (PermissionParameterObject Param, PermissionCacheRepository Repository) permissions, TermGroup_ReportExportType exportType, List<AccountDimDTO> accountDims, MatrixColumnsSelectionDTO matrixColumnsSelection, List<AccountDTO> accountInternals = null, List<ExtraFieldDTO> extraFields = null)
        {
            Terms = terms;
            PermissionParam = permissions.Param;
            PermissionRepository = permissions.Repository;
            ExportType = exportType;
            AccountDims = accountDims;
            AccountInternals = accountInternals;
            MatrixColumnsSelection = matrixColumnsSelection;
            ExtraFields = extraFields;
        }
    }
}
