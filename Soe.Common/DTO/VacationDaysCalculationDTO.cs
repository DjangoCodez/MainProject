namespace SoftOne.Soe.Common.DTO
{
    public class VacationDaysCalculationDTO
    {
        /// <summary>Betalda dagar</summary>
        public decimal PeriodUsedDaysPaidCount { get; set; }
        /// <summary>Obetalda dagar</summary>
        public decimal PeriodUsedDaysUnpaidCount { get; set; }
        /// <summary>Förskott</summary>
        public decimal PeriodUsedDaysAdvanceCount { get; set; }
        /// <summary>Sparade år 1</summary>
        public decimal PeriodUsedDaysYear1Count { get; set; }
        /// <summary>Sparade år 2</summary>
        public decimal PeriodUsedDaysYear2Count { get; set; }
        /// <summary>Sparade år 3</summary>
        public decimal PeriodUsedDaysYear3Count { get; set; }
        /// <summary>Sparade år 4</summary>
        public decimal PeriodUsedDaysYear4Count { get; set; }
        /// <summary>Sparade år 5</summary>
        public decimal PeriodUsedDaysYear5Count { get; set; }
        /// <summary>Förfallna dagar</summary>
        public decimal PeriodUsedDaysOverdueCount { get; set; }

        /// <summary>Betalda dagar</summary>
        public decimal PeriodVacationCompensationPaidCount { get; set; }
        /// <summary>Slutlön - Sparade år 1</summary>
        public decimal PeriodVacationCompensationSavedYear1Count { get; set; }
        /// <summary>Slutlön - Sparade år 2</summary>
        public decimal PeriodVacationCompensationSavedYear2Count { get; set; }
        /// <summary>Slutlön - Sparade år 3</summary>
        public decimal PeriodVacationCompensationSavedYear3Count { get; set; }
        /// <summary>Slutlön - Sparade år 4</summary>
        public decimal PeriodVacationCompensationSavedYear4Count { get; set; }
        /// <summary>Slutlön - Sparade år 5</summary>
        public decimal PeriodVacationCompensationSavedYear5Count { get; set; }
        /// <summary>Slutlön - Förfallna dagar</summary>
        public decimal PeriodVacationCompensationSavedOverdueCount { get; set; }

        /// <summary>Sparade år 1 + Slutlön - Sparade år 1</summary>
        public decimal PeriodUsedAndCompensationDaysYear1Count => PeriodUsedDaysYear1Count + PeriodVacationCompensationSavedYear1Count;
        /// <summary>Sparade år 2 + Slutlön - Sparade år 2</summary>
        public decimal PeriodUsedAndCompensationDaysYear2Count => PeriodUsedDaysYear2Count + PeriodVacationCompensationSavedYear2Count;
        /// <summary>Sparade år 3 + Slutlön - Sparade år 3</summary>
        public decimal PeriodUsedAndCompensationDaysYear3Count => PeriodUsedDaysYear3Count + PeriodVacationCompensationSavedYear3Count;
        /// <summary>Sparade år 4 + Slutlön - Sparade år 4</summary>
        public decimal PeriodUsedAndCompensationDaysYear4Count => PeriodUsedDaysYear4Count + PeriodVacationCompensationSavedYear4Count;
        /// <summary>Sparade år 5 + Slutlön - Sparade år 5</summary>
        public decimal PeriodUsedAndCompensationDaysYear5Count => PeriodUsedDaysYear5Count + PeriodVacationCompensationSavedYear5Count;
        /// <summary>Förfallna dagar + Slutlön - Förfallna dagar</summary>
        public decimal PeriodUsedAndCompensationDaysOverdueCount => PeriodUsedDaysOverdueCount + PeriodVacationCompensationSavedOverdueCount;

        /// <summary>Semestertillägg</summary>
        public decimal PeriodVacationAddition { get; set; }
        /// <summary>Rörligt semestertillägg</summary>
        public decimal PeriodVariableVacationAddition { get; set; }
        /// <summary>Förutbetald tillägg - Utbetald</summary>
        public decimal PeriodVacationPrepaymentPaid { get; set; }
        /// <summary>Förutbetald tillägg - Motbokning</summary>
        public decimal PeriodVacationPrepaymentInvert { get; set; }
        /// <summary>Förutbetald rörligt tillägg - Utbetald</summary>
        public decimal PeriodVariablePrepaymentPaid { get; set; }
        /// <summary>Förutbetald rörligt tillägg - Motbokning</summary>
        public decimal PeriodVariablePrepaymentInvert { get; set; }

        /// <summary>Summa förskott</summary>
        public decimal PeriodDebtAdvanceAmount { get; set; }

    }
}
