using System;
using System.Threading;

namespace SoftOne.Soe.Business.Core.Mobile
{
    public class MobileManangerTexts
    {
        public string GetText(int sysTermId, string defaultTerm)
        {
            string text;

            try
            {
                text = TermCacheManager.Instance.GetText(sysTermId, 1, defaultTerm, Thread.CurrentThread.CurrentCulture.Name);
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
                text = defaultTerm;
            }

            return text;
        }

        public string DocumentTypeMissing
        {
            get
            {
                return GetText(4415, "Typ måste anges");
            }
        }

        public string RecordIdMissing
        {
            get
            {
                return GetText(7545, "Post identifierare saknas");
            }
        }

        public string IncorrectInputMessage
        {
            get
            {
                return GetText(8853, "Felaktiga inparametrar");
            }
        }

        public string CustomersNotFoundMessage
        {
            get
            {
                return GetText(5606, "Inga kunder hittades");
            }
        }
        public string ProductsNotFoundMessage
        {
            get
            {
                return GetText(5607, "Inga artiklar hittades");
            }
        }
        public string OrdersNotFoundMessage
        {
            get
            {
                return GetText(5604, "Inga ordrar hittades");
            }
        }
        public string OrderNotFoundMessage
        {
            get
            {
                return GetText(5605, "Ordern hittades inte");
            }
        }
        public string OrderNotSavedMessage
        {
            get
            {
                return GetText(5610, "Ordern kunde inte sparas");
            }
        }
        public string OrderNotSetToReadyMessage
        {
            get
            {
                return GetText(5653, "Ordern kunde klarmarkeras");
            }
        }
        public string OrderRowsNotFoundMessage
        {
            get
            {
                return GetText(5608, "Inga rader hittades");
            }
        }
        public string OrderRowNotFoundMessage
        {
            get
            {
                return GetText(5611, "Orderraden hittades inte");
            }
        }
        public string OrderRowNotSavedMessage
        {
            get
            {
                return GetText(5650, "Orderraden kunde inte sparas");
            }
        }
        public string OrderRowNotDeletedMessage
        {
            get
            {
                return GetText(5651, "Orderraden kunde inte tas bort");
            }
        }
        public string OrderRowNotAllowedToDeleteMessage
        {
            get
            {
                return GetText(7246, "Behörighet att ta bort orderraden saknas");
            }
        }
        public string TimeRowsNotFoundMessage
        {
            get
            {
                return GetText(5608, "Inga rader hittades");
            }
        }
        public string TimeRowNotFoundMessage
        {
            get
            {
                return GetText(5612, "Tidraden hittades inte");
            }
        }
        public string TimeRowNotSavedMessage
        {
            get
            {
                return GetText(8234, "Tidraden kunde inte sparas");
            }
        }

        public string ExpensesFoundMessage
        {
            get
            {
                return GetText(11828, "Inga utlägg hittades");
            }
        }

        public string ExpenseNotSavedMessage
        {
            get
            {
                return GetText(11829, "Utlägg kunde inte sparas");
            }
        }

        public string EmployeeNotFoundMessage
        {
            get
            {
                return GetText(5027, "Anställd hittades inte");
            }
        }
        public string EmployeeGroupNotFoundMessage
        {
            get
            {
                return GetText(8539, "Tidavtal hittades inte");
            }
        }
        public string PayrollPeriodNotFoundMessage
        {
            get
            {
                return GetText(8250, "Löneperiod hittades inte");
            }
        }

        public string DeviationCausesNotFoundMessage
        {
            get
            {
                return GetText(8251, "Orsaker hittades inte");
            }
        }

        public string DeviationCauseMandatoryNoteMessage
        {
            get
            {
                return GetText(10258, "Du måste ange en notering");
            }
        }


        public string AttestEmployeeDayNotFoundMessage
        {
            get
            {
                return GetText(8252, "Tider hittades inte");
            }
        }

        public string DeviationsNotSavedMessage
        {
            get
            {
                return GetText(8253, "Avvikelser kunde inte sparas");
            }
        }

        public string NoPermissionForOwnAbsenceMessage
        {
            get
            {
                return GetText(3687, "Du har inte behörighet att registrera frånvaro på dig själv");
            }
        }

        public string AbsenceNotSavedMessage
        {
            get
            {
                return GetText(8254, "Frånvaro kunde inte sparas");
            }
        }

        public string PresenceNotSavedMessage
        {
            get
            {
                return GetText(8259, "Närvaro kunde inte sparas");
            }
        }

        public string BreaksNotFoundMessage
        {
            get
            {
                return GetText(8255, "Raster kunde inte hämtas");
            }
        }

        public string AttestNotSavedMessage
        {
            get
            {
                return GetText(8257, "Attest misslyckades");
            }
        }

        public string RestoreToScheduleFailedMessage
        {
            get
            {
                return GetText(8260, "Återställ dag misslyckades");
            }
        }

        public string VatTypesNotFoundMessage
        {
            get
            {
                return GetText(8286, "Momstyper kunder inte hittas");
            }
        }

        public string CurrenciesNotFoundMessage
        {
            get
            {
                return GetText(8287, "Valutor kunde inte hittas");
            }
        }

        public string PriceListTypeNotFoundMessage
        {
            get
            {
                return GetText(8288, "Prislistor kunde inte hittas");
            }
        }

