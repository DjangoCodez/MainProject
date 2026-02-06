using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Evo.Cache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Web.Script.Serialization;

namespace SoftOne.Soe.Business.Core
{
    public class SettingManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConcurrentDictionary<string, UserCompanySettingObject> companySettingCache = new ConcurrentDictionary<string, UserCompanySettingObject>();

        // Max number of concurrently open account years
        private static ReadOnlyCollection<int> maxYearsOpen;
        public static ReadOnlyCollection<int> MaxYearsOpen
        {
            get
            {
                if (maxYearsOpen == null)
                    maxYearsOpen = new ReadOnlyCollection<int>(new List<int>() { 1, 2, 3 });
                return maxYearsOpen;
            }
        }

        // Max number of concurrently open periods
        private static ReadOnlyCollection<int> maxPeriodsOpen;
        public static ReadOnlyCollection<int> MaxPeriodsOpen
        {
            get
            {
                if (maxPeriodsOpen == null)
                    maxPeriodsOpen = new ReadOnlyCollection<int>(new List<int>() { 3, 6, 12, 18, 24, 99 });
                return maxPeriodsOpen;
            }
        }

        // Show ended employees in attest/payroll after nr of months
        private static ReadOnlyCollection<int> norOfMonthsToShowEndedEmployees;
        public static ReadOnlyCollection<int> NorOfMonthsToShowEndedEmployees
        {
            get
            {
                if (norOfMonthsToShowEndedEmployees == null)
                    norOfMonthsToShowEndedEmployees = new ReadOnlyCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
                return norOfMonthsToShowEndedEmployees;
            }
        }

        // Max number of breaks in for each time block
        private static ReadOnlyCollection<int> maxNoOfBreaks;
        public static ReadOnlyCollection<int> MaxNoOfBrakes
        {
            get
            {
                if (maxNoOfBreaks == null)
                    maxNoOfBreaks = new ReadOnlyCollection<int>(new List<int>() { 1, 2, 3, 4 });
                return maxNoOfBreaks;
            }
        }

        #endregion

        #region Ctor

        public SettingManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Check settings

        public IEnumerable<CheckSettingsResult> GetCheckSettingAreas(int actorCompanyId)
        {
            List<CheckSettingsResult> areas = new List<CheckSettingsResult>();

            foreach (int area in Enum.GetValues(typeof(TermGroup_CheckSettingsArea)))
            {
                // TODO: Remove this when all are supported
                if (area == (int)TermGroup_CheckSettingsArea.HouseholdTaxDeduction || area == (int)TermGroup_CheckSettingsArea.ProjectInvoice || area == (int)TermGroup_CheckSettingsArea.Time || area == (int)TermGroup_CheckSettingsArea.Edi || area == (int)TermGroup_CheckSettingsArea.Offer || area == (int)TermGroup_CheckSettingsArea.Order)
                    areas.Add(new CheckSettingsResult((TermGroup_CheckSettingsArea)area, GetText(area, (int)TermGroup.CheckSettingsArea)));
            }

            return areas;
        }

        public List<CheckSettingsResult> CheckSettings(List<int> areas)
        {
            List<CheckSettingsResult> result = new List<CheckSettingsResult>();

            foreach (var area in areas)
            {
                result.AddRange(CheckSettings((TermGroup_CheckSettingsArea)area, base.ActorCompanyId));
            }

            return result;
        }

        public bool SettingIsTrue(SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId, int cacheSeconds = 60)
        {
            string key = $"SettingIsTrue#{settingMainType}#{settingTypeId}#{userId}#{actorCompanyId}#{licenseId}";

            var setting = BusinessMemoryCache<bool?>.Get(key);

            if (!setting.HasValue)
            {
                setting = SettingManager.GetBoolSetting(settingMainType, settingTypeId, userId, actorCompanyId, licenseId);
                BusinessMemoryCache<bool?>.Set(key, setting, cacheSeconds);
            }

            return setting.GetValueOrDefault();
        }

