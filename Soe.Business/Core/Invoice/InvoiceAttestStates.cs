using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class InvoiceAttestStates
    {
        #region Offer

        public readonly int TransferredOfferToOrderId;
        public readonly int TransferredOfferToInvoiceId;
        public readonly int OfferInitialId;

        private readonly List<int> offerClosedIds = new List<int>();
        public IReadOnlyList<int> OfferClosedIds => offerClosedIds;

        private readonly List<int> offerHiddenIds = new List<int>();
        public IReadOnlyList<int> OfferHiddenIds => offerHiddenIds;

        #endregion

        #region Order

        public readonly int TransferredOrderToInvoiceId;
        public readonly int DeliverOrderToStockId;
        public readonly int OrderInitialId;

        private readonly List<int> orderClosedIds = new List<int>();
        public IReadOnlyList<int> OrderClosedIds => orderClosedIds;

        private readonly List<int> orderLockedIds = new List<int>();
        public IReadOnlyList<int> OrderLockedIds => orderLockedIds;

        private readonly List<int> orderHiddenIds = new List<int>();
        public IReadOnlyList<int> OrderHiddenIds => orderHiddenIds;

        #endregion

        private readonly AttestManager attestManager;
        private readonly SettingManager settingManager;
        private readonly List<AttestTransition> attestTransitions;
        
        public InvoiceAttestStates(CompEntities entities, SoeOriginType type, SettingManager settingManager, AttestManager attestManager, int actorCompanyId, int userId, bool loadTransitions)
        {
            this.attestManager = attestManager;
            this.settingManager = settingManager;

            if (type == SoeOriginType.Order || type == SoeOriginType.None)
            {
                TransferredOrderToInvoiceId = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, actorCompanyId, 0);
                DeliverOrderToStockId = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStatusOrderDeliverFromStock, 0, actorCompanyId, 0);

                var orderStates = attestManager.GetAttestStateDTOs(entities, actorCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing, false);
                this.orderClosedIds = orderStates.Where(x=> x.Closed).Select(y => y.AttestStateId).ToList();
                this.orderLockedIds = orderStates.Where(x => x.Locked).Select(y => y.AttestStateId).ToList(); //attestManager.GetLockedAttestStatesIds(entities, actorCompanyId, TermGroup_AttestEntity.Order);
                this.orderHiddenIds = orderStates.Where(x => x.Hidden).Select(y => y.AttestStateId).ToList(); //attestManager.GetHiddenAttestStates(entities, actorCompanyId, TermGroup_AttestEntity.Order).Select(a => a.AttestStateId).ToList();
                OrderInitialId = orderStates.Where(x => x.Initial).Select(y => y.AttestStateId).FirstOrDefault();//attestManager.GetInitialAttestStateId(entities, actorCompanyId, TermGroup_AttestEntity.Order);

                if (loadTransitions)
                {
                    this.attestTransitions = attestManager.GetAttestTransitionsForAttestRoleUser(entities, userId, actorCompanyId, entity: TermGroup_AttestEntity.Order);
                }
            }

            if (type == SoeOriginType.Offer || type == SoeOriginType.None)
            {
                var offerStates = attestManager.GetAttestStateDTOs(entities,actorCompanyId, TermGroup_AttestEntity.Offer,SoeModule.Billing,false);
                TransferredOfferToOrderId = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOfferToOrder, 0, actorCompanyId, 0);
                TransferredOfferToInvoiceId = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOfferToInvoice, 0, actorCompanyId, 0);
                this.offerClosedIds = offerStates.Where(x => x.Closed).Select(y => y.AttestStateId).ToList(); //attestManager.GetClosedAttestStatesIds(entities, actorCompanyId, TermGroup_AttestEntity.Offer);
                this.offerHiddenIds = offerStates.Where(x => x.Hidden).Select(y => y.AttestStateId).ToList();
                OfferInitialId = offerStates.Where(x => x.Initial).Select(y => y.AttestStateId).FirstOrDefault(); //attestManager.GetInitialAttestStateId(entities, actorCompanyId, TermGroup_AttestEntity.Offer);
            }
        }


        public AttestStateDTO GetInitialAttestStateOrder(CompEntities entities, int actorCompanyId)
        {
            return attestManager.GetInitialAttestState(entities, actorCompanyId, TermGroup_AttestEntity.Order).ToDTO();
        }

        public AttestStateDTO GetInitialAttestStateOffer(CompEntities entities, int actorCompanyId)
        {
            return attestManager.GetInitialAttestState(entities, actorCompanyId, TermGroup_AttestEntity.Offer).ToDTO();
        }

        public AttestStateDTO GetInitialAttestState(CompEntities entities, int actorCompanyId, SoeOriginType type)
        {
            if (type == SoeOriginType.Offer)
                return GetInitialAttestStateOffer(entities, actorCompanyId);
            else
                return GetInitialAttestStateOrder(entities, actorCompanyId);
        }

        public IReadOnlyList<int> GetClosedAndHiddenIds(SoeOriginType type)
        {
            if (type == SoeOriginType.Offer)
                return this.offerHiddenIds.Concat(this.offerClosedIds).ToList();
            else
                return this.orderHiddenIds.Concat(this.orderClosedIds).ToList();
        }

        public bool IsAttestStateReadonly(int? attestStateId)
        {
            if (attestStateId == null)
                return false;
            else
                return (TransferredOrderToInvoiceId == attestStateId.Value || this.orderLockedIds.Contains(attestStateId.Value));
        }

        public bool IsAttestStateTransitionValid(int? attestStateFromId, int? attestStateToId)
        {
            if (!attestStateFromId.HasValue || attestTransitions == null || attestTransitions.Count == 0 || !attestStateToId.HasValue)
                return false;

            var attestTransition = (from at in attestTransitions
                                    where at.AttestStateFrom != null &&
                                    at.AttestStateTo != null &&
                                    at.AttestStateFrom.AttestStateId == attestStateFromId &&
                                    at.AttestStateTo.AttestStateId == attestStateToId
                                    select at).FirstOrDefault();

            return attestTransition != null;
        }

        public ActionResult ValidateAttestStateChange(CompEntities entities, CustomerInvoiceRow row, int newAttestStateId)
        {
            var result = new ActionResult();
            var usePartialInvoicingOnOrderRow = settingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingUsePartialInvoicingOnOrderRow,0, settingManager.ActorCompanyId, 0); 

            if (this.orderLockedIds.Contains(newAttestStateId) && row.IsStockRow.GetValueOrDefault() && row.InvoiceQuantity == 0 && usePartialInvoicingOnOrderRow)
            {
                return new ActionResult(7574, settingManager.GetText(7574,1, "Lagerartiklar kan endast klarmarkeras om levererat antal inte är 0)") );
            }

            if (!IsAttestStateTransitionValid(row.AttestStateId, newAttestStateId))
            {
                return new ActionResult(10076, settingManager.GetText(10076, 1, "Ogiltig statusövergång"));
            }

            return result;
        }

        public void DetermineTransferredState(CustomerInvoice customerInvoice, CustomerInvoiceRow customerInvoiceRow, ref bool isRowTransferred, ref bool isRowClosed, ref SoeOriginType transferredTo)
        {
            switch (customerInvoice.Origin.Type)
            {
                case (int)SoeOriginType.Offer:
                    #region Offer

                    if (customerInvoiceRow.AttestStateId.HasValue)
                    {
                        if (customerInvoiceRow.AttestStateId.Value == TransferredOfferToOrderId)
                        {
                            isRowTransferred = true;
                            if (transferredTo == SoeOriginType.None)
                                transferredTo = SoeOriginType.Order;
                        }
                        else if (customerInvoiceRow.AttestStateId.Value == TransferredOfferToInvoiceId)
                        {
                            isRowTransferred = true;
                            if (transferredTo == SoeOriginType.None)
                                transferredTo = SoeOriginType.CustomerInvoice;
                        }
                        else if (this.offerClosedIds.Contains(customerInvoiceRow.AttestStateId.Value))
                        {
                            isRowClosed = true;
                        }
                    }

                    #endregion
                    break;
                case (int)SoeOriginType.Order:
                    #region Order

                    if (customerInvoiceRow.AttestStateId.HasValue)
                    {
                        if (customerInvoiceRow.AttestStateId.Value == TransferredOrderToInvoiceId)
                        {
                            isRowTransferred = true;
                            if (transferredTo == SoeOriginType.None)
                                transferredTo = SoeOriginType.CustomerInvoice;
                        }
                        else if (this.orderClosedIds.Contains(customerInvoiceRow.AttestStateId.Value))
                        {
                            isRowClosed = true;
                        }
                    }

                    #endregion
                    break;
            }
        }

        public ActionResult SetRowReady(CompEntities entities, CustomerInvoiceRow row)
        {
            var defaultStatusTransferredOrderReadyMobile = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStatusOrderReadyMobile, 0, settingManager.ActorCompanyId, 0);
            if (defaultStatusTransferredOrderReadyMobile == 0)
            {
                return new ActionResult(7057, settingManager.GetText(7057, 1, "Inga övergångar är inställda"));
            }
            var result = ValidateAttestStateChange(entities, row, defaultStatusTransferredOrderReadyMobile);
            if (result.Success)
            {
                row.AttestStateId = defaultStatusTransferredOrderReadyMobile;
            }
            return result;
        }

        public static int GetInitialAttestStateId(AttestManager attestManager, CompEntities entities, int actorCompanyId, SoeOriginType originType)
        {
            switch (originType)
                            {
                case SoeOriginType.Offer:
                    return attestManager.GetInitialAttestStateId(entities, actorCompanyId, TermGroup_AttestEntity.Offer);
                case SoeOriginType.Order:
                    return attestManager.GetInitialAttestStateId(entities, actorCompanyId, TermGroup_AttestEntity.Order);
                default:
                    return 0;
            }
        }
    }
}
