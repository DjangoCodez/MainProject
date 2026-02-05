using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class AgiAbsenceMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "AgiAbsenceMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<AgiAbsenceReportDataField> filter { get; set; }
        List<AgiAbsenceItem> agiAbsenceItems { get; set; }
        #endregion

        public AgiAbsenceMatrix(InputMatrix inputMatrix, AgiAbsenceReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            agiAbsenceItems = reportDataOutput?.AgiAbsenceItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            bool socialSecPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec);
           
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AgiAbsenceMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AgiAbsenceMatrixColumns.EmployeeName));
            if(socialSecPermission)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AgiAbsenceMatrixColumns.SocialSec));
            
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_AgiAbsenceMatrixColumns.PaymentDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_AgiAbsenceMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AgiAbsenceMatrixColumns.ProductNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AgiAbsenceMatrixColumns.ProductName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AgiAbsenceMatrixColumns.Type));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AgiAbsenceMatrixColumns.Quantity));

            return possibleColumns;
        }

        public List<MatrixDefinitionColumn> GetMatrixDefinitionColumns()
        {
            if (definitionColumns.IsNullOrEmpty())
            {
                List<MatrixDefinitionColumn> matrixDefinitionColumns = new List<MatrixDefinitionColumn>();

                List<MatrixLayoutColumn> possibleColumns = GetMatrixLayoutColumns();

                if (filter != null)
                {
                    int columnNumber = 0;
                    foreach (var field in filter.OrderBy(o => o.Sort))
                    {
                        MatrixLayoutColumn item = possibleColumns.FirstOrDefault(w => w.Field == field.ColumnKey);

                        if (item != null)
                        {
                            columnNumber++;
                            matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item, columnNumber, field.Selection?.Options != null ? field.Selection.Options : item.Options));
                        }
                    }
                }
                else
                {
                    foreach (MatrixLayoutColumn item in possibleColumns)
                    {
                        matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item.MatrixDataType, item.Field, item.Title, item.Options));
                    }
                }

                definitionColumns = matrixDefinitionColumns;
            }
            return definitionColumns;
        }

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_AgiAbsenceMatrixColumns column, MatrixDefinitionColumnOptions options = null)
        {
            MatrixLayoutColumn matrixLayoutColumn = new MatrixLayoutColumn(dataType, EnumUtility.GetName(column), GetText((int)column, EnumUtility.GetName(column)), options);
            if (IsAccountInternal(column))
            {
                var name = GetAccountInternalName(column, 1);
                if (!string.IsNullOrEmpty(name))
                    matrixLayoutColumn.Title = name;
            }

            return matrixLayoutColumn;
        }

        public MatrixResult GetMatrixResult()
        {
            MatrixResult result = new MatrixResult();
            result.MatrixDefinition = new MatrixDefinition() { MatrixDefinitionColumns = GetMatrixDefinitionColumns() };

            #region Create matrix

            int rowNumber = 1;

            foreach (var employee in agiAbsenceItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, employee));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, AgiAbsenceItem agiAbsenceItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_AgiAbsenceMatrixColumns)))
            {
                
                var type = (TermGroup_AgiAbsenceMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_AgiAbsenceMatrixColumns.EmployeeNr: return new MatrixField(rowNumber, column.Key, agiAbsenceItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_AgiAbsenceMatrixColumns.EmployeeName: return new MatrixField(rowNumber, column.Key, agiAbsenceItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_AgiAbsenceMatrixColumns.SocialSec: return new MatrixField(rowNumber, column.Key, agiAbsenceItem.SocialSec, column.MatrixDataType);
                    case TermGroup_AgiAbsenceMatrixColumns.PaymentDate: return new MatrixField(rowNumber, column.Key, agiAbsenceItem.PaymentDate, column.MatrixDataType);
                    case TermGroup_AgiAbsenceMatrixColumns.Date: return new MatrixField(rowNumber, column.Key, agiAbsenceItem.Date, column.MatrixDataType);
                    case TermGroup_AgiAbsenceMatrixColumns.ProductNr: return new MatrixField(rowNumber, column.Key, agiAbsenceItem.ProductNr, column.MatrixDataType);
                    case TermGroup_AgiAbsenceMatrixColumns.ProductName: return new MatrixField(rowNumber, column.Key, agiAbsenceItem.ProductName, column.MatrixDataType);
                    case TermGroup_AgiAbsenceMatrixColumns.Type: return new MatrixField(rowNumber, column.Key, agiAbsenceItem.Type, column.MatrixDataType);
                    case TermGroup_AgiAbsenceMatrixColumns.Quantity: return new MatrixField(rowNumber, column.Key, agiAbsenceItem.Quantity, column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
