using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class EmployeeIOItem
    {
        #region Variables

        public List<EmployeeIODTO> EmployeeIOs { get; } = new List<EmployeeIODTO>();
        public TermGroup_IOSource Source { get; }
        public TermGroup_IOType IOType { get; }
        public TermGroup_IOImportHeadType HeadType { get; }
        public int ActorCompanyId { get; set; }

        #endregion

        #region XML Nodes

        #region Core

        public const string XML_PARENT_TAG = "Employee";
        public const string XML_EmployeeId_TAG =  "EmployeeId";
        public const string XML_State_TAG = "State";

        #endregion

        #region Personal information

        public const string XML_FirstName_TAG = "FirstName";
        public const string XML_LastName_TAG = "LastName";
        public const string XML_SocialSec_TAG = "SocialSec";
        public const string XML_Sex_TAG = "Sex";
        public const string XML_Email_TAG = "Email";
        public const string XML_DistributionAddress_TAG = "DistributionAddress";
        public const string XML_DistributionCoAddress_TAG = "DistributionCoAddress";
        public const string XML_DistributionPostalCode_TAG = "DistributionPostalCode";
        public const string XML_DistributionPostalAddress_TAG = "DistributionPostalAddress";
        public const string XML_DistributionCountry_TAG = "DistributionCountry";
        public const string XML_PhoneHome_TAG = "PhoneHome";
        public const string XML_PhoneMobile_TAG = "PhoneMobile";
        public const string XML_PhoneJob_TAG = "PhoneJob";
        public const string XML_ClosestRelativeNr_TAG = "ClosestRelativeNr";
        public const string XML_ClosestRelativeName_TAG = "ClosestRelativeName";
        public const string XML_ClosestRelativeRelation_TAG = "ClosestRelativeRelation";
        //ClosestRelativeNr2
        //ClosestRelativeName2
        //ClosestRelativeRelation2
        //DisbursementMethod
        //DisbursementClearingNr
        //DisbursementAccountNr
        public const string XML_LoginName_TAG = "LoginName";
        public const string XML_LangId_TAG = "LangId";
        public const string XML_DefaultCompanyName_TAG = "DefaultCompanyName";
        public const string XML_RoleName1_TAG = "RoleName1";
        public const string XML_RoleName2_TAG = "RoleName2";
        public const string XML_RoleName3_TAG = "RoleName3";
        public const string XML_AttestRoleName1_TAG = "AttestRoleName1";
        public const string XML_AttestRoleName2_TAG = "AttestRoleName2";
        public const string XML_AttestRoleName3_TAG = "AttestRoleName3";
        public const string XML_AttestRoleName4_TAG = "AttestRoleName4";
        public const string XML_AttestRoleName5_TAG = "AttestRoleName5";

        #endregion

        #region Employment information

        public const string XML_EmployeeNr_TAG =  "EmployeeNr";
        //EmploymentType
        public const string XML_EmployeeGroupName_TAG =  "EmployeeGroupName";
        public const string XML_PayrollGroup_TAG = "PayrollGroup";
        public const string XML_VacationGroup_TAG = "VacationGroup";
        public const string XML_EmploymentDate_TAG =  "EmploymentDate";
        public const string XML_EndDate_TAG =  "EndDate";
        //WorkTimeWeek
        public const string XML_WorkTimeWeek = "WorkTimeWeek";
        public const string XML_WorkPercentage = "WorkPercentage";
        //EmploymentPriceTypeCode
        //EmploymentPriceTypeFromDate
        //EmploymentPriceTypeAmount
        public const string XML_CostAccountStd_TAG = "CostAccountStd";
        public const string XML_CostAccountInternal1_TAG = "CostAccountInternal1";
        public const string XML_CostAccountInternal2_TAG = "CostAccountInternal2";
        public const string XML_CostAccountInternal3_TAG = "CostAccountInternal3";
        public const string XML_CostAccountInternal4_TAG = "CostAccountInternal4";
        public const string XML_CostAccountInternal5_TAG = "CostAccountInternal5";
        public const string XML_IncomeAccountStd_TAG = "IncomeAccountStd";
        public const string XML_IncomeAccountInternal1_TAG = "IncomeAccountInternal1";
        public const string XML_IncomeAccountInternal2_TAG = "IncomeAccountInternal2";
        public const string XML_IncomeAccountInternal3_TAG = "IncomeAccountInternal3";
        public const string XML_IncomeAccountInternal4_TAG = "IncomeAccountInternal4";
        public const string XML_IncomeAccountInternal5_TAG = "IncomeAccountInternal5";
        //EarnedDaysPaid
        //UsedDaysPaid
        //RemainingDaysPaid
        //EmploymentRatePaid
        //PaidVacationAllowance
        //EarnedDaysUnpaid
        //UsedDaysUnpaid
        //RemainingDaysUnpaid
        //EarnedDaysAdvance
        //UsedDaysAdvance
        //RemainingDaysAdvance
        //DebtInAdvanceAmount
        //DebtInAdvanceDueDate
        //SavedDaysYear1
        //UsedDaysYear1
        //RemainingDaysYear1
        //EmploymentRateYear1
        //SavedDaysYear2
        //UsedDaysYear2
        //RemainingDaysYear2
        //EmploymentRateYear2
        //SavedDaysYear3
        //UsedDaysYear3
        //RemainingDaysYear3
        //EmploymentRateYear3
        //SavedDaysYear4
        //UsedDaysYear4
        //RemainingDaysYear4
        //EmploymentRateYear4
        //SavedDaysYear5
        //UsedDaysYear5
        //RemainingDaysYear5
        //EmploymentRateYear5
        //SavedDaysOverdue
        //UsedDaysOverdue
        //RemainingDaysOverdue
        //EmploymentRateOverdue
        //HighRiskProtection
        //HighRiskProtectionTo
        //MedicalCertificateReminder
        //MedicalCertificateDays
        //Absence105DaysExcluded
        //Absence105DaysExcludedDays
        //EmployeeFactorType
        //EmployeeFactorFromDate
        //EmployeeFactorFactory
        //PayrollStatisticsPersonalCategory
        //PayrollStatisticsWorkTimeCategory
        //PayrollStatisticsSalaryType
        //PayrollStatisticsWorkPlaceNumber
        //PayrollStatisticsCFARNumber
        //WorkPlaceSCB
        //AFACategory
        //AFASpecialAgreement
        //AFAWorkplaceNr
        //CollectumITPPlan
        //CollectumCostPlace
        //CollectumAgreedOnProduct
        public const string XML_CategoryCode1_TAG =  "CategoryCode1";
        public const string XML_CategoryCode2_TAG =  "CategoryCode2";
        public const string XML_CategoryCode3_TAG =  "CategoryCode3";
        public const string XML_CategoryCode4_TAG =  "CategoryCode4";
        public const string XML_CategoryCode5_TAG =  "CategoryCode5";
        public const string XML_SecondaryCategoryCode1_TAG =  "SecondaryCategoryCode1";
        public const string XML_SecondaryCategoryCode2_TAG =  "SecondaryCategoryCode2";
        public const string XML_SecondaryCategoryCode3_TAG =  "SecondaryCategoryCode3";
        public const string XML_SecondaryCategoryCode4_TAG =  "SecondaryCategoryCode4";
        public const string XML_SecondaryCategoryCode5_TAG =  "SecondaryCategoryCode5";
        public const string XML_DefaultTimeDeviationCauseName_TAG =  "DefaultTimeDeviationCauseName";
        public const string XML_DefaultTimeCodeName_TAG =  "DefaultTimeCodeName";

        #endregion

        #region HR

        public string XML_EmployeeePositionCode_TAG { get; set; }
        public string XML_Note_TAG = "Note";

        #endregion

        #endregion

        #region Constructors

        public EmployeeIOItem()
        {
            //Should not be used. Only for serialization and webservice calls
        }

        public EmployeeIOItem(TermGroup_IOSource source, TermGroup_IOType ioType, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            this.EmployeeIOs = new List<EmployeeIODTO>();
            this.Source = source;
            this.IOType = ioType;
            this.HeadType = headType;
            this.ActorCompanyId = actorCompanyId;
        }

        public EmployeeIOItem(List<EmployeeIODTO> employeeIOs, TermGroup_IOSource source, TermGroup_IOType ioType, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            this.EmployeeIOs = new List<EmployeeIODTO>();
            this.Source = source;
            this.IOType = ioType;
            this.HeadType = headType;
            this.ActorCompanyId = actorCompanyId;

            if (employeeIOs != null)
            {
                this.EmployeeIOs.AddRange(employeeIOs);
                this.ConfigureEmployeeIOs();
            }
        }

        public EmployeeIOItem(List<string> contents, TermGroup_IOSource source, TermGroup_IOType ioType, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            this.EmployeeIOs = new List<EmployeeIODTO>();
            this.Source = source;
            this.IOType = ioType;
            this.HeadType = headType;
            this.ActorCompanyId = actorCompanyId;

            CreateEmployeeIOs(contents);
        }

        public EmployeeIOItem(string content, TermGroup_IOSource source, TermGroup_IOType ioType, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            this.EmployeeIOs = new List<EmployeeIODTO>();
            this.Source = source;
            this.IOType = ioType;
            this.HeadType = headType;
            this.ActorCompanyId = actorCompanyId;
            
            CreateEmployeeIOs(content);
        }

        #endregion

        #region Public methods

        public List<EmployeeIODTO> GetValidEmployeeIOs()
        {
            return this.EmployeeIOs.Where(io => !String.IsNullOrEmpty(io.EmployeeNr) && io.State != SoeEntityState.Deleted).ToList();
        }

        public void CreateEmployeeIOs(List<string> contents)
        {
            if (contents == null || contents.Count == 0)
                return;

            foreach (string content in contents)
            {
                CreateEmployeeIOs(content);
            }
        }

        public void CreateEmployeeIOs(string content)
        {
            CreateEmployeeIOs(XmlUtil.GetChildElements(XDocument.Parse(content), XML_PARENT_TAG));
        }

        public void CreateEmployeeIOs(List<XElement> employeeElements)
        {
            #region Perform

            foreach (XElement employeeElement in employeeElements)
            {
                #region Prereq

                int? employeeId = StringUtility.GetNullableInt(XmlUtil.GetChildElementValue(employeeElement, XML_EmployeeId_TAG));

                EmployeeIODTO io = new EmployeeIODTO(this.Source, this.IOType, this.HeadType, this.ActorCompanyId, employeeId);

                #endregion

                #region Employee

                #region Personal information

                io.FirstName = XmlUtil.GetChildElementValue(employeeElement, XML_FirstName_TAG);
                io.LastName = XmlUtil.GetChildElementValue(employeeElement, XML_LastName_TAG);
                io.SocialSec = XmlUtil.GetChildElementValue(employeeElement, XML_SocialSec_TAG);
                io.Sex = StringUtility.GetNullableInt(XmlUtil.GetChildElementValue(employeeElement, XML_Sex_TAG));
                io.Email = XmlUtil.GetChildElementValue(employeeElement, XML_Email_TAG);
                io.DistributionAddress = XmlUtil.GetChildElementValue(employeeElement, XML_DistributionAddress_TAG);
                io.DistributionCoAddress = XmlUtil.GetChildElementValue(employeeElement, XML_DistributionCoAddress_TAG);
                io.DistributionPostalCode = XmlUtil.GetChildElementValue(employeeElement, XML_DistributionPostalCode_TAG);
                io.DistributionPostalAddress = XmlUtil.GetChildElementValue(employeeElement, XML_DistributionPostalAddress_TAG);
                io.DistributionCountry = XmlUtil.GetChildElementValue(employeeElement, XML_DistributionCountry_TAG);
                io.PhoneHome = XmlUtil.GetChildElementValue(employeeElement, XML_PhoneHome_TAG);
                io.PhoneMobile = XmlUtil.GetChildElementValue(employeeElement, XML_PhoneMobile_TAG);
                io.PhoneJob = XmlUtil.GetChildElementValue(employeeElement, XML_PhoneJob_TAG);
                io.ClosestRelativeNr = XmlUtil.GetChildElementValue(employeeElement, XML_ClosestRelativeNr_TAG);
                io.ClosestRelativeName = XmlUtil.GetChildElementValue(employeeElement, XML_ClosestRelativeName_TAG);
                io.ClosestRelativeRelation = XmlUtil.GetChildElementValue(employeeElement, XML_ClosestRelativeRelation_TAG);
                //ClosestRelativeNr2
                //ClosestRelativeName2
                //ClosestRelativeRelation2
                //DisbursementMethod
                //DisbursementClearingNr
                //DisbursementAccountNr
                io.LoginName = XmlUtil.GetChildElementValue(employeeElement, XML_LoginName_TAG); 
                io.LangId = XmlUtil.GetChildElementValue(employeeElement, XML_LangId_TAG);
                io.DefaultCompanyName = XmlUtil.GetChildElementValue(employeeElement, XML_DefaultCompanyName_TAG);
                io.RoleName1 = XmlUtil.GetChildElementValue(employeeElement, XML_RoleName1_TAG);
                io.RoleName2 = XmlUtil.GetChildElementValue(employeeElement, XML_RoleName2_TAG);
                io.RoleName3 = XmlUtil.GetChildElementValue(employeeElement, XML_RoleName3_TAG);
                io.AttestRoleName1 = XmlUtil.GetChildElementValue(employeeElement, XML_AttestRoleName1_TAG);
                io.AttestRoleName2 = XmlUtil.GetChildElementValue(employeeElement, XML_AttestRoleName2_TAG);
                io.AttestRoleName3 = XmlUtil.GetChildElementValue(employeeElement, XML_AttestRoleName3_TAG);
                io.AttestRoleName4 = XmlUtil.GetChildElementValue(employeeElement, XML_AttestRoleName4_TAG);
                io.AttestRoleName5 = XmlUtil.GetChildElementValue(employeeElement, XML_AttestRoleName5_TAG);

                if (String.IsNullOrEmpty(io.LastName) && !String.IsNullOrEmpty(io.FirstName))
                {
                    string firstName = io.FirstName;
                    string[] name = io.FirstName.Split(' ');
                    io.LastName = name.LastOrDefault();
                    io.FirstName = firstName.Left(firstName.Length - io.LastName.Length).Trim();
                }

                #endregion

                #region Employment information

                io.EmployeeNr = XmlUtil.GetChildElementValue(employeeElement, XML_EmployeeNr_TAG);
                //EmploymentType
                io.EmployeeGroupName = XmlUtil.GetChildElementValue(employeeElement, XML_EmployeeGroupName_TAG);
                io.PayrollGroupName = XmlUtil.GetChildElementValue(employeeElement, XML_PayrollGroup_TAG);
                io.VacationGroupName = XmlUtil.GetChildElementValue(employeeElement, XML_VacationGroup_TAG);
                io.EmploymentDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(employeeElement, XML_EmploymentDate_TAG));
                io.EndDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(employeeElement, XML_EndDate_TAG));
                io.WorkTimeWeek = NumberUtility.ToNullableInteger(XmlUtil.GetChildElementValue(employeeElement, XML_WorkTimeWeek));
                io.WorkPercentage = NumberUtility.ToNullableDecimal(XmlUtil.GetChildElementValue(employeeElement, XML_WorkPercentage), 2);
                //EmploymentPriceTypeCode
                //EmploymentPriceTypeFromDate
                //EmploymentPriceTypeAmount
                io.CostAccountStd = XmlUtil.GetChildElementValue(employeeElement, XML_CostAccountStd_TAG);
                io.CostAccountInternal1 = XmlUtil.GetChildElementValue(employeeElement, XML_CostAccountInternal1_TAG);
                io.CostAccountInternal2 = XmlUtil.GetChildElementValue(employeeElement, XML_CostAccountInternal2_TAG);
                io.CostAccountInternal3 = XmlUtil.GetChildElementValue(employeeElement, XML_CostAccountInternal3_TAG);
                io.CostAccountInternal4 = XmlUtil.GetChildElementValue(employeeElement, XML_CostAccountInternal4_TAG);
                io.CostAccountInternal5 = XmlUtil.GetChildElementValue(employeeElement, XML_CostAccountInternal5_TAG);
                io.IncomeAccountStd = XmlUtil.GetChildElementValue(employeeElement, XML_IncomeAccountStd_TAG);
                io.IncomeAccountInternal1 = XmlUtil.GetChildElementValue(employeeElement, XML_IncomeAccountInternal1_TAG);
                io.IncomeAccountInternal2 = XmlUtil.GetChildElementValue(employeeElement, XML_IncomeAccountInternal2_TAG);
                io.IncomeAccountInternal3 = XmlUtil.GetChildElementValue(employeeElement, XML_IncomeAccountInternal3_TAG);
                io.IncomeAccountInternal4 = XmlUtil.GetChildElementValue(employeeElement, XML_IncomeAccountInternal4_TAG);
                io.IncomeAccountInternal5 = XmlUtil.GetChildElementValue(employeeElement, XML_IncomeAccountInternal5_TAG);
                //EarnedDaysPaid
                //UsedDaysPaid
                //RemainingDaysPaid
                //EmploymentRatePaid
                //PaidVacationAllowance
                //EarnedDaysUnpaid
                //UsedDaysUnpaid
                //RemainingDaysUnpaid
                //EarnedDaysAdvance
                //UsedDaysAdvance
                //RemainingDaysAdvance
                //DebtInAdvanceAmount
                //DebtInAdvanceDueDate
                //SavedDaysYear1
                //UsedDaysYear1
                //RemainingDaysYear1
                //EmploymentRateYear1
                //SavedDaysYear2
                //UsedDaysYear2
                //RemainingDaysYear2
                //EmploymentRateYear2
                //SavedDaysYear3
                //UsedDaysYear3
                //RemainingDaysYear3
                //EmploymentRateYear3
                //SavedDaysYear4
                //UsedDaysYear4
                //RemainingDaysYear4
                //EmploymentRateYear4
                //SavedDaysYear5
                //UsedDaysYear5
                //RemainingDaysYear5
                //EmploymentRateYear5
                //SavedDaysOverdue
                //UsedDaysOverdue
                //RemainingDaysOverdue
                //EmploymentRateOverdue
                //HighRiskProtection
                //HighRiskProtectionTo
                //MedicalCertificateReminder
                //MedicalCertificateDays
                //Absence105DaysExcluded
                //Absence105DaysExcludedDays
                //EmployeeFactorType
                //EmployeeFactorFromDate
                //EmployeeFactorFactory
                //PayrollStatisticsPersonalCategory
                //PayrollStatisticsWorkTimeCategory
                //PayrollStatisticsSalaryType
                //PayrollStatisticsWorkPlaceNumber
                //PayrollStatisticsCFARNumber
                //WorkPlaceSCB
                //AFACategory
                //AFASpecialAgreement
                //AFAWorkplaceNr
                //CollectumITPPlan
                //CollectumCostPlace
                //CollectumAgreedOnProduct
                io.CategoryCode1 = XmlUtil.GetChildElementValue(employeeElement, XML_CategoryCode1_TAG);
                io.CategoryCode2 = XmlUtil.GetChildElementValue(employeeElement, XML_CategoryCode2_TAG);
                io.CategoryCode3 = XmlUtil.GetChildElementValue(employeeElement, XML_CategoryCode3_TAG);
                io.CategoryCode4 = XmlUtil.GetChildElementValue(employeeElement, XML_CategoryCode4_TAG);
                io.CategoryCode5 = XmlUtil.GetChildElementValue(employeeElement, XML_CategoryCode5_TAG);
                io.SecondaryCategoryCode1 = XmlUtil.GetChildElementValue(employeeElement, XML_SecondaryCategoryCode1_TAG);
                io.SecondaryCategoryCode2 = XmlUtil.GetChildElementValue(employeeElement, XML_SecondaryCategoryCode2_TAG);
                io.SecondaryCategoryCode3 = XmlUtil.GetChildElementValue(employeeElement, XML_SecondaryCategoryCode3_TAG);
                io.SecondaryCategoryCode4 = XmlUtil.GetChildElementValue(employeeElement, XML_SecondaryCategoryCode4_TAG);
                io.SecondaryCategoryCode5 = XmlUtil.GetChildElementValue(employeeElement, XML_SecondaryCategoryCode5_TAG);
                io.DefaultTimeDeviationCauseName = XmlUtil.GetChildElementValue(employeeElement, XML_DefaultTimeDeviationCauseName_TAG);
                io.DefaultTimeCodeName = XmlUtil.GetChildElementValue(employeeElement, XML_DefaultTimeCodeName_TAG);

                #endregion

                #region HR

                io.EmployeePositionCode = XmlUtil.GetChildElementValue(employeeElement, XML_EmployeeePositionCode_TAG);
                io.Note = XmlUtil.GetChildElementValue(employeeElement, XML_Note_TAG);

                #endregion

                #region Core

                io.State = StringUtility.GetEntityState(XmlUtil.GetChildElementValue(employeeElement, XML_State_TAG), (int)SoeEntityState.Active);

                #endregion

                EmployeeIOs.Add(io);

                #endregion
            }

            #endregion
        }

        public void CreateEmployeeIO(DataRow row)
        {
            #region Prereq

            //Must have EmployeeId or EmployeeNr
            int? employeeId = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeId));
            string employeeNr = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeNr));
            if ((!employeeId.HasValue || employeeId.Value == 0) && String.IsNullOrEmpty(employeeNr))
                return;

            EmployeeIODTO io = new EmployeeIODTO(this.Source, this.IOType, this.HeadType, this.ActorCompanyId);

            #endregion

            #region Employee

            if (row != null)
            {
                #region Core

                io.EmployeeId = employeeId;

                #endregion

                #region Personal information

                io.FirstName = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.FirstName));
                io.LastName = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.LastName));
                io.SocialSec = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SocialSec));
                io.Sex = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.Sex));
                io.Email = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.Email));
                io.DistributionAddress = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DistributionAddress));
                io.DistributionCoAddress = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DistributionCoAddress));
                io.DistributionPostalCode = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DistributionPostalCode));
                io.DistributionPostalAddress = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DistributionPostalAddress));
                io.DistributionCountry = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DistributionCountry));
                io.PhoneHome = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.PhoneHome));
                io.PhoneMobile = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.PhoneMobile));
                io.PhoneJob = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.PhoneJob));
                io.ClosestRelativeNr = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.ClosestRelativeNr));
                io.ClosestRelativeName = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.ClosestRelativeName));
                io.ClosestRelativeRelation = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.ClosestRelativeRelation));
                io.ClosestRelativeNr2 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.ClosestRelativeNr2));
                io.ClosestRelativeName2 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.ClosestRelativeName2));
                io.ClosestRelativeRelation2 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.ClosestRelativeRelation2));
                io.DisbursementMethod = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DisbursementMethod));
                io.DisbursementClearingNr = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DisbursementClearingNr));
                io.DisbursementAccountNr = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DisbursementAccountNr));
                io.LoginName = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.LoginName));
                io.LangId = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.LangId));
                io.DefaultCompanyName = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DefaultCompanyName));
                io.RoleName1 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RoleName1));
                io.RoleName2 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RoleName2));
                io.RoleName3 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RoleName3));
                io.AttestRoleName1 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AttestRoleName1));
                io.AttestRoleName2 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AttestRoleName2));
                io.AttestRoleName3 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AttestRoleName3));
                io.AttestRoleName4 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AttestRoleName4));
                io.AttestRoleName5 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AttestRoleName5));
                io.AttestRoleAccount1 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AttestRoleNameAccount1));
                io.AttestRoleAccount2 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AttestRoleNameAccount2));
                io.AttestRoleAccount3 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AttestRoleNameAccount3));
                io.AttestRoleAccount4 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AttestRoleNameAccount4));
                io.AttestRoleAccount5 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AttestRoleNameAccount5));

                #endregion

                #region Employment information

                io.EmployeeNr = employeeNr;
                string employmentType = ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentType)?.ToString();
                if (EmploymentTypeDTO.IsStandard(employmentType, out int employmentTypeId, out _))
                    io.EmploymentType = employmentTypeId.ToNullable();
                else
                    io.EmploymentTypeCode = employmentType;
                io.EmployeeGroupName = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeGroupName));
                io.PayrollGroupName = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.PayrollGroupName));
                io.VacationGroupName = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.VacationGroupName));
                io.EmploymentDate = CalendarUtility.GetNullableDateTime(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentDate));
                io.EndDate = CalendarUtility.GetNullableDateTime(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EndDate));
                io.WorkTimeWeek = CalendarUtility.GetMinutesFromString(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.WorkTimeWeek));
                io.EmploymentPriceTypeCode = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentPriceTypeCode));
                io.EmploymentPayrollLevelCode = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentPayrollLevelCode));
                io.EmploymentPriceTypeFromDate = CalendarUtility.GetNullableDateTime(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentPriceTypeFromDate));
                io.EmploymentPriceTypeAmount = StringUtility.GetAmount(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentPriceTypeAmount));
                io.CostAccountStd = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CostAccountStd));
                io.CostAccountInternal1 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CostAccountInternal1));
                io.CostAccountInternal2 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CostAccountInternal2));
                io.CostAccountInternal3 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CostAccountInternal3));
                io.CostAccountInternal4 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CostAccountInternal4));
                io.CostAccountInternal5 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CostAccountInternal5));
                io.IncomeAccountStd = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.IncomeAccountStd));
                io.IncomeAccountInternal1 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.IncomeAccountInternal1));
                io.IncomeAccountInternal2 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.IncomeAccountInternal2));
                io.IncomeAccountInternal3 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.IncomeAccountInternal3));
                io.IncomeAccountInternal4 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.IncomeAccountInternal4));
                io.IncomeAccountInternal5 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.IncomeAccountInternal5));
                io.EarnedDaysPaid = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EarnedDaysPaid));
                io.UsedDaysPaid = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.UsedDaysPaid));
                io.RemainingDaysPaid = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RemainingDaysPaid));
                io.EmploymentRatePaid = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentRatePaid));
                io.PaidVacationAllowance = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.Paidvacationallowance));
                io.EarnedDaysUnpaid = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EarnedDaysUnpaid));
                io.UsedDaysUnpaid = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.UsedDaysUnpaid));
                io.RemainingDaysUnpaid = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RemainingDaysUnpaid));
                io.EarnedDaysAdvance = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EarnedDaysAdvance));
                io.UsedDaysAdvance = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.UsedDaysAdvance));
                io.RemainingDaysAdvance = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RemainingDaysAdvance));
                io.DebtInAdvanceAmount = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DebtInAdvanceAmount));
                io.DebtInAdvanceDueDate = CalendarUtility.GetNullableDateTime(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DebtInAdvanceDueDate));
                io.SavedDaysYear1 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SavedDaysYear1));
                io.UsedDaysYear1 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.UsedDaysYear1));
                io.RemainingDaysYear1 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RemainingDaysYear1));
                io.EmploymentRateYear1 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentRateYear1));
                io.SavedDaysYear2 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SavedDaysYear2));
                io.UsedDaysYear2 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.UsedDaysYear2));
                io.RemainingDaysYear2 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RemainingDaysYear2));
                io.EmploymentRateYear2 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentRateYear2));
                io.SavedDaysYear3 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SavedDaysYear3));
                io.UsedDaysYear3 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.UsedDaysYear3));
                io.RemainingDaysYear3 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RemainingDaysYear3));
                io.EmploymentRateYear3 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentRateYear3));
                io.SavedDaysYear4 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SavedDaysYear4));
                io.UsedDaysYear4 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.UsedDaysYear4));
                io.RemainingDaysYear4 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RemainingDaysYear4));
                io.EmploymentRateYear4 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentRateYear4));
                io.SavedDaysYear5 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SavedDaysYear5));
                io.UsedDaysYear5 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.UsedDaysYear5));
                io.RemainingDaysYear5 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RemainingDaysYear5));
                io.EmploymentRateYear5 = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentRateYear5));
                io.SavedDaysOverdue = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SavedDaysOverdue));
                io.UsedDaysOverdue = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.UsedDaysOverdue));
                io.RemainingDaysOverdue = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.RemainingDaysOverdue));
                io.EmploymentRateOverdue = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmploymentRateOverdue));
                io.HighRiskProtection = StringUtility.GetNullableBool(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.HighRiskProtection));
                io.HighRiskProtectionTo = CalendarUtility.GetNullableDateTime(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.HighRiskProtectionTo));
                io.MedicalCertificateReminder = StringUtility.GetNullableBool(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.MedicalCertificateReminder));
                io.MedicalCertificateDays = StringUtility.GetInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.MedicalCertificateDays));
                io.Absence105DaysExcluded = StringUtility.GetNullableBool(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.Absence105DaysExcluded));
                io.Absence105DaysExcludedDays = StringUtility.GetInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.Absence105DaysExcludedDays));
                io.EmployeeFactorType = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeFactorType));
                io.EmployeeFactorFromDate = CalendarUtility.GetNullableDateTime(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeFactorFromDate));
                io.EmployeeFactorFactor = StringUtility.GetNullableDecimal(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeFactorFactor));
                io.PayrollStatisticsPersonalCategory = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.PayrollStatisticsPersonalCategory));
                io.PayrollStatisticsWorkTimeCategory = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.PayrollStatisticsWorkTimeCategory));
                io.PayrollStatisticsSalaryType = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.PayrollStatisticsSalaryType));
                io.PayrollStatisticsWorkPlaceNumber = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.PayrollStatisticsWorkPlaceNumber));
                io.PayrollStatisticsCFARNumber = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.PayrollStatisticsCFARNumber));
                io.WorkPlaceSCB = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.WorkPlaceSCB));
                io.AFACategory = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AFACategory));
                io.AFASpecialAgreement = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AFASpecialAgreement));
                io.AFAWorkplaceNr = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.AFAWorkplaceNr));
                io.CollectumITPPlan = StringUtility.GetNullableInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CollectumITPPlan));
                io.CollectumCostPlace = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CollectumCostPlace));
                io.CollectumAgreedOnProduct = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CollectumAgreedOnProduct));
                io.CategoryCode1 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CategoryCode1));
                io.CategoryCode2 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CategoryCode2));
                io.CategoryCode3 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CategoryCode3));
                io.CategoryCode4 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CategoryCode4));
                io.CategoryCode5 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.CategoryCode5));
                io.SecondaryCategoryCode1 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SecondaryCategoryCode1));
                io.SecondaryCategoryCode2 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SecondaryCategoryCode2));
                io.SecondaryCategoryCode3 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SecondaryCategoryCode3));
                io.SecondaryCategoryCode4 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SecondaryCategoryCode4));
                io.SecondaryCategoryCode5 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.SecondaryCategoryCode5));
                io.EmployeeAccount1 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeAccount1));
                io.EmployeeAccount2 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeAccount2));
                io.EmployeeAccount3 = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeAccount3));
                io.EmployeeAccountStartDate1 = CalendarUtility.GetNullableDateTime(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeAccountDateFrom1));
                io.EmployeeAccountStartDate2 = CalendarUtility.GetNullableDateTime(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeAccountDateFrom2));
                io.EmployeeAccountStartDate3 = CalendarUtility.GetNullableDateTime(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeAccountDateFrom3));
                io.EmployeeAccountDefault1 = StringUtility.GetNullableBool(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeAccountDefault1));
                io.EmployeeAccountDefault2 = StringUtility.GetNullableBool(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeAccountDefault2));
                io.EmployeeAccountDefault3 = StringUtility.GetNullableBool(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeeAccountDefault3));
                io.DefaultTimeDeviationCauseName = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DefaultTimeDeviationCauseName));
                io.DefaultTimeCodeName = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.DefaultTimeCodeName));
                io.ExperienceMonths = StringUtility.GetInt(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.ExperienceMonths));
                io.ExperienceAgreedOrEstablished = StringUtility.GetBool(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.ExperienceAgreedOrEstablished));
                io.WorkPlace = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.WorkPlace));

                #endregion

                #region HR

                io.EmployeePositionCode = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.EmployeePositionCode));
                io.Note = StringUtility.GetValue(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.Note));

                #endregion

                #region Core

                //Fix for not overwriting existing Employee's state to default value Active. Temporary is changed to the existing state of the employee later in the import
                io.State = StringUtility.GetEntityState(ExcelUtil.GetColumnValue(row, ExcelColumnEmployee.State), (int)SoeEntityState.Temporary);

                #endregion

                EmployeeIOs.Add(io);
            }

            #endregion
        }

        public void ConfigureEmployeeIOs()
        {
            foreach (var employeeIO in this.EmployeeIOs)
            {
                employeeIO.Configure(this.Source, this.IOType, this.HeadType, this.ActorCompanyId);
            }
        }

        #endregion
    }
}