        public string WholeSellersNotFoundMessage
        {
            get
            {
                return GetText(8289, "Grossister kunde inte hittas");
            }
        }

        public string InvoiceDeliveryTypesNotFoundMessage
        {
            get
            {
                return GetText(9600, "Fakturametoder kunde inte hittas");
            }
        }

        public string CustomerNotFoundMessage
        {
            get
            {
                return GetText(8292, "Kund kunde inte hittas");
            }
        }

        public string CustomerHouseholdApplicantNotSaved
        {
            get
            {
                return GetText(7288, "Kunde inte spara sökande");
            }
        }

        public string CustomerHouseholdApplicantNotDeleted
        {
            get
            {
                return GetText(7289, "Kunde inte ta bort sökande");
            }
        }

        public string ImageNotSaved
        {
            get
            {
                return GetText(8293, "Bild kunde inte sparas");
            }
        }

        public string ImageNotDeleted
        {
            get
            {
                return GetText(8302, "Bild kunde inte tas bort");
            }
        }

        public string ImageNotFound
        {
            get
            {
                return GetText(8294, "Bild kunde inte hittas");
            }
        }

        public string LicenseNotFound
        {
            get
            {
                return GetText(8295, "License kunde inte hittas");
            }
        }

        public string OriginUsersNotSaved
        {
            get
            {
                return GetText(8296, "Ägare kunde inte sparas");
            }
        }

        public string SaveFailed
        {
            get
            {
                return GetText(8305, "Misslyckades med att spara");
            }
        }

        public string MapLocationNotSaved
        {
            get
            {
                return GetText(8306, "Kunde inte spara position");
            }
        }

        public string TimeCodesNotFoundMessage
        {
            get
            {
                return GetText(8315, "Kunde inte hämta tidkoder");
            }
        }

        public string ChecklistNotSaved
        {
            get
            {
                return GetText(8340, "Checklista kunde inte läggas till");
            }
        }
        public string ShiftRequestNotFound
        {
            get
            {
                return GetText(8846, "Passförfrågan kunde inte hittas");
            }
        }

        public string SetShiftAsUnWantedFailed
        {
            get
            {
                return GetText(8344, "Erbjud pass misslyckades");
            }
        }
        public string UpdateShiftSetUndoUnWantedFailed
        {
            get
            {
                return GetText(8376, "Ångra erbjud pass misslyckades");
            }
        }

        public string SetShiftAsWantedFailed
        {
            get
            {
                return GetText(8345, "Ta/Önska pass misslyckades");
            }
        }
        public string UpdateShiftSetUndoWantedFailed
        {
            get
            {
                return GetText(8377, "Ångra ta/önska pass misslyckades");
            }
        }

        public string SaveAbsenceRequestFailed
        {
            get
            {
                return GetText(8346, "Spara ledighetsansökan misslyckades");
            }
        }

        public string SaveInterestRequestFailed
        {
            get
            {
                return GetText(8374, "Spara tillgänglighetsanmälan misslyckades");
            }
        }

        public string AbsenceRequestNotFoundMessage
        {
            get
            {
                return GetText(8347, "Ledighetsansökan kunde inte hittas");
            }
        }

        public string InterestRequestNotFoundMessage
        {
            get
            {
                return GetText(8373, "Tillgänglighetsanmälan kunde inte hittas");
            }
        }

        public string DeleteAbsenceRequestFailed
        {
            get
            {
                return GetText(8348, "Ta bort ansökan misslykades");
            }
        }

        public string DeleteInterestRequestFailed
        {
            get
            {
                return GetText(8375, "Ta bort tillgänglighetsanmälan misslykades");
            }
        }

        public string UserNotFound
        {
            get
            {
                return GetText(8349, "Användare kunde inte hittas");
            }
        }
        public string CompanyNotFound
        {
            get
            {
                return GetText(8162, "Företag kunde inte hittas");
            }
        }
        public string MarkMailAsReadFailed
        {
            get
            {
                return GetText(8351, "Gick inte att registrera meddelandet som läst");
            }
        }

        public string MessageNotFound
        {
            get
            {
                return GetText(8352, "Meddelandet kunde inte hittas");
            }
        }

        public string DeleteIncomingMailFailed
        {
            get
            {
                return GetText(8353, "Meddelandet kunde inte tas bort");
            }
        }

        public string ReceiverNotFound
        {
            get
            {
                return GetText(8354, "Mottagare kunde inte hittas");
            }
        }

        public string SendAnswerFailed
        {
            get
            {
                return GetText(8355, "Gick inte att spara svaret");
            }
        }

        public string SendMailFailed
        {
            get
            {
                return GetText(8356, "Gick inte att skicka meddelandet");
            }
        }

        public string FileNotFound
        {
            get
            {
                return GetText(8357, "Filen kunde inte hittas");
            }
        }



        public string SaveEmployeeFailed
        {
            get
            {
                return GetText(5043, "Anställd kunde inte uppdateras");
            }
        }

        public string WorkingDescriptionNotSavedMessage
        {
            get
            {
                return GetText(8443, "Arbetsbeskrivning kunde inte sparas");
            }
        }

        public string InternalErrorMessage
        {
            get
            {
                return GetText(8455, "Internt fel") + " : ";
            }
        }
    }
}
