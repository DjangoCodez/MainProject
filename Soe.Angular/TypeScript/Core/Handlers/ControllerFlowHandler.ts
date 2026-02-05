import { IToolbarFactory } from "./ToolbarFactory";
import { ICoreService } from "../Services/CoreService";
import { Feature } from "../../Util/CommonEnumerations";

export interface IControllerFlowHandler<T extends IControllerFlowHandler<any>> {
    start(permission: IPermissionLoadRequest[] | IPermissionLoadRequest): void;
    onPermissionsLoaded(callback: (Feature: Feature, readPermission?: boolean, modifyPermission?: boolean) => void): T;
    onAllPermissionsLoaded(callback: (response: IPermissionRetrievalResponse) => void): T;
    onDoLookUp(callback: () => ng.IPromise<any>): T;
    onCreateToolbar(callback: (toolbarFactory: IToolbarFactory) => void): T;
    starting(): ng.IPromise<any>;
}

export interface IEditControllerFlowHandler extends IControllerFlowHandler<IEditControllerFlowHandler> {
    onLoadData(callback: () => ng.IPromise<any>): IEditControllerFlowHandler;
    onSetUpGUI(callback: () => void): EditControllerFlowHandler
    onAfterFirstLoad(callback: () => void): EditControllerFlowHandler
}

export interface IGridControllerFlowHandler extends IControllerFlowHandler<IGridControllerFlowHandler> {
    onLoadSettings(callback: () => ng.IPromise<any>): IGridControllerFlowHandler;
    onBeforeSetUpGrid(callback: () => ng.IPromise<any>): IGridControllerFlowHandler;
    onSetUpGrid(callback: () => void): IGridControllerFlowHandler;
    onLoadGridData(callback: () => void): IGridControllerFlowHandler;
}

export interface IPermissionLoadRequest {
    feature: Feature;
    loadReadPermissions?: boolean;
    loadModifyPermissions?: boolean;
}

export interface IPermissionRetrievalResponse {
    [feature: number]: { readPermission?: boolean, modifyPermission?: boolean }
}

export abstract class ControllerFlowHandlerBase {
    private startingPromise: ng.IPromise<any>;

    private callbacks: {
        onPermissionsLoaded: ((Feature: Feature, readPermission?: boolean, modifyPermission?: boolean) => void)[],
        onAllPermissionsLoaded: ((response: IPermissionRetrievalResponse) => void)[],
        onDoLookUps: (() => ng.IPromise<any>)[],
        onCreateToolbar: ((toolbarFactory: IToolbarFactory) => void)[]
    } = {
            onPermissionsLoaded: [],
            onAllPermissionsLoaded: [],
            onDoLookUps: [],
            onCreateToolbar: []
        };

    constructor(protected $q: ng.IQService, private coreService: ICoreService, private toolbarFactory: IToolbarFactory) {
    }

    start(permission: IPermissionLoadRequest[] | IPermissionLoadRequest): void {
        this.startingPromise = this.startCore(permission);
    }
    
    onPermissionsLoaded(callback: (Feature: Feature, readPermission?: boolean, modifyPermission?: boolean) => void) {
        this.callbacks.onPermissionsLoaded.push(callback);
        return this;
    }

    onAllPermissionsLoaded(callback: (response: IPermissionRetrievalResponse) => void) {
        this.callbacks.onAllPermissionsLoaded.push(callback);
        return this;
    }

    onDoLookUp(callback: () => ng.IPromise<any>) {
        this.callbacks.onDoLookUps.push(callback);
        return this;
    }

    onCreateToolbar(callback: (toolbarFactory: IToolbarFactory) => void) {
        this.callbacks.onCreateToolbar.push(callback);
        return this;
    }

    starting(): ng.IPromise<any> {
        return this.startingPromise;
    }

    protected abstract startCore(permission: IPermissionLoadRequest[] | IPermissionLoadRequest): ng.IPromise<any>;
    
    protected loadPermissions(permission: IPermissionLoadRequest[] | IPermissionLoadRequest): ng.IPromise<any> {
        if (permission instanceof Array) {
            return this.loadManyPermissions(<IPermissionLoadRequest[]>permission).then(x => {
                _.each(this.callbacks.onAllPermissionsLoaded, y => {
                    y(x);
                });
            });
        } else {
            return this.loadSinglePermission(<IPermissionLoadRequest>permission).then(x => {
                _.each(this.callbacks.onAllPermissionsLoaded, y => {
                    y(x);
                });
            });
        }
    }

