using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class DistributionSelectionVoucher : ControlBase
    {
        #region Variables

        public NameValueCollection F { get; set; }
        public ReportSelection ReportSelection { get; set; }

        private ReportManager rm;
        private VoucherManager vm;

        #endregion

        public void Populate(bool repopulate, int accountYearIdFrom, int accountYearIdTo)
        {
            #region Init

            rm = new ReportManager(PageBase.ParameterObject);
            vm = new VoucherManager(PageBase.ParameterObject);

            #endregion

            #region Populate

            Dictionary<int, string> voucherSeries = new Dictionary<int, string>();
            if (accountYearIdFrom > 0 && accountYearIdTo > 0)
                voucherSeries = vm.GetVoucherSeriesByYearDict(accountYearIdFrom, accountYearIdTo, PageBase.SoeCompany.ActorCompanyId, false, true);
            VoucherSeries.DataSourceFrom = voucherSeries;
            VoucherSeries.DataSourceTo = voucherSeries;

            #endregion

            #region Set data

            if (repopulate && SoeForm != null && SoeForm.PreviousForm != null)
            {
                VoucherSeries.PreviousForm = SoeForm.PreviousForm;
                VoucherNr.PreviousForm = SoeForm.PreviousForm;
            }
            else
            {
                if (ReportSelection != null)
                {
                    #region ReportSelection

                    bool foundVoucherSeriesTypeNr = false;
                    bool foundVoucherNr = false;
                    IEnumerable<ReportSelectionInt> reportSelectionInts = rm.GetReportSelectionInts(ReportSelection.ReportSelectionId);
                    foreach (ReportSelectionInt reportSelectionInt in reportSelectionInts)
                    {
                        switch (reportSelectionInt.ReportSelectionType)
                        {
                            case (int)SoeSelectionData.Int_Voucher_VoucherSeriesId:
                                VoucherSeries.ValueFrom = reportSelectionInt.SelectFrom.ToString();
                                VoucherSeries.ValueTo = reportSelectionInt.SelectTo.ToString();
                                foundVoucherSeriesTypeNr = true;
                                break;
                            case (int)SoeSelectionData.Int_Voucher_VoucherNr:
                                VoucherNr.ValueFrom = reportSelectionInt.SelectFrom.ToString();
                                VoucherNr.ValueTo = reportSelectionInt.SelectTo.ToString();
                                foundVoucherNr = true;
                                break;
                        }

                        if (foundVoucherSeriesTypeNr && foundVoucherNr)
                            break;
                    }

                    #endregion
                }
            }

            #endregion
        }

        public bool Evaluate(SelectionVoucher s, EvaluatedSelection es)
        {
            if (s == null || es == null)
                return false;

            #region Init

            if (F == null)
                return false;

            if (rm == null)
                rm = new ReportManager(PageBase.ParameterObject);
            if (vm == null)
                vm = new VoucherManager(PageBase.ParameterObject);

            #endregion

            #region Validate input and read interval into SelectionVoucher

            #region Read from Form

            string voucherSeriesNrFrom = F["VoucherSeries-from-1"];
            string voucherSeriesNrTo = F["VoucherSeries-to-1"];
            string voucherNrFrom = F["VoucherNr-from-1"];
            string voucherNrTo = F["VoucherNr-to-1"];

            #endregion

            #region Validate interval

            //Validate VoucherSeries and VoucherNr
            if (!Validator.ValidateSelectInterval(voucherSeriesNrFrom, voucherSeriesNrTo) || !Validator.ValidateTextInterval(voucherNrFrom, voucherNrTo))
            {
                SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1449, "Ange både Från och Till på intervall, eller utelämna båda");
                return false;
            }

            #endregion
            
            #region VoucherSeries

            int voucherSeriesNr = 0;

            //From
            if (Int32.TryParse(voucherSeriesNrFrom, out voucherSeriesNr) && voucherSeriesNr > 0)
                s.VoucherSeriesTypeNrFrom = voucherSeriesNr;

            //To
            if (Int32.TryParse(voucherSeriesNrTo, out voucherSeriesNr) && voucherSeriesNr > 0)
                s.VoucherSeriesTypeNrTo = voucherSeriesNr;

            //Validate
            if (!Validator.ValidateNumericInterval(s.VoucherSeriesTypeNrFrom, s.VoucherSeriesTypeNrTo))
            {
                SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1448, "Till får inte vara mindre än Från i intervall");
                return false;
            }

            #endregion

            #region VoucherNr

            int voucherNr = 0;

            //From
            if (Int32.TryParse(voucherNrFrom, out voucherNr))
                s.VoucherNrFrom = voucherNr;

            //To
            if (Int32.TryParse(voucherNrTo, out voucherNr))
                s.VoucherNrTo = voucherNr;

            //Validate
            if (!Validator.ValidateNumericInterval(s.VoucherNrFrom, s.VoucherNrTo))
            {
                SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1448, "Till får inte vara mindre än Från i intervall");
                return false;
            }

            #endregion

            #endregion

            #region Set EvaluatedSelection from SelectionVoucher

            SetEvaluated(s, es);

            #endregion

            return true;
        }

        public void SetEvaluated(SelectionVoucher s, EvaluatedSelection es)
        {
            if (s == null || es == null)
                return;

            if (s.VoucherSeriesTypeNrFrom.HasValue || s.VoucherSeriesTypeNrTo.HasValue)
            {
                es.SV_HasVoucherSeriesTypeNrInterval = true;
                es.SV_VoucherSeriesTypeNrFrom = s.VoucherSeriesTypeNrFrom.Value;
                es.SV_VoucherSeriesTypeNrTo = s.VoucherSeriesTypeNrTo.Value;
            }

            if (s.VoucherNrFrom.HasValue && s.VoucherNrTo.HasValue)
            {
                es.SV_HasVoucherNrInterval = true;
                es.SV_VoucherNrFrom = s.VoucherNrFrom.Value;
                es.SV_VoucherNrTo = s.VoucherNrTo.Value;
            }

            //Set as evaluated
            es.SV_IsEvaluated = true;
        }
    }
}