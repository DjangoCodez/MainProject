import { IHttpService } from "../../Core/Services/HttpService";
import { SoeLogType, SoeFieldSettingType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { IFieldSettingDTO } from "../../Scripts/TypeLite.Net4";

export interface IPreferencesService {

    // GET
    getFieldSettings(fieldSettingsType: SoeFieldSettingType): ng.IPromise<any>
    getAreas(): ng.IPromise<any>

    // POST
    saveFieldSetting(fieldSettingDTO: IFieldSettingDTO): ng.IPromise<any>
    checkSettings(areas: number[]): ng.IPromise<any>
}

export class PreferencesService implements IPreferencesService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getFieldSettings(fieldSettingsType: SoeFieldSettingType) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_PREFERENCES_FIELDSETTINGS + fieldSettingsType, false);
    }

    getAreas() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_PREFERENCES_SETTINGS_AREAS, false);
    }

    // POST
    saveFieldSetting(fieldSettingDTO: IFieldSettingDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_PREFERENCES_FIELDSETTINGS, fieldSettingDTO);
    }

    checkSettings(areas: number[]) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_PREFERENCES_SETTINGS_CHECK, areas);
    }
}
