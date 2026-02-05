using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class EmployeeVacationSEIOItem
    {

        #region Collections

        public List<EmployeeVacationSEIODTO> employeeVacationSEIOs = new List<EmployeeVacationSEIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "EmployeeVacationSEIODTO";

        public string XML_EmployeeNr_TAG = "EmployeeNr";
        public string XML_TotalDaysVacation_TAG = "TotalDaysVacation";
        public string XML_VacationGroupCode_TAG = "VacationGroupCode";
        public string XML_EarnedDaysPaid_TAG = "EarnedDaysPaid";
        public string XML_EarnedDaysUnpaid_TAG = "EarnedDaysUnpaid";
        public string XML_EarnedDaysAdvance_TAG = "EarnedDaysAdvance ";
        public string XML_SavedDaysYear1_TAG = "SavedDaysYear1";
        public string XML_SavedDaysYear2_TAG = "SavedDaysYear2";
        public string XML_SavedDaysYear3_TAG = "SavedDaysYear3";
        public string XML_SavedDaysYear4_TAG = "SavedDaysYear4";
        public string XML_SavedDaysYear5_TAG = "SavedDaysYear5";
        public string XML_SavedDaysOverdue_TAG = "SavedDaysOverdue";

        public string XML_UsedDaysPaid_TAG = "UsedDaysPaid";
        public string XML_PaidVacationAllowance_TAG = "PaidVacationAllowance";
        public string XML_PaidVacationVariableAllowance_TAG = "PaidVacationVariableAllowance";
        public string XML_UsedDaysUnpaid_TAG = "UsedDaysUnpaid";
        public string XML_UsedDaysAdvance_TAG = "UsedDaysAdvance";
        public string XML_UsedDaysYear1_TAG = "UsedDaysYear1";
        public string XML_UsedDaysYear2_TAG = "UsedDaysYear2";
        public string XML_UsedDaysYear3_TAG = "UsedDaysYear3";
        public string XML_UsedDaysYear4_TAG = "UsedDaysYear4";
        public string XML_UsedDaysYear5_TAG = "UsedDaysYear5";
        public string XML_UsedDaysOverdue_TAG = "UsedDaysOverdue";

        public string XML_RemainingDaysPaid_TAG = "RemainingDaysPaid";
        public string XML_RemainingDaysUnpaid_TAG = "RemainingDaysUnpaid";
        public string XML_RemainingDaysAdvance_TAG = "RemainingDaysAdvance";
        public string XML_RemainingDaysYear1_TAG = "RemainingDaysYear1";
        public string XML_RemainingDaysYear2_TAG = "RemainingDaysYear2";
        public string XML_RemainingDaysYear3_TAG = "RemainingDaysYear3";
        public string XML_RemainingDaysYear4_TAG = "RemainingDaysYear4";
        public string XML_RemainingDaysYear5_TAG = "RemainingDaysYear5";
        public string XML_RemainingDaysOverdue_TAG = "RemainingDaysOverdue";

        public string XML_EarnedDaysRemainingHoursPaid_TAG = "EarnedDaysRemainingHoursPaid";
        public string XML_EarnedDaysRemainingHoursUnpaid_TAG = "EarnedDaysRemainingHoursUnpaid";
        public string XML_EarnedDaysRemainingHoursAdvance_TAG = "EarnedDaysRemainingHoursAdvance";
        public string XML_EarnedDaysRemainingHoursYear1_TAG = "EarnedDaysRemainingHoursYear1";
        public string XML_EarnedDaysRemainingHoursYear2_TAG = "EarnedDaysRemainingHoursYear2";
        public string XML_EarnedDaysRemainingHoursYear3_TAG = "EarnedDaysRemainingHoursYear3";
        public string XML_EarnedDaysRemainingHoursYear4_TAG = "EarnedDaysRemainingHoursYear4";
        public string XML_EarnedDaysRemainingHoursYear5_TAG = "EarnedDaysRemainingHoursYear5";
        public string XML_EarnedDaysRemainingHoursOverdue_TAG = "EarnedDaysRemainingHoursOverdue";

        public string XML_EmploymentRatePaid_TAG = "EmploymentRatePaid";
        public string XML_EmploymentRateYear1_TAG = "EmploymentRateYear1";
        public string XML_EmploymentRateYear2_TAG = "EmploymentRateYear2";
        public string XML_EmploymentRateYear3_TAG = "EmploymentRateYear3";
        public string XML_EmploymentRateYear4_TAG = "EmploymentRateYear4";
        public string XML_EmploymentRateYear5_TAG = "EmploymentRateYear5";
        public string XML_EmploymentRateOverdue_TAG = "EmploymentRateOverdue";

        public string XML_SavedDaysAmountYear1_TAG = "SavedDaysAmountYear1";
        public string XML_SavedDaysAmountYear2_TAG = "SavedDaysAmountYear2";
        public string XML_SavedDaysAmountYear3_TAG = "SavedDaysAmountYear3";
        public string XML_SavedDaysAmountYear4_TAG = "SavedDaysAmountYear4";
        public string XML_SavedDaysAmountYear5_TAG = "SavedDaysAmountYear5";

        public string XML_DebtInAdvanceAmount_TAG = "DebtInAdvanceAmount";
        public string XML_DebtInAdvanceDueDate_TAG = "DebtInAdvanceDueDate";
        public string XML_DebtInAdvanceDelete_TAG = "DebtInAdvanceDelete";


        #endregion

        #region Constructors

        public EmployeeVacationSEIOItem()
        {
        }

        public EmployeeVacationSEIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public EmployeeVacationSEIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(content, headType, actorCompanyId);
        }


        #endregion

        #region Parse

        private void CreateObjects(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var content in contents)
            {
                CreateObjects(content, headType, actorCompanyId);
            }
        }

        private void CreateObjects(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            XDocument xdoc = XDocument.Parse(content);

            List<XElement> elementEmployeeVacationSEs = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(elementEmployeeVacationSEs, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementEmployeeVacationSEs, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var elementEmployeeVacationSE in elementEmployeeVacationSEs)
            {
                EmployeeVacationSEIODTO employeeVacationSEIODTO = new EmployeeVacationSEIODTO();

                employeeVacationSEIODTO.EmployeeNr = XmlUtil.GetChildElementValue(elementEmployeeVacationSE, XML_EmployeeNr_TAG);
                employeeVacationSEIODTO.VacationGroupCode = XmlUtil.GetChildElementValue(elementEmployeeVacationSE, XML_VacationGroupCode_TAG);
                employeeVacationSEIODTO.TotalDaysVacation = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_TotalDaysVacation_TAG);
                employeeVacationSEIODTO.EarnedDaysPaid = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysPaid_TAG);
                employeeVacationSEIODTO.EarnedDaysUnpaid = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysUnpaid_TAG);
                employeeVacationSEIODTO.EarnedDaysAdvance = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysAdvance_TAG);
                employeeVacationSEIODTO.SavedDaysYear1 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_SavedDaysYear1_TAG);
                employeeVacationSEIODTO.SavedDaysYear2 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_SavedDaysYear2_TAG);
                employeeVacationSEIODTO.SavedDaysYear3 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_SavedDaysYear3_TAG);
                employeeVacationSEIODTO.SavedDaysYear4 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_SavedDaysYear4_TAG);
                employeeVacationSEIODTO.SavedDaysYear5 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_SavedDaysYear5_TAG);
                employeeVacationSEIODTO.SavedDaysOverdue = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_SavedDaysOverdue_TAG);

                employeeVacationSEIODTO.UsedDaysPaid = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_UsedDaysPaid_TAG);
                employeeVacationSEIODTO.PaidVacationAllowance = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_PaidVacationAllowance_TAG);
                employeeVacationSEIODTO.PaidVacationVariableAllowance = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_PaidVacationVariableAllowance_TAG);
                employeeVacationSEIODTO.UsedDaysUnpaid = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_UsedDaysUnpaid_TAG);
                employeeVacationSEIODTO.UsedDaysAdvance = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_UsedDaysAdvance_TAG);
                employeeVacationSEIODTO.UsedDaysYear1 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_UsedDaysYear1_TAG);
                employeeVacationSEIODTO.UsedDaysYear2 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_UsedDaysYear2_TAG);
                employeeVacationSEIODTO.UsedDaysYear3 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_UsedDaysYear3_TAG);
                employeeVacationSEIODTO.UsedDaysYear4 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_UsedDaysYear4_TAG);
                employeeVacationSEIODTO.UsedDaysYear5 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_UsedDaysYear5_TAG);
                employeeVacationSEIODTO.UsedDaysOverdue = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_UsedDaysOverdue_TAG);

                employeeVacationSEIODTO.RemainingDaysPaid = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_RemainingDaysPaid_TAG);
                employeeVacationSEIODTO.RemainingDaysUnpaid = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_RemainingDaysUnpaid_TAG);
                employeeVacationSEIODTO.RemainingDaysAdvance = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_RemainingDaysAdvance_TAG);
                employeeVacationSEIODTO.RemainingDaysYear1 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_RemainingDaysYear1_TAG);
                employeeVacationSEIODTO.RemainingDaysYear2 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_RemainingDaysYear2_TAG);
                employeeVacationSEIODTO.RemainingDaysYear3 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_RemainingDaysYear3_TAG);
                employeeVacationSEIODTO.RemainingDaysYear4 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_RemainingDaysYear4_TAG);
                employeeVacationSEIODTO.RemainingDaysYear5 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_RemainingDaysYear5_TAG);
                employeeVacationSEIODTO.RemainingDaysOverdue = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_RemainingDaysOverdue_TAG);

                employeeVacationSEIODTO.EarnedDaysRemainingHoursPaid = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysRemainingHoursPaid_TAG);
                employeeVacationSEIODTO.EarnedDaysRemainingHoursUnpaid = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysRemainingHoursUnpaid_TAG);
                employeeVacationSEIODTO.EarnedDaysRemainingHoursAdvance = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysRemainingHoursAdvance_TAG);
                employeeVacationSEIODTO.EarnedDaysRemainingHoursYear1 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysRemainingHoursYear1_TAG);
                employeeVacationSEIODTO.EarnedDaysRemainingHoursYear2 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysRemainingHoursYear2_TAG);
                employeeVacationSEIODTO.EarnedDaysRemainingHoursYear3 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysRemainingHoursYear3_TAG);
                employeeVacationSEIODTO.EarnedDaysRemainingHoursYear4 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysRemainingHoursYear4_TAG);
                employeeVacationSEIODTO.EarnedDaysRemainingHoursYear5 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysRemainingHoursYear5_TAG);
                employeeVacationSEIODTO.EarnedDaysRemainingHoursOverdue = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EarnedDaysRemainingHoursOverdue_TAG);

                employeeVacationSEIODTO.EmploymentRatePaid = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EmploymentRatePaid_TAG);
                employeeVacationSEIODTO.EmploymentRateYear1 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EmploymentRateYear1_TAG);
                employeeVacationSEIODTO.EmploymentRateYear2 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EmploymentRateYear2_TAG);
                employeeVacationSEIODTO.EmploymentRateYear3 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EmploymentRateYear3_TAG);
                employeeVacationSEIODTO.EmploymentRateYear4 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EmploymentRateYear4_TAG);
                employeeVacationSEIODTO.EmploymentRateYear5 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EmploymentRateYear5_TAG);
                employeeVacationSEIODTO.EmploymentRateOverdue = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_EmploymentRateOverdue_TAG);

                employeeVacationSEIODTO.SavedDaysAmountYear1 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_SavedDaysAmountYear1_TAG);
                employeeVacationSEIODTO.SavedDaysAmountYear2 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_SavedDaysAmountYear2_TAG);
                employeeVacationSEIODTO.SavedDaysAmountYear3 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_SavedDaysAmountYear3_TAG);
                employeeVacationSEIODTO.SavedDaysAmountYear4 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_SavedDaysAmountYear4_TAG);
                employeeVacationSEIODTO.SavedDaysAmountYear5 = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_SavedDaysAmountYear5_TAG);

                employeeVacationSEIODTO.DebtInAdvanceAmount = XmlUtil.GetElementDecimalValue(elementEmployeeVacationSE, XML_DebtInAdvanceAmount_TAG);
                employeeVacationSEIODTO.DebtInAdvanceDueDate = CalendarUtility.GetDateTime(XML_DebtInAdvanceDueDate_TAG, "YYYY-MM-DD");
                employeeVacationSEIODTO.DebtInAdvanceDelete = XmlUtil.GetElementBoolValue(elementEmployeeVacationSE, XML_DebtInAdvanceDelete_TAG);



                employeeVacationSEIOs.Add(employeeVacationSEIODTO);
            }

        }
        #endregion
    }
}
