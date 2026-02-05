using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class ShiftTypeSkillMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "ShiftTypeSkillMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<ShiftTypeSkillReportDataField> filter { get; set; }
        ShiftTypeSkillReportDataOutput _reportDataOutput { get; set; }

        List<GenericType> blocktypes { get; set; }
        #endregion

        public ShiftTypeSkillMatrix(InputMatrix inputMatrix, ShiftTypeSkillReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
            blocktypes = reportDataOutput?.BlockTypes;
        }


        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftTypeSkillMatrixColumns.ShiftType));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftTypeSkillMatrixColumns.ShiftTypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftTypeSkillMatrixColumns.ShiftTypeCatagory));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftTypeSkillMatrixColumns.ShiftTypeDescription));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftTypeSkillMatrixColumns.ShiftTypeNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftTypeSkillMatrixColumns.ShiftTypeScheduleTypeName));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftTypeSkillMatrixColumns.Skill));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_ShiftTypeSkillMatrixColumns.SkillLevel));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ShiftTypeSkillMatrixColumns.Accountingsettings));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_ShiftTypeSkillMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var skillItem in _reportDataOutput.ShiftTypeSkillItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, skillItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, ShiftTypeSkillItem shiftTypeSkillItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_ShiftTypeSkillMatrixColumns)))
            {
                switch ((TermGroup_ShiftTypeSkillMatrixColumns)id)
                {
                    case TermGroup_ShiftTypeSkillMatrixColumns.ShiftTypeName:
                        return new MatrixField(rowNumber, column.Key, shiftTypeSkillItem.ShiftTypeName, column.MatrixDataType);
                    case TermGroup_ShiftTypeSkillMatrixColumns.ShiftType:
                        return new MatrixField(rowNumber, column.Key, blocktypes?.FirstOrDefault(f => f.Id == shiftTypeSkillItem.ShiftType)?.Name, column.MatrixDataType);
                    case TermGroup_ShiftTypeSkillMatrixColumns.ShiftTypeScheduleTypeName:
                        return new MatrixField(rowNumber, column.Key, shiftTypeSkillItem.ShiftTypeScheduleTypeName, column.MatrixDataType);
                    case TermGroup_ShiftTypeSkillMatrixColumns.ScheduleTypeName:
                        return new MatrixField(rowNumber, column.Key, shiftTypeSkillItem.ScheduleTypeName, column.MatrixDataType);
                    case TermGroup_ShiftTypeSkillMatrixColumns.ShiftTypeCatagory:
                        return new MatrixField(rowNumber, column.Key, shiftTypeSkillItem.ShiftTypeCatagory, column.MatrixDataType);
                    case TermGroup_ShiftTypeSkillMatrixColumns.ShiftTypeDescription:
                        return new MatrixField(rowNumber, column.Key, shiftTypeSkillItem.ShiftTypeDescription, column.MatrixDataType);
                    case TermGroup_ShiftTypeSkillMatrixColumns.Skill:
                        return new MatrixField(rowNumber, column.Key, shiftTypeSkillItem.Skill, column.MatrixDataType);
                    case TermGroup_ShiftTypeSkillMatrixColumns.SkillLevel:
                        return new MatrixField(rowNumber, column.Key, shiftTypeSkillItem.SkillLevel, column.MatrixDataType);
                    case TermGroup_ShiftTypeSkillMatrixColumns.ShiftTypeNumber:
                        return new MatrixField(rowNumber, column.Key, shiftTypeSkillItem.ShiftTypeNumber, column.MatrixDataType);
                    case TermGroup_ShiftTypeSkillMatrixColumns.Accountingsettings:
                        return new MatrixField(rowNumber, column.Key, shiftTypeSkillItem.Accountingsettings, column.MatrixDataType);
                        



                    case TermGroup_ShiftTypeSkillMatrixColumns.Unknown:
                        break;

                    default:
                        return new MatrixField(rowNumber, column.Key, "", column.MatrixDataType);
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}