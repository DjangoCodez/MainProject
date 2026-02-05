using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class TimeBalanceIOItem
    {

        #region Collections

        public List<TimeBalanceIODTO> timeBalanceIOs = new List<TimeBalanceIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "TimeBalance";

        public string XML_TimeBalanceIOType_TAG = "TimeBalanceIOType";
        public string XML_Code_TAG = "Code";
        public string XML_Date_TAG = "Date";
        public string XML_EmployeeNr_TAG = "EmployeeNr";
        public string XML_Quantity_TAG = "Quantity";
        public string XML_AdjustmentDate_TAG = "AdjustmentDate";

        #endregion

        #region Constructors

        public TimeBalanceIOItem()
        {
        }

        public TimeBalanceIOItem(List<string> contents)
        {
            CreateObjects(contents);
        }

        public TimeBalanceIOItem(string content)
        {
            CreateObjects(content);
        }


        #endregion

        #region Parse

        private void CreateObjects(List<string> contents)
        {
            foreach (var content in contents)
            {
                CreateObjects(content);
            }
        }

        private void CreateObjects(string content)
        {
            XDocument xdoc = XDocument.Parse(content);

            List<XElement> elementTimeBalances = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(elementTimeBalances);
        }

        public void CreateObjects(List<XElement> elementTimeBalances)
        {
            foreach (var elementTimeBalance in elementTimeBalances)
            {
                TimeBalanceIODTO timeBalanceIODTO = new TimeBalanceIODTO()
                {
                    TimeBalanceIOType = (TimeBalanceIOType)XmlUtil.GetElementIntValue(elementTimeBalance, XML_TimeBalanceIOType_TAG),
                    Code = XmlUtil.GetElementNullableValue(elementTimeBalance, XML_Code_TAG),
                    Date = XmlUtil.GetElementDateTimeValue(elementTimeBalance, XML_Date_TAG),
                    EmployeeNr = XmlUtil.GetElementNullableValue(elementTimeBalance, XML_EmployeeNr_TAG),
                    Quantity = XmlUtil.GetElementDecimalValue(elementTimeBalance, XML_Quantity_TAG),
                    AdjustmentDate = XmlUtil.GetElementNullableDateTimeValue(elementTimeBalance, XML_AdjustmentDate_TAG)
                };

                if (!string.IsNullOrEmpty(timeBalanceIODTO.EmployeeNr) && timeBalanceIODTO.TimeBalanceIOType != TimeBalanceIOType.Unknown)
                    timeBalanceIOs.Add(timeBalanceIODTO);
            }
        }
        #endregion
    }
}