import { CalendarUtility } from "../../Util/CalendarUtility";

declare var soe: any;

interface IUser {
    access_token: string;
}

export interface IAuthenticationServiceProvider {
    setRefreshTimeout(timeoutSecs: number);
}

export interface IAuthenticationService {
    getAccessToken(): ng.IPromise<string>;
    requestSessionRefresh(): void;
}

export class AuthenticationServiceProvider implements IAuthenticationServiceProvider, ng.IServiceProvider {
    private refreshTimeout = 5 * 60;

    public setRefreshTimeout(timeoutSecs: number) {
        this.refreshTimeout = timeoutSecs;
    }

    //@ngInject
    $get($http: ng.IHttpService, $log: ng.ILogService, $q: ng.IQService, $interval: ng.IIntervalService) {
        return new AuthenticationService($log, $q, $interval, this.refreshTimeout);
    }
}

export class AuthenticationService implements IAuthenticationService {
    private userPromise: ng.IPromise<IUser>;
    private refreshRequested = false;

    //@ngInject
    constructor(private $log: ng.ILogService, private $q: ng.IQService, $interval: ng.IIntervalService, private refreshInterval) {
        if (window['soe'] && soe.userManager) {
            //$log.error("Missing soe.userManager");
            //will be missing while using HttpService instead of OidcHttpService
            //and Legacy Login
            soe.userManager.events.addSilentRenewError((e) => this.onSilentRenewError());
        }
        
        // No need to do it the first time as 
        // SessionTimeout.js does a refresh on page load
        ((first) => $interval(() => {
            if (this.refreshRequested && !first) {
                this.refreshSession();
            }
            first = false;
            this.refreshRequested = false;
        }, this.refreshInterval * 1000))(true)
    }

    public getAccessToken(): ng.IPromise<string> {
        return this.getUser().then(user => user.access_token);
    }
    public requestSessionRefresh() {
        this.refreshRequested = true;
    }

    private getUser(): ng.IPromise<IUser> {
        return this.userPromise;
    }
    private refreshSession() {
        this.$log.debug("Refreshing user session using refreshSession");
        window['refreshSession'](true)
    }

    private onSilentRenewError() {
        this.$log.error("Failed to renew access token silently");
        window['doLogout']()
    }
}