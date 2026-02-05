using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Common.Util;
using System;
using System.Globalization;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class UpdateSchedules : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            int type = 0;
            if (!string.IsNullOrEmpty(QS["type"]))
                int.TryParse(QS["type"], out type);

            int timeHalfdayId = 0;
            int dayTypeId = 0;
            int holidayId = 0;
            DateTime? oldDateToDelete = null;

            switch(type)            
            {
                case (int)SoeTimeUniqueDayEdit.AddHalfDay:
                    int.TryParse(QS["halfday"], out timeHalfdayId);
                    break;
                case (int)SoeTimeUniqueDayEdit.UpdateHalfDay:
                    int.TryParse(QS["halfday"], out timeHalfdayId);
                    int.TryParse(QS["daytype"], out dayTypeId);
                    break;
                case (int)SoeTimeUniqueDayEdit.DeleteHalfDay:
                    int.TryParse(QS["halfday"], out timeHalfdayId);
                    break;
                case (int)SoeTimeUniqueDayEdit.AddHoliday:
                    int.TryParse(QS["holiday"], out holidayId);
                    int.TryParse(QS["daytype"], out dayTypeId);
                    break;
                case (int)SoeTimeUniqueDayEdit.UpdateHoliday:
                    int.TryParse(QS["holiday"], out holidayId);
                    int.TryParse(QS["daytype"], out dayTypeId);
                    if (!String.IsNullOrEmpty(QS["oldDate"]))
                        oldDateToDelete = DateTime.ParseExact(QS["oldDate"], "yyyyMMdd", CultureInfo.InvariantCulture);
                    break;
                case (int)SoeTimeUniqueDayEdit.DeleteHoliday:
                    int.TryParse(QS["holiday"], out holidayId);
                    int.TryParse(QS["daytype"], out dayTypeId);              
                    break;
            }

            #endregion

            #region Content

            string action = Url;
            string headerText = string.Empty;
            string infoText = string.Empty;

            action += "&update=true";
            headerText = GetText(4480, "Uppdatera aktiverade scheman");
            infoText = GetText(4479, "En ändring har registrerats. Klicka på OK för att återställa till grundschema samt räkna om dagarna (Om du använder avvikelse). Har du redan gjort manuella ändringar på schemat för dessa dagar rekommenderas att ni klickar på avbryt och räknar om dagarna under Tid - Räkna om. ");

            #endregion

            ((ModalFormMaster)Master).HeaderText = headerText;
            ((ModalFormMaster)Master).InfoText = infoText;
            ((ModalFormMaster)Master).Action = action;

            #region Parse action

            bool update = false;
            if (!String.IsNullOrEmpty(QS["update"]))
                 update = Convert.ToBoolean(QS["update"]);
            
            if(update)
            {
                TimeEngineManager tem = new TimeEngineManager(ParameterObject, SoeCompany.ActorCompanyId, UserId);
                TimeEngineOutputDTO oDTO = null;

                switch ((SoeTimeUniqueDayEdit)type)
                { 
                    case SoeTimeUniqueDayEdit.AddHalfDay:
                        #region AddHalfDay

                        oDTO = tem.SaveUniqueDayFromHalfDay(timeHalfdayId, false);

                        #endregion
                        break;
                    case SoeTimeUniqueDayEdit.UpdateHalfDay:
                        #region UpdateHalfDay

                        oDTO = tem.UpdateUniqueDayFromHalfDay(timeHalfdayId, dayTypeId);

                        #endregion
                        break;
                    case SoeTimeUniqueDayEdit.DeleteHalfDay:
                        #region DeleteHalfDay

                        oDTO = tem.SaveUniqueDayFromHalfDay(timeHalfdayId, false);

                        #endregion
                        break;
                    case SoeTimeUniqueDayEdit.AddHoliday:
                        #region AddHoliday

                        oDTO = tem.AddUniqueDayFromHoliday(holidayId, dayTypeId);

                        #endregion
                        break;
                    case SoeTimeUniqueDayEdit.UpdateHoliday:                    
                        #region UpdateHoliday

                        if (oldDateToDelete.HasValue && CalendarUtility.ToUrlFriendlyDateTime(oldDateToDelete.Value) == CalendarUtility.URL_FRIENDLY_DATETIME_DEFAULT)
                            oldDateToDelete = null;

                        oDTO = tem.UpdateUniqueDayFromHoliday(holidayId, dayTypeId, oldDateToDelete);

                        #endregion
                        break;
                    case SoeTimeUniqueDayEdit.DeleteHoliday:
                        #region DeleteHoliday

                        oDTO = tem.DeleteUniqueDayFromHoliday(holidayId, dayTypeId, null);

                        #endregion
                        break;
                }
                
                Response.Redirect(Request.UrlReferrer.ToString());            
            }

            #endregion
        }
    }
}