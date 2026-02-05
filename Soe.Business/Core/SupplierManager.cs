using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
	public class SupplierManager : ManagerBase
	{
		#region Variables

		// Create a logger for use in this class
		private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Ctor

		public SupplierManager(ParameterObject parameterObject) : base(parameterObject) { }

		#endregion

		#region Supplier

		public List<Supplier> GetSuppliersByCompany(int actorCompanyId, bool onlyActive)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetSuppliersByCompany(entities, actorCompanyId, onlyActive);
		}

		public List<Supplier> GetSuppliersByCompany(CompEntities entities, int actorCompanyId, bool onlyActive)
		{
			IQueryable<Supplier> query = from s in entities.Supplier
						where s.ActorCompanyId == actorCompanyId
						select s;

			if (onlyActive)
				query = query.Where(s => s.State == (int)SoeEntityState.Active);
			else
				query = query.Where(s => s.State != (int)SoeEntityState.Deleted);

			var supplierList = query.ToList();
			return supplierList;
		}


		public List<SupplierSmallDTO> GetSupplierByCompanySmall(CompEntities entities, int actorCompanyId, bool onlyActive, int? actorSupplierId = null)
		{
			IQueryable<Supplier> query = (from s in entities.Supplier
										  where s.ActorCompanyId == actorCompanyId
										  select s);

			if (onlyActive)
				query = query.Where(s => s.State == (int)SoeEntityState.Active);
			else
				query = query.Where(s => s.State != (int)SoeEntityState.Deleted);

			if (actorSupplierId.HasValue)
			{
                query = query.Where(s=> s.ActorSupplierId == actorSupplierId.Value);
			}

			var supplierSmall = query.Select(c => new SupplierSmallDTO
			{
				ActorSupplierId = c.ActorSupplierId,
				Name = c.Name,
				SupplierNr = c.SupplierNr,
				CurrencyId = c.CurrencyId
			}).ToList();

			return supplierSmall;
		}

		public List<SupplierExtendedGridDTO> GetSuppliersByCompanyExtended(int actorCompanyId, bool onlyActive, int? supplierId = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetSuppliersByCompanyExtended(entities, actorCompanyId, onlyActive, supplierId);
		}

		public List<SupplierExtendedGridDTO> GetSuppliersByCompanyExtended(CompEntities entities, int actorCompanyId, bool onlyActive, int? supplierId = null)
		{
			List<SupplierExtendedGridDTO> suppliers = new List<SupplierExtendedGridDTO>();

			//Get categoryRecords
			List<CompanyCategoryRecord> categoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Supplier, SoeCategoryRecordEntity.Supplier, actorCompanyId);

			IQueryable<Supplier> query = (from s in entities.Supplier
					.Include("PaymentCondition")
					.Include("Actor.Contact.ContactECom")
					.Include("Actor.Contact.ContactAddress.ContactAddressRow")
					.Include("Actor.PaymentInformation.PaymentInformationRow")
										  where s.ActorCompanyId == actorCompanyId
										  select s);

			if (onlyActive)
                query = query.Where(s=> s.State == (int)SoeEntityState.Active);
			else
				query = query.Where(s => s.State == (int)SoeEntityState.Active || s.State == (int)SoeEntityState.Inactive);

			if (supplierId.HasValue)
				query = query.Where(s => s.ActorSupplierId == supplierId);

			foreach (var supplier in query.ToList())
			{
				var dto = new SupplierExtendedGridDTO()
				{
					ActorSupplierId = supplier.ActorSupplierId,
					SupplierNr = supplier.SupplierNr,
					Name = supplier.Name,
					OrgNr = supplier.OrgNr,
					State = (SoeEntityState)supplier.State,
					IsPrivatePerson = supplier.IsPrivatePerson,
					PaymentCondition = supplier.PaymentCondition != null ? supplier.PaymentCondition.Name : String.Empty,
					Email = ContactManager.GetContactEcoms(supplier.Actor.Contact.FirstOrDefault(), (int)TermGroup_SysContactEComType.Email).FirstOrDefault()?.Text,
					HomePhone = ContactManager.GetContactEcoms(supplier.Actor.Contact.LastOrDefault(), (int)TermGroup_SysContactEComType.PhoneHome).FirstOrDefault()?.Text,
					WorkPhone = ContactManager.GetContactEcoms(supplier.Actor.Contact.LastOrDefault(), (int)TermGroup_SysContactEComType.PhoneJob).FirstOrDefault()?.Text,
					MobilePhone = ContactManager.GetContactEcoms(supplier.Actor.Contact.LastOrDefault(), (int)TermGroup_SysContactEComType.PhoneMobile).FirstOrDefault()?.Text,
				};

				foreach (var info in supplier.Actor.PaymentInformation.Where(i => i.State == (int)SoeEntityState.Active))
				{

					var defaultInfoRow = info.PaymentInformationRow.FirstOrDefault(x => x.Default && x.State == (int)SoeEntityState.Active);
					dto.PayToAccount = defaultInfoRow == null ? "" : defaultInfoRow.PaymentNr;
				}

				var categories = CategoryManager.GetCategories(categoryRecords, supplier.ActorSupplierId);

				dto.Categories = string.Join(", ", categories.Select(x => x.Name).ToList());

				suppliers.Add(dto);
			}

			return suppliers.OrderBy(s => s.SupplierNr).ToList();
		}

		public List<Supplier> GetSuppliers(int actorCompanyId, bool loadActor, bool loadAccount, bool loadContactAddresses, bool loadCategories, bool loadPaymentInformation, bool loadTemplateAttestHead, List<int> supplierIds = null, string orgNr = null, bool includeDeleted = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Supplier.NoTracking();
            return GetSuppliers(entities, actorCompanyId, loadActor, loadAccount, loadContactAddresses, loadCategories, loadPaymentInformation, loadTemplateAttestHead, supplierIds, orgNr, includeDeleted);
		}

		public List<Supplier> GetSuppliers(CompEntities entities, int actorCompanyId, bool loadActor, bool loadAccount, bool loadContactAddresses, bool loadCategories, bool loadPaymentInformation, bool loadTemplateAttestHead, List<int> supplierIds = null, string orgNr = null, bool includeDeleted = false)
		{
			IQueryable<Supplier> query = entities.Supplier;
			if (loadActor)
				query = query.Include("Actor").Include("Actors");

			if (loadAccount)
				query = query.Include("SupplierAccountStd.AccountStd.Account.AccountDim").Include("SupplierAccountStd.AccountInternal.Account.AccountDim");

			if (loadContactAddresses)
				query = query.Include("Actor.Contact.ContactECom").Include("Actor.Contact.ContactAddress.ContactAddressRow");

			if (loadPaymentInformation)
				query = query.Include("Actor.PaymentInformation.PaymentInformationRow");

			if (loadTemplateAttestHead)
				query = query.Include("TemplateAttestHead");


			List<Supplier> suppliers;
			if (!string.IsNullOrEmpty(orgNr))
			{
				var supps = (from s in query
							 where s.ActorCompanyId == actorCompanyId &&
							 s.OrgNr == orgNr
							 select s);

				if (!includeDeleted)
					suppliers = supps.Where(s => s.State != (int)SoeEntityState.Deleted).ToList();
				else
					suppliers = supps.ToList();
			}
			else if (supplierIds != null && supplierIds.Count > 0)
			{
				var supps = (from s in query
							 where s.ActorCompanyId == actorCompanyId &&
							 supplierIds.Contains(s.ActorSupplierId)
							 select s).ToList();

				if (!includeDeleted)
					suppliers = supps.Where(s => s.State != (int)SoeEntityState.Deleted).ToList();
				else
					suppliers = supps.ToList();
			}
			else
			{
				var supps = (from s in query
							 where s.ActorCompanyId == actorCompanyId
							 select s).ToList();

				if (!includeDeleted)
					suppliers = supps.Where(s => s.State != (int)SoeEntityState.Deleted).ToList();
				else
					suppliers = supps.ToList();
			}

			return suppliers;
		}


		public List<Supplier> GetSuppliersBySearch(int actorCompanyId, string search, int take = 200)
		{
			List<Supplier> suppliers = GetSuppliersByCompany(actorCompanyId, true);

			return (from s in suppliers
					where (s.Name.ToLower().Contains(search.ToLower()) || s.SupplierNr.ToLower() == search.ToLower())
					orderby s.Name ascending
					select s).Take(take).ToList();
		}

		public List<Supplier> GetSuppliersBySearch(int actorCompanyId, string supplierNr, string supplierName, int take = 200)
		{
			List<Supplier> suppliers = GetSuppliersByCompany(actorCompanyId, true);

			if (supplierNr != "" && supplierName != "")
			{
				return (from s in suppliers
						where (s.Name != null && s.Name.ToLower().Contains(supplierName.ToLower())) &&
							  (s.SupplierNr != null && s.SupplierNr.ToLower().Contains(supplierNr.ToLower()))
						orderby s.Name ascending
						select s).Take(take).ToList();
			}
			else if (supplierNr == "" && supplierName != "")
			{
				return (from s in suppliers
						where (s.Name != null && s.Name.ToLower().Contains(supplierName.ToLower()))
						orderby s.Name ascending
						select s).Take(take).ToList();
			}
			else if (supplierNr != "" && supplierName == "")
			{
				return (from s in suppliers
						where (s.SupplierNr != null && s.SupplierNr.ToLower().Contains(supplierNr.ToLower()))
						orderby s.Name ascending
						select s).Take(take).ToList();
			}
			return null;
		}

		public Dictionary<int, string> GetSuppliersByCompanyDict(int actorCompanyId, bool onlyActive, bool addEmptyRow)
		{
			Dictionary<int, string> dict = new Dictionary<int, string>();
			if (addEmptyRow)
				dict.Add(0, " ");

			if (!UserManager.UserValidOnCompany(actorCompanyId, base.UserId))
				return dict;

			List<Supplier> suppliers = GetSuppliersByCompany(actorCompanyId, onlyActive);
			dict.AddRange(suppliers.ToDictionary(i => i.ActorSupplierId, i => i.SupplierNr + " " + i.Name?.Trim()));

			return dict;
		}

		public string GetSupplierName(CompEntities entities, int actorSupplierId, int actorCompanyId)
		{
			return (from s in entities.Supplier
					where s.ActorSupplierId == actorSupplierId && s.ActorCompanyId == actorCompanyId &&
						  s.State != (int)SoeEntityState.Deleted
					select s.Name).FirstOrDefault();
		}

		public int GetSupplierVatType(CompEntities entities, int actorSupplierId, int actorCompanyId)
		{
			return (from s in entities.Supplier
					where s.ActorSupplierId == actorSupplierId && s.ActorCompanyId == actorCompanyId &&
						  s.State != (int)SoeEntityState.Deleted
					select s.VatType).FirstOrDefault();
		}

		public List<AccountingSettingsRowDTO> GetSupplierAccountSettings(int actorCompanyId, int supplierId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetSupplierAccountSettings(entities, actorCompanyId, supplierId);
        }
		public List<AccountingSettingsRowDTO> GetSupplierAccountSettings(CompEntities entities, int actorCompanyId, int supplierId)
		{
            var accountSettings = entities.SupplierAccountStd
				.Where(s => 
						s.Supplier.ActorCompanyId == actorCompanyId && 
						s.Supplier.ActorSupplierId == supplierId
                )
                .Include("AccountStd")
				.Include("AccountStd.Account")
				.Include("AccountInternal")
				.Include("AccountInternal.Account")
                .ToList();

			var accountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, 
				onlyInternal: true, 
				active: null);

			return GetSupplierAccountSettings(accountDims, accountSettings);
        }
		public Dictionary<int, List<AccountingSettingsRowDTO>> GetSuppliersAccountSettings(CompEntities entities, int actorCompanyId, List<int> supplierIds)
		{
            var accountSettings = entities.SupplierAccountStd
				.Where(s => 
						s.Supplier.ActorCompanyId == actorCompanyId && 
						supplierIds.Contains(s.Supplier.ActorSupplierId)
				)
                .Include("AccountStd")
				.Include("AccountStd.Account")
				.Include("AccountInternal")
				.Include("AccountInternal.Account")
                .ToList();

			var accountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, 
				onlyInternal: true, 
				active: null);

			var dict = new Dictionary<int, List<AccountingSettingsRowDTO>>();
            foreach (int supplierId in supplierIds)
			{
				dict.Add(supplierId, GetSupplierAccountSettings(accountDims, accountSettings));
			}
			return dict;
        }
		public List<AccountingSettingsRowDTO> GetSupplierAccountSettings(List<AccountDim> internalDims, List<SupplierAccountStd> supplierAccounts)
		{
			var dimNrs = internalDims 
				.OrderBy(d => d.AccountDimNr)
				.Select(d => d.AccountDimId)
				.ToList();

			var accountTypes = new SupplierAccountType[]
			{
				SupplierAccountType.Credit,
				SupplierAccountType.Debit,
				SupplierAccountType.VAT,
				SupplierAccountType.Interim
			};

			var accountSettingRows = new List<AccountingSettingsRowDTO>();
			foreach (var type in accountTypes)
			{
                var accountSettingRow = new AccountingSettingsRowDTO()
                {
                    Type = (int)type,
                    Percent = 0
                };
                accountSettingRows.Add(accountSettingRow);

                var accountMapper = supplierAccounts 
					.Where(s => s.Type == (int)type)
                    .FirstOrDefault();

				Account accountStd = accountMapper?.AccountStd?.Account;
                if (accountStd != null)
                {
                    accountSettingRow.AccountDim1Nr = Constants.ACCOUNTDIM_STANDARD;
                    accountSettingRow.Account1Id = accountStd.AccountId;
                    accountSettingRow.Account1Nr = accountStd.AccountNr;
                    accountSettingRow.Account1Name = accountStd.Name;
				}

                if (accountMapper != null && accountMapper.AccountInternal != null)
                {
                    foreach (var accInt in accountMapper.AccountInternal)
                    {
						var index = dimNrs.IndexOf(accInt.Account.AccountDimId);
						if (index == -1) continue;
						var dimCounter = index + 2; // +2 because zero-based and index 0 is accountStd
                        var account = accInt.Account;

                        if (dimCounter == 2)
                        {
                            accountSettingRow.AccountDim2Nr = account.AccountDim.AccountDimNr;
                            accountSettingRow.Account2Id = account.AccountId;
                            accountSettingRow.Account2Nr = account.AccountNr;
                            accountSettingRow.Account2Name = account.Name;
                        }
                        else if (dimCounter == 3)
                        {
                            accountSettingRow.AccountDim3Nr = account.AccountDim.AccountDimNr;
                            accountSettingRow.Account3Id = account.AccountId;
                            accountSettingRow.Account3Nr = account.AccountNr;
                            accountSettingRow.Account3Name = account.Name;
                        }
                        else if (dimCounter == 4)
                        {
                            accountSettingRow.AccountDim4Nr = account.AccountDim.AccountDimNr;
                            accountSettingRow.Account4Id = account.AccountId;
                            accountSettingRow.Account4Nr = account.AccountNr;
                            accountSettingRow.Account4Name = account.Name;
                        }
                        else if (dimCounter == 5)
                        {
                            accountSettingRow.AccountDim5Nr = account.AccountDim.AccountDimNr;
                            accountSettingRow.Account5Id = account.AccountId;
                            accountSettingRow.Account5Nr = account.AccountNr;
                            accountSettingRow.Account5Name = account.Name;
                        }
                        else if (dimCounter == 6)
                        {
                            accountSettingRow.AccountDim6Nr = account.AccountDim.AccountDimNr;
                            accountSettingRow.Account6Id = account.AccountId;
                            accountSettingRow.Account6Nr = account.AccountNr;
                            accountSettingRow.Account6Name = account.Name;
                        }

                        dimCounter++;
                    }
                }
            }
			return accountSettingRows;
        }

		public Supplier GetSupplier(int actorSupplierId, bool loadActor = false, bool loadAccount = false, bool loadContactAddresses = false, bool loadCategories = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Supplier.NoTracking();
            return GetSupplier(entities, actorSupplierId, loadActor, loadAccount, loadContactAddresses, loadCategories);
		}

		public Supplier GetSupplier(CompEntities entities, int actorSupplierId, bool loadActor = false, bool loadAccount = false, bool loadContactAddresses = false, bool loadCategories = false, int actorCompanyId = 0)
		{
			if (actorCompanyId == 0)
			{
				actorCompanyId = ActorCompanyId;
			}

			IQueryable<Supplier> query = entities.Supplier;
			if (loadActor)
				query = query.Include("Actor").Include("Actors").Include("Actor.ActorConsent");

			if (loadAccount)
				query = query.Include("SupplierAccountStd.AccountStd.Account.AccountDim").Include("SupplierAccountStd.AccountInternal.Account.AccountDim");

			if (loadContactAddresses)
				query = query.Include("Actor.Contact.ContactECom").Include("Actor.Contact.ContactAddress.ContactAddressRow");


			Supplier supplier = (from s in query
								 where s.ActorSupplierId == actorSupplierId && s.ActorCompanyId == actorCompanyId &&
								 s.State != (int)SoeEntityState.Deleted
								 select s).FirstOrDefault();

			if (supplier != null)
			{
				if (loadCategories)
				{
					supplier.CategoryIds = (from c in entities.CompanyCategoryRecord
											where c.RecordId == supplier.ActorSupplierId &&
											c.Entity == (int)SoeCategoryRecordEntity.Supplier &&
											c.Category.Type == (int)SoeCategoryType.Supplier &&
											c.Category.ActorCompanyId == ActorCompanyId &&
											c.Category.State == (int)SoeEntityState.Active
											select c.CategoryId).ToList();
				}
				if (loadCategories)
				{
					PaymentInformation paymentInformation = (from pi in entities.PaymentInformation
															 where pi.Actor.ActorId == supplier.ActorSupplierId &&
															 pi.State == (int)SoeEntityState.Active
															 select pi).FirstOrDefault();

					if (paymentInformation != null)
					{
						if (!paymentInformation.PaymentInformationRow.IsLoaded)
							paymentInformation.PaymentInformationRow.Load();
						supplier.PaymentInformation = paymentInformation;
					}


				}

				supplier.TemplateAttestHead = AttestManager.GetAttestWorkFlowHeadFromType(entities, supplier.ActorSupplierId, SoeEntityType.Supplier);

				if (supplier.SysCountryId.HasValue)
				{
					var countries = CountryCurrencyManager.GetEUSysCountrieIds(DateTime.Today);
					supplier.IsEUCountryBased = countries.Contains(supplier.SysCountryId.Value);
				}
			}

			return supplier;
		}

		public Supplier GetSupplierBySupplierNr(int actorCompanyId, string supplierNr, bool onlyActive)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Supplier.NoTracking();
            return GetSupplierBySupplierNr(entities, actorCompanyId, supplierNr, onlyActive);
		}

		public Supplier GetSupplierBySupplierNr(CompEntities entities, int actorCompanyId, string supplierNr, bool onlyActive, List<Supplier> suppliers = null)
		{
			if (string.IsNullOrEmpty(supplierNr))
				return null;

			if (suppliers == null)
				suppliers = GetSuppliersByCompany(entities, actorCompanyId, onlyActive: onlyActive);

			return suppliers.FirstOrDefault(c => c.SupplierNr.ToLower() == supplierNr.ToLower());
		}

		public Supplier GetSupplierByOrgNr(CompEntities entities, int actorCompanyId, string orgNr)
		{
			if (string.IsNullOrEmpty(orgNr))
				return null;

			return (from s in entities.Supplier
					where (s.ActorCompanyId == actorCompanyId) && (s.OrgNr.Replace("-", "") == orgNr.Replace("-", "")) &&
					(s.State == (int)SoeEntityState.Active)
					select s).FirstOrDefault();
		}

		public Supplier GetSupplierByVatNr(CompEntities entities, int actorCompanyId, string vatNr)
		{
			if (string.IsNullOrEmpty(vatNr))
				return null;

			return (from s in entities.Supplier
					where (s.ActorCompanyId == actorCompanyId) && (s.VatNr.Replace("-", "") == vatNr.Replace("-", "")) &&
					(s.State == (int)SoeEntityState.Active)
					select s).FirstOrDefault();
		}

		public Supplier GetSupplierByPrio(int actorCompanyId, string name, string orgNr, string bankGiro, string postalGiro, string iban, string bic)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Supplier.NoTracking();
            return GetSupplierByPrio(entities, actorCompanyId, name, orgNr, bankGiro, postalGiro, iban, bic);
		}

		public Supplier GetSupplierByPrio(CompEntities entities, int actorCompanyId, string name, string orgNr, string bankGiro, string postalGiro, string iban, string bic)
		{
			//Try first to only fetch EDI suppliers
			Supplier supplier = GetSupplierByPrio(entities, actorCompanyId, name, orgNr, bankGiro, postalGiro, iban, bic, true);
			if (supplier == null)
				supplier = GetSupplierByPrio(entities, actorCompanyId, name, orgNr, bankGiro, postalGiro, iban, bic, false);
			return supplier;
		}

		public Supplier GetSupplierByPrio(CompEntities entities, int actorCompanyId, string name, string orgNr, string bankGiro, string postalGiro, string iban, string bic, bool onlySearchEDISuppliers)
		{
			Supplier supplier = null;

			var filteredItems = new List<GetSupplierIdentificationResult>();
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var allItems = entitiesReadOnly.GetSupplierIdentification(actorCompanyId).ToList();

			if (allItems.Count > 0)
			{
				#region OrgNr

				if (filteredItems.Count == 0)
				{
					//Search - no items found earlier
					filteredItems.AddRange(FilterSuppliersItemByOrgNr(allItems, orgNr, onlySearchEDISuppliers));
				}
				else if (filteredItems.Count == 1)
				{
					//Do nothing - 1 item found
				}
				else if (filteredItems.Count > 1)
				{
					//Filter - items found earlier
					var items = FilterSuppliersItemByOrgNr(filteredItems, orgNr, onlySearchEDISuppliers);
					if (items.Count > 0 && items.Count != filteredItems.Count)
					{
						filteredItems.Clear();
						filteredItems.AddRange(items);
					}
				}

				#endregion

				#region BankGiro

				if (filteredItems.Count == 0)
				{
					//Search - no items found earlier
					filteredItems.AddRange(FilterSuppliersItemByBankGiro(allItems, bankGiro, onlySearchEDISuppliers));
				}
				else if (filteredItems.Count == 1)
				{
					//Do nothing - 1 item found
				}
				else if (filteredItems.Count > 1)
				{
					//Filter - items found earlier
					var items = FilterSuppliersItemByBankGiro(filteredItems, bankGiro, onlySearchEDISuppliers);
					if (items.Count > 0 && items.Count != filteredItems.Count)
					{
						filteredItems.Clear();
						filteredItems.AddRange(items);
					}
				}

				#endregion

				#region PostalGiro

				if (filteredItems.Count == 0)
				{
					//Search - no items found earlier
					filteredItems.AddRange(FilterSuppliersItemByPostalGiro(allItems, postalGiro, onlySearchEDISuppliers));
				}
				else if (filteredItems.Count == 1)
				{
					//Do nothing - 1 item found
				}
				else if (filteredItems.Count > 1)
				{
					//Filter - items found earlier
					var items = FilterSuppliersItemByPostalGiro(filteredItems, postalGiro, onlySearchEDISuppliers);
					if (items.Count > 0 && items.Count != filteredItems.Count)
					{
						filteredItems.Clear();
						filteredItems.AddRange(items);
					}
				}

				#endregion

				#region IBAN

				if (filteredItems.Count == 0)
				{
					//Search - no items found earlier
					filteredItems.AddRange(FilterSuppliersItemByIban(allItems, iban, onlySearchEDISuppliers));
				}
				else if (filteredItems.Count == 1)
				{
					//Do nothing - 1 item found
				}
				else if (filteredItems.Count > 1)
				{
					//Filter - items found earlier
					var items = FilterSuppliersItemByIban(filteredItems, iban, onlySearchEDISuppliers);
					if (items.Count > 0 && items.Count != filteredItems.Count)
					{
						filteredItems.Clear();
						filteredItems.AddRange(items);
					}
				}

				#endregion

				#region Name

				if (filteredItems.Count == 0)
				{
					//Search - no items found earlier
					filteredItems.AddRange(FilterSuppliersItemByName(allItems, name, onlySearchEDISuppliers));
				}
				else if (filteredItems.Count == 1)
				{
					//Do nothing - 1 item found
				}
				else if (filteredItems.Count > 1)
				{
					//Filter - items found earlier
					var items = FilterSuppliersItemByName(filteredItems, name, onlySearchEDISuppliers);
					if (items.Count > 0 && items.Count != filteredItems.Count)
					{
						filteredItems.Clear();
						filteredItems.AddRange(items);
					}
				}

				#endregion

				if (filteredItems.Count > 0)
					supplier = GetSupplier(entities, filteredItems.First().ActorSupplierId);
			}

			return supplier;
		}

		public Supplier GetSupplierBySysWholeseller(int actorCompanyId, int sysWholesellerId)
		{
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from s in entitiesReadOnly.Supplier
					where
					s.ActorCompanyId == actorCompanyId &&
					s.SysWholeSellerId == sysWholesellerId &&
					s.State == (int)SoeEntityState.Active
					select s).FirstOrDefault();
		}

		public int? GetSupplierIdBySysWholeseller(int actorCompanyId, int sysWholesellerId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from s in entitiesReadOnly.Supplier
					where
					s.ActorCompanyId == actorCompanyId &&
					s.SysWholeSellerId == sysWholesellerId &&
					s.State == (int)SoeEntityState.Active
					select s.ActorSupplierId).FirstOrDefault();
		}

		public string GetNextSupplierNr(int actorCompanyId)
		{
			int lastNr = 0;
			List<Supplier> suppliers = GetSuppliersByCompany(actorCompanyId, onlyActive: false);

			if (suppliers.Count > 0)
			{
				Int32.TryParse(suppliers.Last().SupplierNr, out lastNr);
				// If unable to parse, numeric values are not used
				if (lastNr == 0)
					return string.Empty;
			}

			lastNr++;

			// Check that number is not used
			if (suppliers.Any(s => s.SupplierNr == lastNr.ToString()))
				return string.Empty;

			return lastNr.ToString();
		}

		public bool SupplierExist(Supplier supplier, int actorCompanyId)
		{
			if (supplier == null)
				return false;

			return SupplierExist(supplier.SupplierNr, actorCompanyId);
		}

		public bool SupplierExist(string supplierNr, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Supplier.NoTracking();
            return SupplierExist(entities, supplierNr, actorCompanyId);
		}

		public bool SupplierExist(CompEntities entities, string supplierNr, int actorCompanyId)
		{
			return (from s in entities.Supplier
					where s.SupplierNr == supplierNr &&
					s.State != (int)SoeEntityState.Deleted &&
					s.ActorCompanyId == actorCompanyId
					select s).Any();
		}

		public bool SupplierHasInvoices(int supplierId, int actorCompanyId)
		{
			return InvoiceManager.InvoicesExistForActor(SoeOriginType.SupplierInvoice, supplierId, actorCompanyId);
		}

		public ActionResult AddSupplier(CompEntities entities, Supplier supplier, int? paymentConditionId, int? factoringSupplierId, int currencyId, int actorCompanyId, int? vatCodeId)
		{
			if (supplier == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "Supplier");

			SetCreatedProperties(supplier);

			Actor actor = new Actor()
			{
				ActorType = (int)SoeActorType.Supplier,

				//Set references
				Supplier = supplier,
			};

			if (SupplierExist(actor.Supplier, actorCompanyId))
				return new ActionResult((int)ActionResultSave.SupplierExists);

			//Set FK
			supplier.PaymentConditionId = paymentConditionId.ToNullable();
			supplier.FactoringSupplierId = factoringSupplierId.ToNullable();
			supplier.CurrencyId = currencyId;
			supplier.VatCodeId = vatCodeId;
			supplier.ActorCompanyId = actorCompanyId;

			return AddEntityItem(entities, actor, "Actor");
		}

		public ActionResult UpdateSupplier(CompEntities entities, Supplier supplier, int? paymentConditionId, int? factoringSupplierId, int currencyId, int? vatCodeId)
		{
			if (supplier == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "Supplier");

			Supplier originalSupplier = GetSupplier(entities, supplier.ActorSupplierId);
			if (originalSupplier == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, "Supplier");

			//Set FK
			originalSupplier.PaymentConditionId = paymentConditionId.ToNullable();
			originalSupplier.FactoringSupplierId = factoringSupplierId.ToNullable();
			originalSupplier.CurrencyId = currencyId;
			originalSupplier.VatCodeId = vatCodeId;

			return UpdateEntityItem(entities, originalSupplier, supplier, "Supplier");
		}

		public ActionResult MapSupplierToSysWholeseller(int supplierId, int sysWholesellerId, int actorCompanyId)
		{
			using (var entities = new CompEntities())
			{
				var suppliers = this.GetSuppliersByCompany(entities, actorCompanyId, true).Where(i => i.SysWholeSellerId == sysWholesellerId).ToList();

				foreach (var supp in suppliers)
				{
					supp.SysWholeSellerId = 0;
				}

				if (supplierId > 0)
				{
					var supplier = this.GetSupplier(entities, supplierId);
					supplier.SysWholeSellerId = sysWholesellerId;
				}

				return SaveChanges(entities);
			}
		}

		public ActionResult SaveTrackChangesFromIO(CompEntities entities, TransactionScope transaction, SupplierIO input, Supplier supplier, TermGroup_TrackChangesAction actionType, int actorCompanyId)
		{
			var supplierDto = new SupplierDTO
			{
				ActorSupplierId = input.SupplierId.GetValueOrDefault(),
				Name = input.Name,
				SupplierNr = input.SupplierNr,
				OrgNr = input.OrgNr,
				VatNr = input.VatNr,
				PaymentConditionId = supplier == null || !string.IsNullOrEmpty(input.PaymentConditionCode) ? PaymentManager.GetPaymentConditionId(entities, input.PaymentConditionCode, actorCompanyId) :
																											 supplier.PaymentConditionId
			};

			return SaveTrackChanges(entities, transaction, supplierDto, supplier, actionType, actorCompanyId);
		}

		public ActionResult SaveTrackChanges(CompEntities entities, TransactionScope transaction, SupplierDTO input, Supplier supplier, TermGroup_TrackChangesAction actionType, int actorCompanyId)
		{
			var trackStringFields = new[] {
				new SmallGenericType {Name= "SupplierNr", Id = (int)TermGroup_TrackChangesColumnType.Supplier_Nr },
				new SmallGenericType {Name= "Name", Id = (int)TermGroup_TrackChangesColumnType.Supplier_Name },
				new SmallGenericType {Name= "OrgNr", Id = (int)TermGroup_TrackChangesColumnType.Supplier_OrgNr },
				new SmallGenericType {Name= "VatNr", Id = (int)TermGroup_TrackChangesColumnType.Supplier_VatNr },
			}.ToList();

			var supplierId = actionType == TermGroup_TrackChangesAction.Insert ? input.ActorSupplierId : supplier.ActorSupplierId;

			var changes = TrackChangesManager.CreateTrackStringChanges(trackStringFields, actorCompanyId, SoeEntityType.Supplier, supplierId, supplier, input, actionType);

			if (
					(actionType == TermGroup_TrackChangesAction.Insert && input.PaymentConditionId.GetValueOrDefault() > 0) ||
					(actionType == TermGroup_TrackChangesAction.Delete && supplier.PaymentConditionId.GetValueOrDefault() > 0) ||
					(actionType == TermGroup_TrackChangesAction.Update && input.PaymentConditionId.GetValueOrDefault() != supplier.PaymentConditionId.GetValueOrDefault())
			  )
			{
				var fromValueName = supplier == null ? null : PaymentManager.GetPaymentConditionName(entities, supplier.PaymentConditionId.GetValueOrDefault(), actorCompanyId);
				var toValueName = input == null ? null : PaymentManager.GetPaymentConditionName(entities, input.PaymentConditionId.GetValueOrDefault(), actorCompanyId);
				changes.Add(TrackChangesManager.CreateTrackChangesDTO(actorCompanyId, SoeEntityType.Supplier, TermGroup_TrackChangesColumnType.Supplier_PaymentCondition, supplierId, supplier?.PaymentConditionId.ToString(), input?.PaymentConditionId.ToString(), actionType, SettingDataType.Integer, SoeEntityType.None, 0, fromValueName, toValueName));
			}

			return changes.Any() ? TrackChangesManager.AddTrackChanges(entities, transaction, changes) : new ActionResult();
		}

		public ActionResult SaveSupplier(SupplierDTO supplierInput, List<int> contactPersonIds, List<FileUploadDTO> files, int actorCompanyId, List<ExtraFieldRecordDTO> extraFields = null)
		{
			if (supplierInput == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "Supplier");

			// Default result is successful
			ActionResult result = new ActionResult();

			int supplierId = supplierInput.ActorSupplierId;

			using (var entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();

					using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
					{
						#region Supplier

						if (string.IsNullOrEmpty(supplierInput.SupplierNr) || string.IsNullOrEmpty(supplierInput.Name))
						{
							return new ActionResult((int)ActionResultSave.SupplierExists, GetText(8384, "Leveratörsnr eller leverantörsnamn ej angiven"));
						}

						// Get existing supplier (also if inactive)
						Supplier supplier = GetSupplier(entities, supplierId, true, true, true, true);

						// Check if supplier number already exists
						if (supplier == null || supplier.SupplierNr != supplierInput.SupplierNr)
						{
							if (SupplierExist(entities, supplierInput.SupplierNr, actorCompanyId))
								return new ActionResult((int)ActionResultSave.SupplierExists, GetText(11008, "En leverantör med angivet leverantörsnummer finns redan"));
						}

						if (supplier == null)
						{
							#region Supplier Add

							supplier = new Supplier
							{
								ActorCompanyId = actorCompanyId,
								VatType = (int)supplierInput.VatType,
								PaymentConditionId = supplierInput.PaymentConditionId.ToNullable(),
								FactoringSupplierId = supplierInput.FactoringSupplierId.ToNullable(),
								CurrencyId = supplierInput.CurrencyId,
								SysCountryId = supplierInput.SysCountryId,
								SysLanguageId = supplierInput.SysLanguageId,
								SupplierNr = supplierInput.SupplierNr.Trim(),
								Name = supplierInput.Name.Trim(),
								OrgNr = supplierInput.OrgNr,
								VatNr = supplierInput.VatNr,
								InvoiceReference = supplierInput.InvoiceReference,
								OurReference = supplierInput.OurReference,
								BIC = supplierInput.BIC,
								OurCustomerNr = supplierInput.OurCustomerNr,
								CopyInvoiceNrToOcr = supplierInput.CopyInvoiceNrToOcr,
								Interim = supplierInput.Interim,
								ManualAccounting = supplierInput.ManualAccounting,
								BlockPayment = supplierInput.BlockPayment,
								RiksbanksCode = supplierInput.RiksbanksCode,
								State = (int)supplierInput.State,
								IsEDISupplier = supplierInput.IsEDISupplier,
								ShowNote = supplierInput.ShowNote,
								Note = supplierInput.Note,
								SysWholeSellerId = supplierInput.SysWholeSellerId.ToNullable(),
								VatCodeId = supplierInput.VatCodeId.ToNullable(),
								AttestWorkFlowGroupId = supplierInput.AttestWorkFlowGroupId.ToNullable(),
								IsPrivatePerson = supplierInput.IsPrivatePerson,
								DeliveryTypeId = supplierInput.DeliveryTypeId.ToNullable(),
								DeliveryConditionId = supplierInput.DeliveryConditionId.ToNullable(),
								ContactEcomId = supplierInput.ContactEcomId.ToNullable(),
								IntrastatCodeId = supplierInput.IntrastatCodeId.ToNullable(),
							};
							SetCreatedProperties(supplier);

							#region Actor Add

							var actor = new Actor()
							{
								ActorType = (int)SoeActorType.Supplier,

								//Set references
								Supplier = supplier,
							};

							result = AddEntityItem(entities, actor, "Actor");
							if (result.Success == false)
							{
								result.ErrorNumber = (int)ActionResultSave.SupplierNotSaved;
								result.ErrorMessage = GetText(11009, "Leverantör kunde inte sparas");
								return result;
							}

							supplierId = supplierInput.ActorSupplierId = supplier.ActorSupplierId;

							#endregion

							result = SaveTrackChanges(entities, transaction, supplierInput, null, TermGroup_TrackChangesAction.Insert, actorCompanyId);
							if (!result.Success)
							{
								return result;
							}

							#endregion

						}
						else
						{
							#region Supplier Update

							result = SaveTrackChanges(entities, transaction, supplierInput, supplier, TermGroup_TrackChangesAction.Update, actorCompanyId);
							if (!result.Success)
							{
								return result;
							}

							// Update Supplier
							supplier.VatType = (int)supplierInput.VatType;
							supplier.PaymentConditionId = supplierInput.PaymentConditionId.ToNullable();
							supplier.FactoringSupplierId = supplierInput.FactoringSupplierId.ToNullable();
							supplier.CurrencyId = supplierInput.CurrencyId;
							supplier.SysCountryId = supplierInput.SysCountryId.ToNullable();
							supplier.SysLanguageId = supplierInput.SysLanguageId.ToNullable();
							supplier.SupplierNr = supplierInput.SupplierNr.Trim();
							supplier.Name = supplierInput.Name.Trim();
							supplier.OrgNr = supplierInput.OrgNr;
							supplier.VatNr = supplierInput.VatNr;
							supplier.InvoiceReference = supplierInput.InvoiceReference;
							supplier.OurReference = supplierInput.OurReference;
							supplier.BIC = supplierInput.BIC;
							supplier.OurCustomerNr = supplierInput.OurCustomerNr;
							supplier.CopyInvoiceNrToOcr = supplierInput.CopyInvoiceNrToOcr;
							supplier.Interim = supplierInput.Interim;
							supplier.ManualAccounting = supplierInput.ManualAccounting;
							supplier.BlockPayment = supplierInput.BlockPayment;
							supplier.RiksbanksCode = supplierInput.RiksbanksCode;
							supplier.State = (int)supplierInput.State;
							supplier.IsEDISupplier = supplierInput.IsEDISupplier;
							supplier.ShowNote = supplierInput.ShowNote;
							supplier.Note = supplierInput.Note;
							supplier.SysWholeSellerId = supplierInput.SysWholeSellerId.ToNullable();
							supplier.VatCodeId = supplierInput.VatCodeId.ToNullable();
							supplier.AttestWorkFlowGroupId = supplierInput.AttestWorkFlowGroupId.ToNullable();
							supplier.IsPrivatePerson = supplierInput.IsPrivatePerson;
							supplier.DeliveryTypeId = supplierInput.DeliveryTypeId.ToNullable();
							supplier.DeliveryConditionId = supplierInput.DeliveryConditionId.ToNullable();
							supplier.ContactEcomId = supplierInput.ContactEcomId.ToNullable();
							supplier.IntrastatCodeId = supplierInput.IntrastatCodeId.ToNullable();

							SetModifiedProperties(supplier);

							result = SaveChanges(entities, transaction);
							if (!result.Success)
							{
								result.ErrorNumber = (int)ActionResultSave.SupplierNotUpdated;
								result.ErrorMessage = GetText(11009, "Leverantör kunde inte sparas");
								return result;
							}

							#endregion
						}

						#endregion

						if (supplier.IsPrivatePerson.HasValue && supplier.IsPrivatePerson.Value)
						{
							var consent = supplier.Actor.ActorConsent.FirstOrDefault(a => a.ConsentType == (int)ActorConsentType.Unspecified);
							if (consent == null)
							{
								consent = new ActorConsent();
								supplier.Actor.ActorConsent.Add(consent);
							}

							if ((consent.HasConsent != supplierInput.HasConsent) || (consent.ConsentDate != supplierInput.ConsentDate))
							{
								consent.HasConsent = supplierInput.HasConsent;
								consent.ConsentDate = consent.HasConsent ? supplierInput.ConsentDate : null;
								consent.ConsentModified = DateTime.Now;
								consent.ConsentModifiedBy = GetUserDetails();
							}
						}

						#region Addresses

						result = ContactManager.SaveContactAddresses(entities, supplierInput.ContactAddresses, supplierId, TermGroup_SysContactType.Company);
						if (!result.Success)
						{
							result.ErrorNumber = (int)ActionResultSave.SupplierContactsAndTeleComNotSaved;
							result.ErrorMessage = GetText(11011, "Alla kontakt- och tele/webb-uppgifter kunde inte sparas");
							return result;
						}

						#endregion

						#region Categories

						if (supplierInput.CategoryIds != null)
						{
							// Angular
							result = CategoryManager.SaveCompanyCategoryRecords(entities, transaction, supplierInput.CategoryIds, actorCompanyId, SoeCategoryType.Supplier, SoeCategoryRecordEntity.Supplier, supplierId);
							if (!result.Success)
							{
								result.ErrorNumber = (int)ActionResultSave.SupplierCompanyCategoryNotSaved;
								result.ErrorMessage = GetText(11012, "Alla kategorier kunde inte sparas");
								return result;
							}
						}

						#endregion

						#region ContactPersons

						if (contactPersonIds != null)
						{
							result = ContactManager.SaveContactPersonMappings(entities, contactPersonIds, supplierId);
							if (!result.Success)
								return result;
						}

						#endregion

						#region PaymentInformation

						if (supplierInput.PaymentInformationDomestic != null)
						{
							result = PaymentManager.SavePaymentInformation(entities, transaction, supplierInput.PaymentInformationDomestic.Rows, supplier.ActorSupplierId, supplierInput.PaymentInformationDomestic.DefaultSysPaymentTypeId, actorCompanyId, false, true, SoeEntityType.Supplier);
							if (!result.Success)
								return result;
						}

						if (supplierInput.PaymentInformationForegin != null)
						{
							result = PaymentManager.SavePaymentInformation(entities, transaction, supplierInput.PaymentInformationForegin.Rows, supplier.ActorSupplierId, supplierInput.PaymentInformationForegin.DefaultSysPaymentTypeId, actorCompanyId, true, true, SoeEntityType.Supplier);
							if (!result.Success)
								return result;
						}

						#endregion PaymentInformation

						#region AccountingSettings

						if (supplierInput.AccountingSettings != null && supplierInput.AccountingSettings.Count > 0)
						{
							#region Prereq

							List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true);

							#endregion

							if (supplier.SupplierAccountStd == null || supplier.SupplierAccountStd.Count == 0)
							{
								#region Add AccountingSettings

								if (supplier.SupplierAccountStd == null)
									supplier.SupplierAccountStd = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<SupplierAccountStd>();

								// Only add standard account
								// Internal accounts will be handled below
								foreach (AccountingSettingsRowDTO settingInput in supplierInput.AccountingSettings)
								{
									// Standard account
									if (settingInput.Account1Id != 0)
									{
										AccountStd accStd = AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true);
										if (accStd != null)
										{
											SupplierAccountStd supplierAccountStd = new SupplierAccountStd
											{
												Type = settingInput.Type,
												AccountStd = accStd
											};

											// Internal accounts
											int dimCounter = 1;
											foreach (AccountDim dim in dims)
											{
												// Get internal account from input
												dimCounter++;
												int accountId = 0;
												if (dimCounter == 2)
													accountId = settingInput.Account2Id;
												else if (dimCounter == 3)
													accountId = settingInput.Account3Id;
												else if (dimCounter == 4)
													accountId = settingInput.Account4Id;
												else if (dimCounter == 5)
													accountId = settingInput.Account5Id;
												else if (dimCounter == 6)
													accountId = settingInput.Account6Id;

												// Add account internal
												AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
												if (accountInternal != null)
													supplierAccountStd.AccountInternal.Add(accountInternal);
											}

											supplier.SupplierAccountStd.Add(supplierAccountStd);
										}
									}
								}

								#endregion
							}
							else
							{
								#region Update/Delete AccountingSettings

								// Loop over existing settings
								foreach (SupplierAccountStd supplierAccountStd in supplier.SupplierAccountStd.ToList())
								{
									// Find setting in input
									AccountingSettingsRowDTO settingInput = supplierInput.AccountingSettings.FirstOrDefault(a => a.Type == supplierAccountStd.Type);
									if (settingInput != null)
									{
										// Standard account
										if (settingInput.Account1Id == 0)
										{
											// Delete account
											supplierAccountStd.AccountInternal.Clear();
											supplier.SupplierAccountStd.Remove(supplierAccountStd);
											entities.DeleteObject(supplierAccountStd);
										}
										else
										{
											// Update account
											if (supplierAccountStd.AccountStd.AccountId != settingInput.Account1Id)
											{
												AccountStd accountStd = AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true);
												if (accountStd != null)
													supplierAccountStd.AccountStd = accountStd;
											}

											// Remove existing internal accounts
											// No way to update them
											supplierAccountStd.AccountInternal.Clear();

											// Internal accounts
											int dimCounter = 1;
											foreach (AccountDim dim in dims)
											{
												// Get internal account from input
												dimCounter++;
												int accountId = 0;
												if (dimCounter == 2)
													accountId = settingInput.Account2Id;
												else if (dimCounter == 3)
													accountId = settingInput.Account3Id;
												else if (dimCounter == 4)
													accountId = settingInput.Account4Id;
												else if (dimCounter == 5)
													accountId = settingInput.Account5Id;
												else if (dimCounter == 6)
													accountId = settingInput.Account6Id;

												// Add account internal
												AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
												if (accountInternal != null)
													supplierAccountStd.AccountInternal.Add(accountInternal);
											}
										}
									}
									// Remove from input to prevent adding below
									supplierInput.AccountingSettings.Remove(settingInput);
								}

								#endregion

								#region Add AccountingSettings

								if (supplierInput.AccountingSettings.Count > 0)
								{
									foreach (AccountingSettingsRowDTO settingInput in supplierInput.AccountingSettings)
									{
										// Standard account
										if (settingInput.Account1Id != 0)
										{
											AccountStd accStd = AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true);
											if (accStd != null)
											{
												SupplierAccountStd supplierAccountStd = new SupplierAccountStd
												{
													Type = settingInput.Type,
													AccountStd = accStd
												};
												supplier.SupplierAccountStd.Add(supplierAccountStd);

												// Internal accounts
												int dimCounter = 1;
												foreach (AccountDim dim in dims)
												{
													// Get internal account from input
													dimCounter++;
													int accountId = 0;
													if (dimCounter == 2)
														accountId = settingInput.Account2Id;
													else if (dimCounter == 3)
														accountId = settingInput.Account3Id;
													else if (dimCounter == 4)
														accountId = settingInput.Account4Id;
													else if (dimCounter == 5)
														accountId = settingInput.Account5Id;
													else if (dimCounter == 6)
														accountId = settingInput.Account6Id;

													// Add account internal
													AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
													if (accountInternal != null)
														supplierAccountStd.AccountInternal.Add(accountInternal);
												}
											}
										}
									}
								}

								#endregion
							}
						}

						#endregion

						#region Files

						if (files.Any())
						{
							result = GeneralManager.UpdateFiles(entities, files, supplierId);
							if (!result.Success)
								return result;
						}

						#endregion

						#region ExtraFields

						if (extraFields != null && extraFields.Count > 0)
						{
							result = ExtraFieldManager.SaveExtraFieldRecords(entities, extraFields, (int)SoeEntityType.Supplier, supplierId, actorCompanyId);
							if (!result.Success)
							{
								result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
								return result;
							}
						}

						#endregion

						result = SaveChanges(entities, transaction);

						// Commit transaction
						if (result.Success)
						{
							transaction.Complete();
							SyncSupplier(actorCompanyId, supplier);
						}

						if (!result.Success)
						{
							result.ErrorNumber = (int)ActionResultSave.SupplierNotSaved;
							result.ErrorMessage = GetText(11009, "Leverantör kunde inte sparas");
						}
					}
				}
				catch (Exception ex)
				{
					base.LogError(ex, this.log);
					result.Exception = ex;
					result.IntegerValue = 0;
				}
				finally
				{
					if (result.Success)
					{
						//Set success properties
						result.IntegerValue = supplierId;
					}
					else
						base.LogTransactionFailed(this.ToString(), this.log);

					entities.Connection.Close();
				}

				return result;
			}
		}

		public void SyncSupplier(int actorCompanyId, Supplier supplier)
		{
            // Sync supplier async
            Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() =>
			{
				var azoraOneStatus = SettingManager.GetCompanyIntSetting(CompanySettingType.ScanningUsesAzoraOne);
				if (azoraOneStatus > (int)AzoraOneStatus.Deactivated)
					EdiManager.SyncSupplierWithAzoraOne(actorCompanyId, supplier);
			}));
		}

		public ActionResult DeleteSupplier(int supplierId, int actorCompanyId)
		{
			using (var entities = new CompEntities())
			{
				return DeleteSupplier(entities, supplierId, actorCompanyId, false);
			}
		}

		public ActionResult DeleteSupplier(CompEntities entities, int supplierId, int actorCompanyId, bool clearValues)
		{
			//Check relation dependencies
			if (SupplierHasInvoices(supplierId, actorCompanyId))
				return new ActionResult((int)ActionResultDelete.SupplierHasInvoices, GetText(2141, 1003, "Leverantören kan inte tas bort. Leverantören har fakturor registrerade på sig. Du kan istället inaktivera leverantören."));

			Supplier originalSupplier = GetSupplier(entities, supplierId, true, false, true, false);
			if (originalSupplier == null)
				return new ActionResult((int)ActionResultDelete.EntityNotFound, GetText(1275, "Leverantör hittades inte"));

			if (clearValues)
			{
				var deleteText = GetText(7413, "RADERAD UPPGIFT");

				originalSupplier.SupplierNr = " ";
				originalSupplier.Name = deleteText + " " + DateTime.Today.ToShortDateString();
				originalSupplier.OrgNr = " ";
				originalSupplier.VatNr = " ";
				originalSupplier.InvoiceReference = " ";
				originalSupplier.BIC = " ";
				originalSupplier.OurCustomerNr = " ";
				originalSupplier.RiksbanksCode = " ";
				originalSupplier.Note = " ";
				originalSupplier.OurReference = " ";

				if (!originalSupplier.Actor.Contact.IsLoaded)
					originalSupplier.Actor.Contact.Load();

				var contact = originalSupplier.Actor.Contact.FirstOrDefault();
				if (contact != null)
				{
					foreach (var ecom in contact.ContactECom)
					{
						ecom.Name = deleteText + " " + DateTime.Today.ToShortDateString();
						ecom.Text = " ";
						ecom.Description = " ";

						SetModifiedProperties(ecom);
					}

					foreach (var address in contact.ContactAddress)
					{
						foreach (var addressRow in address.ContactAddressRow)
						{
							addressRow.Text = deleteText + " " + DateTime.Today.ToShortDateString();

							SetModifiedProperties(addressRow);
						}
						address.Name = deleteText + " " + DateTime.Today.ToShortDateString();

						SetModifiedProperties(address);
					}
				}

				if (!originalSupplier.Actor.PaymentInformation.IsLoaded)
					originalSupplier.Actor.PaymentInformation.Load();

				foreach (var info in originalSupplier.Actor.PaymentInformation)
				{
					if (!info.PaymentInformationRow.IsLoaded)
						info.PaymentInformationRow.Load();

					foreach (var infoRow in info.PaymentInformationRow)
					{
						infoRow.PaymentNr = " ";
						infoRow.BIC = " ";
						infoRow.ClearingCode = " ";
						infoRow.PaymentCode = " ";
						infoRow.CurrencyAccount = " ";
						SetModifiedProperties(info);
					}

					SetModifiedProperties(info);
				}

				SetModifiedProperties(originalSupplier);
			}

			var result = SaveTrackChanges(entities, null, null, originalSupplier, TermGroup_TrackChangesAction.Delete, actorCompanyId);

			if (!result.Success)
				return result;

			result = ChangeEntityState(entities, originalSupplier, SoeEntityState.Deleted, true);

			if (!result.Success)
				return result;

			result = ActorManager.DeleteExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.Supplier, supplierId, actorCompanyId, true);

			return result;
		}

		/// <summary>
		/// Set Interim to false on all Suppliers for given Company
		/// </summary>
		/// <param name="entities">The ObjectContext</param>
		/// <param name="actorCompanyId">The Company that the suppliers belongs to</param>
		/// <returns></returns>
		public ActionResult DisAllowInterim(CompEntities entities, int actorCompanyId)
		{
			List<Supplier> suppliers = GetSuppliersByCompany(entities, actorCompanyId, onlyActive: false);
			foreach (Supplier supplier in suppliers)
			{
				supplier.Interim = false;
			}

			return SaveChanges(entities);
		}

		public ActionResult UpdateSuppliersState(Dictionary<int, bool> suppliers)
		{
			ActionResult result = new ActionResult();

			using (CompEntities entities = new CompEntities())
			{
				foreach (KeyValuePair<int, bool> supplier in suppliers)
				{
					Supplier originalSupplier = GetSupplier(entities, supplier.Key);
					if (originalSupplier == null)
						return new ActionResult((int)ActionResultSave.EntityNotFound, "Supplier");

					ChangeEntityState(originalSupplier, supplier.Value ? SoeEntityState.Active : SoeEntityState.Inactive);
				}

				result = SaveChanges(entities);
			}

			return result;
		}

		public ActionResult UpdateSuppliersIsPrivatePerson(Dictionary<int, bool> suppliers)
		{
			using (CompEntities entities = new CompEntities())
			{
				foreach (KeyValuePair<int, bool> supplier in suppliers)
				{
					Supplier originalSupplier = GetSupplier(entities, supplier.Key);
					if (originalSupplier == null)
						return new ActionResult((int)ActionResultSave.EntityNotFound, "Supplier");

					originalSupplier.IsPrivatePerson = supplier.Value;
				}

				return SaveChanges(entities);
			}
		}

		#region Help-methods      

		public Supplier GetSupplierForFinvoiceByPrio(CompEntities entities, int actorCompanyId, string orgNr, string orgUnit, string vatNr, string Iban)
		{
			Supplier supplier = null;


			var filteredItems = new List<GetSupplierIdentificationResult>();
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var allItems = entitiesReadOnly.GetSupplierIdentification(actorCompanyId).ToList();

			if (allItems.Count > 0)
			{
				#region OrgNr

				if (filteredItems.Count == 0)
				{
					//Search - no items found earlier
					filteredItems.AddRange(FilterSuppliersItemByOrgNr(allItems, orgNr, false));
				}
				else if (filteredItems.Count == 1)
				{
					//Do nothing - 1 item found
				}
				else if (filteredItems.Count > 1)
				{
					//Filter - items found earlier
					var items = FilterSuppliersItemByOrgNr(filteredItems, orgNr, false);
					if (items.Count > 0 && items.Count != filteredItems.Count)
					{
						filteredItems.Clear();
						filteredItems.AddRange(items);
					}
				}
				if (filteredItems.Count > 0)

				{
					var noOfSuppliers = filteredItems.DistinctBy(i => i.SupplierNr).Count();
					if (noOfSuppliers == 1)
					{
						supplier = GetSupplier(entities, filteredItems.First().ActorSupplierId);
					}
				}

				#endregion

				#region Iban

				if (filteredItems.Count == 0)
				{
					//Search - no items found earlier
					filteredItems.AddRange(FilterSuppliersItemByIban(allItems, Iban, false));
				}
				else if (filteredItems.Count == 1)
				{
					//Do nothing - 1 item found
				}
				else if (filteredItems.Count > 1)
				{
					//Filter - items found earlier
					var items = FilterSuppliersItemByIban(filteredItems, Iban, false);
					if (items.Count != filteredItems.Count)
					{
						filteredItems.Clear();
						filteredItems.AddRange(items);
					}
					if (filteredItems.Count == 0)
					{
						return supplier;
					}
				}

				#endregion

				#region VatNr

				if (filteredItems.Count == 0)
				{
					//Search - no items found earlier
					filteredItems.AddRange(FilterSuppliersItemByVatNr(allItems, vatNr, false));
				}
				else if (filteredItems.Count == 1)
				{
					//Do nothing - 1 item found
				}
				else if (filteredItems.Count > 1)
				{
					//Filter - items found earlier
					var items = FilterSuppliersItemByVatNr(filteredItems, vatNr, false);
					if (items.Count > 0 && items.Count != filteredItems.Count)
					{
						filteredItems.Clear();
						filteredItems.AddRange(items);
					}
				}

				#endregion

				#region orgUnit

				if (filteredItems.Count == 0)
				{
					//Search - no items found earlier
					filteredItems.AddRange(FilterSuppliersItemByOrgNr(allItems, orgUnit, false));
				}
				else if (filteredItems.Count == 1)
				{
					//Do nothing - 1 item found
				}
				else if (filteredItems.Count > 1)
				{
					//Filter - items found earlier
					var items = FilterSuppliersItemByOrgNr(filteredItems, orgUnit, false);
					if (items.Count > 0 && items.Count != filteredItems.Count)
					{
						filteredItems.Clear();
						filteredItems.AddRange(items);
					}
				}

				#endregion

				if (filteredItems.Count > 0)
					supplier = GetSupplier(entities, filteredItems.First().ActorSupplierId);
			}

			return supplier;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByOrgNr(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers)
		{
			//Try first without removing space and hyphen
			var filteredItems = FilterSuppliersItemByOrgNr(allItems, value, onlySearchEDISuppliers, false);
			if (filteredItems.Count == 0)
				filteredItems = FilterSuppliersItemByOrgNr(allItems, value, onlySearchEDISuppliers, true);
			return filteredItems;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByOrgNr(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers, bool removeWhiteSpaceAndHyphen)
		{
			var filteredItems = new List<GetSupplierIdentificationResult>();
			if (!String.IsNullOrEmpty(value))
			{
				if (removeWhiteSpaceAndHyphen)
				{
					value = value.RemoveWhiteSpaceAndHyphen();

					filteredItems.AddRange((from s in allItems
											where s.OrgNr.RemoveWhiteSpaceAndHyphen() == value &&
											(!onlySearchEDISuppliers || s.IsEDISupplier)
											select s).ToList());
				}
				else
				{
					filteredItems.AddRange((from s in allItems
											where s.OrgNr == value &&
											(!onlySearchEDISuppliers || s.IsEDISupplier)
											select s).ToList());
				}
			}
			return filteredItems;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByVatNr(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers)
		{
			//Try first without removing space and hyphen
			var filteredItems = FilterSuppliersItemByVatNr(allItems, value, onlySearchEDISuppliers, false);
			if (filteredItems.Count == 0)
				filteredItems = FilterSuppliersItemByVatNr(allItems, value, onlySearchEDISuppliers, true);
			return filteredItems;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByVatNr(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers, bool removeWhiteSpaceAndHyphen)
		{
			var filteredItems = new List<GetSupplierIdentificationResult>();
			if (!String.IsNullOrEmpty(value))
			{
				if (removeWhiteSpaceAndHyphen)
					value = value.RemoveWhiteSpaceAndHyphen();

				filteredItems.AddRange((from s in allItems
										where s.VatNr == value &&
										(!onlySearchEDISuppliers || s.IsEDISupplier)
										select s).ToList());
			}
			return filteredItems;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByBankGiro(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers)
		{
			//Try first without removing space and hyphen
			var filteredItems = FilterSuppliersItemByBankGiro(allItems, value, onlySearchEDISuppliers, false);
			if (filteredItems.Count == 0)
				filteredItems = FilterSuppliersItemByBankGiro(allItems, value, onlySearchEDISuppliers, true);
			return filteredItems;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByBankGiro(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers, bool removeWhiteSpaceAndHyphen)
		{
			var filteredItems = new List<GetSupplierIdentificationResult>();
			if (!String.IsNullOrEmpty(value))
			{
				if (removeWhiteSpaceAndHyphen)
					value = value.RemoveWhiteSpaceAndHyphen();

				filteredItems.AddRange((from s in allItems
										where s.HasPaymentInformation == true &&
										s.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BG &&
										(s.PaymentNr == value || s.PaymentNr.ToNumeric() == value) &&
										(!onlySearchEDISuppliers || s.IsEDISupplier)
										select s).ToList());
			}
			return filteredItems;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByPostalGiro(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers)
		{
			//Try first without removing space and hyphen
			var filteredItems = FilterSuppliersItemByPostalGiro(allItems, value, onlySearchEDISuppliers, false);
			if (filteredItems.Count == 0)
				filteredItems = FilterSuppliersItemByPostalGiro(allItems, value, onlySearchEDISuppliers, true);
			return filteredItems;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByPostalGiro(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers, bool removeWhiteSpaceAndHyphen)
		{
			var filteredItems = new List<GetSupplierIdentificationResult>();
			if (!String.IsNullOrEmpty(value))
			{
				if (removeWhiteSpaceAndHyphen)
					value = value.RemoveWhiteSpaceAndHyphen();

				filteredItems.AddRange((from s in allItems
										where s.HasPaymentInformation == true &&
										s.SysPaymentTypeId == (int)TermGroup_SysPaymentType.PG &&
										(s.PaymentNr == value || s.PaymentNr.ToNumeric() == value) &&
										(!onlySearchEDISuppliers || s.IsEDISupplier)
										select s).ToList());
			}
			return filteredItems;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByIban(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers)
		{
			//Try first without removing space and hyphen
			var filteredItems = FilterSuppliersItemByIban(allItems, value, onlySearchEDISuppliers, false);
			if (filteredItems.Count == 0)
				filteredItems = FilterSuppliersItemByIban(allItems, value, onlySearchEDISuppliers, true);
			return filteredItems;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByIban(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers, bool removeWhiteSpaceAndHyphen)
		{
			var filteredItems = new List<GetSupplierIdentificationResult>();
			if (!String.IsNullOrEmpty(value))
			{
				if (removeWhiteSpaceAndHyphen)
					value = value.RemoveWhiteSpaceAndHyphen();

				filteredItems.AddRange((from s in allItems
										where !String.IsNullOrEmpty(s.BIC) && (s.BIC == value || s.BIC.ToNumeric() == value) &&
										(!onlySearchEDISuppliers || s.IsEDISupplier)
										select s).ToList());

				if (filteredItems.Count == 0)
				{
					filteredItems.AddRange((from s in allItems
											where s.HasPaymentInformation == true &&
											s.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC &&
											(s.PaymentNr == value || s.PaymentNr.ToNumeric() == value) &&
											(!onlySearchEDISuppliers || s.IsEDISupplier)
											select s).ToList());
				}
			}
			return filteredItems;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByName(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers)
		{
			//Try first without removing space and hyphen
			var filteredItems = FilterSuppliersItemByName(allItems, value, onlySearchEDISuppliers, false);
			if (filteredItems.Count == 0)
				filteredItems = FilterSuppliersItemByName(allItems, value, onlySearchEDISuppliers, true);
			return filteredItems;
		}

		private List<GetSupplierIdentificationResult> FilterSuppliersItemByName(List<GetSupplierIdentificationResult> allItems, string value, bool onlySearchEDISuppliers, bool removeWhiteSpaceAndHyphen)
		{
			var filteredItems = new List<GetSupplierIdentificationResult>();
			if (!string.IsNullOrEmpty(value))
			{
				if (removeWhiteSpaceAndHyphen)
					value = value.RemoveWhiteSpaceAndHyphen();

				filteredItems.AddRange((from s in allItems
										where s.Name.ToLower() == value.ToLower() &&
										(!onlySearchEDISuppliers || s.IsEDISupplier)
										select s).ToList());

				if (filteredItems.Count == 0 && value.Length >= 4)
				{
					//Search by substring of name
					string shortName = value.Substring(0, 4).ToLower();
					filteredItems.AddRange((from s in allItems
											where s.Name.ToLower().StartsWith(shortName) &&
											(!onlySearchEDISuppliers || s.IsEDISupplier)
											select s).ToList());
				}
			}

			return filteredItems;
		}

		#endregion

		#endregion

		#region SupplierAccount

		public Dictionary<int, string> GetAllSupplierAccounts(int actorCompanyId, SupplierAccountType customerAccountType)
		{
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from sas in entitiesReadOnly.SupplierAccountStd
					where sas.AccountStd.Account.ActorCompanyId == actorCompanyId &&
					(sas.Type == (int)customerAccountType)
					select new
					{
						AccountNr = sas.AccountStd.Account.AccountNr ?? "",
						ActorSupplierId = sas.Supplier.ActorSupplierId
					}).ToDictionary(a => a.ActorSupplierId, a => a.AccountNr);
		}

		public List<SupplierAccountStd> GetSupplierAccounts(CompEntities entities, int actorSupplierId)
		{
			return (from sas in entities.SupplierAccountStd
						.Include("AccountStd.Account")
						.Include("AccountInternal.Account.AccountDim")
					where sas.Supplier.ActorSupplierId == actorSupplierId
					select sas).ToList();
		}

		public SupplierAccountStd GetSupplierAccount(CompEntities entities, int actorSupplierId, SupplierAccountType supplierAccountType)
		{
			int type = (int)supplierAccountType;
			return (from sas in entities.SupplierAccountStd
						.Include("AccountStd.Account")
						.Include("AccountInternal.Account.AccountDim")
					where ((sas.Supplier.ActorSupplierId == actorSupplierId) &&
					(sas.Type == type))
					select sas).FirstOrDefault();
		}

		public Dictionary<string, int> GetSupplierAccountsDict(CompEntities entities, int supplierId)
		{
			Dictionary<string, int> accountIds = new Dictionary<string, int>();

			List<SupplierAccountStd> supplierAccounts = GetSupplierAccounts(entities, supplierId);

			SupplierAccountStd creditAccount = supplierAccounts.FirstOrDefault(i => i.Type == (int)SupplierAccountType.Credit);
			if (creditAccount != null)
				accountIds.Add("credit", creditAccount.AccountStd.AccountId);

			SupplierAccountStd debitAccount = supplierAccounts.FirstOrDefault(i => i.Type == (int)SupplierAccountType.Debit);
			if (debitAccount != null)
				accountIds.Add("debit", debitAccount.AccountStd.AccountId);

			SupplierAccountStd vatAccount = supplierAccounts.FirstOrDefault(i => i.Type == (int)SupplierAccountType.VAT);
			if (vatAccount != null)
				accountIds.Add("vat", vatAccount.AccountStd.AccountId);

			SupplierAccountStd interimAccount = supplierAccounts.FirstOrDefault(i => i.Type == (int)SupplierAccountType.Interim);
			if (interimAccount != null)
				accountIds.Add("interim", interimAccount.AccountStd.AccountId);

			return accountIds;
		}

		public Dictionary<string, SupplierAccountStd> GetSupplierAccountStdDict(CompEntities entities, int supplierId)
		{
			var accountIds = new Dictionary<string, SupplierAccountStd>();

			List<SupplierAccountStd> supplierAccounts = GetSupplierAccounts(entities, supplierId);

			SupplierAccountStd creditAccount = supplierAccounts.FirstOrDefault(i => i.Type == (int)SupplierAccountType.Credit);
			if (creditAccount != null)
				accountIds.Add("credit", creditAccount);

			SupplierAccountStd debitAccount = supplierAccounts.FirstOrDefault(i => i.Type == (int)SupplierAccountType.Debit);
			if (debitAccount != null)
				accountIds.Add("debit", debitAccount);

			SupplierAccountStd vatAccount = supplierAccounts.FirstOrDefault(i => i.Type == (int)SupplierAccountType.VAT);
			if (vatAccount != null)
				accountIds.Add("vat", vatAccount);

			SupplierAccountStd interimAccount = supplierAccounts.FirstOrDefault(i => i.Type == (int)SupplierAccountType.Interim);
			if (interimAccount != null)
				accountIds.Add("interim", interimAccount);

			return accountIds;
		}

		#endregion

		#region Help-methods

		private SupplierAccountStd CreateSupplierAccount(CompEntities entities, List<AccountingSettingDTO> accountSettingItems, int index, int actorCompanyId)
		{
			SupplierAccountStd supplierAccountStd = null;

			// Index
			// 1 = Credit = Intäkt
			// 2 = Debit = Fordran
			// 3 = VAT = Moms
			// 4 = Interim = Interim

			AccountingSettingDTO accountStdItem = accountSettingItems.FirstOrDefault(a => a.DimNr == Constants.ACCOUNTDIM_STANDARD);
			if (accountStdItem != null)
			{
				#region Prereq

				int accountId = 0;
				int accountStdType = 0;

				if (index == 1)
				{
					accountId = accountStdItem.Account1Id;
					accountStdType = (int)SupplierAccountType.Credit;
				}
				else if (index == 2)
				{
					accountId = accountStdItem.Account2Id;
					accountStdType = (int)SupplierAccountType.Debit;
				}
				else if (index == 3)
				{
					accountId = accountStdItem.Account3Id;
					accountStdType = (int)SupplierAccountType.VAT;
				}
				else if (index == 4)
				{
					accountId = accountStdItem.Account4Id;
					accountStdType = (int)SupplierAccountType.Interim;
				}

				#endregion

				#region Add SupplierAccountStd (AccountStd mandatory)

				// Standard account
				AccountStd accountStd = AccountManager.GetAccountStd(entities, accountId, actorCompanyId, false, false);
				if (accountStd != null)
				{
					supplierAccountStd = new SupplierAccountStd
					{
						Type = accountStdType,

						//Set references
						AccountStd = accountStd
					};

					// Add internal accounts
					AddAccountInternalToSupplierAccountStd(entities, supplierAccountStd, accountSettingItems, index, actorCompanyId);
				}

				#endregion
			}

			return supplierAccountStd;
		}

		private void AddAccountInternalToSupplierAccountStd(CompEntities entities, SupplierAccountStd supplierAccountStd, List<AccountingSettingDTO> accountSettingItems, int index, int actorCompanyId)
		{
			if (supplierAccountStd == null || accountSettingItems == null)
				return;

			foreach (AccountingSettingDTO accountInternalItem in accountSettingItems.Where(a => a.DimNr != Constants.ACCOUNTDIM_STANDARD))
			{
				#region Prereq

				int intAccountId = 0;
				if (index == 1)
					intAccountId = accountInternalItem.Account1Id;
				else if (index == 2)
					intAccountId = accountInternalItem.Account2Id;
				else if (index == 3)
					intAccountId = accountInternalItem.Account3Id;
				else if (index == 4)
					intAccountId = accountInternalItem.Account4Id;

				#endregion

				#region Add AccountInternal

				AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, intAccountId, actorCompanyId);
				if (accountInternal != null)
					supplierAccountStd.AccountInternal.Add(accountInternal);

				#endregion
			}
		}

		#endregion
	}
}
