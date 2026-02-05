import { ITranslationService } from "./TranslationService";
import { CoreUtility } from "../../Util/CoreUtility";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { IAuthenticationService } from "./AuthenticationService";
import { IStorageService } from "./StorageService";
import { Guid } from "../../Util/StringUtility";

export interface IHttpService {
    get(url: string, useCache: boolean, acceptType?: string, skipRefreshSession?: boolean): ng.IPromise<any>;
    getCache(path: string, acceptType?: string, minutesToExpire?: number, forceRefreshCache?: boolean, skipRefreshSession?: boolean): ng.IPromise<any>;
    post(url: string, values: any, skipRefreshSession?: boolean): ng.IPromise<any>;
    delete(url: string, skipRefreshSession?: boolean): ng.IPromise<any>;
}

export interface IHttpServiceProvider {
    setUserToken(userToken: string): void;
    setPrefix(prefix: string): void;
    setLanguage(language: string): void;
    setSoeParameters(parameters: string);
}

export class HttpServiceProvider implements IHttpServiceProvider {

    private userToken: string;
    private prefix: string;
    private language: string = "not set";
    private soeParameters: string;

    setPrefix(prefix: string) {
        this.prefix = prefix;
    }

    setUserToken(userToken: string): void {
        this.userToken = userToken;
    }

    setLanguage(language: string) {
        this.language = language;
    }

    setSoeParameters(parameters: string) {
        this.soeParameters = parameters;
    }

    //@ngInject
    $get($http, $q, $log, $timeout: ng.ITimeoutService, translationService: ITranslationService, storageService: IStorageService, authenticationService: IAuthenticationService): any {
        if (this.userToken)
            return new HttpService($http, $q, $log, $timeout, translationService, storageService, authenticationService, this.prefix, this.userToken, this.language, this.soeParameters);
        else
            return new OidcHttpService($http, $q, $log, $timeout, translationService, storageService, authenticationService, this.prefix, this.language, this.soeParameters);
    }
}

export class HttpService implements IHttpService {
    constructor(
        private $http: ng.IHttpService, private $q: ng.IQService, private $log: ng.ILogService, private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private storageService: IStorageService,
        private authenticationService: IAuthenticationService,
        private prefix: string, private userToken: string, language: string, soeParams: string) {

        $log.info("Using HttpService");

        $http.defaults.headers.common['Accept-Language'] = language;

        // !!! If adding more headers here, you also currently need to add them in GoogleMapsService.clearHeaders() and setHeaders() !!!
        $http.defaults.headers.common.Authorization = 'Bearer ' + userToken;

        $http.defaults.headers.common['soeparameters'] = soeParams;

        //$http.defaults.headers.common['LicenseId'] = licenseId;
        //$http.defaults.headers.common['LicenseNr'] = licenseNr;
        //$http.defaults.headers.common['ActorCompanyId'] = actorCompanyId;
        //$http.defaults.headers.common['RoleId'] = roleId;
        //$http.defaults.headers.common['UserId'] = userId;
        //$http.defaults.headers.common['LoginName'] = loginName;
        //$http.defaults.headers.common['SysCountryId'] = sysCountryId;
        //$http.defaults.headers.common['IsSupportAdmin'] = isSupportAdmin;
        //$http.defaults.headers.common['IsSupportSuperAdmin'] = isSupportSuperAdmin;
    }

    get(path: string, useCache: boolean, acceptType?: string, skipRefreshSession?: boolean): ng.IPromise<any> {
        var deferral = this.$q.defer();

        //this.$timeout(() => {
        var config = { cache: useCache };
        if (acceptType) {
            config['headers'] = { 'Accept': acceptType };
        }

        this.$http.get(this.prefix + path, config).then(obj => {
            deferral.resolve(obj.data);
            return null;
        }).catch(error => {
            this.handleError(error, deferral, 'get:' + path);
        });
        //}, 0); // TODO: Add configurable parameter

        if (!skipRefreshSession)
            this.refreshSession();

        return deferral.promise;
    }

