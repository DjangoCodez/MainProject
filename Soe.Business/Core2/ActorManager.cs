using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
	//Test1
	public class ActorManager : ManagerBase
	{
		#region Variables

		// Create a logger for use in this class
		private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Ctor

		public ActorManager(ParameterObject parameterObject) : base(parameterObject) { }

		#endregion

		#region Actor

		#region CompanyExternalCode

		public List<CompanyExternalCode> GetCompanyExternalCodes(int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.CompanyExternalCode.NoTracking();
			return GetCompanyExternalCodes(entities, actorCompanyId);
		}

		public List<CompanyExternalCode> GetCompanyExternalCodes(CompEntities entities, int actorCompanyId)
		{
			return (from cec in entities.CompanyExternalCode
					where cec.ActorCompanyId == actorCompanyId && cec.State == (int)SoeEntityState.Active
					select cec).ToList();
		}

		public List<CompanyExternalCodeGridDTO> GetCompanyExternalCodesForGrid(int actorCompanyId)
		{
			List<GenericType> types = base.GetTermGroupContent(TermGroup.CompanyExternalCodeEntity);
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.CompanyExternalCode.NoTracking();
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			List<CompanyExternalCodeGridDTO> codes = GetCompanyExternalCodes(entitiesReadOnly, actorCompanyId).ToGridDTOs();
			Dictionary<int, string> payrollProducts = new Dictionary<int, string>();

			if (codes.Any(c => c.Entity == TermGroup_CompanyExternalCodeEntity.PayrollProduct))
			{
				payrollProducts = ProductManager.GetPayrollProductsDict(actorCompanyId, false, true);
			}

			foreach (var code in codes)
			{
				code.EntityName = types.FirstOrDefault(f => f.Id == (int)code.Entity)?.Name ?? string.Empty;

				switch (code.Entity)
				{
					case TermGroup_CompanyExternalCodeEntity.PayrollProduct:
						if (payrollProducts.TryGetValue(code.RecordId, out string value))
						{
							code.RecordName = value;
						}
						break;
					default:
						break;
				}
			}
			return codes.OrderBy(c => c.EntityName).ThenBy(c => c.ExternalCode).ToList();
		}

		public List<CompanyExternalCode> GetCompanyExternalCodes(TermGroup_CompanyExternalCodeEntity entity, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return GetCompanyExternalCodes(entities, entity, actorCompanyId);
		}

		public List<CompanyExternalCode> GetCompanyExternalCodes(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, int actorCompanyId)
		{
			return (from cec in entities.CompanyExternalCode
					where cec.Entity == (int)entity &&
					cec.ActorCompanyId == actorCompanyId &&
					cec.State == (int)SoeEntityState.Active
					select cec).ToList();
		}

		public List<CompanyExternalCode> GetCompanyExternalCodes(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, int recordId, int actorCompanyId)
		{
			return (from cec in entities.CompanyExternalCode
					where cec.Entity == (int)entity &&
					cec.ActorCompanyId == actorCompanyId &&
					cec.State == (int)SoeEntityState.Active &&
					cec.RecordId == recordId
					select cec).ToList();
		}

		public List<CompanyExternalCode> GetCompanyExternalCodes(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, List<int> recordIds, int actorCompanyId)
		{
			return (from cec in entities.CompanyExternalCode
					where cec.Entity == (int)entity &&
					cec.ActorCompanyId == actorCompanyId &&
					cec.State == (int)SoeEntityState.Active &&
					recordIds.Contains(cec.RecordId)
					select cec).ToList();
		}

		public List<CompanyExternalCode> TryPreloadCompanyExternalCodes(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, List<int> recordIds, int actorCompanyId)
		{
			if (recordIds.Count > Constants.LINQMAXCOUNTCONTAINS)
				return null;
			return GetCompanyExternalCodes(entities, entity, recordIds, actorCompanyId);
		}

		public CompanyExternalCode GetCompanyExternalCode(int companyExternalCodeId, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.CompanyExternalCode.NoTracking();
			return GetCompanyExternalCode(entities, companyExternalCodeId, actorCompanyId);
		}

		public CompanyExternalCode GetCompanyExternalCode(CompEntities entities, int companyExternalCodeId, int actorCompanyId)
		{
			if (companyExternalCodeId == 0)
				return null;

			return (from cec in entities.CompanyExternalCode
					where cec.ActorCompanyId == actorCompanyId &&
					cec.State == (int)SoeEntityState.Active &&
					cec.CompanyExternalCodeId == companyExternalCodeId
					select cec).FirstOrDefault();
		}

		public CompanyExternalCode GetCompanyExternalCode(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, int recordId, int actorCompanyId)
		{
			return (from cec in entities.CompanyExternalCode
					where cec.Entity == (int)entity &&
					cec.ActorCompanyId == actorCompanyId &&
					cec.State == (int)SoeEntityState.Active &&
					cec.RecordId == recordId
					select cec).FirstOrDefault();
		}

		public List<int> GetInternalIdsListFromExternalNr(TermGroup_CompanyExternalCodeEntity entity, string externalNr, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Actor.NoTracking();
			return GetInternalIdsListFromExternalNr(entities, entity, externalNr, actorCompanyId);
		}

		public List<int> GetInternalIdsListFromExternalNr(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, string externalNr, int actorCompanyId)
		{
			return (from cec in entities.CompanyExternalCode
					where cec.ExternalCode == externalNr && cec.Entity == (int)entity && cec.ActorCompanyId == actorCompanyId && cec.State == (int)SoeEntityState.Active
					select cec.RecordId).ToList();
		}

		public List<string> GetCompanyExternalCodeValues(TermGroup_CompanyExternalCodeEntity entity, int recordId, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Actor.NoTracking();
			return GetCompanyExternalCodeValues(entities, entity, recordId, actorCompanyId);
		}

		public List<string> GetCompanyExternalCodeValues(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, int recordId, int actorCompanyId)
		{
			return (from cec in entities.CompanyExternalCode
					where cec.RecordId == recordId &&
					cec.Entity == (int)entity &&
					cec.ActorCompanyId == actorCompanyId &&
					cec.State == (int)SoeEntityState.Active
					select cec.ExternalCode).ToList();
		}

		public ActionResult SaveCompanyExternalCode(CompanyExternalCodeDTO companyExternalCodeInput, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return SaveCompanyExternalCode(entities, companyExternalCodeInput, actorCompanyId);
		}

		public ActionResult SaveCompanyExternalCode(CompEntities entities, CompanyExternalCodeDTO companyExternalCodeInput, int actorCompanyId)
		{
			if (companyExternalCodeInput == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "CompanyExternalCode");

			// Get original
			CompanyExternalCode companyExternalCode = GetCompanyExternalCode(entities, companyExternalCodeInput.CompanyExternalCodeId, actorCompanyId);
			if (companyExternalCode == null)
			{
				companyExternalCode = new CompanyExternalCode()
				{
					ActorCompanyId = actorCompanyId
				};
				SetCreatedProperties(companyExternalCode);
				entities.CompanyExternalCode.AddObject(companyExternalCode);
			}
			else
			{
				SetModifiedProperties(companyExternalCode);
			}

			companyExternalCode.ExternalCode = companyExternalCodeInput.ExternalCode;
			companyExternalCode.RecordId = companyExternalCodeInput.RecordId;
			companyExternalCode.Entity = (int)companyExternalCodeInput.Entity;

			ActionResult result = SaveChanges(entities);
			if (!result.Success)
				return result;

			result.IntegerValue = companyExternalCode.CompanyExternalCodeId;
			return result;
		}


		public ActionResult DeleteCompanyExternalCode(int companyExternalCodeId, int actorCompanyId)
		{
			using (var entities = new CompEntities())
			{
				return DeleteCompanyExternalCode(entities, companyExternalCodeId, actorCompanyId, true);
			}
		}

		public ActionResult DeleteCompanyExternalCode(CompEntities entities, int companyExternalCodeId, int actorCompanyId, bool saveChanges)
		{
			CompanyExternalCode companyExternalCode = GetCompanyExternalCode(entities, companyExternalCodeId, actorCompanyId);
			if (companyExternalCode == null)
				return new ActionResult((int)ActionResultDelete.EntityNotFound, "CompanyExternalCode");

			SetModifiedProperties(companyExternalCode);
			return ChangeEntityState(entities, companyExternalCode, SoeEntityState.Deleted, saveChanges);
		}

		public ActionResult DeleteCompanyExternalCode(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, int recordId, int actorCompanyId, bool saveChanges)
		{
			var companyExternalCode = GetCompanyExternalCode(entities, entity, recordId, actorCompanyId);
			SetModifiedProperties(companyExternalCode);
			return ChangeEntityState(entities, companyExternalCode, SoeEntityState.Deleted, saveChanges);
		}

		public ActionResult UpsertExternalNbrs(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, int recordId, string externalNbrs, int actorCompanyId, bool save = true)
		{
			DeleteExternalNbrs(entities, entity, recordId, actorCompanyId, save);
			return InsertExternalNbrs(entities, entity, recordId, externalNbrs, actorCompanyId, save);
		}

		public ActionResult DeleteExternalNbrs(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, int recordId, int actorCompanyId, bool save = true)
		{
			var result = new ActionResult();
			var externalCodes = GetCompanyExternalCodes(entities, entity, recordId, actorCompanyId);
			foreach (var externalCode in externalCodes)
			{
				externalCode.State = (int)SoeEntityState.Deleted;
				SetModifiedProperties(externalCode);
			}

			if (save)
				entities.SaveChanges();
			return result;
		}

		public ActionResult InsertExternalNbrs(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, int recordId, string externalNbrs, int actorCompanyId, bool save = true)
		{
			if (string.IsNullOrEmpty(externalNbrs))
				return new ActionResult(false);

			var externalNbrsList = externalNbrs.Split(Constants.Delimiter).ToList();

			return InsertExternalNbrs(entities, entity, recordId, externalNbrsList, actorCompanyId, save);
		}

		public ActionResult InsertExternalNbrs(CompEntities entities, TermGroup_CompanyExternalCodeEntity entity, int recordId, List<string> externalNbrs, int actorCompanyId, bool save = true)
		{
			var result = new ActionResult();
			foreach (var nbr in externalNbrs.Distinct())
			{
				var externalCode = new CompanyExternalCode
				{
					ActorCompanyId = actorCompanyId,
					Entity = (int)entity,
					ExternalCode = nbr,
					RecordId = recordId
				};
				SetCreatedProperties(externalCode);
				entities.CompanyExternalCode.AddObject(externalCode);
			}

			if (save)
				entities.SaveChanges();
			return result;
		}

		#endregion

		public IEnumerable<Actor> GetActorsFromContactPerson(int actorCustomerId)
		{
			List<Actor> actors = new List<Actor>();

			ContactPerson contactPerson = ContactManager.GetContactPerson(actorCustomerId, true);
			foreach (Actor actor in contactPerson.Actors)
			{
				switch (actor.ActorType)
				{
					case (int)SoeActorType.Company:
						if (!actor.CompanyReference.IsLoaded)
							actor.CompanyReference.Load();
						actor.Name = actor.Company.Name;
						actor.TypeName = GetText(1709, "Företag");
						break;
					case (int)SoeActorType.Customer:
						if (!actor.CustomerReference.IsLoaded)
							actor.CustomerReference.Load();
						actor.Name = actor.Customer.Name;
						actor.TypeName = GetText(1710, "Kund");
						break;
					case (int)SoeActorType.Supplier:
						if (!actor.SupplierReference.IsLoaded)
							actor.SupplierReference.Load();
						actor.Name = actor.Supplier.Name;
						actor.TypeName = GetText(1711, "Leverantör");
						break;
					case (int)SoeActorType.ContactPerson:
						//ContactPersons cant belong to a ContactPerson
						continue;
					default:
						actor.Name = "";
						actor.TypeName = GetText(1712, "Okänd");
						break;
				}

				actors.Add(actor);
			}

			return actors;
		}

		public Actor GetActor(int actorId, bool loadActorType)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Actor.NoTracking();
			return GetActor(entities, actorId, loadActorType);
		}

		public Actor GetActor(CompEntities entities, int actorId, bool loadActorType)
		{
			Actor actor = (from a in entities.Actor
						   where a.ActorId == actorId
						   select a).FirstOrDefault();

			if (actor != null && loadActorType)
			{
				switch (actor.ActorType)
				{
					case (int)SoeActorType.Company:
						if (!actor.CompanyReference.IsLoaded)
							actor.CompanyReference.Load();
						break;
					case (int)SoeActorType.Customer:
						if (!actor.CustomerReference.IsLoaded)
							actor.CustomerReference.Load();
						break;
					case (int)SoeActorType.Supplier:
						if (!actor.SupplierReference.IsLoaded)
							actor.SupplierReference.Load();
						break;
					case (int)SoeActorType.ContactPerson:
						if (!actor.ContactPersonReference.IsLoaded)
							actor.ContactPersonReference.Load();
						break;
				}
			}

			return actor;
		}

		public string GetActorTypeName(Actor actor)
		{
			if (actor == null)
				return String.Empty;

			switch (actor.ActorType)
			{
				case (int)SoeActorType.Company:
					if (!actor.CompanyReference.IsLoaded)
						actor.CompanyReference.Load();
					return actor.Company != null ? actor.Company.Name : String.Empty;
				case (int)SoeActorType.Customer:
					if (!actor.CustomerReference.IsLoaded)
						actor.CustomerReference.Load();
					return actor.Customer != null ? actor.Customer.Name : String.Empty;
				case (int)SoeActorType.Supplier:
					if (!actor.SupplierReference.IsLoaded)
						actor.SupplierReference.Load();
					return actor.Supplier != null ? actor.Supplier.Name : String.Empty;
				case (int)SoeActorType.ContactPerson:
					if (!actor.ContactPersonReference.IsLoaded)
						actor.ContactPersonReference.Load();
					return actor.ContactPerson != null ? actor.ContactPerson.Name : String.Empty;
				default:
					return String.Empty;
			}
		}

		public int GetActorCurrencyId(CompEntities entities, int actorId)
		{
			var record = (from a in entities.Actor
						  where a.ActorId == actorId
						  select new
						  {
							  ActorType = a.ActorType,
							  SupplierCurrency = (int?)a.Supplier.CurrencyId,
							  CustomerCurrency = (int?)a.Customer.CurrencyId
						  }
					).FirstOrDefault();

			switch (record.ActorType)
			{
				case (int)SoeActorType.Customer:
					return record.CustomerCurrency.GetValueOrDefault();
				case (int)SoeActorType.Supplier:
					return record.SupplierCurrency.GetValueOrDefault();
				default:
					return 0;
			}
		}

		public List<ActorSearchPersonDTO> SearchPerson(string searchString, string searchEntities)
		{
			List<ActorSearchPersonDTO> searchPersons = new List<ActorSearchPersonDTO>();

			List<GenericType> yesNo = GetTermGroupContent(TermGroup.YesNo, skipUnknown: true);
			string yesText = yesNo.FirstOrDefault(x => x.Id == (int)TermGroup_YesNo.Yes)?.Name ?? string.Empty;
			string noText = yesNo.FirstOrDefault(x => x.Id == (int)TermGroup_YesNo.No)?.Name ?? string.Empty;
			string throughEmpoyment = GetText(7414, "Genom anställningsavtal");

			List<int> entityIds = new List<int>();
			string[] entitys = searchEntities.Split(',');

			using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
			bool hidePersons = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, base.ActorCompanyId);
			if (hidePersons && (entitys.Contains(((int)SoeEntityType.User).ToString()) || entitys.Contains(((int)SoeEntityType.Employee).ToString())))
			{
				List<Employee> employees = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, ActorCompanyId, UserId, RoleId);
				entityIds = employees.Select(s => s.EmployeeId).ToList();

				entitys = entitys.Where(w => w != ((int)SoeEntityType.User).ToString()).ToArray();
				searchEntities = StringUtility.GetCommaSeparatedString(entitys.ToList());
			}

			using (CompEntities entities = new CompEntities())
			{
				var data = entities.SearchPerson(this.ActorCompanyId, $"%{searchString}%", searchEntities);

				foreach (var item in data)
				{
					if (!hidePersons || entityIds.Any(a => a == item.RecordId))
					{
						var person = new ActorSearchPersonDTO
						{
							Name = item.Name,
							Number = item.Number,
							OrgNr = item.OrgNr,
							EntityType = (SoeEntityType)item.EntityType,
							EntityTypeName = GetText(item.EntityType, (int)TermGroup.SoeEntityType, ""),
							RecordId = item.RecordId,
							IsPrivatePerson = true,
							ConsentDate = item.ConsentDate,
							HasConsent = item.HasConsent ?? false,
						};

						if (person.EntityType == SoeEntityType.Employee)
							person.HasConsentString = throughEmpoyment;
						else
							person.HasConsentString = person.HasConsent ? yesText : noText;

						searchPersons.Add(person);
					}
				}
			}

			return searchPersons;
		}

		public List<ActorConsentGridDTO> GetActorsWithoutConsent()
		{
			int actorCompanyId = base.ActorCompanyId;
			List<ActorConsentGridDTO> items = new List<ActorConsentGridDTO>();
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			using (CompEntities entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();

					var customers = (from c in entitiesReadOnly.Customer.Include("Actor.ActorConsent").Include("Actor.Contact.ContactECom")
									 where c.ActorCompanyId == actorCompanyId &&
									   c.IsPrivatePerson == true &&
									   c.State == (int)SoeEntityState.Active
									 select c);

					foreach (Customer customer in customers)
					{
						if (customer.Actor.ActorConsent.Any(a => a.ConsentType == (int)ActorConsentType.Unspecified && a.HasConsent))
							continue;

						bool hasInvoices = (from i in entitiesReadOnly.Invoice
											where i.ActorId == customer.ActorCustomerId
											select i).Any();

						var actorConsentItem = new ActorConsentGridDTO()
						{
							ActorId = customer.ActorCustomerId,
							ActorName = customer.CustomerNr + " " + customer.Name,
							ActorType = SoeActorType.Customer,
							HasConnectedInvoices = hasInvoices,
						};

						var contact = customer.Actor.Contact.FirstOrDefault();
						if (contact != null)
						{
							if (!contact.ContactECom.IsLoaded)
								contact.ContactECom.Load();

							var emailItem = contact.ContactECom.FirstOrDefault(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email);
							if (emailItem != null)
								actorConsentItem.Email = emailItem.Text;
						}

						items.Add(actorConsentItem);
					}

					var suppliers = (from s in entitiesReadOnly.Supplier.Include("Actor.ActorConsent").Include("Actor.Contact.ContactECom")
									 where s.ActorCompanyId == actorCompanyId &&
									 s.IsPrivatePerson == true &&
									 s.State == (int)SoeEntityState.Active
									 select s);

					foreach (Supplier supplier in suppliers)
					{
						if (supplier.Actor.ActorConsent.Any(a => a.ConsentType == (int)ActorConsentType.Unspecified && a.HasConsent))
							continue;

						bool hasInvoices = (from i in entitiesReadOnly.Invoice
											where i.ActorId == supplier.ActorSupplierId
											select i).Any();

						var actorConsentItem = new ActorConsentGridDTO()
						{
							ActorId = supplier.ActorSupplierId,
							ActorName = supplier.SupplierNr + " " + supplier.Name,
							ActorType = SoeActorType.Supplier,
							HasConnectedInvoices = hasInvoices,
						};

						var contact = supplier.Actor.Contact.FirstOrDefault();
						if (contact != null)
						{
							if (!contact.ContactECom.IsLoaded)
								contact.ContactECom.Load();

							var emailItem = contact.ContactECom.FirstOrDefault(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email);
							if (emailItem != null)
								actorConsentItem.Email = emailItem.Text;
						}

						items.Add(actorConsentItem);
					}

					var actor = (from s in entitiesReadOnly.Actor.Include("ContactPersons.Actor.ActorConsent").Include("ContactPersons.Actor.Contact.ContactECom")
								 where s.ActorId == actorCompanyId
								 select s).FirstOrDefault();

					foreach (ContactPerson contactPerson in actor.ContactPersons)
					{
						if (contactPerson.Actor.ActorConsent.Any(a => a.ConsentType == (int)ActorConsentType.Unspecified && a.HasConsent))
							continue;

						var actorConsentItem = new ActorConsentGridDTO()
						{
							ActorId = contactPerson.Actor.ActorId,
							ActorName = contactPerson.Name,
							ActorType = SoeActorType.ContactPerson,
							Email = contactPerson.Email,
						};

						var contact = contactPerson.Actor.Contact.FirstOrDefault();
						if (contact != null)
						{
							if (!contact.ContactECom.IsLoaded)
								contact.ContactECom.Load();

							var emailItem = contact.ContactECom.FirstOrDefault(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email);
							if (emailItem != null)
								actorConsentItem.Email = emailItem.Text;
						}

						items.Add(actorConsentItem);
					}
				}
				catch (Exception ex)
				{
					base.LogError(ex, this.log);
				}
				finally
				{
					entities.Connection.Close();
				}
			}

			return items;
		}

		public ActionResult GiveConsent(DateTime date, List<int> customersDict, List<int> suppliersDict, List<int> contactPersonsDict)
		{
			int actorCompanyId = base.ActorCompanyId;

			using (CompEntities entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();

					var customers = (from c in entities.Customer.Include("Actor.ActorConsent")
									 where c.ActorCompanyId == actorCompanyId &&
									 customersDict.Contains(c.ActorCustomerId)
									 select c);

					foreach (Customer customer in customers)
					{
						ActorConsent consent = customer.Actor.ActorConsent.FirstOrDefault(a => a.ConsentType == (int)ActorConsentType.Unspecified);
						if (consent == null)
						{
							consent = new ActorConsent();
							consent.ActorId = customer.Actor.ActorId;
							consent.HasConsent = true;
							consent.ConsentDate = date;
							consent.ConsentModified = DateTime.Now;
							consent.ConsentModifiedBy = GetUserDetails();

							entities.ActorConsent.AddObject(consent);
						}
						else
						{
							consent.HasConsent = true;
							consent.ConsentDate = date;
							consent.ConsentModified = DateTime.Now;
							consent.ConsentModifiedBy = GetUserDetails();
						}
					}

					var suppliers = (from s in entities.Supplier.Include("Actor.ActorConsent")
									 where s.ActorCompanyId == actorCompanyId &&
									 suppliersDict.Contains(s.ActorSupplierId)
									 select s);

					foreach (Supplier supplier in suppliers)
					{
						ActorConsent consent = supplier.Actor.ActorConsent.FirstOrDefault(a => a.ConsentType == (int)ActorConsentType.Unspecified);
						if (consent == null)
						{
							consent = new ActorConsent();
							consent.ActorId = supplier.Actor.ActorId;
							consent.HasConsent = true;
							consent.ConsentDate = date;
							consent.ConsentModified = DateTime.Now;
							consent.ConsentModifiedBy = GetUserDetails();

							entities.ActorConsent.AddObject(consent);
						}
						else
						{
							consent.HasConsent = true;
							consent.ConsentDate = date;
							consent.ConsentModified = DateTime.Now;
							consent.ConsentModifiedBy = GetUserDetails();
						}
					}

					var contactPersons = (from s in entities.ContactPerson.Include("Actor.ActorConsent")
										  where contactPersonsDict.Contains(s.Actor.ActorId)
										  select s);

					foreach (ContactPerson contactPerson in contactPersons)
					{
						ActorConsent consent = contactPerson.Actor.ActorConsent.FirstOrDefault(a => a.ConsentType == (int)ActorConsentType.Unspecified);
						if (consent == null)
						{
							consent = new ActorConsent();
							consent.ActorId = contactPerson.Actor.ActorId;
							consent.HasConsent = true;
							consent.ConsentDate = date;
							consent.ConsentModified = DateTime.Now;
							consent.ConsentModifiedBy = GetUserDetails();

							entities.ActorConsent.AddObject(consent);
						}
						else
						{
							consent.HasConsent = true;
							consent.ConsentDate = date;
							consent.ConsentModified = DateTime.Now;
							consent.ConsentModifiedBy = GetUserDetails();
						}
					}

					return SaveChanges(entities);
				}
				catch (Exception ex)
				{
					base.LogError(ex, this.log);
					return new ActionResult(ex);
				}
				finally
				{
					entities.Connection.Close();
				}
			}
		}

		public ActionResult DeleteActorsWithoutConsent(List<int> customersDict, List<int> suppliersDict, List<int> contactPersonsDict)
		{
			ActionResult result = new ActionResult();

			using (CompEntities entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();

					foreach (int customerId in customersDict)
					{
						result = CustomerManager.DeleteCustomer(entities, customerId, base.ActorCompanyId, clearValues: true);
						if (!result.Success)
							return new ActionResult(false);
					}

					foreach (int supplierId in suppliersDict)
					{
						result = SupplierManager.DeleteSupplier(entities, supplierId, base.ActorCompanyId, true);
						if (!result.Success)
							return new ActionResult(false);
					}

					foreach (int contactPersonId in contactPersonsDict)
					{
						result = ContactManager.DeleteContactPerson(entities, contactPersonId, clearValues: true);
						if (!result.Success)
							return new ActionResult(false);
					}
				}
				catch (Exception ex)
				{
					base.LogError(ex, this.log);
				}
				finally
				{
					entities.Connection.Close();
				}
			}

			return result;
		}

		#endregion

		#region EmployeeUserDTO

		public EmployeeUserDTO GetEmployeeUserDTOFromUser(int userId, int actorCompanyId, int? currentUserId, bool loadExternalAuthId = false)
		{
			User user = UserManager.GetUserIgnoreState(userId, loadEmployee: true, loadContactPerson: true, loadAttestRoleUser: true);
			if (user == null)
				return null;

			Company company = CompanyManager.GetCompany(actorCompanyId);

			EmployeeUserDTO dto = user.ToDTO(null, company, user.ContactPerson, null);

			Employee employee = EmployeeManager.GetEmployeeForUser(user, actorCompanyId);
			if (employee != null)
			{
				dto.EmployeeId = employee.EmployeeId;
				dto.EmployeeNr = employee.EmployeeNr;
			}

			if (loadExternalAuthId && user.idLoginGuid != null)
			{
				string value = SettingManager.GetStringSetting(SettingMainType.License, (int)LicenseSettingType.SSO_Key, 0, 0, dto.LicenseId);
				if (!string.IsNullOrEmpty(value) && Guid.TryParse(value, out Guid idProviderGuid))
				{
					dto.ExternalAuthId = SoftOneIdConnector.GetExternalAuthId(user.idLoginGuid.Value, idProviderGuid);

					if (string.IsNullOrEmpty(dto.ExternalAuthId))
					{
						dto.ExternalAuthId = SettingManager.GetStringSetting(SettingMainType.User, (int)UserSettingType.ExternalAuthId, dto.UserId, 0, 0);
					}
				}
			}

			if (SettingManager.SettingIsTrue(SettingMainType.License, (int)LicenseSettingType.LifetimeSecondsEnabledOnUser, 0, 0, user.LicenseId, 60 * 10))
				dto.LifetimeSeconds = SettingManager.GetIntSetting(SettingMainType.User, (int)UserSettingType.LifetimeSeconds, dto.UserId, 0, 0);

			return dto;
		}

		public EmployeeUserDTO GetEmployeeUserDTOFromEmployee(int employeeId, int actorCompanyId, DateTime? date = null,
			bool applyFeatures = false,
			bool loadEmploymentAccounting = false,
			bool loadEmploymentPriceTypes = false,
			bool loadEmploymentVacationGroups = false,
			bool loadEmploymentVacationGroupSE = false,
			bool loadEmployeeAccounts = false,
			bool loadEmployeeChilds = false,
			bool loadEmployeeChildCares = false,
			bool loadEmployeeFactors = false,
			bool loadEmployeeMeetings = false,
			bool loadEmployeeSkills = false,
			bool loadEmployeeTemplate = false,
			bool loadEmployeeTemplateGroups = false,
			bool loadEmployeeTimeWorkAccounts = false,
			bool loadEmployeeUnionFees = false,
			bool loadEmployeeVacation = false,
			bool loadRoles = false,
			bool loadEmployeeSettings = false,
			bool loadExternalAuthId = false,
			bool clearPassword = false,
			bool fromApi = false
			)
		{
			Employee employee = EmployeeManager.GetEmployeeIgnoreState(actorCompanyId, employeeId,
				loadContactPerson: true,
				loadUser: true,
				loadEmployment: true,
				loadEmploymentAccounting: loadEmploymentAccounting,
				loadEmploymentPriceType: loadEmploymentPriceTypes,
				loadEmploymentVacationGroupSE: loadEmploymentVacationGroupSE,
				loadEmployeeAccounts: loadEmployeeAccounts,
				loadEmployeeFactors: loadEmployeeFactors,
				loadEmployeeSettings: loadEmployeeSettings,
				loadEmployeeSkill: loadEmployeeSkills,
				loadEmployeeTemplate: loadEmployeeTemplate,
				loadEmployeeVacation: loadEmployeeVacation);
			if (employee == null)
				return null;

			Company company = GetCompanyFromCache(actorCompanyId);
			User user = employee?.User;
			ContactPerson contactPerson = employee?.ContactPerson;
			EmployeeManager.SetEmployeeFactorNames(employee.EmployeeFactor);

			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadOnly, actorCompanyId);
			List<CompanyCategoryRecord> categoryRecords = useAccountHierarchy ? null : CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employee.EmployeeId, actorCompanyId);

			AttestState attestStateResultingPayroll = loadEmployeeChilds || loadEmployeeChildCares ? AttestManager.GetAttestState(SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, 0, actorCompanyId, 0)) : null;
			List<EmployeeChild> employeeChilds = loadEmployeeChilds ? EmployeeManager.GetEmployeeChilds(employee.EmployeeId, actorCompanyId, true, attestStateResultingPayroll) : null;
			List<EmployeeChildCareDTO> employeeChildCares = loadEmployeeChildCares ? EmployeeManager.GetEmployeeChildCareDTOs(employee.EmployeeId, actorCompanyId, attestStateResultingPayroll) : null;
			List<EmployeeMeeting> employeeMeetings = loadEmployeeMeetings ? EmployeeManager.GetEmployeeMeetings(employee.EmployeeId, actorCompanyId, base.UserId, true) : null;
			List<EmployeeTimeWorkAccount> employeeTimeWorkAccounts = loadEmployeeTimeWorkAccounts ? EmployeeManager.GetEmployeeTimeWorkAccounts(employee.EmployeeId) : null;
			List<TimeScheduleTemplateGroupEmployee> employeeTemplateGroups = loadEmployeeTemplateGroups ? TimeScheduleManager.GetTimeScheduleTemplateGroupsForEmployee(actorCompanyId, employee.EmployeeId, true, true) : null;
			List<EmployeeUnionFee> employeeUnionFees = loadEmployeeUnionFees ? EmployeeManager.GetEmployeeUnionFees(employee.EmployeeId) : null;

			EmployeeUserDTO employeeUser = employee.ToDTO(
				company,
				user,
				contactPerson,
				date,
				categoryRecords,
				employeeChilds,
				employeeChildCares,
				employeeUnionFees,
				employeeMeetings,
				employeeTemplateGroups,
				employeeTimeWorkAccounts,
				loadEmploymentAccounting: loadEmploymentAccounting,
				loadEmploymentPriceTypes: loadEmploymentPriceTypes,
				loadEmploymentVacationGroups: loadEmploymentVacationGroups,
				loadEmploymentVacationGroupSE: loadEmploymentVacationGroupSE,
				loadEmployeeSkills: loadEmployeeSkills
				);

			if (employeeUser != null)
			{
				EmployeeManager.ApplyEmployment(employeeUser, DateTime.Today, actorCompanyId);
				if (applyFeatures)
				{
					EmployeeUserApplyFeaturesResult applyFeaturesResult = ApplyFeaturesOnEmployee(employeeUser);
					if (applyFeaturesResult?.EmployeeUserDTO != null)
						employeeUser = applyFeaturesResult.EmployeeUserDTO;
				}
			}
			if (employeeUser != null)
			{
				if (clearPassword)
				{
					employeeUser.Password = null;
					employeeUser.NewPassword = null;
				}
				if (loadExternalAuthId && user?.idLoginGuid != null)
				{
					string value = SettingManager.GetStringSetting(SettingMainType.License, (int)LicenseSettingType.SSO_Key, 0, 0, employeeUser.LicenseId);
					if (!string.IsNullOrEmpty(value) && Guid.TryParse(value, out Guid idProviderGuid))
					{
						employeeUser.ExternalAuthId = SoftOneIdConnector.GetExternalAuthId(user.idLoginGuid.Value, idProviderGuid);
						if (string.IsNullOrEmpty(employeeUser.ExternalAuthId))
						{
							employeeUser.ExternalAuthId = SettingManager.GetStringSetting(SettingMainType.User, (int)UserSettingType.ExternalAuthId, employeeUser.UserId, 0, 0);
						}
					}
				}
				if (SettingManager.SettingIsTrue(SettingMainType.License, (int)LicenseSettingType.LifetimeSecondsEnabledOnUser, 0, 0, employeeUser.LicenseId, 60 * 10))
				{
					employeeUser.LifetimeSeconds = SettingManager.GetIntSetting(SettingMainType.User, (int)UserSettingType.LifetimeSeconds, employeeUser.UserId, 0, 0);
				}
				if (loadRoles)
				{
					employeeUser.TryAddUserRoles(UserManager.GetUserRolesDTO(employeeUser.UserId, true));
				}
			}

			if (!fromApi && employeeUser != null && loadEmploymentPriceTypes && !FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary_IncludeAllPeriods, Permission.Readonly, RoleId, actorCompanyId, LicenseId))
			{
				var firstDate = employeeUser.Employments?
					.SelectMany(e => e.PriceTypes ?? Enumerable.Empty<EmploymentPriceTypeDTO>())
					.SelectMany(p => p.Periods ?? Enumerable.Empty<EmploymentPriceTypePeriodDTO>())
					.Min(p => p.FromDate);
				var lastDate = DateTime.Today.AddYears(1);

				foreach (var employment in employeeUser.Employments)
				{
					if (employment.PriceTypes == null)
						continue;
					foreach (var priceType in employment.PriceTypes)
					{
						if (priceType.Periods == null)
							continue;

						foreach (var period in priceType.Periods)
						{
							bool hide = false;
							var next = priceType.Periods.FirstOrDefault(p => p.FromDate > period.FromDate);

							if (period.FromDate.HasValue && !UserManager.HasPermissionToEmployee(entitiesReadOnly, period.FromDate.Value, next?.FromDate ?? DateTime.Today.AddYears(10), actorCompanyId, UserId, RoleId, employee))
								hide = true;

							if (hide)
							{
								period.Hidden = true;
								period.Amount = 0;
							}
						}
					}
				}
			}




			//if (!fromApi && employeeUser != null && loadEmploymentPriceTypes && !FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary_IncludeAllPeriods, Permission.Readonly, RoleId, actorCompanyId, LicenseId))
			//{
			//    var firstDate = employeeUser.Employments.SelectMany(e => e.PriceTypes).SelectMany(p => p.Periods).Min(p => p.FromDate) ?? CalendarUtility.DATETIME_DEFAULT;
			//    var lastDate = DateTime.Today.AddYears(1);
			//    var validIntervals = UserManager.HasPermissionToEmployeeIntervals(entitiesReadOnly, firstDate, lastDate, actorCompanyId, UserId, RoleId, employee);
			//    foreach (var interval in validIntervals)
			//    {
			//        foreach (var employment in employeeUser.Employments)
			//        {
			//            foreach (var priceType in employment.PriceTypes)
			//            {
			//                foreach (var period in priceType.Periods)
			//                {
			//                    bool hide = false;
			//                    var next = priceType.Periods.FirstOrDefault(p => p.FromDate > period.FromDate);
			//                    if (!CalendarUtility.IsCurrentOverlappedByNew(period.FromDate ?? CalendarUtility.DATETIME_DEFAULT, next?.FromDate ?? lastDate, interval.StartDateValue, interval.StopDateValue))
			//                        hide = true;

			//                    if (hide)
			//                    {
			//                        period.Hidden = true;
			//                        period.Amount = 0;
			//                    }
			//                }
			//            }
			//        }
			//    }
			//}

			return employeeUser;
		}

		public EmployeeUserDTO DownloadEmployeeUserDTOFromEmployee(int employeeId, int actorCompanyId)
		{
			EmployeeUserDTO dto = GetEmployeeUserDTOFromEmployee(employeeId, actorCompanyId,
				applyFeatures: false,
				loadEmploymentAccounting: true,
				loadEmploymentPriceTypes: true,
				loadEmploymentVacationGroups: true,
				loadEmploymentVacationGroupSE: true,
				loadEmployeeAccounts: true,
				loadEmployeeChilds: true,
				loadEmployeeChildCares: true,
				loadEmployeeFactors: true,
				loadEmployeeMeetings: true,
				loadEmployeeSkills: true,
				loadEmployeeTemplateGroups: true,
				loadEmployeeUnionFees: true,
				loadEmployeeVacation: true,
				clearPassword: true);
			if (dto == null)
				return null;

			bool userIsMySelf = dto.UserId == base.UserId;
			bool gdprPermsission = FeatureManager.HasRolePermission(Feature.Manage_GDPR_Logs, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId);
			if (!userIsMySelf && !gdprPermsission)
				return null;

			return dto;
		}

		public EmployeeUserApplyFeaturesResult ApplyFeaturesOnEmployee(EmployeeUserDTO employeeUser)
		{
			if (employeeUser == null)
				return null;

			EmployeeUserApplyFeaturesResult result = new EmployeeUserApplyFeaturesResult(employeeUser, base.LicenseId, base.ActorCompanyId, base.RoleId, base.UserId);
			result.SetPermissions(FeatureManager.HasRolePermissions(result.Features, Permission.Readonly, result.LicenseId, result.ActorCompanyId, result.RoleId));

			#region Personal

			bool socialSecReadPermission = result.IsMySelfOrHasPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec);
			if (!socialSecReadPermission)
				result.BlankSocialSec();


			//cardNumberReadPermission
			if (!result.IsMySelfOrHasPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_User_CardNumber))
				result.BlankCardNumber();

			//contactDisbursementAccountReadPermission
			if (!result.HasPermission(Feature.Time_Employee_Employees_Edit_MySelf_Contact_DisbursementAccount, Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount))
				result.BlankDisbursement();

			#endregion

			#region Employment

			if (employeeUser.Employments != null)
			{
				foreach (EmploymentDTO employment in employeeUser.Employments)
				{
					//employmentPayrollSalaryReadPermission
					if (!result.HasPermission(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Payroll, Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary))
						result.BlankPriceTypes(employment);

					//employmentAccountsReadPermission
					if (!result.HasPermission(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Accounts, Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
						result.BlankAccounts(employment);
				}
			}
			//employeeWorkTimeAccount
			if (!result.HasPermission(Feature.Time_Employee_Employees_Edit_MySelf_WorkTimeAccount, Feature.Time_Employee_Employees_Edit_OtherEmployees_WorkTimeAccount))
				result.BlankTimeWorkAccounts();

			//employeeUnionFeeReadPermission
			if (!result.HasPermission(Feature.Time_Employee_Employees_Edit_MySelf_UnionFee, Feature.Time_Employee_Employees_Edit_OtherEmployees_UnionFee))
				result.BlankUnionFees();

			//absenceVacationVacationReadPermission
			if (!result.HasPermission(Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Vacation, Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation))
				result.BlankEmployeeVacationSE();

			//absenceVacationAbsenceReadPermission
			if (!result.HasPermission(Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Absence, Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Absence))
			{
				result.BlankHighRiskProtection();
				result.BlankMedicalCertificateReminder();
			}

			//employeeChildReadPermission
			if (!result.HasPermission(Feature.Time_Employee_Employees_Edit_MySelf_Contact_Children, Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_Children))
				result.BlankChildCare();

			#endregion

			#region HR

			//skillsReadPermission
			if (!result.HasPermission(Feature.Time_Employee_Employees_Edit_MySelf_Skills, Feature.Time_Employee_Employees_Edit_OtherEmployees_Skills))
				result.BlankEmployeeSkills();

			//employeeMeetingReadPermission
			if (!result.HasPermission(Feature.Time_Employee_Employees_Edit_MySelf_EmployeeMeeting, Feature.Time_Employee_Employees_Edit_OtherEmployees_EmployeeMeeting))
				result.BlankEmployeeMeetings();

			//noteReadPermission
			if (!result.HasPermission(Feature.Time_Employee_Employees_Edit_MySelf_Note, Feature.Time_Employee_Employees_Edit_OtherEmployees_Note))
				result.BlankNote();

			#endregion

			return result;
		}

		public List<EmploymentDTO> GetEmployments(int employeeId, int actorCompanyId, DateTime? date = null, bool loadEmploymentAccounting = false)
		{
			Employee employee = EmployeeManager.GetEmployeeIgnoreState(actorCompanyId, employeeId,
				loadEmployment: true,
				loadEmploymentAccounting: true,
				loadEmploymentPriceType: true,
				loadEmploymentVacationGroupSE: true
				);

			Company company = CompanyManager.GetCompany(actorCompanyId);

			EmployeeUserDTO dto = employee.ToDTO(company, null, null, date,
				loadEmploymentAccounting: loadEmploymentAccounting,
				loadEmploymentPriceTypes: true,
				loadEmploymentVacationGroups: true,
				loadEmploymentVacationGroupSE: true,
				loadEmployeeSkills: false
				);

			if (dto != null)
				EmployeeManager.ApplyEmployment(dto, date ?? DateTime.Today, actorCompanyId);

			return dto?.Employments ?? new List<EmploymentDTO>();
		}

		public ActionResult ValidateSaveEmployee(EmployeeUserDTO employeeUser, List<ContactAddressItem> contactAdresses)
		{
			ActionResult result = new ActionResult(true);

			if (employeeUser == null)
				return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "EmployeeUser");

			bool invalidChange = false;

			if (employeeUser.SaveEmployee)
			{
				#region Employee

				#region Employments

				if (result.Success && employeeUser.Employments != null)
				{
					List<EmployeeGroup> employeeGroups = EmployeeManager.GetEmployeeGroups(employeeUser.ActorCompanyId);

					result = employeeUser.Employments.ValidateEmployments(employeeGroups: employeeGroups.ToDTOs().ToList(), validateFixedTerm14days: true, validateWorkTimeWeek: true);
					if (!result.Success)
					{
						if (result.ErrorNumber == (int)ActionResultSave.DatesInvalid)
						{
							result.ErrorMessage = GetText(11053, "Anställning har startdatum större än stoppdatum");
							invalidChange = true;
						}
						else if (result.ErrorNumber == (int)ActionResultSave.DatesOverlapping)
						{
							result.ErrorMessage = GetText(11054, "En anställning överlappar en annan anställning");
							invalidChange = true;
						}
						else if (result.ErrorNumber == (int)ActionResultSave.EmployeeGroupMandatory)
						{
							result.ErrorMessage = GetText(8539, "Tidavtal hittades inte");
							invalidChange = true;
						}
						else if (result.ErrorNumber == (int)ActionResultSave.EmployeeEmploymentsInvalidFixedTerm14days)
						{
							result.ErrorMessage = GetText(11528, "Anställning med anställningsform 'Allmän visstidsanställning 14 dagar' måste vara 14 dagar");
							invalidChange = true;
						}
						else if (result.ErrorNumber == (int)ActionResultSave.EmployeeEmploymentsInvalidWorkTimeWeek)
						{
							result.ErrorMessage = String.Format(GetText(11709, "Veckoarbetstiden är {0} men tidavtalets är {1}. Detta påverkar bl.a. arbetstidsregler, årsarbetstid och löneberäkningar där veckoarbetstid eller sysselsättningsgrad ingår"), result.Strings != null && result.Strings.Count > 0 ? result.Strings[0] : "0", result.Strings != null && result.Strings.Count > 1 ? result.Strings[1] : "> 0");
						}
						else
						{
							result.ErrorMessage = GetText(11710, "Anställningar har ogiltiga interval");
							invalidChange = true;
						}
					}
				}

				#endregion

				#region EmploymentVacationGroup

				if (result.Success && employeeUser.Employments != null)
				{
					result = employeeUser.Employments.ValidateEmploymentVacationGroups();
					if (!result.Success && result.ErrorNumber == (int)ActionResultSave.EmploymentVacationGroupsCannotBeDuplicate)
					{
						result.ErrorMessage = string.Format(GetText(0, "Anställning har flera semesteravtal med samma fr.o.m datum {0}"), result.StringValue);
						invalidChange = true;
					}
				}

				#endregion

				#region EmployeeAuth

				if (result.Success)
				{
					using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
					bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, employeeUser.ActorCompanyId);
					if (useAccountHierarchy)
					{
						if (employeeUser.Accounts != null && employeeUser.Accounts.IsEndingBeforeLastEmployment(employeeUser.Employments))
						{
							result = new ActionResult((int)ActionResultSave.EmployeeCategoriesEndingBeforeLastEmployment, GetText(11871, "Den anställdes tillhörighet slutar innan sista anställningen slutar"));
							invalidChange = true;
						}
					}
					else
					{
						if (employeeUser.CategoryRecords != null && employeeUser.CategoryRecords.IsEndingBeforeLastEmployment(employeeUser.Employments))
						{
							result = new ActionResult((int)ActionResultSave.EmployeeCategoriesEndingBeforeLastEmployment, GetText(11712, "Den anställdes kategori slutar innan sista anställningen slutar"));
							invalidChange = true;
						}
					}
				}

				#endregion

				#region EmployeeChilds

				if (result.Success && employeeUser.EmployeeChilds != null && employeeUser.EmployeeChilds.Any(i => !i.BirthDate.HasValue))
				{
					result = new ActionResult((int)ActionResultSave.EmployeeChildBirthDateMissing, GetText(11711, "Födelsedatum är obligatorisk på barn"));
					invalidChange = true;
				}

				#endregion

				#region Email

				if (result.Success && !employeeUser.Vacant && (contactAdresses == null || !contactAdresses.Any(c => !c.IsAddress && !String.IsNullOrEmpty(c.EComText) && c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email)))
				{
					result = new ActionResult(false);
					result.Success = false;
					result.ErrorMessage = GetText(11059, "Du måste ange e-postadress");
					invalidChange = true;
				}

				#endregion

				#region Meetings

				if (result.Success && employeeUser.IsEmployeeMeetingsChanged && employeeUser.EmployeeMeetings.Any() && employeeUser.EmployeeMeetings.Any(i => i.ParticipantIds.Count == 0))
				{
					result = new ActionResult(false);
					result.Success = false;
					result.ErrorMessage = GetText(11060, "Du måste välja deltagare i samtal");
					invalidChange = true;
				}

				#endregion

				#endregion
			}

			if (!result.Success && invalidChange)
				result.BooleanValue = invalidChange;

			return result;
		}

		public List<InactivateEmployeeDTO> ValidateInactivateEmployees(List<int> employeeIds, int actorCompanyId)
		{
			List<InactivateEmployeeDTO> result = new List<InactivateEmployeeDTO>();

			foreach (int employeeId in employeeIds)
			{
				Employee employee = EmployeeManager.GetEmployee(employeeId, actorCompanyId, onlyActive: false, loadContactPerson: true);
				InactivateEmployeeDTO dto = new InactivateEmployeeDTO();
				dto.EmployeeId = employeeId;
				dto.EmployeeNr = employee != null ? employee.EmployeeNr : string.Empty;
				dto.EmployeeName = employee != null ? employee.Name : string.Empty;

				ActionResult employeeResult = ValidateInactivateEmployee(employeeId);
				dto.Success = employeeResult.Success;
				dto.Message = employeeResult.Strings.JoinToString(", ");

				result.Add(dto);
			}

			return result;
		}

		public List<InactivateEmployeeDTO> InactivateEmployees(List<int> employeeIds, int actorCompanyId)
		{
			List<InactivateEmployeeDTO> result = new List<InactivateEmployeeDTO>();

			foreach (int employeeId in employeeIds)
			{
				InactivateEmployeeDTO dto = new InactivateEmployeeDTO();
				dto.EmployeeId = employeeId;

				DeleteEmployeeDTO input = new DeleteEmployeeDTO();
				input.EmployeeId = employeeId;
				input.Action = DeleteEmployeeAction.Inactivate;
				ActionResult employeeResult = EmployeeManager.DeleteEmployee(input);

				dto.Success = employeeResult.Success;
				if (!employeeResult.Success)
				{
					Employee employee = EmployeeManager.GetEmployee(employeeId, actorCompanyId, onlyActive: false, loadContactPerson: true);
					dto.EmployeeNr = employee != null ? employee.EmployeeNr : string.Empty;
					dto.EmployeeName = employee != null ? employee.Name : string.Empty;
					dto.Message = employeeResult.ErrorMessage;
				}

				result.Add(dto);
			}

			return result;
		}

		public ActionResult ValidateInactivateEmployee(int employeeId)
		{
			bool success = true;
			List<string> messages = new List<string>();

			//Validate employee
			Employee employee = EmployeeManager.GetEmployee(employeeId, base.ActorCompanyId, onlyActive: false, loadEmployment: true);
			if (employee == null)
			{
				success = false;
				messages.Add(GetText(10083, "Anställd hittades inte") + ".");
			}
			else
			{
				//Validate employment
				Employment lastEmployment = employee.GetLastEmployment();
				if (lastEmployment != null)
				{
					//Validate employment enddate
					DateTime? endDate = lastEmployment.GetEndDate();
					if (!endDate.HasValue)
					{
						success = false;
						messages.Add(GetText(11729, "Den sista anställningen saknar slutdatum."));
					}
				}
			}

			return new ActionResult(success)
			{
				Strings = messages
			};
		}

		public ActionResult ValidateDeleteEmployee(int employeeId)
		{
			bool success = true;
			List<string> messages = new List<string>();

			//Validate employee
			Employee employee = EmployeeManager.GetEmployee(employeeId, base.ActorCompanyId, onlyActive: false, loadEmployment: true);
			if (employee == null)
			{
				success = false;
				messages.Add(GetText(10083, "Anställd hittades inte") + ".");
			}
			else
			{
				//Validate employment
				Employment lastEmployment = employee.GetLastEmployment();
				if (lastEmployment == null)
				{
					success = false;
					messages.Add(GetText(10084, "Anställning hittades inte") + ".");
				}
				else
				{
					//Validate employment enddate
					DateTime? endDate = lastEmployment.GetEndDate();
					if (!endDate.HasValue)
					{
						success = false;
						messages.Add(GetText(11729, "Den sista anställningen saknar slutdatum."));
					}
					else if (employee.GetEmployments(CalendarUtility.DATETIME_DEFAULT, endDate.Value).Count != 1 || lastEmployment.GetEndDate() > lastEmployment.GetEmploymentDate())
					{
						// If only one employment and employment date and end date are the same, it's not neccessary to keep

						//Validate keep employee years after end
						int keepYearsAfterEnd = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.EmployeeKeepNbrOfYearsAfterEnd, 0, base.ActorCompanyId, 0);
						if (keepYearsAfterEnd == 0)
							keepYearsAfterEnd = 7; //default
						DateTime minEndDate = CalendarUtility.GetBeginningOfNextYear(endDate.Value).AddYears(keepYearsAfterEnd);
						if (minEndDate > DateTime.Today)
						{
							success = false;
							messages.Add(String.Format(GetText(11730, "Den anställde måste behållas till {0}."), minEndDate.ToShortDateString()));
						}
					}

					//Validate payroll finalsalary
					bool usePayroll = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UsePayroll, 0, base.ActorCompanyId, 0);
					if (usePayroll)
					{
						if (lastEmployment.FinalSalaryStatus == (int)SoeEmploymentFinalSalaryStatus.ApplyFinalSalary)
						{
							success = false;
							messages.Add(GetText(11750, "Slutavräknas har markerats för den sista anställningen, men har inte slutavräknats ännu."));
						}
						else if (lastEmployment.FinalSalaryStatus == (int)SoeEmploymentFinalSalaryStatus.None)
						{
							//Do not set success false, ONLY WARNING
							messages.Add(GetText(11731, "Varning. Slutlön har inte körts för den sista anställningen."));
						}
					}
				}
			}

			return new ActionResult(success)
			{
				Strings = messages
			};
		}

		public ActionResult ValidateImmediateDeleteEmployee(int employeeId)
		{
			bool success = true;
			List<string> messages = new List<string>();

			//Validate employee
			Employee employee = EmployeeManager.GetEmployee(employeeId, base.ActorCompanyId, onlyActive: false, loadEmployment: true);
			if (employee == null)
			{
				success = false;
				messages.Add(GetText(10083, "Anställd hittades inte") + ".");
			}
			else
			{
				//Validate employment
				List<Employment> employments = employee.GetActiveEmployments();
				if (employments.Any(e => !e.DateTo.HasValue || e.DateTo.Value > DateTime.Today))
				{
					success = false;
					messages.Add(GetText(11732, "Den anställde har en aktiv anställning."));
				}
				else
				{
					Employment lastEmployment = employee.GetLastEmployment();
					if (lastEmployment != null)
					{
						//Validate employment enddate
						DateTime? endDate = lastEmployment.GetEndDate();
						if (!endDate.HasValue)
						{
							success = false;
							messages.Add(GetText(11729, "Den sista anställningen saknar slutdatum."));
						}
						else if (employee.GetEmployments(CalendarUtility.DATETIME_DEFAULT, endDate.Value).Count != 1 || lastEmployment.GetEndDate() > lastEmployment.GetEmploymentDate())
						{
							// If only one employment and employment date and end date are the same, it's not neccessary to keep

							//Validate keep employee years after end
							int keepYearsAfterEnd = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.EmployeeKeepNbrOfYearsAfterEnd, 0, base.ActorCompanyId, 0);
							if (keepYearsAfterEnd == 0)
								keepYearsAfterEnd = 7; //default
							DateTime minEndDate = CalendarUtility.GetBeginningOfNextYear(endDate.Value).AddYears(keepYearsAfterEnd);
							if (minEndDate > DateTime.Today)
							{
								success = false;
								messages.Add(String.Format(GetText(11730, "Den anställde måste behållas till {0}."), minEndDate.ToShortDateString()));
							}
						}

						//Validate payroll finalsalary
						bool usePayroll = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UsePayroll, 0, base.ActorCompanyId, 0);
						if (usePayroll)
						{
							if (lastEmployment.FinalSalaryStatus == (int)SoeEmploymentFinalSalaryStatus.ApplyFinalSalary)
							{
								success = false;
								messages.Add(GetText(11750, "Slutavräknas har markerats för den sista anställningen, men har inte slutavräknats ännu."));
							}
							else if (lastEmployment.FinalSalaryStatus == (int)SoeEmploymentFinalSalaryStatus.None)
							{
								//Do not set success false, ONLY WARNING
								messages.Add(GetText(11731, "Varning. Slutlön har inte körts för den sista anställningen."));
							}
						}
					}
				}

				//Validate transactions
				if (TimeTransactionManager.HasEmployeeTimePayrollTransactions(employee.EmployeeId, CalendarUtility.GetBeginningOfYear(DateTime.Today).AddYears(-1)))
				{
					success = false;
					messages.Add(GetText(11733, "Den anställde har aktiva tidstransaktioner."));
				}
			}

			return new ActionResult(success)
			{
				Strings = messages
			};
		}

		public ActionResult ValidateSaveUser(EmployeeUserDTO employeeUser, List<ContactAddressItem> contactAdresses)
		{
			ActionResult result = new ActionResult(true);

			if (employeeUser == null)
				return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "EmployeeUser");

			bool invalidChange = false;
			if (employeeUser.SaveUser && result.Success && (contactAdresses == null || !contactAdresses.Any(c => !c.IsAddress && !String.IsNullOrEmpty(c.EComText) && c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email)))
			{
				result = new ActionResult(false, (int)ActionResultSave.Unknown, GetText(11059, "Du måste ange e-postadress"));
				invalidChange = true;
			}

			if (!result.Success && invalidChange)
				result.BooleanValue = invalidChange;

			return result;
		}

		public ActionResult ValidateDeleteUser(int userId)
		{
			List<string> messages = new List<string>();

			User user = UserManager.GetUser(userId, onlyActive: false, loadEmployee: true);
			if (user == null)
				messages.Add(GetText(10112, "Användare hittades inte") + ".");
			if (user != null && user.Employee.Any(i => i.State != (int)SoeEntityState.Deleted))
				messages.Add(GetText(12078, "Användare är kopplad till anställd. Den måste kopplas bort innan användaren kan tas bort") + ".");

			return new ActionResult(success: !messages.Any())
			{
				Strings = messages
			};
		}

		public ActionResult SaveEmployeeUserFromTerminal(int actorCompanyId, EmployeeUserDTO dto, List<ContactAddressItem> contactAddresses)
		{
			int userActorCompanyId = dto.DefaultActorCompanyId ?? actorCompanyId;

			using (var entities = new CompEntities())
			{
				#region Employee

				if (dto.SaveEmployee)
				{
					#region Employments

					if (dto.Employments == null)
						dto.Employments = new List<EmploymentDTO>();
					if (dto.EmployeeId > 0)
						dto.Employments.AddRange(EmployeeManager.GetEmployments(dto.EmployeeId, userActorCompanyId).ToDTOs(includeEmployeeGroup: true));

					if (dto.Employments.Count == 0)
					{
						dto.Employments.Add(new EmploymentDTO()
						{
							EmploymentType = (int)TermGroup_EmploymentType.Unknown,
							DateFrom = null,
							DateTo = null,

							//Set FK
							EmployeeGroupId = dto.CurrentEmployeeGroupId,
							PayrollGroupId = null,
							ActorCompanyId = userActorCompanyId,
						});
					}

					#endregion

					#region Categories

					if (dto.CategoryId > 0)
					{
						if (dto.CategoryRecords == null)
							dto.CategoryRecords = new List<CompanyCategoryRecordDTO>();

						if (!dto.CategoryRecords.Any(c => c.CategoryId == dto.CategoryId))
						{
							dto.CategoryRecords.Add(new CompanyCategoryRecordDTO()
							{
								CategoryId = dto.CategoryId,
								ActorCompanyId = userActorCompanyId,
								Entity = SoeCategoryRecordEntity.Employee,
								Default = true,
								RecordId = dto.EmployeeId,
							});
						}
					}

					#endregion
				}

				#endregion

				#region Save

				var result = this.SaveEmployeeUser(TermGroup_TrackChangesActionMethod.Employee_FromTerminal, dto, autoAddDefaultRole: true);
				var saveEmployeeIntDict = result.IntDict;
				if (result.Success)
				{
					dto.UserId = result.IntDict[(int)SaveEmployeeUserResult.UserId];
					dto.EmployeeId = result.IntDict[(int)SaveEmployeeUserResult.EmployeeId];
					dto.ActorContactPersonId = result.IntDict[(int)SaveEmployeeUserResult.ActorContactPersonId];
				}

				#endregion

				#region AttestRoles

				if (result.Success && !dto.AttestRoleIds.IsNullOrEmpty())
				{
					result = AttestManager.SaveAttestRoleUsers(entities, userActorCompanyId, dto.UserId, dto.AttestRoleIds.ToArray());
				}

				#endregion

				#region Addresses

				if (result.Success && contactAddresses != null && contactAddresses.Count > 0)
				{
					ContactPerson contactPerson = dto.ActorContactPersonId != 0 ? ContactManager.GetContactPersonIgnoreState(entities, dto.ActorContactPersonId) : null;
					if (contactPerson != null)
					{
						result = ContactManager.SaveContactAddresses(entities, contactAddresses, contactPerson.ActorContactPersonId, TermGroup_SysContactType.Company);
						if (!result.Success)
						{
							result.ErrorNumber = (int)ActionResultSave.EmployeeUserContactsAndTeleComNotSaved;
							result.ErrorMessage = GetText(11048, "Kontaktuppgifter ej sparade");
						}
					}
				}

				#endregion

				// Set return values
				if (result.Success)
					result.IntDict = saveEmployeeIntDict;

				return result;
			}
		}

		public ActionResult SaveEmployeeUser(
				TermGroup_TrackChangesActionMethod actionMethod,
				EmployeeUserDTO employeeUser,
				List<ContactAddressItem> contactAddresses = null,
				List<EmployeePositionDTO> employeePositions = null,
				List<EmployeeSkillDTO> employeeSkills = null,
				UserReplacementDTO userReplacement = null,
				EmployeeTaxSEDTO employeeTax = null,
				List<FileUploadDTO> files = null,
				List<UserRolesDTO> userRoles = null,
				bool saveRoles = false,
				bool saveAttestRoles = false,
				bool autoAddDefaultRole = false,
				bool generateCurrentChanges = false,
				bool doAcceptAttestedTemporaryEmployments = false,
				bool logChanges = true,
				bool skipCategoryCheck = false,
				bool onlyValidateAttestRolesInCompany = false,
				List<ExtraFieldRecordDTO> extraFields = null)
		{
			ActionResult result = null;
			SoeEntityType topEntity = (actionMethod == TermGroup_TrackChangesActionMethod.User_Save ? SoeEntityType.User : SoeEntityType.Employee);

			EmployeeUserApplyFeaturesResult applyFeaturesResult = ApplyFeaturesOnEmployee(employeeUser);
			EmployeeUserChangesRepositoryDTO changesRepository = TrackChangesManager.CreateEmployeeUserChangesRepository(employeeUser.ActorCompanyId, Guid.NewGuid(), actionMethod, topEntity, applyFeaturesResult);

			if (logChanges)
			{
				#region Collect before values

				Employee employee = EmployeeManager.GetEmployee(employeeUser.EmployeeId, employeeUser.ActorCompanyId, loadEmployment: true, loadVacationGroup: true, loadUser: true, loadEmployeeTax: true, loadContactPerson: true, loadEmployeeAccount: true, loadEmployeeSetting: true);
				if (employee != null)
				{
					changesRepository.SetBeforeValue(employee);
					changesRepository.SetBeforeValue(employee.User);

					if (employee.ContactPerson != null)
						changesRepository.SetBeforeValue(employee.ContactPerson);
					if (contactAddresses != null)
						changesRepository.SetBeforeValue(ContactManager.GetContactAddressItems(employeeUser.ActorContactPersonId), employee.EmployeeId);
					if (employee.EmployeeTaxSE != null)
						changesRepository.SetBeforeValue(employee.EmployeeTaxSE.ToList());
					if (employee.EmployeeAccount != null)
					{
						using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
						List<Account> accounts = entitiesReadOnly.Account.Where(a => a.ActorCompanyId == employeeUser.ActorCompanyId && a.State == (int)SoeEntityState.Active).ToList();
						changesRepository.SetBeforeValue(employee.EmployeeAccount.Where(e => e.State == (int)SoeEntityState.Active).ToList(), accounts);
					}
					if (employee.EmployeeSetting != null)
						changesRepository.SetBeforeValue(employee.EmployeeSetting.Where(e => e.State == (int)SoeEntityState.Active).ToList());
				}

				if (saveRoles || saveAttestRoles)
					changesRepository.SetBeforeValue(UserManager.GetUserRolesDTO(employeeUser.UserId, true));

				#endregion
			}

			bool changeAffectTerminal = false;
			Dictionary<int, int> keys = null;

			using (CompEntities entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();

					using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
					{
						result = SaveEmployeeUser(entities, transaction, actionMethod, employeeUser, applyFeaturesResult, changesRepository,
							contactAddresses: contactAddresses,
							employeePositions: employeePositions,
							employeeSkills: employeeSkills,
							userReplacement: userReplacement,
							employeeTax: employeeTax,
							files: files,
							userRoles: userRoles,
							saveRoles: saveRoles,
							saveAttestRoles: saveAttestRoles,
							autoAddDefaultRole: autoAddDefaultRole,
							generateCurrentChanges: generateCurrentChanges,
							doAcceptAttestedTemporaryEmployments: doAcceptAttestedTemporaryEmployments,
							skipCategoryCheck: skipCategoryCheck,
							onlyValidateAttestRolesInCompany: onlyValidateAttestRolesInCompany,
							extraFields: extraFields
							);

						if (result.Success)
						{
							keys = result.IntDict;
							if (result.BooleanValue2)
								changeAffectTerminal = true;

							if (logChanges)
							{
								if (contactAddresses != null)
									changesRepository.SetAfterValue(ContactManager.GetContactAddressItems(entities, employeeUser.ActorContactPersonId));

								if (saveRoles || saveAttestRoles)
									changesRepository.SetAfterValue(UserManager.GetUserRolesDTO(entities, employeeUser.UserId, true));

								ActionResult logResult = TrackChangesManager.SaveEmployeeUserChanges(entities, transaction, changesRepository);
								if (!logResult.Success)
									result = logResult;
							}

							if (employeeUser.ClearScheduleFrom.HasValue && employeeUser.EmployeeId != 0)
								result = TimeScheduleManager.SetScheduleAndDeviationsToDeleted(entities, employeeUser.EmployeeId, employeeUser.ActorCompanyId, employeeUser.ClearScheduleFrom.Value, saveChanges: true);
						}

						if (result.Success)
							transaction.Complete();
					}
				}
				catch (Exception ex)
				{
					base.LogError(ex, this.log);
					result = new ActionResult(ex);
				}
				finally
				{
					if (result.Success)
					{
						#region WebPubSub

						// Employment changes are checked inside of the actual save method
						// See result.BooleanValue2

						if (!changeAffectTerminal)
						{
							List<TrackChangesDTO> allTrackChangesItems = changesRepository?.GetChanges();
							if (!allTrackChangesItems.IsNullOrEmpty())
							{
								foreach (TrackChangesDTO item in allTrackChangesItems)
								{
									switch (item.ColumnType)
									{
										case TermGroup_TrackChangesColumnType.ContactPerson_FirstName:
										case TermGroup_TrackChangesColumnType.ContactPerson_LastName:
										case TermGroup_TrackChangesColumnType.Employee_EmployeeNr:
										case TermGroup_TrackChangesColumnType.Employee_Cardnumber:
										case TermGroup_TrackChangesColumnType.EmployeeAccount_Account:
										case TermGroup_TrackChangesColumnType.EmployeeAccount_DateFrom:
										case TermGroup_TrackChangesColumnType.EmployeeAccount_DateTo:
										case TermGroup_TrackChangesColumnType.EmployeeAccount_Default:
										case TermGroup_TrackChangesColumnType.Employee_EmploymentDate:
										case TermGroup_TrackChangesColumnType.Employee_EndDate:
											changeAffectTerminal = true;
											break;
									}
								}
							}
						}

						if (changeAffectTerminal && keys != null)
						{
							int employeeId = keys[(int)SaveEmployeeUserResult.EmployeeId];
							SendWebPubSubMessage(entities, base.ActorCompanyId, employeeId, WebPubSubMessageAction.Update);
						}

						var affectedEmployeeAccountsForCalculation = false;
						var affectedFromDate = (DateTime?)null;
						List<TrackChangesDTO> trackChangesItems = changesRepository?.GetChanges();
						bool settingRecalculateFutureAccountingWhenChangingMainAllocation = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.RecalculateFutureAccountingWhenChangingMainAllocation, 0, employeeUser.ActorCompanyId, 0, defaultValue: false);
						if (settingRecalculateFutureAccountingWhenChangingMainAllocation && !trackChangesItems.IsNullOrEmpty())
						{
							// Clear employee account cache.
							// Important to do this before recalculation if account changes has been made in a previous save short before this one.
							// Happens for example when doing an account change from the API. First message ends current account, second message creates new account shortly after.
							BusinessMemoryCache<List<EmployeeAccountDTO>>.Delete(AccountManager.GetEmployeeAccountCacheKey(employeeUser.EmployeeId, EmploymentAccountType.Cost));

							try
							{
								List<DateTime> possibleStartDates = new List<DateTime>();
								foreach (TrackChangesDTO item in trackChangesItems.Where(w => w.RecordId != 0 && w.Entity == SoeEntityType.EmployeeAccount))
								{
									EmployeeAccount employeeAccount = null;
									switch (item.ColumnType)
									{

										case TermGroup_TrackChangesColumnType.EmployeeAccount_Account:
										case TermGroup_TrackChangesColumnType.EmployeeAccount_DateFrom:
										case TermGroup_TrackChangesColumnType.EmployeeAccount_DateTo:
										case TermGroup_TrackChangesColumnType.EmployeeAccount_Default:
											employeeAccount = entities.EmployeeAccount.FirstOrDefault(ea => ea.EmployeeAccountId == item.RecordId);
											break;
										default:
											if (item.Action == TermGroup_TrackChangesAction.Insert)
												employeeAccount = entities.EmployeeAccount.FirstOrDefault(ea => ea.EmployeeAccountId == item.RecordId);
											break;
									}

									if (employeeAccount != null && employeeAccount.DateFrom != CalendarUtility.DATETIME_DEFAULT)
									{
										possibleStartDates.Add(employeeAccount.DateFrom);
										affectedEmployeeAccountsForCalculation = true;
									}
								}

                                if (affectedEmployeeAccountsForCalculation && possibleStartDates.Any())
                                {
                                    // Start recalculation from the earliest date, but not earlier than 45 days back
                                    DateTime minStartDate = possibleStartDates.Min();
                                    if (minStartDate < DateTime.Today.AddDays(-45))
                                        minStartDate = DateTime.Today.AddDays(-45);

									if (minStartDate >= DateTime.Today)
										affectedFromDate = minStartDate;
									else
									{
										// Check for last exported payroll attest status
										int setting = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, 0, employeeUser.ActorCompanyId, 0);
										if (setting == 0)
											affectedFromDate = minStartDate;
										else
										{
											var lastExportedDate = entities.TimePayrollTransaction.Include("TimeBlockDate")
																	.Where(w => w.EmployeeId == employeeUser.EmployeeId && w.AttestStateId == setting && w.State == (int)SoeEntityState.Active)
																	.OrderByDescending(o => o.TimeBlockDate.Date)
																	.FirstOrDefault()?.TimeBlockDate.Date;

											if (lastExportedDate.HasValue)
												affectedFromDate = lastExportedDate.Value.AddDays(1);
											else
												affectedFromDate = minStartDate;
										}
									}

                                    if (affectedFromDate.HasValue)
                                    {
                                        var lastScheduleDate = entities.TimeScheduleTemplateBlock
                                            .Where(w => w.EmployeeId == employeeUser.EmployeeId && w.State == (int)SoeEntityState.Active)
                                            .OrderByDescending(o => o.Date)
                                            .Select(s => s.Date)
                                            .FirstOrDefault();

										if (!lastScheduleDate.HasValue)
											lastScheduleDate = DateTime.Today;

										List<TimeBlockDate> timeBlockDates = TimeBlockManager.GetTimeBlockDates(employeeUser.EmployeeId, affectedFromDate.Value, lastScheduleDate.Value);
										if (timeBlockDates.Any())
										{
											var lastDateInTimeBlockDates = timeBlockDates.Max(m => m.Date);

											if (lastDateInTimeBlockDates < lastScheduleDate)
												timeBlockDates.AddRange(TimeBlockManager.CreateAndGetNewTimeBlockDates(entities, lastDateInTimeBlockDates.AddDays(1), lastScheduleDate.Value, ActorCompanyId, employeeUser.EmployeeId));

											List<AttestEmployeeDaySmallDTO> items = new List<AttestEmployeeDaySmallDTO>();
											foreach (TimeBlockDate timeBlockDate in timeBlockDates)
											{
												items.Add(new AttestEmployeeDaySmallDTO
												{
													EmployeeId = employeeUser.EmployeeId,
													Date = timeBlockDate.Date,
													TimeBlockDateId = timeBlockDate.TimeBlockDateId,
												});
											}

											TimeEngineManager tem = new TimeEngineManager(parameterObject, ActorCompanyId, UserId);
											var recalculatedResult = tem.RecalculateAccounting(items, SoeRecalculateAccountingMode.FromShiftType);
										}
									}
								}
							}
							catch (Exception ex)
							{
								base.LogError(ex, this.log);
							}
						}

						#endregion

						#region Event

						if (keys != null && employeeUser.IsNew && base.HasEventActivatedScheduledJob(entities, base.ActorCompanyId, TermGroup_ScheduleJobEventActivationType.EmployeeCreated))
						{
							var eventJobsSettings = ScheduledJobManager.GetScheduledJobSettingsWithEventActivaction(entities, base.ActorCompanyId).Where(w => w.IntData.HasValue && w.IntData == (int)TermGroup_ScheduleJobEventActivationType.EmployeeCreated);
							int employeeId = keys[(int)SaveEmployeeUserResult.EmployeeId];

							foreach (var eventSettings in eventJobsSettings)
							{
								try
								{
									ScheduledJobManager.RunBridgeJobFireAndForget(base.ActorCompanyId, 0, DateTime.Now, ScheduledJobManager.GetScheduledJobHead(entities, eventSettings.ScheduledJobHeadId, base.ActorCompanyId, loadRows: true, loadLogs: true, loadSettings: true, loadSettingOptions: true, false, false), null, eventInfo: employeeId.ToString());
								}
								catch
								{
								}
							}
						}

						#endregion

						if (employeeUser.UserId != 0)
						{
							User user = entities.User.Include("License").FirstOrDefault(f => f.UserId == employeeUser.UserId);
							if (user != null)
							{
								#region UserLinkConnectionKey

								if (!string.IsNullOrEmpty(employeeUser.UserLinkConnectionKey) && employeeUser.UserLinkConnectionKey.Length > 20)
								{
									EmployeeUserDTO currentUser = GetEmployeeUserDTOFromUser(base.UserId, base.ActorCompanyId, base.UserId, false);
									if (currentUser != null)
									{
										var userLinkConnectionKeyResult = SoftOneIdConnector.GetIdLoginGuidFromUserLinkConnectionKey(employeeUser.UserLinkConnectionKey, user.License.LicenseNr, currentUser.Email.ToLower());

										MessageEditDTO messageDto = new MessageEditDTO()
										{
											LicenseId = user.LicenseId,
											MessagePriority = TermGroup_MessagePriority.Normal,
											MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
											MessageTextType = TermGroup_MessageTextType.Text,
											MessageType = TermGroup_MessageType.AutomaticInformation,
											RoleId = base.RoleId.ToNullable(),
											MarkAsOutgoing = false,
											SenderName = currentUser.Name,
											SenderEmail = string.Empty,
											AnswerType = XEMailAnswerType.None,
											Entity = 0,
											RecordId = 0,
											ActorCompanyId = base.ActorCompanyId
										};

										if (userLinkConnectionKeyResult != null && !string.IsNullOrEmpty(userLinkConnectionKeyResult.Email) && userLinkConnectionKeyResult.IdLoginGuid != Guid.Empty)
										{
											var changedLoginText = GetText(11047, "Förändrad inloggning, tidigare inloggning är borttagen på användare ");
											var connectedToEmailText = GetText(11047, ". Nu kopplad till inloggning med epost ");

											messageDto.Recievers = new List<MessageRecipientDTO>() {
												   new MessageRecipientDTO()   { SendCopyAsEmail = true, EmailAddress = currentUser.Email.ToLower(), UserId = currentUser.UserId },
												   new MessageRecipientDTO()   { SendCopyAsEmail = true, EmailAddress = userLinkConnectionKeyResult.Email.ToLower(), UserId = user.UserId }};
											messageDto.Subject = changedLoginText + user.Name;
											messageDto.Text = changedLoginText + user.Name + connectedToEmailText + userLinkConnectionKeyResult.Email;
											messageDto.ShortText = messageDto.Text;

											TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, base.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.User, employeeUser.UserId, SoeEntityType.User, employeeUser.UserId, SettingDataType.String, "UserLinkConnectionKey", TermGroup_TrackChangesColumnType.User_UserLinkConnectionKey, "***********" + user.idLoginGuid.ToString().Right(5), "***********" + userLinkConnectionKeyResult.IdLoginGuid.ToString().Right(5)));
											user.idLoginGuid = userLinkConnectionKeyResult.IdLoginGuid;
											SaveChanges(entities);
										}
										else
										{
											messageDto.Recievers = new List<MessageRecipientDTO>() {
												   new MessageRecipientDTO()   { SendCopyAsEmail = true, EmailAddress = currentUser.Email.ToLower(), UserId = currentUser.UserId } };
											var connectionFailedText = GetText(11047, "Förändrad inloggning misslyckades för ");
											var connectedToEmailFailedText = GetText(11047, ". Nu kopplad till inloggning med epost ");
											messageDto.Subject = connectionFailedText + user.Name;
											messageDto.Text = connectionFailedText + user.Name + connectedToEmailFailedText + userLinkConnectionKeyResult.Email + (string.IsNullOrEmpty(userLinkConnectionKeyResult.UserName) ? "" : " E: " + userLinkConnectionKeyResult.UserName);
											messageDto.ShortText = messageDto.Text;
										}

										CommunicationManager.SendXEMail(messageDto, base.ActorCompanyId, base.RoleId, base.UserId);
									}
								}

								#endregion

								bool addToSoftOneIdDirectlyOnSave = !SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.DoNotAddToSoftOneIdDirectlyOnSave, 0, employeeUser.ActorCompanyId, 0);
								bool idLoginActive = user != null && user.idLoginActive;
								bool skipActivationEmailOnSSO = SettingManager.GetBoolSetting(entities, SettingMainType.License, (int)LicenseSettingType.SSO_SkipActivationEmailOnSSO, 0, 0, user?.LicenseId ?? 0);

								if (addToSoftOneIdDirectlyOnSave || idLoginActive || (employeeUser.ExternalAuthIdModified && skipActivationEmailOnSSO))
								{
									var addResult = UserManager.AddUserToSoftOneId(
										user: user,
										fireAndForget: !user.License.LicenseNr.StartsWith("100"), // Not sure about this one. Why would you want fire and forget when there is logic ahead that expects the user to exist? And why all numbers starting with 100 -- should it be equal?
										externalAuthId: (employeeUser.ExternalAuthIdModified ? employeeUser.ExternalAuthId : null));

									if (addResult != null && !string.IsNullOrEmpty(addResult.StringValue) && addResult.StringValue.Contains("Merge#"))
									{
										if (Guid.TryParse(addResult.StringValue.Split('#')[1], out Guid guid) && guid != Guid.Empty)
										{
											user.idLoginGuid = guid;
											SaveChanges(entities);
										}
									}

									#region ExternalAuth (SSO)

									if (employeeUser.ExternalAuthIdModified)
									{
										string sso_key = SettingManager.GetStringSetting(entities, SettingMainType.License, (int)LicenseSettingType.SSO_Key, 0, 0, user.LicenseId);
										if (!string.IsNullOrEmpty(sso_key) && Guid.TryParse(sso_key, out Guid idProviderGuid))
										{
											SoftOneIdConnector.AddExternalAuthId(user.idLoginGuid.Value, employeeUser.ExternalAuthId, idProviderGuid);
											string existing = SettingManager.GetStringSetting(entities, SettingMainType.User, (int)UserSettingType.ExternalAuthId, employeeUser.UserId, 0, 0);
											TermGroup_TrackChangesAction action = TermGroup_TrackChangesAction.Insert;

											if (!string.IsNullOrEmpty(employeeUser.ExternalAuthId) && existing != employeeUser.ExternalAuthId)
												action = TermGroup_TrackChangesAction.Update;

											if (existing != employeeUser.ExternalAuthId)
												TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, base.ActorCompanyId, actionMethod, action, SoeEntityType.User, employeeUser.UserId, SoeEntityType.User, employeeUser.UserId, SettingDataType.String, "ExternalAuthId", TermGroup_TrackChangesColumnType.User_ExternalAuthId, existing, employeeUser.ExternalAuthId));

											SettingManager.UpdateInsertStringSetting(entities, SettingMainType.User, (int)UserSettingType.ExternalAuthId, employeeUser.ExternalAuthId, employeeUser.UserId, 0, 0);
										}
										else if (SettingManager.SiteType == TermGroup_SysPageStatusSiteType.Test)
										{
											LogCollector.LogInfo($"ExternalAuth on userid {user.UserId} not modified {employeeUser.ExternalAuthId} because of missing SSO_KEY");
										}
									}
									else if (!employeeUser.ExternalAuthIdModified && !string.IsNullOrEmpty(employeeUser.ExternalAuthId) && SettingManager.SiteType == TermGroup_SysPageStatusSiteType.Test)
									{
										LogCollector.LogInfo($"ExternalAuth on userid {user.UserId} not modified {employeeUser.ExternalAuthId}");
									}

									#endregion

									#region LifetimeSeconds (Token)

									if (employeeUser.LifetimeSecondsModified && SettingManager.SettingIsTrue(SettingMainType.License, (int)LicenseSettingType.LifetimeSecondsEnabledOnUser, 0, 0, user.LicenseId, 60 * 10))
									{
										SoftOneIdConnector.AddLifetimeSeconds(user.idLoginGuid.Value, employeeUser.LifetimeSeconds);

										int existing = SettingManager.GetIntSetting(entities, SettingMainType.User, (int)UserSettingType.LifetimeSeconds, employeeUser.UserId, 0, 0);

										TermGroup_TrackChangesAction action = TermGroup_TrackChangesAction.Insert;

										if (!string.IsNullOrEmpty(employeeUser.ExternalAuthId) && existing != employeeUser.LifetimeSeconds)
											action = TermGroup_TrackChangesAction.Update;

										if (employeeUser.LifetimeSeconds != existing)
											TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, base.ActorCompanyId, actionMethod, action, SoeEntityType.User, employeeUser.UserId, SoeEntityType.User, employeeUser.UserId, SettingDataType.String, "LifetimeSeconds", TermGroup_TrackChangesColumnType.User_ExternalAuthId, existing.ToString(), employeeUser.LifetimeSeconds.ToString()));

										SettingManager.UpdateInsertIntSetting(entities, SettingMainType.User, (int)UserSettingType.LifetimeSeconds, employeeUser.LifetimeSeconds, employeeUser.UserId, 0, 0);
									}

									#endregion

									#region UserLinkConnectionKey

									if (!string.IsNullOrEmpty(employeeUser.UserLinkConnectionKey) && employeeUser.UserLinkConnectionKey.Length > 20)
									{
										var currentUser = GetEmployeeUserDTOFromUser(base.UserId, base.ActorCompanyId, base.UserId, false);

										if (currentUser != null)
										{
											var userLinkConnectionKeyResult = SoftOneIdConnector.GetIdLoginGuidFromUserLinkConnectionKey(employeeUser.UserLinkConnectionKey, user.License.LicenseNr, currentUser.Email.ToLower());

											MessageEditDTO messageDto = new MessageEditDTO()
											{
												LicenseId = user.LicenseId,
												MessagePriority = TermGroup_MessagePriority.Normal,
												MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
												MessageTextType = TermGroup_MessageTextType.Text,
												MessageType = TermGroup_MessageType.AutomaticInformation,
												RoleId = base.RoleId.ToNullable(),
												MarkAsOutgoing = false,
												SenderName = currentUser.Name,
												SenderEmail = string.Empty,
												AnswerType = XEMailAnswerType.None,
												Entity = 0,
												RecordId = 0,
												ActorCompanyId = base.ActorCompanyId
											};

											if (userLinkConnectionKeyResult != null && !string.IsNullOrEmpty(userLinkConnectionKeyResult.Email) && userLinkConnectionKeyResult.IdLoginGuid != Guid.Empty)
											{
												var changedLoginText = GetText(11047, "Förändrad inloggning, tidigare inloggning är borttagen på användare ");
												var connectedToEmailText = GetText(11047, ". Nu kopplad till inloggning med epost ");

												messageDto.Recievers = new List<MessageRecipientDTO>() {
												   new MessageRecipientDTO()   { SendCopyAsEmail = true, EmailAddress = currentUser.Email.ToLower(), UserId = currentUser.UserId },
												   new MessageRecipientDTO()   { SendCopyAsEmail = true, EmailAddress = userLinkConnectionKeyResult.Email.ToLower(), UserId = user.UserId }};
												messageDto.Subject = changedLoginText + user.Name;
												messageDto.Text = changedLoginText + user.Name + connectedToEmailText + userLinkConnectionKeyResult.Email;
												messageDto.ShortText = messageDto.Text;

												TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, base.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.User, employeeUser.UserId, SoeEntityType.User, employeeUser.UserId, SettingDataType.String, "UserLinkConnectionKey", TermGroup_TrackChangesColumnType.User_ExternalAuthId, "***********" + user.idLoginGuid.ToString().Right(5), "***********" + userLinkConnectionKeyResult.IdLoginGuid.ToString().Right(5)));
												user.idLoginGuid = userLinkConnectionKeyResult.IdLoginGuid;
												SaveChanges(entities);
											}
											else
											{
												messageDto.Recievers = new List<MessageRecipientDTO>() {
												   new MessageRecipientDTO()   { SendCopyAsEmail = true, EmailAddress = currentUser.Email.ToLower(), UserId = currentUser.UserId } };
												var connectionFailedText = GetText(11047, "Förändrad inloggning misslyckades för ");
												var connectedToEmailFailedText = GetText(11047, ". Nu kopplad till inloggning med epost ");
												messageDto.Subject = connectionFailedText + user.Name;
												messageDto.Text = connectionFailedText + user.Name + connectedToEmailFailedText + userLinkConnectionKeyResult.Email + (string.IsNullOrEmpty(userLinkConnectionKeyResult.UserName) ? "" : " E: " + userLinkConnectionKeyResult.UserName);
												messageDto.ShortText = messageDto.Text;
											}

											CommunicationManager.SendXEMail(messageDto, base.ActorCompanyId, base.RoleId, base.UserId);
										}
									}
									#endregion
								}
							}
						}
					}
					else
						base.LogTransactionFailed(this.ToString(), this.log);


					entities.Connection.Close();
				}
			}

			SessionCache.ReloadUser(employeeUser?.UserId ?? 0, base.ActorCompanyId);
			return result;
		}

		public ActionResult TryChangingGuid(string mergeLoginKey, string licenseNr, string email, Guid idLoginGuid)
		{
			LogCollector.LogInfo($"TryChangingGuid from guid {idLoginGuid} email {email} mergeLoginKey {mergeLoginKey}");
			ActionResult result = new ActionResult(false);
			try
			{
				if (!string.IsNullOrEmpty(mergeLoginKey) && mergeLoginKey.Length > 10)
				{
					LogCollector.LogInfo($"TryChangingGuid from guid {idLoginGuid} validation passed");
					var userLinkConnectionKeyResult = SoftOneIdConnector.GetIdLoginGuidFromUserMergeKey(mergeLoginKey, idLoginGuid, email.ToLower());
					LogCollector.LogInfo($"TryChangingGuid from guid {idLoginGuid} userLinkConnectionKeyResult to guid {userLinkConnectionKeyResult?.ConfidentialSecond} valided {idLoginGuid.Equals(Guid.Parse(userLinkConnectionKeyResult?.ConfidentialSecond))}");
					if (userLinkConnectionKeyResult != null && !string.IsNullOrEmpty(userLinkConnectionKeyResult.Email) && userLinkConnectionKeyResult.IdLoginGuid != Guid.Empty && idLoginGuid.Equals(Guid.Parse(userLinkConnectionKeyResult.ConfidentialSecond)))
					{
						LogCollector.LogInfo($"TryChangingGuid from guid {idLoginGuid} passed second validation");

						using (CompEntities entities = new CompEntities())
						{
							var user = entities.User.FirstOrDefault(f => f.idLoginGuid == idLoginGuid);
							if (user != null)
							{
								LogCollector.LogInfo($"TryChangingGuid old user found {user.UserId} new guid userLinkConnectionKeyResult.IdLoginGuid");
								user.idLoginGuid = userLinkConnectionKeyResult.IdLoginGuid;
								result = SaveChanges(entities);
								TrackChangesManager.AddTrackChanges(entities, null, TrackChangesManager.InitTrackChanges(entities, user.DefaultActorCompanyId ?? 0, TermGroup_TrackChangesActionMethod.User_Save, TermGroup_TrackChangesAction.Update, SoeEntityType.User, user.UserId, SoeEntityType.User, user.UserId, SettingDataType.String, "UserLinkConnectionKey", TermGroup_TrackChangesColumnType.User_ExternalAuthId, "***********" + user.idLoginGuid.ToString().Right(5), "***********" + userLinkConnectionKeyResult.IdLoginGuid.ToString().Right(5)));
								result = SaveChanges(entities);

							}
						}
					}
					else
					{
						result.ErrorMessage = $"TryChangingGuid second validation failed userLinkConnectionKeyResult";
					}
				}
				else
				{
					result.ErrorMessage = $"TryChangingGuid First validation failed !string.IsNullOrEmpty({mergeLoginKey}) && mergeLoginKey.Length > 10";
				}
			}
			catch (Exception ex)
			{
				LogCollector.LogError("TryChangingGuid failed " + ex.ToString());
			}

			return result;
		}

		public ActionResult SaveEmployeeUser(
			CompEntities entities,
			TransactionScope transaction,
			TermGroup_TrackChangesActionMethod actionMethod,
			EmployeeUserDTO employeeUser,
			EmployeeUserApplyFeaturesResult applyFeaturesResult,
			EmployeeUserChangesRepositoryDTO changesRepository = null,
			List<ContactAddressItem> contactAddresses = null,
			List<EmployeePositionDTO> employeePositions = null,
			List<EmployeeSkillDTO> employeeSkills = null,
			UserReplacementDTO userReplacement = null,
			EmployeeTaxSEDTO employeeTax = null,
			List<FileUploadDTO> files = null,
			List<UserRolesDTO> userRoles = null,
			bool saveRoles = false,
			bool saveAttestRoles = false,
			bool autoAddDefaultRole = false,
			bool checkEmployeeNrDuplicates = true,
			int? attestRoleId = null,
			bool generateCurrentChanges = false,
			bool doAcceptAttestedTemporaryEmployments = false,
			bool skipCategoryCheck = false,
			bool onlyValidateAttestRolesInCompany = false,
			List<ExtraFieldRecordDTO> extraFields = null
			)
		{
			/* This method affects TimeStamp so please test that it works to add a new employee from TimeStamp after changes has been made */

			ActionResult result = null;
			bool changeAffectTerminal = false;
			bool dontAllowIndenticalSSN = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.DontAllowIdenticalSSN, 0, ActorCompanyId, 0);

			#region Validate

			if (employeeUser.SaveEmployee)
			{
				bool editPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId, entities);
				if (!editPermission && employeeUser.UserId == base.UserId)
					editPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId, entities);
				if (!editPermission)
					return new ActionResult((int)ActionResultSave.InsufficienPermissionToSave, "No permission to save employee");
			}

			if (employeeUser.SaveUser)
			{
				bool editPermission = FeatureManager.HasRolePermission(Feature.Manage_Users_Edit, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId, entities);
				if (!editPermission && employeeUser.UserId == base.UserId)
					editPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId, entities);
				if (!editPermission)
					return new ActionResult((int)ActionResultSave.InsufficienPermissionToSave, "No permission to save user information");
			}

			if (employeeUser.SocialSec != "" && dontAllowIndenticalSSN)
			{
				ActionResult validateSSN = EmployeeManager.ValidateEmployeeSocialSecNumberNotExists(entities, employeeUser.SocialSec, ActorCompanyId, employeeUser.EmployeeId);
				if (!validateSSN.Success)
					return new ActionResult((int)ActionResultSave.EmployeeSocialSecNotAllowedAccordingToCompanySetting, GetText(10623, "Enligt företagsinställning tillåts ej identiska personnummer"));
			}

			#endregion

			#region Init

			if (employeeUser == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeUserDTO");
			if (applyFeaturesResult == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeUserApplyFeaturesResult");

			if (!employeeUser.SaveEmployee && !employeeUser.SaveUser)
				return new ActionResult((int)ActionResultSave.NothingSaved);
			if (!IsEmployeeUserDTOValid(employeeUser))
				return new ActionResult((int)ActionResultSave.EmployeeUserMandatoryFieldsMissing, GetText(11047, "Obligatorisk information saknas"));

			int selectedUserId = employeeUser.UserId;
			int selectedEmployeeId = employeeUser.EmployeeId;
			StringBuilder infoMessage = new StringBuilder();

			#endregion

			#region Prereq

			License license = LicenseManager.GetLicense(entities, employeeUser.LicenseId);
			if (license == null)
			{
				license = LicenseManager.GetLicenseByCompany(entities, employeeUser.ActorCompanyId);
				if (license == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11889, "Licensen hittades inte"));
			}

			Company company = CompanyManager.GetCompany(entities, employeeUser.ActorCompanyId);
			if (company == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

			User user = employeeUser.UserId != 0 ? UserManager.GetUserIgnoreState(entities, employeeUser.UserId, loadEmployee: true) : null;
			if (user != null && user.SysUser && (employeeUser.State == SoeEntityState.Deleted || employeeUser.State == SoeEntityState.Inactive))
				return new ActionResult((int)ActionResultSave.EmployeeUserCannotDeleteSysUser, GetText(11051, "Kan ej ta bort eller inaktivera sys-användaren"));

			Employee employee = employeeUser.EmployeeId != 0 ? EmployeeManager.GetEmployeeIgnoreState(entities, employeeUser.ActorCompanyId, employeeUser.EmployeeId,
				loadUser: true,
				loadEmployment: true,
				loadEmploymentAccounting: true,
				loadEmploymentPriceType: true,
				loadEmployeeFactors: true,
				loadEmployeeAccounts: true,
				loadEmployeeSettings: true
				) : null;

			ContactPerson contactPerson = employeeUser.ActorContactPersonId != 0 ? ContactManager.GetContactPersonIgnoreState(entities, employeeUser.ActorContactPersonId, true) : null;
			int originalActorContactPersonId = employeeUser.ActorContactPersonId;

			List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();
			Dictionary<int, EntityObject> mappingDict = new Dictionary<int, EntityObject>();

			if (!skipCategoryCheck)
			{
				bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, employeeUser.ActorCompanyId);
				if (useAccountHierarchy)
					skipCategoryCheck = true;
			}

			bool flushCompanyCategoryRecords = false;
			bool flushEmployeeAccounts = false;

			#endregion

			#region Validate Default Company

			int defaultCompanyId = 0;
			if (!userRoles.IsNullOrEmpty() && userRoles.Any(r => r.DefaultCompany))
				defaultCompanyId = userRoles.First(r => r.DefaultCompany).ActorCompanyId;
			else if (employeeUser.DefaultActorCompanyId.HasValue)
				defaultCompanyId = employeeUser.DefaultActorCompanyId.Value;

			if (defaultCompanyId == 0)
				defaultCompanyId = ActorCompanyId;

			bool changedCompany = user != null && user.DefaultActorCompanyId.HasValue && defaultCompanyId != 0 && user.DefaultActorCompanyId.Value != defaultCompanyId;
			int previousDefaultActorCompanyId = changedCompany ? user.DefaultActorCompanyId.Value : 0;

			#endregion

			#region Validate Default Role

			// Check if default role has been selected
			int defaultRoleId = 0;
			UserCompanyRoleDTO defaultRole = null;

			bool isApiUpdateWithoutRoles = actionMethod == TermGroup_TrackChangesActionMethod.Employee_Import && !saveRoles && user != null && !autoAddDefaultRole;
			if (!isApiUpdateWithoutRoles && FeatureManager.HasRolePermission(Feature.Manage_Users_Edit_UserMapping, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId, entities))
			{
				if (!userRoles.IsNullOrEmpty())
				{
					// First try to find a default role in default company
					defaultRole = UserManager.GetSelectedDefaultRole(userRoles.Where(r => r.ActorCompanyId == defaultCompanyId).ToList());
					if (defaultRole == null)
						defaultRole = UserManager.GetSelectedDefaultRole(userRoles.Where(r => r.ActorCompanyId != defaultCompanyId).ToList());
					if (defaultRole != null)
						defaultRoleId = defaultRole.RoleId;
				}

				if (defaultRoleId == 0)
				{
					// Get default role from company setting
					defaultRoleId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultRole, 0, ActorCompanyId, 0);
					if (defaultRoleId != 0)
						autoAddDefaultRole = true;
					else
						return new ActionResult((int)ActionResultSave.EmployeeUserDefaultRoleMandatory, GetText(12041, "Standardroll måste väljas"));
				}
			}

			#endregion

			#region ContactPerson

			//Create new ContactPerson if disconnected or changed. Disconnected/Changed User/Employee keeps original ContactPerson
			bool disconnected = employeeUser.DisconnectExistingUser || employeeUser.DisconnectExistingEmployee;
			bool changedUser = employee != null && employee.UserId != employeeUser.UserId.ToNullable();
			bool changedEmployee = employeeUser.SaveEmployee && user != null && user.Employee != null && user.Employee.Count == 1 && user.Employee.First().EmployeeId != employeeUser.EmployeeId;
			if (disconnected || changedUser || changedEmployee)
				employeeUser.ActorContactPersonId = 0;
			int actorContactPersonId = employeeUser.ActorContactPersonId;

			if (employeeUser.ActorContactPersonId == 0)
			{
				#region Add

				// Actor
				Actor actor = new Actor()
				{
					ActorType = (int)SoeActorType.ContactPerson,
				};
				SetCreatedProperties(actor);

				contactPerson = new ContactPerson()
				{
					//References
					Actor = actor,
					Position = 0 // Deprecated, use EmployeePosition instead
				};
				SetCreatedProperties(contactPerson);
				entities.ContactPerson.AddObject(contactPerson);

				#endregion
			}
			else
			{
				#region Update

				if (contactPerson == null)
					return new ActionResult((int)ActionResultSave.EmployeeUserAttestContactPersonNotFound, GetText(11046, "Kontaktperson hittades inte"));

				SetModifiedProperties(contactPerson);

				#endregion
			}

			//Fields
			bool nameChanged = (contactPerson.FirstName != employeeUser.FirstName || contactPerson.LastName != employeeUser.LastName);
			if (nameChanged)
			{
				contactPerson.FirstName = employeeUser.FirstName;
				contactPerson.LastName = employeeUser.LastName;

				// If saving from employee, SaveUser is false, but we still need to update the name field on the user
				if (user != null && !employeeUser.SaveUser)
					user.Name = StringUtility.GetName(employeeUser.FirstName, employeeUser.LastName, Constants.APPLICATION_NAMESTANDARD);
			}

			if (!applyFeaturesResult.HasBlankedSocialSec && employeeUser.SaveEmployee)
			{
				contactPerson.SocialSec = employeeUser.SocialSec;
				contactPerson.Sex = (int)CalendarUtility.GetSexFromSocialSecNr(contactPerson.SocialSec);
			}
			contactPerson.State = (int)employeeUser.State;

			#region Portrait consent

			ActorConsent consent = contactPerson.Actor.ActorConsent.FirstOrDefault(a => a.ConsentType == (int)ActorConsentType.EmployeePortrait);
			if (consent == null)
			{
				consent = new ActorConsent();
				consent.ConsentType = (int)ActorConsentType.EmployeePortrait;
				contactPerson.Actor.ActorConsent.Add(consent);
			}

			if ((consent.HasConsent != employeeUser.PortraitConsent) || (consent.ConsentDate != employeeUser.PortraitConsentDate))
			{
				consent.HasConsent = employeeUser.PortraitConsent;
				consent.ConsentDate = consent.HasConsent ? employeeUser.PortraitConsentDate : null;
				consent.ConsentModified = DateTime.Now;
				consent.ConsentModifiedBy = GetUserDetails();
			}

			#endregion

			if (changesRepository != null)
				changesRepository.SetAfterValue(contactPerson);

			#endregion

			#region User

			if (employeeUser.DisconnectExistingUser)
			{
				if (user == null && employee.UserId.HasValue)
					user = UserManager.GetUserIgnoreState(entities, employee.UserId.Value, loadEmployee: true);

				if (user != null)
					user.ContactPerson = contactPerson;
			}

			if (employeeUser.SaveUser && !employeeUser.Vacant && !employeeUser.DisconnectExistingUser)
			{
				if (string.IsNullOrEmpty(employeeUser.LoginName))
					LogCollector.LogWithTrace($"string.IsNullOrEmpty(employeeUser.LoginName) should never happen when employee.SaveUser is true base.Actorcompanyid{base.ActorCompanyId} employeeUser.ActorCompanyId {employeeUser.ActorCompanyId} employeeNr = {employeeUser.EmployeeNr}", LogLevel.Error);

				if (employeeUser.UserId == 0)
				{
					#region Add

					result = LicenseManager.ValidateUser(entities, license, employeeUser, true, employeeUser.State == SoeEntityState.Active ? (int)SoeEntityStateTransition.InactiveToActive : 0);
					if (!result.Success)
						return result;

					user = new User
					{
						License = license
					};
					entities.User.AddObject(user);
					SetCreatedProperties(user);

					#region Set startpage

					if (defaultRoleId > 0)
					{
						FavoriteItem favoriteItem = SettingManager.GetFavoriteItemOptionFromRole(RoleManager.GetRole(entities, defaultRoleId));
						if (favoriteItem != null)
						{
							UserFavorite userFavorite = new UserFavorite()
							{
								Name = favoriteItem.FavoriteName,
								Url = favoriteItem.FavoriteUrl,
								IsDefault = true,

								//References
								Company = company,
								User = user,
							};
							entities.AddToUserFavorite(userFavorite);
						}
					}

					#endregion

					#endregion
				}
				else
				{
					#region Update

					if (user == null)
						return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10112, "Användare hittades inte"));

					SoeEntityStateTransition userStateTransition = GetStateTransition(user, employeeUser);

					result = LicenseManager.ValidateUser(entities, license, employeeUser, true, (int)userStateTransition);
					if (!result.Success)
						return result;

					SetModifiedProperties(user);

					#endregion
				}

				#region User fields

				if (!isApiUpdateWithoutRoles)
					user.DefaultRoleId = defaultRoleId; //Remove when field in databse is removed or made nullable
				user.DefaultActorCompanyId = defaultCompanyId;
				user.LangId = employeeUser.LangId ?? (int)TermGroup_Languages.Swedish;
				user.EstatusLoginId = employeeUser.EstatusLoginId;
				user.Name = StringUtility.GetName(employeeUser.FirstName, employeeUser.LastName, Constants.APPLICATION_NAMESTANDARD);
				string loginName = (employeeUser?.LoginName ?? user.LoginName ?? employeeUser?.EmployeeNr ?? user.Name ?? user.idLoginGuid?.ToString().Substring(1, 11) ?? string.Empty).Replace(" ", "");
				user.LoginName = loginName.Length > 50 ? loginName.Substring(0, 50) : loginName;
				user.ChangePassword = employeeUser.ChangePassword;
				user.PasswordHomePage = employeeUser.PasswordHomePage;
				user.EmailCopy = employeeUser.EmailCopy;
				user.IsMobileUser = true;
				user.BlockedFromDate = employeeUser.BlockedFromDate;
				user.State = (int)employeeUser.State;

				if (!user.idLoginGuid.HasValue)
					user.idLoginGuid = Guid.NewGuid();

				//References
				user.ContactPerson = contactPerson;

				#endregion

				#region AttestRoleUser

				if (attestRoleId.HasValue && attestRoleId > 0)
				{
					AttestRoleUser attestRoleUser = new AttestRoleUser()
					{
						User = user,
						AttestRoleId = attestRoleId.Value,
						MaxAmount = 0,
					};

					user.AttestRoleUser.Add(attestRoleUser);
					SetCreatedProperties(attestRoleUser);
				}

				#endregion

				#region UserCompanyRole

				if (autoAddDefaultRole)
				{
					bool addUserCompanyRole = false;
					if (user.UserId == 0)
					{
						//Add UserCompanyRole if new User
						addUserCompanyRole = true;
					}
					else if (user != null && defaultCompanyId != 0 && defaultRoleId != 0 && !UserManager.ExistUserCompanyRoleMapping(entities, user.UserId, defaultCompanyId, defaultRoleId))
					{
						//Add UserCompanyRole if updating User and UserCompanyRole for DefaultRole not exists
						addUserCompanyRole = true;
					}

					if (addUserCompanyRole)
					{
						var userCompanyRole = new UserCompanyRole()
						{
							//References
							ActorCompanyId = defaultCompanyId,
							RoleId = defaultRoleId,
							Default = true
						};
						SetCreatedProperties(userCompanyRole);
						user.UserCompanyRole.Add(userCompanyRole);
					}
				}

				#endregion

				#region Set AfterValues

				if (changesRepository != null)
					changesRepository.SetAfterValue(user);

				#endregion

				#region ECom

				//Set Email from ECom
				if (contactAddresses != null)
				{
					var contactItem = ContactManager.GetContactAddressItem(contactAddresses.ToList(), false, TermGroup_SysContactEComType.Email, true);
					if (contactItem != null)
					{
						user.Email = contactItem.EComText.Trim();
					}
					else
					{
						contactItem = ContactManager.GetContactAddressItem(contactAddresses.ToList(), false, TermGroup_SysContactEComType.Email);
						if (contactItem != null)
							user.Email = contactItem.EComText.Trim();
						else
							user.Email = String.Empty;
					}
				}

				#endregion

				#region Connect/Disconnect Employee

				if (employeeUser.DisconnectExistingEmployee)
				{
					user.Employee.Clear();

					foreach (ContactAddressItem addr in contactAddresses)
					{
						// Will copy addresses to new contact person
						addr.ContactAddressId = 0;
						addr.ContactEComId = 0;
					}
				}
				else
				{
					if (employee != null && user.Employee != null)
					{
						bool sameEmployee = user.Employee.Count == 1 && user.Employee.First().EmployeeId == employee.EmployeeId;
						if (!sameEmployee)
						{
							foreach (Employee connectedEmployee in user.Employee.ToList())
							{
								if (connectedEmployee.ActorCompanyId == company.ActorCompanyId && connectedEmployee.EmployeeId != employee.EmployeeId)
									user.Employee.Remove(connectedEmployee);
							}

							user.Employee.Add(employee);
						}
					}
				}

				#endregion
			}

			#endregion

			#region Employee

			if (employeeUser.SaveEmployee)
			{
				if (!skipCategoryCheck && employeeUser.CategoryRecords.IsNullOrEmpty())
					return new ActionResult((int)ActionResultSave.EmployeeCategoriesMandatory, GetText(11041, "Du måste välja minst en kategori"));

				if (employeeUser.EmployeeId > 0 && employeeUser.State == SoeEntityState.Inactive && TimeScheduleManager.HasEmployeeAnySchedulePlacements(entities, employeeUser.EmployeeId))
					return new ActionResult((int)ActionResultSave.EmployeeUserCannotInactivateWhenScheduledPlacementExists, GetText(11042, "Anställd kan inte inaktiveras eftersom det finns ett aktiverat schema"));

				if (employeeUser.EmployeeId == 0)
				{
					#region Add

					result = LicenseManager.ValidateEmployee(entities, license, employeeUser, true);
					if (!result.Success)
						return result;

					Int32.TryParse(employeeUser.EmployeeNr, out int seqNbr);
					if (seqNbr != 0)
						SequenceNumberManager.UpdateSequenceNumber(entities, ActorCompanyId, "Employee", seqNbr);

					employee = new Employee();

					if (employeeUser.EmployeeTemplateId.HasValue)
						employee.EmployeeTemplateId = employeeUser.EmployeeTemplateId;

					entities.Employee.AddObject(employee);
					SetCreatedProperties(employee);

					#endregion
				}
				else
				{
					#region Update

					if (employee == null)
						return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

					if (checkEmployeeNrDuplicates && employee.EmployeeNr != employeeUser.EmployeeNr && EmployeeManager.EmployeeExists(entities, employeeUser.EmployeeNr, employeeUser.ActorCompanyId))
						return new ActionResult((int)ActionResultSave.EmployeeNumberExists, String.Format(GetText(5882, "Anställningsnumret '{0}' är upptaget"), employeeUser.EmployeeNr));

					SetModifiedProperties(employee);

					#endregion
				}

				#region Employee fields

				employee.Vacant = employeeUser.Vacant;
				employee.EmployeeNr = employeeUser.EmployeeNr;
				employee.EmploymentDate = employeeUser.EmploymentDate;
				employee.EndDate = employeeUser.EndDate;

				if (!applyFeaturesResult.HasBlankedCardNumber)
				{
					employee.CardNumber = StringUtility.EmptyToNull(employeeUser.CardNumber);
				}
				if (!applyFeaturesResult.HasBlankedDisbursement)
				{
					employee.DisbursementMethod = (int)employeeUser.DisbursementMethod;
					employee.DisbursementClearingNr = employeeUser.DisbursementClearingNr;
					employee.DisbursementAccountNr = employeeUser.DisbursementAccountNr;
					employee.DontValidateDisbursementAccountNr = employeeUser.DontValidateDisbursementAccountNr;
					employee.DisbursementCountryCode = employeeUser.DisbursementCountryCode;
					employee.DisbursementBIC = employeeUser.DisbursementBIC;
					employee.DisbursementIBAN = employeeUser.DisbursementIBAN;

				}
				if (!applyFeaturesResult.HasBlankedNote)
				{
					employee.Note = employeeUser.Note;
					employee.ShowNote = employeeUser.ShowNote;
				}
				if (!applyFeaturesResult.HasBlankedHighRiskProtection)
				{
					employee.HighRiskProtection = employeeUser.HighRiskProtection;
					employee.HighRiskProtectionTo = employeeUser.HighRiskProtectionTo;
				}
				if (!applyFeaturesResult.HasBlankedMedicalCertificateReminder)
				{
					employee.MedicalCertificateReminder = employeeUser.MedicalCertificateReminder;
					employee.MedicalCertificateDays = employeeUser.MedicalCertificateDays;
				}

				if (employeeUser.ParentEmployeeNr != null && employeeUser.EmployeeNr != employeeUser.ParentEmployeeNr)
				{
					Employee parentEmployee = EmployeeManager.GetEmployeeByNr(entities, employeeUser.ParentEmployeeNr, employeeUser.ActorCompanyId);
					employee.ParentId = parentEmployee != null ? parentEmployee.EmployeeId : (int?)null;
				}
				else if (employeeUser.ParentEmployeeNr == string.Empty)
				{
					employee.ParentId = null;
				}

				employee.Absence105DaysExcluded = employeeUser.Absence105DaysExcluded;
				employee.Absence105DaysExcludedDays = employeeUser.Absence105DaysExcludedDays;
				employee.ExternalCode = employeeUser.ExternalCode;
				employee.UseFlexForce = employeeUser.UseFlexForce;
				employee.WantsExtraShifts = employeeUser.WantsExtraShifts;
				employee.DontNotifyChangeOfDeviations = employeeUser.DontNotifyChangeOfDeviations;
				employee.DontNotifyChangeOfAttestState = employeeUser.DontNotifyChangeOfAttestState;
				employee.ExcludeFromPayroll = employeeUser.ExcludeFromPayroll;
				employee.PayrollStatisticsPersonalCategory = employeeUser.PayrollReportsPersonalCategory;
				employee.PayrollStatisticsWorkTimeCategory = employeeUser.PayrollReportsWorkTimeCategory;
				employee.PayrollStatisticsSalaryType = employeeUser.PayrollReportsSalaryType;
				employee.PayrollStatisticsWorkPlaceNumber = employeeUser.PayrollReportsWorkPlaceNumber;
				employee.PayrollStatisticsCFARNumber = employeeUser.PayrollReportsCFARNumber;
				employee.WorkPlaceSCB = employeeUser.WorkPlaceSCB;
				employee.PartnerInCloseCompany = employeeUser.PartnerInCloseCompany;
				employee.BenefitAsPension = employeeUser.BenefitAsPension;
				employee.AFACategory = employeeUser.AFACategory;
				employee.AFASpecialAgreement = employeeUser.AFASpecialAgreement;
				employee.AFAWorkplaceNr = employeeUser.AFAWorkplaceNr;
				employee.AFAParttimePensionCode = employeeUser.AFAParttimePensionCode;
				employee.CollectumITPPlan = employeeUser.CollectumITPPlan;
				employee.CollectumAgreedOnProduct = employeeUser.CollectumAgreedOnProduct;
				employee.CollectumCostPlace = employeeUser.CollectumCostPlace;
				employee.CollectumCancellationDate = employeeUser.CollectumCancellationDate;
				employee.CollectumCancellationDateIsLeaveOfAbsence = employeeUser.CollectumCancellationDateIsLeaveOfAbsence;
				employee.KPARetirementAge = employeeUser.KpaRetirementAge;
				employee.KPABelonging = employeeUser.KpaBelonging;
				employee.KPAEndCode = employeeUser.KpaEndCode;
				employee.BygglosenAgreementArea = employeeUser.BygglosenAgreementArea;
				employee.BygglosenAllocationNumber = employeeUser.BygglosenAllocationNumber;
				employee.BygglosenMunicipalCode = employeeUser.BygglosenMunicipalCode;
				employee.BygglosenSalaryFormula = employeeUser.BygglosenSalaryFormula;
				employee.BygglosenProfessionCategory = employeeUser.BygglosenProfessionCategory;
				employee.BygglosenWorkPlaceNumber = employeeUser.BygglosenWorkPlaceNumber;
				employee.BygglosenLendedToOrgNr = employeeUser.BygglosenLendedToOrgNr;
				employee.BygglosenAgreedHourlyPayLevel = employeeUser.BygglosenAgreedHourlyPayLevel;
				employee.BygglosenSalaryType = employeeUser.BygglosenSalaryType;
				employee.KPAAgreementType = employeeUser.KpaAgreementType;
				employee.GTPAgreementNumber = employeeUser.GtpAgreementNumber;
				employee.GTPExcluded = employeeUser.GtpExcluded;
				employee.IFAssociationNumber = employeeUser.IFAssociationNumber;
				employee.IFPaymentCode = employeeUser.IFPaymentCode;
				employee.IFWorkPlace = employeeUser.IFWorkPlace;
				employee.AGIPlaceOfEmploymentAddress = employeeUser.AGIPlaceOfEmploymentAddress;
				employee.AGIPlaceOfEmploymentCity = employeeUser.AGIPlaceOfEmploymentCity;
				employee.AGIPlaceOfEmploymentIgnore = employeeUser.AGIPlaceOfEmploymentIgnore;
				employee.State = (int)employeeUser.State;
				employee.AGIPlaceOfEmploymentIgnore = employeeUser.AGIPlaceOfEmploymentIgnore;
				employee.AGIPlaceOfEmploymentAddress = employeeUser.AGIPlaceOfEmploymentAddress;
				employee.AGIPlaceOfEmploymentCity = employeeUser.AGIPlaceOfEmploymentCity;

				//Set FK
				employee.ActorCompanyId = employeeUser.ActorCompanyId;
				employee.TimeCodeId = employeeUser.TimeCodeId.HasValue && employeeUser.TimeCodeId.Value > 0 ? employeeUser.TimeCodeId.Value : (int?)null;
				employee.TimeDeviationCauseId = employeeUser.TimeDeviationCauseId.HasValue && employeeUser.TimeDeviationCauseId.Value > 0 ? employeeUser.TimeDeviationCauseId.Value : (int?)null;

				//Set references
				employee.ContactPerson = contactPerson;

				if (changesRepository != null)
					changesRepository.SetAfterValue(employee);

				#endregion

				#region Connect/Disconnect User

				if (originalActorContactPersonId != 0 && employee.ContactPersonId == 0 && !contactAddresses.IsNullOrEmpty())
				{
					foreach (ContactAddressItem addr in contactAddresses)
					{
						// Will copy addresses to new contact person
						addr.ContactAddressId = 0;
						addr.ContactEComId = 0;
					}
				}

				if (employeeUser.DisconnectExistingUser)
				{
					employee.User = null;
					if (originalActorContactPersonId != 0 && employee.ContactPersonId == 0)
						employee.ContactPerson = ContactManager.GetContactPersonIgnoreState(entities, originalActorContactPersonId, true);
				}
				else
				{
					if (user != null)
					{
						bool sameEmployee = employee.User != null && employee.UserId == user.UserId;
						if (!sameEmployee)
						{
							employee.User = user;
						}
					}
				}

				#endregion

				#region Employment

				if (employee != null && employeeUser.Employments != null)
				{
					result = EmployeeManager.SaveEmployments(entities, transaction, employee, employeeUser.Employments, generateCurrentChanges: generateCurrentChanges, doAcceptAttestedTemporaryEmployments: doAcceptAttestedTemporaryEmployments);
					if (!result.Success)
						return result;

					int tempIdCounter = 0;
					Employment prevEmployment = null;
					foreach (Employment employment in employee.Employment.GetActiveOrHidden())
					{
						EmploymentDTO employmentInput = employeeUser.Employments.FirstOrDefault(e => e.UniqueId == employment.UniqueId);
						if (employmentInput == null)
							continue;

						#region EmploymentPriceType

						bool doTrySavePricesTypes = !applyFeaturesResult.HasBlankedPriceTypes;

						if (employeeUser.SavingFromApi && employmentInput.PriceTypes.IsNullOrEmpty())
							doTrySavePricesTypes = false;

						if (applyFeaturesResult.HasBlankedPriceTypes && employmentInput.EmploymentId == 0 && employmentInput.IsNewFromCopy)
						{
							if (EmployeeManager.IsEmploymentValidToKeepPriceTypes(prevEmployment, employmentInput))
							{
								employmentInput.PriceTypes = prevEmployment.EmploymentPriceType?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs(true, true).ToList();
								doTrySavePricesTypes = true;
							}
							else
							{
								infoMessage.Append(GetText(12126, "Anställningen är sparad, men behörighet till löneuppgifter saknas. \n\n Eftersom tidavtal, löneavtal eller semesteravtal skiljer sig från tidigare anställning kommer inte löneuppgifterna att kopieras över till den nya anställningen. \n\n Observera att löneuppgifter behöver kompletteras till den nya anställningen."));
							}
						}

						if (doTrySavePricesTypes && employmentInput.PriceTypes != null)
						{
							if (employmentInput.PriceTypes.Where(p => !p.Periods.IsNullOrEmpty()).GroupBy(p => p.PayrollPriceTypeId).Any(p => p.Count() > 1))
								return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91941, "En lönetyp får bara förekomma en gång per anställning"));

							if (!employment.EmploymentPriceType.IsLoaded && CanEntityLoadReferences(entities, employment))
							{
								employment.EmploymentPriceType.Load();
								foreach (EmploymentPriceType priceType in employment.EmploymentPriceType)
								{
									if (!priceType.EmploymentPriceTypePeriod.IsLoaded)
										priceType.EmploymentPriceTypePeriod.Load();
								}
							}

							foreach (EmploymentPriceType priceType in employment.EmploymentPriceType.ToList())
							{
								EmploymentPriceTypeDTO priceTypeInput = employmentInput.PriceTypes.FirstOrDefault(p => p.EmploymentPriceTypeId == priceType.EmploymentPriceTypeId);
								if (priceTypeInput != null)
								{
									bool hasDeletedPeriods = false;
									foreach (EmploymentPriceTypePeriod period in priceType.EmploymentPriceTypePeriod.Where(p => p.State == (int)SoeEntityState.Active).ToList())
									{
										EmploymentPriceTypePeriodDTO periodInput = priceTypeInput.Periods.FirstOrDefault(p => p.EmploymentPriceTypePeriodId == period.EmploymentPriceTypePeriodId);
										if (periodInput != null)
										{
											if (!periodInput.Hidden)
												EmployeeManager.UpdateEmploymentPriceTypePeriod(entities, employee, priceType, period, periodInput, trackChangesItems, actionMethod);
											priceTypeInput.Periods.Remove(periodInput);
										}
										else
										{
											EmployeeManager.DeleteEmploymentPriceTypePeriod(entities, employee, priceType, period, trackChangesItems, actionMethod);
											hasDeletedPeriods = true;
										}
									}

									if (hasDeletedPeriods)
										EmployeeManager.DeleteEmploymentPriceTypeIfAllPeriodsAreDeleted(entities, employee, employment, priceType, priceTypeInput, trackChangesItems);

									foreach (EmploymentPriceTypePeriodDTO periodInput in priceTypeInput.Periods)
									{
										tempIdCounter++;
										EmploymentPriceTypePeriod period = EmployeeManager.CreateEmploymentPriceTypePeriod(entities, employee, priceType, periodInput, trackChangesItems, actionMethod, tempIdCounter, priceType.EmploymentPriceTypeId);
										if (period != null)
											mappingDict.Add(tempIdCounter, period);
									}
								}
								else
								{
									foreach (EmploymentPriceTypePeriod period in priceType.EmploymentPriceTypePeriod.ToList())
									{
										if (period.State != (int)SoeEntityState.Deleted)
											EmployeeManager.DeleteEmploymentPriceTypePeriod(entities, employee, priceType, period, trackChangesItems, actionMethod);
									}
									if (priceType.State != (int)SoeEntityState.Deleted)
										EmployeeManager.DeleteEmploymentPriceType(entities, employee, employment, priceType, trackChangesItems, actionMethod);
								}
								employmentInput.PriceTypes.Remove(priceTypeInput);
							}

							foreach (EmploymentPriceTypeDTO priceTypeInput in employmentInput.PriceTypes)
							{
								tempIdCounter++;
								EmploymentPriceType priceType = EmployeeManager.CreateEmploymentPriceType(entities, employee, employment, priceTypeInput, trackChangesItems, actionMethod, tempIdCounter);
								if (priceType != null)
									mappingDict.Add(tempIdCounter, priceType);

								if (!priceTypeInput.Periods.IsNullOrEmpty())
								{
									foreach (EmploymentPriceTypePeriodDTO periodInput in priceTypeInput.Periods)
									{
										tempIdCounter++;
										EmploymentPriceTypePeriod period = EmployeeManager.CreateEmploymentPriceTypePeriod(entities, employee, priceType, periodInput, trackChangesItems, actionMethod, tempIdCounter);
										if (period != null)
											mappingDict.Add(tempIdCounter, period);
									}
								}
							}
						}

						#endregion

						#region AccountSettings

						if (!applyFeaturesResult.HasBlankedAccounts && !employmentInput.AccountingSettings.IsNullOrEmpty())
						{
							List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(entities, ActorCompanyId, onlyInternal: true);

							if (employment.EmploymentAccountStd.IsNullOrEmpty())
							{
								#region Add

								if (employment.EmploymentAccountStd == null)
									employment.EmploymentAccountStd = new EntityCollection<EmploymentAccountStd>();

								foreach (AccountingSettingsRowDTO settingInput in employmentInput.AccountingSettings)
								{
									if (employmentInput.FixedAccounting && settingInput.Percent == 0)
										continue;

									// Standard account
									EmploymentAccountStd employmentAccountStd = new EmploymentAccountStd
									{
										Type = settingInput.Type,
										AccountId = settingInput.Account1Id.ToNullable(),
										Percent = settingInput.Percent
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

										if (accountId != 0)
										{
											AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, ActorCompanyId);
											if (accountInternal != null)
												employmentAccountStd.AccountInternal.Add(accountInternal);
										}
									}

									employment.EmploymentAccountStd.Add(employmentAccountStd);
								}

								#endregion
							}
							else
							{
								#region Update/Delete

								// Loop over existing settings
								foreach (EmploymentAccountStd employmentAccountStd in employment.EmploymentAccountStd.ToList())
								{
									// Find setting in input
									AccountingSettingsRowDTO settingInput = employmentInput.AccountingSettings.FirstOrDefault(a => a.Type == employmentAccountStd.Type);
									if (settingInput != null)
									{
										// Update account
										employmentAccountStd.AccountId = settingInput.Account1Id.ToNullable();
										employmentAccountStd.Percent = settingInput.Percent;

										// Remove existing internal accounts
										// No way to update them
										employmentAccountStd.AccountInternal.Clear();

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

											if (accountId != 0)
											{
												// Add account internal
												AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, ActorCompanyId);
												if (accountInternal != null)
													employmentAccountStd.AccountInternal.Add(accountInternal);
											}
										}
									}
									// Remove from input to prevent adding below
									employmentInput.AccountingSettings.Remove(settingInput);
								}

								#endregion

								#region Add AccountingSettings

								if (!employmentInput.AccountingSettings.IsNullOrEmpty())
								{
									foreach (AccountingSettingsRowDTO settingInput in employmentInput.AccountingSettings)
									{
										if (employmentInput.FixedAccounting && settingInput.Percent == 0)
											continue;

										EmploymentAccountStd employmentAccountStd = new EmploymentAccountStd
										{
											Type = settingInput.Type,
											AccountId = settingInput.Account1Id.ToNullable(),
											Percent = settingInput.Percent
										};
										employment.EmploymentAccountStd.Add(employmentAccountStd);

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

											if (accountId != 0)
											{
												// Add account internal
												AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, ActorCompanyId);
												if (accountInternal != null)
													employmentAccountStd.AccountInternal.Add(accountInternal);
											}
										}
									}
								}

								#endregion
							}
						}

						#endregion

						prevEmployment = employment;
					}
				}

				#endregion

				#region EmployeeCalculatedCosts

				if (employee != null && employeeUser.CalculatedCosts != null && employeeUser.CalculatedCosts.Any(c => c.IsDeleted || c.IsModified))
				{
					result = EmployeeManager.SaveEmployeeCalculatedCosts(entities, transaction, employee, employeeUser.CalculatedCosts);
					if (!result.Success)
						return result;
				}

				#endregion

				#region Tax

				if (employeeTax != null)
				{
					if (employeeTax.EmployeeTaxId == 0 && employeeTax.Type == TermGroup_EmployeeTaxType.NotSelected)
					{
						// Do not save new employee tax with no type selected
					}
					else
					{
						if (employeeTax.EmployeeId == 0)
							employeeTax.EmployeeId = employee.EmployeeId;
						result = EmployeeManager.SaveEmployeeTaxSE(entities, transaction, employeeTax, ActorCompanyId, actionMethod, changesRepository);
						if (!result.Success)
							return result;
					}
				}
				else if (employeeUser.IsNew && SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UsePayroll, 0, base.ActorCompanyId, 0))
				{
					EmployeeManager.CsrInquiry(entities, transaction, ActorCompanyId, employee.EmployeeId, employeeUser.Employments?.FirstOrDefault()?.DateFrom?.Year ?? DateTime.Today.Year);
				}

				#endregion

				#region Factors

				bool doSaveFactors = employeeUser.Factors != null;
				if (employeeUser.SavingFromApi && employeeUser.Factors.IsNullOrEmpty())
					doSaveFactors = false;

				if (doSaveFactors)
				{
					#region Update/Delete

					foreach (EmployeeFactor factor in employee.EmployeeFactor.Where(p => p.State == (int)SoeEntityState.Active).ToList())
					{
						EmployeeFactorDTO factorInput = employeeUser.Factors.FirstOrDefault(i => i.EmployeeFactorId == factor.EmployeeFactorId);
						if (factorInput != null)
						{
							#region Update

							if (factor.Type != (int)factorInput.Type || factor.VacationGroupId != factorInput.VacationGroupId || factor.FromDate != factorInput.FromDate || factor.Factor != factorInput.Factor)
							{
								factor.Type = (int)factorInput.Type;
								factor.VacationGroupId = factorInput.VacationGroupId;
								factor.FromDate = factorInput.FromDate;
								factor.Factor = factorInput.Factor;
								SetModifiedProperties(factor);
							}

							#endregion
						}
						else
						{
							#region Delete

							ChangeEntityState(factor, SoeEntityState.Deleted);

							#endregion
						}

						// Remove from input to prevent adding it again below
						employeeUser.Factors.Remove(factorInput);
					}

					#endregion

					#region Add

					// Add all periods that is left in the input
					foreach (EmployeeFactorDTO factorInput in employeeUser.Factors)
					{
						EmployeeFactor factor = new EmployeeFactor()
						{
							Type = (int)factorInput.Type,
							VacationGroupId = factorInput.VacationGroupId,
							FromDate = factorInput.FromDate,
							Factor = factorInput.Factor
						};
						SetCreatedProperties(factor);
						employee.EmployeeFactor.Add(factor);
					}

					#endregion
				}

				#endregion

				#region Vacation

				if (!applyFeaturesResult.HasBlankedEmployeeVacationSE && employeeUser.EmployeeVacationSE != null)
				{
					// Only update if changed
					// Always create a new record (setting state to deleted on existing)
					int prevVacationSEId = 0;

					List<EmployeeVacationSE> vacations = EmployeeManager.GetEmployeeVacationSEs(entities, employeeUser.EmployeeId);
					foreach (EmployeeVacationSE vacation in vacations)
					{
						ChangeEntityState(vacation, SoeEntityState.Deleted);
						prevVacationSEId = vacation.EmployeeVacationSEId;
					}

					EmployeeVacationSE newVacation = new EmployeeVacationSE()
					{
						Employee = employee,
						PrevEmployeeVacationSEId = prevVacationSEId != 0 ? prevVacationSEId : (int?)null,
						AdjustmentDate = employeeUser.EmployeeVacationSE.AdjustmentDate,
						EarnedDaysPaid = employeeUser.EmployeeVacationSE.EarnedDaysPaid,
						EarnedDaysUnpaid = employeeUser.EmployeeVacationSE.EarnedDaysUnpaid,
						EarnedDaysAdvance = employeeUser.EmployeeVacationSE.EarnedDaysAdvance,
						SavedDaysYear1 = employeeUser.EmployeeVacationSE.SavedDaysYear1,
						SavedDaysYear2 = employeeUser.EmployeeVacationSE.SavedDaysYear2,
						SavedDaysYear3 = employeeUser.EmployeeVacationSE.SavedDaysYear3,
						SavedDaysYear4 = employeeUser.EmployeeVacationSE.SavedDaysYear4,
						SavedDaysYear5 = employeeUser.EmployeeVacationSE.SavedDaysYear5,
						SavedDaysOverdue = employeeUser.EmployeeVacationSE.SavedDaysOverdue,

						UsedDaysPaid = employeeUser.EmployeeVacationSE.UsedDaysPaid,
						PaidVacationAllowance = employeeUser.EmployeeVacationSE.PaidVacationAllowance,
						PaidVacationVariableAllowance = employeeUser.EmployeeVacationSE.PaidVacationVariableAllowance,
						UsedDaysUnpaid = employeeUser.EmployeeVacationSE.UsedDaysUnpaid,
						UsedDaysAdvance = employeeUser.EmployeeVacationSE.UsedDaysAdvance,
						UsedDaysYear1 = employeeUser.EmployeeVacationSE.UsedDaysYear1,
						UsedDaysYear2 = employeeUser.EmployeeVacationSE.UsedDaysYear2,
						UsedDaysYear3 = employeeUser.EmployeeVacationSE.UsedDaysYear3,
						UsedDaysYear4 = employeeUser.EmployeeVacationSE.UsedDaysYear4,
						UsedDaysYear5 = employeeUser.EmployeeVacationSE.UsedDaysYear5,
						UsedDaysOverdue = employeeUser.EmployeeVacationSE.UsedDaysOverdue,

						RemainingDaysPaid = employeeUser.EmployeeVacationSE.RemainingDaysPaid,
						RemainingDaysUnpaid = employeeUser.EmployeeVacationSE.RemainingDaysUnpaid,
						RemainingDaysAdvance = employeeUser.EmployeeVacationSE.RemainingDaysAdvance,
						RemainingDaysYear1 = employeeUser.EmployeeVacationSE.RemainingDaysYear1,
						RemainingDaysYear2 = employeeUser.EmployeeVacationSE.RemainingDaysYear2,
						RemainingDaysYear3 = employeeUser.EmployeeVacationSE.RemainingDaysYear3,
						RemainingDaysYear4 = employeeUser.EmployeeVacationSE.RemainingDaysYear4,
						RemainingDaysYear5 = employeeUser.EmployeeVacationSE.RemainingDaysYear5,
						RemainingDaysOverdue = employeeUser.EmployeeVacationSE.RemainingDaysOverdue,

						EarnedDaysRemainingHoursPaid = employeeUser.EmployeeVacationSE.EarnedDaysRemainingHoursPaid,
						EarnedDaysRemainingHoursUnpaid = employeeUser.EmployeeVacationSE.EarnedDaysRemainingHoursUnpaid,
						EarnedDaysRemainingHoursAdvance = employeeUser.EmployeeVacationSE.EarnedDaysRemainingHoursAdvance,
						EarnedDaysRemainingHoursYear1 = employeeUser.EmployeeVacationSE.EarnedDaysRemainingHoursYear1,
						EarnedDaysRemainingHoursYear2 = employeeUser.EmployeeVacationSE.EarnedDaysRemainingHoursYear2,
						EarnedDaysRemainingHoursYear3 = employeeUser.EmployeeVacationSE.EarnedDaysRemainingHoursYear3,
						EarnedDaysRemainingHoursYear4 = employeeUser.EmployeeVacationSE.EarnedDaysRemainingHoursYear4,
						EarnedDaysRemainingHoursYear5 = employeeUser.EmployeeVacationSE.EarnedDaysRemainingHoursYear5,
						EarnedDaysRemainingHoursOverdue = employeeUser.EmployeeVacationSE.EarnedDaysRemainingHoursOverdue,

						EmploymentRatePaid = employeeUser.EmployeeVacationSE.EmploymentRatePaid,
						EmploymentRateYear1 = employeeUser.EmployeeVacationSE.EmploymentRateYear1,
						EmploymentRateYear2 = employeeUser.EmployeeVacationSE.EmploymentRateYear2,
						EmploymentRateYear3 = employeeUser.EmployeeVacationSE.EmploymentRateYear3,
						EmploymentRateYear4 = employeeUser.EmployeeVacationSE.EmploymentRateYear4,
						EmploymentRateYear5 = employeeUser.EmployeeVacationSE.EmploymentRateYear5,
						EmploymentRateOverdue = employeeUser.EmployeeVacationSE.EmploymentRateOverdue,

						DebtInAdvanceAmount = employeeUser.EmployeeVacationSE.DebtInAdvanceAmount,
						DebtInAdvanceDueDate = employeeUser.EmployeeVacationSE.DebtInAdvanceDueDate,
						DebtInAdvanceDelete = employeeUser.EmployeeVacationSE.DebtInAdvanceDelete,

						RemainingDaysAllowanceYear1 = employeeUser.EmployeeVacationSE.RemainingDaysAllowanceYear1,
						RemainingDaysAllowanceYear2 = employeeUser.EmployeeVacationSE.RemainingDaysAllowanceYear2,
						RemainingDaysAllowanceYear3 = employeeUser.EmployeeVacationSE.RemainingDaysAllowanceYear3,
						RemainingDaysAllowanceYear4 = employeeUser.EmployeeVacationSE.RemainingDaysAllowanceYear4,
						RemainingDaysAllowanceYear5 = employeeUser.EmployeeVacationSE.RemainingDaysAllowanceYear5,
						RemainingDaysAllowanceYearOverdue = employeeUser.EmployeeVacationSE.RemainingDaysAllowanceYearOverdue,

						RemainingDaysVariableAllowanceYear1 = employeeUser.EmployeeVacationSE.RemainingDaysVariableAllowanceYear1,
						RemainingDaysVariableAllowanceYear2 = employeeUser.EmployeeVacationSE.RemainingDaysVariableAllowanceYear2,
						RemainingDaysVariableAllowanceYear3 = employeeUser.EmployeeVacationSE.RemainingDaysVariableAllowanceYear3,
						RemainingDaysVariableAllowanceYear4 = employeeUser.EmployeeVacationSE.RemainingDaysVariableAllowanceYear4,
						RemainingDaysVariableAllowanceYear5 = employeeUser.EmployeeVacationSE.RemainingDaysVariableAllowanceYear5,
						RemainingDaysVariableAllowanceYearOverdue = employeeUser.EmployeeVacationSE.RemainingDaysVariableAllowanceYearOverdue
					};

					if (vacations.Count == 0)
					{
						SetCreatedProperties(newVacation);
					}
					else
					{
						newVacation.Created = vacations.First().Created;
						newVacation.CreatedBy = vacations.First().CreatedBy;
						SetModifiedProperties(newVacation);
					}
				}

				#endregion

				#region Categories

				if (employeeUser.CategoryRecords != null)
				{
					result = CategoryManager.SaveCompanyCategoryRecords(entities, transaction, employeeUser.CategoryRecords, employeeUser.ActorCompanyId, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employee.EmployeeId);
					if (!result.Success)
						return result;

					if (result.BooleanValue2)
						changeAffectTerminal = true;

					flushCompanyCategoryRecords = true;
				}

				#endregion

				#region EmployeeAccounts

				#region Update/Delete

				if (!employee.EmployeeAccount.IsNullOrEmpty())
				{
					// Loop through all existing parents
					foreach (EmployeeAccount empAccount in employee.EmployeeAccount.Where(a => a.State == (int)SoeEntityState.Active && !a.ParentEmployeeAccountId.HasValue).ToList())
					{
						EmployeeAccountDTO empAccountInput = employeeUser.Accounts.FirstOrDefault(a => a.EmployeeAccountId == empAccount.EmployeeAccountId);
						if (empAccountInput != null)
						{
							#region Update

							if (empAccount.AccountId != empAccountInput.AccountId ||
								empAccount.MainAllocation != empAccountInput.MainAllocation ||
								empAccount.Default != empAccountInput.Default ||
								empAccount.DateFrom != empAccountInput.DateFrom ||
								empAccount.DateTo != empAccountInput.DateTo ||
								empAccountInput.State == SoeEntityState.Deleted)
							{
								empAccount.AccountId = empAccountInput.AccountId;
								empAccount.MainAllocation = empAccountInput.MainAllocation;
								empAccount.Default = empAccountInput.Default;
								empAccount.DateFrom = empAccountInput.DateFrom;
								empAccount.DateTo = empAccountInput.DateTo;
								empAccount.State = (int)empAccountInput.State;
								SetModifiedProperties(empAccount);
							}

							// Remove from input to prevent adding it again below
							employeeUser.Accounts.Remove(empAccountInput);

							#region Children

							#region Update/Delete

							if (empAccount.Children != null)
							{
								// Existing children
								foreach (EmployeeAccount empAccountChild in empAccount.Children.Where(a => a.State == (int)SoeEntityState.Active).ToList())
								{
									EmployeeAccountDTO empAccountChildInput = empAccountInput.Children.FirstOrDefault(a => a.EmployeeAccountId == empAccountChild.EmployeeAccountId);
									if (empAccountChildInput != null)
									{
										#region Update

										if (empAccountChild.AccountId != empAccountChildInput.AccountId ||
											empAccountChild.MainAllocation != empAccountChildInput.MainAllocation ||
											empAccountChild.Default != empAccountChildInput.Default ||
											empAccountChild.DateFrom != empAccountChildInput.DateFrom ||
											empAccountChild.DateTo != empAccountChildInput.DateTo)
										{
											empAccountChild.AccountId = empAccountChildInput.AccountId;
											empAccountChild.MainAllocation = empAccountChildInput.MainAllocation;
											empAccountChild.Default = empAccountChildInput.Default;
											empAccountChild.DateFrom = empAccountChildInput.DateFrom;
											empAccountChild.DateTo = empAccountChildInput.DateTo;
											SetModifiedProperties(empAccountChild);
										}

										// Remove from input to prevent adding it again below
										empAccountInput.Children.Remove(empAccountChildInput);

										// Existing sub children
										foreach (EmployeeAccount empAccountSubChild in empAccountChild.Children.Where(a => a.State == (int)SoeEntityState.Active).ToList())
										{
											EmployeeAccountDTO empAccountSubChildInput = empAccountChildInput.Children.FirstOrDefault(a => a.EmployeeAccountId == empAccountSubChild.EmployeeAccountId);
											if (empAccountSubChildInput != null)
											{
												#region Update

												if (empAccountSubChild.AccountId != empAccountSubChildInput.AccountId ||
													empAccountSubChild.MainAllocation != empAccountSubChildInput.MainAllocation ||
													empAccountSubChild.Default != empAccountSubChildInput.Default ||
													empAccountSubChild.DateFrom != empAccountSubChildInput.DateFrom ||
													empAccountSubChild.DateTo != empAccountSubChildInput.DateTo)
												{
													empAccountSubChild.AccountId = empAccountSubChildInput.AccountId;
													empAccountSubChild.MainAllocation = empAccountSubChildInput.MainAllocation;
													empAccountSubChild.Default = empAccountSubChildInput.Default;
													empAccountSubChild.DateFrom = empAccountSubChildInput.DateFrom;
													empAccountSubChild.DateTo = empAccountSubChildInput.DateTo;
													SetModifiedProperties(empAccountSubChild);
												}

												#endregion
											}
											else
											{
												#region Delete

												SetModifiedProperties(empAccountSubChild);
												ChangeEntityState(empAccountSubChild, SoeEntityState.Deleted);

												#endregion
											}

											// Remove from input to prevent adding it again below
											empAccountChildInput.Children.Remove(empAccountSubChildInput);
										}

										// New sub children

										#region Add

										foreach (EmployeeAccountDTO empAccountSubChildInput in empAccountChildInput.Children)
										{
											EmployeeAccount empAccountSubChild = new EmployeeAccount()
											{
												ActorCompanyId = ActorCompanyId,
												Employee = employee,
												AccountId = empAccountSubChildInput.AccountId,
												MainAllocation = empAccountSubChildInput.MainAllocation,
												Default = empAccountSubChildInput.Default,
												DateFrom = empAccountSubChildInput.DateFrom,
												DateTo = empAccountSubChildInput.DateTo
											};
											SetCreatedProperties(empAccountSubChild);
											if (empAccountChild.Children == null)
												empAccountChild.Children = new EntityCollection<EmployeeAccount>();
											empAccountChild.Children.Add(empAccountSubChild);
										}

										#endregion

										#endregion
									}
									else
									{
										#region Delete

										SetModifiedProperties(empAccountChild);
										ChangeEntityState(empAccountChild, SoeEntityState.Deleted);

										foreach (EmployeeAccount empAccountSubChild in empAccountChild.Children.Where(a => a.State == (int)SoeEntityState.Active).ToList())
										{
											SetModifiedProperties(empAccountSubChild);
											ChangeEntityState(empAccountSubChild, SoeEntityState.Deleted);
										}

										#endregion
									}
								}
							}

							#endregion

							#region Add

							foreach (EmployeeAccountDTO empAccountChildInput in empAccountInput.Children.Where(a => !a.ParentEmployeeAccountId.HasValue || a.ParentEmployeeAccountId.Value == 0).ToList())
							{
								EmployeeAccount empAccountChild = new EmployeeAccount()
								{
									ActorCompanyId = ActorCompanyId,
									Employee = employee,
									AccountId = empAccountChildInput.AccountId,
									MainAllocation = empAccountChildInput.MainAllocation,
									Default = empAccountChildInput.Default,
									DateFrom = empAccountChildInput.DateFrom,
									DateTo = empAccountChildInput.DateTo
								};
								SetCreatedProperties(empAccountChild);
								if (empAccount.Children == null)
									empAccount.Children = new EntityCollection<EmployeeAccount>();
								empAccount.Children.Add(empAccountChild);

								if (empAccountChildInput.Children != null)
								{
									foreach (EmployeeAccountDTO empAccountSubChildInput in empAccountChildInput.Children)
									{
										EmployeeAccount empAccountSubChild = new EmployeeAccount()
										{
											ActorCompanyId = ActorCompanyId,
											Employee = employee,
											AccountId = empAccountSubChildInput.AccountId,
											MainAllocation = empAccountSubChildInput.MainAllocation,
											Default = empAccountSubChildInput.Default,
											DateFrom = empAccountSubChildInput.DateFrom,
											DateTo = empAccountSubChildInput.DateTo
										};
										SetCreatedProperties(empAccountSubChild);
										if (empAccountChild.Children == null)
											empAccountChild.Children = new EntityCollection<EmployeeAccount>();
										empAccountChild.Children.Add(empAccountSubChild);
									}
								}
							}

							#endregion

							#endregion

							#endregion
						}
						else
						{
							#region Delete

							if (empAccount.Children != null)
							{
								// Delete children
								foreach (EmployeeAccount empAccountChild in empAccount.Children)
								{
									SetModifiedProperties(empAccountChild);
									ChangeEntityState(empAccountChild, SoeEntityState.Deleted);

									foreach (EmployeeAccount empAccountSubChild in empAccountChild.Children)
									{
										SetModifiedProperties(empAccountSubChild);
										ChangeEntityState(empAccountSubChild, SoeEntityState.Deleted);
									}
								}
							}

							SetModifiedProperties(empAccount);
							ChangeEntityState(empAccount, SoeEntityState.Deleted);

							#endregion
						}
					}

					flushEmployeeAccounts = true;
				}

				#endregion

				#region Add

				// Add all accounts that is left in the input
				if (!employeeUser.Accounts.IsNullOrEmpty())
				{
					foreach (EmployeeAccountDTO empAccountInput in employeeUser.Accounts)
					{
						EmployeeAccount empAccount = new EmployeeAccount()
						{
							ActorCompanyId = ActorCompanyId,
							AccountId = empAccountInput.AccountId,
							MainAllocation = empAccountInput.MainAllocation,
							Default = empAccountInput.Default,
							DateFrom = empAccountInput.DateFrom,
							DateTo = empAccountInput.DateTo,
							AddedOtherEmployeeAccount = empAccountInput.AddedOtherEmployeeAccount
						};
						SetCreatedProperties(empAccount);
						employee.EmployeeAccount.Add(empAccount);

						if (empAccountInput.Children != null)
						{
							foreach (EmployeeAccountDTO empAccountChildInput in empAccountInput.Children)
							{
								EmployeeAccount empAccountChild = new EmployeeAccount()
								{
									ActorCompanyId = ActorCompanyId,
									Employee = employee,
									AccountId = empAccountChildInput.AccountId,
									MainAllocation = empAccountChildInput.MainAllocation,
									Default = empAccountChildInput.Default,
									DateFrom = empAccountChildInput.DateFrom,
									DateTo = empAccountChildInput.DateTo
								};
								SetCreatedProperties(empAccountChild);
								if (empAccount.Children == null)
									empAccount.Children = new EntityCollection<EmployeeAccount>();
								empAccount.Children.Add(empAccountChild);

								if (empAccountChildInput.Children != null)
								{
									foreach (EmployeeAccountDTO empAccountSubChildInput in empAccountChildInput.Children)
									{
										EmployeeAccount empAccountSubChild = new EmployeeAccount()
										{
											ActorCompanyId = ActorCompanyId,
											Employee = employee,
											AccountId = empAccountSubChildInput.AccountId,
											MainAllocation = empAccountSubChildInput.MainAllocation,
											Default = empAccountSubChildInput.Default,
											DateFrom = empAccountSubChildInput.DateFrom,
											DateTo = empAccountSubChildInput.DateTo
										};
										SetCreatedProperties(empAccountSubChild);
										if (empAccountChild.Children == null)
											empAccountChild.Children = new EntityCollection<EmployeeAccount>();
										empAccountChild.Children.Add(empAccountSubChild);
									}
								}
							}
						}
					}

					flushEmployeeAccounts = true;
				}

				#endregion

				if (changesRepository != null && employee.EmployeeAccount != null)
					changesRepository.SetAfterValue(employee.EmployeeAccount.ToList());

				#endregion

				#region EmployeeSettings

				if (employeeUser.EmployeeSettings != null)
				{
					foreach (EmployeeSettingDTO settingInput in employeeUser.EmployeeSettings)
					{
						EmployeeSetting setting = employee.EmployeeSetting.FirstOrDefault(i => i.EmployeeSettingId != 0 && i.EmployeeSettingId == settingInput.EmployeeSettingId);
						if (setting == null)
						{
							setting = new EmployeeSetting()
							{
								ActorCompanyId = ActorCompanyId
							};
							SetCreatedProperties(setting);
							employee.EmployeeSetting.Add(setting);
						}
						else
						{
							SetModifiedProperties(setting);
						}

						setting.EmployeeSettingAreaType = (int)settingInput.EmployeeSettingAreaType;
						setting.EmployeeSettingGroupType = (int)settingInput.EmployeeSettingGroupType;
						setting.EmployeeSettingType = settingInput.EmployeeSettingType;
						setting.DataType = (int)settingInput.DataType;
						setting.ValidFromDate = settingInput.ValidFromDate;
						setting.ValidToDate = settingInput.ValidToDate;
						setting.Name = Enum.GetName(typeof(TermGroup_EmployeeSettingType), settingInput.EmployeeSettingType) ?? settingInput.TypeName;
						setting.StrData = settingInput.StrData;
						setting.IntData = settingInput.IntData;
						setting.DecimalData = settingInput.DecimalData;
						setting.BoolData = settingInput.BoolData;
						setting.DateData = settingInput.DateData;
						setting.TimeData = settingInput.TimeData;
						setting.State = (int)settingInput.State;
					}
				}

				if (changesRepository != null && employee.EmployeeSetting != null)
					changesRepository.SetAfterValue(employee.EmployeeSetting.ToList());

				#endregion

				#region Employer

				if (employeeUser.EmployeeEmployeers != null)
				{
					try
					{
						var employers = entities.Employer.Where(e => e.ActorCompanyId == ActorCompanyId && e.State == (int)SoeEntityState.Active).ToList();

						if (!employee.EmployeeEmployer.IsLoaded)
							employee.EmployeeEmployer.Load();

						foreach (var empEmployer in employeeUser.EmployeeEmployeers)
						{
							var employer = employers.FirstOrDefault(e => e.OrgNr == empEmployer.EmployerRegistrationNumber);

							if (employer == null)
							{
								LogCollector.LogInfo($"Employer with registration number {empEmployer.EmployerRegistrationNumber} not found for employee {employeeUser.EmployeeNr} {company.Name}.");
								continue;
							}

							var matchingEmployers = employee.EmployeeEmployer.FirstOrDefault(ee => ee.EmployerId == employer.EmployerId && ee.DateFrom == empEmployer.DateFrom);

							if (matchingEmployers == null)
							{
								// Add new relation
								EmployeeEmployer employeeEmployer = new EmployeeEmployer()
								{
									ActorCompanyId = ActorCompanyId,
									Employee = employee,
									EmployerId = employer.EmployerId,
									DateFrom = empEmployer.DateFrom,
									DateTo = empEmployer.DateTo
								};
								SetCreatedProperties(employeeEmployer);
								employee.EmployeeEmployer.Add(employeeEmployer);
							}
							else
							{
								if (empEmployer.DateTo != matchingEmployers.DateTo)
								{
									// Update existing relation
									matchingEmployers.DateTo = empEmployer.DateTo;
									SetModifiedProperties(matchingEmployers);
								}
								if (empEmployer.State == SoeEntityState.Deleted)
								{
									// Delete existing relation
									ChangeEntityState(matchingEmployers, SoeEntityState.Deleted);
									SetModifiedProperties(matchingEmployers);
								}
							}
						}
					}
					catch (Exception ex)
					{
						LogCollector.LogError(new Exception($"Error processing EmployeeEmployers for employee {employeeUser.EmployeeNr} in company {company.Name}: {ex.ToString()}"));
					}
				}

				#endregion
			}

			#endregion

			#region Save

			result = SaveChanges(entities, transaction);
			if (result.Success)
			{
				if (contactPerson != null)
					actorContactPersonId = contactPerson.ActorContactPersonId;
				if (user != null)
					selectedUserId = user.UserId;
				if (employee != null)
					selectedEmployeeId = employee.EmployeeId;

				if (flushCompanyCategoryRecords)
					FlushCompanyCategoryRecordsFromCache(CacheConfig.Company(base.ActorCompanyId));
				if (flushEmployeeAccounts)
					FlushEmployeeAccountsFromCache(CacheConfig.Company(base.ActorCompanyId), "NoKey");

				#region Addresses

				result = ContactManager.SaveContactAddresses(entities, contactAddresses, contactPerson.ActorContactPersonId, TermGroup_SysContactType.Company);
				if (!result.Success)
				{
					result.ErrorNumber = (int)ActionResultSave.EmployeeUserContactsAndTeleComNotSaved;
					result.ErrorMessage = GetText(11048, "Kontaktuppgifter ej sparade");
					return result;
				}

				#endregion

				#region ExtraFields

				if (employee != null && !extraFields.IsNullOrEmpty())
				{
					result = ExtraFieldManager.SaveExtraFieldRecords(entities, extraFields, (int)SoeEntityType.Employee, employee.EmployeeId, base.ActorCompanyId);
					if (!result.Success)
					{
						result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
						return result;
					}
				}

				#endregion

				#region EmployeePosition

				if (employee != null && employeePositions != null)
				{
					result = EmployeeManager.SaveEmployeePositions(entities, transaction, employeePositions, employee);
					if (!result.Success)
					{
						result.ErrorNumber = (int)ActionResultSave.EmployeeUserPositionsNotSaved;
						result.ErrorMessage = GetText(11050, "Befattningar ej sparade");
						return result;
					}
				}

				#endregion

				#region EmployeeSkill

				if (!applyFeaturesResult.HasBlankedEmployeeSkills && employee != null && employeeSkills != null)
				{
					result = EmployeeManager.SaveEmployeeSkills(entities, transaction, employee, employeeSkills);
					if (!result.Success)
					{
						result.ErrorNumber = (int)ActionResultSave.EmployeeUserSkillsNotSaved;
						result.ErrorMessage = GetText(11049, "Kompetenser ej sparade");
						return result;
					}
				}

				#endregion

				#region Replacement user

				if (userReplacement != null)
				{
					result = SaveReplacementUser(entities, transaction, userReplacement, UserReplacementType.AttestFlow);
					if (!result.Success)
					{
						result.ErrorNumber = (int)ActionResultSave.EmployeeUserAttestReplacementNotSaved;
						return result;
					}
				}

				#endregion

				#region EmployeeChild

				if (!applyFeaturesResult.HasBlankedChildCare && employee != null && employeeUser.EmployeeChilds != null)
				{
					result = EmployeeManager.SaveEmployeeChilds(entities, transaction, employee, employeeUser.EmployeeChilds);
					if (!result.Success)
					{
						result.ErrorNumber = (int)ActionResultSave.EmployeeUserChildsNotSaved;
						result.ErrorMessage = GetText(11055, "Barn kunde inte sparas");
						return result;
					}
				}

				#endregion

				#region Template groups

				if (!applyFeaturesResult.HasBlankedTemplateGroups && employee != null && employeeUser.TemplateGroups != null && employeeUser.IsTemplateGroupsChanged)
				{
					result = EmployeeManager.SaveEmployeeTemplateGroups(entities, transaction, employee, employeeUser.TemplateGroups, employeeUser.ActorCompanyId);
					if (!result.Success)
					{
						result.ErrorNumber = (int)ActionResultSave.EmployeeTemplateGroupsNotSaved;
						result.ErrorMessage = GetText(12052, "Schemagrupper kunde inte sparas");
						return result;
					}
				}

				#endregion

				#region EmployeeMeetings

				if (!applyFeaturesResult.HasBlankedEmployeeMeetings && employee != null && employeeUser.EmployeeMeetings != null && employeeUser.IsEmployeeMeetingsChanged)
				{
					result = EmployeeManager.SaveEmployeeMeetings(entities, transaction, employee, employeeUser.EmployeeMeetings, employeeUser.ActorCompanyId);
					if (!result.Success)
					{
						result.ErrorNumber = (int)ActionResultSave.EmployeeMeetingsNotSaved;
						result.ErrorMessage = GetText(11056, "HR - Samtal kunde inte sparas");
						return result;
					}
				}

				#endregion

				#region EmployeeUnionFees

				if (!applyFeaturesResult.HasBlankedUnionFees && employee != null && employeeUser.UnionFees != null)
				{
					result = EmployeeManager.SaveEmployeeUnionFees(entities, transaction, employee, employeeUser.UnionFees);
					if (!result.Success)
					{
						result.ErrorNumber = (int)ActionResultSave.EmployeeUnionFeesNotSaved;
						result.ErrorMessage = GetText(11057, "Fackavgifter kunde inte sparas");
						return result;
					}
				}

				#endregion

				#region EmployeeTimeWorkAccounts

				if (!applyFeaturesResult.HasBlankedTimeWorkAccounts && employee != null && employeeUser.EmployeeTimeWorkAccounts != null)
				{
					result = EmployeeManager.SaveEmployeeTimeWorkAccounts(entities, transaction, employee, employeeUser.EmployeeTimeWorkAccounts);
					if (!result.Success)
						return result;
				}

				#endregion

				#region Roles and attest roles

				if (userRoles == null)
				{
					saveRoles = false;
					saveAttestRoles = false;
				}

				if (saveRoles || saveAttestRoles)
				{
					User currentUser = UserManager.GetUser(entities, base.UserId, loadUserCompanyRole: saveRoles, loadAttestRoleUser: saveAttestRoles);

					if (saveRoles)
					{
						result = UserManager.SaveUserCompanyRoles(entities, userRoles, employeeUser.LicenseId, user, currentUser);
						if (!result.Success)
						{
							if (string.IsNullOrEmpty(result.ErrorMessage))
							{
								result.ErrorNumber = (int)ActionResultSave.EmployeeUserRolesNotSaved;
								result.ErrorMessage = GetText(11746, "Roller kunde inte sparas");
							}
							else
								result.ErrorMessage = $"{GetText(11746, "Roller kunde inte sparas")}. {result.ErrorMessage}";
							return result;
						}
					}

					if (saveAttestRoles)
					{
						result = AttestManager.SaveAttestRoleUsers(entities, userRoles, user, currentUser, employeeUser.ActorCompanyId, onlyValidateAttestRolesInCompany);
						if (result.Success)
						{
							UserCompanySetting userCompanySetting = SettingManager.GetUserCompanySetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountHierarchyId, user.UserId, company.ActorCompanyId, 0);
							if (userCompanySetting != null)
							{
								userCompanySetting.StrData = null;
								SetModifiedProperties(userCompanySetting);
								result = SaveChanges(entities);
							}
						}
						else
						{
							if (string.IsNullOrEmpty(result.ErrorMessage))
							{
								result.ErrorNumber = (int)ActionResultSave.EmployeeUserAttestRolesNotSaved;
								result.ErrorMessage = GetText(11747, "Attestroller kunde inte sparas");
							}
							else
								result.ErrorMessage = $"{GetText(11747, "Attestroller kunde inte sparas")}. {result.ErrorMessage}";
							return result;
						}
					}
				}

				if (changedCompany && previousDefaultActorCompanyId > 0 && user != null)
				{
					#region UserCompanySettings

					UserCompanySetting settingCoreCompanyId = SettingManager.GetUserCompanySetting(entities, SettingMainType.User, (int)UserSettingType.CoreCompanyId, user.UserId, 0, 0);
					if (settingCoreCompanyId != null)
						settingCoreCompanyId.IntData = user.DefaultActorCompanyId ?? 0;

					//Update setting Default Role
					UserCompanySetting settingCoreRoleId = SettingManager.GetUserCompanySetting(entities, SettingMainType.User, (int)UserSettingType.CoreRoleId, user.UserId, 0, 0);
					if (settingCoreRoleId != null)
						settingCoreRoleId.IntData = user.DefaultActorCompanyId.HasValue ? UserManager.GetDefaultRoleId(entities, user.DefaultActorCompanyId.Value, user) : 0;

					#endregion

					#region UserFavorites

					//Delete default favorites
					List<UserFavorite> userFavorites = SettingManager.GetUserFavorites(entities, user.UserId, previousDefaultActorCompanyId);
					foreach (UserFavorite userFavorite in userFavorites.Where(i => i.IsDefault))
					{
						entities.DeleteObject(userFavorite);
					}

					#endregion

					result = SaveChanges(entities);
					if (!result.Success)
						return result;
				}

				#endregion

				#region Files

				if (files != null)
				{
					result = SaveEmployeeFiles(entities, files);
					if (!result.Success)
						return result;
				}

				#endregion

				#region TrackChanges

				// Add track changes
				foreach (TrackChangesDTO dto in trackChangesItems.Where(t => t.Action == TermGroup_TrackChangesAction.Insert))
				{
					// Replace temp ids with actual ids created on save
					if (dto.Entity == SoeEntityType.EmploymentPriceType && mappingDict[dto.RecordId] is EmploymentPriceType)
					{
						EmploymentPriceType priceType = mappingDict[dto.RecordId] as EmploymentPriceType;
						dto.RecordId = priceType.EmploymentPriceTypeId;
					}
					else if (dto.Entity == SoeEntityType.EmploymentPriceTypePeriod)
					{
						EmploymentPriceTypePeriod period = mappingDict[dto.RecordId] as EmploymentPriceTypePeriod;
						dto.RecordId = period.EmploymentPriceTypePeriodId;
						dto.ParentRecordId = period.EmploymentPriceTypeId;
					}
				}
				if (trackChangesItems.Any())
					result = TrackChangesManager.AddTrackChanges(entities, transaction, trackChangesItems);

				#endregion

				#region WebPubSub

				// Check if there are any recent changes in employment that might affect data synced to terminal
				if (!changeAffectTerminal && employee != null && employee.EmploymentChangeBatch != null && employee.EmploymentChange != null)
				{
					EmploymentChangeBatch batch = employee.EmploymentChangeBatch.OrderByDescending(b => b.Created).FirstOrDefault(b => b.Created > DateTime.Now.AddMinutes(-2));
					if (batch != null)
					{
						List<EmploymentChange> changes = employee.EmploymentChange.Where(c => c.EmploymentChangeBatchId == batch.EmploymentChangeBatchId).ToList();
						foreach (EmploymentChange change in changes)
						{
							switch ((TermGroup_EmploymentChangeFieldType)change.FieldType)
							{
								case TermGroup_EmploymentChangeFieldType.DateFrom:
								case TermGroup_EmploymentChangeFieldType.DateTo:
								case TermGroup_EmploymentChangeFieldType.EmployeeGroupId:
								case TermGroup_EmploymentChangeFieldType.State:
									changeAffectTerminal = true;
									break;
							}
						}
					}
				}

				if (changeAffectTerminal)
					result.BooleanValue2 = true;

				#endregion

				// Set result values
				Dictionary<int, int> dict = new Dictionary<int, int>()
				{
					{ (int)SaveEmployeeUserResult.ActorContactPersonId, actorContactPersonId },
					{ (int)SaveEmployeeUserResult.UserId, selectedUserId },
					{ (int)SaveEmployeeUserResult.EmployeeId, selectedEmployeeId },
				};
				if (result != null)
				{
					result.IntDict = dict;
					if (result.Success && infoMessage.Length > 0)
						result.InfoMessage = infoMessage.ToString();
				}

				#region SoftOneId

				if (user != null && !string.IsNullOrEmpty(user.Email))
				{
					//Used in calling method
					employeeUser.UserId = user.UserId;
				}

				#endregion
			}

			if (result.Success && base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId))
			{
				FlushEmployeeAccountsFromCache(CacheConfig.Company(base.ActorCompanyId), "NoKey");
			}

			#endregion

			return result;
		}

		public ActionResult ValidateEmployeeAccounts(CompEntities entities, int actorCompanyId, int employeeId, bool mustHaveMainAllocation, bool mustHaveDefault)
		{
			var employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, actorCompanyId, employeeId.ObjToList()).ToDTOs().ToList();
			return ValidateEmployeeAccounts(employeeAccounts.Where(x => !x.ParentEmployeeAccountId.HasValue).ToList(), mustHaveMainAllocation, mustHaveDefault);
		}

		public ActionResult ValidateEmployeeAccounts(List<EmployeeAccountDTO> accounts, bool mustHaveMainAllocation, bool mustHaveDefault)
		{
			List<EmployeeAccountFlattenedDTO> flattened = new List<EmployeeAccountFlattenedDTO>();

			if (accounts == null)
				accounts = new List<EmployeeAccountDTO>();

			// Flatten all accounts, setting a level number instead of hierarchy
			// Also set end date if null
			// If data not saved yet, use temporary ID to be able to link parent and child
			DateTime maxDateTo = DateTime.Today.AddYears(1);
			int idCounter = 0;
			accounts
				.Where(parent => parent != null)
				.ToList()
				.ForEach(parent =>
			{
				flattened.Add(new EmployeeAccountFlattenedDTO()
				{
					Level = 1,
					EmployeeAccountId = parent.EmployeeAccountId > 0 ? parent.EmployeeAccountId : ++idCounter,
					AccountId = parent.AccountId,
					MainAllocation = parent.MainAllocation,
					Default = parent.Default,
					DateFrom = parent.DateFrom,
					DateTo = parent.DateTo.HasValue ? parent.DateTo.Value : maxDateTo,
				});

				parent.Children?.ForEach(child =>
				{
					flattened.Add(new EmployeeAccountFlattenedDTO()
					{
						Level = 2,
						EmployeeAccountId = child.EmployeeAccountId > 0 ? child.EmployeeAccountId : ++idCounter,
						ParentEmployeeAccountId = parent.EmployeeAccountId,
						AccountId = child.AccountId,
						MainAllocation = child.MainAllocation,
						Default = child.Default,
						DateFrom = child.DateFrom,
						DateTo = child.DateTo ?? maxDateTo,
					});


					child.Children?
						.Where(subChild => subChild != null)
						.ToList()
						.ForEach(subChild =>
						{
							flattened.Add(new EmployeeAccountFlattenedDTO()
							{
								Level = 3,
								EmployeeAccountId = subChild.EmployeeAccountId > 0 ? subChild.EmployeeAccountId : ++idCounter,
								ParentEmployeeAccountId = child.EmployeeAccountId,
								AccountId = subChild.AccountId,
								MainAllocation = subChild.MainAllocation,
								Default = subChild.Default,
								DateFrom = subChild.DateFrom,
								DateTo = subChild.DateTo ?? maxDateTo,
							});
						});
				});
			});

			// Check if there are multiple main allocations on same level that overlaps date range
			for (int i = 1; i <= 3; i++)
			{
				if (CheckOverlappingEmployeeAccounts(GetEmployeeAccountsWithMainAllocation(flattened, i)))
					return new ActionResult((int)ActionResultSave.EmployeeOverlappingMainAffiliation, TermCacheManager.Instance.GetText("time.employee.employeeaccount.multiplemainonsamelevel"));
			}

			// Check if there are any children marked as main allocations without parents marked as main allocations
			List<EmployeeAccountFlattenedDTO> level1Mains = GetEmployeeAccountsWithMainAllocation(flattened, 1);
			List<EmployeeAccountFlattenedDTO> level2Mains = GetEmployeeAccountsWithMainAllocation(flattened, 2);
			List<EmployeeAccountFlattenedDTO> level3Mains = GetEmployeeAccountsWithMainAllocation(flattened, 3);
			if (level3Mains.Any())
			{
				foreach (EmployeeAccountFlattenedDTO a in level3Mains)
				{
					// Get parent
					EmployeeAccountFlattenedDTO parent = level2Mains.FirstOrDefault(p => p.EmployeeAccountId == a.ParentEmployeeAccountId);
					if (parent == null)
						return new ActionResult(TermCacheManager.Instance.GetText("time.employee.employeeaccount.mainchildwithoutparent"));
				}
			}
			if (level2Mains.Any())
			{
				foreach (EmployeeAccountFlattenedDTO a in level2Mains)
				{
					// Get parent
					EmployeeAccountFlattenedDTO parent = level1Mains.FirstOrDefault(p => p.EmployeeAccountId == a.ParentEmployeeAccountId);
					if (parent == null)
						return new ActionResult(TermCacheManager.Instance.GetText("time.employee.employeeaccount.mainchildwithoutparent"));
				}
			}

			// Check that there is a main allocation at top level at all dates
			if (mustHaveMainAllocation)
			{
				List<DateTime> mainDates = new List<DateTime>();
				foreach (EmployeeAccountFlattenedDTO main in level1Mains)
				{
					mainDates.AddRange(CalendarUtility.GetDates(main.DateFrom, main.DateTo));
				}

				List<EmployeeAccountFlattenedDTO> noMains = GetEmployeeAccountsWithoutMainAllocation(flattened, 1);
				List<DateTime> noMainDates = new List<DateTime>();
				foreach (EmployeeAccountFlattenedDTO noMain in noMains)
				{
					noMainDates.AddRange(CalendarUtility.GetDates(noMain.DateFrom, noMain.DateTo));
				}

				bool missingOverlappedByMain = false;
				foreach (DateTime date in noMainDates)
				{
					if (!mainDates.Contains(date))
					{
						missingOverlappedByMain = true;
						break;
					}
				}
				if (missingOverlappedByMain)
				{
					return new ActionResult(TermCacheManager.Instance.GetText("time.employee.employeeaccount.nomainonalldates"));
				}
			}

			// Check that there is a default at top level at all dates
			if (mustHaveDefault)
			{
				List<EmployeeAccountFlattenedDTO> level1Defaults = GetEmployeeAccountsWithDefault(flattened, 1);
				List<DateTime> defaultDates = new List<DateTime>();
				foreach (EmployeeAccountFlattenedDTO @default in level1Defaults)
				{
					defaultDates.AddRange(CalendarUtility.GetDates(@default.DateFrom, @default.DateTo));
				}

				List<EmployeeAccountFlattenedDTO> noDefaults = GetEmployeeAccountsWithoutDefault(flattened, 1);
				List<DateTime> noDefaultDates = new List<DateTime>();
				foreach (EmployeeAccountFlattenedDTO noDefault in noDefaults)
				{
					noDefaultDates.AddRange(CalendarUtility.GetDates(noDefault.DateFrom, noDefault.DateTo));
				}

				bool missingOverlappedByDefault = false;
				foreach (DateTime date in noDefaultDates)
				{
					if (!defaultDates.Contains(date))
					{
						missingOverlappedByDefault = true;
						break;
					}
				}
				if (missingOverlappedByDefault)
				{
					return new ActionResult(TermCacheManager.Instance.GetText("time.employee.employeeaccount.nodefaultonalldates"));
				}
			}

			return new ActionResult();
		}

		private List<EmployeeAccountFlattenedDTO> GetEmployeeAccountsWithMainAllocation(List<EmployeeAccountFlattenedDTO> accounts, int level)
		{
			return accounts.Where(a => a.MainAllocation && a.Level == level).OrderBy(a => a.DateFrom).ThenBy(a => a.DateTo).ToList();
		}

		private List<EmployeeAccountFlattenedDTO> GetEmployeeAccountsWithoutMainAllocation(List<EmployeeAccountFlattenedDTO> accounts, int level)
		{
			return accounts.Where(a => !a.MainAllocation && a.Level == level).OrderBy(a => a.DateFrom).ThenBy(a => a.DateTo).ToList();
		}

		private List<EmployeeAccountFlattenedDTO> GetEmployeeAccountsWithDefault(List<EmployeeAccountFlattenedDTO> accounts, int level)
		{
			return accounts.Where(a => a.Default && a.Level == level).OrderBy(a => a.DateFrom).ThenBy(a => a.DateTo).ToList();
		}

		private List<EmployeeAccountFlattenedDTO> GetEmployeeAccountsWithoutDefault(List<EmployeeAccountFlattenedDTO> accounts, int level)
		{
			return accounts.Where(a => !a.Default && a.Level == level).OrderBy(a => a.DateFrom).ThenBy(a => a.DateTo).ToList();
		}

		private bool CheckOverlappingEmployeeAccounts(List<EmployeeAccountFlattenedDTO> accounts)
		{
			bool overlappingDates = false;
			if (accounts.Count > 1)
			{
				DateTime? prevDateFrom = null;
				DateTime? prevDateTo = null;
				foreach (EmployeeAccountFlattenedDTO a in accounts)
				{
					if (prevDateFrom.HasValue && a.DateFrom <= prevDateTo)
					{
						overlappingDates = true;
						break;
					}
					prevDateFrom = a.DateFrom;
					prevDateTo = a.DateTo;
				}
			}

			return overlappingDates;
		}

		private void SendWebPubSubMessage(CompEntities entities, int actorCompanyId, int employeeId, WebPubSubMessageAction action)
		{
			List<int> terminalIds = TimeStampManager.GetTimeTerminalIdsForPubSub(entities, actorCompanyId);
			bool useCache = false;
			foreach (int terminalId in terminalIds)
			{
				if (terminalIds.Count() == 1 || TimeStampManager.IsEmployeeConnectedToTimeTerminal(actorCompanyId, terminalId, employeeId, null, useCache))
					base.WebPubSubSendMessage(GoTimeStampExtensions.GetTerminalPubSubKey(actorCompanyId, terminalId), GoTimeStampExtensions.GetEmployeeUpdateMessage(actorCompanyId, employeeId, action));

				useCache = true;
			}
		}

		/// <summary>
		/// Supports deleting files or updating the name/description
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		private ActionResult SaveEmployeeFiles(CompEntities entities, IEnumerable<FileUploadDTO> files)
		{
			files = files ?? Enumerable.Empty<FileUploadDTO>();
			foreach (FileUploadDTO file in files)
			{
				if (file.IsDeleted)
				{
					GeneralManager.DeleteDataStorageRecord(entities, file.Id.Value, false);
				}
				else if (file.Id.HasValue)
				{
					DataStorageRecord record = entities.DataStorageRecord.SingleOrDefault(f => f.DataStorageRecordId == file.Id.Value);
					if (record != null && record.RecordNumber != file.Description)
					{
						record.RecordNumber = file.Description;
						if (!record.DataStorageReference.IsLoaded)
							record.DataStorageReference.Load();
						if (record.DataStorage != null)
						{
							record.DataStorage.Description = file.Description;
							SetModifiedProperties(record.DataStorage);
						}
					}
				}
			}

			return SaveChanges(entities);
		}

		public ActionResult CreateVacantEmployees(List<CreateVacantEmployeeDTO> vacants, int licenseId, int actorCompanyId, int userId)
		{
			ActionResult result = new ActionResult(true);

			if (vacants.IsNullOrEmpty())
				return result;

			int defaultTimeCodeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimeCode, 0, actorCompanyId, 0);
			List<(EmployeeUserDTO Employee, EmployeeUserApplyFeaturesResult ApplyFeaturesResult)> items = new List<(EmployeeUserDTO, EmployeeUserApplyFeaturesResult)>();
			foreach (CreateVacantEmployeeDTO vacant in vacants)
			{
				EmployeeUserDTO employee = vacant.ToEmployeeUserDTO(actorCompanyId);
				if (employee != null)
				{
					employee.LicenseId = licenseId;
					employee.TimeCodeId = defaultTimeCodeId;
					employee.Vacant = true;
					employee.SaveEmployee = true;
					items.Add((employee, ApplyFeaturesOnEmployee(employee)));
				}
			}

			using (CompEntities entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();

					using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
					{
						foreach (var item in items)
						{
							result = SaveEmployeeUser(entities, transaction, TermGroup_TrackChangesActionMethod.Employee_CreateVacant, item.Employee, item.ApplyFeaturesResult);
							if (!result.Success)
								return result;
						}

						if (result.Success)
							transaction.Complete();
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
			}

			return result;
		}

		public ActionResult ChangeEmployeeState(CompEntities entities, License license, int employeeId, SoeEntityState newState, int actorCompanyId)
		{
			if (license == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11889, "Licensen hittades inte"));

			Employee originalEmployee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId, onlyActive: false);
			if (originalEmployee == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, "Employee");

			var employeeStateTransition = GetStateTransition((SoeEntityState)originalEmployee.State, newState);

			ActionResult result = LicenseManager.ValidateEmployee(license, actorCompanyId, originalEmployee, employeeStateTransition: (int)employeeStateTransition);
			if (!result.Success)
				return result;

			result = ChangeEntityState(originalEmployee, newState);

			return result;
		}

		#region Help-methods

		private bool IsEmployeeUserDTOValid(EmployeeUserDTO dto)
		{
			return dto != null &&
				   !String.IsNullOrEmpty(dto.FirstName) &&
				   !String.IsNullOrEmpty(dto.LastName);
		}

		#endregion

		#endregion

		#region UserReplacement

		public UserReplacement GetUserReplacement(int actorCompanyId, UserReplacementType type, int originUserId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserReplacement.NoTracking();
			return GetUserReplacement(entities, actorCompanyId, type, originUserId);
		}

		public UserReplacement GetUserReplacement(CompEntities entities, int actorCompanyId, UserReplacementType type, int originUserId)
		{
			return (from r in entities.UserReplacement
					where r.ActorCompanyId == actorCompanyId &&
					r.Type == (int)type &&
					r.OriginUserId == originUserId &&
					r.State == (int)SoeEntityState.Active
					select r).FirstOrDefault();
		}

		public ActionResult SaveReplacementUser(CompEntities entities, TransactionScope transaction, UserReplacementDTO userReplacementDTO, UserReplacementType type)
		{
			UserReplacement userReplacement = GetUserReplacement(entities, userReplacementDTO.ActorCompanyId, type, userReplacementDTO.OriginUserId);
			if (userReplacement == null && userReplacementDTO.StartDate != null)
			{
				#region Add

				userReplacement = new UserReplacement()
				{
					ActorCompanyId = userReplacementDTO.ActorCompanyId,
					OriginUserId = userReplacementDTO.OriginUserId,
					ReplacementUserId = userReplacementDTO.ReplacementUserId,
					Type = (int)UserReplacementType.AttestFlow,
					StartDate = userReplacementDTO.StartDate,
					StopDate = userReplacementDTO.StopDate,
					State = (int)SoeEntityState.Active
				};
				SetCreatedProperties(userReplacement);
				entities.UserReplacement.AddObject(userReplacement);

				#endregion
			}
			else
			{
				#region Update

				if (userReplacementDTO.ReplacementUserId == 0 || userReplacementDTO.StartDate == null)
				{
					//Set deleted
					ChangeEntityState(entities, userReplacement, SoeEntityState.Deleted, false);
				}
				else
				{
					// Update UserReplacement
					if (userReplacement != null)
					{
						userReplacement.OriginUserId = userReplacementDTO.OriginUserId;
						userReplacement.ReplacementUserId = userReplacementDTO.ReplacementUserId;
						userReplacement.StartDate = userReplacementDTO.StartDate;
						userReplacement.StopDate = userReplacementDTO.StopDate;
						userReplacement.State = (int)userReplacementDTO.State;
					}
				}
				SetModifiedProperties(userReplacement);

				#endregion
			}

			return SaveChanges(entities, transaction);
		}

		#endregion

	}
}