    protected doLookups() {
        var deferral = this.$q.defer();

        if (this.callbacks.onDoLookUps.length > 0) {
            let promises: ng.IPromise<any>[] = [];
            _.each(this.callbacks.onDoLookUps, x => {
                let promise = x();
                if (promise) {
                    promises.push(promise);
                }
            });
            this.$q.all(promises).then(() => {
                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    protected createToolbar() {
        _.each(this.callbacks.onCreateToolbar, x => {
            x(this.toolbarFactory);
        });
    }

    private loadManyPermissions(requests: IPermissionLoadRequest[]) {
        let deferral = this.$q.defer<IPermissionRetrievalResponse>()

        var featureIds: number[] = [];
        requests.forEach(x => featureIds.push(x.feature));

        let result: IPermissionRetrievalResponse = {};

        if (_.find(requests, x => x.loadReadPermissions)) { // At least one request requires read
            this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
                requests.forEach(y => {
                    result[y.feature] = y.loadReadPermissions ? { readPermission: !!x[y.feature] } : {};
                });

                if (_.find(requests, r => r.loadModifyPermissions)) { // At least one request requires modify
                    this.coreService.hasModifyPermissions(featureIds).then((f) => {
                        requests.forEach(y => {
                            if (y.loadModifyPermissions) {
                                result[y.feature].modifyPermission = !!f[y.feature];
                            }
                        });

                        requests.forEach(z => {
                            this.permissionsLoaded(z.feature, result[z.feature]);
                        });
                        deferral.resolve(result);
                    });
                } else {
                    requests.forEach(z => {
                        this.permissionsLoaded(z.feature, result[z.feature]);
                    });
                    deferral.resolve(result);
                }

            });
        } else if (_.find(requests, x => x.loadModifyPermissions)) { // At least one request requires modify
            this.coreService.hasModifyPermissions(featureIds).then((x) => {
                requests.forEach(y => {
                    let modify = y.loadModifyPermissions ? !!x[y.feature] : undefined;
                    result[y.feature] = { modifyPermission: modify };
                });

                requests.forEach(z => {
                    this.permissionsLoaded(z.feature, result[z.feature]);
                });
                deferral.resolve(result);
            });
        } else {
            deferral.reject("Have to request at least one type of permission retrieval");
        }
        return deferral.promise;
    }

    private loadSinglePermission(request: IPermissionLoadRequest): ng.IPromise<IPermissionRetrievalResponse> {
        var deferral = this.$q.defer<IPermissionRetrievalResponse>();

        let result: IPermissionRetrievalResponse = {};
        result[request.feature] = {};

        var featureIds: number[] = [];
        featureIds.push(request.feature);
        
        if (request.loadReadPermissions) {
            this.coreService.hasReadOnlyPermissions(featureIds).then(r => {
                result[request.feature].readPermission = !!r[request.feature];

                if (request.loadModifyPermissions) {
                    this.coreService.hasModifyPermissions(featureIds).then(m => {
                        result[request.feature].modifyPermission = !!m[request.feature]

                        this.permissionsLoaded(request.feature, result[request.feature]);

                        deferral.resolve(result);
                    });
                } else {
                    this.permissionsLoaded(request.feature, result[request.feature]);

                    deferral.resolve(result);
                }

            });
        } else if (request.loadModifyPermissions) {
            this.coreService.hasModifyPermissions(featureIds).then(m => {
                result[request.feature].modifyPermission = !!m[request.feature];

                this.permissionsLoaded(request.feature, result[request.feature]);

                deferral.resolve(result);
            });
        } else {
            deferral.reject("Have to request at least one type of permission retrieval");
        }
        return deferral.promise;
    }

    private permissionsLoaded(feature: Feature, permissions: { readPermission?: boolean, modifyPermission?: boolean }) {
        _.each(this.callbacks.onPermissionsLoaded, y => {
            y(feature, permissions.readPermission, permissions.modifyPermission);
        });
    }
}

export class EditControllerFlowHandler extends ControllerFlowHandlerBase implements IEditControllerFlowHandler {
    private onLoadDataCallback: (() => ng.IPromise<any>)[] = [];
    private onSetUpGUICallback: (() => void)[] = [];
    private onAfterFirstLoadCallback: (() => void)[] = [];

    constructor($q: ng.IQService, coreService: ICoreService, toolbarFactory: IToolbarFactory) {
        super($q, coreService, toolbarFactory);
    }

    public startCore(permission: IPermissionLoadRequest[] | IPermissionLoadRequest): ng.IPromise<any> {
        return this.loadPermissions(permission).then(() => {
            this.setUpGUI();
            return this.doLookups().then(() => {
                return this.loadData().then(() => {
                    this.afterFirstLoad();
                    this.createToolbar();
                });
            });
        });
    }

    public onSetUpGUI(callback: () => void): EditControllerFlowHandler {
        this.onSetUpGUICallback.push(callback);
        return this;
    }

    public onLoadData(callback: () => ng.IPromise<any>): EditControllerFlowHandler {
        this.onLoadDataCallback.push(callback);
        return this;
    }

    public onAfterFirstLoad(callback: () => void): EditControllerFlowHandler {
        this.onAfterFirstLoadCallback.push(callback);
        return this;
    }
    
    private setUpGUI() {
        if (this.onSetUpGUICallback.length > 0) {
            _.each(this.onSetUpGUICallback, x => {
                x();
            });
        };
    }

    private afterFirstLoad() {
        if (this.onAfterFirstLoadCallback.length > 0) {
            _.each(this.onAfterFirstLoadCallback, x => {
                x();
            });
        };
    }

    private loadData() {
        var deferral = this.$q.defer();

        if (this.onLoadDataCallback.length > 0) {
            let promises: ng.IPromise<any>[] = [];
            _.each(this.onLoadDataCallback, x => {
                let promise = x();
                if (promise) {
                    promises.push(promise);
                }
            });
            this.$q.all(promises).then(() => {
                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }
}

export class GridControllerFlowHandler extends ControllerFlowHandlerBase implements IGridControllerFlowHandler {
    private onLoadSettingsCallback: (() => ng.IPromise<any>)[] = [];
    private onBeforeSetUpGridCallback: (() => ng.IPromise<any>)[] = [];
    private onSetUpGridCallback: (() => void)[] = [];
    private onLoadGridDataCallback: (() => void)[] = [];

    constructor($q: ng.IQService, coreService: ICoreService, toolbarFactory: IToolbarFactory, private $timeout: ng.ITimeoutService) {
        super($q, coreService, toolbarFactory);
    }

    public startCore(permission: IPermissionLoadRequest[] | IPermissionLoadRequest): ng.IPromise<any> {
        return this.loadPermissions(permission).then(() => {
            this.beforeSetUpGrid();
            return this.doLookups().then(() => {
                this.createToolbar();
            });
        });
    }

    public onLoadSettings(callback: () => ng.IPromise<any>): GridControllerFlowHandler {
        this.onLoadSettingsCallback.push(callback);
        return this;
    }

    public onBeforeSetUpGrid(callback: () => ng.IPromise<any>): GridControllerFlowHandler {
        this.onBeforeSetUpGridCallback.push(callback);
        return this;
    }

    public onSetUpGrid(callback: () => void): GridControllerFlowHandler {
        this.onSetUpGridCallback.push(callback);
        return this;
    }

    public onLoadGridData(callback: () => void): GridControllerFlowHandler {
        this.onLoadGridDataCallback.push(callback);
        return this;
    }

    private beforeSetUpGrid() {
        if (this.onLoadSettingsCallback.length > 0) {
            let settingspromises: ng.IPromise<any>[] = [];
            _.each(this.onLoadSettingsCallback, x => {
                let promise = x();
                if (promise) {
                    settingspromises.push(promise);
                }
            });

            this.$q.all(settingspromises).then(() => {
                if (this.onBeforeSetUpGridCallback.length > 0) {
                    let setuppromises: ng.IPromise<any>[] = [];
                    _.each(this.onBeforeSetUpGridCallback, x => {
                        let promise = x();
                        if (promise) {
                            setuppromises.push(promise);
                        }
                    });
                    this.$q.all(setuppromises).then(() => {
                        this.setUpGrid();
                    });
                } else {
                    this.setUpGrid();
                }
            });
        }
        else {
            if (this.onBeforeSetUpGridCallback.length > 0) {
                let promises: ng.IPromise<any>[] = [];
                _.each(this.onBeforeSetUpGridCallback, x => {
                    let promise = x();
                    if (promise) {
                        promises.push(promise);
                    }
                });
                this.$q.all(promises).then(() => {
                    this.setUpGrid();
                });
            } else {
                this.setUpGrid();
            }
        }
    }

    private setUpGrid() {
        //timeout because of problem with state restore...
        this.$timeout(() => {
            if (this.onSetUpGridCallback.length > 0) {
                _.each(this.onSetUpGridCallback, x => {
                    x();
                });
                this.loadGridData();
            } else {
                this.loadGridData();
            }
        })
    }

    private loadGridData() {
        if (this.onLoadGridDataCallback.length > 0) {
            _.each(this.onLoadGridDataCallback, x => {
                x();
            });
        }
    }
}
