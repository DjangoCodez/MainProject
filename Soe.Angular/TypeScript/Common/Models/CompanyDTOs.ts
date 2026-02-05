import { ICompanyDTO, ICompanyEditDTO, ICompanyExternalCodeDTO, ICompanyExternalCodeGridDTO, IContactAddressItem, IPaymentInformationDTO, ICopyFromTemplateCompanyInputDTO, System } from "../../Scripts/TypeLite.Net4";
import { TermGroup_CompanyExternalCodeEntity, TermGroup_Languages } from "../../Util/CommonEnumerations";

export class CompanyDTO implements ICompanyDTO {
	actorCompanyId: number;
	allowSupportLogin: boolean;
	allowSupportLoginTo: Date;
	demo: boolean;
	global: boolean;
	language: TermGroup_Languages;
	licenseId: number;
	licenseNr: string;
	licenseSupport: boolean;
	name: string;
	number: number;
	orgNr: string;
	shortName: string;
	sysCountryId: number;
	template: boolean;
	timeSpotId: number;
	vatNr: string;
}

export class CompanyEditDTO extends CompanyDTO implements ICompanyEditDTO {
	baseEntCurrencyId: number;
	baseSysCurrencyId: number;
	companyApiKey: string;
	companyTaxSupport: boolean;
	contactAddresses: IContactAddressItem[];
	created: Date;
	createdBy: string;
	defaultSysPaymentTypeId: number;
	ediActivated: Date;
	ediActivatedBy: string;
	ediModified: Date;
	ediModifiedBy: string;
	ediPassword: string;
	ediUsername: string;
	isEdiActivated: boolean;
	isEdiGOActivated: boolean;
	maxNrOfSMS: number;
	modified: Date;
	modifiedBy: string;
	paymentInformation: IPaymentInformationDTO;
}

export class CopyFromTemplateCompanyInputDTO implements ICopyFromTemplateCompanyInputDTO {
	actorCompanyId: number;
	copyDict: any;
	liberCopy: boolean;
	templateCompanyId: number;
	update: boolean;
	userId: number;
}