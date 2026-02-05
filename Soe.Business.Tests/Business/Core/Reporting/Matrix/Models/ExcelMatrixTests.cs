using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Tests
{
    [TestClass()]
    public class ExcelMatrixTests
    {
        [TestMethod()]
        public void GetExcelFileTest()
        {
            MatrixResult matrixResult = new MatrixResult();
            matrixResult.MatrixDefinition = new MatrixDefinition();

            List<MatrixDefinitionColumn> matrixDefinitionColumns = new List<MatrixDefinitionColumn>();
            TimeTransactionMatrix matrix = new TimeTransactionMatrix(new InputMatrix(new List<GenericType>(), (new PermissionParameterObject(null, 1, 0, 0, null), new PermissionCacheRepository(1)), TermGroup_ReportExportType.MatrixExcel, new List<AccountDimDTO>(), null), new Reporting.Models.Time.TimeTransactionReportDataOutput(null));

            var possibleColumns = matrix.GetMatrixLayoutColumns();
            foreach (var item in possibleColumns)
            {
                matrixDefinitionColumns.Add(new MatrixDefinitionColumn()
                {
                    Key = Guid.NewGuid(),
                    Field = item.Field,
                    MatrixDataType = item.MatrixDataType,
                    Title = item.Title,
                });
            }

            foreach (var item in matrixDefinitionColumns)
                matrixResult.MatrixDefinition.MatrixDefinitionColumns.Add(item);

            int rows = 100000;
            int row = 1;
            int add = 0;
            var date = new DateTime(2020, 1, 1);

            while (row <= rows)
            {

                List<MatrixField> fields = new List<MatrixField>();
                foreach (var column in matrixDefinitionColumns)
                {
                    object value = null;

                    switch (column.MatrixDataType)
                    {
                        case MatrixDataType.String:
                            value = "Test";
                            break;
                        case MatrixDataType.Integer:
                            value = 1;
                            break;
                        case MatrixDataType.Decimal:
                            value = decimal.Add(new decimal(1.1), add);
                            break;
                        case MatrixDataType.Date:
                            value = date.AddMinutes(add);
                            break;
                        case MatrixDataType.Boolean:
                            value = true;
                            break;
                        case MatrixDataType.Time:
                            value = date.AddMinutes(add);
                            break;
                        case MatrixDataType.DateAndTime:
                            value = date.AddMinutes(add);
                            break;
                        default:
                            break;

                    }

                    add++;

                    fields.Add(new MatrixField(row, column.Key, value, column.MatrixDataType));
                }


                matrixResult.MatrixFields.AddRange(fields);
                row++;

            }

            var arr = ExcelMatrix.GetExcelFile(matrixResult, "GetExcelFileTest");

            Assert.IsNotNull(arr);
            Assert.IsTrue(arr.Length > 1000);
            File.WriteAllBytes($"C:\\Temp\\GetExcelFileTest{DateTime.Now.Millisecond}.xlsx", arr);
        }
    }
}