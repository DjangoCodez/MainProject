using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class ShiftTypeSkillReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly ShiftTypeSkillReportDataInput _reportDataInput;
        private readonly ShiftTypeSkillReportDataOutput _reportDataOutput;

        private bool loadBlockTypes => _reportDataInput.Columns.Any(a => a.Column == TermGroup_ShiftTypeSkillMatrixColumns.ShiftType);

        public ShiftTypeSkillReportData(ParameterObject parameterObject, ShiftTypeSkillReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new ShiftTypeSkillReportDataOutput(reportDataInput);
        }

        public ShiftTypeSkillReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private ActionResult LoadData()
        {
            #region Prereq
           
            if (loadBlockTypes)
                _reportDataOutput.BlockTypes = GetTermGroupContent(TermGroup.TimeScheduleTemplateBlockType);

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionShiftTypeIds, "shiftTypes"))
                return new ActionResult(true);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                var accountingStringAccountNames = "";
                var dims = AccountManager.GetAccountDimsByCompany(base.ActorCompanyId).Where(x => !x.IsStandard).OrderBy(x => x.AccountDimNr).ToList();
                List<ShiftType> shiftTypesList = base.GetShiftTypesFromCache(entities, CacheConfig.Company(base.ActorCompanyId, 100));
                var categoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.ShiftType, base.ActorCompanyId);
               
                foreach (var shiftTypeList in shiftTypesList)
                {
                    if(!shiftTypeList.AccountReference.IsLoaded)
                        shiftTypeList.AccountReference.Load();
                    if (!selectionShiftTypeIds.Contains(shiftTypeList.ShiftTypeId) && selectionShiftTypeIds.Count!=0)
                        continue;
                    if (shiftTypeList?.ShiftTypeSkill !=null && !shiftTypeList.ShiftTypeSkill.IsLoaded) shiftTypeList.ShiftTypeSkill.Load();
                    if (categoryRecords != null)
                    {
                        List<string> categoryNames = categoryRecords.GetCategoryRecords(shiftTypeList.ShiftTypeId).OrderByDescending(c => c.Default).ThenBy(c => c.Category.Name).Select(c => c.Category.Name).ToList();
                        shiftTypeList.CategoryNames = StringUtility.GetCommaSeparatedString(categoryNames, addWhiteSpace: true);
                    }
                    List<ShiftTypeSkill>shiftTypeSkillList = TimeScheduleManager.GetShiftTypeSkills(entities, shiftTypeList.ShiftTypeId);
                    accountingStringAccountNames = "";
                    if (dims != null && shiftTypeList.Account!=null)
                    {
                       
                        foreach (AccountDim dim in dims)
                        {
                            if(shiftTypeList.Account?.AccountDimId == dim.AccountDimId)

                                accountingStringAccountNames = string.Join(", ", shiftTypeList.Account.Name);
                        }

                        
                    }
                    if (shiftTypeSkillList.Count != 0)
                    {
                        foreach (var skillLists in shiftTypeSkillList)
                        {

                            ShiftTypeSkillItem shiftTyoeSkillItem = new ShiftTypeSkillItem()
                            {
                                ShiftTypeName = shiftTypeList.Name,
                                ShiftType = shiftTypeList.TimeScheduleTemplateBlockType,
                                ShiftTypeScheduleTypeName = shiftTypeList.TimeScheduleType?.Name ?? string.Empty,
                                ShiftTypeNumber = shiftTypeList.NeedsCode,

                                ShiftTypeDescription = shiftTypeList.Description,
                                ShiftTypeCatagory = shiftTypeList.CategoryNames,

                                ExtternalCode = shiftTypeList.ExternalCode,
                                Skill = skillLists.Skill.Name,
                                SkillLevel = skillLists.SkillLevel,
                                Accountingsettings = accountingStringAccountNames
                            };
                            _reportDataOutput.ShiftTypeSkillItems.Add(shiftTyoeSkillItem);
                        }
                    }
                    else
                    {
                        ShiftTypeSkillItem shiftTyoeSkillItem = new ShiftTypeSkillItem()
                        {
                            ShiftTypeName = shiftTypeList.Name,
                            ShiftType = shiftTypeList.TimeScheduleTemplateBlockType,
                            ShiftTypeScheduleTypeName = shiftTypeList.TimeScheduleType?.Name ?? string.Empty,
                            ShiftTypeNumber = shiftTypeList.NeedsCode,

                            ShiftTypeDescription = shiftTypeList.Description,
                            ShiftTypeCatagory = shiftTypeList.CategoryNames,
                            //ScheduleTypeName

                            //ShiftTypeScheduleType
                            ExtternalCode = shiftTypeList.ExternalCode,
                            Skill = "",
                            SkillLevel = null,
                            Accountingsettings = accountingStringAccountNames

                        };
                        _reportDataOutput.ShiftTypeSkillItems.Add(shiftTyoeSkillItem);
                    }

                   

                }
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }
    }

    public class ShiftTypeSkillReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_ShiftTypeSkillMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public ShiftTypeSkillReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_ShiftTypeSkillMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_ShiftTypeSkillMatrixColumns.Unknown;
        }
    }

    public class ShiftTypeSkillReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<ShiftTypeSkillReportDataField> Columns { get; set; }

        public ShiftTypeSkillReportDataInput(CreateReportResult reportResult, List<ShiftTypeSkillReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class ShiftTypeSkillReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public ShiftTypeSkillReportDataInput Input { get; set; }
        public List<ShiftTypeSkillItem> ShiftTypeSkillItems { get; set; }
        public List<GenericType> BlockTypes { get; set; }

        public ShiftTypeSkillReportDataOutput(ShiftTypeSkillReportDataInput input)
        {
            this.Input = input;
            this.ShiftTypeSkillItems = new List<ShiftTypeSkillItem>();
            this.BlockTypes = new List<GenericType>();
        }
    }
}

