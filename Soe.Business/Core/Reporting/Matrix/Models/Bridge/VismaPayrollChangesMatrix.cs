using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Bridge;
using SoftOne.Soe.Business.Core.Reporting.Models.Status.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class VismaPayrollChangesMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "VismaPayrollChangesMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<VismaPayrollChangesReportDataField> filter { get; set; }
        private readonly bool validBridge;
        List<VismaPayrollChangesItem> vismaPayrollChangesItems { get; set; }
        #endregion

        public VismaPayrollChangesMatrix(InputMatrix inputMatrix, VismaPayrollChangesReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            vismaPayrollChangesItems = reportDataOutput?.ResultItems ?? new List<VismaPayrollChangesItem>();
            CompanyManager companyManager = new CompanyManager(null);
            var company = companyManager.GetCompany(actorCompanyId);
            if (company != null)
            {
                LicenseManager licenseManager = new LicenseManager(null);
                var license = licenseManager.GetLicense(company.LicenseId);

                if (license != null && license.LicenseNr.StartsWith("40"))
                {
                    ScheduledJobManager scheduledJobManager = new ScheduledJobManager(null);
                    validBridge = scheduledJobManager.GetScheduledJobHeads(actorCompanyId).Any();
                }
            }
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!validBridge || !base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VismaPayrollChangesMatrixColumns.EmployerRegistrationNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VismaPayrollChangesMatrixColumns.PersonName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_VismaPayrollChangesMatrixColumns.Time));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VismaPayrollChangesMatrixColumns.Entity));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VismaPayrollChangesMatrixColumns.Info));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VismaPayrollChangesMatrixColumns.Field));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VismaPayrollChangesMatrixColumns.OldValue));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VismaPayrollChangesMatrixColumns.NewValue));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VismaPayrollChangesMatrixColumns.PersonId));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VismaPayrollChangesMatrixColumns.VismaPayrollEmploymentId));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VismaPayrollChangesMatrixColumns.VismaPayrollBatchId));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VismaPayrollChangesMatrixColumns.VismaPayrollChangeId));


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
                    // Hidden
                    foreach (MatrixLayoutColumn item in possibleColumns.Where(c => c.IsHidden()))
                    {
                        matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item.MatrixDataType, item.Field, item.Title, item.Options));
                    }

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_VismaPayrollChangesMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in vismaPayrollChangesItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, VismaPayrollChangesItem vismaPayrollChangesItem)
        {
            if (base.GetEnumId<TermGroup_VismaPayrollChangesMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_VismaPayrollChangesMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_VismaPayrollChangesMatrixColumns.Time:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.Time, column.MatrixDataType);
                    case TermGroup_VismaPayrollChangesMatrixColumns.Entity:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.Entity, column.MatrixDataType);
                    case TermGroup_VismaPayrollChangesMatrixColumns.Info:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.Info, column.MatrixDataType);
                    case TermGroup_VismaPayrollChangesMatrixColumns.Field:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.Field, column.MatrixDataType);
                    case TermGroup_VismaPayrollChangesMatrixColumns.OldValue:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.OldValue, column.MatrixDataType);
                    case TermGroup_VismaPayrollChangesMatrixColumns.NewValue:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.NewValue, column.MatrixDataType);
                    case TermGroup_VismaPayrollChangesMatrixColumns.PersonId:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.PersonId, column.MatrixDataType);
                    case TermGroup_VismaPayrollChangesMatrixColumns.VismaPayrollEmploymentId:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.VismaPayrollEmploymentId, column.MatrixDataType);
                    case TermGroup_VismaPayrollChangesMatrixColumns.VismaPayrollBatchId:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.VismaPayrollBatchId, column.MatrixDataType);
                    case TermGroup_VismaPayrollChangesMatrixColumns.VismaPayrollChangeId:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.VismaPayrollChangeId, column.MatrixDataType);
                    case TermGroup_VismaPayrollChangesMatrixColumns.EmployerRegistrationNumber:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.EmployerRegistrationNumber, column.MatrixDataType);
                    case TermGroup_VismaPayrollChangesMatrixColumns.PersonName:
                        return new MatrixField(rowNumber, column.Key, vismaPayrollChangesItem.PersonName, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
