using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface
{
    interface IMatrixModel
    {
        MatrixResult GetMatrixResult();
        List<MatrixDefinitionColumn> GetMatrixDefinitionColumns();
        List<MatrixLayoutColumn> GetMatrixLayoutColumns();
    }
}
