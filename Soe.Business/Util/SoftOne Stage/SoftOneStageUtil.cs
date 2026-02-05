using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.SoftOne_Stage
{
    public class SoftOneStageUtil
    {
        public StageSyncDTO CreatestageSyncDTO(int actorCompanyId)
        {
            StageSyncDTO stageSyncDTO = new StageSyncDTO();
            stageSyncDTO.StageSyncItemDTOs = new List<StageSyncItemDTO>();
            SettingManager sm = new SettingManager(null);
            var settingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.PayrollEmploymentTypes_SE, actorCompanyId);
            bool stageSync = sm.GetBoolSettingFromDict(settingsDict, (int)CompanySettingType.StageSync);

            bool orderSync = sm.GetBoolSettingFromDict(settingsDict, (int)CompanySettingType.OrderSync, true);
            bool customerInvoiceSync = sm.GetBoolSettingFromDict(settingsDict, (int)CompanySettingType.CustomerInvoiceSync, true);
            bool offerSync = sm.GetBoolSettingFromDict(settingsDict, (int)CompanySettingType.OfferSync, true);
            bool contractSync = sm.GetBoolSettingFromDict(settingsDict, (int)CompanySettingType.ContractSync, true);
            bool supplierInvoiceSync = sm.GetBoolSettingFromDict(settingsDict, (int)CompanySettingType.SupplierInvoiceSync, true);
            bool timeCodeTransactionSync = sm.GetBoolSettingFromDict(settingsDict, (int)CompanySettingType.TimeCodeTransactionSync, true);
            bool timePayrollTransactionSync = sm.GetBoolSettingFromDict(settingsDict, (int)CompanySettingType.TimePayrollTransactionSync, true);
            bool voucherSync = sm.GetBoolSettingFromDict(settingsDict, (int)CompanySettingType.Vouchers, true);

            if (orderSync)
                stageSyncDTO.StageSyncItemDTOs.Add(new StageSyncItemDTO() { StageSyncItemType = StageSyncItemType.Orders });

            if (customerInvoiceSync)
                stageSyncDTO.StageSyncItemDTOs.Add(new StageSyncItemDTO() { StageSyncItemType = StageSyncItemType.Customerinvoices });

            if (offerSync)
                stageSyncDTO.StageSyncItemDTOs.Add(new StageSyncItemDTO() { StageSyncItemType = StageSyncItemType.Offers });

            if (contractSync)
                stageSyncDTO.StageSyncItemDTOs.Add(new StageSyncItemDTO() { StageSyncItemType = StageSyncItemType.Contracts });

            if (supplierInvoiceSync)
                stageSyncDTO.StageSyncItemDTOs.Add(new StageSyncItemDTO() { StageSyncItemType = StageSyncItemType.SupplierInvoices });

            if (timeCodeTransactionSync)
                stageSyncDTO.StageSyncItemDTOs.Add(new StageSyncItemDTO() { StageSyncItemType = StageSyncItemType.TimeCodeTransactions });

            if (timePayrollTransactionSync)
                stageSyncDTO.StageSyncItemDTOs.Add(new StageSyncItemDTO() { StageSyncItemType = StageSyncItemType.TimePayrollTransactions });

            if (voucherSync)
                stageSyncDTO.StageSyncItemDTOs.Add(new StageSyncItemDTO() { StageSyncItemType = StageSyncItemType.Vouchers });

            return stageSyncDTO;

        }

        public ActionResult Sync(StageSyncDTO stageSyncDTO)
        {
            ActionResult result = new ActionResult();

            try
            {
                var client = new GoRestClient("http://softonestage.azurewebsites.net/");
                var request = new RestRequest("trigger/sync", Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(stageSyncDTO);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(request);

                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.ToString();
            }

            return result;
        }
    }
}
