import { CalendarUtility } from './CalendarUtility';
import { SOEMessageBoxSize } from './Enumerations';
import { Feature, SoeCategoryType } from './CommonEnumerations';

export class CoreUtility {
    // PUBLIC PROPERTIES

    public static get apiPrefix(): string {
        if (soeConfig)
            return soeConfig.apiPrefix;
        else
            return "/api/";
    }

    public static get userToken(): string {
        if (soeConfig) {
            return soeConfig.token;
        } else {
            return "";
        }
    }

    public static get licenseId(): number {
        if (soeConfig)
            return soeConfig.licenseId;
        else
            return 0;
    }

    public static get licenseNr(): string {
        if (soeConfig)
            return soeConfig.licenseNr;
        else
            return "";
    }

    public static get actorCompanyId(): number {
        if (soeConfig)
            return soeConfig.actorCompanyId;
        else
            return 0;
    }

    public static get roleId(): number {
        if (soeConfig)
            return soeConfig.roleId;
        else
            return 0;
    }

    public static get userId(): number {
        if (soeConfig)
            return soeConfig.userId;
        else
            return 0;
    }

    public static get loginName(): string {
        if (soeConfig)
            return soeConfig.loginName;
        else
            return "";
    }

    public static get language(): string {
        if (soeConfig)
            return soeConfig.language;
        else
            return "sv-SE";
    }

    public static get languageDateFormat(): string {
        var lang: string = CoreUtility.language;
        if (lang.startsWith("sv-"))
            return "YYYY-MM-DD";
        else if (lang.startsWith("fi-"))
            return "DD.MM.YYYY";
        else
            return "MM/DD/YYYY";
    }

    public static get languageId(): number {
        if (soeConfig) {
            var lang: string = CoreUtility.language.substring(0, 2);
            if (lang.toLowerCase() === "sv")
                return 1;
            if (lang.toLowerCase() === "en")
                return 2;
            if (lang.toLowerCase() === "fi")
                return 3;
        }
        else
            return 1;
    }

    public static get termVersionNr(): string {
        if (soeConfig)
            return soeConfig.termVersionNr;
        else
            return CalendarUtility.getDateNow().toDateString();
    }

    public static get sysCountryId(): number {
        if (soeConfig)
            return soeConfig.sysCountryId;
        else
            return 1;
    }

    public static get isSupportAdmin(): boolean {
        if (soeConfig)
            return soeConfig.isSupportAdmin;
        else
            return false;
    }

    public static get isSupportSuperAdmin(): boolean {
        if (soeConfig)
            return soeConfig.isSupportSuperAdmin;
        else
            return false;
    }

    public static get soeParameters(): string {
        if (soeConfig)
            return soeConfig.soeParameters;
        else
            return "";
    }

    public static get baseUrl(): string {
        if (soeConfig)
            return soeConfig.baseUrl;
        else
            return "";
    }

    public static get isDebugMode(): boolean {
        return this.baseUrl.contains("/build");
    }

    // CUSTOMER SPECIFIC

    public static get isMartinServera(): boolean {
        return this.licenseNr.startsWith("500");
    }

    // FUNCTIONS

    public static iterate(obj: any) {
        for (var property in obj) {
            if (obj.hasOwnProperty(property)) {
                if (typeof obj[property] == "object") {
                    CoreUtility.iterate(obj[property]);
                }
            }
        }
    }

    public static cloneDTO(dto: any) {
        return JSON.parse(JSON.stringify(dto));
    }

    public static cloneDTOs(dtos: any[]) {
        let clones: any[] = [];
        if (dtos) {
            _.forEach(dtos, dto => {
                clones.push(this.cloneDTO(dto));
            });
        }

        return clones;
    }

    public static diffDTO(original: any, modified: any, skipKeys: string[] = [], toLower: boolean = false) {
        var diff = {}
        for (let key in original) {
            if (!_.includes(skipKeys, key) && !angular.equals(original[key], modified[key])) {
                diff[toLower ? key.toLowerCase() : key] = modified[key] == undefined ? null : modified[key];
            }
        }

        for (let key in modified) {
            if ((!_.includes(skipKeys, key)) && (!original.hasOwnProperty(key))) {
                diff[toLower ? key.toLowerCase() : key] = modified[key] == undefined ? null : modified[key];
            }
        }
        return diff;
    }

    public static toDTO(item: any, skipKeys: string[] = [], toLower: boolean = false) {
        var dto = {}
        for (var key in item) {
            dto[toLower ? key.toLowerCase() : key] = item[key];
        }
        return dto;
    }

    public static getSOEMessageBoxSizeString(size: SOEMessageBoxSize): string {
        var str = 'sm';
        if (size == SOEMessageBoxSize.Medium)
            str = 'md';
        else if (size == SOEMessageBoxSize.Large)
            str = 'lg';

        return str;
    }

    public static getCategoryType(feature: Feature): SoeCategoryType {
        var categoryType: SoeCategoryType = SoeCategoryType.Unknown;

        switch (soeConfig.feature) {
            case (Feature.Common_Categories_Product_Edit):
                categoryType = SoeCategoryType.Product;
                break;
            case (Feature.Common_Categories_Customer_Edit):
                categoryType = SoeCategoryType.Customer;
                break;
            case (Feature.Common_Categories_Supplier_Edit):
                categoryType = SoeCategoryType.Supplier;
                break;
            case (Feature.Common_Categories_ContactPersons_Edit):
                categoryType = SoeCategoryType.ContactPerson;
                break;
            case (Feature.Common_Categories_AttestRole_Edit):
                categoryType = SoeCategoryType.AttestRole;
                break;
            case (Feature.Common_Categories_Employee_Edit):
                categoryType = SoeCategoryType.Employee;
                break;
            case (Feature.Common_Categories_Project_Edit):
                categoryType = SoeCategoryType.Project;
                break;
            case (Feature.Common_Categories_Contract_Edit):
                categoryType = SoeCategoryType.Contract;
                break;
            case (Feature.Common_Categories_Inventory_Edit):
                categoryType = SoeCategoryType.Inventory;
                break;
            case (Feature.Common_Categories_Order_Edit):
                categoryType = SoeCategoryType.Order;
                break;
            case (Feature.Common_Categories_PayrollProduct_Edit):
                categoryType = SoeCategoryType.PayrollProduct;
                break;
        }

        return categoryType;
    }
}
