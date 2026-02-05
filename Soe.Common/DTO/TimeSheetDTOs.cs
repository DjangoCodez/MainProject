using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class TimeSheetDTO
    {
        #region Properties

        public int TimeSheetWeekId { get; set; }
        public int? ProjectInvoiceWeekId { get; set; }

        public int RowNr { get; set; }

        #region Project

        public int ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }

        #endregion

        #region Customer

        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        #endregion

        #region Order/Invoice

        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }

        #endregion

        #region TimeCode

        public int TimeCodeId { get; set; }
        public string TimeCodeName { get; set; }

        #endregion

        #region Attest

        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }

        #endregion

        #region Days

        public DateTime WeekStartDate { get; set; }

        public TimeSpan Monday { get; set; }

        public TimeSpan MondayActual { get; set; }

        public TimeSpan MondayOther { get; set; }

        public TimeSpan Tuesday { get; set; }

        public TimeSpan TuesdayActual { get; set; }

        public TimeSpan TuesdayOther { get; set; }

        public TimeSpan Wednesday { get; set; }

        public TimeSpan WednesdayActual { get; set; }

        public TimeSpan WednesdayOther { get; set; }

        public TimeSpan Thursday { get; set; }

        public TimeSpan ThursdayActual { get; set; }

        public TimeSpan ThursdayOther { get; set; }

        public TimeSpan Friday { get; set; }

        public TimeSpan FridayActual { get; set; }

        public TimeSpan FridayOther { get; set; }

        public TimeSpan Saturday { get; set; }

        public TimeSpan SaturdayActual { get; set; }

        public TimeSpan SaturdayOther { get; set; }

        public TimeSpan Sunday { get; set; }

        public TimeSpan SundayActual { get; set; }

        public TimeSpan SundayOther { get; set; }

        public TimeSpan WeekSum { get; set; }

        public TimeSpan WeekSumActual { get; set; }

        public TimeSpan WeekSumOther { get; set; }

        #endregion

        #region Notes

        public string MondayNote { get; set; }
        public string MondayNoteExternal { get; set; }
        public bool HasMondayNote { get { return !String.IsNullOrEmpty(MondayNote) || !String.IsNullOrEmpty(MondayNoteExternal); } }

        public string TuesdayNote { get; set; }
        public string TuesdayNoteExternal { get; set; }
        public bool HasTuesdayNote { get { return !String.IsNullOrEmpty(TuesdayNote) || !String.IsNullOrEmpty(TuesdayNoteExternal); } }

        public string WednesdayNote { get; set; }
        public string WednesdayNoteExternal { get; set; }
        public bool HasWednesdayNote { get { return !String.IsNullOrEmpty(WednesdayNote) || !String.IsNullOrEmpty(WednesdayNoteExternal); } }

        public string ThursdayNote { get; set; }
        public string ThursdayNoteExternal { get; set; }
        public bool HasThursdayNote { get { return !String.IsNullOrEmpty(ThursdayNote) || !String.IsNullOrEmpty(ThursdayNoteExternal); } }

        public string FridayNote { get; set; }
        public string FridayNoteExternal { get; set; }
        public bool HasFridayNote { get { return !String.IsNullOrEmpty(FridayNote) || !String.IsNullOrEmpty(FridayNoteExternal); } }

        public string SaturdayNote { get; set; }
        public string SaturdayNoteExternal { get; set; }
        public bool HasSaturdayNote { get { return !String.IsNullOrEmpty(SaturdayNote) || !String.IsNullOrEmpty(SaturdayNoteExternal); } }

        public string SundayNote { get; set; }
        public string SundayNoteExternal { get; set; }
        public bool HasSundayNote { get { return !String.IsNullOrEmpty(SundayNote) || !String.IsNullOrEmpty(SundayNoteExternal); } }

        public string WeekNote { get; set; }
        public string WeekNoteExternal { get; set; }
        public bool HasWeekNote { get { return !String.IsNullOrEmpty(WeekNote) || !String.IsNullOrEmpty(WeekNoteExternal); } }

        #endregion

        #region Flags

        public bool IsDeleted { get; set; }
        public bool IsReadOnly { get; set; }
        
        #endregion
        #endregion
    }

    public class TimeSheetRowDTO
    {
        #region Properties

        public int TimeSheetWeekId { get; set; }
        public int ProjectInvoiceWeekId { get; set; }
        public int RowNr { get; set; }

        #region Project

        public int ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }
        public string ProjectNumberName { get; set; }
        public TermGroup_ProjectAllocationType AllocationType { get; set; }

        #endregion

        #region Customer

        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        #endregion

        #region Order/Invoice

        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public OrderInvoiceRegistrationType RegistrationType { get; set; }

        #endregion

        #region TimeCode

        public int TimeCodeId { get; set; }
        public string TimeCodeName { get; set; }

        #endregion

        #region Attest

        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }

        #endregion

        #region Days

        public DateTime WeekStartDate { get; set; }
        public int MondayQuantity { get; set; }
        public int MondayInvoiceQuantity { get; set; }
        public int TuesdayQuantity { get; set; }
        public int TuesdayInvoiceQuantity { get; set; }
        public int WednesdayQuantity { get; set; }
        public int WednesdayInvoiceQuantity { get; set; }
        public int ThursdayQuantity { get; set; }
        public int ThursdayInvoiceQuantity { get; set; }
        public int FridayQuantity { get; set; }
        public int FridayInvoiceQuantity { get; set; }
        public int SaturdayQuantity { get; set; }
        public int SaturdayInvoiceQuantity { get; set; }
        public int SundayQuantity { get; set; }
        public int SundayInvoiceQuantity { get; set; }
        public int WeekSumQuantity { get; set; }
        public int WeekSumInvoiceQuantity { get; set; }

        #endregion

        #region Notes

        public string MondayNoteInternal { get; set; }
        public string MondayNoteExternal { get; set; }
        public string TuesdayNoteInternal { get; set; }
        public string TuesdayNoteExternal { get; set; }
        public string WednesdayNoteInternal { get; set; }
        public string WednesdayNoteExternal { get; set; }
        public string ThursdayNoteInternal { get; set; }
        public string ThursdayNoteExternal { get; set; }
        public string FridayNoteInternal { get; set; }
        public string FridayNoteExternal { get; set; }
        public string SaturdayNoteInternal { get; set; }
        public string SaturdayNoteExternal { get; set; }
        public string SundayNoteInternal { get; set; }
        public string SundayNoteExternal { get; set; }
        public string WeekNoteInternal { get; set; }
        public string WeekNoteExternal { get; set; }

        #endregion

        #endregion
    }

    public class TimeSheetScheduleDTO
    {
        #region Days

        public DateTime WeekStartDate { get; set; }

        public TimeSpan Monday { get; set; }
        public TimeSpan Tuesday { get; set; }
        public TimeSpan Wednesday { get; set; }
        public TimeSpan Thursday { get; set; }
        public TimeSpan Friday { get; set; }
        public TimeSpan Saturday { get; set; }
        public TimeSpan Sunday { get; set; }
        public TimeSpan WeekSum { get; set; }

        #endregion
    }
}
