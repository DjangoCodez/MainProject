using Newtonsoft.Json;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class ReportMenuDTO
    {
        public string GroupName { get; set; }
        public SoeReportType SysReportType { get; set; }
        public int SysReportTemplateTypeId { get; set; }
        public int? ReportNr { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool IsFavorite { get; set; }
        public int? ReportId { get; set; }
        public bool IsCompanyTemplate { get; set; }
        public bool IsStandard { get; set; }
        public bool IsSystemReport { get; set; }
        public int GroupOrder { get; set; }
        public int ReportTemplateId { get; set; }
        public bool PrintableFromMenu { get; set; }
        public bool NoPrintPermission { get; set; }
        public bool NoRolesSpecified { get; set; }
        public SoeModule Module { get; set; }
        public string Description { get; set; }
    }

    public class ReportJobStatusDTO
    {
        public string Name { get; set; }
        public TermGroup_ReportExportType ExportType { get; set; }
        public DateTime PrintoutRequested { get; set; }
        public DateTime? PrintoutDelivered { get; set; }
        public TermGroup_ReportPrintoutStatus PrintoutStatus { get; set; }
        public string PrintoutErrorMessage { get; set; }
        public int ReportPrintoutId { get; set; }
        public SoeReportTemplateType? SysReportTemplateTypeId { get; set; }
    }

    public class ReportItemDTO
    {
        public int ReportId { get; set; }
        public string Description { get; set; }
        public bool IncludeBudget { get; set; }
        public bool ShowRowsByAccount { get; set; }
        public List<SmallGenericType> SupportedExportTypes { get; set; }
        public SmallGenericType DefaultExportType { get; set; }
        public int ExportFileType { get; set; }
    }

    #region Data Output DTOs

    public class SelectablePayrollTypeDTO
    {
        public int Id { get; set; }
        public int SysTermId { get; set; }
        public int ParentSysTermId { get; set; }
        public string Name { get; set; }
    }
    [TSInclude]
    public class SelectableTimePeriodDTO
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    public class SelectablePayrollMonthYearDTO
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public List<int> TimePeriodIds { get; set; }
    }

    #endregion  

    #region Input DTOs

    public class ReportJobDefinitionDTO
    {
        public const string LANG = "lang";

        public int ReportId { get; set; }
        public int LangId { get; set; }
        public SoeReportTemplateType SysReportTemplateTypeId { get; set; }
        public TermGroup_ReportExportType ExportType { get; set; }
        public ICollection<ReportDataSelectionDTO> Selections { get; set; }
        public bool ForceValidation { get; set; }

        public ReportJobDefinitionDTO()
        {
            Selections = new List<ReportDataSelectionDTO>();
        }

        public ReportJobDefinitionDTO(int reportId, SoeReportTemplateType sysReportTemplateTypeId, TermGroup_ReportExportType exportType)
        {
            ReportId = reportId;
            SysReportTemplateTypeId = sysReportTemplateTypeId;
            ExportType = exportType;
            Selections = new List<ReportDataSelectionDTO>();
        }

        public int GetLang()
        {
            var selection = this.Selections?.GetSelection<IdSelectionDTO>(LANG);
            return selection?.Id.ToNullable() ?? (int)TermGroup_Languages.Swedish;
        }

        public void SetLang(int langId)
        {
            var existing = this.Selections?.FirstOrDefault(f => f.Key == LANG);

            if (existing != null)
                this.Selections.Remove(existing);

            this.Selections.Add(new IdSelectionDTO(langId, LANG));
        }
    }

    public class ReportSelectionDefinitionDTO
    {
        public int ReportId { get; set; }
        public int ReportUserSelectionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<ReportDataSelectionDTO> Selections { get; set; }

        public ReportSelectionDefinitionDTO()
        {
            Selections = new List<ReportDataSelectionDTO>();
        }
    }

    [TSInclude]
    public class ReportDataSelectionDTO
    {
        public ReportDataSelectionDTO() { }
        public ReportDataSelectionDTO(string typeName, string key)
        {
            TypeName = typeName;
            Key = key;
        }

        public string TypeName { get; set; }
        public string Key { get; set; }

        public virtual void Beautify() { }

        public static List<ReportDataSelectionDTO> FromJSON(string json)
        {
            if (String.IsNullOrEmpty(json))
                return new List<ReportDataSelectionDTO>();
            return JsonConvert.DeserializeObject<List<ReportDataSelectionDTO>>(json, serializerSettings);
        }

        public static string ToJSON(IEnumerable<ReportDataSelectionDTO> selection)
        {
            if (selection.IsNullOrEmpty())
                return string.Empty;
            return JsonConvert.SerializeObject(selection, serializerSettings).ToString();
        }

        private static JsonSerializerSettings serializerSettings
        {
            get
            {
                return new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto,
                    //TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                };
            }
        }
    }

    public class GeneralReportSelectionDTO : ReportDataSelectionDTO
    {
        public TermGroup_ReportExportType ExportType { get; set; }
    }

    public class AccountFilterSelectionDTO : ReportDataSelectionDTO
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Id { get; set; }
    }

    public class AccountDimSelectionDTO : ReportDataSelectionDTO
    { //three mandatory parameters taken from FE
        public int Level { get; set; }
        public int AccountDimId { get; set; }
        public int?[] AccountIds { get; set; }
    }


    public class AccountFilterSelectionsDTO : ReportDataSelectionDTO
    {
        public List<AccountFilterSelectionDTO> Filters { get; set; }

    }

    //public class AccountDimSelectionsDTO : ReportDataSelectionDTO
    //{
    //    public List<AccountDimSelectionDTO> Filters { get; set; }
    //}

    public class AccountIntervalSelectionDTO : ReportDataSelectionDTO
    {
        public int? Value { get; set; }
        public int? YearId { get; set; }

    }

    public class BoolSelectionDTO : ReportDataSelectionDTO
    {
        public bool Value { get; set; }
        public BoolSelectionDTO() { }
        public BoolSelectionDTO(bool value, string typeName, string key) : base(typeName, key)
        {
            Value = value;
        }
    }

    public class DateSelectionDTO : ReportDataSelectionDTO
    {
        public DateTime Date { get; set; }
        public int? Id { get; set; }

        public override void Beautify()
        {
            Date = Date.Date;
        }
    }

    public class DatesSelectionDTO : ReportDataSelectionDTO
    {
        public List<DateTime> Dates { get; set; }
    }

    public class DateRangeSelectionDTO : ReportDataSelectionDTO
    {
        public string RangeType { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int? Id { get; set; }
        public bool UseMinMaxIfEmpty { get; set; }

        public override void Beautify()
        {
            From = From.Date;
            To = To.Date;
        }
    }

    public class EmployeeSelectionDTO : ReportDataSelectionDTO
    {
        public EmployeeSelectionDTO(ICollection<int> employeeIds, string key)
        {
            EmployeeIds = employeeIds;
            Key = key;
        }

        public EmployeeSelectionDTO() { }

        public ICollection<int> EmployeeIds { get; set; }
        public ICollection<string> EmployeeNrs { get; set; }
        public ICollection<int> AccountIds { get; set; }
        public List<int> EmployeeGroupIds { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<int> VacationGroupIds { get; set; }
        public List<int> PayrollGroupIds { get; set; }
        public bool IsEmployeePost { get; set; }
        public bool IncludeInactive { get; set; }
        public bool OnlyInactive { get; set; }
        public bool IncludeEnded { get; set; }
        public bool IncludeHidden { get; set; }
        public bool IncludeVacant { get; set; }
        public bool IncludeSecondary { get; set; }
        public bool DoValidateEmployment { get; set; }
        public TermGroup_EmployeeSelectionAccountingType AccountingType { get; set; }

        public ICollection<int> EmployeePostIds
        {
            get
            {
                return IsEmployeePost ? EmployeeIds : new List<int>();
            }
        }
    }

    public class UserDataSelectionDTO : ReportDataSelectionDTO
    {
        public List<int> Ids { get; set; }
        public bool IncludeInactive { get; set; }

        public UserDataSelectionDTO()
        {
            Ids = Enumerable.Empty<int>().ToList();
            IncludeInactive = false;
        }

        public UserDataSelectionDTO(List<int> ids, string typeName, string key)
        {
            this.IncludeInactive = false;
            this.Ids = ids;
            base.Key = key;
            base.TypeName = typeName;
        }
    }

    public class IdSelectionDTO : ReportDataSelectionDTO
    {
        public int Id { get; set; }

        public IdSelectionDTO(int id, string key)
        {
            this.Id = id;
            base.Key = key;
        }
    }

    public class IdListSelectionDTO : ReportDataSelectionDTO
    {
        public List<int> Ids { get; set; }

        public IdListSelectionDTO()
        {
            Ids = Enumerable.Empty<int>().ToList();
        }

        public IdListSelectionDTO(List<int> ids, string typeName, string key)
        {
            this.Ids = ids;
            base.Key = key;
            base.TypeName = typeName;
        }

        public class DateListSelectionDTO : ReportDataSelectionDTO
        {
            public List<string> Dates { get; set; }

            public DateListSelectionDTO()
            {
                Dates = Enumerable.Empty<string>().ToList();
            }
        }
    }

    public class AttachmentsListSelectionDTO : ReportDataSelectionDTO
    {
        public List<KeyValuePair<string, byte[]>> Attachments { get; set; }

        public AttachmentsListSelectionDTO()
        {
            Attachments = new List<KeyValuePair<string, byte[]>>();
        }

        public AttachmentsListSelectionDTO(List<KeyValuePair<string, byte[]>> attachments, string typeName, string key)
        {
            Attachments = attachments;
            base.Key = key;
            base.TypeName = typeName;
        }
    }

    public class PayrollProductRowSelectionDTO : ReportDataSelectionDTO
    {
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public List<int> PayrollProductIds { get; set; }

        public PayrollProductRowSelectionDTO()
        {
            PayrollProductIds = Enumerable.Empty<int>().ToList();
        }
    }

    public class PayrollPriceTypeSelectionDTO : ReportDataSelectionDTO
    {
        public List<int> Ids { get; set; }
        public List<int> TypeIds { get; set; }

        public PayrollPriceTypeSelectionDTO()
        {
            Ids = Enumerable.Empty<int>().ToList();
            TypeIds = Enumerable.Empty<int>().ToList();
        }
    }

    public class TextSelectionDTO : ReportDataSelectionDTO
    {
        public string Text { get; set; }

        public TextSelectionDTO(string text, string key)
        {
            this.Text = text;
            base.Key = key;
        }
    }

    public class YearAndPeriodSelectionDTO : ReportDataSelectionDTO
    {
        public string RangeType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public int Id { get; set; }
    }

    public class MatrixColumnSelectionDTO : ReportDataSelectionDTO
    {
        public string Field { get; set; }
        public int Sort { get; set; }
        public MatrixDataType MatrixDataType { get; set; }
        public MatrixDefinitionColumnOptions Options { get; set; }
    }

    public class MatrixColumnsSelectionDTO : ReportDataSelectionDTO
    {
        public List<MatrixColumnSelectionDTO> Columns { get; set; }
        public AnalysisMode AnalysisMode { get; set; }
        public int InsightId { get; set; }
        public string InsightName { get; set; }
        public TermGroup_InsightChartTypes ChartType { get; set; }
        public int ValueType { get; set; }
    }

    #endregion

    #region ApiMatrix

    public static class ApiMatrixSelectionExtensions
    {
        public static ReportSelectionDefinitionDTO ToReportSelectionDefinitionDTO(this ApiMatrixDataSelection apiMatrixDataSelection)
        {
            var definition = new ReportSelectionDefinitionDTO()
            {
                ReportId = apiMatrixDataSelection.ReportId ?? 0,
                ReportUserSelectionId = apiMatrixDataSelection.ReportUserSelectionId ?? 0,
            };

            apiMatrixDataSelection.ApiMatrixDataSelectionIdLists.ForEach(f => definition.Selections.Add(f.ToIdListSelectionDTO()));
            apiMatrixDataSelection.ApiMatrixDataSelectionDateRanges.ForEach(f => definition.Selections.Add(f.ToDateRangeSelectionDTO()));
            apiMatrixDataSelection.ApiMatrixDataSelectionBools.ForEach(f => definition.Selections.Add(f.ToBoolSelectionDTO()));
            apiMatrixDataSelection.ApiMatrixDataSelectionTexts.ForEach(f => definition.Selections.Add(f.ToTextSelectionDTO()));
            apiMatrixDataSelection.ApiMatrixDataSelectionDates.ForEach(f => definition.Selections.Add(f.ToDateSelectionDTO()));
            apiMatrixDataSelection.ApiMatrixDataSelectionIds.ForEach(f => definition.Selections.Add(f.ToIdSelectionDTO()));

            if (apiMatrixDataSelection.ApiMatrixColumnsSelection != null)
                definition.Selections.Add(apiMatrixDataSelection.ApiMatrixColumnsSelection.ToMatrixColumnsSelectionDTO());

            if (apiMatrixDataSelection.ApiMatrixEmployeeSelections != null)
                definition.Selections.Add(apiMatrixDataSelection.ApiMatrixEmployeeSelections.ToEmployeeSelectionDTO());

            if (apiMatrixDataSelection.ApiMatrixPayrollProductRowSelections != null)
                apiMatrixDataSelection.ApiMatrixPayrollProductRowSelections.ForEach(f => definition.Selections.Add(f.ToPayrollProductRowSelectionDTO()));

            return definition;

        }
        public static EmployeeSelectionDTO ToEmployeeSelectionDTO(this ApiMatrixEmployeeSelection apiMatrixEmployeeSelection)
        {
            var employeeSelectionDTO = new EmployeeSelectionDTO()
            {
                Key = "employees",
                EmployeeIds = apiMatrixEmployeeSelection.EmployeeIds,
                EmployeeNrs = apiMatrixEmployeeSelection.EmployeeNumbers,
                IncludeEnded = apiMatrixEmployeeSelection.IncludeEnded,
                IncludeHidden = apiMatrixEmployeeSelection.IncludeHidden,
                IncludeVacant = apiMatrixEmployeeSelection.IncludeVacant
            };

            return employeeSelectionDTO;
        }

        public static PayrollProductRowSelectionDTO ToPayrollProductRowSelectionDTO(this ApiMatrixPayrollProductRowSelection apiMatrixEmployeeSelection)
        {
            var selection = new PayrollProductRowSelectionDTO()
            {
                SysPayrollTypeLevel1 = apiMatrixEmployeeSelection.SysPayrollTypeLevel1 == TermGroup_SysPayrollType.None ? null : (int?)apiMatrixEmployeeSelection.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = apiMatrixEmployeeSelection.SysPayrollTypeLevel2 == TermGroup_SysPayrollType.None ? null : (int?)apiMatrixEmployeeSelection.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = apiMatrixEmployeeSelection.SysPayrollTypeLevel3 == TermGroup_SysPayrollType.None ? null : (int?)apiMatrixEmployeeSelection.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = apiMatrixEmployeeSelection.SysPayrollTypeLevel4 == TermGroup_SysPayrollType.None ? null : (int?)apiMatrixEmployeeSelection.SysPayrollTypeLevel4,
            };

            return selection;
        }
        public static BoolSelectionDTO ToBoolSelectionDTO(this ApiMatrixDataSelectionBool apiMatrixDataSelectionBool)
        {
            return new BoolSelectionDTO(apiMatrixDataSelectionBool.BooleanValue,
                apiMatrixDataSelectionBool.TypeName,
                apiMatrixDataSelectionBool.Key);
        }

        public static TextSelectionDTO ToTextSelectionDTO(this ApiMatrixDataSelectionText apiMatrixDataSelectionText)
        {
            return new TextSelectionDTO(apiMatrixDataSelectionText.StringValue, apiMatrixDataSelectionText.Key)
            {
                TypeName = apiMatrixDataSelectionText.TypeName,
            };
        }

        public static IdListSelectionDTO ToIdListSelectionDTO(this ApiMatrixDataSelectionIdList apiMatrixDataSelectionIdList)
        {
            return new IdListSelectionDTO()
            {
                Key = apiMatrixDataSelectionIdList.Key,
                TypeName = apiMatrixDataSelectionIdList.TypeName,
                Ids = apiMatrixDataSelectionIdList.Ids,
            };
        }

        public static IdSelectionDTO ToIdSelectionDTO(this ApiMatrixDataSelectionId apiMatrixDataSelectionId)
        {
            return new IdSelectionDTO(apiMatrixDataSelectionId.Id, apiMatrixDataSelectionId.Key)
            {
                Key = apiMatrixDataSelectionId.Key,
                TypeName = apiMatrixDataSelectionId.TypeName,
                Id = apiMatrixDataSelectionId.Id,
            };
        }

        public static DateRangeSelectionDTO ToDateRangeSelectionDTO(this ApiMatrixDataSelectionDateRange apiMatrixDataSelectionDateRange)
        {
            return new DateRangeSelectionDTO()
            {
                Key = apiMatrixDataSelectionDateRange.Key,
                TypeName = apiMatrixDataSelectionDateRange.TypeName,
                From = apiMatrixDataSelectionDateRange.SelectFrom,
                To = apiMatrixDataSelectionDateRange.SelectTo,
            };
        }

        public static DateSelectionDTO ToDateSelectionDTO(this ApiMatrixDataSelectionDate apiMatrixDataSelectionDate)
        {
            return new DateSelectionDTO()
            {
                Key = apiMatrixDataSelectionDate.Key,
                TypeName = apiMatrixDataSelectionDate.TypeName,
                Date = apiMatrixDataSelectionDate.Date,
            };
        }

        public static MatrixColumnsSelectionDTO ToMatrixColumnsSelectionDTO(this ApiMatrixColumnsSelection apiMatrixColumnsSelection)
        {
            var dto = new MatrixColumnsSelectionDTO()
            {
                Key = apiMatrixColumnsSelection.Key,
                TypeName = apiMatrixColumnsSelection.TypeName,
                Columns = new List<MatrixColumnSelectionDTO>()
            };
            if (!apiMatrixColumnsSelection.ApiMatrixColumnSelections.IsNullOrEmpty())
                apiMatrixColumnsSelection.ApiMatrixColumnSelections?.ForEach(column => dto.Columns.Add(
                    new MatrixColumnSelectionDTO() 
                    { 
                        Field = column.Field,
                        Options = column.ItemId != 0 ? new MatrixDefinitionColumnOptions() { Key = column.ItemId.ToString() } : null
                    }
                )
            );

            return dto;
        }
    }
    #endregion
}
