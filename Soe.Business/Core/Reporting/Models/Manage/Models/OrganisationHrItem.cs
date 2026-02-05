using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models
{
    public class OrganisationHrItem
    {
        public OrganisationHrItem()
        {
            ExtraFieldAnalysisFields = new List<ExtraFieldAnalysisField>();
            AccountAnalysisFields = new List<AccountAnalysisField>();
        }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmployeeNr { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool CategoryIsDefault { get; set; }
        public string CategoryName { get; set; }
        public string CategoryGroup { get; set; }
        public string SubCategory { get; set; }
        public bool AccountIsPrimary { get; set; }

        public List<ExtraFieldAnalysisField> ExtraFieldAnalysisFields { get; set; }
        public List<AccountAnalysisField> AccountAnalysisFields { get; set; }
    }

    public static class OrganisationHrItemExtensions
    {
        public static string GroupOn(this OrganisationHrItem organisationHrItem, List<TermGroup_OrganisationHrMatrixColumns> columns, bool mergeOnAccount = false)
        {
            var value = new System.Text.StringBuilder("");

            foreach (var column in columns)
            {
                switch (column)
                {

                    case TermGroup_OrganisationHrMatrixColumns.EmployeeNr:
                        value.Append($"#{organisationHrItem.EmployeeNr}");
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.FirstName:
                        value.Append($"#{organisationHrItem.FirstName}");
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.LastName:
                        value.Append($"#{organisationHrItem.LastName}");
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.Name:
                        value.Append($"#{organisationHrItem.Name}");
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.DateFrom:
                        value.Append($"#{organisationHrItem.DateFrom}");
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.DateTo:
                        value.Append($"#{organisationHrItem.DateTo}");
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.AccountIsPrimary:
                        value.Append($"#{organisationHrItem.AccountIsPrimary}");
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.CategoryIsDefault:
                        value.Append($"#{organisationHrItem.CategoryIsDefault}");
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.CategoryName:
                        value.Append($"#{organisationHrItem.CategoryName}");
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.CategoryGroup:
                        value.Append($"#{organisationHrItem.CategoryGroup}");
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.SubCategory:
                        value.Append($"#{organisationHrItem.SubCategory}");
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.AccountInternalNrs:
                    case TermGroup_OrganisationHrMatrixColumns.AccountInternalNames:
                        foreach (var account in organisationHrItem.AccountAnalysisFields)
                        {
                            value.Append($"#{organisationHrItem.AccountAnalysisFields[organisationHrItem.AccountAnalysisFields.IndexOf(account)].AccountDimId}");
                        }
                        break;
                    case TermGroup_OrganisationHrMatrixColumns.ExtraFieldAccount:

                        foreach (var extraFiled in organisationHrItem.ExtraFieldAnalysisFields)
                        {
                            var extraFieldAnalysisFields = organisationHrItem.ExtraFieldAnalysisFields;
                            var efr = extraFieldAnalysisFields[extraFieldAnalysisFields.IndexOf(extraFiled)].ExtraFieldRecord != null ?
                                extraFieldAnalysisFields[extraFieldAnalysisFields.IndexOf(extraFiled)].ExtraFieldRecord.ExtraFieldRecordId.ToString() : "";
                            value.Append($"#{efr}");
                        }
                        break;
                    default:
                        break;
                }
            }

            return value.ToString();
        }
    }
}