    getCache(path: string, acceptType?: string, minutesToExpire?: number, forceRefreshCache?: boolean, skipRefreshSession?: boolean): ng.IPromise<any> {

        var deferral = this.$q.defer();
        var timeToLive = minutesToExpire != null ? minutesToExpire : 2;

        this.$timeout(() => {
            var config = { cache: false };
            if (acceptType) {
                config['headers'] = { 'Accept': acceptType };
            }

            var key = this.storageService.createCompanyObjectKey(CoreUtility.actorCompanyId.toString(), path);
            var cache = this.storageService.fetch(key);

            if (cache && !forceRefreshCache) {
                deferral.resolve(cache);
            }
            else {
                this.$http.get(this.prefix + path, config).then(obj => {
                    deferral.resolve(obj.data);
                    this.storageService.clearOld(key);
                    this.storageService.add(key, obj.data, CalendarUtility.getDateNow().addMinutes(timeToLive));
                }).catch(error => {
                    this.handleError(error, deferral, 'getCache:' + path);
                });
            }
        }, 0);

        if (!skipRefreshSession)
            this.refreshSession();

        return deferral.promise;
    }

    post(path: string, values, skipRefreshSession?: boolean): ng.IPromise<any> {
        var deferral = this.$q.defer();

        var key = this.storageService.createCompanyObjectKey(CoreUtility.actorCompanyId.toString(), path);
        this.storageService.remove(key);

        this.$http.post(this.prefix + path, values).then(obj => {
            deferral.resolve(obj.data);
            return null;
        }).catch(error => {
            this.handleError(error, deferral, 'post:' + path);
        });

        if (!skipRefreshSession)
            this.refreshSession();

        return deferral.promise;
    }

    delete(path: string, skipRefreshSession?: boolean): ng.IPromise<any> {
        var deferral = this.$q.defer();
        this.$http.delete(this.prefix + path).then(obj => {
            deferral.resolve(obj.data);
            return null;
        }).catch(error => {
            this.handleError(error, deferral, 'delete:' + path);
        });

        if (!skipRefreshSession)
            this.refreshSession();

        return deferral.promise;
    }

    private handleError(error: any, deferral: ng.IDeferred<any>, method: string) {
        if (error.data && error.data.message) {
            this.$log.error("Error: ", error.data.message, error);
        }

        if (!error.data) {
            console.log("Try to handle error with no data! Method: " + method, error);
            deferral.reject({ error: error });
        } else {
            if (!error.data.translationKey) {
                error.data.translationKey = "error.default_error";
            }

            this.translationService.translate(error.data.translationKey).then((str) => {
                deferral.reject({ error: error.data, message: str });
            }).catch((err) => {
                deferral.reject({ error: err, originalError: error.data, message: error.message });
            });
        }
    }

    private refreshSession() {
        this.authenticationService.requestSessionRefresh();
    }
}

export class OidcHttpService implements IHttpService {
    constructor(
        private $http: ng.IHttpService, private $q: ng.IQService, private $log: ng.ILogService, private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService, private storageService: IStorageService, private authenticationService: IAuthenticationService,
        private prefix: string, language: string, soeParams: string) {

        $log.info("Using OidcHttpService");

        $http.defaults.headers.common['Accept-Language'] = language;
        $http.defaults.headers.common['soeparameters'] = soeParams;

        // !!! If adding more headers here, you also currently need to add them in GoogleMapsService.clearHeaders() and setHeaders() !!!
        //$http.defaults.headers.common['LicenseId'] = licenseId;
        //$http.defaults.headers.common['LicenseNr'] = licenseNr;
        //$http.defaults.headers.common['ActorCompanyId'] = actorCompanyId;
        //$http.defaults.headers.common['RoleId'] = roleId;
        //$http.defaults.headers.common['UserId'] = userId;
        //$http.defaults.headers.common['LoginName'] = loginName;
        //$http.defaults.headers.common['SysCountryId'] = sysCountryId;
        //$http.defaults.headers.common['IsSupportAdmin'] = isSupportAdmin;
        //$http.defaults.headers.common['IsSupportSuperAdmin'] = isSupportSuperAdmin;
    }

