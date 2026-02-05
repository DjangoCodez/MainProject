import { ICoreService } from "../../Core/Services/CoreService";
import { INotificationService } from "../../Core/Services/NotificationService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { IUserGaugeDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeModule, TermGroup_SysPageStatusSiteType } from "../../Util/CommonEnumerations";
import { IconLibrary, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../Util/Enumerations";
import { ToolBarButton, ToolBarButtonGroup } from "../../Util/ToolBarUtility";
import { DashboardWidgetSelectionController } from "../Dialogs/DashboardWidgetSelection/DashboardWidgetSelectionController";
import { DashboardSelectionController } from "./Dialogs/DashboardSelection/DashboardSelectionController";
import { DashboardSettingsController, UserGaugeHeadDTO } from "./Dialogs/DashboardSettings/DashboardSettingsController";

export class DashboardDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getViewUrl('dashboard.html'),
            scope: {
            },
            restrict: 'E',
            replace: true,
            controller: DashboardController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class DashboardController {
    private siteType: TermGroup_SysPageStatusSiteType;
    private assemblyDate: Date;
    private showAssemblyDate: boolean = false;
    private oldAssembly: boolean = false;

    private module: SoeModule;
    private widgets: WidgetModel[];
    private userGaugeHead: UserGaugeHeadDTO;
    private sortableOptions: any;

    private userGauges: IUserGaugeDTO[];
    private loadingWidgets: boolean = false;
    private isLoaded: boolean = false;

    // ToolBar
    private header: string;
    private buttonGroups = new Array<ToolBarButtonGroup>();

    // Timer
    private reloadTimer;
    readonly RELOAD_TIMER_INTERVAL: number = (60 * 1000 * 5);  // 5 minutes

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private $uibModal,
        private $q: ng.IQService,
        private $interval: ng.IIntervalService,
        private $scope: any) {

        // Config
        this.module = soeConfig.module;

        this.sortableOptions = {
            handle: '.panel-heading',
            placeholder: 'col-sm-1',
            'ui-floating': true,
            stop: this.onMove.bind(this)
        };

        this.setupToolBar();

        this.$q.all([
            this.getSiteType(),
            this.getAssemblyDate()
        ]).then(() => {
            if (this.siteType === TermGroup_SysPageStatusSiteType.Test)
                this.showAssemblyDate = true;
        });

        if (soeConfig.autoLoadOnStart)
            this.loadGaugeHead();
    }

    private setLabel() {
        let keys: string[] = [
            "common.dashboard.header"
        ];

        let moduleKey: string;
        switch (this.module) {
            case SoeModule.Economy:
                moduleKey = "common.dashboard.header.economy";
                break;
            case SoeModule.Billing:
                moduleKey = "common.dashboard.header.billing";
                break;
            case SoeModule.Time:
                moduleKey = "common.dashboard.header.time";
                break;
        }
        if (moduleKey)
            keys.push(moduleKey);

        this.translationService.translateMany(keys).then((terms) => {
            this.header = terms["common.dashboard.header"];
            if (moduleKey)
                this.header += " - " + terms[moduleKey];
        });
    }

    private setupToolBar() {
        this.setLabel();

        let gaugeButtonGroup = new ToolBarButtonGroup();
        gaugeButtonGroup.buttons.push(
        )
        this.buttonGroups.push(gaugeButtonGroup);

        
        let gaugeHeadButtonGroup = new ToolBarButtonGroup();
        gaugeHeadButtonGroup.buttons.push(
            new ToolBarButton(null, "common.dashboard.browse", IconLibrary.FontAwesome, "fa-search",
                () => this.browseDashboards(),
                () => false
            ),
            new ToolBarButton(null, "core.edit", IconLibrary.FontAwesome, "fa-pen",
                () => this.editDashboard(),
                () => !this.userGaugeHead || !this.userGaugeHead.userGaugeHeadId,
                () => !this.userGaugeHead || !this.userGaugeHead.userGaugeHeadId,
                { labelValue: this.userGaugeHead?.name }
            ),
            new ToolBarButton(null, "common.dashboard.addwidget", IconLibrary.FontAwesome, "fa-plus",
                () => this.addNew(),
                () => !this.isLoaded
            ),
            new ToolBarButton(null, "common.dashboard.reload", IconLibrary.FontAwesome, "fa-sync",
                () => this.reloadAll(),
                () => !this.widgets || this.widgets.length === 0,
            ),
        )
        this.buttonGroups.push(gaugeHeadButtonGroup);
    }

    private setupReloadTimer() {
        this.reloadTimer = this.$interval(() => {
            this.reloadAll();
        }, this.RELOAD_TIMER_INTERVAL);

        this.$scope.$on('$destroy', () => {
            this.$interval.cancel(this.reloadTimer);
        });
    }

    private getSiteType(): ng.IPromise<any> {
        return this.coreService.getSiteType().then(type => {
            this.siteType = type;
        });
    }
    private getAssemblyDate(): ng.IPromise<any> {
        return this.coreService.getAssemblyDate().then(d => {
            this.assemblyDate = CalendarUtility.convertToDate(d);
            if (this.assemblyDate.addHours(3).isBeforeOnMinute(new Date()))
                this.oldAssembly = true;
        });
    }

    private loadWidgets() {
        this.loadingWidgets = true;

        this.coreService.getUserGagues(this.module).then(data => {
            this.userGauges = data;
            this.widgets = _.sortBy(this.userGauges, 'sort').map(x => this.toWidget(x));
            this.isLoaded = true;
            this.loadingWidgets = false;

            this.setupReloadTimer();
        });
    }

    private loadGaugeHead(userGaugeHeadId?: number) {
        this.loadingWidgets = true;
        this.coreService.getUserGaugeHead(userGaugeHeadId, this.module).then(head => {
            this.userGaugeHead = head;
            this.userGauges = head.userGauges || [];

            if (head.name) {
                this.header = head.name;
            }

            this.widgets = _.sortBy(this.userGauges, 'sort').map(x => this.toWidget(x));

            this.isLoaded = true;
            this.loadingWidgets = false;

            this.setupReloadTimer();
        })
    }

    private toWidget(userGauge: IUserGaugeDTO) {
        return new WidgetModel(userGauge);
    }

    private onMove(e, ui) {
        this.refreshSortOrder();
    }

    private refreshSortOrder() {
        let sort: number = 0;
        _.forEach(this.widgets, widget => {
            widget.userGauge.sort = sort++;
            this.coreService.saveUserGaugeSort(widget.userGauge.userGaugeId, sort);
        })
    }

    private remove(widget) {
        var keys: string[] = [
            "common.dashboard.askremove.title",
            "common.dashboard.askremove.message"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["common.dashboard.askremove.title"], terms["common.dashboard.askremove.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this.widgets = this.widgets.filter(x => x !== widget);
                this.userGauges = this.userGauges.filter(x => x !== widget.userGauge);
                this.coreService.deleteUserGauge(widget.userGauge.userGaugeId);
            });
        });
    }

    private reloadAll() {
        this.$scope.$broadcast('reload');
    }

    private addNew() {
        this.showDialog(this.userGauges).result.then(result => {
            var maxSort = _.max(this.userGauges.map((ug) => ug.sort));
            var sort = (maxSort || 0) + 1;

            var future = result.map(r => {//TODO: Add all at once instead of multiple calls?
                return this.coreService.addUserGauge(r.sysGaugeId, sort++, this.module, this.userGaugeHead?.userGaugeHeadId || null).then(data => {
                    this.userGauges.push(data);
                    this.widgets.push(this.toWidget(data));
                });
            });

            this.$q.all(future).then(() => {
                this.refreshSortOrder(); //they might come back out of order, so we set a new order based on how they come back since that is how they will be shown to the user.
            });
        });
    }

    private browseDashboards() {
        let modal = DashboardSelectionController.openAsDialog(this.$uibModal, this.urlHelperService, this.module);
        modal.result.then(val => {
            if (val && val.userGaugeHeadId) {
                this.loadGaugeHead(val.userGaugeHeadId);
            }
        })
    }

    private createDashboard() {
        let modal = DashboardSettingsController.openAsDialog(this.$uibModal, this.urlHelperService, this.module, 0);
        modal.result.then(val => {
            if (val && val > 0) {
                this.loadGaugeHead(val);
            }
        })
    }

    private editDashboard() {
        let modal = DashboardSettingsController.openAsDialog(this.$uibModal, this.urlHelperService, this.module, this.userGaugeHead.userGaugeHeadId);
        modal.result.then(val => {
            if (val && val > 0) {
                this.loadGaugeHead(val);
            }
        })
    }

    private showDialog(userGauges: IUserGaugeDTO[]): any {
        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/DashboardWidgetSelection", "DashboardWidgetSelection.html"),
            controller: DashboardWidgetSelectionController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                module: () => { return this.module },
                userGauges: () => { return userGauges },
            }
        });

        return modal;
    }
}

export class WidgetModel {
    public directive: string;
    public userGauge: IUserGaugeDTO;

    constructor(userGauge: IUserGaugeDTO) {
        this.directive = userGauge.sysGaugeName;
        this.userGauge = userGauge;
    }
}