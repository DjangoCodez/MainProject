using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
	public class ContactManager : ManagerBase
	{
		#region Variables

		// Create a logger for use in this class
		private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static ImmutableHashSet<TermGroup_SysContactEComType> PhoneEComTypes { get; } = ImmutableHashSet.Create(
			TermGroup_SysContactEComType.PhoneHome,
			TermGroup_SysContactEComType.PhoneJob,
			TermGroup_SysContactEComType.PhoneMobile);

		#endregion

		#region Ctor

		public ContactManager(ParameterObject parameterObject) : base(parameterObject) { }

		#endregion

		#region Contact

		public Contact GetContact(CompEntities entities, int contactId, bool loadActor)
		{
			Contact contact = (from c in entities.Contact
							   where c.ContactId == contactId
							   select c).FirstOrDefault();

			if (contact != null && loadActor && !contact.ActorReference.IsLoaded)
				contact.ActorReference.Load();

			return contact;
		}

		public Contact GetContactFromActor(int actorId, bool loadActor = false, bool loadAllContactInfo = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Contact.NoTracking();
			return GetContactFromActor(entities, actorId, loadActor, loadAllContactInfo);
		}

		public Contact GetContactFromActor(CompEntities entities, int actorId, bool loadActor = false, bool loadAllContactInfo = false)
		{
			var query = (from c in entities.Contact
						 where c.Actor.ActorId == actorId
						 select c);

			if (loadActor)
				query = query.Include("Actor");
			if (loadAllContactInfo)
			{
				query = query.Include("ContactAddress.ContactAddressRow");
				query = query.Include("ContactECom");
			}

			return query.FirstOrDefault();
		}

		public List<Contact> GetContactsFromActors(CompEntities entities, List<int> actorIds, bool loadActor, bool loadAddresses)
		{
			var query = (from c in entities.Contact
						 where actorIds.Contains(c.Actor.ActorId)
						 select c);

			if (loadActor)
				query = query.Include("Actor");
			if (loadAddresses)
			{
				query = query.Include("ContactAddress.ContactAddressRow");
				query = query.Include("ContactECom");
			}

			return query.ToList();
		}

		public Contact GetContactAndEcomFromActor(CompEntities entities, int actorId)
		{
			return (from c in entities.Contact.Include("ContactEcom")
					where c.Actor.ActorId == actorId
					select c).FirstOrDefault();
		}

		public int GetContactIdFromActorId(CompEntities entities, int customerId)
		{
			return (from c in entities.Contact
					where c.Actor.ActorId == customerId &&
					c.State == (int)SoeEntityState.Active
					select c.ContactId).FirstOrDefault();
		}

		public int GetContactIdFromActorId(int customerId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Contact.NoTracking();
			return GetContactIdFromActorId(entities, customerId);
		}

		public ActionResult AddContact(Contact contact, int actorId)
		{
			if (contact == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "Contact");

			using (CompEntities entities = new CompEntities())
			{
				contact.Actor = ActorManager.GetActor(entities, actorId, false);
				if (contact.Actor == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "Actor");

				return AddEntityItem(entities, contact, "Contact");
			}
		}

		public ActionResult UpdateContact(Contact contact)
		{
			if (contact == null || contact.Actor == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "Contact");

			using (CompEntities entities = new CompEntities())
			{
				Contact orginalContact = GetContactFromActor(entities, contact.Actor.ActorId, loadActor: true);
				if (orginalContact == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "Contact");

				return UpdateEntityItem(entities, orginalContact, contact, "Contact");
			}
		}

		public ActionResult SaveContact(int actorId)
		{
			ActionResult result;

			//Get Contact
			Contact contact = GetContactFromActor(actorId, loadActor: true);
			if (contact == null)
			{
				//Add Contact
				contact = new Contact()
				{
					SysContactTypeId = (int)TermGroup_SysContactType.Company,
				};
				result = AddContact(contact, actorId);
			}
			else
			{
				//Update Contact
				result = UpdateContact(contact);
			}

			result.Value = contact;

			return result;
		}

		public ActionResult SaveContact(CompEntities entities, int actorId, TermGroup_SysContactType sysContactType, bool saveChanges)
		{
			ActionResult result = new ActionResult(true);

			//Get Contact
			Contact contact = GetContactFromActor(entities, actorId, loadActor: true);
			if (contact == null)
			{
				//Add Contact
				contact = new Contact()
				{
					SysContactTypeId = (int)sysContactType,
				};
				SetCreatedProperties(contact);

				//Set Actor
				contact.Actor = ActorManager.GetActor(entities, actorId, false);
				if (contact.Actor == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "Actor");
			}
			else
			{
				//Update Contact
				SetModifiedProperties(contact);
			}

			if (saveChanges)
				SaveChanges(entities);

			result.Value = contact;
			return result;
		}

		public ActionResult SaveDemoContact(int actorId)
		{
			using (CompEntities entites = new CompEntities())
			{
				#region Prereq

				Contact contact = GetContactFromActor(entites, actorId, loadActor: true);
				if (contact == null)
					return new ActionResult((int)ActionResultSave.EntityIsNull, "ContactAddress");
				if (contact.Actor == null)
					return new ActionResult((int)ActionResultSave.EntityIsNull, "Actor");

				//Addresses
				string demoStreetName = GetText(5555, "Demogatan");
				string demoPostalCode = "123 45";
				string demoPostalAddress = GetText(5556, "Demostaden");
				string demoCountry = GetText(5557, "Sverige");
				string demoEntranceCode = "1234";

				//ECom
				string demoPhoneJob = "00-1234501";
				string demoFax = "00-1234502";
				string demoEmail = "info@demo.com";
				string demoWeb = "www.demo.com";

				//PaymentInformation
				string demoPaymentBG = "1234-1234";
				string demoPaymentPG = "123456-0";
				string demoPaymentBank = "12345678";
				string demoPaymentBIC = "12345678";

				#endregion

				#region Adresses

				#region Distribution

				ContactAddress contactAddressDistribution = new ContactAddress()
				{
					Contact = contact,
					Name = GetText((int)TermGroup_SysContactAddressType.Distribution, (int)TermGroup.SysContactAddressType),
					SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.Distribution,
				};
				SetCreatedProperties(contactAddressDistribution);

				AddContactAddressRowToContactAddress(contactAddressDistribution, TermGroup_SysContactAddressRowType.Address, $"{demoStreetName} {1}");
				AddContactAddressRowToContactAddress(contactAddressDistribution, TermGroup_SysContactAddressRowType.PostalCode, demoPostalCode);
				AddContactAddressRowToContactAddress(contactAddressDistribution, TermGroup_SysContactAddressRowType.PostalAddress, demoPostalAddress);
				AddContactAddressRowToContactAddress(contactAddressDistribution, TermGroup_SysContactAddressRowType.Country, demoCountry);

				#endregion

				#region Visiting

				ContactAddress contactAddressVisiting = new ContactAddress()
				{
					Contact = contact,
					Name = GetText((int)TermGroup_SysContactAddressType.Visiting, (int)TermGroup.SysContactAddressType),
					SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.Visiting,
				};
				SetCreatedProperties(contactAddressVisiting);

				AddContactAddressRowToContactAddress(contactAddressVisiting, TermGroup_SysContactAddressRowType.StreetAddress, $"{demoStreetName} {2}");
				AddContactAddressRowToContactAddress(contactAddressVisiting, TermGroup_SysContactAddressRowType.PostalCode, demoPostalCode);
				AddContactAddressRowToContactAddress(contactAddressVisiting, TermGroup_SysContactAddressRowType.PostalAddress, demoPostalAddress);
				AddContactAddressRowToContactAddress(contactAddressVisiting, TermGroup_SysContactAddressRowType.Country, demoCountry);
				AddContactAddressRowToContactAddress(contactAddressVisiting, TermGroup_SysContactAddressRowType.EntranceCode, demoEntranceCode);

				#endregion

				#region Billing

				ContactAddress contactAddressBilling = new ContactAddress()
				{
					Contact = contact,
					Name = GetText((int)TermGroup_SysContactAddressType.Billing, (int)TermGroup.SysContactAddressType),
					SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.Billing,
				};
				SetCreatedProperties(contactAddressBilling);

				AddContactAddressRowToContactAddress(contactAddressBilling, TermGroup_SysContactAddressRowType.Address, $"{demoStreetName} {3}");
				AddContactAddressRowToContactAddress(contactAddressBilling, TermGroup_SysContactAddressRowType.PostalCode, demoPostalCode);
				AddContactAddressRowToContactAddress(contactAddressBilling, TermGroup_SysContactAddressRowType.PostalAddress, demoPostalAddress);
				AddContactAddressRowToContactAddress(contactAddressBilling, TermGroup_SysContactAddressRowType.Country, demoCountry);

				#endregion

				#region Delivery

				ContactAddress contactAddressDelivery = new ContactAddress()
				{
					Contact = contact,
					Name = GetText((int)TermGroup_SysContactAddressType.Delivery, (int)TermGroup.SysContactAddressType),
					SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.Delivery,
				};
				SetCreatedProperties(contactAddressDelivery);

				AddContactAddressRowToContactAddress(contactAddressDelivery, TermGroup_SysContactAddressRowType.Address, $"{demoStreetName} {4}");
				AddContactAddressRowToContactAddress(contactAddressDelivery, TermGroup_SysContactAddressRowType.PostalCode, demoPostalCode);
				AddContactAddressRowToContactAddress(contactAddressDelivery, TermGroup_SysContactAddressRowType.PostalAddress, demoPostalAddress);
				AddContactAddressRowToContactAddress(contactAddressDelivery, TermGroup_SysContactAddressRowType.Country, demoCountry);

				#endregion

				#region BoardHQ

				ContactAddress contactAddressBoardHQ = new ContactAddress()
				{
					Contact = contact,
					Name = GetText((int)TermGroup_SysContactAddressType.BoardHQ, (int)TermGroup.SysContactAddressType),
					SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.BoardHQ,
				};
				SetCreatedProperties(contactAddressBoardHQ);

				AddContactAddressRowToContactAddress(contactAddressBoardHQ, TermGroup_SysContactAddressRowType.Address, $"{demoStreetName} {5}");
				AddContactAddressRowToContactAddress(contactAddressBoardHQ, TermGroup_SysContactAddressRowType.PostalCode, demoPostalCode);

				#endregion

				#endregion

				#region ECom

				ContactECom contactEComPhoneJob = new ContactECom()
				{
					Contact = contact,
					Name = GetText((int)TermGroup_SysContactEComType.PhoneJob, (int)TermGroup.SysContactEComType),
					SysContactEComTypeId = (int)TermGroup_SysContactEComType.PhoneJob,
					Text = demoPhoneJob,
				};
				SetCreatedProperties(contactEComPhoneJob);

				ContactECom contactEComFax = new ContactECom()
				{
					Contact = contact,
					Name = GetText((int)TermGroup_SysContactEComType.Fax, (int)TermGroup.SysContactEComType),
					SysContactEComTypeId = (int)TermGroup_SysContactEComType.Fax,
					Text = demoFax,
				};
				SetCreatedProperties(contactEComFax);

				ContactECom contactEComEmail = new ContactECom()
				{
					Contact = contact,
					Name = GetText((int)TermGroup_SysContactEComType.Email, (int)TermGroup.SysContactEComType),
					SysContactEComTypeId = (int)TermGroup_SysContactEComType.Email,
					Text = demoEmail,
				};
				SetCreatedProperties(contactEComEmail);

				ContactECom contactEComWeb = new ContactECom()
				{
					Contact = contact,
					Name = GetText((int)TermGroup_SysContactEComType.Web, (int)TermGroup.SysContactEComType),
					SysContactEComTypeId = (int)TermGroup_SysContactEComType.Web,
					Text = demoWeb,
				};
				SetCreatedProperties(contactEComWeb);

				#endregion

				#region Payment information

				PaymentInformation paymentInformationBG = new PaymentInformation()
				{
					Actor = contact.Actor,
					DefaultSysPaymentTypeId = (int)TermGroup_SysPaymentType.BG,
				};
				SetCreatedProperties(paymentInformationBG);

				PaymentManager.AddPaymentInformationRowToPaymentInformation(paymentInformationBG, TermGroup_SysPaymentType.BG, demoPaymentBG, true);
				PaymentManager.AddPaymentInformationRowToPaymentInformation(paymentInformationBG, TermGroup_SysPaymentType.PG, demoPaymentPG, false);
				PaymentManager.AddPaymentInformationRowToPaymentInformation(paymentInformationBG, TermGroup_SysPaymentType.Bank, demoPaymentBank, false);
				PaymentManager.AddPaymentInformationRowToPaymentInformation(paymentInformationBG, TermGroup_SysPaymentType.BIC, demoPaymentBIC, false);

				#endregion

				return SaveChanges(entites, null);
			}
		}

		#endregion

		#region ContactAdress

		public ContactAddress CreateContactAddress(CompEntities entities, Contact contact, TermGroup_SysContactAddressType type, string name)
		{
			var contactAddress = new ContactAddress
			{
				Name = string.IsNullOrEmpty(name) ? GetText((int)type, (int)TermGroup.SysContactAddressType) : name,

				//Set FK
				SysContactAddressTypeId = (int)type,

				//Set references
				Contact = contact,
			};
			SetCreatedProperties(contactAddress);
			entities.ContactAddress.AddObject(contactAddress);
			return contactAddress;
		}

		public List<ContactAddress> GetContactAddresses(int contactId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactAddress.NoTracking();
			return GetContactAddresses(entities, contactId);
		}

		public List<ContactAddress> GetContactAddresses(CompEntities entities, int contactId)
		{
			return (from ca in entities.ContactAddress
						.Include("ContactAddressRow")
						.Include("Contact")
					where ca.Contact.ContactId == contactId
					select ca).ToList();
		}

		public List<ContactAddress> GetContactAddresses(int contactId, TermGroup_SysContactAddressType addressType, bool addEmptyRow, bool includeCareOf = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactAddress.NoTracking();
			return GetContactAddresses(entities, contactId, addressType, addEmptyRow, includeCareOf);
		}

		public List<ContactAddress> GetContactAddresses(CompEntities entities, int contactId, TermGroup_SysContactAddressType addressType, bool addEmptyRow, bool includeCareOf = false)
		{
			int type = (int)addressType;

			List<ContactAddress> addresses = new List<ContactAddress>();
			if (addEmptyRow)
			{
				addresses.Add(new ContactAddress()
				{
					ContactAddressId = 0,
					Address = " "
				});
			}

			addresses.AddRange((from ca in entities.ContactAddress.Include("ContactAddressRow")
								where ca.Contact.ContactId == contactId &&
								ca.SysContactAddressTypeId == type
								select ca).ToList());

			FormatAddresses(addresses, includeCareOf);

			return addresses;
		}

		public List<ContactAddress> GetContactAddresses(CompEntities entities, List<int> contactAddressIds, bool loadContact, bool loadContactAddressRow = false)
		{
			IQueryable<ContactAddress> query = entities.ContactAddress;

			if (loadContact)
				query = query.Include("Contact");
			if (loadContactAddressRow)
				query = query.Include("ContactAddressRow");

			return (from ca in query
					where
					contactAddressIds.Contains(ca.ContactAddressId)
					select ca).ToList<ContactAddress>();
		}

		public List<ContactAddress> GetContactAddressesFromActorCompany(int actorCompanyId)
		{
			Contact contact = ContactManager.GetContactFromActor(actorCompanyId, true, true);
			return contact.ContactAddress.Where(s => s.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Distribution).ToList();
		}

		public List<ContactAdressIODTO> ContactAddressesToIODTO(List<ContactAddress> contactAddress)
		{
			var result = new List<ContactAdressIODTO>();

			foreach (var address in contactAddress)
			{
				var dto = new ContactAdressIODTO
				{
					ContactAddressId = address.ContactAddressId
				};

				foreach (var addressRow in address.ContactAddressRow)
				{
					switch (addressRow.SysContactAddressRowTypeId)
					{
						case (int)TermGroup_SysContactAddressRowType.Name:
							dto.Name = addressRow.Text;
							break;
						case (int)TermGroup_SysContactAddressRowType.Address:
						case (int)TermGroup_SysContactAddressRowType.StreetAddress:
							dto.Address = addressRow.Text;
							break;
						case (int)TermGroup_SysContactAddressRowType.AddressCO:
							dto.CoAddress = addressRow.Text;
							break;
						case (int)TermGroup_SysContactAddressRowType.PostalCode:
							dto.PostalCode = addressRow.Text;
							break;
						case (int)TermGroup_SysContactAddressRowType.PostalAddress:
							dto.PostalAddress = addressRow.Text;
							break;
						case (int)TermGroup_SysContactAddressRowType.Country:
							dto.Country = addressRow.Text;
							break;
					}
				}
				result.Add(dto);
			}

			return result;
		}

		public Dictionary<int, string> GetContactAddressFirst(CompEntities entities, int contactId, List<int> addressIds)
		{
			var result = new Dictionary<int, string>();

			addressIds = addressIds.Where(x => x != 0).ToList();
			if (addressIds.Any())
			{
				var adresslist = (from ca in entities.ContactAddress.Include("ContactAddressRow")
								  where ca.Contact.ContactId == contactId &&
								   addressIds.Contains(ca.ContactAddressId)
								  select ca);

				foreach (var id in addressIds)
				{
					var first = adresslist.FirstOrDefault(a => a.ContactAddressId == id);
					if (first != null)
					{
						FormatAddress(first, false);
						result.Add(id, first.Address);
					}
				}
			}
			return result;
		}

		public ContactAddress GetContactAddress(int contactAddressId, bool loadContact, bool loadContactAddressRow = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactAddress.NoTracking();
			return GetContactAddress(entities, contactAddressId, loadContact, loadContactAddressRow);
		}

		public ContactAddress GetContactAddress(CompEntities entities, int contactAddressId, bool loadContact, bool loadContactAddressRow = false)
		{
			IQueryable<ContactAddress> query = entities.ContactAddress;
			if (loadContact)
				query = query.Include("Contact");
			if (loadContactAddressRow)
				query = query.Include("ContactAddressRow");

			return (from ca in query
					where ca.ContactAddressId == contactAddressId
					select ca).FirstOrDefault();
		}

		public ContactAddress GetContactAddress(CompEntities entities, int contactId, int contactAddressId, bool loadContact, bool loadContactAddressRow = false)
		{
			IQueryable<ContactAddress> query = entities.ContactAddress;

			if (loadContact)
				query = query.Include("Contact");
			if (loadContactAddressRow)
				query = query.Include("ContactAddressRow");

			return (from ca in query
					where ((ca.Contact.ContactId == contactId) &&
					(ca.ContactAddressId == contactAddressId))
					select ca).FirstOrDefault<ContactAddress>();
		}

		public ActionResult AddContactAddress(ContactAddress contactAddr)
		{
			if (contactAddr == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "ContactAddress");

			using (CompEntities entities = new CompEntities())
			{
				var contactAddress = new ContactAddress()
				{
					SysContactAddressTypeId = contactAddr.SysContactAddressTypeId,
					Name = contactAddr.Name,
				};

				contactAddress.Contact = GetContact(entities, contactAddr.Contact.ContactId, false);
				if (contactAddress.Contact == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "Contact");

				var result = AddEntityItem(entities, contactAddress, "ContactAddress");
				if (result.Success)
					result.IntegerValue = contactAddress.ContactAddressId;

				return result;
			}
		}

		public ActionResult DeleteContactAddress(ContactAddress contactAddress, int contactId)
		{
			if (contactAddress == null)
				return new ActionResult((int)ActionResultDelete.EntityIsNull, "ContactAddress");

			using (CompEntities entities = new CompEntities())
			{
				ContactAddress orginalContactAddress = GetContactAddress(entities, contactId, contactAddress.ContactAddressId, false);
				if (orginalContactAddress == null)
					return new ActionResult((int)ActionResultDelete.EntityNotFound, "ContactAddress");

				return DeleteEntityItem(entities, orginalContactAddress);
			}
		}

		public ActionResult ImportContactAdresses(CompEntities entities, Contact contact, List<ContactAdressIODTO> addresses, TermGroup_SysContactAddressType type)
		{
			if (contact == null || addresses.IsNullOrEmpty())
				return new ActionResult(false);

			List<ContactAddress> contactAddresses = ContactManager.GetContactAddresses(entities, contact.ContactId);

			foreach (ContactAdressIODTO address in addresses)
			{
				ContactAddress contactAddress = null;

				if (address.ContactAddressId.GetValueOrDefault() > 0)
					contactAddress = contactAddresses.FirstOrDefault(i => i.ContactAddressId == address.ContactAddressId.Value && i.SysContactAddressTypeId == (int)type);
				if (contactAddress == null)
				{
					contactAddress = CreateContactAddress(entities, contact, type, address.Name);
					contactAddresses.Add(contactAddress);
				}

				UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.Address, contactAddress, address.Address, null, false);
				UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.AddressCO, contactAddress, address.CoAddress, null, false);
				UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalCode, contactAddress, address.PostalCode, null, false);
				UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.PostalAddress, contactAddress, address.PostalAddress, null, false);
				UpdateContactAddressRow(entities, TermGroup_SysContactAddressRowType.Country, contactAddress, address.Country, null, false);
			}

			return SaveChanges(entities);
		}

		public void AddContactAddressRowToContactAddress(ContactAddress contactAddress, TermGroup_SysContactAddressRowType sysContactAddressRowType, string text)
		{
			ContactAddressRow contactAddressRow = new ContactAddressRow()
			{
				ContactAddress = contactAddress,
				SysContactAddressRowTypeId = (int)sysContactAddressRowType,
				Text = text
			};
			SetCreatedProperties(contactAddressRow);
			contactAddress.ContactAddressRow.Add(contactAddressRow);
		}

		public void FormatAddresses(List<ContactAddress> addresses, bool includeCareOf = false)
		{
			if (addresses.IsNullOrEmpty())
				return;

			foreach (ContactAddress address in addresses)
			{
				FormatAddress(address, includeCareOf);
			}
		}

		public void FormatAddress(ContactAddress address, bool includeCareOf = false)
		{
			if (address.ContactAddressId != 0)
			{
				if (includeCareOf)
				{
					ContactAddressRow name = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Name);
					ContactAddressRow careOf = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO);
					ContactAddressRow addr = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address);
					ContactAddressRow streetAddr = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.StreetAddress);
					ContactAddressRow postalCode = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode);
					ContactAddressRow postalAddr = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress);

					address.Address = String.Format("{0} {1}, {2} {3} {4} {5}",
						name?.Text ?? String.Empty,
						careOf?.Text ?? String.Empty,
						addr?.Text ?? String.Empty,
						streetAddr?.Text ?? String.Empty,
						postalCode?.Text ?? String.Empty,
						postalAddr?.Text ?? String.Empty).Trim();
				}
				else
				{
					ContactAddressRow name = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Name);
					ContactAddressRow addr = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address);
					ContactAddressRow streetAddr = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.StreetAddress);
					ContactAddressRow postalCode = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode);
					ContactAddressRow postalAddr = address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress);

					address.Address = String.Format("{0} {1}, {2} {3} {4}",
						name?.Text ?? String.Empty,
						addr?.Text ?? String.Empty,
						streetAddr?.Text ?? String.Empty,
						postalCode?.Text ?? String.Empty,
						postalAddr?.Text ?? String.Empty).Trim();
				}
			}
		}


		public ActionResult AddOrFindActorAddress(CompEntities entities, TermGroup_SysContactAddressType type, int actorId, ContactAdressIODTO address)
		{
			int contactId = GetContactIdFromActorId(entities, actorId);
			var addresses = GetContactAddresses(entities, contactId, type, false);
			var contactAddressId = addresses.ReturnExistingContactAddressId(TermGroup_SysContactAddressRowType.Address, address.Address);

			if (!contactAddressId.HasValue)
			{
				var result = AddAddress(entities, type, contactId, address);
				return result;
			}
			else
			{
				return new ActionResult(true)
				{
					IntegerValue = contactAddressId.Value
				};
			}
		}

		public ActionResult AddAddress(CompEntities entities, TermGroup_SysContactAddressType addressType, int contactId, ContactAdressIODTO address)
		{
			var contact = ContactManager.GetContact(entities, contactId, false);
			var ca = new ContactAddress()
			{
				Contact = contact,
				SysContactAddressTypeId = (int)addressType,
			};

			if (ca.ContactAddressRow == null)
				ca.ContactAddressRow = new EntityCollection<ContactAddressRow>();

			if (!string.IsNullOrEmpty(address.Address))
			{
				ca.ContactAddressRow.Add(
						new ContactAddressRow()
						{
							SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.Address,
							Text = address.Address,
						});
			}
			if (!string.IsNullOrEmpty(address.PostalCode))
			{
				ca.ContactAddressRow.Add(
					new ContactAddressRow()
					{
						SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.PostalCode,
						Text = address.PostalCode,
					});
			}
			if (!string.IsNullOrEmpty(address.CoAddress))
			{
				ca.ContactAddressRow.Add(
						new ContactAddressRow()
						{
							SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.AddressCO,
							Text = address.CoAddress,
						});
			}
			if (!string.IsNullOrEmpty(address.PostalAddress))
			{
				ca.ContactAddressRow.Add(
						new ContactAddressRow()
						{
							SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.PostalAddress,
							Text = address.PostalAddress,
						});
			}

			if (!string.IsNullOrEmpty(address.Country))
			{
				ca.ContactAddressRow.Add(
						new ContactAddressRow()
						{
							SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.Country,
							Text = address.Country,
						});
			}

			if (!string.IsNullOrEmpty(address.Name))
			{
				ca.ContactAddressRow.Add(
						new ContactAddressRow()
						{
							SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.Name,
							Text = address.Name,
						});
			}

			var result = SaveChanges(entities);
			result.IntegerValue = ca.ContactAddressId;
			return result;
		}

		#endregion

		#region ContactAdressRow

		public ContactAddressRow CreateContactAddressRow(CompEntities entities, TermGroup_SysContactAddressRowType type, string text)
		{
			var contactAddressRow = new ContactAddressRow()
			{
				Text = text,

				//Set FK
				SysContactAddressRowTypeId = (int)type,
			};
			SetCreatedProperties(contactAddressRow);
			entities.ContactAddressRow.AddObject(contactAddressRow);


			return contactAddressRow;
		}

		public void ParseContactHQAddress(ContactAddress contactAddress, TermGroup_SysContactAddressType addressType, out string addressCity, out string addressCountry)
		{
			ParseContactAddress(contactAddress, addressType, out _, out _, out _, out _, out addressCity, out addressCountry);
		}

		public void ParseContactAddress(ContactAddress contactAddress, TermGroup_SysContactAddressType addressType, out string addressName, out string addressAddress, out string addressCo, out string addressPostNr, out string addressCity, out string addressCountry)
		{
			addressName = string.Empty;
			addressAddress = string.Empty;
			addressCo = string.Empty;
			addressPostNr = string.Empty;
			addressCity = string.Empty;
			addressCountry = string.Empty;

			if (contactAddress == null)
				return;

			var customerBillingAddresseRows = contactAddress.SysContactAddressTypeId == (int)addressType && !contactAddress.ContactAddressRow.IsNullOrEmpty() ? contactAddress.ContactAddressRow.ToList() : ContactManager.GetContactAddressRows(contactAddress, (int)addressType);

			foreach (var billingAddressRow in customerBillingAddresseRows)
			{
				switch (billingAddressRow.SysContactAddressRowTypeId)
				{
					case (int)TermGroup_SysContactAddressRowType.Name:
						addressName = billingAddressRow.Text;
						break;
					case (int)TermGroup_SysContactAddressRowType.Address:
					case (int)TermGroup_SysContactAddressRowType.StreetAddress:
						addressAddress = billingAddressRow.Text;
						break;
					case (int)TermGroup_SysContactAddressRowType.AddressCO:
						addressCo = billingAddressRow.Text;
						break;
					case (int)TermGroup_SysContactAddressRowType.PostalCode:
						addressPostNr = billingAddressRow.Text;
						break;
					case (int)TermGroup_SysContactAddressRowType.PostalAddress:
						addressCity = billingAddressRow.Text;
						break;
					case (int)TermGroup_SysContactAddressRowType.Country:
						addressCountry = billingAddressRow.Text;
						break;
				}
			}
		}

		public List<ContactAddressRow> GetContactAddressRows(int contactId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactAddress.NoTracking();
			return GetContactAddressRows(entities, contactId);
		}

		public List<ContactAddressRow> GetContactAddressRows(CompEntities entities, int contactId)
		{
			return (from car in entities.ContactAddressRow
						.Include("ContactAddress")
					where car.ContactAddress.Contact.ContactId == contactId
					select car).ToList();
		}

		public List<ContactAddressRow> GetContactAddressRows(int contactId, int sysContactAddressTypeId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactAddress.NoTracking();
			return GetContactAddressRows(entities, contactId, sysContactAddressTypeId);
		}

		public List<ContactAddressRow> GetContactAddressRows(CompEntities entities, int contactId, int sysContactAddressTypeId)
		{
			return (from car in entities.ContactAddressRow
						.Include("ContactAddress")
					where car.ContactAddress.Contact.ContactId == contactId &&
					car.ContactAddress.SysContactAddressTypeId == sysContactAddressTypeId
					select car).ToList();
		}


		public List<ContactAddressRow> GetContactAddressRows(CompEntities entities, List<int> contactIds)
		{
			return (from car in entities.ContactAddressRow
						.Include("ContactAddress.Contact")
					where contactIds.Contains(car.ContactAddress.Contact.ContactId)
					select car).ToList();
		}

		public List<ContactAddressRow> GetContactAddressRows(ContactAddress contactAddress, int sysContactAddressTypeId)
		{
			if (contactAddress?.ContactAddressRow == null)
				return new List<ContactAddressRow>();

			return (from car in contactAddress.ContactAddressRow
					where car.ContactAddress.SysContactAddressTypeId == sysContactAddressTypeId
					select car).ToList();
		}

		public ContactAddressRow GetContactAddressRow(CompEntities entities, int contactAddressRowNr, bool loadContactAddress)
		{
			ContactAddressRow contactAddressRow = (from car in entities.ContactAddressRow
												   where car.RowNr == contactAddressRowNr
												   select car).FirstOrDefault();

			if (contactAddressRow != null && loadContactAddress && !contactAddressRow.ContactAddressReference.IsLoaded)
				contactAddressRow.ContactAddressReference.Load();

			return contactAddressRow;
		}

		public string GetContactAddressRowText(ContactAddress address, TermGroup_SysContactAddressRowType rowType)
		{
			ContactAddressRow row = address?.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)rowType);
			return row?.Text ?? string.Empty;
		}

		private bool TryUpdateTextOnContactAddressRow(ContactAddressRow contactAddressRow, string text)
		{
			if (!String.IsNullOrEmpty(text) && contactAddressRow != null && contactAddressRow.Text != text)
			{
				contactAddressRow.Text = text;
				return true;
			}

			return false;
		}

		public ActionResult AddContactAddressRow(ContactAddressRow car, ContactAddress contactAddress)
		{
			if (car == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "ContactAddressRow");
			if (contactAddress == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "ContactAddress");
			if (contactAddress.Contact == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "Contact");

			using (CompEntities entities = new CompEntities())
			{
				var contactAddressRow = new ContactAddressRow()
				{
					SysContactAddressRowTypeId = car.SysContactAddressRowTypeId,
					Text = car.Text
				};

				contactAddressRow.ContactAddress = GetContactAddress(entities, contactAddress.Contact.ContactId, contactAddress.ContactAddressId, true);
				if (contactAddressRow.ContactAddress == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "ContactAddress");

				return AddEntityItem(entities, contactAddressRow, "ContactAddressRow");
			}
		}

		public ActionResult AddContactAddressRow(CompEntities entities, TermGroup_SysContactAddressRowType sysContactAddressRowTypeId, string text, ContactAddress contactAddress, TransactionScope transaction, bool save = false)
		{
			ActionResult result = new ActionResult(true);

			//No need to save empty, return success
			if (string.IsNullOrEmpty(text))
				return new ActionResult(true);

			var contactAddressRow = new ContactAddressRow()
			{
				SysContactAddressRowTypeId = (int)sysContactAddressRowTypeId,
				Text = text,

				//Set references
				ContactAddress = contactAddress,
			};
			SetCreatedProperties(contactAddressRow);
			entities.ContactAddressRow.AddObject(contactAddressRow);

			if (save)
				result = SaveChanges(entities, transaction);

			return result;
		}

		public ActionResult UpdateContactAddressRow(CompEntities entities, TermGroup_SysContactAddressRowType addressRowType, ContactAddress contactAddress, string text, TransactionScope transaction, bool save = false)
		{
			ActionResult result = new ActionResult(true);

			var contactAddressRow = contactAddress.ContactAddressRow.FirstOrDefault(i => i.SysContactAddressRowTypeId == (int)addressRowType);
			if (contactAddressRow != null)
			{
				if (contactAddressRow.Text != text)
				{
					if (string.IsNullOrEmpty(text))
					{
						contactAddress.ContactAddressRow.Remove(contactAddressRow);
						entities.ContactAddressRow.DeleteObject(contactAddressRow);
					}
					else
					{
						contactAddressRow.Text = text;
						SetModifiedProperties(contactAddressRow);
					}
					if (save)
						result = SaveChanges(entities, transaction);
				}
			}
			else if (!string.IsNullOrEmpty(text))
			{
				result = AddContactAddressRow(entities, addressRowType, text, contactAddress, transaction, save);
			}

			return result;
		}

		public ActionResult UpdateContactAddressRow(ContactAddressRow contactAddressRow)
		{
			if (contactAddressRow == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "ContactAddressRow");

			using (CompEntities entities = new CompEntities())
			{
				ContactAddressRow orginalContactAddressRow = GetContactAddressRow(entities, contactAddressRow.RowNr, false);
				if (orginalContactAddressRow == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "ContactAddressRow");

				return UpdateEntityItem(entities, orginalContactAddressRow, contactAddressRow, "ContactAddressRow");
			}
		}

		public ActionResult DeleteContactAddressRow(ContactAddressRow contactAddressRow)
		{
			if (contactAddressRow == null)
				return new ActionResult((int)ActionResultDelete.EntityIsNull, "ContactAddressRow");

			using (CompEntities entities = new CompEntities())
			{
				ContactAddressRow orginalContactAddressRow = GetContactAddressRow(entities, contactAddressRow.RowNr, false);
				if (orginalContactAddressRow == null)
					return new ActionResult((int)ActionResultDelete.EntityNotFound, "ContactAddressRow");

				return DeleteEntityItem(entities, orginalContactAddressRow);
			}
		}

		#endregion

		#region CustomerContactAddressRowView

		public List<CustomerContactAddressRowView> GetCustomerContactAddressRowView(CompEntities entities, int actorCompanyId)
		{
			return (from v in entities.CustomerContactAddressRowView
					where v.ActorCompanyId == actorCompanyId
					select v).ToList();
		}

		public CustomerContactAddressRowView FilterCustomerContactAddressRowViewOnAddress(List<CustomerContactAddressRowView> adressItems, int contactAddressId, int sysContactAddressTypeId, int sysContactAddressRowTypeId)
		{
			return (from v in adressItems
					where v.ContactAddressId == contactAddressId &&
					v.SysContactAddressTypeId == sysContactAddressTypeId &&
					v.SysContactAddressRowTypeId == sysContactAddressRowTypeId
					select v).FirstOrDefault();
		}

		public CustomerContactAddressRowView FilterCustomerContactAddressRowViewOnActor(List<CustomerContactAddressRowView> adressItems, int actorId, int sysContactAddressTypeId, int sysContactAddressRowTypeId)
		{
			return (from v in adressItems
					where v.ActorId == actorId &&
					v.SysContactAddressTypeId == sysContactAddressTypeId &&
					v.SysContactAddressRowTypeId == sysContactAddressRowTypeId
					select v).FirstOrDefault();
		}

		#endregion

		#region ContactAddressItem

		public List<ContactAddressItem> GetContactAddressItems(int actorId, ContactAddressItemType filterType = ContactAddressItemType.Unknown, bool? address = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Contact.NoTracking();
			return GetContactAddressItems(entities, actorId, filterType, address);
		}

		public List<ContactAddressItem> GetContactAddressItems(CompEntities entities, int actorId, ContactAddressItemType filterType = ContactAddressItemType.Unknown, bool? address = null, List<Contact> preLoadedContacts = null)
		{
			if (actorId == 0)
				return new List<ContactAddressItem>();

			List<ContactAddressItem> items = new List<ContactAddressItem>();

			Contact contact = preLoadedContacts.IsNullOrEmpty() ? (from c in entities.Contact
								   .Include("ContactECom")
								   .Include("ContactAddress.ContactAddressRow")
																   where c.Actor.ActorId == actorId
																   select c).FirstOrDefault() : preLoadedContacts.FirstOrDefault(w => w.Actor.ActorId == actorId);

			if (contact != null)
			{
				#region ContactAddress

				if (!address.HasValue || address.Value)
				{
					foreach (ContactAddress contactAddress in contact.ContactAddress.Where(i => i.SysContactAddressTypeId > 0))
					{
						ContactAddressItemType type = (ContactAddressItemType)contactAddress.SysContactAddressTypeId;
						if (filterType != ContactAddressItemType.Unknown && filterType != type)
							continue;

						ContactAddressItem item = new ContactAddressItem()
						{
							ContactId = contact.ContactId,
							ContactAddressId = contactAddress.ContactAddressId,
							SysContactAddressTypeId = contactAddress.SysContactAddressTypeId,
							ContactAddressItemType = type,
							TypeName = base.GetText(contactAddress.SysContactAddressTypeId, (int)TermGroup.SysContactAddressType),

							Name = contactAddress.Name,
							IsSecret = contactAddress.IsSecret,
							IsAddress = true,

							AddressName = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.Name),
							Address = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.Address),
							AddressCO = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.AddressCO),
							PostalCode = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.PostalCode),
							PostalAddress = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.PostalAddress),
							StreetAddress = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.StreetAddress),
							EntranceCode = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.EntranceCode),
							Country = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.Country),
						};

						item.SetDisplayAddress();
						items.Add(item);
					}
				}

				#endregion

				#region ContactECom

				if (!address.HasValue || !address.Value)
				{
					foreach (ContactECom contactECom in contact.ContactECom.Where(i => i.SysContactEComTypeId > 0))
					{
						ContactAddressItemType type = (ContactAddressItemType)contactECom.SysContactEComTypeId;
						if (filterType != ContactAddressItemType.Unknown && filterType != type)
							continue;

						ContactAddressItem item = new ContactAddressItem()
						{
							ContactId = contact.ContactId,
							ContactEComId = contactECom.ContactEComId,
							SysContactAddressTypeId = contactECom.SysContactEComTypeId,
							SysContactEComTypeId = contactECom.SysContactEComTypeId,
							ContactAddressItemType = (ContactAddressItemType)(contactECom.SysContactEComTypeId + 10),
							TypeName = base.GetText(contactECom.SysContactEComTypeId, (int)TermGroup.SysContactEComType),

							Name = contactECom.Name,
							IsSecret = contactECom.IsSecret,
							IsAddress = false,

							EComText = contactECom.Text,
							EComDescription = contactECom.Description,
						};

						item.SetDisplayAddress();
						items.Add(item);
					}
				}

				#endregion
			}

			return items.OrderBy(i => i.ContactAddressItemType).ThenBy(i => i.Address).ToList();
		}

		public ContactAddressItem GetContactAddressItem(List<ContactAddressItem> contactItems, bool isAddress, TermGroup_SysContactEComType type, bool filterSecret = false)
		{
			if (contactItems == null)
				return null;

			if (filterSecret)
				return contactItems.FirstOrDefault(i => i.IsAddress == isAddress && i.SysContactEComTypeId == (int)type && !i.IsSecret);
			else
				return contactItems.FirstOrDefault(i => i.IsAddress == isAddress && i.SysContactEComTypeId == (int)type);
		}

		public List<ContactAddressItem> GetContactInfoForEmployee(int employeeId, int actorCompanyId, bool isForPlannning = true)
		{
			List<ContactAddressItem> items = new List<ContactAddressItem>();
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.Employee.NoTracking();
			entitiesReadOnly.Contact.NoTracking();

			#region Prereq

			Employee employee = (from e in entitiesReadOnly.Employee.Include("ContactPerson")
								 where e.EmployeeId == employeeId
								 select e).FirstOrDefault();

			if (employee == null)
				return items;

			#endregion

			#region Company settings

			(List<int> addressTypeIds, List<int> ecomTypeIds) = GetAddressAndEcomTypesForPlanning(actorCompanyId);

			#endregion

			List<ContactAddressItem> allContactAddressItems = GetContactAddressItems(employee.ContactPerson.ActorContactPersonId);

			if (isForPlannning)
			{
				foreach (ContactAddressItem item in allContactAddressItems)
				{
					if (!item.IsAddress && ecomTypeIds.Contains(item.SysContactEComTypeId))
						items.Add(item);
					else if (item.IsAddress && addressTypeIds.Contains(item.SysContactAddressTypeId))
						items.Add(item);
				}
			}
			else
				items = allContactAddressItems;

			return items;
		}

		public (List<int> addressTypeIds, List<int> ecomTypeIds) GetAddressAndEcomTypesForPlanning(int actorCompanyId)
		{
			List<int> addressTypeIds = new List<int>();
			string addressTypesString = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningContactAddressTypes, 0, actorCompanyId, 0);
			if (!string.IsNullOrEmpty(addressTypesString))
			{
				string[] addressTypesList = addressTypesString.Split(',');
				foreach (string item in addressTypesList)
				{
					if (int.TryParse(item, out int id))
						addressTypeIds.Add(id);
				}
			}

			List<int> ecomTypeIds = new List<int>();
			string ecomTypesString = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningContactEComTypes, 0, actorCompanyId, 0);
			if (!string.IsNullOrEmpty(ecomTypesString))
			{
				string[] ecomTypesList = ecomTypesString.Split(',');
				foreach (string item in ecomTypesList)
				{
					if (int.TryParse(item, out int id))
						ecomTypeIds.Add(id);
				}
			}

			return (addressTypeIds, ecomTypeIds);
		}

		public ActionResult SaveContactAddresses(List<ContactAddressItem> contactItems, int actorId, TermGroup_SysContactType contactType)
		{
			using (CompEntities entities = new CompEntities())
			{
				return SaveContactAddresses(entities, contactItems, actorId, contactType);
			}
		}

		/// <summary>
		/// Updates, saves or deletes contactsAddresses from a user. 
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="contactItems"></param>
		/// <param name="actorId">The actorId, creates a new Contact if this actor does not have a Contaact.</param>
		/// <param name="contactType"></param>
		/// <param name="transaction">Optional transaction, will not be used if null.</param>
		/// <returns></returns>
		public ActionResult SaveContactAddresses(CompEntities entities, List<ContactAddressItem> contactItems, int actorId, TermGroup_SysContactType contactType, TransactionScope transaction = null, SoeEntityType entityType = SoeEntityType.None)
		{
			ActionResult result = new ActionResult();

			#region Contact

			// Get contact
			Contact contact = GetContactFromActor(entities, actorId, loadAllContactInfo: true);
			if (contact == null)
			{
				// Get actor
				Actor actor = ActorManager.GetActor(entities, actorId, false);
				if (actor == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "Actor not found");

				// Create new Contact
				contact = new Contact()
				{
					Actor = actor,
					SysContactTypeId = (int)contactType
				};
				SetCreatedProperties(contact);
			}
			else
			{
				// Update existing contact
				SetModifiedProperties(contact);
			}

			#endregion

			if (contactItems != null)
			{
				#region ContactAddress

				// Update or delete existing addresses
				foreach (ContactAddress contactAddress in contact.ContactAddress.ToList())
				{
					ContactAddressItem item = contactItems.FirstOrDefault(c => c.ContactAddressId == contactAddress.ContactAddressId);
					if (item != null)
					{
						#region Update

						contactAddress.Name = item.Name;
						contactAddress.IsSecret = item.IsAddress ? item.AddressIsSecret : item.IsSecret;
						SetModifiedProperties(contactAddress);

						// Update/Delete existing ContactAddressRows
						foreach (ContactAddressRow row in contactAddress.ContactAddressRow.ToList())
						{
							bool rowUpdated = false;
							bool rowDeleted = false;
							switch ((TermGroup_SysContactAddressRowType)row.SysContactAddressRowTypeId)
							{
								case TermGroup_SysContactAddressRowType.Name:
									rowUpdated = TryUpdateTextOnContactAddressRow(row, item.AddressName);
									rowDeleted = String.IsNullOrEmpty(item.AddressName);
									break;
								case TermGroup_SysContactAddressRowType.Address:
									rowUpdated = TryUpdateTextOnContactAddressRow(row, item.Address);
									rowDeleted = String.IsNullOrEmpty(item.Address);
									break;
								case TermGroup_SysContactAddressRowType.AddressCO:
									rowUpdated = TryUpdateTextOnContactAddressRow(row, item.AddressCO);
									rowDeleted = String.IsNullOrEmpty(item.AddressCO);
									break;
								case TermGroup_SysContactAddressRowType.PostalAddress:
									rowUpdated = TryUpdateTextOnContactAddressRow(row, item.PostalAddress);
									rowDeleted = String.IsNullOrEmpty(item.PostalAddress);
									break;
								case TermGroup_SysContactAddressRowType.PostalCode:
									rowUpdated = TryUpdateTextOnContactAddressRow(row, item.PostalCode);
									rowDeleted = String.IsNullOrEmpty(item.PostalCode);
									break;
								case TermGroup_SysContactAddressRowType.StreetAddress:
									rowUpdated = TryUpdateTextOnContactAddressRow(row, item.StreetAddress);
									rowDeleted = String.IsNullOrEmpty(item.StreetAddress);
									break;
								case TermGroup_SysContactAddressRowType.EntranceCode:
									rowUpdated = TryUpdateTextOnContactAddressRow(row, item.EntranceCode);
									rowDeleted = String.IsNullOrEmpty(item.EntranceCode);
									break;
								case TermGroup_SysContactAddressRowType.Country:
									rowUpdated = TryUpdateTextOnContactAddressRow(row, item.Country);
									rowDeleted = String.IsNullOrEmpty(item.Country);
									break;
							}

							if (rowUpdated || contactAddress.IsSecret != item.IsSecret)
							{
								contactAddress.IsSecret = item.IsSecret;
								SetModifiedProperties(row);
							}
							if (rowDeleted)
							{
								entities.DeleteObject(row);
								contactAddress.ContactAddressRow.Remove(row);
							}
						}

						// Add new ContactAddressRows
						if (!string.IsNullOrEmpty(item.AddressName) && !contactAddress.ContactAddressRow.Any(a => (TermGroup_SysContactAddressRowType)a.SysContactAddressRowTypeId == TermGroup_SysContactAddressRowType.Name))
							AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.Name, item.AddressName);
						if (!string.IsNullOrEmpty(item.Address) && !contactAddress.ContactAddressRow.Any(a => (TermGroup_SysContactAddressRowType)a.SysContactAddressRowTypeId == TermGroup_SysContactAddressRowType.Address))
							AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.Address, item.Address);
						if (!string.IsNullOrEmpty(item.AddressCO) && !contactAddress.ContactAddressRow.Any(a => (TermGroup_SysContactAddressRowType)a.SysContactAddressRowTypeId == TermGroup_SysContactAddressRowType.AddressCO))
							AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.AddressCO, item.AddressCO);
						if (!string.IsNullOrEmpty(item.PostalCode) && !contactAddress.ContactAddressRow.Any(a => (TermGroup_SysContactAddressRowType)a.SysContactAddressRowTypeId == TermGroup_SysContactAddressRowType.PostalCode))
							AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.PostalCode, item.PostalCode);
						if (!string.IsNullOrEmpty(item.PostalAddress) && !contactAddress.ContactAddressRow.Any(a => (TermGroup_SysContactAddressRowType)a.SysContactAddressRowTypeId == TermGroup_SysContactAddressRowType.PostalAddress))
							AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.PostalAddress, item.PostalAddress);
						if (!string.IsNullOrEmpty(item.StreetAddress) && !contactAddress.ContactAddressRow.Any(a => (TermGroup_SysContactAddressRowType)a.SysContactAddressRowTypeId == TermGroup_SysContactAddressRowType.StreetAddress))
							AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.StreetAddress, item.StreetAddress);
						if (!string.IsNullOrEmpty(item.EntranceCode) && !contactAddress.ContactAddressRow.Any(a => (TermGroup_SysContactAddressRowType)a.SysContactAddressRowTypeId == TermGroup_SysContactAddressRowType.EntranceCode))
							AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.EntranceCode, item.EntranceCode);
						if (!string.IsNullOrEmpty(item.Country) && !contactAddress.ContactAddressRow.Any(a => (TermGroup_SysContactAddressRowType)a.SysContactAddressRowTypeId == TermGroup_SysContactAddressRowType.Country))
							AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.Country, item.Country);

						#endregion
					}
					else
					{
						#region Delete

						foreach (ContactAddressRow row in contactAddress.ContactAddressRow.ToList())
						{
							entities.DeleteObject(row);
						}
						entities.DeleteObject(contactAddress);

						#endregion
					}
				}

				#region ContactAddress Add

				foreach (ContactAddressItem item in contactItems.Where(c => c.IsAddress && c.ContactAddressId == 0))
				{
					var contactAddress = new ContactAddress
					{
						Contact = contact,
						Name = item.Name,
						IsSecret = item.IsSecret,
						SysContactAddressTypeId = item.SysContactAddressTypeId,
					};
					SetCreatedProperties(contactAddress);
					contact.ContactAddress.Add(contactAddress);

					if (!string.IsNullOrEmpty(item.AddressName))
						AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.Name, item.AddressName);
					if (!string.IsNullOrEmpty(item.Address))
						AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.Address, item.Address);
					if (!string.IsNullOrEmpty(item.AddressCO))
						AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.AddressCO, item.AddressCO);
					if (!string.IsNullOrEmpty(item.PostalCode))
						AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.PostalCode, item.PostalCode);
					if (!string.IsNullOrEmpty(item.PostalAddress))
						AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.PostalAddress, item.PostalAddress);
					if (!string.IsNullOrEmpty(item.StreetAddress))
						AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.StreetAddress, item.StreetAddress);
					if (!string.IsNullOrEmpty(item.EntranceCode))
						AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.EntranceCode, item.EntranceCode);
					if (!string.IsNullOrEmpty(item.Country))
						AddContactAddressRowToContactAddress(contactAddress, TermGroup_SysContactAddressRowType.Country, item.Country);
				}

				#endregion

				#endregion

				#region ContactECom

				foreach (ContactECom contactECom in contact.ContactECom.ToList())
				{
					ContactAddressItem item = contactItems.FirstOrDefault(c => c.ContactEComId == contactECom.ContactEComId);

					if (contactECom.SysContactEComTypeId == (int)TermGroup_SysContactEComType.GlnNumber && (item == null || contactECom.Text != item.EComText))
					{
						ActorManager.DeleteCompanyExternalCode(entities, TermGroup_CompanyExternalCodeEntity.CustomerContact_InexchangeCompanyId, contactECom.ContactEComId, this.ActorCompanyId, false);
					}

					if (!string.IsNullOrEmpty(item?.EComText))
					{
						#region ContactECom Update
						item.EComText = item.EComText?.Trim();

						bool isEmail = item.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email || item.SysContactEComTypeId == (int)TermGroup_SysContactEComType.CompanyAdminEmail;
						if (isEmail && !Validator.IsValidEmailAddress(item.EComText))
						{
							result = new ActionResult((int)ActionResultSave.NothingSaved, GetText(11672, "Angiven e-post är inte korrekt") + ": " + item.EComText);
							return result;
						}

						contactECom.Name = item.Name;
						contactECom.Text = item.EComText;
						contactECom.Description = item.EComDescription;
						contactECom.IsSecret = item.IsSecret;
						SetModifiedProperties(contactECom);

						#endregion
					}
					else
					{
						#region ContactECom Delete

						if (entityType == SoeEntityType.Customer)
						{
							RemoveEmailFromInvoice(entities, contactECom.ContactEComId);
						}

						entities.DeleteObject(contactECom);

						#endregion
					}
				}

				#region ContactECom Add

				foreach (ContactAddressItem item in contactItems.Where(c => !c.IsAddress && c.ContactEComId == 0 && !String.IsNullOrEmpty(c.EComText)))
				{
					bool isEmail = item.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email || item.SysContactEComTypeId == (int)TermGroup_SysContactEComType.CompanyAdminEmail;
					item.EComText = item.EComText?.Trim();
					if (isEmail && !Validator.IsValidEmailAddress(item.EComText))
					{
						result = new ActionResult((int)ActionResultSave.NothingSaved, GetText(11672, "Angiven e-post är inte korrekt") + ": " + item.EComText);
						return result;
					}

					var contactECom = new ContactECom
					{
						Contact = contact,
						Name = item.Name,
						SysContactEComTypeId = item.SysContactEComTypeId,
						Text = item.EComText,
						IsSecret = item.IsSecret,
						Description = item.EComDescription,
					};
					SetCreatedProperties(contactECom);
					contact.ContactECom.Add(contactECom);
				}

				#endregion

				#endregion
			}

			if (transaction == null)
				result.ObjectsAffected = entities.SaveChanges();
			else
				result = SaveChanges(entities, transaction);

			if (result.ObjectsAffected == 0 && transaction == null)
			{
				result.Success = false;
				result.ErrorNumber = (int)ActionResultSave.NothingSaved;
			}

			return result;
		}

		/*public void RemoveAddressFromInvoice(CompEntities entities, int contactAddressId)
        {
            var invoices = (from i in CompEntities.Invoice.OfType<CustomerInvoice>()
                            where i.BillingAddressId == contactAddressId &&
                            i.State == (int)SoeEntityState.Active
                            select i);

            foreach(var invoice in invoices)
            {
                invoice.BillingAddressId = (int?)null;
            }
        }*/

		public void RemoveEmailFromInvoice(CompEntities entities, int contactEcomId)
		{
			var invoices = (from i in entities.Invoice
							where i.ContactEComId == contactEcomId &&
							i.State == (int)SoeEntityState.Active
							select i);

			foreach (var invoice in invoices)
			{
				invoice.ContactEComId = (int?)null;
				SetModifiedProperties(invoice);
			}
		}

		#endregion

		#region ContactECom

		public ContactECom CreateContactECom(CompEntities entities, Contact contact, int type, string text, string name = null)
		{
			var contactECom = new ContactECom
			{
				Name = string.IsNullOrEmpty(name) ? GetText(type, (int)TermGroup.SysContactEComType) : name,
				SysContactEComTypeId = type,
				Text = text,
			};
			SetCreatedProperties(contact);
			entities.ContactECom.AddObject(contactECom);

			//Add to Contact
			contact.ContactECom.Add(contactECom);

			return contactECom;
		}

		public List<ContactEcomView> GetContactEcoms(int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactEcomView.NoTracking();
			return GetContactEcoms(entities, actorCompanyId);
		}

		public List<ContactEcomView> GetContactEcoms(CompEntities entities, int actorCompanyId)
		{
			return (from ecom in entities.ContactEcomView
					where ecom.ActorCompanyId == actorCompanyId
					select ecom).ToList();
		}

		public List<ContactECom> GetContactEcoms(Contact contact, int sysContactEcomTypeId)
		{
			if (contact == null)
				return new List<ContactECom>();

			return (from car in contact.ContactECom
					where car.SysContactEComTypeId == sysContactEcomTypeId
					select car).ToList();
		}

		public List<ContactECom> GetContactEComs(int contactId, bool loadContact = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactECom.NoTracking();
			return GetContactEComs(entities, contactId, loadContact);
		}

		public List<ContactECom> GetContactEComs(CompEntities entities, int contactId, bool loadContact = false)
		{
			var query = from ecom in entities.ContactECom
						where ecom.Contact.ContactId == contactId
						select ecom;

			if (loadContact)
				query = query.Include(i => i.Contact);

			return query.ToList();
		}

		public List<ContactECom> GetContactEComsFromActor(int actorId, bool loadContact, TermGroup_SysContactEComType type = TermGroup_SysContactEComType.Unknown, string ecomText = "")
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactECom.NoTracking();
			return GetContactEComsFromActor(entities, actorId, loadContact, type, ecomText);
		}

		public List<ContactECom> GetContactEComsFromActor(CompEntities entities, int actorId, bool loadContact, TermGroup_SysContactEComType type = TermGroup_SysContactEComType.Unknown, string ecomText = "")
		{
			var contactEcoms = (from cec in entities.ContactECom
											  .Include(i => i.Contact)
								where cec.Contact.Actor.ActorId == actorId
								select cec);

			if (type != TermGroup_SysContactEComType.Unknown)
			{
				contactEcoms = contactEcoms.Where(c => c.SysContactEComTypeId == (int)type);
			}

			if (!string.IsNullOrEmpty(ecomText))
			{
				contactEcoms = contactEcoms.Where(c => c.Text.ToLower() == ecomText.ToLower());
			}

			if (loadContact)
			{
				foreach (ContactECom contactEcom in contactEcoms)
				{

					if (!contactEcom.ContactReference.IsLoaded)
						contactEcom.ContactReference.Load();
				}
			}

			return contactEcoms.ToList();
		}

		public ContactECom GetContactECom(int contactEComId, bool loadContact)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactECom.NoTracking();
			return GetContactECom(entities, contactEComId, loadContact);
		}

		public ContactECom GetContactECom(CompEntities entities, int contactEComId, bool loadContact)
		{
			ContactECom contactEcom = (from ecom in entities.ContactECom
									   where ecom.ContactEComId == contactEComId
									   select ecom).FirstOrDefault();

			if (contactEcom != null && loadContact && !contactEcom.ContactReference.IsLoaded)
				contactEcom.ContactReference.Load();

			return contactEcom;
		}

		public ContactECom GetContactECom(int contactId, int sysContactEComTypeId, bool loadContact)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactECom.NoTracking();
			return GetContactECom(entities, contactId, sysContactEComTypeId, loadContact);
		}

		public ContactECom GetContactECom(CompEntities entities, int contactId, int sysContactEcomTypeId, bool loadContact, string ecomText = null)
		{
			IQueryable<ContactECom> query = (from ecom in entities.ContactECom
											 where ((ecom.Contact.ContactId == contactId) &&
											 (ecom.SysContactEComTypeId == sysContactEcomTypeId))
											 select ecom);

			if (!string.IsNullOrEmpty(ecomText))
			{
				query = query.Where(c => c.Text == ecomText);
			}

			var contactEcom = query.FirstOrDefault();

			if (contactEcom != null && loadContact && !contactEcom.ContactReference.IsLoaded)
				contactEcom.ContactReference.Load();

			return contactEcom;
		}

		public string GetContactEComText(CompEntities entities, int contactEComId)
		{
			return entities.ContactECom.FirstOrDefault(ecom => ecom.ContactEComId == contactEComId)?.Text ?? string.Empty;
		}

		public string MergeClosestRelative(object source, object relation)
		{
			return string.Format("{0};{1}", StringUtility.NullToEmpty(source), StringUtility.NullToEmpty(relation));
		}

		public ActionResult SaveContactECom(Collection<FormIntervalEntryItem> formIntervalEntryItems, int actorId)
		{
			if (formIntervalEntryItems == null || formIntervalEntryItems.Count == 0)
				return new ActionResult(true);

			using (CompEntities entities = new CompEntities())
			{
				// Default result is unsuccessful
				ActionResult result = new ActionResult(false);

				try
				{
					entities.Connection.Open();

					using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
					{
						Contact contact = GetContactFromActor(entities, actorId);
						if (contact == null)
							return new ActionResult((int)ActionResultSave.EntityNotFound, "Contact");

						result = DeleteContactEComs(entities, contact);
						if (!result.Success)
							return result;

						// Default result is successfull
						result = new ActionResult(true);
						foreach (FormIntervalEntryItem formIntervalItem in formIntervalEntryItems)
						{
							result = AddContactECom(entities, contact, formIntervalItem.LabelType, formIntervalItem.From, transaction, true);
							if (!result.Success)
								break;
						}

						//Commit transaction
						if (result.Success)
							transaction.Complete();
					}
				}
				finally
				{
					if (result.Success)
					{
						//Set success properties
					}
					else
						base.LogTransactionFailed(this.ToString(), this.log);

					entities.Connection.Close();
				}

				return result;
			}
		}

		public ActionResult AddContactECom(CompEntities entities, Contact contact, int sysContactEComTypeId, string text, TransactionScope transaction, bool save = false, string description = null, bool? isSecret = null)
		{
			var result = new ActionResult(true);

			//No need to save empty, return success
			if (string.IsNullOrEmpty(text))
				return new ActionResult(true);

			if (contact == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "Contact");

			var contactEcom = CreateContactECom(entities, contact, sysContactEComTypeId, text, null);
			contactEcom.Description = description;
			contactEcom.IsSecret = isSecret.HasValue && isSecret.Value;

			if (save)
			{
				result = (transaction == null) ? SaveChanges(entities) : SaveChanges(entities, transaction);
				result.IntegerValue = contactEcom.ContactEComId;
			}

			return result;
		}

		public ActionResult UpdateContactECom(CompEntities entities, int contactEcomId, string text, TransactionScope transaction, bool save = false, string description = null, bool? isSecret = null)
		{
			ActionResult result = new ActionResult(true);

			ContactECom contactEcom = ContactManager.GetContactECom(entities, contactEcomId, false);
			if (contactEcom != null)
			{
				contactEcom.Text = text;
				if (!String.IsNullOrEmpty(description))
					contactEcom.Description = description;
				if (isSecret.HasValue)
					contactEcom.IsSecret = isSecret.Value;
				SetModifiedProperties(contactEcom);

				if (save)
					result = SaveChanges(entities, transaction);
			}
			return result;
		}
		public ActionResult DeleteContactECom(CompEntities entities, int contactEcomId)
		{
			ContactECom contactEcom = ContactManager.GetContactECom(entities, contactEcomId, false);
			if (contactEcom != null)
			{
				entities.DeleteObject(contactEcom);
			}

			return SaveDeletions(entities);
		}

		private ActionResult DeleteContactEComs(CompEntities entities, Contact contact)
		{
			if (contact == null)
				return new ActionResult((int)ActionResultDelete.EntityIsNull, "Contact");

			var contactEComs = GetContactEComs(entities, contact.ContactId);
			if (contactEComs.IsNullOrEmpty())
				return new ActionResult(true);

			foreach (ContactECom contactECom in contactEComs)
			{
				entities.DeleteObject(contactECom);
			}

			return SaveDeletions(entities);
		}

		#endregion

		#region ContactPerson

		public List<ContactPerson> GetContactPersonsAll(int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Actor.NoTracking();
			return GetContactPersonsAll(entities, actorCompanyId);
		}

		public List<ContactPerson> GetContactPersonsAll(CompEntities entities, int actorCompanyId)
		{
			List<ContactPerson> allContactPersons = new List<ContactPerson>();

			// Company
			var actor = (from a in entities.Actor
						 where a.ActorId == actorCompanyId
						 select a).FirstOrDefault();

			if (actor != null)
				allContactPersons = actor.ActiveContactPersons.ToList();

			// Supplier
			List<Supplier> suppliers = SupplierManager.GetSuppliersByCompany(entities, actorCompanyId, onlyActive: true);
			foreach (Supplier supplier in suppliers)
			{
				if (!supplier.ActorReference.IsLoaded)
					supplier.ActorReference.Load();

				List<ContactPerson> contactPersons = supplier.Actor.ActiveContactPersons;
				if (contactPersons != null && contactPersons.Any())
					allContactPersons = allContactPersons.Concat(contactPersons).ToList();
			}

			// Customer
			List<Customer> customers = CustomerManager.GetCustomersByCompany(entities, actorCompanyId, onlyActive: true);
			foreach (Customer customer in customers)
			{
				if (!customer.ActorReference.IsLoaded)
					customer.ActorReference.Load();

				List<ContactPerson> contactPersons = customer.Actor.ActiveContactPersons;
				if (contactPersons != null && contactPersons.Any())
					allContactPersons = allContactPersons.Concat(contactPersons).ToList();
			}

			// Remove duplicates
			List<ContactPerson> validatedContactPersons = new List<ContactPerson>();
			foreach (ContactPerson contactPersonOuter in allContactPersons)
			{
				bool exist = false;
				foreach (ContactPerson contactPersonInner in validatedContactPersons)
				{
					if (contactPersonOuter.ActorContactPersonId == contactPersonInner.ActorContactPersonId)
					{
						exist = true;
						break;
					}
				}
				if (!exist)
					validatedContactPersons.Add(contactPersonOuter);
			}

			return validatedContactPersons;
		}

		public List<ContactPerson> GetContactPersons(int actorId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Actor.NoTracking();
			return GetContactPersons(entities, actorId);
		}

		public List<ContactPerson> GetContactPersons(CompEntities entities, int actorId)
		{
			var actor = (from a in entities.Actor.Include("ContactPersons.Actor.ActorConsent")
						 where a.ActorId == actorId
						 select a).FirstOrDefault<Actor>();

			return (actor != null) ? actor.ActiveContactPersons.ToList() : new List<ContactPerson>();
		}

		public List<ContactPersonDTO> GetContactPersonsByActorId(int actorId)
		{
			var dtos = new List<ContactPersonDTO>();

			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var result = entitiesReadOnly.GetContactPersons(base.ActorCompanyId, actorId.ToString()).ToList();
			var positionTerms = base.GetTermGroupContent(TermGroup.ContactPersonPosition, skipUnknown: true);

			foreach (var cp in result)
			{
				var dto = cp.ToDTO();
				GenericType term = positionTerms.FirstOrDefault(t => t.Id == cp.Position);
				dto.PositionName = term != null ? term.Name : String.Empty;
				dtos.Add(dto);
			}
			return dtos;

		}

		//Actors are converted to list of ints in procedure. Should be supplier/customer actors sent in as comma separated string.
		public List<ContactPersonGridDTO> GetContactPersonsByActorIdsForGrid(string actors = "", int? contactpersonId = null)
		{
			var dtos = new List<ContactPersonGridDTO>();

			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var result = entitiesReadOnly.GetContactPersons(base.ActorCompanyId, actors).ToList();

			if (contactpersonId.HasValue)
				result = result.Where(cp => cp.ActorContactPersonId == contactpersonId.Value).ToList();

			var positionTerms = base.GetTermGroupContent(TermGroup.ContactPersonPosition, skipUnknown: true);

			foreach (var cp in result)
			{
				var dto = cp.ToGridDTO();
				GenericType term = positionTerms.FirstOrDefault(t => t.Id == cp.Position);
				dto.PositionName = term != null ? term.Name : String.Empty;
				dtos.Add(dto);
			}
			return dtos;
		}

		public List<int> GetContactPersonCategories(int actorContactPersonId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactPerson.NoTracking();
			return (from c in entities.CompanyCategoryRecord
					where c.RecordId == actorContactPersonId &&
					c.Entity == (int)SoeCategoryRecordEntity.ContactPerson &&
					c.Category.Type == (int)SoeCategoryType.ContactPerson &&
					c.Category.ActorCompanyId == ActorCompanyId &&
					c.Category.State == (int)SoeEntityState.Active
					select c.CategoryId).ToList();
		}

		public Dictionary<int, string> GetCustomerReferencesDict(int actorId, bool addEmptyRow)
		{
			var dict = new Dictionary<int, string>();
			if (addEmptyRow)
				dict.Add(0, " ");

			List<ContactPerson> contactPersons = GetContactPersons(actorId);
			foreach (ContactPerson contactPerson in contactPersons)
			{
				dict.Add(contactPerson.ActorContactPersonId, contactPerson.Name);
			}

			return dict;
		}

		public Dictionary<int, string> GetContactAddressItemsDict(int actorContactPersonId)
		{
			var dict = new Dictionary<int, string>();

			var Rows = GetContactAddressItems(actorContactPersonId);
			foreach (var row in Rows)
			{
				dict.Add(row.SysContactEComTypeId, row.EComText);
			}

			return dict;
		}

		public ContactPerson GetContactPerson(int actorContactPersonId, bool loadActor = false, bool onlyActive = true)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactPerson.NoTracking();
			return GetContactPerson(entities, actorContactPersonId, loadActor, onlyActive);
		}

		public ContactPerson GetContactPerson(CompEntities entities, int actorContactPersonId, bool loadActor = false, bool onlyActive = true)
		{
			var query = (from cp in entities.ContactPerson
						 where cp.ActorContactPersonId == actorContactPersonId &&
						 cp.State != (int)SoeEntityState.Deleted
						 select cp);

			if (loadActor)
				query = query.Include("Actor.ActorConsent");

			if (onlyActive)
				query = query.Where(e => e.State == (int)SoeEntityState.Active);

			return query.FirstOrDefault();
		}

		public ContactPerson GetContactPersonForExport(int actorContactPersonId, bool loadActor)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactPerson.NoTracking();
			return GetContactPersonForExport(entities, actorContactPersonId, loadActor);
		}

		public ContactPerson GetContactPersonForExport(CompEntities entities, int actorContactPersonId, bool loadActor = false)
		{
			IQueryable<ContactPerson> query = entities.ContactPerson;

			if (loadActor)
				query = query.Include("Actor.ActorConsent");

			var contactPerson = (from cp in query
								 where cp.ActorContactPersonId == actorContactPersonId && cp.State == (int)SoeEntityState.Active
								 select cp).FirstOrDefault();

			List<GenericType> positionTerms = base.GetTermGroupContent(TermGroup.ContactPersonPosition, skipUnknown: true);
			GenericType term = positionTerms.FirstOrDefault(t => t.Id == contactPerson.Position);
			contactPerson.PositionName = term != null ? term.Name : String.Empty;

			//Email and Phone
			List<ContactAddressItem> Rows = GetContactAddressItems(entities, contactPerson.ActorContactPersonId);
			foreach (var row in Rows)
			{
				if (row.ContactAddressItemType == ContactAddressItemType.EComEmail)
				{
					contactPerson.Email = row.EComText;
				}
				else if (row.ContactAddressItemType == ContactAddressItemType.EComPhoneMobile)
				{
					contactPerson.PhoneNumber = row.EComText;
				}
			}

			return contactPerson;
		}

		public ContactPerson GetContactPersonIgnoreState(CompEntities entities, int actorContactPersonId, bool loadActor = false)
		{
			//Ignore all states
			ContactPerson contactPerson = (from cp in entities.ContactPerson
										   where cp.ActorContactPersonId == actorContactPersonId
										   select cp).FirstOrDefault();

			if (contactPerson != null && loadActor)
			{
				if (!contactPerson.ActorReference.IsLoaded)
					contactPerson.ActorReference.Load();
				if (contactPerson.Actor != null && !contactPerson.Actor.ActorConsent.IsLoaded)
					contactPerson.Actor.ActorConsent.Load();
				if (!contactPerson.Actors.IsLoaded)
					contactPerson.Actors.Load();
			}

			return contactPerson;
		}

		public ContactPerson GetContactPersonByName(CompEntities entities, string firstName, string lastName)
		{
			return (from cp in entities.ContactPerson
					where cp.FirstName.ToLower() == firstName.ToLower() &&
					cp.LastName.ToLower() == lastName.ToLower() &&
					cp.State == (int)SoeEntityState.Active
					select cp).FirstOrDefault();
		}

		public ContactPerson GetPrevNextContactPerson(int actorContactPersonId, int actorCompanyId, SoeFormMode mode)
		{
			ContactPerson contactPerson = null;
			List<ContactPerson> contactPersons = GetContactPersons(actorCompanyId);

			if (mode == SoeFormMode.Next)
			{
				contactPerson = (from cp in contactPersons
								 where ((cp.ActorContactPersonId > actorContactPersonId) &&
								 (cp.State == (int)SoeEntityState.Active))
								 orderby cp.ActorContactPersonId ascending
								 select cp).FirstOrDefault<ContactPerson>();
			}
			else
			{
				contactPerson = (from cp in contactPersons
								 where ((cp.ActorContactPersonId < actorContactPersonId) &&
								 (cp.State == (int)SoeEntityState.Active))
								 orderby cp.ActorContactPersonId descending
								 select cp).FirstOrDefault<ContactPerson>();
			}

			return contactPerson;
		}

		public string GetContactPersonName(int userId)
		{
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.User.NoTracking();
			entitiesReadOnly.ContactPerson.NoTracking();
			User user = (from u in entitiesReadOnly.User.Include("ContactPerson")
						 where u.UserId == userId &&
						 u.State == (int)SoeEntityState.Active
						 select u).FirstOrDefault();

			if (user == null)
				return String.Empty;
			return user.ContactPerson != null ? user.ContactPerson.Name : user.Name;
		}

		public ActionResult AddContactPerson(ContactPerson contactPerson, int actorCompanyId)
		{
			using (CompEntities entities = new CompEntities())
			{
				return AddContactPerson(entities, contactPerson, actorCompanyId);
			}
		}

		public ActionResult AddContactPerson(CompEntities entities, ContactPerson contactPerson, int actorCompanyId)
		{
			if (contactPerson == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "ContactPerson");

			Actor actor = new Actor()
			{
				ActorType = (int)SoeActorType.ContactPerson,
			};
			actor.ContactPerson = contactPerson;
			SetCreatedProperties(actor.ContactPerson);

			ActionResult result = AddEntityItem(entities, actor, "Actor");
			if (!result.Success)
				return result;

			result = MapActorToContactPerson(entities, contactPerson, actorCompanyId);
			if (result.Success)
				result.IntegerValue = actor.ContactPerson.ActorContactPersonId;

			return result;
		}

		public ActionResult SaveContactPerson(int actorId, ContactPersonDTO contactPerson)
		{
			ActionResult result = new ActionResult();
			int contactPersonId = contactPerson.ActorContactPersonId;

			using (CompEntities entities = new CompEntities())
			{
				using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
				{
					result = AddUpdateContactPerson(entities, transaction, contactPerson, actorId);
					contactPersonId = result.IntegerValue;
					if (!result.Success)
					{
						return result;
					}

					using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
					result = CategoryManager.SaveCompanyCategoryRecords(entitiesReadOnly, transaction, contactPerson.CategoryRecords, ActorCompanyId, SoeCategoryType.ContactPerson, SoeCategoryRecordEntity.ContactPerson, contactPerson.ActorContactPersonId);
					if (!result.Success)
					{
						return result;
					}

					if (result.Success)
					{
						transaction.Complete();
					}
				}

			}

			var savedAdressesResult = SaveContactAdresses(contactPerson.ActorContactPersonId, contactPerson.Email, contactPerson.PhoneNumber);
			if (savedAdressesResult.Success)
			{
				result = savedAdressesResult;
			}

			if (result.Success)
			{
				//Set success properties
				result.Value = contactPerson;
			}
			result.IntegerValue = contactPersonId;

			return result;
		}

		public ActionResult SaveContactAdresses(int actorContactPersonId, string email, string phoneNumer)
		{
			using (CompEntities entities = new CompEntities())
			{
				var rows = ValidateContactAddressItems(entities, actorContactPersonId, email, phoneNumer);
				return SaveContactAddresses(entities, rows, actorContactPersonId, TermGroup_SysContactType.Company);
			}
		}

		public List<ContactAddressItem> ValidateContactAddressItems(CompEntities entities, int actorContactPersonId, string email, string phoneNumer)
		{
			var Rows = GetContactAddressItems(entities, actorContactPersonId);
			if (string.IsNullOrEmpty(email))
			{
				//Remove if present
				ContactAddressItem item = Rows.FirstOrDefault(t => t.ContactAddressItemType == ContactAddressItemType.EComEmail);
				if (item != null)
					Rows.Remove(item);
			}
			else
			{
				bool found = false;
				//update
				foreach (var row in Rows)
				{
					if (row.ContactAddressItemType == ContactAddressItemType.EComEmail)
					{
						row.EComText = email;
						found = true;
					}
				}
				//add if not there
				if (!found)
				{
					//New must be added
					ContactAddressItem row = new ContactAddressItem()
					{
						ContactAddressItemType = ContactAddressItemType.EComEmail
					};

					row.IsAddress = false;
					row.SysContactEComTypeId = (int)TermGroup_SysContactEComType.Email;
					row.EComText = email;
					row.ContactId = actorContactPersonId;
					Rows.Add(row);
				}
			}

			if (string.IsNullOrEmpty(phoneNumer))
			{
				//Remove if present
				ContactAddressItem item2 = Rows.FirstOrDefault(t => t.ContactAddressItemType == ContactAddressItemType.EComPhoneMobile);
				if (item2 != null)
					Rows.Remove(item2);
			}
			else
			{
				bool foundtoo = false;
				//Update
				foreach (var row in Rows)
				{
					if (row.ContactAddressItemType == ContactAddressItemType.EComPhoneMobile)
					{
						row.EComText = phoneNumer;
						foundtoo = true;
					}
				}
				//Update
				if (!foundtoo)
				{
					//New must be added
					ContactAddressItem row = new ContactAddressItem()
					{
						ContactAddressItemType = ContactAddressItemType.EComPhoneMobile
					};

					row.IsAddress = false;
					row.SysContactEComTypeId = (int)TermGroup_SysContactEComType.PhoneMobile;
					row.EComText = phoneNumer;
					row.ContactId = actorContactPersonId;
					Rows.Add(row);
				}
			}

			return Rows;
		}

		public ActionResult UpdateContactPerson(ContactPerson contactPerson)
		{
			using (CompEntities entities = new CompEntities())
			{
				return UpdateContactPerson(entities, contactPerson);
			}
		}

		public ActionResult UpdateContactPerson(CompEntities entities, ContactPerson contactPerson)
		{
			if (contactPerson == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "ContactPerson");

			ContactPerson originalContactPerson = GetContactPerson(entities, contactPerson.ActorContactPersonId);
			if (originalContactPerson == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, "ContactPerson");

			//Update the values from incoming data
			originalContactPerson.FirstName = contactPerson.FirstName;
			originalContactPerson.LastName = contactPerson.LastName;
			originalContactPerson.Position = contactPerson.Position;
			originalContactPerson.Description = contactPerson.Description;
			originalContactPerson.SocialSec = contactPerson.SocialSec;
			originalContactPerson.Sex = contactPerson.Sex;
			originalContactPerson.State = contactPerson.State;
			SetModifiedProperties(contactPerson);

			return SaveChanges(entities);
		}

		public ActionResult AddUpdateContactPerson(CompEntities entities, TransactionScope transaction, ContactPersonDTO contactPersonDTO, int actorId)
		{
			if (contactPersonDTO == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "ContactPerson");

			ActionResult result = new ActionResult();

			ContactPerson originalContactPerson = contactPersonDTO.ActorContactPersonId > 0 ? GetContactPerson(entities, contactPersonDTO.ActorContactPersonId, true) : null;

			if (originalContactPerson == null)
			{
				#region Add

				originalContactPerson = new ContactPerson();
				SetContactProperies(originalContactPerson, contactPersonDTO);
				SetCreatedProperties(originalContactPerson);

				var actor = new Actor { ActorType = (int)SoeActorType.ContactPerson };
				actor.ContactPerson = originalContactPerson;

				result = AddEntityItem(entities, actor, "Actor");
				if (!result.Success)
					return result;

				result = MapActorToContactPerson(entities, originalContactPerson, actorId);
				contactPersonDTO.ActorContactPersonId = originalContactPerson.ActorContactPersonId;

				if (!result.Success)
				{
					return new ActionResult((int)ActionResultSave.ContactPersonNotSaved, "Failed mapping contact to actor");
				}

				#endregion
			}
			else
			{
				#region Update

				SetContactProperies(originalContactPerson, contactPersonDTO);
				SetModifiedProperties(originalContactPerson);

				#endregion
			}

			SetConsentProperties(originalContactPerson, contactPersonDTO);
			result = SaveChanges(entities, transaction);
			if (result.Success)
			{
				result.IntegerValue = contactPersonDTO.ActorContactPersonId;
			}
			return result;
		}

		private void SetContactProperies(ContactPerson contactPerson, ContactPersonDTO contactPersonDTO)
		{
			//Update the values from incoming data
			contactPerson.FirstName = contactPersonDTO.FirstName;
			contactPerson.LastName = contactPersonDTO.LastName;
			contactPerson.Position = contactPersonDTO.Position;
			contactPerson.Description = contactPersonDTO.Description;
			contactPerson.SocialSec = contactPersonDTO.SocialSec;
			contactPerson.Sex = (int)contactPersonDTO.Sex;
			contactPerson.State = (int)contactPersonDTO.State;
		}

		private void SetConsentProperties(ContactPerson contactPerson, ContactPersonDTO contactPersonDTO)
		{
			var consent = contactPerson.Actor.ActorConsent.FirstOrDefault(a => a.ConsentType == (int)ActorConsentType.Unspecified);
			if (consent == null)
			{
				consent = new ActorConsent();
				contactPerson.Actor.ActorConsent.Add(consent);
			}

			if ((consent.HasConsent != contactPersonDTO.HasConsent) || (consent.ConsentDate != contactPersonDTO.ConsentDate))
			{
				consent.HasConsent = contactPersonDTO.HasConsent;
				consent.ConsentDate = consent.HasConsent ? contactPersonDTO.ConsentDate : null;
				consent.ConsentModified = DateTime.Now;
				consent.ConsentModifiedBy = GetUserDetails();
			}
		}

		public ActionResult MapActorToContactPerson(int contactPersonId, int actorId)
		{
			using (CompEntities entities = new CompEntities())
			{
				return MapActorToContactPerson(entities, contactPersonId, actorId);
			}
		}

		public ActionResult MapActorToContactPerson(CompEntities entities, int contactPersonId, int actorId)
		{
			ContactPerson contactPerson = GetContactPerson(entities, contactPersonId);
			return MapActorToContactPerson(entities, contactPerson, actorId);
		}

		public ActionResult MapActorToContactPerson(CompEntities entities, ContactPerson contactPerson, int actorId)
		{
			if (contactPerson == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "ContactPerson");

			Actor actor = ActorManager.GetActor(entities, actorId, false);
			if (actor == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, "Actor");

			//Make sure Actors is loaded
			if (!contactPerson.Actors.IsLoaded)
				contactPerson.Actors.Load();

			//Add
			contactPerson.Actors.Add(actor);

			return SaveEntityItem(entities, contactPerson);
		}

		public ActionResult UnMapActorFromContactPerson(int contactPersonId, int actorId)
		{
			using (CompEntities entities = new CompEntities())
			{
				ContactPerson contactPerson = GetContactPerson(entities, contactPersonId);
				return UnMapActorFromContactPerson(entities, contactPerson, actorId);
			}
		}

		public ActionResult UnMapActorFromContactPerson(CompEntities entities, int contactPersonId, int actorId)
		{
			ContactPerson contactPerson = GetContactPerson(entities, contactPersonId);
			return UnMapActorFromContactPerson(entities, contactPerson, actorId);
		}

		public ActionResult UnMapActorFromContactPerson(CompEntities entities, ContactPerson contactPerson, int actorId)
		{
			if (contactPerson == null)
				return new ActionResult((int)ActionResultDelete.EntityIsNull, "ContactPerson");

			Actor actor = ActorManager.GetActor(entities, actorId, true);
			if (actor == null)
				return new ActionResult((int)ActionResultDelete.EntityNotFound, "Actor");

			//Make sure Actors is loaded
			if (!contactPerson.Actors.IsLoaded)
				contactPerson.Actors.Load();

			//Remove
			contactPerson.Actors.Remove(actor);

			return SaveEntityItem(entities, contactPerson);
		}

		/// <summary>
		/// Sets a ContactPerson to Deleted
		/// </summary>
		/// <param name="contacPerson">ContactPerson to delete</param>
		/// <param name="actorCompanyId">The Company to delete the ContactPerson from</param>
		/// <returns>ActionResult</returns>
		public ActionResult DeleteContactPerson(ContactPerson contactPerson, int actorCompanyId)
		{
			if (contactPerson == null)
				return new ActionResult((int)ActionResultDelete.EntityIsNull, "ContactPerson");

			using (CompEntities entities = new CompEntities())
			{
				ContactPerson originalContactPerson = GetContactPerson(entities, contactPerson.ActorContactPersonId, true);
				if (originalContactPerson == null)
					return new ActionResult((int)ActionResultDelete.EntityNotFound, "ContactPerson");

				ActionResult result = UnMapActorFromContactPerson(entities, originalContactPerson, actorCompanyId);
				if (result.Success)
				{
					SetModifiedProperties(originalContactPerson);

					//Set the ContactPerson to deleted if no other Companies use it
					if (originalContactPerson.Actors.Count == 0)
						result = ChangeEntityState(entities, originalContactPerson, SoeEntityState.Deleted, true);
				}

				return result;
			}
		}

		public ActionResult DeleteContactPerson(int contactPersonId, bool saveChanges = true, bool clearValues = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.ContactPerson.NoTracking();
			return DeleteContactPerson(entities, contactPersonId, saveChanges, clearValues);
		}

		/// <summary>
		/// Sets a ContactPerson to Deleted
		/// </summary>
		/// <param name="contacPerson">ContactPerson to delete</param>
		/// <param name="actorCompanyId">The Company to delete the ContactPerson from</param>
		/// <returns>ActionResult</returns>
		public ActionResult DeleteContactPerson(CompEntities entities, int contactPersonId, bool saveChanges = true, bool clearValues = false)
		{
			ActionResult result = new ActionResult();
			ContactPerson originalContactPerson = GetContactPerson(entities, contactPersonId, true);
			if (originalContactPerson == null)
				return new ActionResult((int)ActionResultDelete.EntityNotFound, "ContactPerson");

			if (!originalContactPerson.Actors.IsLoaded)
				originalContactPerson.Actors.Load();

			var actorIds = originalContactPerson.Actors.Select(a => a.ActorId).ToList();
			foreach (int actorId in actorIds)
			{
				result = UnMapActorFromContactPerson(entities, originalContactPerson, actorId);
			}

			if (result.Success)
			{
				if (clearValues)
				{
					var deleteText = GetText(7413, "RADERAD UPPGIFT");

					originalContactPerson.Description = " ";
					originalContactPerson.Email = " ";
					originalContactPerson.FirstName = deleteText + " " + DateTime.Today.ToShortDateString();
					originalContactPerson.LastName = deleteText + " " + DateTime.Today.ToShortDateString();
					originalContactPerson.PhoneNumber = " ";
					originalContactPerson.PositionName = " ";
					originalContactPerson.SocialSec = " ";
				}

				SetModifiedProperties(originalContactPerson);

				result = ChangeEntityState(entities, originalContactPerson, SoeEntityState.Deleted, saveChanges);
			}

			return result;
		}

		public ActionResult DeleteContactPersons(List<int> contactPersonsDict)
		{
			ActionResult result = null;

			using (CompEntities entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();

					foreach (int contactPersonId in contactPersonsDict)
					{
						result = ContactManager.DeleteContactPerson(entities, contactPersonId, false, true);
						if (!result.Success)
							return new ActionResult(false);
					}

					result = SaveChanges(entities);
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

		public ActionResult SaveContactPersonMappings(CompEntities entities, List<int> contactPersonIds, int actorId)
		{
			ActionResult result = new ActionResult();

			int errorCounter = 0;

			//Find ContactPersons to unmap from actor
			List<ContactPerson> currentContactPersons = GetContactPersons(entities, actorId);
			foreach (ContactPerson currentcontactPerson in currentContactPersons)
			{
				//If an already mapped contactPersons doesn't exist in contactPersonIds, then it has to be unmapped
				if (!contactPersonIds.Contains(currentcontactPerson.ActorContactPersonId))
				{
					result = UnMapActorFromContactPerson(entities, currentcontactPerson.ActorContactPersonId, actorId);
					if (!result.Success)
						errorCounter++;
				}
			}

			//Find ContactPersons to map to actor
			foreach (int contactPersonId in contactPersonIds)
			{
				// if a contactperson from contactPersonIds isn't already mapped to the actor, then it has to be mapped
				ContactPerson contactPerson = currentContactPersons.FirstOrDefault(cp => cp.ActorContactPersonId == contactPersonId);
				if (contactPerson == null)
				{
					result = MapActorToContactPerson(entities, contactPersonId, actorId);
					if (!result.Success)
						errorCounter++;
				}
			}

			if (errorCounter > 0)
			{
				result.ErrorNumber = (int)ActionResultSave.ContactPersonsMappedWithErrors;
				result.ErrorMessage = GetText(11013, "Alla kontaktpersoner kunde inte sparas");
			}

			return result;
		}

		#endregion

		#region SysContactType

		public List<SysContactType> GetSysContactTypes()
		{
			using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
			return sysEntitiesReadOnly.SysContactType
							.ToList<SysContactType>();
		}

		#endregion

		#region SysContactAddressType

		/// <summary>
		/// Get all SysContactAddressType's
		/// Accessor for SysDbCache
		/// </summary>
		/// <returns></returns>
		public List<SysContactAddressType> GetSysContactAddressTypes()
		{
			using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
			return sysEntitiesReadOnly.SysContactAddressType
							.Include("SysContactType")
							.ToList<SysContactAddressType>();
		}

		public IEnumerable<SysContactAddressType> GetSysContactAddressTypes(int contactTypeId)
		{
			//Uses SysDbCache
			return (from scat in SysDbCache.Instance.SysContactAddressTypes
					where scat.SysContactTypeId == contactTypeId
					orderby scat.SysTermId
					select scat).ToList<SysContactAddressType>();
		}

		public List<int> GetSysContactAddressTypeIds(int contactTypeId)
		{
			//Uses SysDbCache
			return (from scat in SysDbCache.Instance.SysContactAddressTypes
					where scat.SysContactTypeId == contactTypeId
					orderby scat.SysTermId
					select scat.SysContactAddressTypeId).ToList();
		}

		#endregion

		#region SysContactAddressRowType

		/// <summary>
		/// Get all SysContactAddressRowType's
		/// Accessor for SysDbCache
		/// </summary>
		/// <returns></returns>
		public List<SysContactAddressRowType> GetSysContactAddressRowTypes()
		{
			using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
			return sysEntitiesReadOnly.SysContactAddressRowType
							.Include("SysContactAddressType.SysContactType")
							.ToList<SysContactAddressRowType>();
		}

		public IEnumerable<SysContactAddressRowType> GetSysContactAddressRowTypes(int sysContactTypeId)
		{
			//Uses SysDbCache
			return (from scat in SysDbCache.Instance.SysContactAddressRowTypes
					where scat.SysContactTypeId == sysContactTypeId
					orderby scat.SysTermId
					select scat).ToList<SysContactAddressRowType>();
		}

		public List<GenericType<int, int>> GetSysContactAddressRowTypesWithAddressTypes(int sysContactTypeId)
		{
			//Uses SysDbCache
			IEnumerable<SysContactAddressRowType> types = (from scat in SysDbCache.Instance.SysContactAddressRowTypes
														   where scat.SysContactTypeId == sysContactTypeId
														   orderby scat.SysTermId
														   select scat).ToList();

			List<GenericType<int, int>> gTypes = new List<GenericType<int, int>>();
			foreach (var type in types)
			{
				GenericType<int, int> gType = new GenericType<int, int>();
				gType.Field1 = type.SysContactAddressTypeId;
				gType.Field2 = type.SysContactAddressRowTypeId;
				gTypes.Add(gType);
			}

			return gTypes;
		}

		#endregion

		#region SysContactEComType

		/// <summary>
		/// Get all SysContactEComType's
		/// Accessor for SysDbCache
		/// </summary>
		/// <returns></returns>
		public List<SysContactEComType> GetSysContactEComTypes()
		{
			using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
			return sysEntitiesReadOnly.SysContactEComType
							.Include("SysContactType")
							.ToList<SysContactEComType>();
		}

		public List<int> GetSysContactEComsTypeIds(int sysContactTypeId)
		{
			//Uses SysDbCache
			return (from ecom in SysDbCache.Instance.SysContactEComTypes
					where ecom.SysContactTypeId == sysContactTypeId
					select ecom.SysContactEComTypeId).ToList();
		}

		#endregion

		#region Validators

		public bool ValidateContactEComDeletion(SoeEntityType entityType, ContactAddressItemType contactType, int contactEcomId)
		{
			if (entityType == SoeEntityType.Customer)
			{
				switch (contactType)
				{
					case ContactAddressItemType.EComEmail:
						return ValidateEComEmailDeletion(contactEcomId);
					case ContactAddressItemType.AddressBilling:
						return ValidateBillingAddressDeletion(contactEcomId);
				}
			}

			return true;
		}

		public bool ValidateEComEmailDeletion(int contactEcomId)
		{
			using (CompEntities entities = new CompEntities())
			{
				return !(from i in entities.Invoice
						 where i.ContactEComId == contactEcomId &&
						 i.State == (int)SoeEntityState.Active
						 select i).Any();
			}
		}

		public bool ValidateBillingAddressDeletion(int contactEcomId)
		{
			using (CompEntities entities = new CompEntities())
			{
				return !(from i in entities.Invoice.OfType<CustomerInvoice>()
						 where i.BillingAddressId == contactEcomId &&
						 i.State == (int)SoeEntityState.Active
						 select i).Any();
			}
		}

		#endregion
	}
}