    get(path: string, useCache: boolean, acceptType?: string, skipRefreshSession?: boolean): ng.IPromise<any> {
        var deferral = this.$q.defer();

        var config = { cache: useCache };
        if (acceptType) {
            config['headers'] = { 'Accept': acceptType };
        }

        this.$http.get(this.prefix + path, config).then(obj => {
            deferral.resolve(obj.data);
            return null;
        }).catch(error => {
            this.handleError(error, deferral, 'get:' + path);
        });

        return deferral.promise;
    }

    getCache(path: string, acceptType?: string, minutesToExpire?: number, forceRefreshCache?: boolean, skipRefreshSession?: boolean): ng.IPromise<any> {
        var deferral = this.$q.defer();
        var timeToLive = minutesToExpire != null ? minutesToExpire : 2;

        var config = { cache: false };

        var key = this.storageService.createCompanyObjectKey(CoreUtility.actorCompanyId.toString(), path);
        var cache = this.storageService.fetch(key);

        if (cache != null && !forceRefreshCache) {
            deferral.resolve(cache);
        }
        else {
            this.get(path, false, acceptType).then(obj => {
                try {
                    this.storageService.clearOld(key);
                    this.storageService.add(key, obj, CalendarUtility.getDateNow().addMinutes(timeToLive));
                }
                catch (error) {
                    console.log("Failed to add to local storage: ", path);
                }
                finally {
                    deferral.resolve(obj);
                }
            }).catch(error => {
                this.handleError(error, deferral, 'GetCached:' + path);
            });
        }

        if (!skipRefreshSession)
            this.refreshSession();

        return deferral.promise;
    }

    post(path: string, values, skipRefreshSession?: boolean): ng.IPromise<any> {
        var deferral = this.$q.defer();

        var appendTransform = function appendTransform(defaults, transform) {

            // We can't guarantee that the default transformation is an array
            defaults = angular.isArray(defaults) ? defaults : [defaults];

            // Append the new transformation to the defaults
            return defaults.concat(transform);
        };

        var iterate = function (obj) {
            for (var key in obj) {
                if (!obj.hasOwnProperty(key)) continue;

                if (typeof obj[key] == "object") {
                    iterate(obj[key]);
                }

                if (typeof obj[key] == "string") {

                    var regexIso8601 = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/;

                    if (regexIso8601.test(obj[key]) && obj[key] !== "0001-01-01T00:00:00.000Z") {
                        var value = new Date(obj[key]);
                        obj[key] = value.toISODateTimeString();
                    }
                }
            }
        };

        var transformDates = function (data) {
            var obj = angular.fromJson(data);

            iterate(obj);

            return angular.toJson(obj);
        };

        var config = { headers: { 'PostGuid': Guid.newGuid() }, transformRequest: appendTransform(this.$http.defaults.transformRequest, transformDates) };
        //var config = { headers: { 'Authorization': 'Bearer ' + token } };

        this.$http.post(this.prefix + path, values, config).then(obj => {
            deferral.resolve(obj.data);
            return null;
        }).catch(error => {
            this.handleError(error, deferral, "Post " + path);
        });
        return deferral.promise;
    }

    delete(path: string, skipRefreshSession?: boolean): ng.IPromise<any> {
        var deferral = this.$q.defer();
        this.$http.delete(this.prefix + path).then(obj => {
            deferral.resolve(obj.data);
            return null;
        }).catch(error => {
            this.handleError(error, deferral, 'delete:' + path);
        });

        return deferral.promise;
    }

    private handleError(error: any, deferral: ng.IDeferred<any>, method: string) {
        if (error.data && error.data.message) {
            this.$log.error("Error: ", error.data.message, error);
        }

        if (!error.data) {
            console.log("Try to handle error with no data! Method: " + method, error);
            deferral.reject({ error: error });
        } else {
            if (!error.data.translationKey) {
                error.data.translationKey = "error.default_error";
            }

            this.translationService.translate(error.data.translationKey).then((str) => {
                deferral.reject({ error: error.data, message: str });
            }).catch((err) => {
                deferral.reject({ error: err, originalError: error.data, message: error.message });
            });
        }
    }

    private refreshSession() {
        this.authenticationService.requestSessionRefresh();
    }
} 