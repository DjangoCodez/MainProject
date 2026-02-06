using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class BatchUpdateManager : ManagerBase
    {
        #region Ctor

        public BatchUpdateManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Entry-point
        public List<BatchUpdateDTO> GetBatchUpdate(SoeEntityType entityType)
        {
            using (CompEntities entities = new CompEntities())
            {
                var batchUpdateData = new List<BatchUpdateDTO>();
                switch (entityType)
                {
                    case SoeEntityType.Account:
                        batchUpdateData = GetAccountStdBatchUpdate(entities);
                        break;
                    case SoeEntityType.Customer:
                        batchUpdateData = GetCustomerBatchUpdate(entities);
                        break;
                    case SoeEntityType.Employee:
                        batchUpdateData = GetEmployeeBatchUpdate();
                        break;
                    case SoeEntityType.InvoiceProduct:
                        batchUpdateData = GetInvoiceProductBatchUpdate(entities);
                        break;
                    case SoeEntityType.PayrollProduct:
                        batchUpdateData = GetPayrollProductBatchUpdate();
                        break;
                    case SoeEntityType.Supplier:
                        batchUpdateData = GetSupplierBatchUpdate(entities);
                        break;
                }
                return batchUpdateData
                    .OrderBy(r => r.Label)
                    .ToList();
            }
        }
        public List<SmallGenericType> GetBatchUpdateFilterOptions(SoeEntityType entityType)
        {
            List<SmallGenericType> filterOptions = new List<SmallGenericType>();
            switch (entityType)
            {
                case SoeEntityType.PayrollProduct:
                    filterOptions.Add(new SmallGenericType(-1, GetText(4371, "Uppdatera existerande löneavtal")));
                    filterOptions.Add(new SmallGenericType(0, GetText(4366, "Alla")));
                    filterOptions.AddRange(PayrollManager.GetPayrollGroups(base.ActorCompanyId).ToSmallGenericTypes());
                    break;
            }
            return filterOptions;
        }
        public BatchUpdateDTO RefreshBatchUpdateOptions(SoeEntityType entityType, BatchUpdateDTO batchUpdate)
        {
            switch (entityType)
            {
                case SoeEntityType.Employee:
                    return RefreshEmployeeBatchUpdateOptions(batchUpdate);
                case SoeEntityType.PayrollProduct:
                    return RefreshPayrollProductBatchUpdateOptions(batchUpdate);
                default:
                    return batchUpdate;
            }
        }
        public ActionResult PerformBatchUpdate(SoeEntityType entityType, List<BatchUpdateDTO> batchUpdateDTOs, List<int> ids, List<int> filterIds = null)
        {
            var result = new ActionResult();

            if (batchUpdateDTOs.IsNullOrEmpty() || ids.IsNullOrEmpty())
                return result;

            using (var entities = new CompEntities())
            {
                bool saveChanges = true;

                entities.Connection.Open();
                switch (entityType)
                {
                    case SoeEntityType.Account:
                        result = PerformAccountBatchUpdate(entities, batchUpdateDTOs, ids);
                        break;
                    case SoeEntityType.Customer:
                        result = PerformCustomerBatchUpdate(entities, batchUpdateDTOs, ids);
                        break;
                    case SoeEntityType.Employee:
                        result = PerformEmployeeBatchUpdate(batchUpdateDTOs, ids);
                        saveChanges = false;
                        break;
                    case SoeEntityType.InvoiceProduct:
                        result = PerformInvoiceProductBatchUpdate(entities, batchUpdateDTOs, ids);
                        break;
                    case SoeEntityType.PayrollProduct:
                        result = PerformPayrollProductBatchUpdate(entities, batchUpdateDTOs, ids, filterIds);
                        break;
                    case SoeEntityType.Supplier:
                        result = PerformSupplierBatchUpdate(entities, batchUpdateDTOs, ids);
                        break;
                }

                if (result.Success && saveChanges)
                    result = SaveChanges(entities);
            }

            return result;
        }
        #endregion

        #region AccountStd
        public List<BatchUpdateDTO> GetAccountStdBatchUpdate(CompEntities entities)
        {
            var batchUpdateData = new List<BatchUpdateDTO>()
            {
                GetYesNoAlternative((int)BatchUpdateAccountStd.Active, 3273),
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateAccountStd.AccountType,
                    Label = GetText(1044, "Kontotyp"),
                    Options = GetTermGroup(TermGroup.AccountType, false)
                },
                GetYesNoAlternative((int)BatchUpdateAccountStd.IsAccrualAccount, 9345),
                GetYesNoAlternative((int)BatchUpdateAccountStd.ExcludeVatVerification, 9272),
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateAccountStd.AmountStop,
                    Label = GetText(2069, "Stanna i"),
                    Options = GetTermGroup(TermGroup.AmountStop, false)
                },
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.String,
                    Field = (int)BatchUpdateAccountStd.AccountUnit,
                    Label = GetText(1881, "Enhet"),
                },
                GetYesNoAlternative((int)BatchUpdateAccountStd.AccountUnitStop, 2071),
                GetYesNoAlternative((int)BatchUpdateAccountStd.AccountTextStop, 1272),
            };
            AddSysVatAccount(batchUpdateData);
            AddSruCodes(batchUpdateData);
            AddDimOptions(entities, batchUpdateData);

            return batchUpdateData;
        }
        public ActionResult PerformAccountBatchUpdate(CompEntities entities, List<BatchUpdateDTO> batchUpdateDTOs, List<int> accountIds)
        {

            var result = new ActionResult();

            if (!FeatureManager.HasRolePermission(Feature.Economy_Accounting_Accounts_BatchUpdate, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId))
            {
                result.Success = false;
                return result;
            }

            int actorCompanyId = base.ActorCompanyId;

            AccountInclude(batchUpdateDTOs, out bool loadAccountSru, out bool loadAccountStd, out bool loadAccountMapping);

            var accounts = AccountManager.GetAccountsByCompany(entities, actorCompanyId, false, false, loadAccountSru || loadAccountStd, false, loadAccountMapping, includeInactive: true, ids: accountIds);
            var dimMap = LoadDimMap(entities, loadAccountMapping);

            void UpdateSruCode(Account account, BatchUpdateAccountStd type, int value)
            {
                if (account == null || account.AccountStd == null || account.AccountStd.AccountSru == null) return;

                int n = type == BatchUpdateAccountStd.SruCode1 ? 1 : 2;
                int idx = n - 1;
                bool delete = value == 0;
                bool update = account.AccountStd.AccountSru.Count >= n;

                if (!update)
                {
                    //add new, but cannot delete...
                    if (delete) return;

                    var newSru = new AccountSru()
                    {
                        SysAccountSruCodeId = value,
                    };
                    account.AccountStd.AccountSru.Add(newSru);
                    return;
                }

                int i = 0;
                foreach (var sru in account.AccountStd.AccountSru.ToList())
                {
                    if (idx == i)
                    {
                        if (delete)
                        {
                            entities.DeleteObject(sru);
                        }
                        else if (update)
                        {
                            sru.SysAccountSruCodeId = value;
                        }
                    }
                    i++;
                }
            }

            void UpdateAccountMapping(Account account, BatchUpdateAccountStd type, int value, bool setNavType, bool setDefault)
            {
                if (!dimMap.TryGetValue(type, out int dimId))
                    return; //No dimid is required to continue

                if (account == null || account.AccountMapping == null)
                    return;
                var mapping = account.AccountMapping.FirstOrDefault(m => m.AccountDimId == dimId);

                if (mapping == null)
                {
                    mapping = new AccountMapping()
                    {
                        AccountDimId = dimId,
                    };
                    account.AccountMapping.Add(mapping);
                    SetCreatedProperties(mapping);
                }
                else
                {
                    SetModifiedProperties(mapping);
                }

                if (setNavType)
                    mapping.MandatoryLevel = value;
                if (setDefault)
                    mapping.DefaultAccountId = NotZeroOrNull(value);
            }

            foreach (var account in accounts)
            {
                foreach (var dto in batchUpdateDTOs)
                {
                    var type = (BatchUpdateAccountStd)dto.Field;
                    switch (type)
                    {
                        case BatchUpdateAccountStd.Active:
                            account.State = GetBoolFromYesNo(dto.IntValue) ? (int)SoeEntityState.Active : (int)SoeEntityState.Inactive;
                            break;
                        case BatchUpdateAccountStd.AccountType:
                            if (dto.IntValue == 0) continue;
                            account.AccountStd.AccountTypeSysTermId = dto.IntValue;
                            break;
                        case BatchUpdateAccountStd.SysVatAccount:
                            account.AccountStd.SysVatAccountId = NotZeroOrNull(dto.IntValue);
                            break;
                        case BatchUpdateAccountStd.IsAccrualAccount:
                            account.AccountStd.isAccrualAccount = GetBoolFromYesNo(dto.IntValue);
                            break;
                        case BatchUpdateAccountStd.ExcludeVatVerification:
                            account.AccountStd.ExcludeVatVerification = GetBoolFromYesNo(dto.IntValue);
                            break;
                        case BatchUpdateAccountStd.AmountStop:
                            account.AccountStd.AmountStop = dto.IntValue;
                            break;
                        case BatchUpdateAccountStd.AccountUnit:
                            account.AccountStd.Unit = dto.StringValue;
                            break;
                        case BatchUpdateAccountStd.AccountUnitStop:
                            account.AccountStd.UnitStop = GetBoolFromYesNo(dto.IntValue);
                            break;
                        case BatchUpdateAccountStd.AccountTextStop:
                            account.AccountStd.RowTextStop = GetBoolFromYesNo(dto.IntValue);
                            break;
                        case BatchUpdateAccountStd.SruCode1:
                        case BatchUpdateAccountStd.SruCode2:
                            UpdateSruCode(account, type, dto.IntValue);
                            break;
                        case BatchUpdateAccountStd.AccountDim1Default:
                        case BatchUpdateAccountStd.AccountDim2Default:
                        case BatchUpdateAccountStd.AccountDim3Default:
                        case BatchUpdateAccountStd.AccountDim4Default:
                        case BatchUpdateAccountStd.AccountDim5Default:
                        case BatchUpdateAccountStd.AccountDim6Default:
                            UpdateAccountMapping(account, type, dto.IntValue, false, true);
                            break;
                        case BatchUpdateAccountStd.AccountDim1NavigationType:
                        case BatchUpdateAccountStd.AccountDim2NavigationType:
                        case BatchUpdateAccountStd.AccountDim3NavigationType:
                        case BatchUpdateAccountStd.AccountDim4NavigationType:
                        case BatchUpdateAccountStd.AccountDim5NavigationType:
                        case BatchUpdateAccountStd.AccountDim6NavigationType:
                            UpdateAccountMapping(account, type, dto.IntValue, true, false);
                            break;
                    }
                }
                SetModifiedProperties(account);
            }
            return result;
        }
        public Dictionary<BatchUpdateAccountStd, int> LoadDimMap(CompEntities entities, bool loadAccountMapping)
        {
            var dimByType = new Dictionary<BatchUpdateAccountStd, int>();
            if (!loadAccountMapping) return dimByType;

            var dims = GetAccountDims(entities);

            var defaultDim = (int)BatchUpdateAccountStd.AccountDim1Default;
            var navType = (int)BatchUpdateAccountStd.AccountDim1NavigationType;
            foreach (var dim in dims)
            {
                dimByType[(BatchUpdateAccountStd)defaultDim] = dim.AccountDimId;
                dimByType[(BatchUpdateAccountStd)navType] = dim.AccountDimId;

                defaultDim += 2;
                navType += 2;
            }
            return dimByType;
        }
        public void AccountInclude(List<BatchUpdateDTO> batchUpdateDTOs, out bool loadAccountSru, out bool loadAccountStd, out bool loadAccountMapping)
        {
            loadAccountSru = false;
            loadAccountStd = false;
            loadAccountMapping = false;

            BatchUpdateAccountStd[] accountSruTypes =
            {
                BatchUpdateAccountStd.SruCode1,
                BatchUpdateAccountStd.SruCode2
            };
            BatchUpdateAccountStd[] accountStdTypes =
            {
                BatchUpdateAccountStd.SysVatAccount,
                BatchUpdateAccountStd.AccountType,
                BatchUpdateAccountStd.AmountStop,
                BatchUpdateAccountStd.AccountUnit,
                BatchUpdateAccountStd.AccountUnitStop,
                BatchUpdateAccountStd.AccountTextStop,
                BatchUpdateAccountStd.IsAccrualAccount
            };
            BatchUpdateAccountStd[] accountMappingTypes =
            {
                BatchUpdateAccountStd.AccountDim1Default,
                BatchUpdateAccountStd.AccountDim1NavigationType,
                BatchUpdateAccountStd.AccountDim2Default,
                BatchUpdateAccountStd.AccountDim2NavigationType,
                BatchUpdateAccountStd.AccountDim3Default,
                BatchUpdateAccountStd.AccountDim3NavigationType,
                BatchUpdateAccountStd.AccountDim4Default,
                BatchUpdateAccountStd.AccountDim4NavigationType,
                BatchUpdateAccountStd.AccountDim5Default,
                BatchUpdateAccountStd.AccountDim5NavigationType,
                BatchUpdateAccountStd.AccountDim6Default,
                BatchUpdateAccountStd.AccountDim6NavigationType,
            };

            foreach (var dto in batchUpdateDTOs)
            {
                var type = (BatchUpdateAccountStd)dto.Field;
                if (accountSruTypes.Contains(type))
                    loadAccountSru = true;
                else if (accountStdTypes.Contains(type))
                    loadAccountStd = true;
                else if (accountMappingTypes.Contains(type))
                    loadAccountMapping = true;
            }
        }
        private void AddDimOptions(CompEntities entities, List<BatchUpdateDTO> batchRows)
        {
            string defaultName = GetText(1130, "Standard");
            string navigationName = GetText(9346, "Navigationstyp");
            var dims = GetAccountDims(entities);

            var batchUpdateMapping = new List<(BatchUpdateAccountStd, BatchUpdateAccountStd)>()
            {
                (BatchUpdateAccountStd.AccountDim1Default, BatchUpdateAccountStd.AccountDim1NavigationType),
                (BatchUpdateAccountStd.AccountDim2Default, BatchUpdateAccountStd.AccountDim2NavigationType),
                (BatchUpdateAccountStd.AccountDim3Default, BatchUpdateAccountStd.AccountDim3NavigationType),
                (BatchUpdateAccountStd.AccountDim4Default, BatchUpdateAccountStd.AccountDim4NavigationType),
                (BatchUpdateAccountStd.AccountDim5Default, BatchUpdateAccountStd.AccountDim5NavigationType),
                (BatchUpdateAccountStd.AccountDim6Default, BatchUpdateAccountStd.AccountDim6NavigationType),
            };

            int dimCount = 0;
            foreach (var dim in dims)
            {
                var mapping = batchUpdateMapping[dimCount];
                var defaultValue = new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)mapping.Item1,
                    Label = $"{dim.Name} - {defaultName}",
                    Options = dim.Account.Select(a => new NameAndIdDTO
                    {
                        Id = a.AccountId,
                        Name = $"{a.AccountNr} - {a.Name}",
                    })
                    .ToList(),
                };
                batchRows.Add(defaultValue);

                var navigationValue = new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)mapping.Item2,
                    Label = $"{dim.Name} - {navigationName}",
                    Options = GetTermGroup(TermGroup.AccountMandatoryLevel)
                };
                batchRows.Add(navigationValue);

                dimCount++;
            }
        }
        private void AddSysVatAccount(List<BatchUpdateDTO> batchRows)
        {
            int countryId = CompanyManager.GetCompanySysCountryId(this.ActorCompanyId);
            var sruCodesRaw = AccountManager.GetSysVatAccounts(false)
                    .Where(a => a.LangId == countryId)
                    .ToList();

            var sruCodes = new List<NameAndIdDTO>();

            foreach (var sruCode in sruCodesRaw)
            {
                string desc = sruCode.VatNr1 != null ? sruCode.VatNr1.ToString() : "";
                if (sruCode.VatNr2 != null)
                {
                    desc += "+";
                    desc += sruCode.VatNr2;
                }
                if (!String.IsNullOrEmpty(desc))
                    desc += ". ";
                desc += sruCode.Name;
                sruCodes.Add(new NameAndIdDTO() { Id = sruCode.SysVatAccountId, Name = desc });
            }

            sruCodes = sruCodes.OrderBy(r => r.Name).ToList();

            var batchUpdateDto = new BatchUpdateDTO()
            {
                DataType = BatchUpdateFieldType.Id,
                Field = (int)BatchUpdateAccountStd.SysVatAccount,
                Label = GetText(2067, "Momsredovisning"),
                Options = sruCodes,
            };

            batchRows.Add(batchUpdateDto);
        }
        private void AddSruCodes(List<BatchUpdateDTO> batchRows)
        {
            var sruCodes = AccountManager.GetSysAccountSruCodes()
                .Select(t => new NameAndIdDTO
                {
                    Id = t.SysAccountSruCodeId,
                    Name = $"{t.SruCode} - {t.Name}",
                })
                .ToList();

            var sruCode1 = new BatchUpdateDTO()
            {
                DataType = BatchUpdateFieldType.Id,
                Field = (int)BatchUpdateAccountStd.SruCode1,
                Label = GetText(1265, "SRU-kod 1"),
                Options = sruCodes,
            };
            batchRows.Add(sruCode1);

            var sruCode2 = new BatchUpdateDTO()
            {
                DataType = BatchUpdateFieldType.Id,
                Field = (int)BatchUpdateAccountStd.SruCode2,
                Label = GetText(1266, "SRU-kod 2"),
                Options = sruCodes,
            };
            batchRows.Add(sruCode2);
        }
        private List<AccountDim> GetAccountDims(CompEntities entities)
        {
            return AccountManager.GetAccountDimsByCompany(entities, base.ActorCompanyId, onlyInternal: true, loadInternalAccounts: true)
                .Where(d => d.AccountDimNr > 1)
                .OrderBy(d => d.AccountDimNr)
                .Take(6)
                .ToList();
        }
        #endregion

        #region Customer
        public List<BatchUpdateDTO> GetCustomerBatchUpdate(CompEntities entities)
        {
            var batchUpdateData = new List<BatchUpdateDTO>()
            {
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateCustomer.VatType,
                    Label = GetText(3520, "Momstyp"),
                    Options = GetTermGroup(TermGroup.InvoiceVatType, true)
                },
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateCustomer.DefaultPricelist,
                    Label = GetText(6414, "Standardprislista"),
                    Options = ProductPricelistManager.GetPriceListTypes(entities, this.ActorCompanyId)
                        .Select(t => new NameAndIdDTO
                        {
                            Id = t.PriceListTypeId,
                            Name = t.Name
                        })
                        .ToList(),
                },
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateCustomer.DefaultWholeseller,
                    Label = GetText(7011, "Standardgrossist"),
                    Options = WholeSellerManager.GetSysWholesellersByCompany(entities, this.ActorCompanyId)
                        .Select(t => new NameAndIdDTO
                        {
                            Id = t.SysWholesellerId,
                            Name = t.Name
                        })
                        .ToList(),
                },
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Decimal,
                    Field = (int)BatchUpdateCustomer.DiscountMerchandise,
                    Label = GetText(4106, "Rabatt varor %"),
                },
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Decimal,
                    Field = (int)BatchUpdateCustomer.DiscountService,
                    Label = GetText(3377, "Rabatt tjänster %"),
                },
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Integer,
                    Field = (int)BatchUpdateCustomer.CreditLimit,
                    Label = GetText(6415, "Kreditgräns"),
                },
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.String,
                    Field = (int)BatchUpdateCustomer.InvoiceReference,
                    Label = GetText(3289, "Fakturareferens"),
                },
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateCustomer.InvoiceDeliveryType,
                    Label = GetText(6416, "Fakturametod"),
                    Options = GetTermGroup(TermGroup.InvoiceDeliveryType, true)
                },
                GetYesNoAlternative((int)BatchUpdateCustomer.DisableInvoiceFee, 6417),
                GetYesNoAlternative((int)BatchUpdateCustomer.AddAttachmentsToEInvoice, 6418),
                GetYesNoAlternative((int)BatchUpdateCustomer.ShowNote, 3714),
                GetYesNoAlternative((int)BatchUpdateCustomer.AddSupplierInvoicesToEInvoice, 6419),
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.String,
                    Field = (int)BatchUpdateCustomer.InvoiceLabel,
                    Label = GetText(9117, "Märkning"),
                },
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateCustomer.PaymentCondition,
                    Label = GetText(3081, "Betalningsvillkor"),
                    Options = PaymentManager.GetPaymentConditions(entities, this.ActorCompanyId)
                        .Select(t => new NameAndIdDTO
                        {
                            Id = t.PaymentConditionId,
                            Name = t.Name
                        })
                        .ToList(),
                },
                GetYesNoAlternative((int)BatchUpdateCustomer.Active, 3273),
                GetYesNoAlternative((int)BatchUpdateCustomer.ImportInvoicesDetailed, 8017),
            };
            return batchUpdateData;
        }
        public ActionResult PerformCustomerBatchUpdate(CompEntities entities, List<BatchUpdateDTO> batchUpdateDTOs, List<int> customerIds)
        {
            var result = new ActionResult();

            int actorCompanyId = base.ActorCompanyId;
            IQueryable<Customer> query = entities.Customer;
            var customers = query.Where(c => c.ActorCompanyId == actorCompanyId && customerIds.Contains(c.ActorCustomerId)).ToList();

            foreach (var customer in customers)
            {
                foreach (var field in batchUpdateDTOs)
                {
                    switch ((BatchUpdateCustomer)field.Field)
                    {
                        case BatchUpdateCustomer.VatType:
                            customer.VatType = field.IntValue;
                            break;
                        case BatchUpdateCustomer.DefaultPricelist:
                            customer.PriceListTypeId = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateCustomer.DefaultWholeseller:
                            customer.SysWholeSellerId = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateCustomer.DiscountMerchandise:
                            customer.DiscountMerchandise = field.DecimalValue;
                            break;
                        case BatchUpdateCustomer.DiscountService:
                            customer.DiscountService = field.DecimalValue;
                            break;
                        case BatchUpdateCustomer.CreditLimit:
                            customer.CreditLimit = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateCustomer.InvoiceReference:
                            customer.InvoiceReference = field.StringValue;
                            break;
                        case BatchUpdateCustomer.InvoiceDeliveryType:
                            customer.InvoiceDeliveryType = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateCustomer.DisableInvoiceFee:
                            customer.DisableInvoiceFee = GetBoolFromYesNo(field.IntValue);
                            break;
                        case BatchUpdateCustomer.AddAttachmentsToEInvoice:
                            customer.AddAttachementsToEInvoice = GetBoolFromYesNo(field.IntValue);
                            break;
                        case BatchUpdateCustomer.ShowNote:
                            customer.ShowNote = GetBoolFromYesNo(field.IntValue);
                            break;
                        case BatchUpdateCustomer.AddSupplierInvoicesToEInvoice:
                            customer.AddSupplierInvoicesToEInvoices = GetBoolFromYesNo(field.IntValue);
                            break;
                        case BatchUpdateCustomer.InvoiceLabel:
                            customer.InvoiceLabel = field.StringValue;
                            break;
                        case BatchUpdateCustomer.PaymentCondition:
                            customer.PaymentConditionId = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateCustomer.Active:
                            customer.State = GetBoolFromYesNo(field.IntValue) ? (int)SoeEntityState.Active : (int)SoeEntityState.Inactive;
                            break;
                        case BatchUpdateCustomer.ImportInvoicesDetailed:
                            customer.ImportInvoicesDetailed = GetBoolFromYesNo(field.IntValue);
                            break;
                    }
                }
                SetModifiedProperties(customer);
            }

            return result;
        }
        #endregion

        #region Employee
        public List<BatchUpdateDTO> GetEmployeeBatchUpdate()
        {
            var batchUpdateData = new List<BatchUpdateDTO>();
            int langId = GetLangId();

            //Employee
            AddEmployeeField(BatchUpdateEmployee.Active);
            AddEmployeeField(BatchUpdateEmployee.ExternalCode);
            if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId))
                AddEmployeeField(BatchUpdateEmployee.EmploymentPriceType, doShowFromDate: true);
            AddEmployeeField(BatchUpdateEmployee.HierarchicalAccount, doShowFromDate: true, doShowToDate: true);
            if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Skills, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId))
                AddEmployeeField(BatchUpdateEmployee.EmployeePositions);
            AddEmployeeField(BatchUpdateEmployee.ExcludeFromPayroll);
            if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_WorkTimeAccount, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId))
                AddEmployeeField(BatchUpdateEmployee.TimeWorkAccount, doShowFromDate: true, doShowToDate: true);
            if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId))
                AddEmployeeField(BatchUpdateEmployee.DoNotValidateAccount);

            //Employment
            if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId))
            {
                AddEmploymentField(BatchUpdateEmployee.EmploymentExternalCode);
                AddEmploymentField(BatchUpdateEmployee.EmploymentType);
                AddEmploymentField(BatchUpdateEmployee.EmployeeGroup);
                AddEmploymentField(BatchUpdateEmployee.PayrollGroup);
                AddEmploymentField(BatchUpdateEmployee.VacationGroup);
                AddEmploymentField(BatchUpdateEmployee.WorkTimeWeekMinutes);
                AddEmploymentField(BatchUpdateEmployee.EmploymentPercent);
                if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId))
                    AddEmploymentField(BatchUpdateEmployee.AccountNrSieDim);
                AddEmploymentField(BatchUpdateEmployee.WorkPlace);
                AddEmploymentField(BatchUpdateEmployee.WorkTasks);
            }

            //User
            if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_User, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId))
            {
                AddUserField(BatchUpdateEmployee.UserRole, doShowFromDate: true, doShowToDate: true);
                AddUserField(BatchUpdateEmployee.AttestRole, doShowFromDate: true, doShowToDate: true);
                AddUserField(BatchUpdateEmployee.BlockedFromDate);
            }

            //Reports
            if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Reports, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId))
            {
                AddReportField(BatchUpdateEmployee.PayrollStatisticsPersonalCategory);
                AddReportField(BatchUpdateEmployee.PayrollStatisticsWorkTimeCategory);
                AddReportField(BatchUpdateEmployee.PayrollStatisticsSalaryType);
                AddReportField(BatchUpdateEmployee.PayrollStatisticsWorkPlaceNumber);
                AddReportField(BatchUpdateEmployee.PayrollStatisticsCFARNumber);
                AddReportField(BatchUpdateEmployee.ControlTaskWorkPlacSCB);
                AddReportField(BatchUpdateEmployee.ControlTaskPartnerInCloseCompany);
                AddReportField(BatchUpdateEmployee.ControlTaskBenefitAsPension);
                AddReportField(BatchUpdateEmployee.AFACategory);
                AddReportField(BatchUpdateEmployee.AFASpecialAgreement);
                AddReportField(BatchUpdateEmployee.AFAWorkplaceNr);
                AddReportField(BatchUpdateEmployee.AFAParttimePensionCode);
                AddReportField(BatchUpdateEmployee.CollectumITPPlan);
                AddReportField(BatchUpdateEmployee.CollectumAgreedOnProduct);
                AddReportField(BatchUpdateEmployee.CollectumCostPlace);
                AddReportField(BatchUpdateEmployee.CollectumCancellationDate);
                AddReportField(BatchUpdateEmployee.CollectumCancellationDateIsLeaveOfAbsence);
                AddReportField(BatchUpdateEmployee.KPARetirementAge);
                AddReportField(BatchUpdateEmployee.KPABelonging);
                AddReportField(BatchUpdateEmployee.KPAEndCode);
                AddReportField(BatchUpdateEmployee.KPAAgreementType);
                AddReportField(BatchUpdateEmployee.BygglosenAgreementArea);
                AddReportField(BatchUpdateEmployee.BygglosenAllocationNumber);
                AddReportField(BatchUpdateEmployee.BygglosenSalaryFormula);
                AddReportField(BatchUpdateEmployee.BygglosenMunicipalCode);
                AddReportField(BatchUpdateEmployee.BygglosenProfessionCategory);
                AddReportField(BatchUpdateEmployee.BygglosenSalaryType);
                AddReportField(BatchUpdateEmployee.BygglosenWorkPlaceNumber);
                AddReportField(BatchUpdateEmployee.BygglosenLendedToOrgNr);
                AddReportField(BatchUpdateEmployee.BygglosenAgreedHourlyPayLevel);
                AddReportField(BatchUpdateEmployee.GTPAgreementNumber);
                AddReportField(BatchUpdateEmployee.GTPExcluded);
                AddReportField(BatchUpdateEmployee.AGIPlaceOfEmploymentAddress);
                AddReportField(BatchUpdateEmployee.AGIPlaceOfEmploymentCity);
                AddReportField(BatchUpdateEmployee.AGIPlaceOfEmploymentIgnore);
                AddReportField(BatchUpdateEmployee.IFAssociationNumber);
                AddReportField(BatchUpdateEmployee.IFPaymentCode);
                AddReportField(BatchUpdateEmployee.IFWorkPlace);
            }

            void AddEmployeeField(BatchUpdateEmployee field, bool doShowFromDate = false, bool doShowToDate = false)
            {
                AddField(field, GetText(3178, "Anställd"), doShowFromDate, doShowToDate);
            }
            void AddEmploymentField(BatchUpdateEmployee field)
            {
                AddField(field, GetText(11910, "Anställning"), true, true);
            }
            void AddReportField(BatchUpdateEmployee field)
            {
                AddField(field, GetText(7189, "Rapporter"), false, false);
            }
            void AddUserField(BatchUpdateEmployee field, bool doShowFromDate = false, bool doShowToDate = false)
            {
                AddField(field, GetText(1062, "Användare"), doShowFromDate, doShowToDate);
            }
            void AddField(BatchUpdateEmployee field, string labelPrefix, bool doShowFromDate, bool doShowToDate)
            {
                List<BatchFieldDefinition> definitions = GetEmployeeBatchUpdateFieldDefinitions(field, langId, labelPrefix);
                batchUpdateData.Add(definitions.CreateField(doShowFromDate, doShowToDate));
            }

            return batchUpdateData.OrderBy(b => b.Label).ToList();
        }
        public BatchUpdateDTO RefreshEmployeeBatchUpdateOptions(BatchUpdateDTO batchUpdate)
        {
            if (batchUpdate == null || batchUpdate.DataType != BatchUpdateFieldType.Id)
                return null;

            CacheConfig config = CacheConfig.Company(base.ActorCompanyId);
            int langId = GetLangId();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            switch (batchUpdate.Field)
            {
                case (int)BatchUpdateEmployee.HierarchicalAccount:
                case (int)BatchUpdateEmployee.AccountNrSieDim:
                    batchUpdate.Options = AccountManager.GetAccountsSortedByDim(base.ActorCompanyId)
                            .OrderBy(e => e.DimNameNumberAndName)
                            .Select(e => new NameAndIdDTO { Id = e.AccountId, Name = e.DimNameNumberAndName }).ToList();
                    break;
                case (int)BatchUpdateEmployee.TimeWorkAccount:
                    batchUpdate.Options = GetTimeWorkAccountsFromCache(entitiesReadOnly, config)
                        .OrderBy(e => e.Name)
                        .Select(e => new NameAndIdDTO { Id = e.TimeWorkAccountId, Name = e.Name }).ToList();
                    break;
                case (int)BatchUpdateEmployee.EmployeeGroup:
                    batchUpdate.Options = GetEmployeeGroupsFromCache(entitiesReadOnly, config)
                        .OrderBy(e => e.Name)
                        .Select(e => new NameAndIdDTO { Id = e.EmployeeGroupId, Name = e.Name }).ToList();
                    break;
                case (int)BatchUpdateEmployee.PayrollGroup:
                    batchUpdate.Options = GetPayrollGroupsFromCache(entitiesReadOnly, config)
                        .OrderBy(e => e.Name)
                        .Select(e => new NameAndIdDTO { Id = e.PayrollGroupId, Name = e.Name }).ToList();
                    break;
                case (int)BatchUpdateEmployee.VacationGroup:
                    batchUpdate.Options = GetVacationGroupsFromCache(entitiesReadOnly, config)
                        .OrderBy(e => e.Name)
                        .Select(e => new NameAndIdDTO { Id = e.VacationGroupId, Name = e.Name }).ToList();
                    break;
                case (int)BatchUpdateEmployee.EmploymentType:
                    batchUpdate.Options = base.GetEmploymentTypesFromCache(entitiesReadOnly, config, (TermGroup_Languages)langId)
                        .OrderBy(e => e.CodeAndName)
                        .Select(e => new NameAndIdDTO { Id = e.GetEmploymentType(), Name = e.CodeAndName }).ToList();
                    break;
                case (int)BatchUpdateEmployee.EmployeePositions:
                    batchUpdate.Options = base.GetPositionsFromCache(entitiesReadOnly, config)
                        .OrderBy(e => e.Name)
                        .Select(e => new NameAndIdDTO { Id = e.PositionId, Name = e.NameAndCode }).ToList();
                    break;
                case (int)BatchUpdateEmployee.EmploymentPriceType:
                    batchUpdate.Options = GetPayrollPriceTypeDTOsFromCache(entitiesReadOnly, config)
                       .OrderBy(e => e.Name)
                       .Select(e => new NameAndIdDTO { Id = e.PayrollPriceTypeId, Name = e.Name }).ToList();
                    BatchUpdateDTO payrollLevel = batchUpdate.GetFirstChild(BatchUpdateFieldType.Id);
                    if (payrollLevel != null)
                    {
                        payrollLevel.Options = PayrollManager.GetPayrollLevels(base.ActorCompanyId)
                            .OrderBy(e => e.Name)
                            .Select(e => new NameAndIdDTO { Id = e.PayrollLevelId, Name = e.CodeAndName }).ToList();
                    }
                    break;
                case (int)BatchUpdateEmployee.UserRole:
                    batchUpdate.Options = GetRolesFromCache(entitiesReadOnly, config)
                        .OrderBy(e => e.Name)
                        .Select(e => new NameAndIdDTO { Id = e.RoleId, Name = e.Name }).ToList();
                    break;
                case (int)BatchUpdateEmployee.AttestRole:
                    batchUpdate.Options = GetTimeAttestRolesFromCache(entitiesReadOnly, config)
                        .OrderBy(e => e.Name)
                        .Select(e => new NameAndIdDTO { Id = e.AttestRoleId, Name = e.Name }).ToList();
                    BatchUpdateDTO attestRoleAccount = batchUpdate.GetFirstChild(BatchUpdateFieldType.Id);
                    if (attestRoleAccount != null)
                    {
                        attestRoleAccount.Options = AccountManager.GetAccountsSortedByDim(base.ActorCompanyId)
                            .OrderBy(e => e.DimNameNumberAndName)
                            .Select(e => new NameAndIdDTO { Id = e.AccountId, Name = e.DimNameNumberAndName }).ToList();
                    }
                    break;
                case (int)BatchUpdateEmployee.PayrollStatisticsPersonalCategory:
                case (int)BatchUpdateEmployee.PayrollStatisticsWorkTimeCategory:
                case (int)BatchUpdateEmployee.PayrollStatisticsSalaryType:
                case (int)BatchUpdateEmployee.AFACategory:
                case (int)BatchUpdateEmployee.AFASpecialAgreement:
                case (int)BatchUpdateEmployee.CollectumITPPlan:
                case (int)BatchUpdateEmployee.KPABelonging:
                case (int)BatchUpdateEmployee.KPAEndCode:
                case (int)BatchUpdateEmployee.KPAAgreementType:
                case (int)BatchUpdateEmployee.GTPAgreementNumber:
                case (int)BatchUpdateEmployee.IFPaymentCode:
                case (int)BatchUpdateEmployee.BygglosenSalaryType:
                    batchUpdate.Options = GetTerms();
                    break;
                case (int)BatchUpdateEmployee.BygglosenSalaryFormula:
                    batchUpdate.Options = GetPayrollPriceFormulaDTOsFromCache(entitiesReadOnly, config)
                        .Select(e => new NameAndIdDTO { Id = e.PayrollPriceFormulaId, Name = e.Name }).ToList();
                    break;
            }

            List<NameAndIdDTO> GetTerms()
            {
                EmployeeChangeType employeeChangeType = (EmployeeChangeType)batchUpdate.Field;
                TermGroup termGroup = ApiManager.GetEmployeeTermGroup(employeeChangeType);
                return ApiManager.GetApiTerms(termGroup, langId)
                    .Select(e => new NameAndIdDTO { Id = e.Id, Name = e.Name }).ToList();
            }

            return batchUpdate;
        }
        public List<BatchFieldDefinition> GetEmployeeBatchUpdateFieldDefinitions(BatchUpdateEmployee field, int langId = 0, string labelPrefix = "")
        {
            List<BatchFieldDefinition> defintions = new List<BatchFieldDefinition>();
            switch (field)
            {
                case BatchUpdateEmployee.Active:
                    AddDefinition(BatchUpdateFieldType.Boolean);
                    break;
                case BatchUpdateEmployee.ExternalCode:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.HierarchicalAccount:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.AccountNrSieDim:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.ExcludeFromPayroll:
                    AddDefinition(BatchUpdateFieldType.Boolean);
                    break;
                case BatchUpdateEmployee.TimeWorkAccount:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.EmployeeGroup:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.PayrollGroup:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.VacationGroup:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.WorkTimeWeekMinutes:
                    AddDefinition(BatchUpdateFieldType.Time);
                    break;
                case BatchUpdateEmployee.EmploymentPercent:
                    AddDefinition(BatchUpdateFieldType.Decimal);
                    break;
                case BatchUpdateEmployee.EmploymentExternalCode:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.EmploymentType:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.WorkTasks:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.WorkPlace:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.EmploymentPriceType:
                    AddDefinition(BatchUpdateFieldType.Id);
                    AddDefinition(BatchUpdateFieldType.Decimal, GetText(7135, "Belopp"));
                    AddDefinition(BatchUpdateFieldType.Id, GetText(91942, "Lönenivå"));
                    break;
                case BatchUpdateEmployee.EmployeePositions:
                case BatchUpdateEmployee.UserRole:
                    AddDefinition(BatchUpdateFieldType.Id);
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(1130, "Standard"));
                    break;
                case BatchUpdateEmployee.AttestRole:
                    AddDefinition(BatchUpdateFieldType.Id);
                    AddDefinition(BatchUpdateFieldType.Id, GetText(1258, "Konto"));
                    break;
                case BatchUpdateEmployee.BlockedFromDate:
                    AddDefinition(BatchUpdateFieldType.Date);
                    break;
                case BatchUpdateEmployee.PayrollStatisticsPersonalCategory:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.PayrollStatisticsWorkTimeCategory:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.PayrollStatisticsSalaryType:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.PayrollStatisticsWorkPlaceNumber:
                    AddDefinition(BatchUpdateFieldType.Integer);
                    break;
                case BatchUpdateEmployee.PayrollStatisticsCFARNumber:
                    AddDefinition(BatchUpdateFieldType.Integer);
                    break;
                case BatchUpdateEmployee.ControlTaskWorkPlacSCB:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.ControlTaskPartnerInCloseCompany:
                    AddDefinition(BatchUpdateFieldType.Boolean);
                    break;
                case BatchUpdateEmployee.ControlTaskBenefitAsPension:
                    AddDefinition(BatchUpdateFieldType.Boolean);
                    break;
                case BatchUpdateEmployee.AFACategory:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.AFASpecialAgreement:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.AFAWorkplaceNr:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.AFAParttimePensionCode:
                    AddDefinition(BatchUpdateFieldType.Boolean);
                    break;
                case BatchUpdateEmployee.CollectumITPPlan:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.CollectumAgreedOnProduct:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.CollectumCostPlace:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.CollectumCancellationDate:
                    AddDefinition(BatchUpdateFieldType.Date);
                    break;
                case BatchUpdateEmployee.CollectumCancellationDateIsLeaveOfAbsence:
                    AddDefinition(BatchUpdateFieldType.Boolean);
                    break;
                case BatchUpdateEmployee.KPARetirementAge:
                    AddDefinition(BatchUpdateFieldType.Integer);
                    break;
                case BatchUpdateEmployee.KPABelonging:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.KPAEndCode:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.KPAAgreementType:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.BygglosenAgreementArea:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.BygglosenAllocationNumber:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.BygglosenSalaryFormula:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.BygglosenMunicipalCode:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.BygglosenProfessionCategory:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.BygglosenSalaryType:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.BygglosenAgreedHourlyPayLevel:
                    AddDefinition(BatchUpdateFieldType.Decimal);
                    break;
                case BatchUpdateEmployee.BygglosenWorkPlaceNumber:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.BygglosenLendedToOrgNr:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.GTPAgreementNumber:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.GTPExcluded:
                    AddDefinition(BatchUpdateFieldType.Boolean);
                    break;
                case BatchUpdateEmployee.AGIPlaceOfEmploymentAddress:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.AGIPlaceOfEmploymentCity:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.AGIPlaceOfEmploymentIgnore:
                    AddDefinition(BatchUpdateFieldType.Boolean);
                    break;
                case BatchUpdateEmployee.IFAssociationNumber:
                    AddDefinition(BatchUpdateFieldType.Integer);
                    break;
                case BatchUpdateEmployee.IFPaymentCode:
                    AddDefinition(BatchUpdateFieldType.Id);
                    break;
                case BatchUpdateEmployee.IFWorkPlace:
                    AddDefinition(BatchUpdateFieldType.String);
                    break;
                case BatchUpdateEmployee.DoNotValidateAccount:
                    AddDefinition(BatchUpdateFieldType.Boolean);
                    break;
            }
            void AddDefinition(BatchUpdateFieldType type, string label = null)
            {
                if (label == null && langId > 0)
                    label = GetLabel();
                defintions.Add(new BatchFieldDefinition((int)field, type, label));
            }
            string GetLabel()
            {
                string label = ApiManager.GetEmployeeTerm((int)field, langId);
                if (!labelPrefix.IsNullOrEmpty())
                    label = $"{labelPrefix} - {label}";
                return label;
            }
            return defintions;
        }
        public ActionResult PerformEmployeeBatchUpdate(List<BatchUpdateDTO> batchUpdateDTOs, List<int> employeeIds)
        {
            if (batchUpdateDTOs.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.IncorrectInput, GetText(91926, "Inga fält valda"));
            if (batchUpdateDTOs.Any(b => b.Field == (int)BatchUpdateEmployee.WorkTimeWeekMinutes) && batchUpdateDTOs.Any(b => b.Field == (int)BatchUpdateEmployee.EmploymentPercent))
                return new ActionResult((int)ActionResultSave.IncorrectInput, GetText(91927, "Ej tillåtet att ändra både veckoarbetstid och sysselsättningsgrad samtidigt"));

            List<EmployeeChangeIODTO> employeeChanges = ConvertToEmployeeChangeDTOs(batchUpdateDTOs, employeeIds);
            EmployeeChangeResult employeeChangeResult = ApiManager.ImportEmployeeChangesFromMassUpdate(employeeChanges, out ActionResult result);
            if (result.Success && employeeChangeResult.NrOfReceivedEmployees > 0 && result.Value is EmployeeUserImportBatch)
            {
                EmployeeUserImportBatch batch = result.Value as EmployeeUserImportBatch;
                if (batch.HasValidationErrors)
                    result.InfoMessage += $"{GetText(91929, "Sparat. Fel uppstod")}. {GetText(91930, "Se flik 'Massuppdatering' för detaljerad information")}";
                else if (batch.IsAlreadyUpdated)
                    result.InfoMessage += $"{GetText(12054, "Inga förändringar. Alla uppgifter är redan uppdaterade")}";
            }
            return result;
        }
        private List<EmployeeChangeIODTO> ConvertToEmployeeChangeDTOs(List<BatchUpdateDTO> batchUpdateDTOs, List<int> employeeIds)
        {
            List<EmployeeChangeIODTO> employeeChanges = new List<EmployeeChangeIODTO>();

            if (!batchUpdateDTOs.IsNullOrEmpty() && !employeeIds.IsNullOrEmpty())
            {
                foreach (int employeeId in employeeIds)
                {
                    Employee employee = EmployeeManager.GetEmployee(employeeId, base.ActorCompanyId);
                    if (employee == null)
                        continue;

                    EmployeeChangeIODTO employeeChange = new EmployeeChangeIODTO
                    {
                        EmployeeNr = employee.EmployeeNr,
                        EmployeeChangeRowIOs = new List<EmployeeChangeRowIODTO>(),
                    };
                    batchUpdateDTOs.ForEach(batchUpdateDTO => employeeChange.EmployeeChangeRowIOs.AddRange(ConvertToEmployeeChangeDTO(batchUpdateDTO)));
                    employeeChanges.Add(employeeChange);
                }
            }

            return employeeChanges;
        }
        private List<EmployeeChangeRowIODTO> ConvertToEmployeeChangeDTO(BatchUpdateDTO batchUpdate)
        {
            List<EmployeeChangeRowIODTO> rows = new List<EmployeeChangeRowIODTO>();

            if (batchUpdate == null)
                return rows;

            Type enumType = typeof(EmployeeChangeType);
            if (!Enum.IsDefined(enumType, batchUpdate.Field))
                return rows;

            EmployeeChangeType employeeChangeType = (EmployeeChangeType)batchUpdate.Field;
            var (value, optionalExternalCode) = GetEmployeeBatchUpdateValue(batchUpdate);

            AddRow(employeeChangeType);
            if (!batchUpdate.Children.IsNullOrEmpty())
            {
                BatchUpdateDTO childBatchUpdateDTO = batchUpdate.Children.FirstOrDefault();
                if (childBatchUpdateDTO != null)
                {
                    if (employeeChangeType == EmployeeChangeType.EmployeePosition && childBatchUpdateDTO.BoolValue)
                        AddRow(EmployeeChangeType.EmployeePositionDefault);
                    else if (employeeChangeType == EmployeeChangeType.UserRole && childBatchUpdateDTO.BoolValue)
                        AddRow(EmployeeChangeType.DefaultUserRole);
                }
            }

            void AddRow(EmployeeChangeType rowEmployeeChangeType)
            {
                rows.Add(new EmployeeChangeRowIODTO
                {
                    EmployeeChangeType = rowEmployeeChangeType,
                    Value = value,
                    OptionalExternalCode = optionalExternalCode,
                    OptionalEmploymentDate = batchUpdate.FromDate ?? CalendarUtility.DATETIME_DEFAULT,
                    FromDate = batchUpdate.FromDate,
                    ToDate = batchUpdate.ToDate,
                    Delete = false,
                });
            }

            return rows;
        }
        (string value, string optionalExternalCode) GetEmployeeBatchUpdateValue(BatchUpdateDTO batchUpdateDTO)
        {
            string value = null;
            string optionalExternalCode = null;

            if (batchUpdateDTO != null)
            {
                BatchUpdateEmployee field = (BatchUpdateEmployee)batchUpdateDTO.Field;
                List<BatchFieldDefinition> definitions = GetEmployeeBatchUpdateFieldDefinitions(field);
                BatchUpdateFieldType dataType = definitions.Count == 1 ? definitions.First().DataType : BatchUpdateFieldType.Id;

                if (dataType == BatchUpdateFieldType.String)
                    value = batchUpdateDTO.StringValue;
                else if (dataType == BatchUpdateFieldType.Integer)
                    value = batchUpdateDTO.IntValue.ToString();
                else if (dataType == BatchUpdateFieldType.Boolean)
                    value = batchUpdateDTO.BoolValue ? Boolean.TrueString : Boolean.FalseString;
                else if (dataType == BatchUpdateFieldType.Date)
                    value = batchUpdateDTO.DateValue.ToShortDateString();
                else if (dataType == BatchUpdateFieldType.Decimal)
                    value = batchUpdateDTO.DecimalValue.ToString();
                else if (dataType == BatchUpdateFieldType.Time)
                    value = batchUpdateDTO.IntValue.ToString();
                else if (dataType == BatchUpdateFieldType.Id)
                {
                    int? childId = batchUpdateDTO.GetFirstChild(BatchUpdateFieldType.Id)?.IntValue;
                    switch (field)
                    {
                        case BatchUpdateEmployee.HierarchicalAccount:
                            Account hierarchicalAccount = AccountManager.GetAccount(base.ActorCompanyId, batchUpdateDTO.IntValue);
                            value = hierarchicalAccount?.ExternalCode.EmptyToNull() ?? hierarchicalAccount?.AccountNr.EmptyToNull() ?? hierarchicalAccount?.Name;
                            break;
                        case BatchUpdateEmployee.AccountNrSieDim:
                            Account sieAccount = AccountManager.GetAccount(base.ActorCompanyId, batchUpdateDTO.IntValue, loadAccountDim: true);
                            value = sieAccount?.ExternalCode.EmptyToNull() ?? sieAccount?.AccountNr.EmptyToNull() ?? sieAccount?.Name;
                            optionalExternalCode = sieAccount?.AccountDim?.SysSieDimNr?.ToString();
                            break;
                        case BatchUpdateEmployee.TimeWorkAccount:
                            TimeWorkAccount timeWorkAccount = TimeWorkAccountManager.GetTimeWorkAccount(batchUpdateDTO.IntValue);
                            value = timeWorkAccount?.Name;
                            break;
                        case BatchUpdateEmployee.EmployeeGroup:
                            EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(batchUpdateDTO.IntValue);
                            value = employeeGroup?.Name;
                            break;
                        case BatchUpdateEmployee.PayrollGroup:
                            PayrollGroup payrollGroup = PayrollManager.GetPayrollGroup(batchUpdateDTO.IntValue);
                            value = payrollGroup?.Name;
                            break;
                        case BatchUpdateEmployee.VacationGroup:
                            VacationGroup vacationGroup = PayrollManager.GetVacationGroup(batchUpdateDTO.IntValue);
                            value = vacationGroup?.Name;
                            break;
                        case BatchUpdateEmployee.EmploymentType:
                            EmploymentTypeDTO employmentType = EmployeeManager.GetEmploymentType(base.ActorCompanyId, batchUpdateDTO.IntValue);
                            value = employmentType?.Code.EmptyToNull() ?? employmentType?.Name.EmptyToNull() ?? employmentType?.Type.ToString();
                            break;
                        case BatchUpdateEmployee.EmploymentPriceType:
                            PayrollPriceType payrollPriceType = PayrollManager.GetPayrollPriceType(batchUpdateDTO.IntValue);
                            PayrollLevel payrollLevel = childId.HasValue ? PayrollManager.GetPayrollLevel(base.ActorCompanyId, childId.Value) : null;
                            if (payrollLevel != null)
                                optionalExternalCode = $"{payrollPriceType?.Code}{EmployeeChangeIOItem.DELIMETER}{payrollLevel.ExternalCode.EmptyToNull() ?? payrollLevel.Code.EmptyToNull() ?? payrollLevel.Name}";
                            else
                                optionalExternalCode = payrollPriceType?.Code;
                            value = batchUpdateDTO.GetFirstChild(BatchUpdateFieldType.Decimal)?.DecimalValue.ToString();
                            break;
                        case BatchUpdateEmployee.EmployeePositions:
                            Position position = EmployeeManager.GetPosition(batchUpdateDTO.IntValue);
                            value = position?.Name;
                            break;
                        case BatchUpdateEmployee.UserRole:
                            Role role = RoleManager.GetRole(batchUpdateDTO.IntValue);
                            value = role?.SystemRoleName ?? role?.Name;
                            break;
                        case BatchUpdateEmployee.AttestRole:
                            AttestRole attestRole = AttestManager.GetAttestRole(batchUpdateDTO.IntValue, base.ActorCompanyId);
                            Account attestRoleAccount = childId.HasValue ? AccountManager.GetAccount(base.ActorCompanyId, childId.Value, loadAccountDim: true) : null;
                            optionalExternalCode = attestRoleAccount?.ExternalCode.EmptyToNull() ?? attestRoleAccount?.AccountNr.EmptyToNull() ?? attestRoleAccount?.Name;
                            value = attestRole?.Description ?? attestRole?.Name;
                            break;
                        case BatchUpdateEmployee.PayrollStatisticsPersonalCategory:
                        case BatchUpdateEmployee.PayrollStatisticsWorkTimeCategory:
                        case BatchUpdateEmployee.PayrollStatisticsSalaryType:
                        case BatchUpdateEmployee.AFACategory:
                        case BatchUpdateEmployee.AFASpecialAgreement:
                        case BatchUpdateEmployee.CollectumITPPlan:
                        case BatchUpdateEmployee.KPABelonging:
                        case BatchUpdateEmployee.KPAEndCode:
                        case BatchUpdateEmployee.KPAAgreementType:
                        case BatchUpdateEmployee.GTPAgreementNumber:
                        case BatchUpdateEmployee.IFPaymentCode:
                        case BatchUpdateEmployee.BygglosenSalaryType:
                            value = batchUpdateDTO.IntValue.ToString();
                            break;
                        case BatchUpdateEmployee.BygglosenSalaryFormula:
                            PayrollPriceFormula payrollPriceFormula = PayrollManager.GetPayrollPriceFormula(base.ActorCompanyId, batchUpdateDTO.IntValue);
                            value = payrollPriceFormula?.Name;
                            break;
                    }
                }
            }

            return (value, optionalExternalCode);
        }

        #endregion

        #region InvoiceProduct
        public List<BatchUpdateDTO> GetInvoiceProductBatchUpdate(CompEntities entities)
        {
            var batchUpdateData = new List<BatchUpdateDTO>();

            batchUpdateData.Add(
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateInvoiceProduct.MaterialCode,
                    Label = GetText(5993, "Materialkod"),
                    Options = TimeCodeManager.GetTimeCodes(entities, this.ActorCompanyId, SoeTimeCodeType.Material)
                        .Select(t => new NameAndIdDTO
                        {
                            Id = t.TimeCodeId,
                            Name = t.Name
                        })
                        .ToList()
                }
            );
            batchUpdateData.Add(
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateInvoiceProduct.ProductUnit,
                    Label = GetText(1881, "Enhet"),
                    Options = ProductManager.GetProductUnits(entities, this.ActorCompanyId)
                        .Select(t => new NameAndIdDTO
                        {
                            Id = t.ProductUnitId,
                            Name = t.Name
                        })
                        .ToList(),
                }
            );
            batchUpdateData.Add(
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateInvoiceProduct.ProductGroup,
                    Label = GetText(4255, "Produktgrupp"),
                    Options = ProductGroupManager.GetProductGroups(entities, this.ActorCompanyId)
                        .Select(t => new NameAndIdDTO
                        {
                            Id = t.ProductGroupId,
                            Name = t.Name
                        })
                        .ToList(),
                }
            );
            batchUpdateData.Add(
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateInvoiceProduct.Type,
                    Label = GetText(1873, "Typ"),
                    Options = GetTermGroup(TermGroup.InvoiceProductVatType, true)
                }
            );
            batchUpdateData.Add(
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateInvoiceProduct.VatCode,
                    Label = GetText(189, "Momskod"),
                    Options = AccountManager.GetVatCodes(entities, this.ActorCompanyId)
                        .Select(t => new NameAndIdDTO
                        {
                            Id = t.VatCodeId,
                            Name = t.Name
                        })
                        .ToList(),
                }
            );
            batchUpdateData.Add(
                GetYesNoAlternative((int)BatchUpdateInvoiceProduct.Active, 3273)
            );
            //batchUpdateData.Add(
            //    GetYesNoAlternative((int)BatchUpdateProduct.IsStockProduct, 6420)
            //);
            batchUpdateData.Add(
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.String,
                    Field = (int)BatchUpdateInvoiceProduct.Description,
                    Label = GetText(1328, "Beskrivning"),
                }
            );
            batchUpdateData.Add(
                 new BatchUpdateDTO()
                 {
                     DataType = BatchUpdateFieldType.Id,
                     Field = (int)BatchUpdateInvoiceProduct.IntrastatCode,
                     Label = GetText(5802, (int)TermGroup.AngularCommon, "Statistisk varukod"),
                     Options = CommodityCodeManager.GetCustomerCommodityCodes(base.ActorCompanyId, true)
                         .Select(t => new NameAndIdDTO
                         {
                             Id = (int)t.IntrastatCodeId,
                             Name = t.Code + " " + t.Text
                         })
                         .ToList(),
                 }
             );
            batchUpdateData.Add(
                  new BatchUpdateDTO()
                  {
                      DataType = BatchUpdateFieldType.Id,
                      Field = (int)BatchUpdateInvoiceProduct.CountryOfOrigin,
                      Label = GetText(518, (int)TermGroup.AngularCommon, "Ursprungsland"),
                      Options = CountryCurrencyManager.GetSysCountries(true).OrderBy(c => c.Name)
                          .Select(t => new NameAndIdDTO
                          {
                              Id = t.SysCountryId,
                              Name = t.Name + " (" + t.Code + ")"
                          })
                          .ToList(),
                  }
              );
            return batchUpdateData;
        }
        public ActionResult PerformInvoiceProductBatchUpdate(CompEntities entities, List<BatchUpdateDTO> batchUpdateDTOs, List<int> productIds)
        {
            var result = new ActionResult();

            int actorCompanyId = base.ActorCompanyId;
            var query = from entry in entities.Product.OfType<InvoiceProduct>()
                        where
                        entry.Company.Any(i => i.ActorCompanyId == actorCompanyId) &&
                        productIds.Contains(entry.ProductId)
                        select entry;
            var products = query.ToList();

            foreach (var product in products)
            {
                foreach (var field in batchUpdateDTOs)
                {
                    switch ((BatchUpdateInvoiceProduct)field.Field)
                    {
                        case BatchUpdateInvoiceProduct.Description:
                            product.Description = field.StringValue;
                            break;
                        case BatchUpdateInvoiceProduct.Active:
                            product.State = GetBoolFromYesNo(field.IntValue) ? (int)SoeEntityState.Active : (int)SoeEntityState.Inactive;
                            break;
                        //case BatchUpdateProduct.IsStockProduct:
                        //    product.IsStockProduct = GetBoolFromYesNo(field.IntValue);
                        //    break;
                        case BatchUpdateInvoiceProduct.MaterialCode:
                            product.TimeCodeId = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateInvoiceProduct.ProductUnit:
                            product.ProductUnitId = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateInvoiceProduct.ProductGroup:
                            product.ProductGroupId = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateInvoiceProduct.Type:
                            product.VatType = field.IntValue;
                            break;
                        case BatchUpdateInvoiceProduct.VatCode:
                            product.VatCodeId = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateInvoiceProduct.IntrastatCode:
                            product.IntrastatCodeId = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateInvoiceProduct.CountryOfOrigin:
                            product.SysCountryId = NotZeroOrNull(field.IntValue);
                            break;
                    }
                }
                SetModifiedProperties(product);
            }

            return result;
        }
        #endregion

        #region PayrollProduct
        public List<BatchUpdateDTO> GetPayrollProductBatchUpdate()
        {
            var batchUpdateData = new List<BatchUpdateDTO>();

            AddHeaderBoolField(BatchUpdatePayrollProduct.Active);
            AddHeaderBoolField(BatchUpdatePayrollProduct.Payed);
            AddHeaderBoolField(BatchUpdatePayrollProduct.ExcludeInWorkTimeSummary);
            AddHeaderBoolField(BatchUpdatePayrollProduct.AverageCalculated);
            AddHeaderBoolField(BatchUpdatePayrollProduct.UseInPayroll);
            AddHeaderBoolField(BatchUpdatePayrollProduct.DontUseFixedAccounting);
            AddHeaderBoolField(BatchUpdatePayrollProduct.Export);

            AddPayrollGroupField(BatchUpdatePayrollProduct.PrintOnSalarySpecification);
            AddPayrollGroupField(BatchUpdatePayrollProduct.DontPrintOnSalarySpecificationWhenZeroAmount);
            AddPayrollGroupField(BatchUpdatePayrollProduct.PrintDate);
            AddPayrollGroupField(BatchUpdatePayrollProduct.DontIncludeInRetroactivePayroll);
            AddPayrollGroupField(BatchUpdatePayrollProduct.VacationSalaryPromoted);
            AddPayrollGroupField(BatchUpdatePayrollProduct.UnionFeePromoted);
            AddPayrollGroupField(BatchUpdatePayrollProduct.WorkingTimePromoted);
            AddPayrollGroupField(BatchUpdatePayrollProduct.CalculateSupplementCharge);
            AddPayrollGroupField(BatchUpdatePayrollProduct.CalculateSicknessSalary);
            AddPayrollGroupField(BatchUpdatePayrollProduct.PensionCompany);
            AddPayrollGroupField(BatchUpdatePayrollProduct.TimeUnit);
            AddPayrollGroupField(BatchUpdatePayrollProduct.TaxCalculationType);
            AddPayrollGroupField(BatchUpdatePayrollProduct.DontIncludeInAbsenceCost);
            AddPayrollGroupField(BatchUpdatePayrollProduct.AccountInternal);
            AddPayrollGroupField(BatchUpdatePayrollProduct.AccountingPrio);
            AddPayrollGroupField(BatchUpdatePayrollProduct.PayrollProductPriceTypesAndFormulas, true);

            void AddHeaderBoolField(BatchUpdatePayrollProduct field)
            {
                AddField(field, GetText(4601, (int)TermGroup.AngularTime, "Löneart"));
            }
            void AddPayrollGroupField(BatchUpdatePayrollProduct field, bool doShowFromDate = false, bool doShowToDate = false)
            {
                AddField(field, GetText(6915, (int)TermGroup.AngularTime, "Inställningar per löneavtal"), doShowFromDate, doShowToDate);
            }
            void AddField(BatchUpdatePayrollProduct field, string labelPrefix, bool doShowFromDate = false, bool doShowToDate = false)
            {
                List<BatchFieldDefinition> definitions = GetPayrollProductBatchUpdateFieldDefinitions(field, labelPrefix);
                batchUpdateData.Add(definitions.CreateField(doShowFromDate, doShowToDate));
            }
            return batchUpdateData;
        }
        public BatchUpdateDTO RefreshPayrollProductBatchUpdateOptions(BatchUpdateDTO batchUpdate)
        {
            if (batchUpdate == null || batchUpdate.DataType != BatchUpdateFieldType.Id)
                return null;

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            switch (batchUpdate.Field)
            {
                case (int)BatchUpdatePayrollProduct.PensionCompany:
                    batchUpdate.Options = GetTermGroup(TermGroup.PensionCompany, addEmptyRow: true);
                    break;
                case (int)BatchUpdatePayrollProduct.TimeUnit:
                    batchUpdate.Options = GetTermGroup(TermGroup.PayrollProductTimeUnit, addEmptyRow: true);
                    break;
                case (int)BatchUpdatePayrollProduct.TaxCalculationType:
                    batchUpdate.Options = GetTermGroup(TermGroup.PayrollProductTaxCalculationType, addEmptyRow: true);
                    break;
                case (int)BatchUpdatePayrollProduct.AccountInternal:
                case (int)BatchUpdatePayrollProduct.AccountingPrio:
                    batchUpdate.Options = base.GetPayrollProductsFromCache(entitiesReadOnly, CacheConfig.Company(this.ActorCompanyId))
                        .OrderBy(e => e.NumberSort)
                        .Where(e => e.State == (int)SoeEntityState.Active)
                        .Select(e => new NameAndIdDTO { Id = e.ProductId, Name = e.NumberAndName })
                        .ToList();
                    break;
                case (int)BatchUpdatePayrollProduct.PayrollProductPriceTypesAndFormulas:
                    var payrollProductsAndFormulas = ProductManager.GetPayrollPriceTypesAndFormulas(base.ActorCompanyId).ToList();
                    batchUpdate.Options = payrollProductsAndFormulas
                        .Where(w => w.PayrollPriceTypeId.HasValue)
                        .Select(e => new NameAndIdDTO { Id = e.PayrollPriceTypeId.Value, Name = e.Name }).ToList();
                    batchUpdate.Options.AddRange(payrollProductsAndFormulas
                        .Where(w => w.PayrollPriceFormulaId.HasValue)
                        .Select(e => new NameAndIdDTO { Id = 0 - e.PayrollPriceFormulaId.Value, Name = e.Name }));

                    batchUpdate.Options = batchUpdate.Options
                        .OrderBy(e => e.Name)
                        .ToList();
                    break;
            }

            return batchUpdate;
        }
        public List<BatchFieldDefinition> GetPayrollProductBatchUpdateFieldDefinitions(BatchUpdatePayrollProduct field, string labelPrefix = "")
        {
            List<BatchFieldDefinition> defintions = new List<BatchFieldDefinition>();
            switch (field)
            {
                case BatchUpdatePayrollProduct.Active:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(3273, "Aktiv"));
                    break;
                case BatchUpdatePayrollProduct.Payed:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6905, (int)TermGroup.AngularTime, "Godkänd tid"));
                    break;
                case BatchUpdatePayrollProduct.ExcludeInWorkTimeSummary:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6908, (int)TermGroup.AngularTime, "Exkludera från årsarbetstid"));
                    break;
                case BatchUpdatePayrollProduct.AverageCalculated:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6911, (int)TermGroup.AngularTime, "Dagberäkning (löneperiod)"));
                    break;
                case BatchUpdatePayrollProduct.UseInPayroll:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6910, (int)TermGroup.AngularTime, "Visa i löneberäkning"));
                    break;
                case BatchUpdatePayrollProduct.DontUseFixedAccounting:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6982, (int)TermGroup.AngularTime, "Fast konteras ej"));
                    break;
                case BatchUpdatePayrollProduct.Export:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6909, (int)TermGroup.AngularTime, "Export till lön"));
                    AddChildDefinition(BatchUpdatePayrollProduct.Export_IncludeAmountInExport, BatchUpdateFieldType.Boolean, GetText(6920, (int)TermGroup.AngularTime, "Ta med pris i export"));
                    break;

                case BatchUpdatePayrollProduct.PrintOnSalarySpecification:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6983, (int)TermGroup.AngularTime, "Visa på lönespecifikation"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.DontPrintOnSalarySpecificationWhenZeroAmount:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6983, (int)TermGroup.AngularTime, "Visa ej på lönespecifikation om 0 kr"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.PrintDate:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6956, (int)TermGroup.AngularTime, "Visa datum på lönespecifikation"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.DontIncludeInRetroactivePayroll:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6984, (int)TermGroup.AngularTime, "Ingår ej i retroaktiv lön"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.VacationSalaryPromoted:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6957, (int)TermGroup.AngularTime, "Semesterlönegrundande"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.UnionFeePromoted:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6958, (int)TermGroup.AngularTime, "Fackföreningsavgiftsgrundande"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.WorkingTimePromoted:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6959, (int)TermGroup.AngularTime, "Arbetstidskontogrundande"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.CalculateSupplementCharge:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6960, (int)TermGroup.AngularTime, "Påslagsgrundande"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.CalculateSicknessSalary:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6961, (int)TermGroup.AngularTime, "Beräkningsunderlag sjuklön"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.PensionCompany:
                    AddDefinition(BatchUpdateFieldType.Id, GetText(6954, (int)TermGroup.AngularTime, "Pensionsbolag"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.TimeUnit:
                    AddDefinition(BatchUpdateFieldType.Id, GetText(4901, (int)TermGroup.AngularTime, "Tidenhet"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.TaxCalculationType:
                    AddDefinition(BatchUpdateFieldType.Id, GetText(6953, (int)TermGroup.AngularTime, "Skatteberäkning"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.DontIncludeInAbsenceCost:
                    AddDefinition(BatchUpdateFieldType.Boolean, GetText(6986, (int)TermGroup.AngularTime, "Inkludera inte i frånvarokostnad"), doShowFilter: true);
                    break;
                case BatchUpdatePayrollProduct.AccountInternal:
                    AddDefinition(BatchUpdateFieldType.Id, GetText(9303, (int)TermGroup.AngularTime, "Kontering"), doShowFilter: true, label2: GetText(6987, (int)TermGroup.AngularTime, "Kopiera från löneart"));
                    break;
                case BatchUpdatePayrollProduct.AccountingPrio:
                    AddDefinition(BatchUpdateFieldType.Id, GetText(6966, (int)TermGroup.AngularTime, "Konteringsprio"), doShowFilter: true, label2: GetText(6987, (int)TermGroup.AngularTime, "Kopiera från löneart"));
                    break;
                case BatchUpdatePayrollProduct.PayrollProductPriceTypesAndFormulas:
                    AddDefinition(BatchUpdateFieldType.Id, GetText(6972, (int)TermGroup.AngularTime, "Lönetyp/Löneformel"), doShowFilter: true, doShowFromDate: true);
                    AddDefinition(BatchUpdateFieldType.DecimalNull, label: "", label2: GetText(7135, "Belopp"), doShowFilter: true, doShowFromDate: true);
                    break;

            }
            void AddDefinition(BatchUpdateFieldType type, string label, bool doShowFilter = false, string label2 = "", bool doShowFromDate = false, bool doShowToDate = false)
            {
                defintions.Add(new BatchFieldDefinition((int)field, type, GetLabel(label, label2), doShowFilter, doShowFromDate, doShowToDate));
            }
            void AddChildDefinition(BatchUpdatePayrollProduct childField, BatchUpdateFieldType type, string label, bool doShowFilter = false, string label2 = "")
            {
                defintions.Add(new BatchFieldDefinition((int)childField, type, GetLabel(label, label2), doShowFilter));
            }
            string GetLabel(string label, string label2)
            {
                if (!labelPrefix.IsNullOrEmpty() && !label.IsNullOrEmpty())
                {
                    label = $"{labelPrefix} - {label}";
                    if (label2 != "")
                        label = $"{label} - {label2}";
                }
                else if (!label2.IsNullOrEmpty())
                {
                    label = label2;
                }
                return label;
            }
            return defintions;
        }
        public ActionResult PerformPayrollProductBatchUpdate(CompEntities entities, List<BatchUpdateDTO> batchUpdateDTOs, List<int> productIds, List<int> payrollGroupIds = null)
        {
            var result = new ActionResult();

            int actorCompanyId = base.ActorCompanyId;
            var query = from entry in entities.Product.OfType<PayrollProduct>().Include("PayrollProductSetting")
                        where entry.Company.Any(i => i.ActorCompanyId == actorCompanyId) &&
                        productIds.Contains(entry.ProductId)
                        select entry;

            if (batchUpdateDTOs.Any(w => w.Field == (int)BatchUpdatePayrollProduct.AccountInternal))
                query = query.Include("PayrollProductSetting.PayrollProductAccountStd.AccountStd").Include("PayrollProductSetting.PayrollProductAccountStd.AccountInternal");
            if (batchUpdateDTOs.Any(w => w.Field == (int)BatchUpdatePayrollProduct.PayrollProductPriceTypesAndFormulas))
                query = query.Include("PayrollProductSetting.PayrollProductPriceType.PayrollProductPriceTypePeriod").Include("PayrollProductSetting.PayrollProductPriceType.PayrollPriceType").Include("PayrollProductSetting.PayrollProductPriceFormula.PayrollPriceFormula");

            var products = query.ToList();

            foreach (var product in products)
            {
                var settings = new List<PayrollProductSetting>();
                var updatedSettings = new HashSet<int?>();
                if (!payrollGroupIds.IsNullOrEmpty())
                {
                    if (payrollGroupIds.Any(id => id == -1))
                    {
                        if (product.PayrollProductSetting.Any(s => s.State == (int)SoeEntityState.Active))
                            settings.AddRange(product.PayrollProductSetting.Where(s => s.State == (int)SoeEntityState.Active).ToList());
                    }
                    else if (payrollGroupIds.Any(id => id == 0))
                    {
                        var setting = product.PayrollProductSetting.FirstOrDefault(s => !s.PayrollGroupId.HasValue && s.State == (int)SoeEntityState.Active);
                        if (setting != null)
                            settings.Add(setting);
                    }

                    foreach (int payrollGroupId in payrollGroupIds.Where(id => id > 0))
                    {
                        var setting = product.GetSetting(payrollGroupId, getDefaultIfNotFound: false) ?? ProductManager.CreatePayrollProductSetting(product, payrollGroupId);
                        if (setting != null)
                            settings.Add(setting);
                    }

                }

                var disableAllExcepySalarySpecificationSettings = (
                    product.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Tax ||
                    product.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit ||
                    product.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxDebit ||
                    product.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_SupplementChargeCredit ||
                    product.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_SupplementChargeDebit);

                var disableCalculationExpander = product.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Deduction_SalaryDistress;
                var isAbsenceVacation = product.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                                        product.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                                        product.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation;
                var isVacationAddition = product.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                                        product.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAddition;
                var isVacationSalary = product.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                                       product.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationSalary;

                if (!settings.Any())
                    batchUpdateDTOs = batchUpdateDTOs.Where(b => !b.DoShowFilter).ToList();

                foreach (var field in batchUpdateDTOs.OrderBy(b => b.Field))
                {
                    switch ((BatchUpdatePayrollProduct)field.Field)
                    {
                        case BatchUpdatePayrollProduct.Active:
                            product.State = field.BoolValue ? (int)SoeEntityState.Active : (int)SoeEntityState.Inactive;
                            break;
                        case BatchUpdatePayrollProduct.Payed:
                            product.Payed = field.BoolValue;
                            break;
                        case BatchUpdatePayrollProduct.ExcludeInWorkTimeSummary:
                            product.ExcludeInWorkTimeSummary = field.BoolValue;
                            break;
                        case BatchUpdatePayrollProduct.AverageCalculated:
                            product.AverageCalculated = field.BoolValue;
                            break;
                        case BatchUpdatePayrollProduct.UseInPayroll:
                            product.UseInPayroll = field.BoolValue;
                            break;
                        case BatchUpdatePayrollProduct.DontUseFixedAccounting:
                            product.DontUseFixedAccounting = field.BoolValue;
                            break;
                        case BatchUpdatePayrollProduct.Export:
                            product.Export = field.BoolValue;
                            product.IncludeAmountInExport = field.Children?.FirstOrDefault(f => f.Field == (int)BatchUpdatePayrollProduct.Export_IncludeAmountInExport).BoolValue ?? product.IncludeAmountInExport;
                            break;

                        case BatchUpdatePayrollProduct.PrintOnSalarySpecification:
                            if (!disableCalculationExpander)
                            {
                                settings.ForEach(s => s.PrintOnSalarySpecification = field.BoolValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.DontPrintOnSalarySpecificationWhenZeroAmount:
                            if (!disableCalculationExpander)
                            {
                                settings.ForEach(s => s.DontPrintOnSalarySpecificationWhenZeroAmount = field.BoolValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.PrintDate:
                            if (!disableCalculationExpander)
                            {
                                settings.ForEach(s => s.PrintDate = field.BoolValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.DontIncludeInRetroactivePayroll:
                            if (!disableCalculationExpander)
                            {
                                settings.ForEach(s => s.DontIncludeInRetroactivePayroll = field.BoolValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.VacationSalaryPromoted:
                            if (!disableCalculationExpander && !disableAllExcepySalarySpecificationSettings)
                            {
                                settings.ForEach(s => s.VacationSalaryPromoted = field.BoolValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.UnionFeePromoted:
                            if (!disableCalculationExpander && !disableAllExcepySalarySpecificationSettings)
                            {
                                settings.ForEach(s => s.UnionFeePromoted = field.BoolValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.WorkingTimePromoted:
                            if (!disableCalculationExpander && !disableAllExcepySalarySpecificationSettings)
                            {
                                settings.ForEach(s => s.WorkingTimePromoted = field.BoolValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.CalculateSupplementCharge:
                            if (!disableCalculationExpander && !disableAllExcepySalarySpecificationSettings)
                            {
                                settings.ForEach(s => s.CalculateSupplementCharge = field.BoolValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.CalculateSicknessSalary:
                            if (!disableCalculationExpander && !disableAllExcepySalarySpecificationSettings)
                            {
                                settings.ForEach(s => s.CalculateSicknessSalary = field.BoolValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.PensionCompany:
                            if (!disableCalculationExpander && !disableAllExcepySalarySpecificationSettings)
                            {
                                settings.ForEach(s => s.PensionCompany = field.IntValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.TimeUnit:
                            if ((isAbsenceVacation || isVacationAddition || isVacationSalary) && field.IntValue == (int)TermGroup_PayrollProductTimeUnit.CalenderDayFactor)
                                break;
                            else if (field.IntValue == (int)TermGroup_PayrollProductTimeUnit.VacationCoefficient)
                                break;

                            if (product.ResultType != (int)TermGroup_PayrollResultType.Quantity && !disableAllExcepySalarySpecificationSettings && !disableCalculationExpander)
                            {
                                settings.ForEach(s => s.TimeUnit = field.IntValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.TaxCalculationType:
                            if (!disableAllExcepySalarySpecificationSettings && !disableCalculationExpander)
                            {
                                settings.ForEach(s => s.TaxCalculationType = field.IntValue);
                                updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            }
                            break;
                        case BatchUpdatePayrollProduct.DontIncludeInAbsenceCost:
                            settings.ForEach(s => s.DontIncludeInAbsenceCost = field.BoolValue);
                            updatedSettings.AddRange(settings.Select(s => s.PayrollGroupId));
                            break;
                        case BatchUpdatePayrollProduct.AccountInternal:
                            if (field.IntValue > 0)
                            {
                                if (field.IntValue == product.ProductId)
                                    continue;

                                var payrollProduct = ProductManager.GetPayrollProduct(entities, field.IntValue, loadSettings: true, loadAccounts: true);
                                foreach (PayrollProductSetting sourceSetting in payrollProduct.PayrollProductSetting)
                                {
                                    var sourcePayrollProductStandard = sourceSetting.PayrollProductAccountStd.FirstOrDefault();

                                    if (sourcePayrollProductStandard == null)
                                        continue;

                                    var targetSetting = settings.FirstOrDefault(s => s.PayrollGroupId == sourceSetting.PayrollGroupId);
                                    if (targetSetting == null)
                                        continue;

                                    foreach (PayrollProductAccountStd productAccountStd in targetSetting.PayrollProductAccountStd.ToList())
                                    {
                                        //Delete account
                                        productAccountStd.AccountInternal.Clear();
                                        targetSetting.PayrollProductAccountStd.Remove(productAccountStd);
                                        entities.DeleteObject(productAccountStd);
                                    }

                                    PayrollProductAccountStd newPayrollProductAccountStd = new PayrollProductAccountStd
                                    {
                                        AccountId = sourcePayrollProductStandard.AccountId,
                                        Type = sourcePayrollProductStandard.Type,
                                        Percent = sourcePayrollProductStandard.Percent
                                    };

                                    newPayrollProductAccountStd.AccountInternal.AddRange(sourcePayrollProductStandard.AccountInternal);

                                    targetSetting.PayrollProductAccountStd.Add(newPayrollProductAccountStd);

                                    entities.AddToPayrollProductAccountStd(newPayrollProductAccountStd);
                                    updatedSettings.Add(targetSetting.PayrollGroupId);
                                }
                            }
                            break;
                        case BatchUpdatePayrollProduct.AccountingPrio:
                            if (field.IntValue > 0)
                            {
                                if (field.IntValue == product.ProductId)
                                    continue;

                                var payrollProduct = ProductManager.GetPayrollProduct(entities, field.IntValue, loadSettings: true);
                                foreach (PayrollProductSetting sourceSetting in payrollProduct.PayrollProductSetting)
                                {

                                    var targetSetting = settings.FirstOrDefault(s => s.PayrollGroupId == sourceSetting.PayrollGroupId);
                                    if (targetSetting == null)
                                        continue;

                                    targetSetting.AccountingPrio = sourceSetting.AccountingPrio;
                                    updatedSettings.Add(targetSetting.PayrollGroupId);
                                }
                            }
                            break;
                        case BatchUpdatePayrollProduct.PayrollProductPriceTypesAndFormulas:
                            foreach (PayrollProductSetting targetSetting in settings)
                            {
                                decimal? decimalValue = field.GetFirstChild(BatchUpdateFieldType.DecimalNull)?.DecimalValue ?? null;
                                if (field.IntValue > 0) // PayrollPriceTypeId
                                    CreatePayrollProductPriceTypes(targetSetting, field.IntValue, field.FromDate, decimalValue);

                                else if (field.IntValue < 0) // PayrollPriceFormulaId
                                    CreatePayrollProductPriceFormulas(targetSetting, Math.Abs(field.IntValue), field.FromDate);

                                updatedSettings.Add(targetSetting.PayrollGroupId);
                            }
                            break;
                    }
                }
                SetModifiedProperties(product);

                foreach (var setting in settings)
                {
                    if (setting.PayrollProductSettingId > 0 && updatedSettings.Contains(setting.PayrollGroupId))
                        SetModifiedProperties(setting);

                    else if (setting.PayrollProductSettingId == 0 && !updatedSettings.Contains(setting.PayrollGroupId))
                        setting.State = (int)SoeEntityState.Deleted;
                }
            }

            return result;
        }
        #endregion

        #region Supplier
        public List<BatchUpdateDTO> GetSupplierBatchUpdate(CompEntities entities)
        {
            var batchUpdateData = new List<BatchUpdateDTO>();

            batchUpdateData.Add(
                GetYesNoAlternative((int)BatchUpdateSupplier.Active, 3273)
            );
            batchUpdateData.Add(
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateSupplier.VatType,
                    Label = GetText(3520, TermGroup.General),
                    Options = GetTermGroup(TermGroup.InvoiceVatType, true) // Supplier and Invoice both link to the same table, VatCode.
                });
            batchUpdateData.Add(
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateSupplier.PaymentCondition,
                    Label = GetText(3081, TermGroup.General),
                    Options = PaymentManager.GetPaymentConditions(entities, this.ActorCompanyId)
                        .Select(t => new NameAndIdDTO
                        {
                            Id = t.PaymentConditionId,
                            Name = t.Name
                        })
                        .ToList(),
                });
            batchUpdateData.Add(
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateSupplier.AttestWorkFlowGroup,
                    Label = GetText(4835, TermGroup.General),
                    Options = AttestManager.GetAttestWorkFlowGroups(entities, this.ActorCompanyId)
                        .Select(ag => new NameAndIdDTO()
                        {
                            Id = ag.AttestWorkFlowHeadId,
                            Name = ag.Name
                        })
                        .ToList()
                });
            batchUpdateData.Add(
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateSupplier.DeliveryType,
                    Label = GetText(3234, TermGroup.General),
                    Options = InvoiceManager.GetDeliveryTypes(entities, this.ActorCompanyId)
                        .Select(dt => new NameAndIdDTO()
                        {
                            Id = dt.DeliveryTypeId,
                            Name = dt.Name
                        })
                        .ToList()
                });
            batchUpdateData.Add(
                new BatchUpdateDTO()
                {
                    DataType = BatchUpdateFieldType.Id,
                    Field = (int)BatchUpdateSupplier.DeliveryCondition,
                    Label = GetText(3231, TermGroup.General),
                    Options = InvoiceManager.GetDeliveryConditions(entities, this.ActorCompanyId)
                        .Select(dc => new NameAndIdDTO()
                        {
                            Id = dc.DeliveryConditionId,
                            Name = dc.Name
                        })
                        .ToList()
                });

            return batchUpdateData.OrderBy(s => s.Label).ToList();
        }

        public ActionResult PerformSupplierBatchUpdate(CompEntities entities, List<BatchUpdateDTO> batchUpdateDTOs, List<int> supplierIds)
        {
            var result = new ActionResult();

            int actorCompanyId = base.ActorCompanyId;
            var suppliers = entities.Supplier
                .Where(s =>
                    s.ActorCompanyId == actorCompanyId &&
                    supplierIds.Contains(s.ActorSupplierId))
                .ToList();

            foreach (var supplier in suppliers)
            {
                foreach (var field in batchUpdateDTOs)
                {
                    switch ((BatchUpdateSupplier)field.Field)
                    {
                        case BatchUpdateSupplier.Active:
                            supplier.State = GetBoolFromYesNo(field.IntValue) ? (int)SoeEntityState.Active : (int)SoeEntityState.Inactive;
                            break;
                        case BatchUpdateSupplier.VatType:
                            supplier.VatType = field.IntValue;
                            break;
                        case BatchUpdateSupplier.PaymentCondition:
                            supplier.PaymentConditionId = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateSupplier.AttestWorkFlowGroup:
                            supplier.AttestWorkFlowGroupId = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateSupplier.DeliveryType:
                            supplier.DeliveryTypeId = NotZeroOrNull(field.IntValue);
                            break;
                        case BatchUpdateSupplier.DeliveryCondition:
                            supplier.DeliveryConditionId = NotZeroOrNull(field.IntValue);
                            break;
                    }
                }
                SetModifiedProperties(supplier);
            }

            return result;
        }
        #endregion

        #region Helpers
        private int? NotZeroOrNull(int val)
        {
            int? newVal = null;
            if (val > 0)
                newVal = val;
            return newVal;
        }
        private bool GetBoolFromYesNo(int value)
        {
            //1 = YES
            //2 = NO
            if (value == 2)
                return false;
            return true;
        }
        private BatchUpdateDTO GetYesNoAlternative(int field, int labelTermId)
        {
            return new BatchUpdateDTO()
            {
                DataType = BatchUpdateFieldType.Id,
                Field = field,
                IntValue = 1,
                Label = GetText(labelTermId),
                Options = GetTermGroup(TermGroup.YesNo)
            };
        }
        private List<NameAndIdDTO> GetTermGroup(TermGroup termGroup, bool addEmptyRow = false)
        {
            return GetTermGroupContent(termGroup, addEmptyRow: addEmptyRow).Select(e => new NameAndIdDTO { Id = e.Id, Name = e.Name }).ToList();
        }

        private void CreatePayrollProductPriceTypes(PayrollProductSetting targetSetting, int id, DateTime? fromDate, decimal? amount)
        {
            var priceType = targetSetting.PayrollProductPriceType.FirstOrDefault(w => w.PayrollPriceTypeId == id && w.State == (int)SoeEntityState.Active);
            if (priceType != null)
            {
                if (priceType.PayrollProductPriceTypePeriod.Any(w => w.FromDate == fromDate && w.State == (int)SoeEntityState.Active))
                    return;
            }

            PayrollProductPriceType newPriceType = new PayrollProductPriceType()
            {
                PayrollPriceTypeId = id,
            };
            SetCreatedProperties(newPriceType);
            targetSetting.PayrollProductPriceType.Add(newPriceType);

            PayrollProductPriceTypePeriod period = new PayrollProductPriceTypePeriod()
            {
                FromDate = fromDate,
                Amount = amount
            };
            SetCreatedProperties(period);
            newPriceType.PayrollProductPriceTypePeriod.Add(period);
        }

        private void CreatePayrollProductPriceFormulas(PayrollProductSetting targetSetting, int id, DateTime? fromDate)
        {
            if (targetSetting.PayrollProductPriceFormula.Any(w => w.PayrollPriceFormulaId == id && w.FromDate == fromDate && w.State == (int)SoeEntityState.Active))
                return;

            PayrollProductPriceFormula newPriceFormula = new PayrollProductPriceFormula()
            {
                PayrollPriceFormulaId = id,
                FromDate = fromDate,
            };
            SetCreatedProperties(newPriceFormula);
            targetSetting.PayrollProductPriceFormula.Add(newPriceFormula);
        }
        #endregion
    }
}
