using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class UserMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "UserMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<UserReportDataReportDataField> filter { get; set; }
        UserReportDataOutput _reportDataOutput { get; set; }
        readonly bool useAccountHierarchy;
        #endregion

        public UserMatrix(InputMatrix inputMatrix, UserReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;

            SettingManager sm = new SettingManager(null);
            this.useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, actorCompanyId, 0);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Manage_Users_Edit))
                return possibleColumns;

            if (base.HasReadPermission(Feature.Common_Categories_Employee))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_UserMatrixColumns.EmployeeNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_UserMatrixColumns.Name));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_UserMatrixColumns.Email));
            }
            if (base.HasReadPermission(Feature.Manage_Users_Edit))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_UserMatrixColumns.LoginName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_UserMatrixColumns.DateCreated));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_UserMatrixColumns.CreatedBy));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_UserMatrixColumns.DateModified));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_UserMatrixColumns.ModifiedBy));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_UserMatrixColumns.IsActive));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_UserMatrixColumns.IsMobileUser));
            }
            if (base.HasReadPermission(Feature.Manage_Roles_Edit))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_UserMatrixColumns.Roles));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_UserMatrixColumns.AttestRoles));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_UserMatrixColumns.AttestRoleAccount));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_UserMatrixColumns.RoleDateFrom));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_UserMatrixColumns.RoleDateTo));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_UserMatrixColumns.AttestRoleDateFrom));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_UserMatrixColumns.AttestRoleDateTo));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_UserMatrixColumns.ShowAllCategories));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_UserMatrixColumns.ShowUncategorized));
            }
            if (base.HasReadPermission(Feature.Manage_Users_Edit_Sessions))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_UserMatrixColumns.LastLogin));
            if (this.useAccountHierarchy)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_UserMatrixColumns.AttestRoleAccountName));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_UserMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var userItem in _reportDataOutput.UserItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, userItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, UserItem userItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_UserMatrixColumns)))
            {
                var type = (TermGroup_UserMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_UserMatrixColumns.LoginName:
                        return new MatrixField(rowNumber, column.Key, userItem.LoginName, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.Name:
                        return new MatrixField(rowNumber, column.Key, userItem.Name, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, userItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.Email:
                        return new MatrixField(rowNumber, column.Key, userItem.Email, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.Roles:
                        return new MatrixField(rowNumber, column.Key, userItem.Roles, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.AttestRoles:
                        return new MatrixField(rowNumber, column.Key, userItem.AttestRoles, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.AttestRoleAccount:
                        return new MatrixField(rowNumber, column.Key, userItem.AttestRoleAccount, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.DateCreated:
                        return new MatrixField(rowNumber, column.Key, userItem.DateCreated, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.DateModified:
                        return new MatrixField(rowNumber, column.Key, userItem.DateModified, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.CreatedBy:
                        return new MatrixField(rowNumber, column.Key, userItem.CreatedBy, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.ModifiedBy:
                        return new MatrixField(rowNumber, column.Key, userItem.ModifiedBy, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.IsActive:
                        return new MatrixField(rowNumber, column.Key, userItem.IsActive, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.IsMobileUser:
                        return new MatrixField(rowNumber, column.Key, userItem.IsMobileUser, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.LastLogin:
                        return new MatrixField(rowNumber, column.Key, userItem.LastLogin, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.RoleDateFrom:
                        return new MatrixField(rowNumber, column.Key, userItem.RoleDateFrom, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.RoleDateTo:
                        return new MatrixField(rowNumber, column.Key, userItem.RoleDateTo, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.AttestRoleDateFrom:
                        return new MatrixField(rowNumber, column.Key, userItem.AttestRoleDateFrom, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.AttestRoleDateTo:
                        return new MatrixField(rowNumber, column.Key, userItem.AttestRoleDateTo, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.ShowAllCategories:
                        return new MatrixField(rowNumber, column.Key, userItem.ShowAllCategories, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.ShowUncategorized:
                        return new MatrixField(rowNumber, column.Key, userItem.ShowUncategorized, column.MatrixDataType);
                    case TermGroup_UserMatrixColumns.AttestRoleAccountName:
                        return new MatrixField(rowNumber, column.Key, userItem.AccountName, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