        public IEnumerable<CheckSettingsResult> CheckSettings(TermGroup_CheckSettingsArea area, int actorCompanyId)
        {
            #region Init

            // Create result
            // One item is created for each check on specified area
            List<CheckSettingsResult> result = new List<CheckSettingsResult>();
            CheckSettingsResult item;

            UserCompanySetting setting;

            //const string EXISTS = "Finns";
            //const string MISSING = "Saknas";
            //const string INVALID = "Felaktig";
            string EXISTS = GetText(8084, "Finns");
            string MISSING = GetText(8085, "Saknas");
            string INVALID = GetText(8086, "Felaktig");

            int sort = 1;

            #endregion

            #region Prereq

            // Get all roles for specified company
            IEnumerable<Role> roles = RoleManager.GetRolesByCompany(actorCompanyId);
            bool hasPermission = false;
            bool hasRole = false;
            bool roleHasUser = false;
            string missingRole = "";
            string missingUser = "";

            #endregion

            string areaName = GetText((int)area, (int)TermGroup.CheckSettingsArea);
            AttestState initialStatePayroll = null;
            AccountStd accountCost = null;
            int accountCostId;
            TimeCode timeCode = null;
            int timeCodeId;
            AttestState attestState = null;
            int attestStateId;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            switch (area)
            {
                case TermGroup_CheckSettingsArea.SupplierInvoice:
                    #region SupplierInvoice

                    // Test
                    item = new CheckSettingsResult(area, areaName, sort++, "Test");
                    item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                    result.Add(item);
                    break;

                #endregion
                case TermGroup_CheckSettingsArea.CustomerLedger:
                    #region CustomerLedger

                    // Test
                    item = new CheckSettingsResult(area, areaName, sort++, "Test");
                    item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                    result.Add(item);
                    break;

                #endregion
                case TermGroup_CheckSettingsArea.Voucher:
                    #region Voucher

                    // Test
                    item = new CheckSettingsResult(area, areaName, sort++, "Test");
                    item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                    result.Add(item);
                    break;

                #endregion
                case TermGroup_CheckSettingsArea.Offer:
                    #region Offer
                    /*Initialnivå
                        Minst två nivåer
                        Övergångar (antal)
                        Attestroll med övergångar kopplade till sig innehållande Offertstatusövergångar
                        FTG-inst om vad det ska bli för status efter övergång, som inte är offertinitialnivå.
                        Användare som har statusroll kopplad till sig.
                        Att det finns initialnivå på order.
                        */

                    // Test

                    #region Attest state: Initial state for offer
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7052, "Attest - Startnivå för offert"));
                    AttestState initialStateOffer = AttestManager.GetInitialAttestState(actorCompanyId, TermGroup_AttestEntity.Offer);
                    if (initialStateOffer != null)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0} - {1}", initialStateOffer.Name, initialStateOffer.Description);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(7053, "Lägg till en attestnivå av typen '{0}' markerad som startnivå under Försäljning->Inställningar->Statusnivåer"), GetText((int)TermGroup_AttestEntity.Offer, (int)TermGroup.AttestEntity));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest: Minimum two states for order

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7054, "Attest - Minst två nivåer"));

                    List<AttestState> offerAttestStates = AttestManager.GetAttestStates(actorCompanyId, TermGroup_AttestEntity.Offer, SoeModule.Billing);
                    if (offerAttestStates.Count > 1)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(7055, "Minst två statusnivåer måste vara inställda på {0}"), GetText((int)TermGroup_AttestEntity.Offer, (int)TermGroup.AttestEntity));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest: Number of transitions

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7056, "Attest - Övergångar"));

                    IEnumerable<AttestTransition> offerAttestTransitions = AttestManager.GetAttestTransitions(TermGroup_AttestEntity.Offer, SoeModule.Billing, true, actorCompanyId);

                    if (offerAttestTransitions.Count() > 1)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS + " (" + offerAttestTransitions.Count().ToString() + " st)";
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(7057, "Inga övergångar är inställda");
                    }
                    result.Add(item);
                    #endregion

                    #region Företagsinställning - Status överfört till faktura

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7059, "Företagsinställning - Status överförd till order"));

                    setting = GetUserCompanySetting(entitiesReadOnly, SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOfferToOrder, 0, actorCompanyId, 0);
                    if (setting != null && setting.IntData != 0)
                    {
                        var offerAttestState = offerAttestStates.FirstOrDefault(r => r.AttestStateId == setting.IntData);

                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = offerAttestState != null ? offerAttestState.Name : String.Empty;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(7058, "Markera önskat värde på företagsinställningen under Försäljning->Inställningar->Inställningar Fakturering->Status"); //"Markera önskat värde på företagsinställningen '{0}' under.."
                    }
                    result.Add(item);

                    #endregion

                    #region Företagsinställning - Status överfört till faktura

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7060, "Företagsinställning - Status överförd till faktura"));
                    setting = GetUserCompanySetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOfferToInvoice, 0, actorCompanyId, 0);
                    if (setting != null && setting.IntData != 0)
                    {
                        var offerAttestState = offerAttestStates.FirstOrDefault(r => r.AttestStateId == setting.IntData);

                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = offerAttestState != null ? offerAttestState.Name : String.Empty;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(7058, "Markera önskat värde på företagsinställningen under Försäljning->Inställningar->Inställningar Försäljning->Status"); //"Markera önskat värde på företagsinställningen '{0}' under.."
                    }
                    result.Add(item);

                    #endregion

                    #region Attest: Attestroles exist for ALL transitions

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7061, "Attest - Roller för övergångar"));
                    hasRole = true;
                    roleHasUser = true;
                    missingRole = String.Empty;
                    missingUser = String.Empty;

                    if (offerAttestTransitions.Any())
                    {
                        foreach (AttestTransition transition in offerAttestTransitions)
                        {
                            if (transition.AttestRole.Count < 1)
                            {
                                hasRole = false;
                                missingRole += String.IsNullOrEmpty(missingRole) ? transition.Name : ", " + transition.Name;
                            }
                            else
                            {
                                foreach (AttestRole attestRole in transition.AttestRole)
                                {
                                    if (!attestRole.AttestRoleUser.IsLoaded)
                                        attestRole.AttestRoleUser.Load();
                                    if (attestRole.AttestRoleUser.Count < 1)
                                    {
                                        roleHasUser = false;
                                        missingUser += String.IsNullOrEmpty(missingUser) ? attestRole.Name : ", " + attestRole.Name;
                                    }
                                }
                            }
                        }
                    }

                    if (hasRole)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(7062, "Minst en av övergångarna har ingen attestroll knuten till sig") + " (" + missingRole + ")";
                    }
                    result.Add(item);

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7063, "Attest - Användare knuten till roller"));

                    if (roleHasUser)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(7065, "Minst en av rollerna har ingen användare knuten till sig") + " (" + missingUser + ")";
                    }
                    result.Add(item);
                    #endregion

                    #region Attest state: Initial state for order
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7064, "Attest - Startnivå för order"));
                    AttestState initialStateOfferOrder = AttestManager.GetInitialAttestState(actorCompanyId, TermGroup_AttestEntity.Order);
                    if (initialStateOfferOrder != null)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0} - {1}", initialStateOfferOrder.Name, initialStateOfferOrder.Description);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(7053, "Lägg till en attestnivå av typen '{0}' markerad som startnivå under Försäljning->Inställningar->Statusnivåer"), GetText((int)TermGroup_AttestEntity.Order, (int)TermGroup.AttestEntity));
                    }
                    result.Add(item);

                    #endregion

                    break;
                #endregion
                case TermGroup_CheckSettingsArea.Order:
                    #region Order

                    #region Attest state: Initial state for offer
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7064, "Attest - Startnivå för order"));
                    AttestState initialStateOrder = AttestManager.GetInitialAttestState(actorCompanyId, TermGroup_AttestEntity.Order);
                    if (initialStateOrder != null)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0} - {1}", initialStateOrder.Name, initialStateOrder.Description);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(7053, "Lägg till en attestnivå av typen '{0}' markerad som startnivå under Försäljning->Inställningar->Statusnivåer"), GetText((int)TermGroup_AttestEntity.Order, (int)TermGroup.AttestEntity));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest: Minimum two states for order

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7054, "Attest - Minst två nivåer"));

                    List<AttestState> orderAttestStates = AttestManager.GetAttestStates(actorCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing);
                    if (orderAttestStates.Count > 1)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(7055, "Minst två statusnivåer måste vara inställda på {0}"), GetText((int)TermGroup_AttestEntity.Order, (int)TermGroup.AttestEntity));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest: Number of transitions

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7056, "Attest - Övergångar"));

                    IEnumerable<AttestTransition> orderAttestTransitions = AttestManager.GetAttestTransitions(TermGroup_AttestEntity.Order, SoeModule.Billing, true, actorCompanyId);

                    if (orderAttestTransitions.Count() > 1)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS + " (" + orderAttestTransitions.Count().ToString() + " st)";
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(7057, "Inga övergångar är inställda");
                    }
                    result.Add(item);
                    #endregion

                    #region Företagsinställning - Status överfört till faktura

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7060, "Företagsinställning - Status överförd till faktura"));
                    setting = GetUserCompanySetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, actorCompanyId, 0);
                    if (setting != null && setting.IntData != 0)
                    {
                        var orderAttestState = orderAttestStates.FirstOrDefault(r => r.AttestStateId == setting.IntData);

                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = orderAttestState != null ? orderAttestState.Name : String.Empty;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(7058, "Markera önskat värde på företagsinställningen under Försäljning->Inställningar->Inställningar Fakturering->Status"); //"Markera önskat värde på företagsinställningen '{0}' under.."
                    }
                    result.Add(item);

                    #endregion

                    #region Attest: Attestroles exist for ALL transitions

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7061, "Attest - Roller för övergångar"));
                    hasRole = true;
                    roleHasUser = true;
                    missingRole = String.Empty;
                    missingUser = String.Empty;

                    if (orderAttestTransitions.Any())
                    {
                        foreach (AttestTransition transition in orderAttestTransitions)
                        {
                            if (transition.AttestRole.Count < 1)
                            {
                                hasRole = false;
                                missingRole += String.IsNullOrEmpty(missingRole) ? transition.Name : ", " + transition.Name;
                            }
                            else
                            {
                                foreach (AttestRole attestRole in transition.AttestRole)
                                {
                                    if (!attestRole.AttestRoleUser.IsLoaded)
                                        attestRole.AttestRoleUser.Load();
                                    if (attestRole.AttestRoleUser.Count < 1)
                                    {
                                        roleHasUser = false;
                                        missingUser += String.IsNullOrEmpty(missingUser) ? attestRole.Name : ", " + attestRole.Name;
                                    }
                                }
                            }
                        }
                    }

                    if (hasRole)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(7062, "Minst en av övergångarna har ingen attestroll knuten till sig") + " (" + missingRole + ")";
                    }
                    result.Add(item);

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(7063, "Attest - Användare knuten till roller"));

                    if (roleHasUser)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(7065, "Minst en av rollerna har ingen användare knuten till sig") + " (" + missingUser + ")";
                    }
                    result.Add(item);
                    #endregion

                    break;

                #endregion
                case TermGroup_CheckSettingsArea.CustomerInvoice:
                    #region CustomerInvoice

                    // Test
                    item = new CheckSettingsResult(area, areaName, sort++, "Test");
                    item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                    result.Add(item);
                    break;

                #endregion
                case TermGroup_CheckSettingsArea.Contract:
                    #region Contract

                    // Test
                    item = new CheckSettingsResult(area, areaName, sort++, "Test");
                    item.ResultType = TermGroup_CheckSettingsResultType.Warning;
                    result.Add(item);
                    break;

                #endregion
                case TermGroup_CheckSettingsArea.HouseholdTaxDeduction:
                    #region HouseholdTaxDeduction

                    #region Household product

                    #region Household product: Base product
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8108, "Basartikel husavdrag"));
                    InvoiceProduct householdProduct = null;
                    int householdProductId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHouseholdTaxDeduction, 0, actorCompanyId, 0);
                    if (householdProductId != 0)
                    {
                        householdProduct = ProductManager.GetInvoiceProduct(householdProductId, true, true, true);
                        if (householdProduct != null && householdProduct.State == (int)SoeEntityState.Active)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", householdProduct.Number, householdProduct.Name);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8109, "Välj en artikel under Försäljning->Inställningar->Basartiklar->{0}"), GetText(3287));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format("Välj en artikel under Försäljning->Inställningar->Basartiklar->{0}", GetText(3287));
                    }
                    result.Add(item);
                    #endregion

                    #region Household product: accounting
                    ProductAccountStd householdProductAccStd = null;
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8110, "Kontering på basartikel husavdrag"));
                    if (householdProduct?.ProductAccountStd != null)
                    {
                        householdProductAccStd = householdProduct.ProductAccountStd.FirstOrDefault(p => p.Type == (int)ProductAccountType.Purchase);
                        if (householdProductAccStd != null && householdProductAccStd.AccountStd != null && householdProductAccStd.AccountStd.Account != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", householdProductAccStd.AccountStd.Account.AccountNr, householdProductAccStd.AccountStd.Account.Name);
                        }
                    }
                    if (item.ResultType == TermGroup_CheckSettingsResultType.NotChecked)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8087, "Lägg till ett fordranskonto på ovanstående basartikel");
                    }
                    result.Add(item);
                    #endregion

                    #endregion

                    #region Household product denied

                    #region Household product denied: Base product
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(9010, "Basartikel avslaget husavdrag"));
                    InvoiceProduct householdProductDenied = null;
                    int householdProductDeniedId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHouseholdTaxDeductionDenied, 0, actorCompanyId, 0);
                    if (householdProductDeniedId != 0)
                    {
                        householdProductDenied = ProductManager.GetInvoiceProduct(householdProductDeniedId, true, true, true);
                        if (householdProductDenied != null && householdProductDenied.State == (int)SoeEntityState.Active)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", householdProductDenied.Number, householdProductDenied.Name);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8109, "Välj en artikel under Försäljning->Inställningar->Basartiklar->{0}"), GetText(9009));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8109, "Välj en artikel under Försäljning->Inställningar->Basartiklar->{0}"), GetText(9009));
                    }
                    result.Add(item);
                    #endregion

                    #region Household product denied: accounting
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(9013, "Kontering på basartikel avslaget husavdrag"));
                    if (householdProductDenied?.ProductAccountStd != null)
                    {
                        ProductAccountStd accStd = householdProductDenied.ProductAccountStd.FirstOrDefault(p => p.Type == (int)ProductAccountType.Sales);
                        if (accStd != null && accStd.AccountStd != null && accStd.AccountStd.Account != null)
                        {
                            if (householdProductAccStd != null && householdProductAccStd.AccountStd != null &&
                                householdProductAccStd.AccountStd.Account.AccountNr == accStd.AccountStd.Account.AccountNr)
                            {
                                item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                                item.Description = String.Format("{0} - {1}", accStd.AccountStd.Account.AccountNr, accStd.AccountStd.Account.Name);
                            }
                            else
                            {
                                item.ResultType = TermGroup_CheckSettingsResultType.Error;
                                item.Description = INVALID;
                                item.Adjustment = GetText(9011, "Intäktskontot för avslaget husavdrag är inte samma som fordranskontot för husavdrag");
                            }
                        }
                    }
                    if (item.ResultType == TermGroup_CheckSettingsResultType.NotChecked)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(9012, "Lägg till ett intäktskonto på ovanstående basartikel");
                    }
                    result.Add(item);
                    #endregion

                    #endregion

                    #region Household product 50%

                    #region Household product: Base product
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8108, "Basartikel husavdrag") + " 50%");
                    InvoiceProduct householdProduct50 = null;
                    int household50ProductId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHousehold50TaxDeduction, 0, actorCompanyId, 0);
                    if (household50ProductId != 0)
                    {
                        householdProduct50 = ProductManager.GetInvoiceProduct(household50ProductId, true, true, true);
                        if (householdProduct50 != null && householdProduct50.State == (int)SoeEntityState.Active)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", householdProduct50.Number, householdProduct50.Name);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8109, "Välj en artikel under Försäljning->Inställningar->Basartiklar->{0}"), GetText(3287));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format("Välj en artikel under Försäljning->Inställningar->Basartiklar->{0}", GetText(3287));
                    }
                    result.Add(item);
                    #endregion

                    #region Household product: accounting
                    ProductAccountStd householdProduct50AccStd = null;
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8110, "Kontering på basartikel husavdrag") + " 50%");
                    if (householdProduct50?.ProductAccountStd != null)
                    {
                        householdProduct50AccStd = householdProduct50.ProductAccountStd.FirstOrDefault(p => p.Type == (int)ProductAccountType.Purchase);
                        if (householdProduct50AccStd != null && householdProduct50AccStd.AccountStd != null && householdProduct50AccStd.AccountStd.Account != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", householdProduct50AccStd.AccountStd.Account.AccountNr, householdProduct50AccStd.AccountStd.Account.Name);
                        }
                    }
                    if (item.ResultType == TermGroup_CheckSettingsResultType.NotChecked)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8087, "Lägg till ett fordranskonto på ovanstående basartikel");
                    }
                    result.Add(item);
                    #endregion

                    #endregion

                    #region Household product denied 50%

                    #region Household product denied: Base product
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(9010, "Basartikel avslaget husavdrag") + " 50%");
                    InvoiceProduct householdProductDenied50 = null;
                    int household50ProductDeniedId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHousehold50TaxDeductionDenied, 0, actorCompanyId, 0);
                    if (household50ProductDeniedId != 0)
                    {
                        householdProductDenied50 = ProductManager.GetInvoiceProduct(household50ProductDeniedId, true, true, true);
                        if (householdProductDenied50 != null && householdProductDenied50.State == (int)SoeEntityState.Active)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", householdProductDenied50.Number, householdProductDenied50.Name);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8109, "Välj en artikel under Försäljning->Inställningar->Basartiklar->{0}"), GetText(9009));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8109, "Välj en artikel under Försäljning->Inställningar->Basartiklar->{0}"), GetText(9009));
                    }
                    result.Add(item);
                    #endregion

                    #region Household product denied: accounting
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(9013, "Kontering på basartikel avslaget husavdrag") + " 50%");
                    if (householdProductDenied50?.ProductAccountStd != null)
                    {
                        ProductAccountStd accStd = householdProductDenied50.ProductAccountStd.FirstOrDefault(p => p.Type == (int)ProductAccountType.Sales);
                        if (accStd != null && accStd.AccountStd != null && accStd.AccountStd.Account != null)
                        {
                            if (householdProductAccStd != null && householdProductAccStd.AccountStd != null &&
                                householdProductAccStd.AccountStd.Account.AccountNr == accStd.AccountStd.Account.AccountNr)
                            {
                                item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                                item.Description = String.Format("{0} - {1}", accStd.AccountStd.Account.AccountNr, accStd.AccountStd.Account.Name);
                            }
                            else
                            {
                                item.ResultType = TermGroup_CheckSettingsResultType.Error;
                                item.Description = INVALID;
                                item.Adjustment = GetText(9011, "Intäktskontot för avslaget husavdrag är inte samma som fordranskontot för husavdrag");
                            }
                        }
                    }
                    if (item.ResultType == TermGroup_CheckSettingsResultType.NotChecked)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(9012, "Lägg till ett intäktskonto på ovanstående basartikel");
                    }
                    result.Add(item);
                    #endregion

                    #endregion

                    #region Reports

                    #region Report template: Household tax deduction
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format("Rapportmall - {0}", GetText(18, (int)TermGroup.SysReportTemplateType)));
                    Report householdReport = ReportManager.GetStandardReport(actorCompanyId, SoeReportTemplateType.HousholdTaxDeduction);
                    if (householdReport != null)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0} - {1}", householdReport.ReportNr, householdReport.Name);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format("Lägg till en rapport av typen '{0}'", GetText(18, (int)TermGroup.SysReportTemplateType));
                    }
                    result.Add(item);
                    #endregion

                    #endregion

                    #region Permissions

                    #region Permission: Apply for deduction
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format(GetText(8088, "Behörighet - {0}"), TermManager.GetSysTermForFeature(Feature.Billing_Invoice_Household)));
                    hasPermission = false;
                    foreach (Role role in roles)
                    {
                        if (FeatureManager.HasRolePermission(Feature.Billing_Invoice_Household, Permission.Modify, role.RoleId, actorCompanyId))
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                    if (hasPermission)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8113, "Minst en användare måste ha följande skrivrättighet: '{0}'"), TermManager.GetSysTermForFeature(Feature.Billing_Invoice_Household));
                    }
                    result.Add(item);
                    #endregion

                    #region Permission: Print
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format("Behörighet - {0}", TermManager.GetSysTermForFeature(Feature.Billing_Distribution_Reports_Selection)));
                    hasPermission = false;
                    foreach (Role role in roles)
                    {
                        if (FeatureManager.HasRolePermission(Feature.Billing_Distribution_Reports_Selection, Permission.Readonly, role.RoleId, actorCompanyId) &&
                            FeatureManager.HasRolePermission(Feature.Billing_Distribution_Reports_Selection_Download, Permission.Readonly, role.RoleId, actorCompanyId))
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                    if (hasPermission)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format("Minst en användare måste ha följande läsrättigheter: '{0}', '{1}'", TermManager.GetSysTermForFeature(Feature.Billing_Distribution_Reports_Selection), TermManager.GetSysTermForFeature(Feature.Billing_Distribution_Reports_Selection_Download));
                    }
                    result.Add(item);
                    #endregion

                    #region Permission: Unpaid customer invoices
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format("Behörighet - {0}", TermManager.GetSysTermForFeature(Feature.Economy_Customer_Invoice_Status_OriginToPayment)));
                    hasPermission = false;
                    foreach (Role role in roles)
                    {
                        if (FeatureManager.HasRolePermission(Feature.Economy_Customer_Invoice_Status_OriginToPayment, Permission.Modify, role.RoleId, actorCompanyId))
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                    if (hasPermission)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format("Minst en användare måste ha följande skrivrättighet: '{0}'", TermManager.GetSysTermForFeature(Feature.Economy_Customer_Invoice_Status_OriginToPayment));
                    }
                    result.Add(item);
                    #endregion

                    #endregion

                    break;

                #endregion
                case TermGroup_CheckSettingsArea.ProjectInvoice:
                    #region ProjectInvoice
                    TimeCode standardTimeCode = null;

                    #region Company settings

                    #region Company setting: Create project on new invoice
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8111, "Företagsinställning - Skapa projekt"));
                    setting = GetUserCompanySetting(SettingMainType.Company, (int)CompanySettingType.ProjectAutoGenerateOnNewInvoice, 0, actorCompanyId, 0);
                    if (setting != null)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = setting.BoolData.HasValue && setting.BoolData.Value ? GetText(8115, "Ja") : GetText(8116, "Nej");
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8112, "Markera önskat värde på företagsinställningen '{0}' under Tid->Inställningar->Projektinställningar->Företagsspecifika"), GetText(4522));
                    }
                    result.Add(item);
                    #endregion

                    #region Company setting: Create invoice row from transaction
                    item = new CheckSettingsResult(area, areaName, sort++, "Företagsinställning - Flytta transaktioner till fakturarad");
                    setting = GetUserCompanySetting(SettingMainType.Company, (int)CompanySettingType.ProjectCreateInvoiceRowFromTransaction, 0, actorCompanyId, 0);
                    if (setting != null)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = setting.BoolData.HasValue && setting.BoolData.Value ? "Ja" : "Nej";
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format("Markera önskat värde på företagsinställningen '{0}' under Tid->Inställningar->Projektinställningar->Företagsspecifika", GetText(4524));
                    }
                    result.Add(item);
                    #endregion

                    #region Company setting: Standard TimeCode
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format("Företagsinställning - {0}", GetText(4450)));
                    timeCodeId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, actorCompanyId, 0);
                    if (timeCodeId != 0)
                    {
                        standardTimeCode = TimeCodeManager.GetTimeCode(timeCodeId, actorCompanyId, false);
                        if (standardTimeCode != null && standardTimeCode.State == (int)SoeEntityState.Active)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", standardTimeCode.Code, standardTimeCode.Name);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format("Välj en tidkod under Tid->Inställningar->Generella inställningar->Företagsspecifika->{0}", GetText(4450));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format("Välj en tidkod under Tid->Inställningar->Generella inställningar->Företagsspecifika->{0}", GetText(4450));
                    }
                    result.Add(item);
                    #endregion

                    #region Company setting: Time project report
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format(GetText(8114, "Företagsinställning - {0}"), GetText(8011)));

                    int timeProjectReportId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultTimeProjectReportTemplate, 0, actorCompanyId, 0);
                    if (timeProjectReportId != 0)
                    {
                        Report timeProjectReport = ReportManager.GetStandardReport(actorCompanyId, SoeReportTemplateType.ProjectStatisticsReport);
                        if (timeProjectReport != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", timeProjectReport.ReportNr, timeProjectReport.Name);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = MISSING;
                            item.Adjustment = String.Format(GetText(8117, "Lägg till en rapport av typen '{0}'"), GetText(19, (int)TermGroup.SysReportTemplateType));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8118, "Välj en rapportmall under Försäljning->Inställningar->Generella->{0}"), GetText(8011));
                    }
                    result.Add(item);
                    #endregion

                    #region Company setting: Base accounts
                    // EmployeeGroup cost
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format(GetText(8119, "Företagsinställning - Baskonto {0}"), GetText(5204)));
                    accountCostId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupCost, 0, actorCompanyId, 0);
                    if (accountCostId != 0)
                    {
                        accountCost = AccountManager.GetAccountStd(actorCompanyId, accountCostId, true, false);
                        if (accountCost != null && accountCost.Account != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", accountCost.Account.AccountNr, accountCost.Account.Name);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8120, "Välj ett baskonto under Tid->Inställningar->Inställningar tid->Baskonton->{0}->{1}"), GetText(5203), GetText(5204));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8120, "Välj ett baskonto under Tid->Inställningar->Inställningar tid->Baskonton->{0}->{1}"), GetText(5203), GetText(5204));
                    }
                    result.Add(item);

                    // EmployeeGroup income
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format(GetText(8119, "Företagsinställning - Baskonto {0}"), GetText(5205)));
                    AccountStd accountIncome = null;
                    int accountIncomeId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupIncome, 0, actorCompanyId, 0);
                    if (accountIncomeId != 0)
                    {
                        accountIncome = AccountManager.GetAccountStd(actorCompanyId, accountIncomeId, true, false);
                        if (accountIncome != null && accountCost.Account != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", accountIncome.Account.AccountNr, accountIncome.Account.Name);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8120, "Välj ett baskonto under Tid->Inställningar->Inställningar tid->Baskonton->{0}->{1}"), GetText(5203), GetText(5205));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8120, "Välj ett baskonto under Tid->Inställningar->Inställningar tid->Baskonton->{0}->{1}"), GetText(5203), GetText(5205));
                    }
                    result.Add(item);
                    #endregion

                    #region Company setting: Salary Export states

                    #region Minimum attest state for payroll transactions
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format(GetText(8121, "Företagsinställning - {0} '{1}'"), GetText(4494), GetText(4497)));
                    attestStateId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus, 0, actorCompanyId, 0);
                    if (attestStateId != 0)
                    {
                        attestState = AttestManager.GetAttestState(attestStateId);
                        if (attestState != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", attestState.Name, attestState.Description);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8122, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4497));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8122, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4497));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest state after export for payroll transactions
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format(GetText(8121, "Företagsinställning - {0} '{1}'"), GetText(4494), GetText(4498)));
                    attestState = null;
                    attestStateId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, 0, actorCompanyId, 0);
                    if (attestStateId != 0)
                    {
                        attestState = AttestManager.GetAttestState(attestStateId);
                        if (attestState != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", attestState.Name, attestState.Description);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8122, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4498));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8122, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4498));
                    }
                    result.Add(item);
                    #endregion

                    #region Minimum attest state for invoice transactions
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format(GetText(8121, "Företagsinställning - {0} '{1}'"), GetText(4494), GetText(4499)));
                    attestState = null;
                    attestStateId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportInvoiceMinimumAttestStatus, 0, actorCompanyId, 0);
                    if (attestStateId != 0)
                    {
                        attestState = AttestManager.GetAttestState(attestStateId);
                        if (attestState != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", attestState.Name, attestState.Description);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8122, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4499));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8122, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4499));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest state after export for invoice transactions
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format(GetText(8121, "Företagsinställning - {0} '{1}'"), GetText(4494), GetText(4500)));
                    attestState = null;
                    attestStateId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportInvoiceResultingAttestStatus, 0, actorCompanyId, 0);
                    if (attestStateId != 0)
                    {
                        attestState = AttestManager.GetAttestState(attestStateId);
                        if (attestState != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", attestState.Name, attestState.Description);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8089, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4500));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8090, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4500));
                    }
                    result.Add(item);
                    #endregion

                    #endregion

                    #endregion

                    #region Attest

                    #region Invoice products

                    #region Standard timecode: Standard timecode is connected to InvoiceProduct
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8237, "Tidkod - Artikel kopplad till standardtidkod"));
                    if (standardTimeCode != null && !standardTimeCode.TimeCodeInvoiceProduct.IsLoaded)
                        standardTimeCode.TimeCodeInvoiceProduct.Load();

                    TimeCodeInvoiceProduct tciProduct = null;
                    if (standardTimeCode != null && standardTimeCode.TimeCodeInvoiceProduct != null)
                        tciProduct = standardTimeCode.TimeCodeInvoiceProduct.FirstOrDefault();

                    if (tciProduct != null && !tciProduct.InvoiceProductReference.IsLoaded)
                        tciProduct.InvoiceProductReference.Load();

                    if (tciProduct != null && tciProduct.InvoiceProduct != null)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0} - {1}", tciProduct.InvoiceProduct.Name, tciProduct.InvoiceProduct.Description);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8235, "Koppla artikel till standardtidkoden under Tid->Inställningar->{0}"), GetText(5057));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest state: Initial state for invoice products
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8123, "Attest - Startnivå för artiklar"));
                    AttestState initialStateInvoice = AttestManager.GetInitialAttestState(actorCompanyId, TermGroup_AttestEntity.InvoiceTime);
                    if (initialStateInvoice != null)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0} - {1}", initialStateInvoice.Name, initialStateInvoice.Description);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8124, "Lägg till en attestnivå av typen '{0}' markerad som startnivå under Tid->Attest->Register->Attestnivåer"), GetText((int)TermGroup_AttestEntity.InvoiceTime, (int)TermGroup.AttestEntity));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest transition: From initial state for invoice products
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8091, "Attest - Övergång från startnivå för artiklar"));
                    if (initialStateInvoice != null && !initialStateInvoice.AttestTransitionTo.IsLoaded)
                        initialStateInvoice.AttestTransitionTo.Load();

                    if (initialStateInvoice != null && initialStateInvoice.AttestTransitionTo != null && initialStateInvoice.AttestTransitionTo.Count > 0)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0}", initialStateInvoice.AttestTransitionTo.First().Name);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8092, "Lägg till en övergång från ovanstående startnivå under Tid->Attest->Register->Attestövergångar");
                    }
                    result.Add(item);
                    #endregion

                    #region Attest role: With transition from initial state for invoice products
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8125, "Attest - Attestroll för artiklar"));
                    hasRole = false;
                    if (initialStateInvoice != null)
                    {
                        IEnumerable<AttestTransition> transitionsInvoice = AttestManager.GetAttestTransitionsFromState(initialStateInvoice.AttestStateId);
                        foreach (AttestTransition transition in transitionsInvoice)
                        {
                            if (!transition.AttestRole.IsLoaded)
                                transition.AttestRole.Load();
                            foreach (AttestRole attestRole in transition.AttestRole)
                            {
                                if (!attestRole.AttestRoleUser.IsLoaded)
                                    attestRole.AttestRoleUser.Load();
                                if (attestRole.AttestRoleUser.Count > 0)
                                {
                                    hasRole = true;
                                    break;
                                }
                            }
                            if (hasRole)
                                break;
                        }
                    }
                    if (hasRole)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8093, "Minst en användare måste vara knuten till en attestroll med en övergång från startnivån för artiklar");
                    }
                    result.Add(item);
                    #endregion

                    #endregion

                    #region Payroll products

                    #region Standard timecode: Standard timecode is connected to InvoiceProduct
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8238, "Tidkod - Löneart kopplad till standardtidkod"));
                    if (standardTimeCode != null && !standardTimeCode.TimeCodePayrollProduct.IsLoaded)
                        standardTimeCode.TimeCodePayrollProduct.Load();

                    TimeCodePayrollProduct tcpProduct = null;
                    if (standardTimeCode != null && standardTimeCode.TimeCodePayrollProduct != null)
                        tcpProduct = standardTimeCode.TimeCodePayrollProduct.FirstOrDefault();

                    if (tcpProduct != null && !tcpProduct.PayrollProductReference.IsLoaded)
                        tcpProduct.PayrollProductReference.Load();

                    if (tcpProduct != null && tcpProduct.PayrollProduct != null)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0} - {1}", tcpProduct.PayrollProduct.Name, tcpProduct.PayrollProduct.Description);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8236, "Koppla löneart till standardtidkoden under Tid->Inställningar->{0}"), GetText(5057));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest state: Initial state for payroll products
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8126, "Attest - Startnivå för lönearter"));
                    initialStatePayroll = AttestManager.GetInitialAttestState(actorCompanyId, TermGroup_AttestEntity.PayrollTime);
                    if (initialStatePayroll != null)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0} - {1}", initialStatePayroll.Name, initialStatePayroll.Description);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8127, "Lägg till en attestnivå av typen '{0}' markerad som startnivå under Tid->Attest->Register->Attestnivåer"), GetText((int)TermGroup_AttestEntity.PayrollTime, (int)TermGroup.AttestEntity));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest transition: From initial state for payroll products
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8094, "Attest - Övergång från startnivå för lönearter"));
                    if (initialStatePayroll != null && !initialStatePayroll.AttestTransitionTo.IsLoaded)
                        initialStatePayroll.AttestTransitionTo.Load();

                    if (initialStatePayroll != null && initialStatePayroll.AttestTransitionTo != null && initialStatePayroll.AttestTransitionTo.Count > 0)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0}", initialStatePayroll.AttestTransitionTo.First().Name);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8095, "Lägg till en övergång från ovanstående startnivå under Tid->Attest->Register->Attestövergångar");
                    }
                    result.Add(item);
                    #endregion

                    #region Attest role: With transition from initial state for invoice products
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8096, "Attest - Attestroll för lönearter"));
                    hasRole = false;
                    if (initialStatePayroll != null)
                    {
                        IEnumerable<AttestTransition> transitionsPayroll = AttestManager.GetAttestTransitionsFromState(initialStatePayroll.AttestStateId);
                        foreach (AttestTransition transition in transitionsPayroll)
                        {
                            if (!transition.AttestRole.IsLoaded)
                                transition.AttestRole.Load();
                            foreach (AttestRole attestRole in transition.AttestRole)
                            {
                                if (!attestRole.AttestRoleUser.IsLoaded)
                                    attestRole.AttestRoleUser.Load();
                                if (attestRole.AttestRoleUser.Count > 0)
                                {
                                    hasRole = true;
                                    break;
                                }
                            }
                            if (hasRole)
                                break;
                        }
                    }
                    if (hasRole)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8097, "Minst en användare måste vara knuten till en attestroll med en övergång från startnivån för lönearter");
                    }
                    result.Add(item);
                    #endregion

                    #endregion

                    #endregion

                    #region Permissions

                    #region Permission: Project
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format("Behörighet - {0}", TermManager.GetSysTermForFeature(Feature.Time_Project_Invoice_Edit)));
                    hasPermission = false;
                    foreach (Role role in roles)
                    {
                        if (FeatureManager.HasRolePermission(Feature.Time_Project_Invoice_Edit, Permission.Modify, role.RoleId, actorCompanyId))
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                    if (hasPermission)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format("Minst en användare måste ha följande skrivrättighet: '{0}'", TermManager.GetSysTermForFeature(Feature.Time_Project_Invoice_Edit));
                    }
                    result.Add(item);
                    #endregion

                    #endregion

                    break;

                #endregion
                case TermGroup_CheckSettingsArea.Time:
                    #region Time

                    #region Company settings

                    #region Company setting: Base account
                    // EmployeeGroup cost
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format(GetText(8119, "Företagsinställning - Baskonto {0}"), GetText(5204)));
                    accountCostId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupCost, 0, actorCompanyId, 0);
                    if (accountCostId != 0)
                    {
                        accountCost = AccountManager.GetAccountStd(actorCompanyId, accountCostId, true, false);
                        if (accountCost != null && accountCost.Account != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", accountCost.Account.AccountNr, accountCost.Account.Name);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8120, "Välj ett baskonto under Tid->Inställningar->Inställningar tid->Baskonton->{0}->{1}"), GetText(5203), GetText(5204));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8120, "Välj ett baskonto under Tid->Inställningar->Inställningar tid->Baskonton->{0}->{1}"), GetText(5203), GetText(5204));
                    }
                    result.Add(item);
                    #endregion

                    #region Company setting: Standard TimeCode
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format("Företagsinställning - {0}", GetText(4450)));
                    timeCodeId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, actorCompanyId, 0);
                    if (timeCodeId != 0)
                    {
                        timeCode = TimeCodeManager.GetTimeCode(timeCodeId, actorCompanyId, false);
                        if (timeCode != null && timeCode.State == (int)SoeEntityState.Active)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", timeCode.Code, timeCode.Name);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format("Välj en tidkod under Tid->Inställningar->Generella inställningar->Företagsspecifika->{0}", GetText(4450));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format("Välj en tidkod under Tid->Inställningar->Generella inställningar->Företagsspecifika->{0}", GetText(4450));
                    }
                    result.Add(item);
                    #endregion

                    #region Company setting: Salary Export states

                    #region Minimum attest state for payroll transactions
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format(GetText(8121, "Företagsinställning - {0} '{1}'"), GetText(4494), GetText(4497)));
                    attestStateId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus, 0, actorCompanyId, 0);
                    if (attestStateId != 0)
                    {
                        attestState = AttestManager.GetAttestState(attestStateId);
                        if (attestState != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", attestState.Name, attestState.Description);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8122, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4497));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8122, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4497));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest state after export for payroll transactions
                    item = new CheckSettingsResult(area, areaName, sort++, String.Format(GetText(8121, "Företagsinställning - {0} '{1}'"), GetText(4494), GetText(4498)));
                    attestState = null;
                    attestStateId = GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, 0, actorCompanyId, 0);
                    if (attestStateId != 0)
                    {
                        attestState = AttestManager.GetAttestState(attestStateId);
                        if (attestState != null)
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                            item.Description = String.Format("{0} - {1}", attestState.Name, attestState.Description);
                        }
                        else
                        {
                            item.ResultType = TermGroup_CheckSettingsResultType.Error;
                            item.Description = INVALID;
                            item.Adjustment = String.Format(GetText(8122, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4498));
                        }
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8122, "Välj en status under Tid->Inställningar->Inställningar tid->{0}->{1}"), GetText(4494), GetText(4498));
                    }
                    result.Add(item);
                    #endregion

                    #endregion

                    #endregion

                    #region EmployeeGroup
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(5036, "Tidavtal"));
                    bool employeegroupExists = (EmployeeManager.GetEmployeeGroupsDict(actorCompanyId, false)).Count > 0;
                    if (employeegroupExists)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8128, "Lägg till tidavtal under Tid->Anställd->Tidavtal");
                    }
                    result.Add(item);
                    #endregion

                    #region Category
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(5052, "Anställdakategorier"));
                    bool categiriesExists = (CategoryManager.GetCategories(SoeCategoryType.Employee, actorCompanyId).Count > 0);
                    if (categiriesExists)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8151, "Lägg till Anställdakategorier under Tid->Inställningar->Kategorier->Anställdakategorier");
                    }
                    result.Add(item);
                    #endregion

                    #region Employee
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(5018, "Anställda"));
                    bool employeeExist = (EmployeeManager.GetAllEmployeeIds(actorCompanyId).Count) > 0;
                    if (employeeExist)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8129, "Lägg till anställda under Tid->Anställd->Anställda");
                    }
                    result.Add(item);

                    #endregion

                    #region Payroll product

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(6000, "Lönearter"));
                    bool payrollProductExists = (ProductManager.GetPayrollProductsDict(actorCompanyId, false)).Count > 0;
                    if (payrollProductExists)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8130, "Lägg till lönearter under Tid->Inställningar->Register->Lönearter");
                    }
                    result.Add(item);

                    #endregion

                    #region TimeCodes

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8132, "Tidkoder"));
                    bool timecodesExists = (TimeCodeManager.GetTimeCodes(actorCompanyId)).Count > 0;
                    if (timecodesExists)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8131, "Lägg till tidkoder under Tid->Inställningar->Register->...");
                    }
                    result.Add(item);

                    #endregion

                    #region TimeCodeBreaks

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(5059, "Rasttyper"));
                    bool timecodebreaksExists = (TimeCodeManager.GetTimeCodes(actorCompanyId, SoeTimeCodeType.Break)).Count > 0;
                    if (timecodebreaksExists)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8133, "Lägg till Rasttyper under Tid->Inställningar->Register->Rasttyper");
                    }
                    result.Add(item);

                    #endregion

                    #region TimeDeviationCauseManager

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(4304, "Avvikelseorsaker"));
                    bool deviationCausesExists = TimeDeviationCauseManager.ExistsTimeDeviationCause(actorCompanyId);
                    if (deviationCausesExists)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8134, "Lägg till avvikelseorsaker under Tid->Inställningar->Register->Avvikelseorsaker");
                    }
                    result.Add(item);

                    #endregion

                    #region Daytypes

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(3055, "Dagtyper"));
                    bool daytypeExists = CalendarManager.GetDayTypesByCompany(actorCompanyId).Count > 0;
                    if (daytypeExists)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8135, "Lägg till Dagtyper under Tid->Schema->Register->Dagtyper");
                    }
                    result.Add(item);

                    #endregion

                    #region Holidays

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(3054, "Avvikelsedagar"));
                    bool holidaysExists = CalendarManager.GetHolidaysByCompany(actorCompanyId).Count > 0;
                    if (holidaysExists)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8136, "Lägg till Avvikelsedagar under Tid->Schema->Register->Avvikelsedagar");
                    }
                    result.Add(item);

                    #endregion

                    #region Payrolltransactions

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8137, "Lönetransaktioner"));
                    bool payrollTransactionsExists = TimeTransactionManager.HasTimePayrollTransactions(actorCompanyId);
                    if (payrollTransactionsExists)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                    }
                    result.Add(item);

                    #endregion

                    #region TimeBlock

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8138, "Tidblock"));
                    bool timeBlocksExists = TimeBlockManager.HasCompanyActiveTimeBlocks(actorCompanyId);
                    if (timeBlocksExists)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                    }
                    result.Add(item);

                    #endregion

                    #region TimeStampEntries

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8139, "Stämplingar"));
                    bool timestampEntries = TimeStampManager.TimeStampEntriesExists(actorCompanyId);
                    if (timestampEntries)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                    }
                    result.Add(item);

                    #endregion

                    #region Attest

                    #region Attest state: Initial state
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8126, "Attest - Startnivå för lönearter"));
                    initialStatePayroll = AttestManager.GetInitialAttestState(actorCompanyId, TermGroup_AttestEntity.PayrollTime);
                    if (initialStatePayroll != null)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0} - {1}", initialStatePayroll.Name, initialStatePayroll.Description);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = String.Format(GetText(8127, "Lägg till en attestnivå av typen '{0}' markerad som startnivå under Tid->Attest->Register->Attestnivåer"), GetText((int)TermGroup_AttestEntity.PayrollTime, (int)TermGroup.AttestEntity));
                    }
                    result.Add(item);
                    #endregion

                    #region Attest transition: From initial state for payroll products
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8094, "Attest - Övergång från startnivå för lönearter"));
                    if (initialStatePayroll != null && !initialStatePayroll.AttestTransitionTo.IsLoaded)
                        initialStatePayroll.AttestTransitionTo.Load();

                    if (initialStatePayroll != null && initialStatePayroll.AttestTransitionTo != null && initialStatePayroll.AttestTransitionTo.Count > 0)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = String.Format("{0}", initialStatePayroll.AttestTransitionTo.First().Name);
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8095, "Lägg till en övergång från ovanstående startnivå under Tid->Attest->Register->Attestövergångar");
                    }
                    result.Add(item);
                    #endregion

                    #region Attest role: With transition from initial state for payroll products
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8096, "Attest - Attestroll för lönearter"));
                    hasRole = false;
                    if (initialStatePayroll != null)
                    {
                        IEnumerable<AttestTransition> transitionsPayroll = AttestManager.GetAttestTransitionsFromState(initialStatePayroll.AttestStateId);
                        foreach (AttestTransition transition in transitionsPayroll)
                        {
                            if (!transition.AttestRole.IsLoaded)
                                transition.AttestRole.Load();
                            foreach (AttestRole attestRole in transition.AttestRole)
                            {
                                if (!attestRole.AttestRoleUser.IsLoaded)
                                    attestRole.AttestRoleUser.Load();
                                if (attestRole.AttestRoleUser.Count > 0)
                                {
                                    hasRole = true;
                                    break;
                                }
                            }
                            if (hasRole)
                                break;
                        }
                    }
                    if (hasRole)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8097, "Minst en användare måste vara knuten till en attestroll med en övergång från startnivån för lönearter");
                    }
                    result.Add(item);
                    #endregion

                    #region Attest role: Users connected to any attestrole
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8140, "Attest - Användare kopplat till attestroll"));
                    bool anyUserConnectedToAnyAttestRole = AttestManager.HasAttestRoleUsersForCompany(actorCompanyId);
                    if (anyUserConnectedToAnyAttestRole)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8141, "Koppla användare till attestroll under Administrera->Användare->Koppla mot attestroller");
                    }
                    result.Add(item);


                    #endregion

                    #endregion

                    #region Schedule

                    #region TimeScheduleTemplateHead

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8142, "Schema - Schemamallar"));
                    bool templateHeadExists = TimeScheduleManager.TimeScheduleTemplateHeadsExists(actorCompanyId);
                    if (templateHeadExists)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8143, "Lägg till schemamallar under Tid->Schema->Hantera schema->Schemamallar");
                    }
                    result.Add(item);

                    #endregion

                    #region TimeScheduleTemplatePeriod

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8144, "Schema - Schemaperioder för alla schemamallar"));
                    bool allScheduleTemplatesHasPeriods = TimeScheduleManager.AllScheduleTemplatesHasPeriods(actorCompanyId);
                    if (allScheduleTemplatesHasPeriods)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8145, "Administrera schemamallar under Tid->Schema->Hantera schema->Schemamallar");
                    }
                    result.Add(item);

                    #endregion

                    #region TimeScheduleTemplateBlock

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8146, "Schema - Schemablock för alla schemaperioder"));
                    bool allPeriodsHasTemplateBlocks = allScheduleTemplatesHasPeriods && TimeScheduleManager.HasScheduleTemplatePeriodsHasTemplateBlocks(actorCompanyId);
                    if (allPeriodsHasTemplateBlocks)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8145, "Administrera schemamallar under Tid->Schema->Hantera schema->Schemamallar");
                    }
                    result.Add(item);

                    #endregion

                    #endregion

                    #region Placement

                    #region EmployeeSchedule

                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8147, "Aktivera schema- Schema för alla anställda"));
                    bool scheduleExistsForAllEmployees = TimeScheduleManager.EmployeeSchedulesForAllEmployeesExists(actorCompanyId);
                    if (scheduleExistsForAllEmployees)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = EXISTS;
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = MISSING;
                        item.Adjustment = GetText(8148, "Aktivera schema under Personal->Planering->Aktivera schema");
                    }
                    result.Add(item);

                    #endregion

                    #endregion

                    break;

                #endregion
                case TermGroup_CheckSettingsArea.Edi:
                    #region Edi

                    #region CompanyEdi
                    item = new CheckSettingsResult(area, areaName, sort++, GetText(8241, "EDI - Inställning"));
                    var usesEdi = EdiManager.CompanyUsesEdi(actorCompanyId);
                    if (usesEdi)
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Passed;
                        item.Description = GetText(5495, "EDI är aktiverat");
                    }
                    else
                    {
                        item.ResultType = TermGroup_CheckSettingsResultType.Error;
                        item.Description = GetText(5496, "EDI är inte aktiverat");
                        item.Adjustment = string.Format(GetText(8240, "Aktivera EDI under Administrera->Företag->Redigera företag->Aktivera EDI"), GetText(4522));
                    }
                    result.Add(item);

                    #endregion

                    break;
                    #endregion
            }

            if (result.Count == 0)
            {
                item = new CheckSettingsResult(area, GetText((int)area, (int)TermGroup.CheckSettingsArea), sort, null);
                item.ResultType = TermGroup_CheckSettingsResultType.Passed;

                result.Add(item);
            }

            return result;
        }

        #endregion

        #region Config settings

        public bool GetBoolConfigSetting(string name)
        {
            return ConfigurationManager.AppSettings[name] == "true";
        }

        public TermGroup_SysPageStatusSiteType SiteType
        {
            get
            {
                return CompDbCache.Instance.SiteType;
            }
        }

        public string GetApiInternalURL(bool force = false)
        {
            if (isTest() || force)
            {
                var redirectUrl = SoftOneIdUtil.OidcClientRedirectUri;

                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    return redirectUrl.ToLower().Replace("/login.aspx", "/ApiInternal");
                }
            }

            return string.Empty;
        }

        public bool isTest()
        {
            if (SiteType == TermGroup_SysPageStatusSiteType.Test)
                return true;

            return false;
        }

        public bool isDev()
        {
            if (SiteType != TermGroup_SysPageStatusSiteType.Test)
                return false;

            string conn = ConfigurationManager.ConnectionStrings["CompEntities"].ConnectionString;

            if (!string.IsNullOrEmpty(conn) && conn.ToLower().Contains("\\dev"))
                return true;

            return false;
        }

        #endregion

        #region UserCompanySetting - Multiple settings

        public List<UserCompanySetting> GetUserCompanySettingsForCompany(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetUserCompanySettingsForCompany(entities, actorCompanyId);
        }
        public List<UserCompanySetting> GetUserCompanySettingsForCompany(CompEntities entities, int actorCompanyId)
        {
            return entities.UserCompanySetting.Where(x => x.ActorCompanyId == actorCompanyId && !x.UserId.HasValue).ToList();
        }

        #region By type

        public List<UserCompanySetting> GetAllCompanySettings(int settingTypeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanySetting.NoTracking();
            return GetAllCompanySettings(entities, settingTypeId);
        }

        public List<UserCompanySetting> GetAllCompanySettings(CompEntities entities, int settingTypeId)
        {
            return (from setting in entities.UserCompanySetting
                    where setting.ActorCompanyId.HasValue &&
                    !setting.UserId.HasValue &&
                    !setting.LicenseId.HasValue &&
                    setting.SettingTypeId == settingTypeId
                    orderby setting.SettingTypeId
                    select setting).ToList();
        }

        #endregion

        #region By group

        public List<UserCompanySetting> GetCompanySettings(int companySettingTypeGroup, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanySetting.NoTracking();
            return GetCompanySettings(entities, companySettingTypeGroup, actorCompanyId);
        }

        public List<UserCompanySetting> GetCompanySettings(CompEntities entities, int companySettingTypeGroup, int actorCompanyId)
        {
            List<UserCompanySetting> allSettings = (from setting in entities.UserCompanySetting
                                                    where setting.ActorCompanyId == actorCompanyId &&
                                                    !setting.UserId.HasValue &&
                                                    !setting.LicenseId.HasValue
                                                    orderby setting.SettingTypeId
                                                    select setting).ToList();

            return allSettings.Where(w => GetCompanySettingTypesForGroup(companySettingTypeGroup).Contains((CompanySettingType)w.SettingTypeId)).ToList();
        }

        public List<CompanySettingType> GetCompanySettingTypesForGroup(int companySettingTypeGroup)
        {
            List<CompanySettingType> settings = new List<CompanySettingType>();
            List<CompanySettingType> allSettingTypes = Enum.GetValues(typeof(CompanySettingType)).Cast<CompanySettingType>().ToList();

            switch (companySettingTypeGroup)
            {
                #region All

                case (int)UserSettingTypeGroup.All:
                    settings = allSettingTypes;
                    break;

                #endregion

                #region Modules and Areas

                case (int)CompanySettingTypeGroup.Core:
                    settings = allSettingTypes.Where(i => (int)i >= 1 && (int)i <= 100).ToList();
                    break;
                case (int)CompanySettingTypeGroup.Accounting:
                    settings = allSettingTypes.Where(i => (int)i >= 101 && (int)i <= 200).ToList();
                    break;
                case (int)CompanySettingTypeGroup.Supplier:
                    settings = allSettingTypes.Where(i => (int)i >= 201 && (int)i <= 300 || ((int)i >= 7201 && (int)i <= 7400)).ToList();
                    break;
                case (int)CompanySettingTypeGroup.Customer:
                    settings = allSettingTypes.Where(i => (int)i >= 301 && (int)i <= 400).ToList();
                    break;
                case (int)CompanySettingTypeGroup.Billing:
                    settings = allSettingTypes.Where(i => (int)i >= 401 && (int)i <= 500 || ((int)i >= 6001 && (int)i <= 6500)).ToList();
                    break;
                case (int)CompanySettingTypeGroup.Time:
                    settings = allSettingTypes.Where(i => ((int)i >= 501 && (int)i <= 600) || ((int)i >= 901 && (int)i <= 1000) || ((int)i >= 8000 && (int)i <= 8999)).ToList();
                    break;
                case (int)CompanySettingTypeGroup.Project:
                    settings = allSettingTypes.Where(i => (int)i >= 601 && (int)i <= 700).ToList();
                    break;
                case (int)CompanySettingTypeGroup.Inventory:
                    settings = allSettingTypes.Where(i => (int)i >= 701 && (int)i <= 800).ToList();
                    break;
                case (int)CompanySettingTypeGroup.Manage:
                    settings = allSettingTypes.Where(i => ((int)i >= 801 && (int)i <= 900) || (int)i == (int)CompanySettingType.UseMissingMandatoryInformation).ToList();
                    break;
                case (int)CompanySettingTypeGroup.Payroll:
                    settings = allSettingTypes.Where(i => (int)i >= 3001 && (int)i <= 6000).ToList();
                    break;
                case (int)CompanySettingTypeGroup.PayrollAgreement:
                    settings = allSettingTypes.Where(i => (int)i >= 3001 && (int)i <= 3100).ToList();
                    break;
                case (int)CompanySettingTypeGroup.PayrollEmploymentTypes_SE:
                    settings = allSettingTypes.Where(i => (int)i >= 3101 && (int)i <= 3200).ToList();
                    break;
                case (int)CompanySettingTypeGroup.PayrollEmploymentTypes_FI:
                    settings = allSettingTypes.Where(i => (int)i >= 3201 && (int)i <= 3300).ToList();
                    break;
                case (int)CompanySettingTypeGroup.PayrollEmploymentTypes_NO:
                    settings = allSettingTypes.Where(i => (int)i >= 3301 && (int)i <= 3400).ToList();
                    break;
                case (int)CompanySettingTypeGroup.SoftOneStage:
                    settings = allSettingTypes.Where(i => (int)i >= 7000 && (int)i <= 7099).ToList();
                    break;

                #endregion

                #region BaseAccounts

                case (int)CompanySettingTypeGroup.BaseAccounts:
                    settings = allSettingTypes.Where(i => (int)i >= 1001 && (int)i <= 2000).ToList();
                    break;
                case (int)CompanySettingTypeGroup.BaseAccountsCommon:
                    settings = allSettingTypes.Where(i => (int)i >= 1001 && (int)i <= 1100).ToList();
                    break;
                case (int)CompanySettingTypeGroup.BaseAccountsSupplier:
                    settings = allSettingTypes.Where(i => (int)i >= 1101 && (int)i <= 1200).ToList();
                    break;
                case (int)CompanySettingTypeGroup.BaseAccountsCustomer:
                    settings = allSettingTypes.Where(i => (int)i >= 1201 && (int)i <= 1300).ToList();
                    break;
                case (int)CompanySettingTypeGroup.BaseAccountsInvoiceProduct:
                    settings = allSettingTypes.Where(i => (int)i >= 1301 && (int)i <= 1400).ToList();
                    break;
                case (int)CompanySettingTypeGroup.BaseAccountsEmployeeGroup:
                    settings = allSettingTypes.Where(i => (int)i >= 1401 && (int)i <= 1500).ToList();
                    break;
                case (int)CompanySettingTypeGroup.BaseAccountsEmployee:
                    settings = allSettingTypes.Where(i => (int)i >= 1501 && (int)i <= 1600).ToList();
                    break;
                case (int)CompanySettingTypeGroup.BaseAccountsInventory:
                    settings = allSettingTypes.Where(i => (int)i >= 1601 && (int)i <= 1700).ToList();
                    break;

                #endregion

                #region BaseProducts

                case (int)CompanySettingTypeGroup.BaseProducts:
                    settings = allSettingTypes.Where(i => (int)i >= 2001 && (int)i <= 3000).ToList();
                    break;

                    #endregion
            }

            return settings;

        }

        public Dictionary<int, object> GetCompanySettingsDict(int companySettingTypeGroup, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanySetting.NoTracking();
            return GetCompanySettingsDict(entities, companySettingTypeGroup, actorCompanyId);
        }

        public Dictionary<int, object> GetCompanySettingsDict(CompEntities entities, int companySettingTypeGroup, int actorCompanyId)
        {
            Dictionary<int, object> dict = new Dictionary<int, object>();

            List<UserCompanySetting> settings = GetCompanySettings(entities, companySettingTypeGroup, actorCompanyId);
            foreach (UserCompanySetting setting in settings)
            {
                int key = setting.SettingTypeId;
                object value = null;
                switch (setting.DataTypeId)
                {
                    case (int)SettingDataType.String:
                        value = setting.StrData;
                        break;
                    case (int)SettingDataType.Integer:
                        value = setting.IntData;
                        break;
                    case (int)SettingDataType.Boolean:
                        value = setting.BoolData;
                        break;
                    case (int)SettingDataType.Date:
                    case (int)SettingDataType.Time:
                        value = setting.DateData;
                        break;
                    case (int)SettingDataType.Decimal:
                        value = setting.DecimalData;
                        break;
                }
                if (!dict.Keys.Contains(key))
                    dict.Add(key, value);
            }

            return dict;
        }

        #endregion

        #region By setting

        public Dictionary<int, object> GetUserCompanySettings(SettingMainType settingMainType, List<int> settingTypeIds, int userId, int actorCompanyId, int licenseId)
        {
            Dictionary<int, object> dict = new Dictionary<int, object>();

            if (settingTypeIds.IsNullOrEmpty())
                return dict;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.UserCompanySetting.NoTracking();
            IQueryable<UserCompanySetting> query = from s in entitiesReadOnly.UserCompanySetting select s;
            switch (settingMainType)
            {
                case SettingMainType.User:
                    query = query.Where(s => s.UserId.HasValue && s.UserId.Value == userId && (!s.ActorCompanyId.HasValue || s.ActorCompanyId.Value == 0));
                    break;
                case SettingMainType.Company:
                    query = query.Where(s => (!s.UserId.HasValue || s.UserId.Value == 0) && s.ActorCompanyId.HasValue && s.ActorCompanyId.Value == actorCompanyId);
                    break;
                case SettingMainType.UserAndCompany:
                    query = query.Where(s => s.UserId.HasValue && s.UserId.Value == userId && s.ActorCompanyId.HasValue && s.ActorCompanyId.Value == actorCompanyId);
                    break;
                case SettingMainType.License:
                    query = query.Where(s => (!s.UserId.HasValue || s.UserId.Value == 0) && (!s.ActorCompanyId.HasValue || s.ActorCompanyId.Value == 0) && s.LicenseId.HasValue && s.LicenseId.Value == licenseId);
                    break;
                case SettingMainType.Application:
                    query = query.Where(s => (!s.UserId.HasValue || s.UserId.Value == 0) && (!s.ActorCompanyId.HasValue || s.ActorCompanyId.Value == 0) && (!s.LicenseId.HasValue || s.LicenseId.Value == 0));
                    break;
            }
            List<UserCompanySetting> settings = query.Where(s => settingTypeIds.Contains(s.SettingTypeId)).ToList();
            foreach (int settingTypeId in settingTypeIds)
            {
                if (dict.ContainsKey(settingTypeId))
                    continue;

                UserCompanySetting setting = settings.FirstOrDefault(i => i.SettingTypeId == settingTypeId);
                if (setting != null)
                    dict.Add(settingTypeId, setting.GetSettingValue());
            }

            return dict;
        }

        #endregion

        #region Specific settings

        public List<string> GetSysServiceUrisFromSettings()
        {
            try
            {
                string fromStatus = ConfigurationSetupUtil.IsTestBasedOnMachine() ? SoftOneStatusConnector.GetDefaultSysServiceUrl(ConfigurationSetupUtil.GetCurrentSysCompDbId(), ConfigurationSetupUtil.IsTestBasedOnMachine(), false) : SoftOneStatusConnector.GetSysServiceUrl(false, false);
                if (!String.IsNullOrEmpty(fromStatus))
                    return new List<string>() { fromStatus };

                List<int> settingTypes = new List<int>();
                settingTypes.Add((int)ApplicationSettingType.SysServiceUri1);
                settingTypes.Add((int)ApplicationSettingType.SysServiceUri2);
                settingTypes.Add((int)ApplicationSettingType.SysServiceUri3);
                settingTypes.Add((int)ApplicationSettingType.SysServiceUri4);

                Dictionary<int, object> applicationSettings = GetUserCompanySettings(SettingMainType.Application, settingTypes, 0, 0, 0);
                if (applicationSettings == null || applicationSettings.Count == 0)
                    return new List<string>() { "http://localhost:24998/" };

                return applicationSettings.Select(s => s.Value.ToString()).Where(s => !String.IsNullOrEmpty(s)).ToList();
            }
            catch (Exception ex)
            {
                LogError("GetSysServiceUrisFromSettings error: " + ex.ToString());
            }

            return new List<string>();
        }

        public Dictionary<int, int> GetInventoryEditTriggerAccountsFromSettings(int actorCompanyId)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();

            string accounts = GetStringSetting(SettingMainType.Company, (int)CompanySettingType.InventoryEditTriggerAccounts, 0, actorCompanyId, 0);
            if (!String.IsNullOrEmpty(accounts))
            {
                string[] records = accounts.Split(',');
                if (records.Length > 0)
                {
                    foreach (var record in records)
                    {
                        string[] valuePair = record.Split(':');

                        Int32.TryParse(valuePair[0], out int accountId);
                        if (accountId == 0 || dict.ContainsKey(accountId))
                            continue;

                        Int32.TryParse(valuePair[1], out int templateId);
                        if (templateId == 0)
                            continue;

                        dict.Add(accountId, templateId);
                    }
                }
            }

            return dict;
        }

        #endregion

        #endregion

        #region UserCompanySetting - Single setting (use cache)

        #region By type

        public UserCompanySetting GetUserCompanySetting(SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanySetting.NoTracking();
            return GetUserCompanySetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId);
        }

        public UserCompanySetting GetUserCompanySetting(CompEntities entities, SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            IQueryable<UserCompanySetting> query = from s in entities.UserCompanySetting select s;
            switch (settingMainType)
            {
                case SettingMainType.User:
                    query = query.Where(s => s.UserId.HasValue && s.UserId.Value == userId && (!s.ActorCompanyId.HasValue || s.ActorCompanyId.Value == 0));
                    break;
                case SettingMainType.Company:
                    query = query.Where(s => (!s.UserId.HasValue || s.UserId.Value == 0) && s.ActorCompanyId.HasValue && s.ActorCompanyId.Value == actorCompanyId);
                    break;
                case SettingMainType.UserAndCompany:
                    query = query.Where(s => s.UserId.HasValue && s.UserId.Value == userId && s.ActorCompanyId.HasValue && s.ActorCompanyId.Value == actorCompanyId);
                    break;
                case SettingMainType.License:
                    query = query.Where(s => (!s.UserId.HasValue || s.UserId.Value == 0) && (!s.ActorCompanyId.HasValue || s.ActorCompanyId.Value == 0) && s.LicenseId.HasValue && s.LicenseId.Value == licenseId);
                    break;
                case SettingMainType.Application:
                    query = query.Where(s => (!s.UserId.HasValue || s.UserId.Value == 0) && (!s.ActorCompanyId.HasValue || s.ActorCompanyId.Value == 0) && (!s.LicenseId.HasValue || s.LicenseId.Value == 0));
                    break;
            }

            return query.FirstOrDefault(s => s.SettingTypeId == settingTypeId);
        }

        public string GetSettingFromDict(Dictionary<int, object> dict, int key, int settingDataType, string defaultValue = "")
        {
            string settingValue = defaultValue;

            object value = dict.ContainsKey(key) ? dict[key] : null;
            if (value == null || String.IsNullOrEmpty(value.ToString()))
                return settingValue;

            switch (settingDataType)
            {
                case (int)SettingDataType.String:
                    settingValue = value.ToString().ToString();
                    break;
                case (int)SettingDataType.Integer:
                    settingValue = Convert.ToInt32(value).ToString();
                    break;
                case (int)SettingDataType.Boolean:
                    settingValue = StringUtility.GetBool(value.ToString()).ToString();
                    break;
                case (int)SettingDataType.Date:
                case (int)SettingDataType.Time:
                    settingValue = Convert.ToDateTime(value).ToString();
                    break;
                case (int)SettingDataType.Decimal:
                    settingValue = Convert.ToDecimal(value).ToString();
                    break;
            }
            return settingValue;
        }

        #endregion

        #region Specific setting

        public UserCompanySetting GetCompanySettingWithUniqueStringValue(int settingTypeId, string value)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompanySettingWithUniqueStringValue(entities, settingTypeId, value);
        }

        public UserCompanySetting GetCompanySettingWithUniqueStringValue(CompEntities entities, int settingTypeId, string value)
        {
            return (from setting in entities.UserCompanySetting
                    where setting.ActorCompanyId.HasValue &&
                    !setting.UserId.HasValue &&
                    !setting.LicenseId.HasValue &&
                    setting.SettingTypeId == settingTypeId &&
                    setting.DataTypeId == (int)SettingDataType.String &&
                    setting.StrData == value
                    orderby setting.SettingTypeId
                    select setting).FirstOrDefault();
        }

        public List<UserCompanySettingDTO> GetCompanySettingsWithUniqueStringValue(int settingTypeId, string value)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompanySettingsWithUniqueStringValue(entities, settingTypeId, value);
        }

        public List<UserCompanySettingDTO> GetCompanySettingsWithUniqueStringValue(CompEntities entities, int settingTypeId, string value)
        {
            return (from setting in entities.UserCompanySetting
                    where setting.ActorCompanyId.HasValue &&
                    !setting.UserId.HasValue &&
                    !setting.LicenseId.HasValue &&
                    setting.SettingTypeId == settingTypeId &&
                    setting.DataTypeId == (int)SettingDataType.String &&
                    setting.StrData == value
                    orderby setting.SettingTypeId
                    select new UserCompanySettingDTO
                    {
                        ActorCompanyId = setting.ActorCompanyId ?? 0,
                        SettingTypeId = setting.SettingTypeId,
                        DataTypeId = setting.DataTypeId,
                        BoolData = setting.BoolData,
                        IntData = setting.IntData,
                        StrData = setting.StrData,
                    }).ToList();
        }

        public List<int> GetCompanyIdsWithCompanyBoolSetting(CompanySettingType settingType, bool value = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanySetting.NoTracking();
            return (from s in entities.UserCompanySetting
                    where s.SettingTypeId == (int)settingType &&
                    !s.UserId.HasValue &&
                    !s.LicenseId.HasValue &&
                    s.ActorCompanyId.HasValue &&
                    s.BoolData.HasValue &&
                    s.BoolData == value
                    select s.ActorCompanyId.Value).ToList();
        }

        public List<int> GetCompanyIdsWithCompanyIntSetting(CompanySettingType settingType)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanySetting.NoTracking();
            return (from s in entities.UserCompanySetting
                    where s.SettingTypeId == (int)settingType &&
                    s.ActorCompanyId.HasValue &&
                    !s.UserId.HasValue &&
                    !s.LicenseId.HasValue &&
                    s.IntData.HasValue &&
                    s.IntData != 0
                    select s.ActorCompanyId.Value).ToList();
        }

        #endregion

        #region By data type

        #region Common

        private UserCompanySettingObject GetSetting(CompEntities entities, SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            if ((settingMainType == SettingMainType.User && userId == 0) ||
                (settingMainType == SettingMainType.Company && actorCompanyId == 0) ||
                (settingMainType == SettingMainType.License && licenseId == 0))
                return new UserCompanySettingObject();

            string key = $"{(int)settingMainType}_{settingTypeId}_{userId}_{actorCompanyId}_{licenseId}";
            if (companySettingCache.TryGetValue(key, out var settingObject))
                return settingObject ?? new UserCompanySettingObject();

            settingObject = new UserCompanySettingObject();

            UserCompanySetting setting = GetUserCompanySetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId);
            if (setting != null)
            {
                settingObject.DataTypeId = setting.DataTypeId;
                switch (settingObject.DataTypeId)
                {
                    case (int)SettingDataType.Integer:
                        settingObject.IntSetting = setting.IntData;
                        break;
                    case (int)SettingDataType.Boolean:
                        settingObject.BoolSetting = setting.BoolData;
                        break;
                    case (int)SettingDataType.String:
                        settingObject.StringSetting = setting.StrData;
                        break;
                    case (int)SettingDataType.Date:
                    case (int)SettingDataType.Time:
                        settingObject.DateSetting = setting.DateData;
                        break;
                    case (int)SettingDataType.Decimal:
                        settingObject.DecimalSetting = setting.DecimalData;
                        break;
                }
            }

            companySettingCache.TryAdd(key, settingObject);

            return settingObject;
        }

        #endregion

        #region String

        public string GetStringSetting(SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetStringSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId);
        }

        public string GetStringSetting(CompEntities entities, SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            return GetSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId).StringSetting;
        }

        #endregion

        #region Int

        public int GetCompanyIntSetting(CompanySettingType type)
        {
            return GetIntSetting(SettingMainType.Company, (int)type, 0, base.ActorCompanyId, 0);
        }

        public int GetCompanyIntSetting(CompEntities entities, CompanySettingType type)
        {
            return GetIntSetting(entities, SettingMainType.Company, (int)type, 0, base.ActorCompanyId, 0);
        }

        public int GetIntSetting(SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetIntSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId);
        }

        public int GetIntSetting(CompEntities entities, SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            return Convert.ToInt32(GetSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId).IntSetting, CultureInfo.InvariantCulture);
        }

        public int GetIntSettingFromDict(Dictionary<int, object> dict, int key, int defaultValue = 0)
        {
            return dict.ContainsKey(key) ? Convert.ToInt32(dict[key]) : defaultValue;
        }

        #endregion

        #region Int?

        public int? GetNullableIntSetting(SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetNullableIntSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId);
        }

        public int? GetNullableIntSetting(CompEntities entities, SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            return GetSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId).IntSetting;
        }

        #endregion

        #region Bool

        public bool GetCompanyBoolSetting(CompanySettingType type)
        {
            return GetBoolSetting(SettingMainType.Company, (int)type, 0, base.ActorCompanyId, 0);
        }

        public bool GetBoolSetting(SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId, bool defaultValue = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetBoolSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId, defaultValue: defaultValue);
        }

        public bool GetBoolSetting(CompEntities entities, SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId, bool defaultValue = false)
        {
            bool? b = GetSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId).BoolSetting;
            return b ?? defaultValue;
        }

        public bool GetBoolSettingFromDict(Dictionary<int, object> dict, int key, bool defaultValue = false)
        {
            return dict.ContainsKey(key) ? StringUtility.GetBool(dict[key]) : defaultValue;
        }

        #endregion

        #region Date

        public DateTime GetDateSetting(SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetDateSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId);
        }

        public DateTime GetDateSetting(CompEntities entities, SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            return Convert.ToDateTime(GetSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId).DateSetting, CultureInfo.InvariantCulture);
        }

        public DateTime GetDateTimeSettingFromDict(Dictionary<int, object> dict, int key)
        {
            return dict.ContainsKey(key) ? Convert.ToDateTime(dict[key]) : CalendarUtility.DATETIME_DEFAULT;
        }

        #endregion

        #region Decimal

        public decimal GetDecimalSetting(SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetDecimalSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId);
        }

        public decimal GetDecimalSetting(CompEntities entities, SettingMainType settingMainType, int settingTypeId, int userId, int actorCompanyId, int licenseId)
        {
            return Convert.ToDecimal(GetSetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId).DecimalSetting, CultureInfo.InvariantCulture);
        }

        public decimal GetDecimalSettingFromDict(Dictionary<int, object> dict, int key, decimal defaultValue = Decimal.Zero, int? decimals = null)
        {
            decimal value = dict.ContainsKey(key) ? Convert.ToDecimal(dict[key]) : defaultValue;
            return decimals.HasValue ? Decimal.Round(value, decimals.Value) : value;
        }

        #endregion

        #endregion

        #endregion

        #region UserCompanySetting - Modify settings

        #region Load for edit

        #region License

        public string GetLicenseSettingName(int userCompanySettingId)
        {
            string text = string.Empty;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            UserCompanySetting setting = entitiesReadOnly.UserCompanySetting.FirstOrDefault(u => u.UserCompanySettingId == userCompanySettingId);
            if (setting != null)
            {
                LicenseSettingType settingType = (LicenseSettingType)setting.SettingTypeId;

                string level1 = GetLicenceSettingGroupLevel1(settingType);
                string level2 = GetLicenceSettingGroupLevel2(settingType);
                string level3 = GetLicenceSettingGroupLevel3(settingType);
                string name = GetLicenceSettingName(settingType);

                if (!level1.IsNullOrEmpty())
                    text += level1 + ", ";
                if (!level2.IsNullOrEmpty())
                    text += level2 + ", ";
                if (!level3.IsNullOrEmpty())
                    text += level3 + ", ";
                text += name;
            }

            return text;
        }

        public List<UserCompanySettingEditDTO> GetLicenseSettingsForEdit(int licenseId)
        {
            List<UserCompanySettingEditDTO> dtos = new List<UserCompanySettingEditDTO>();

            // Get all settings from enum
            foreach (LicenseSettingType settingType in EnumUtility.GetValues<LicenseSettingType>())
            {
                UserCompanySettingEditDTO dto = new UserCompanySettingEditDTO(SettingMainType.License, (int)settingType);
                dto.DataType = GetLicenceSettingDataType(settingType);
                dto.Options = GetLicenseSettingOptions(settingType);
                dto.GroupLevel1 = GetLicenceSettingGroupLevel1(settingType);
                dto.GroupLevel2 = GetLicenceSettingGroupLevel2(settingType);
                dto.GroupLevel3 = GetLicenceSettingGroupLevel3(settingType);
                dto.Name = GetLicenceSettingName(settingType);
                dto.VisibleOnlyForSupportAdmin = GetLicenceSettingVisibility(settingType);
                if (!string.IsNullOrEmpty(dto.GroupLevel1))
                    dtos.Add(dto);
            }

            // Get settings for current license from database
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanySetting.NoTracking();
            List<UserCompanySetting> settings = (from s in entities.UserCompanySetting
                                                 where (!s.UserId.HasValue || s.UserId.Value == 0) &&
                                                 (!s.ActorCompanyId.HasValue || s.ActorCompanyId.Value == 0) &&
                                                 s.LicenseId.HasValue && s.LicenseId.Value == licenseId
                                                 select s).ToList();

            foreach (UserCompanySetting setting in settings)
            {
                // Set values from database
                UserCompanySettingEditDTO dto = dtos.FirstOrDefault(s => s.SettingTypeId == setting.SettingTypeId);
                if (dto != null)
                {
                    dto.UserCompanySettingId = setting.UserCompanySettingId;

                    if (dto.DataType != (SettingDataType)setting.DataTypeId)
                    {
                        // TODO: Mismatching datatype
                    }

                    // Get data by datatype
                    switch (dto.DataType)
                    {
                        case SettingDataType.String:
                            dto.StringValue = setting.StrData;
                            break;
                        case SettingDataType.Integer:
                            dto.IntegerValue = setting.IntData;
                            break;
                        case SettingDataType.Decimal:
                            dto.DecimalValue = setting.DecimalData;
                            break;
                        case SettingDataType.Boolean:
                            dto.BooleanValue = setting.BoolData;
                            break;
                        case SettingDataType.Date:
                        case SettingDataType.Time:
                            dto.DateValue = setting.DateData;
                            break;
                    }
                }
            }

            return !parameterObject.IsSupportLoggedIn ? dtos.Where(d => !d.VisibleOnlyForSupportAdmin).ToList() : dtos;
        }

        private SettingDataType GetLicenceSettingDataType(LicenseSettingType settingType)
        {
            switch (settingType)
            {
                case LicenseSettingType.SSO_ForceLogin:
                case LicenseSettingType.LifetimeSecondsEnabledOnUser:
                case LicenseSettingType.SSO_SoftForce:
                case LicenseSettingType.SSO_SkipActivationEmailOnSSO:
                    return SettingDataType.Boolean;
                case LicenseSettingType.SSO_Key:
                    return SettingDataType.String;
                case LicenseSettingType.BrandingCompany:
                    return SettingDataType.Integer;
            }

            return SettingDataType.String;
        }

        private List<SmallGenericType> GetLicenseSettingOptions(LicenseSettingType settingType)
        {
            switch (settingType)
            {
                case LicenseSettingType.BrandingCompany:
                    return GetTermGroupContent(TermGroup.BrandingCompanies).ToSmallGenericTypes();
                default:
                    return new List<SmallGenericType>();
            }
        }

        private string GetLicenceSettingGroupLevel1(LicenseSettingType settingType)
        {
            switch (settingType)
            {
                case LicenseSettingType.SSO_ForceLogin:
                case LicenseSettingType.SSO_Key:
                case LicenseSettingType.LifetimeSecondsEnabledOnUser:
                case LicenseSettingType.SSO_SoftForce:
                case LicenseSettingType.SSO_SkipActivationEmailOnSSO:
                    return GetText(11956, "Inloggning");
                case LicenseSettingType.BrandingCompany:
                    return GetText(12526, "Branding");
                default:
                    return String.Empty;
            }
        }

        private string GetLicenceSettingGroupLevel2(LicenseSettingType settingType)
        {
            switch (settingType)
            {
                case LicenseSettingType.SSO_ForceLogin:
                case LicenseSettingType.SSO_Key:
                case LicenseSettingType.SSO_SoftForce:
                case LicenseSettingType.SSO_SkipActivationEmailOnSSO:
                    return GetText(11957, "Single Sign On");
                case LicenseSettingType.LifetimeSecondsEnabledOnUser:
                    return GetText(12011, "Session");
                default:
                    return String.Empty;
            }
        }

        private string GetLicenceSettingGroupLevel3(LicenseSettingType settingType)
        {
            switch (settingType)
            {
                default:
                    return String.Empty;
            }
        }

        private string GetLicenceSettingName(LicenseSettingType settingType)
        {
            switch (settingType)
            {
                case LicenseSettingType.SSO_ForceLogin:
                    return GetText(11958, "Tvinga SSO-inloggning");
                case LicenseSettingType.SSO_Key:
                    return GetText(11959, "SSO-nyckel");
                case LicenseSettingType.LifetimeSecondsEnabledOnUser:
                    return GetText(12010, "Justera inaktivitetstimer på användare");
                case LicenseSettingType.SSO_SoftForce:
                    return GetText(12999, "Tvinga användare att logga in enbart om användare har external identitet");
                case LicenseSettingType.SSO_SkipActivationEmailOnSSO:
                    return GetText(13000, "Skicka inget aktiveringsmeddelande till SSO-Användare");
                case LicenseSettingType.BrandingCompany:
                    return GetText(12527, "Koppla licens mot varumärke");
                default:
                    return String.Empty;
            }
        }

        private bool GetLicenceSettingVisibility(LicenseSettingType settingType)
        {
            switch (settingType)
            {
                case LicenseSettingType.BrandingCompany:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #endregion

        #region Save

        public ActionResult SaveUserCompanySettings(List<UserCompanySettingEditDTO> settingsInput, int userId, int actorCompanyId, int licenseId)
        {
            ActionResult result = new ActionResult();

            if (settingsInput.IsNullOrEmpty())
                return result;

            #region Prereq

            SettingMainType settingMainType = settingsInput[0].SettingMainType;

            List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();
            Dictionary<int, EntityObject> tcDict = new Dictionary<int, EntityObject>();
            int tcTempIdCounter = 0;

            string yesString = GetText(52, "Ja");
            string noString = GetText(53, "Nej");

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (UserCompanySettingEditDTO settingInput in settingsInput)
                        {

                            UserCompanySetting setting = GetUserCompanySetting(entities, settingMainType, settingInput.SettingTypeId, userId, actorCompanyId, licenseId);
                            bool isNew = (setting == null);
                            if (isNew)
                            {
                                #region Add

                                tcTempIdCounter++;

                                setting = new UserCompanySetting()
                                {
                                    SettingTypeId = settingInput.SettingTypeId,
                                    DataTypeId = (int)settingInput.DataType
                                };

                                switch (settingMainType)
                                {
                                    case SettingMainType.Application:
                                        break;
                                    case SettingMainType.License:
                                        setting.LicenseId = licenseId;
                                        break;
                                    case SettingMainType.Company:
                                        setting.ActorCompanyId = actorCompanyId;
                                        break;
                                    case SettingMainType.UserAndCompany:
                                        setting.ActorCompanyId = actorCompanyId;
                                        setting.UserId = userId;
                                        break;
                                    case SettingMainType.User:
                                        setting.UserId = userId;
                                        break;
                                }
                                SetCreatedProperties(setting);
                                entities.UserCompanySetting.AddObject(setting);

                                #endregion
                            }

                            #region Set data

                            // Track changes data
                            List<SmallGenericType> options = null;
                            if (settingMainType == SettingMainType.License && (LicenseSettingType)settingInput.SettingTypeId == LicenseSettingType.BrandingCompany)
                                options = GetTermGroupContent(TermGroup.BrandingCompanies).ToSmallGenericTypes();
                            SetUserCompanySettingData(settingInput, setting, out string columnName, out TermGroup_TrackChangesColumnType columnType, out string fromValue, out string toValue, out string fromValueName, out string toValueName, yesString, noString, options);

                            #region Track changes

                            SoeEntityType entity = SoeEntityType.None;
                            int topRecordId = 0;
                            switch (settingMainType)
                            {
                                case SettingMainType.Application:
                                    entity = SoeEntityType.UserCompanySetting_Application;
                                    break;
                                case SettingMainType.License:
                                    entity = SoeEntityType.UserCompanySetting_License;
                                    topRecordId = licenseId;
                                    break;
                                case SettingMainType.Company:
                                    entity = SoeEntityType.UserCompanySetting_Company;
                                    topRecordId = actorCompanyId;
                                    break;
                                case SettingMainType.UserAndCompany:
                                    entity = SoeEntityType.UserCompanySetting_UserAndCompany;
                                    topRecordId = userId;
                                    break;
                                case SettingMainType.User:
                                    entity = SoeEntityType.UserCompanySetting_User;
                                    topRecordId = userId;
                                    break;
                            }

                            trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, ActorCompanyId, TermGroup_TrackChangesActionMethod.UserCompanySetting_Save, isNew ? TermGroup_TrackChangesAction.Insert : TermGroup_TrackChangesAction.Update, entity, topRecordId, entity, isNew ? tcTempIdCounter : setting.UserCompanySettingId, settingInput.DataType, columnName, columnType, fromValue, toValue, fromValueName, toValueName));
                            if (isNew)
                                tcDict.Add(tcTempIdCounter, setting);
                            else
                                SetModifiedProperties(setting);

                            #endregion

                            #endregion
                        }

                        result = SaveChanges(entities);
                        if (result.Success)
                        {
                            #region TrackChanges

                            // Add track changes
                            foreach (TrackChangesDTO dto in trackChangesItems.Where(t => t.Action == TermGroup_TrackChangesAction.Insert))
                            {
                                // Replace temp ids with actual ids created on save
                                UserCompanySetting setting = tcDict[dto.RecordId] as UserCompanySetting;
                                if (setting != null)
                                    dto.RecordId = setting.UserCompanySettingId;
                            }

                            if (trackChangesItems.Any())
                                result = TrackChangesManager.AddTrackChanges(entities, transaction, trackChangesItems);

                            #endregion
                        }

                        if (result.Success)
                        {
                            var ssoSettings = settingsInput.Where(a => a.SettingMainType == SettingMainType.License && (a.SettingTypeId == (int)LicenseSettingType.SSO_ForceLogin || a.SettingTypeId == (int)LicenseSettingType.SSO_Key || a.SettingTypeId == (int)LicenseSettingType.SSO_SoftForce || a.SettingTypeId == (int)LicenseSettingType.SSO_SkipActivationEmailOnSSO));


                            var providerGuidSetting = ssoSettings.FirstOrDefault(w => w.SettingTypeId == (int)LicenseSettingType.SSO_Key);
                            string providerGuid = providerGuidSetting?.StringValue;
                            if (String.IsNullOrEmpty(providerGuid))
                                providerGuid = GetStringSetting(entities, SettingMainType.License, (int)LicenseSettingType.SSO_Key, 0, 0, licenseId);
                            int? sysCompDbid = SysServiceManager.GetSysCompDBIdFromSetting();


                            if (sysCompDbid.HasValue && !String.IsNullOrEmpty(providerGuid) && Guid.TryParse(providerGuid, out Guid guid))
                            {
                                IdLoginConfidential idLoginConfidential = new IdLoginConfidential();
                                idLoginConfidential.IdProviderGuid = guid;
                                idLoginConfidential.IdLoginGuid = entities.User.First(f => f.UserId == userId).idLoginGuid.Value;
                                idLoginConfidential.LicenseId = licenseId;
                                idLoginConfidential.SysCompDbId = sysCompDbid.Value;

                                var forceExternalProviderLogin = ssoSettings.FirstOrDefault(w => w.BooleanValue.HasValue && w.SettingTypeId == (int)LicenseSettingType.SSO_ForceLogin);
                                if (forceExternalProviderLogin != null)
                                {
                                    idLoginConfidential.BoolValue = forceExternalProviderLogin.BooleanValue.Value;
                                    SoftOneIdConnector.AddidProviderGuid(idLoginConfidential);
                                    idLoginConfidential.BoolValue = null;
                                }
                                else
                                {
                                    SoftOneIdConnector.AddidProviderGuid(idLoginConfidential);
                                }

                                var softForceExternalProviderLogin = ssoSettings.FirstOrDefault(w => w.BooleanValue.HasValue && w.SettingTypeId == (int)LicenseSettingType.SSO_SoftForce);
                                if (softForceExternalProviderLogin != null)
                                {
                                    idLoginConfidential.BoolValue = softForceExternalProviderLogin.BooleanValue.Value;
                                    SoftOneIdConnector.AddSoftForceSetting(idLoginConfidential);
                                    idLoginConfidential.BoolValue = null;
                                }

                                var skipActivationEmail = ssoSettings.FirstOrDefault(w => w.BooleanValue.HasValue && w.SettingTypeId == (int)LicenseSettingType.SSO_SkipActivationEmailOnSSO);
                                if (skipActivationEmail != null)
                                {
                                    idLoginConfidential.BoolValue = skipActivationEmail.BooleanValue.Value;
                                    SoftOneIdConnector.AddSkipActivationEmail(idLoginConfidential);
                                    idLoginConfidential.BoolValue = null;
                                }
                            }

                            transaction.Complete();
                            EvoSettingCacheInvalidationConnector.InvalidateUserCompanySettingEditDTOs(settingsInput, userId, actorCompanyId, licenseId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private void SetUserCompanySettingData(UserCompanySettingEditDTO settingInput, UserCompanySetting setting, out string columnName, out TermGroup_TrackChangesColumnType columnType, out string fromValue, out string toValue, out string fromValueName, out string toValueName, string yesString, string noString, List<SmallGenericType> options = null)
        {
            // Track changes data
            columnName = String.Empty;
            columnType = TermGroup_TrackChangesColumnType.Unspecified;
            fromValue = null;
            toValue = null;
            fromValueName = null;
            toValueName = null;

            switch (settingInput.DataType)
            {
                case SettingDataType.String:
                    columnName = "StrData";
                    columnType = TermGroup_TrackChangesColumnType.UserCompanySetting_String;
                    fromValue = setting.StrData;
                    setting.StrData = settingInput.StringValue;
                    toValue = setting.StrData;
                    break;
                case SettingDataType.Integer:
                    columnName = "IntData";
                    columnType = TermGroup_TrackChangesColumnType.UserCompanySetting_Integer;
                    if (setting.IntData.HasValue)
                    {
                        fromValue = setting.IntData.ToString();
                        if (options != null)
                            fromValueName = options.FirstOrDefault(o => o.Id == setting.IntData.Value)?.Name;
                    }
                    setting.IntData = settingInput.IntegerValue;
                    if (setting.IntData.HasValue)
                    {
                        toValue = setting.IntData.ToString();
                        if (options != null)
                            toValueName = options.FirstOrDefault(o => o.Id == setting.IntData.Value)?.Name;
                    }
                    break;
                case SettingDataType.Decimal:
                    columnName = "DecimalData";
                    columnType = TermGroup_TrackChangesColumnType.UserCompanySetting_Decimal;
                    if (setting.DecimalData.HasValue)
                        fromValue = setting.DecimalData.ToString();
                    setting.DecimalData = settingInput.DecimalValue.HasValue ? Decimal.Round(settingInput.DecimalValue.Value, 2, MidpointRounding.AwayFromZero) : 0;
                    if (setting.DecimalData.HasValue)
                        toValue = setting.DecimalData.ToString();
                    break;
                case SettingDataType.Boolean:
                    columnName = "BoolData";
                    columnType = TermGroup_TrackChangesColumnType.UserCompanySetting_Boolean;
                    if (setting.BoolData.HasValue)
                        fromValue = setting.BoolData.Value ? yesString : noString;
                    setting.BoolData = settingInput.BooleanValue;
                    if (setting.BoolData.HasValue)
                        toValue = setting.BoolData.Value ? yesString : noString;
                    break;
                case SettingDataType.Date:
                    columnName = "DateData";
                    columnType = TermGroup_TrackChangesColumnType.UserCompanySetting_Date;
                    if (setting.DateData.HasValue)
                        fromValue = setting.DateData.Value.ToShortTimeString();
                    setting.DateData = settingInput.DateValue;
                    if (setting.DateData.HasValue)
                        toValue = setting.DateData.Value.ToShortTimeString();
                    break;
                case SettingDataType.Time:
                    columnName = "DateData";
                    columnType = TermGroup_TrackChangesColumnType.UserCompanySetting_Date;
                    if (setting.DateData.HasValue)
                        fromValue = setting.DateData.Value.ToShortDateString();
                    setting.DateData = settingInput.DateValue;
                    if (setting.DateData.HasValue)
                        toValue = setting.DateData.Value.ToShortDateString();
                    break;
            }
        }

        #endregion

        #region General

        public ActionResult UpdateInsertSettings(SettingMainType settingMainType, SettingDataType dataType, Dictionary<int, string> dict, int userId, int actorCompanyId, int licenseId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                bool disallowInterim = false;

                foreach (var pair in dict)
                {
                    if (!result.Success)
                        return result;

                    int settingTypeId = pair.Key;
                    string value = pair.Value;
                    bool settingExist = true;

                    UserCompanySetting setting = GetUserCompanySetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId);
                    if (setting == null)
                    {
                        #region Add

                        setting = new UserCompanySetting()
                        {
                            SettingTypeId = settingTypeId,
                            DataTypeId = (int)dataType,
                        };
                        SetCreatedProperties(setting);
                        entities.UserCompanySetting.AddObject(setting);

                        if (settingMainType == SettingMainType.User)
                            setting.UserId = userId;
                        else if (settingMainType == SettingMainType.Company)
                            setting.ActorCompanyId = actorCompanyId;
                        else if (settingMainType == SettingMainType.License)
                            setting.LicenseId = licenseId;
                        else if (settingMainType == SettingMainType.UserAndCompany)
                        {
                            setting.UserId = userId;
                            setting.ActorCompanyId = actorCompanyId;
                        }

                        settingExist = false;

                        #endregion
                    }
                    else
                    {
                        #region Update

                        SetModifiedProperties(setting);

                        #endregion
                    }

                    #region Set data

                    switch (dataType)
                    {
                        case SettingDataType.String:
                            #region String

                            string strValue = value;
                            if (settingExist && setting.StrData == strValue)
                                continue;

                            setting.StrData = strValue;

                            #endregion
                            break;
                        case SettingDataType.Integer:
                            #region Integer

                            int? intValue = null;
                            intValue = String.IsNullOrEmpty(value) ? intValue : Convert.ToInt32(value);
                            if (settingExist && setting.IntData == intValue)
                                continue;

                            setting.IntData = intValue;

                            #endregion
                            break;
                        case SettingDataType.Boolean:
                            #region Boolean

                            bool? boolValue = null;
                            boolValue = String.IsNullOrEmpty(value) ? boolValue : Convert.ToBoolean(value);
                            if (settingExist && setting.BoolData == boolValue)
                                continue;

                            setting.BoolData = boolValue;

                            #endregion
                            break;
                        case SettingDataType.Date:
                        case SettingDataType.Time:
                            #region Date / Time

                            DateTime? dateValue = null;
                            dateValue = String.IsNullOrEmpty(value) ? dateValue : Convert.ToDateTime(value);
                            if (settingExist && setting.DateData == dateValue)
                                continue;

                            setting.DateData = dateValue;

                            #endregion
                            break;
                        case SettingDataType.Decimal:
                            #region Decimal

                            Decimal? decimalValue = null;
                            decimalValue = String.IsNullOrEmpty(value) ? decimalValue : Convert.ToDecimal(value);
                            if (settingExist && setting.DecimalData == decimalValue)
                                continue;

                            setting.DecimalData = decimalValue.HasValue ? Decimal.Round(decimalValue.Value, 2, MidpointRounding.AwayFromZero) : 0;

                            #endregion
                            break;
                        default:
                            continue;
                    }

                    //If not changed it should be catched in a continue block above
                    SetModifiedProperties(setting);

                    if (!disallowInterim && setting.SettingTypeId == (int)CompanySettingType.SupplierInvoiceAllowInterim && setting.BoolData == false)
                        disallowInterim = true;

                    #endregion
                }

                result = SaveChanges(entities);
                // If company setting AllowInterim is set to false, update all suppliers with the same setting
                if (result.Success && disallowInterim)
                    result = SupplierManager.DisAllowInterim(entities, actorCompanyId);

                if (result.Success)
                    EvoSettingCacheInvalidationConnector.InvalidateCacheUserCompanySetting(licenseId, actorCompanyId, userId);

                return result;
            }
        }

        public ActionResult UpdateInsertSetting(SettingMainType settingMainType, int settingTypeId, UserCompanySettingObject templateSetting, int userId, int actorCompanyId, int licenseId)
        {
            return UpdateInsertSetting(null, settingMainType, settingTypeId, templateSetting, userId, actorCompanyId, licenseId);
        }

        public ActionResult UpdateInsertSetting(CompEntities entities, SettingMainType settingMainType, int settingTypeId, UserCompanySettingObject templateSetting, int userId, int actorCompanyId, int licenseId)
        {
            bool settingExist = true;

            UserCompanySetting setting = entities != null ? GetUserCompanySetting(entities, settingMainType, settingTypeId, userId, actorCompanyId, licenseId) : GetUserCompanySetting(settingMainType, settingTypeId, userId, actorCompanyId, licenseId);
            if (setting == null)
            {
                #region Add

                setting = new UserCompanySetting()
                {
                    SettingTypeId = settingTypeId,
                    DataTypeId = templateSetting.DataTypeId,
                };
                SetCreatedProperties(setting);
                settingExist = false;

                #endregion
            }
            else
                SetModifiedProperties(setting);

            switch (templateSetting.DataTypeId)
            {
                case (int)SettingDataType.String:
                    #region String

                    if (settingExist && setting.StrData != null && setting.StrData.Equals(templateSetting.StringSetting))
                        return new ActionResult(true);

                    setting.StrData = templateSetting.StringSetting;

                    #endregion
                    break;
                case (int)SettingDataType.Integer:
                    #region Integer

                    if (settingExist && setting.IntData.HasValue && templateSetting.IntSetting.HasValue && setting.IntData.Value == templateSetting.IntSetting.Value)
                        return new ActionResult(true);

                    setting.IntData = templateSetting.IntSetting;

                    #endregion
                    break;
                case (int)SettingDataType.Boolean:
                    #region Boolean

                    if (settingExist && setting.BoolData.HasValue && templateSetting.BoolSetting.HasValue && setting.BoolData.Value == templateSetting.BoolSetting.Value)
                        return new ActionResult(true);

                    setting.BoolData = templateSetting.BoolSetting;

                    #endregion
                    break;
                case (int)SettingDataType.Date:
                case (int)SettingDataType.Time:
                    #region Date / Time

                    if (settingExist && setting.DateData.HasValue && setting.DateData.HasValue && setting.DateData.Value.CompareTo(templateSetting.DateSetting.Value) == 0)
                        return new ActionResult(true);

                    setting.DateData = templateSetting.DateSetting;

                    #endregion
                    break;
                case (int)SettingDataType.Decimal:
                    #region Decimal

                    if (settingExist && setting.DecimalData.HasValue && setting.DecimalData.HasValue && setting.DecimalData.Value == templateSetting.DecimalSetting.Value)
                        return new ActionResult(true);

                    setting.DecimalData = templateSetting.DecimalSetting;

                    #endregion
                    break;
            }

            if (settingExist)
                return UpdateUserCompanySetting(entities, setting, actorCompanyId);
            else
                return AddUserCompanySetting(entities, setting, settingMainType, userId, actorCompanyId, licenseId);
        }

        public ActionResult AddUserCompanySetting(CompEntities entities, UserCompanySetting setting, SettingMainType settingMainType, int userId, int actorCompanyId, int licenseId)
        {
            if (setting == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "UserCompanySetting");


            bool entitiesWasNull = entities == null;
            if (entitiesWasNull)
                entities = new CompEntities();

            try
            {
                if (settingMainType == SettingMainType.User || settingMainType == SettingMainType.UserAndCompany)
                    setting.UserId = userId;

                if (settingMainType == SettingMainType.Company || settingMainType == SettingMainType.UserAndCompany)
                    setting.ActorCompanyId = actorCompanyId;
                else if (settingMainType == SettingMainType.License)
                    setting.LicenseId = licenseId;

                ActionResult result = AddEntityItem(entities, setting, "UserCompanySetting");

                // If company setting AllowInterim is set to false, update all suppliers with the same setting
                if (result.Success && setting.BoolData == false && setting.SettingTypeId == (int)CompanySettingType.SupplierInvoiceAllowInterim)
                    result = SupplierManager.DisAllowInterim(entities, actorCompanyId);

                if (result.Success)
                    EvoSettingCacheInvalidationConnector.InvalidateCacheUserCompanySetting(licenseId, actorCompanyId, userId);

                return result;
            }
            finally
            {
                if (entitiesWasNull)
                    entities.Dispose();
            }

        }

        public ActionResult UpdateUserCompanySetting(CompEntities entities, UserCompanySetting setting, int actorCompanyId)
        {
            if (setting == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "UserFavorite");

            bool entitiesWasNull = entities == null;
            if (entitiesWasNull)
                entities = new CompEntities();

            try
            {
                UserCompanySetting orginalSetting = entities.UserCompanySetting.FirstOrDefault(s => s.UserCompanySettingId == setting.UserCompanySettingId);
                if (orginalSetting == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "UserCompanySetting");

                ActionResult result = UpdateEntityItem(entities, orginalSetting, setting, "UserCompanySetting");

                // If company setting AllowInterim is set to false, update all suppliers with the same setting
                if (result.Success && !setting.BoolData == true && setting.SettingTypeId == (int)CompanySettingType.SupplierInvoiceAllowInterim)
                    result = SupplierManager.DisAllowInterim(entities, actorCompanyId);

                if (result.Success)
                    EvoSettingCacheInvalidationConnector.InvalidateCacheUserCompanySetting(setting.LicenseId, actorCompanyId, setting.UserId);

                return result;
            }
            finally
            {
                if (entitiesWasNull)
                    entities.Dispose();
            }
        }

        #endregion

        #region String

        public ActionResult UpdateInsertStringSettings(SettingMainType settingMainType, Dictionary<int, string> dict, int userId, int actorCompanyId, int licenseId)
        {
            if (dict.Count == 0)
                return new ActionResult(true);

            return UpdateInsertSettings(settingMainType, SettingDataType.String, dict, userId, actorCompanyId, licenseId);
        }

        public ActionResult UpdateInsertStringSetting(SettingMainType settingMainType, int settingTypeId, string value, int userId, int actorCompanyId, int licenseId)
        {
            return UpdateInsertStringSetting(null, settingMainType, settingTypeId, value, userId, actorCompanyId, licenseId);
        }

        public ActionResult UpdateInsertStringSetting(CompEntities entities, SettingMainType settingMainType, int settingTypeId, string value, int userId, int actorCompanyId, int licenseId)
        {
            UserCompanySettingObject setting = new UserCompanySettingObject()
            {
                DataTypeId = (int)SettingDataType.String,
                StringSetting = value,
            };
            return UpdateInsertSetting(entities, settingMainType, settingTypeId, setting, userId, actorCompanyId, licenseId);
        }

        #endregion

        #region Int

        public ActionResult UpdateInsertIntSettings(SettingMainType settingMainType, Dictionary<int, int> dict, int userId, int actorCompanyId, int licenseId)
        {
            if (dict.Count == 0)
                return new ActionResult(true);

            var convertedDict = new Dictionary<int, string>();
            foreach (var item in dict)
            {
                convertedDict.Add(item.Key, item.Value.ToString());
            }

            return UpdateInsertSettings(settingMainType, SettingDataType.Integer, convertedDict, userId, actorCompanyId, licenseId);
        }

        public ActionResult UpdateInsertIntSetting(SettingMainType settingMainType, int settingTypeId, int value, int userId, int actorCompanyId, int licenseId)
        {
            UserCompanySettingObject setting = new UserCompanySettingObject()
            {
                DataTypeId = (int)SettingDataType.Integer,
                IntSetting = value,
            };
            return UpdateInsertSetting(settingMainType, settingTypeId, setting, userId, actorCompanyId, licenseId);
        }

        public ActionResult UpdateInsertIntSetting(CompEntities entities, SettingMainType settingMainType, int settingTypeId, int value, int userId, int actorCompanyId, int licenseId)
        {
            UserCompanySettingObject setting = new UserCompanySettingObject()
            {
                DataTypeId = (int)SettingDataType.Integer,
                IntSetting = value,
            };
            return UpdateInsertSetting(entities, settingMainType, settingTypeId, setting, userId, actorCompanyId, licenseId);
        }

        #endregion

        #region Bool

        public ActionResult UpdateInsertBoolSettings(SettingMainType settingMainType, Dictionary<int, bool> dict, int userId, int actorCompanyId, int licenseId)
        {
            if (dict.Count == 0)
                return new ActionResult(true);

            var convertedDict = new Dictionary<int, string>();
            foreach (var item in dict)
            {
                convertedDict.Add(item.Key, item.Value.ToString());
            }

            return UpdateInsertSettings(settingMainType, SettingDataType.Boolean, convertedDict, userId, actorCompanyId, licenseId);
        }

        public ActionResult UpdateInsertBoolSetting(SettingMainType settingMainType, int settingTypeId, bool value, int userId, int actorCompanyId, int licenseId)
        {
            UserCompanySettingObject setting = new UserCompanySettingObject()
            {
                DataTypeId = (int)SettingDataType.Boolean,
                BoolSetting = value,
            };
            return UpdateInsertSetting(settingMainType, settingTypeId, setting, userId, actorCompanyId, licenseId);
        }

        #endregion

        #region Date

        public ActionResult UpdateInsertDateSettings(SettingMainType settingMainType, Dictionary<int, DateTime> dict, int userId, int actorCompanyId, int licenseId)
        {
            if (dict.Count == 0)
                return new ActionResult(true);

            foreach (var item in dict)
            {
                var result = UpdateInsertDateSetting(settingMainType, item.Key, item.Value, userId, actorCompanyId, licenseId);
                if (!result.Success)
                    return result;
            }

            return new ActionResult(true);
        }

        public ActionResult UpdateInsertDateSetting(SettingMainType settingMainType, int settingTypeId, DateTime value, int userId, int actorCompanyId, int licenseId)
        {
            UserCompanySettingObject setting = new UserCompanySettingObject()
            {
                DataTypeId = (int)SettingDataType.Date,
                DateSetting = value,
            };
            return UpdateInsertSetting(settingMainType, settingTypeId, setting, userId, actorCompanyId, licenseId);
        }

        #endregion

        #region Decimal

        public ActionResult UpdateInsertDecimalSettings(SettingMainType settingMainType, Dictionary<int, decimal> dict, int userId, int actorCompanyId, int licenseId)
        {
            if (dict.Count == 0)
                return new ActionResult(true);

            var convertedDict = new Dictionary<int, string>();
            foreach (var item in dict)
            {
                convertedDict.Add(item.Key, item.Value.ToString());
            }

            return UpdateInsertSettings(settingMainType, SettingDataType.Decimal, convertedDict, userId, actorCompanyId, licenseId);
        }

        public ActionResult UpdateInsertDecimalSetting(SettingMainType settingMainType, int settingTypeId, decimal value, int userId, int actorCompanyId, int licenseId)
        {
            UserCompanySettingObject setting = new UserCompanySettingObject()
            {
                DataTypeId = (int)SettingDataType.Decimal,
                DecimalSetting = value,
            };
            return UpdateInsertSetting(settingMainType, settingTypeId, setting, userId, actorCompanyId, licenseId);
        }

        #endregion

        #endregion

        #region UserFavorite

        public List<UserFavorite> GetUserFavorites(int userId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserFavorite.NoTracking();
            return GetUserFavorites(entities, userId, actorCompanyId);
        }

        public List<UserFavorite> GetUserFavorites(CompEntities entities, int userId, int actorCompanyId)
        {
            return (from uf in entities.UserFavorite
                    where uf.UserId == userId &&
                    uf.Company.ActorCompanyId == actorCompanyId
                    select uf).ToList();
        }

        public List<FavoriteItem> GetUserFavoriteItems(int userId)
        {
            List<FavoriteItem> favoriteItems = new List<FavoriteItem>();

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.UserFavorite.NoTracking();
            var query = from uf in entitiesReadOnly.UserFavorite
                        where uf.UserId == userId
                        orderby uf.Name
                        select new
                        {
                            uf.UserFavoriteId,
                            uf.Name,
                            uf.Url,
                            uf.IsDefault,
                            ActorCompanyId = uf.Company != null ? uf.Company.ActorCompanyId : 0,
                        };

            foreach (var item in query)
            {
                FavoriteItem favoriteItem = new FavoriteItem()
                {
                    FavoriteId = item.UserFavoriteId,
                    FavoriteName = item.Name,
                    FavoriteUrl = item.Url,
                    IsDefault = item.IsDefault,
                };

                if (item.ActorCompanyId > 0)
                    favoriteItem.FavoriteCompany = item.ActorCompanyId;

                favoriteItems.Add(favoriteItem);
            }

            return favoriteItems;
        }

        public List<FavoriteItem> GetFavoriteItemOptions()
        {
            List<FavoriteItem> items = new List<FavoriteItem>();

            items.Add(new FavoriteItem()
            {
                FavoriteName = GetText(6, "Ekonomi") + " - " + GetText(1798, "Leverantör"),
                FavoriteUrl = UrlUtil.GetSectionUrl(Constants.SOE_MODULE_ECONOMY, Constants.SOE_SECTION_SUPPLIER),
                FavoriteOption = SoeFavoriteOption.Economy_Supplier,
            });

            items.Add(new FavoriteItem()
            {
                FavoriteName = GetText(6, "Ekonomi") + " - " + GetText(1797, "Redovisning"),
                FavoriteUrl = UrlUtil.GetSectionUrl(Constants.SOE_MODULE_ECONOMY, Constants.SOE_SECTION_ACCOUNTING),
                FavoriteOption = SoeFavoriteOption.Economy_Accounting,
            });

            items.Add(new FavoriteItem()
            {
                FavoriteName = GetText(1829, "Försäljning") + " - " + GetText(5321, "Offert"),
                FavoriteUrl = UrlUtil.GetSectionUrl(Constants.SOE_MODULE_BILLING, Constants.SOE_SECTION_ORDER),
                FavoriteOption = SoeFavoriteOption.Billing_Offer,
            });

            items.Add(new FavoriteItem()
            {
                FavoriteName = GetText(1829, "Försäljning") + " - " + GetText(5327, "Order"),
                FavoriteUrl = UrlUtil.GetSectionUrl(Constants.SOE_MODULE_BILLING, Constants.SOE_SECTION_ORDER),
                FavoriteOption = SoeFavoriteOption.Billing_Order,
            });

            items.Add(new FavoriteItem()
            {
                FavoriteName = GetText(1829, "Försäljning") + " - " + GetText(1830, "Faktura"),
                FavoriteUrl = UrlUtil.GetSectionUrl(Constants.SOE_MODULE_BILLING, Constants.SOE_SECTION_INVOICE),
                FavoriteOption = SoeFavoriteOption.Billing_Invoice,
            });

            items.Add(new FavoriteItem()
            {
                FavoriteName = GetText(5002, "Personal") + " - " + GetText(5003, "Min tid"),
                FavoriteUrl = UrlUtil.GetSectionUrl(Constants.SOE_MODULE_TIME, Constants.SOE_SECTION_TIME, Constants.SOE_SECTION_ATTESTUSER),
                FavoriteOption = SoeFavoriteOption.Time_Process,
            });

            items.Add(new FavoriteItem()
            {
                FavoriteName = GetText(5002, "Personal") + " - " + GetText(8040, "Attestera tid"),
                FavoriteUrl = UrlUtil.GetSectionUrl(Constants.SOE_MODULE_TIME, Constants.SOE_SECTION_TIME, Constants.SOE_SECTION_ATTEST),
                FavoriteOption = SoeFavoriteOption.Time_Attest,
            });

            return items;
        }

        public Dictionary<int, string> GetFavoriteItemOptionsDict(bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<FavoriteItem> items = GetFavoriteItemOptions();
            foreach (FavoriteItem item in items)
            {
                dict.Add((int)item.FavoriteOption, item.FavoriteName);
            }

            return dict;
        }

        public UserFavorite GetUserFavorite(int userFavoriteId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserFavorite.NoTracking();
            return GetUserFavorite(entities, userFavoriteId, userId);
        }

        public UserFavorite GetUserFavorite(CompEntities entities, int userFavoriteId, int userId)
        {
            return (from uf in entities.UserFavorite
                    where uf.UserFavoriteId == userFavoriteId &&
                    uf.UserId == userId
                    select uf).FirstOrDefault();
        }

        public UserFavorite GetUserFavoriteDefault(int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserFavorite.NoTracking();
            return GetUserFavoriteDefault(entities, userId);
        }

        public UserFavorite GetUserFavoriteDefault(CompEntities entities, int userId)
        {
            return (from uf in entities.UserFavorite
                    where uf.IsDefault &&
                    uf.UserId == userId
                    select uf).FirstOrDefault();
        }

        public FavoriteItem GetFavoriteItemOptionFromRole(Role role)
        {
            FavoriteItem favoriteItem = null;
            if (role != null && role.FavoriteOption > 0)
                favoriteItem = GetFavoriteItemOption((SoeFavoriteOption)role.FavoriteOption);

            return favoriteItem;
        }

        public FavoriteItem GetFavoriteItemOption(SoeFavoriteOption favoriteOption)
        {
            List<FavoriteItem> items = GetFavoriteItemOptions();
            return items.FirstOrDefault(i => i.FavoriteOption == favoriteOption);
        }

        public ActionResult AddUserFavorite(UserFavorite userFavorite, int userId, int? actorCompanyId)
        {
            if (userFavorite == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "UserFavorite");

            using (CompEntities entities = new CompEntities())
            {
                userFavorite.User = UserManager.GetUser(entities, userId);
                if (userFavorite.User == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                userFavorite.Company = actorCompanyId.HasValue ? CompanyManager.GetCompany(entities, actorCompanyId.Value) : null;

                return AddEntityItem(entities, userFavorite, "UserFavorite");
            }
        }

        public ActionResult DeleteUserFavorite(int userId, int userFavoriteId)
        {
            using (CompEntities entities = new CompEntities())
            {
                UserFavorite userFavorite = GetUserFavorite(entities, userFavoriteId, userId);
                if (userFavorite == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "UserFavorite");

                return DeleteEntityItem(entities, userFavorite);
            }
        }

        public ActionResult DeleteUserFavoritesForCompany(int userId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<UserFavorite> userFavorites = SettingManager.GetUserFavorites(entities, userId, actorCompanyId);
                foreach (UserFavorite userFavorite in userFavorites)
                {
                    entities.DeleteObject(userFavorite);
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult UpdateUserFavorite(int userId, UserFavorite userFavorite)
        {
            if (userFavorite == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "UserFavorite");

            using (CompEntities entities = new CompEntities())
            {
                UserFavorite orginalUserFavorite = GetUserFavorite(entities, userFavorite.UserFavoriteId, userId);
                if (orginalUserFavorite == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "UserFavorite");

                return UpdateEntityItem(entities, orginalUserFavorite, userFavorite, "UserFavorite");
            }
        }

        #endregion

        #region GridState

        #region UserGridState

        public List<int> GetCompaniesWithUserStates(string grid)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.UserGridState.Where(i => i.Grid == grid).Select(i => i.ActorCompanyId).Distinct().ToList();
        }

        public List<UserGridState> GetUserGridStates(CompEntities entities, string grid, int actorCompanyId)
        {
            return (from g in entities.UserGridState
                    where g.ActorCompanyId == actorCompanyId &&
                    g.Grid == grid
                    select g).ToList();
        }

        public UserGridState GetUserGridState(CompEntities entities, string grid)
        {
            return (from g in entities.UserGridState
                    where g.ActorCompanyId == ActorCompanyId &&
                    g.UserId == UserId &&
                    g.Grid == grid
                    select g).FirstOrDefault();
        }

        public string GetUserGridStateValue(string grid)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserGridState.NoTracking();
            return GetUserGridState(entities, grid)?.GridState ?? GetSysGridStateValue(grid);
        }

        public ActionResult SaveUserGridState(string grid, string gridState)
        {
            if (String.IsNullOrEmpty(grid) || String.IsNullOrEmpty(gridState))
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            using (CompEntities entities = new CompEntities())
            {
                #region UserGridState

                // Get existing
                UserGridState userGridState = GetUserGridState(entities, grid);
                if (userGridState == null)
                {
                    #region Add

                    userGridState = new UserGridState()
                    {
                        ActorCompanyId = ActorCompanyId,
                        UserId = UserId,
                        Grid = grid
                    };
                    entities.UserGridState.AddObject(userGridState);

                    #endregion
                }
                else
                {
                    #region Update

                    // Set fields below

                    #endregion
                }

                userGridState.GridState = gridState;

                #endregion

                return SaveChanges(entities);
            }
        }

        public ActionResult DeleteUserGridState(string grid)
        {
            if (String.IsNullOrEmpty(grid))
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            using (CompEntities entities = new CompEntities())
            {
                // Get existing
                UserGridState userGridState = GetUserGridState(entities, grid);
                if (userGridState != null)
                    entities.DeleteObject(userGridState);

                return SaveChanges(entities);
            }
        }

        #endregion

        #region SysGridState

        public SysGridState GetSysGridState(string grid)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysGridState(sysEntitiesReadOnly, grid);
        }

        public SysGridState GetSysGridState(SOESysEntities entities, string grid)
        {
            return entities.SysGridState.FirstOrDefault(g => g.Grid == grid);
        }

        public string GetSysGridStateValue(string grid)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysGridState(sysEntitiesReadOnly, grid)?.GridState ?? string.Empty;
        }

        public ActionResult SaveSysGridState(string grid, string gridState, int userId)
        {
            if (String.IsNullOrEmpty(grid) || String.IsNullOrEmpty(gridState))
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            using (SOESysEntities entities = new SOESysEntities())
            {
                #region SysGridState

                // Get existing
                SysGridState sysGridState = GetSysGridState(entities, grid);
                if (sysGridState == null)
                {
                    #region Add

                    sysGridState = new SysGridState()
                    {
                        Grid = grid
                    };
                    entities.SysGridState.Add(sysGridState);

                    #endregion
                }
                else
                {
                    #region Update

                    // Set fields below

                    #endregion
                }

                sysGridState.GridState = gridState;

                #endregion

                ActionResult result = SaveChanges(entities);
                if (result.Success)
                    base.LogInfo(String.Format("SysGridState for grid {0} was modified by user {1}", grid, userId));

                return result;
            }
        }

        public ActionResult DeleteSysGridState(string grid, int userId)
        {
            if (String.IsNullOrEmpty(grid))
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            using (SOESysEntities entities = new SOESysEntities())
            {
                // Get existing
                SysGridState sysGridState = GetSysGridState(entities, grid);
                if (sysGridState != null)
                    entities.SysGridState.Remove(sysGridState);

                ActionResult result = SaveChanges(entities);
                if (result.Success)
                    base.LogInfo(String.Format("SysGridState for grid {0} was deleted by user {1}", grid, userId));

                return result;
            }
        }

        #endregion

        #region AgGridSetting

        public List<AgGridColumnSettingDTO> GetAgGridSettings(AgGridType type)
        {
            string grid = GetAgGridTypeName(type);
            return GetAgGridSettings(grid);
        }

        public List<AgGridColumnSettingDTO> GetAgGridSettings(string grid)
        {
            string gridState = GetUserGridStateValue(grid);
            return DeserializeGridState(gridState);
        }

        public List<AgGridColumnSettingDTO> DeserializeGridState(string gridState)
        {
            if (string.IsNullOrEmpty(gridState))
                return new List<AgGridColumnSettingDTO>();

            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            return (List<AgGridColumnSettingDTO>)jsSerializer.Deserialize(gridState, typeof(List<AgGridColumnSettingDTO>));
        }

        public string SerializeGridState(List<AgGridColumnSettingDTO> settings)
        {
            if (settings.IsNullOrEmpty())
                return null;

            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            return jsSerializer.Serialize(settings);
        }

        public string GetAgGridTypeName(AgGridType type)
        {
            switch (type)
            {
                case AgGridType.AttestEmployee:
                    return "Common_Directives_AttestEmployee";
                case AgGridType.AttestMyTime:
                    return "Common_Directives_AttestMyTime";
                case AgGridType.AttestGroup:
                    return "Common_Directives_AttestGroup";
                default:
                    return null;
            }
        }

        public (AgGridType gridType, string gridName) GetAgGridTypeName(int enumValue)
        {
            AgGridType gridType = AgGridType.Unknown;
            string gridName = string.Empty;

            if (enumValue > 0 && Enum.TryParse(enumValue.ToString(), out AgGridType enumType))
            {
                gridType = enumType;
                gridName = GetAgGridTypeName(enumType);
            }
            return (gridType, gridName);
        }

        public ActionResult FormatSysGridState(AgGridType type, string grid, out bool formatted)
        {
            formatted = false;

            using (SOESysEntities entities = new SOESysEntities())
            {
                SysGridState sysGridState = GetSysGridState(entities, grid);
                if (TryUpdateGridState(sysGridState, type))
                {
                    ActionResult result = SaveChanges(entities);
                    if (!result.Success)
                        return result;

                    formatted = true;
                }
            }

            return new ActionResult(true);
        }

        public ActionResult FormatUserGridState(AgGridType type, string grid, int actorCompanyId, out int total, out int formatted)
        {
            total = 0;
            formatted = 0;

            SysGridState sysGridState = GetSysGridState(grid);
            List<AgGridColumnSettingDTO> sysColumnSettings = sysGridState != null ? DeserializeGridState(sysGridState.GridState) : null;

            using (CompEntities entities = new CompEntities())
            {
                List<UserGridState> userGridStates = GetUserGridStates(entities, grid, actorCompanyId);
                total = userGridStates.Count;

                foreach (UserGridState userGridState in userGridStates)
                {
                    if (TryUpdateGridState(userGridState, type, sysColumnSettings))
                        formatted++;
                }

                return formatted > 0 ? SaveChanges(entities) : new ActionResult(true);
            }
        }

        private bool TryUpdateGridState<T>(T grid, AgGridType type, List<AgGridColumnSettingDTO> templateSettings = null) where T : IGridState
        {
            List<AgGridColumnSettingDTO> settings = DeserializeGridState(grid.GridState);
            if (settings.IsNullOrEmpty())
                return false;

            settings = TryFormatColumnSettings(type, settings, templateSettings);
            if (settings.IsNullOrEmpty())
                return false;

            string formattedGridState = SerializeGridState(settings);
            if (string.IsNullOrEmpty(formattedGridState) || formattedGridState.Equals(grid.GridState))
                return false;

            grid.GridState = formattedGridState;
            return true;
        }

        private List<AgGridColumnSettingDTO> TryFormatColumnSettings(AgGridType type, List<AgGridColumnSettingDTO> settings, List<AgGridColumnSettingDTO> templateSettings = null)
        {
            switch (type)
            {
                case AgGridType.AttestMyTime:
                case AgGridType.AttestEmployee:
                    return GetValidColumnSettingsForTimeAttest(settings, templateSettings, isMyTime: type == AgGridType.AttestMyTime);
                case AgGridType.AttestGroup:
                    return new List<AgGridColumnSettingDTO>(); //No formatting needed
                default:
                    return new List<AgGridColumnSettingDTO>(); //No formatting supported
            }
        }

        private List<AgGridColumnSettingDTO> GetValidColumnSettingsForTimeAttest(List<AgGridColumnSettingDTO> settings, List<AgGridColumnSettingDTO> templateSettings = null, bool isMyTime = false)
        {
            List<AgGridColumnSettingDTO> validSettings = new List<AgGridColumnSettingDTO>();

            if (settings.IsNullOrEmpty())
                return validSettings;

            List<string> pinnedCols = templateSettings?.Where(i => i.pinned != null).Select(i => i.colId).ToList() ?? new List<string>();

            AddPrecedingSystemSettings();
            AddFixedSetting(AgGridTimeAttestEmployee.day);
            AddFixedSetting(AgGridTimeAttestEmployee.date);
            AddFixedSetting(AgGridTimeAttestEmployee.dayName);
            AddFixedSetting(AgGridTimeAttestEmployee.weekNr);
            AddFixedSetting(AgGridTimeAttestEmployee.attestStateColor, true);
            AddFixedSetting(AgGridTimeAttestEmployee.attestStateName);
            AddFixedSetting(AgGridTimeAttestEmployee.workedInsideScheduleColor, true);
            AddFixedSetting(AgGridTimeAttestEmployee.workedOutsideScheduleColor, true);
            AddFixedSetting(AgGridTimeAttestEmployee.absenceTimeColor, true);
            AddFixedSetting(AgGridTimeAttestEmployee.standbyTimeColor, true);
            AddFixedSetting(AgGridTimeAttestEmployee.expenseColor, true);
            AddOptionalSettings();
            AddMissingSettings();
            AddPinnedSettings();
            AddAppendingSystemSettings();

            return validSettings;

            void AddPrecedingSystemSettings()
            {
                AddSettingFromColId(AgGridColumnSettingDTO.SOE_ROW_SELECTION);
                AddSettingFromColId(AgGridColumnSettingDTO.SOE_AG_SINGLE_VALUE_COLUMN);
            }
            void AddAppendingSystemSettings()
            {
                AddSettingFromColId(AgGridColumnSettingDTO.SOE_GRID_MENU_COLUMN);
            }
            void AddFixedSetting(AgGridTimeAttestEmployee column, bool createIfNotExists = false)
            {
                string colId = GetColId(column);
                if (!string.IsNullOrEmpty(colId))
                    AddOrCreateSetting(colId, createIfNotExists, clearPinned: true);
            }
            void AddOptionalSettings()
            {
                settings.ForEach(setting => AddSetting(setting, skipPinned: true));
            }
            void AddMissingSettings()
            {
                List<string> definedColIds = Enum.GetNames(typeof(AgGridTimeAttestEmployee)).ToList();
                List<string> userColIds = settings.Where(c => !c.IsSystemColumn()).Select(i => i.colId).ToList();
                List<string> missingColIds = definedColIds.Except(userColIds).ToList();
                if (missingColIds.Any())
                    missingColIds.ForEach(colId => AddSetting(CreateSetting(colId, hide: true), skipSystem: true, skipPinned: true));
            }
            void AddPinnedSettings()
            {
                pinnedCols.ForEach(colId => AddOrCreateSetting(colId, createIfNotExists: true, skipSystem: true));
            }
            void AddSettingFromColId(string colId)
            {
                AddSetting(settings.Get(colId));
            }
            void AddSetting(AgGridColumnSettingDTO setting, bool skipSystem = false, bool skipPinned = false)
            {
                if (setting == null)
                    return;
                if (skipSystem && setting.IsSystemColumn())
                    return;
                if (skipPinned && pinnedCols.Contains(setting.colId))
                    return;
                if (isMyTime && DoHideColumnForMyTime(setting.colId))
                    return;
                if (!validSettings.Any(s => s.colId == setting.colId))
                    validSettings.Add(setting);
            }
            void AddOrCreateSetting(string colId, bool createIfNotExists = false, bool skipSystem = false, bool skipPinned = false, bool clearPinned = false)
            {
                AgGridColumnSettingDTO setting = settings.Get(colId);
                if (clearPinned && !string.IsNullOrEmpty(setting?.pinned))
                    setting.pinned = null;
                if (setting == null && createIfNotExists)
                    setting = CreateSetting(colId);
                if (setting != null)
                    AddSetting(setting, skipSystem, skipPinned);
            }
            AgGridColumnSettingDTO CreateSetting(string colId, bool hide = false)
            {
                AgGridColumnSettingDTO templateSetting = templateSettings.Get(colId);
                return new AgGridColumnSettingDTO
                {
                    colId = colId,
                    hide = hide,
                    width = templateSetting?.width,
                    pinned = templateSetting?.pinned,
                    aggFunc = templateSetting?.aggFunc,
                    rowGroupIndex = templateSetting?.rowGroupIndex,
                    pivotIndex = templateSetting?.pivotIndex,
                };
            }
            string GetColId(AgGridTimeAttestEmployee column)
            {
                try
                {
                    return Enum.GetName(typeof(AgGridTimeAttestEmployee), column);
                }
                catch (Exception ex)
                {
                    LogError(ex, this.log);
                    return string.Empty;
                }
            }
            bool DoHideColumnForMyTime(string colId)
            {
                return colId == "isPreliminary";
            }
        }

        #endregion

        #endregion

        #region SysParameters

        public Dictionary<string, string> GetSysParameters(int parameterTypeId, bool addEmptyRow, bool addValue = false)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (addEmptyRow) dict.Add(" ", " ");

            List<SysParameter> sysParameters = GetSysParameters(parameterTypeId);
            foreach (var sysParameter in sysParameters.OrderBy(e => e.Name))
            {
                if (!dict.ContainsKey(sysParameter.Code))
                {
                    if (addValue)
                        dict.Add(sysParameter.Code, $"{sysParameter.Name} - {sysParameter.Code}");
                    else
                        dict.Add(sysParameter.Code, sysParameter.Name);
                }
            }
            return dict;
        }

        public List<SysParameter> GetSysParameters(int parameterTypeId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysParameter
                             .ToList();
        }

        #endregion

        #region SystemInfoSetting

        public SystemInfoSetting GetSystemInfoSetting(int systemInfoSettingId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SystemInfoSetting.NoTracking();
            return GetSystemInfoSetting(entities, systemInfoSettingId);
        }

        public SystemInfoSetting GetSystemInfoSetting(CompEntities entities, int systemInfoSettingId)
        {
            return (from sis in entities.SystemInfoSetting
                    where sis.SystemInfoSettingId == systemInfoSettingId
                    select sis).FirstOrDefault<SystemInfoSetting>();
        }

        public SystemInfoSetting GetSystemInfoSetting(int type, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SystemInfoSetting.NoTracking();
            return GetSystemInfoSetting(entities, type, actorCompanyId);
        }

        public SystemInfoSetting GetSystemInfoSetting(CompEntities entities, int type, int actorCompanyId)
        {
            return (from sis in entities.SystemInfoSetting
                    where sis.Type == type && sis.ActorCompanyId == actorCompanyId
                    select sis).FirstOrDefault<SystemInfoSetting>();
        }

        public List<SystemInfoSetting> GetSystemInfoSettings(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SystemInfoSetting.NoTracking();
            return GetSystemInfoSettings(entities, actorCompanyId);
        }

        public List<SystemInfoSetting> GetSystemInfoSettings(CompEntities entities, int actorCompanyId)
        {
            return (from sis in entities.SystemInfoSetting
                    where sis.ActorCompanyId == actorCompanyId
                    select sis).ToList();
        }

        public List<int> GetCompanyIdsWithBoolInfoSetting(SystemInfoType infoType, bool value = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserCompanySetting.NoTracking();
            return (from s in entities.SystemInfoSetting
                    where s.Type == (int)infoType &&
                    s.BoolData.HasValue &&
                    s.BoolData == value
                    select s.ActorCompanyId).ToList();
        }

        public ActionResult AddUpdateSystemInfoSettings(Dictionary<int, object> dict, int settingDataType, int actorCompanyId)
        {
            if (dict == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SystemInfoSetting");

            using (CompEntities entities = new CompEntities())
            {
                foreach (var pair in dict)
                {
                    bool settingExist = true;

                    SystemInfoSetting setting = GetSystemInfoSetting(entities, pair.Key, actorCompanyId);

                    if (setting == null)
                    {
                        settingExist = false;

                        setting = new SystemInfoSetting()
                        {
                            Type = pair.Key,
                            DataType = settingDataType,
                            ActorCompanyId = actorCompanyId,
                            Name = "",
                        };
                        SetCreatedProperties(setting);
                        entities.SystemInfoSetting.AddObject(setting);
                    }

                    switch (settingDataType)
                    {
                        case (int)SettingDataType.String:
                            #region String

                            string strValue = pair.Value.ToString();
                            if (settingExist && setting.StrData == strValue)
                                continue;

                            setting.StrData = strValue;

                            #endregion
                            break;
                        case (int)SettingDataType.Integer:
                            #region Integer

                            int? intValue = null;
                            intValue = String.IsNullOrEmpty(pair.Value.ToString()) ? intValue : Convert.ToInt32(pair.Value);
                            if (settingExist && setting.IntData == intValue)
                                continue;

                            setting.IntData = intValue;

                            #endregion
                            break;
                        case (int)SettingDataType.Boolean:
                            #region Boolean

                            bool? boolValue = null;
                            boolValue = String.IsNullOrEmpty(pair.Value.ToString()) ? boolValue : Convert.ToBoolean(pair.Value);
                            if (settingExist && setting.BoolData == boolValue)
                                continue;

                            setting.BoolData = boolValue;

                            #endregion
                            break;
                        case (int)SettingDataType.Date:
                        case (int)SettingDataType.Time:
                            #region Date / Time

                            DateTime? dateValue = null;
                            dateValue = String.IsNullOrEmpty(pair.Value.ToString()) ? dateValue : Convert.ToDateTime(pair.Value);
                            if (settingExist && setting.DateData == dateValue)
                                continue;

                            setting.DateData = dateValue;

                            #endregion
                            break;
                        case (int)SettingDataType.Decimal:
                            #region Decimal

                            Decimal? decimalValue = null;
                            decimalValue = String.IsNullOrEmpty(pair.Value.ToString()) ? decimalValue : Convert.ToDecimal(pair.Value);
                            if (settingExist && setting.DecimalData == decimalValue)
                                continue;

                            setting.DecimalData = decimalValue;

                            #endregion
                            break;
                        default:
                            continue;
                    }

                    SetModifiedProperties(setting);

                }

                return SaveChanges(entities);
            }
        }

        #endregion
    }
}
