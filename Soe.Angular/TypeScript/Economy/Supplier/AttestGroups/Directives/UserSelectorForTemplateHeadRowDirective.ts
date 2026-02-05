import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { IAttestWorkFlowTemplateRowDTO, IAttestWorkFlowHeadDTO, IUserSmallDTO, IAttestRoleDTO, IAttestWorkFlowRowDTO, ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { SupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { Feature, TermGroup, TermGroup_AttestWorkFlowRowProcessType } from "../../../../Util/CommonEnumerations";
import { AttestEmployeeDayTimeInvoiceTransactionDTO } from "../../../../Common/Models/TimeEmployeeTreeDTO";

export class UserSelectorForTemplateHeadRowDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getDirectiveViewUrl('UserSelectorForTemplateHeadRow.html'),
            scope: {
                row: '=',
                head: '=',
                mode: '=',
                registerControl: '&' //yes, i could have used require, but since we will use this on multiple places with different controllers this seemed easier.
            },
            restrict: 'E',
            replace: true,
            controller: UserSelectorForTemplateHeadRowDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class UserSelectorForTemplateHeadRowDirectiveController extends GridControllerBase {
    private row: IAttestWorkFlowTemplateRowDTO;
    private head: IAttestWorkFlowHeadDTO;
    private terms: any;
    private mode: number;
    private registerControl: Function;

    private relevantRows: any[];
    private checkableUsers: Checkable<IUserSmallDTO>[];
    private checkableRoles: Checkable<IUserSmallDTO>[];
    private loginNameColumn: uiGrid.IColumnDef;
    private attestWorkFlowTypes: ISmallGenericType[] = [];

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private supplierService: SupplierService,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("NoName", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public $onInit() {
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.setMinRowsToShow(8);

        this.$q.all([this.loadTerms(), this.loadAttestWorkFlowTypes(), this.loadData()]).then(() => this.setupGridColumns());

        if (this.registerControl)
            this.registerControl({ control: this });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.categories.selected",
            "common.name",
            "common.user"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private setupGridColumns() {
        this.soeGridOptions.addColumnBool("checked", this.terms["common.categories.selected"], "15%", true, "rowSelected");
        this.soeGridOptions.addColumnText("entity.name", this.terms["common.name"], null, undefined, undefined, ' ');//empty tooltip changes what template we use, and we want to use the tooltip one
        this.loginNameColumn = this.soeGridOptions.addColumnText("entity.loginName", this.terms["common.user"], null, undefined, undefined, ' ');

        this.updateGridData();
        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.mode, this.updateGridData.bind(this));
    }

    private loadAttestWorkFlowTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AttestWorkFlowType, false, false).then((data) => {
            this.attestWorkFlowTypes = data;
        });
    }

    private loadData() {
        var userPromise = this.supplierService.getAttestWorkFlowUsersByAttestTransitionId(this.row.attestTransitionId).then((data: IUserSmallDTO[]) => {
            this.checkableUsers = data.map(u => new Checkable(u));

            if (!this.head.rows)
                return;

            this.relevantRows = this.head.rows.filter(r => r.attestTransitionId === this.row.attestTransitionId && (<any>r.processType) !== TermGroup_AttestWorkFlowRowProcessType.Registered);

            this.relevantRows.forEach(rr => {
                var user = _.find(this.checkableUsers, u => u.entity.userId === rr.userId);

                if (user)
                    user.checked = true;
            });
        });

        var rolePromise = this.supplierService.getAttestWorkFlowAttestRolesByAttestTransitionId(this.row.attestTransitionId).then((data: IAttestRoleDTO[]) => {
            this.checkableRoles = data.map(r => {
                return <IUserSmallDTO>{ userId: 0, name: r.name, attestRoleId: r.attestRoleId };
            }).map(u => new Checkable(u));

            if (!this.head.rows)
                return;

            this.relevantRows = this.head.rows.filter(r => r.attestTransitionId === this.row.attestTransitionId && (<any>r.processType) !== TermGroup_AttestWorkFlowRowProcessType.Registered);

            this.relevantRows.forEach(rr => {
                var role = _.find(this.checkableRoles, u => u.entity.attestRoleId === rr.attestRoleId);

                if (role)
                    role.checked = true;
            });
        });

        return this.$q.all([userPromise, rolePromise]);
    }

    public rowSelected(row) {
        this.updateGridData();
    }

    public getRowsToSave() {
        var rows = [];
        var row: IAttestWorkFlowRowDTO;

        var attestUsers = (this.mode === 0 ? this.checkableUsers : this.checkableRoles).filter(cu => cu.checked).map(cu => cu.entity);
        attestUsers.forEach(user => {
            row = null;
            if (user.attestFlowRowId !== 0) {
                row = _.find(this.head.rows, r => r.attestWorkFlowRowId === user.attestFlowRowId);
            }

            if (!row) {
                row = <IAttestWorkFlowRowDTO>{};
            }

            row.attestTransitionId = this.row.attestTransitionId;
            row.attestWorkFlowRowId = user.attestFlowRowId;
            row.type = this.row.type;

            if (!user.userId) {
                row.attestRoleId = user.attestRoleId;
                row.userId = null;
            } else {
                row.userId = user.userId;
            }

            rows.push(row);
        });

        return rows;
    }

    public getAttestTransitionId() {
        return this.row.attestTransitionId;
    }

    private updateGridData() {
        var data;

        this.$timeout(() => {
            if (this.mode === 0) {
                this.loginNameColumn.visible = true;
                data = _.orderBy(this.checkableUsers, ['checked', 'entity.name'], ['desc', 'asc']);
                super.gridDataLoaded(data);
            } else {
                this.loginNameColumn.visible = false;
                data = _.orderBy(this.checkableRoles, ['checked', 'entity.name'], ['desc', 'asc']);
                super.gridDataLoaded(data);
            }
        });
    }
}

class Checkable<T> {
    public entity: T;
    public checked: boolean;

    constructor(entity: T) {
        this.entity = entity;
        this.checked = false;
    }
}