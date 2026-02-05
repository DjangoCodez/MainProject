using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Common.DTO
{
    #region Export

    [TSInclude]
    public class ExportDTO
    {
        public int ExportId { get; set; }
        public int ActorCompanyId { get; set; }
        public int ExportDefinitionId { get; set; }
        public int Module { get; set; }
        public bool Standard { get; set; }
        public string Name { get; set; }
        public string Filename { get; set; }
        public string Emailaddress { get; set; }
        public string Subject { get; set; }
        public bool AttachFile { get; set; }
        public bool SendDirect { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int State { get; set; }
        public Guid Guid { get; set; }
        public string SpecialFunctionality { get; set; }
    }

    [TSInclude]
    public class ExportGridDTO
    {
        public int ExportId { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region ExportDefinition

    [TSInclude]
    public class ExportDefinitionDTO
    {
        public ExportDefinitionDTO()
        {
            ExportDefinitionLevels = new List<ExportDefinitionLevelDTO>();
        }

        public int ExportDefinitionId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? SysExportHeadId { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public string Separator { get; set; }
        public string XmlTagHead { get; set; }
        public string SpecialFunctionality { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }


        // Extensions
        public List<ExportDefinitionLevelDTO> ExportDefinitionLevels { get; set; }
        public string ExportHeadName { get; set; }
        public string IsStandardExport { get; set; }
        public string ExportTypeName { get; set; }
        public int? ReportSelectionId { get; set; }
        public ReportUserSelectionDTO ReportUserSelection { get; set; }
    }

    [TSInclude]
    public class ExportDefinitionGridDTO
    {
        public ExportDefinitionGridDTO()
        {
            Exports = new List<ExportDTO>();
        }

        public int ExportDefinitionId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        [TSIgnore]
        public List<ExportDTO> Exports { get; set; }
    }

    #endregion

    #region ExportDefinitionLevel

    [TSInclude]
    public class ExportDefinitionLevelDTO
    {
        public ExportDefinitionLevelDTO()
        {
            ExportDefinitionLevelColumns = new List<ExportDefinitionLevelColumnDTO>();
        }
        public int ExportDefinitionLevelId { get; set; }
        public int ExportDefinitionId { get; set; }
        public int Level { get; set; }
        public string Xml { get; set; }
        public bool UseColumnHeaders { get; set; } //TODO in DB
        public List<ExportDefinitionLevelColumnDTO> ExportDefinitionLevelColumns { get; set; }
    }

    [TSInclude]
    public class ExportDefinitionLevelColumnDTO
    {
        public int ExportDefinitionLevelColumnId { get; set; }
        public int ExportDefinitionLevelId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
        public int Position { get; set; }
        public string DefaultValue { get; set; }
        public MatrixDefinitionColumn MatrixDefinitionColumn { get; set; }
        public bool IsDelimiter
        {
            get
            {
                return Position != 0 && ColumnLength == 0;
            }
        }
        public bool IsPosition
        {
            get
            {
                return Position != 0 && ColumnLength != 0;
            }
        }
        public string FillChar { get; set; }
        public bool? FillBeginning { get; set; }
        public string FormatDate { get; set; }
        public int ColumnLength { get; set; }
        public string XmlTag { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public string ColumnHeader
        {
            get
            {
                return Name;
            }
        }
        public string ConvertValue { get; set; } //TODO in db

        public string GetValueWithKey(CompanyDTO company)
        {
            if (Key.StartsWith("Company."))
            {
                try
                {
                    return company.GetPropertyValue(Key.Replace("Company.", "")) as string;
                }
                catch
                {
                    return "Exception Invalid Key " + Key;
                }
            }

            return GetDateValueFromKey;
        }

        public string GetDateValueFromKey
        {
            get
            {
                if (Key == "DateTime.Today")
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(FormatDate))
                        {
                            return DateTime.Today.ToString(FormatDate);
                        }
                    }
                    catch
                    {
                        // Intentionally ignored, safe to continue
                        // NOSONAR
                    }
                    return DateTime.Today.ToString();
                }
                else if (Key == "DateTime.Now")
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(FormatDate))
                        {
                            return DateTime.Now.ToString(FormatDate);
                        }
                    }
                    catch
                    {
                        // Intentionally ignored, safe to continue
                        // NOSONAR
                    }
                    return DateTime.Now.ToString();
                }

                return string.Empty;
            }

        }
    }
    #endregion
}
