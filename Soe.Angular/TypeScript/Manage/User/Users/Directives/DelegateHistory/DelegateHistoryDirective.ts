import { UserCompanyRoleDelegateHistoryGridDTO, UserCompanyRoleDelegateHistoryUserDTO } from "../../../../../Common/Models/UserDTO";
import { EmbeddedGridController } from "../../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { Feature } from "../../../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { IUserService } from "../../../UserService";
import { DelegateHistoryDialogController } from "./DelegateHistoryDialogController";

export class DelegateHistoryDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Manage/User/Users/Directives/DelegateHistory/DelegateHistory.html'),
            scope: {
                userId: '=',
                isVisible: '=',
                readOnly: '='
            },
            restrict: 'E',
            replace: true,
            controller: DelegateHistoryController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class DelegateHistoryController {

    // Init parameters
    private userId: number;
    private isVisible: boolean;
    private readOnly: boolean;

    // Terms
    private terms: { [index: string]: string; };

    // Permissions
    private delegateOwnRolesAndAttestRolesModifyPermission: boolean = false;

    // Data
    private records: UserCompanyRoleDelegateHistoryGridDTO[] = [];

    // Grid
    private gridHandler: EmbeddedGridController;

    private progress: IProgressHandler;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private userService: IUserService,
        progressHandlerFactory: IProgressHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "DelegateHistoryGrid");
        this.gridHandler.gridAg.options.setMinRowsToShow(15);

        this.$q.all([
            this.loadModifyPermissions()
        ]).then(() => {
            this.setupGrid();
            this.setupWatchers();
        });
    }

    // SETUP

    private setupWatchers() {
        this.$scope.$watch(() => this.userId, (newVal, oldVal) => {
            this.loadModifyPermissions().then(() => {
                this.loadData();
            });
        });

        this.$scope.$watch(() => this.isVisible, (newVal, oldVal) => {
            if (newVal !== oldVal && this.records.length === 0)
                this.loadData();
        });
    }

    public setupGrid(): void {
        const keys: string[] = [
            "common.created",
            "common.from",
            "common.to",
            "common.role",
            "common.entitylogviewer.datalog.batchnbr",
            "common.user.attestrole",
            "manage.user.user.delegatehistory.fromuser",
            "manage.user.user.delegatehistory.touser",
            "manage.user.user.delegatehistory.byuser",
            "manage.user.user.delegatehistory.delete",
            "manage.user.user.delegatehistory.delete.warning"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.gridHandler.gridAg.options.useGrouping(false, false, { keepColumnsAfterGroup: true, selectChildren: false });
            this.gridHandler.gridAg.options.groupHideOpenParents = true;
            this.gridHandler.gridAg.options.addGroupTimeSpanSumAggFunction(true, true);

            const isDeletedFunc = (data: UserCompanyRoleDelegateHistoryGridDTO) => data && data.isDeleted;

            this.gridHandler.gridAg.addColumnText("userCompanyRoleDelegateHistoryHeadId", this.terms["common.entitylogviewer.datalog.batchnbr"], 50, false, { strikeThrough: isDeletedFunc, enableRowGrouping: true });
            this.gridHandler.gridAg.addColumnText("toUserName", this.terms["manage.user.user.delegatehistory.touser"], 150, false, { strikeThrough: isDeletedFunc, enableRowGrouping: true });
            this.gridHandler.gridAg.addColumnText("roleNames", this.terms["common.role"], null, false, { strikeThrough: isDeletedFunc, enableRowGrouping: true });
            this.gridHandler.gridAg.addColumnText("attestRoleNames", this.terms["common.user.attestrole"], null, false, { strikeThrough: isDeletedFunc, enableRowGrouping: true });
            this.gridHandler.gridAg.addColumnDate("dateFrom", this.terms["common.from"], 90, false, null, { strikeThrough: isDeletedFunc, enableRowGrouping: true });
            this.gridHandler.gridAg.addColumnDate("dateTo", this.terms["common.to"], 90, false, null, { strikeThrough: isDeletedFunc, enableRowGrouping: true });
            this.gridHandler.gridAg.addColumnText("fromUserName", this.terms["manage.user.user.delegatehistory.fromuser"], 150, false, { strikeThrough: isDeletedFunc, enableRowGrouping: true });
            this.gridHandler.gridAg.addColumnText("byUserName", this.terms["manage.user.user.delegatehistory.byuser"], 150, false, { strikeThrough: isDeletedFunc, enableRowGrouping: true });
            this.gridHandler.gridAg.addColumnDateTime("created", this.terms["common.created"], 110, false, null, { strikeThrough: isDeletedFunc, enableRowGrouping: true });
            if (!this.readOnly)
                this.gridHandler.gridAg.addColumnDelete(this.terms["manage.user.user.delegatehistory.delete"], this.deleteRow.bind(this), false, (row: UserCompanyRoleDelegateHistoryGridDTO) => { return row && row.showDelete });

            this.gridHandler.gridAg.finalizeInitGrid("manage.user.user.delegatehistory", true, "records-totals-grid");
        });
    }

    // SERVICE CALLS

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        if (this.userIsMySelf) {
            features.push(Feature.Manage_Users_Edit_Delegate_MySelf_OwnRolesAndAttestRoles);
        } else {
            features.push(Feature.Manage_Users_Edit_Delegate_OtherUsers_OwnRolesAndAttestRoles);
        }

        return this.coreService.hasModifyPermissions(features).then(x => {
            if (this.userIsMySelf) {
                this.delegateOwnRolesAndAttestRolesModifyPermission = x[Feature.Manage_Users_Edit_Delegate_MySelf_OwnRolesAndAttestRoles];
            } else {
                this.delegateOwnRolesAndAttestRolesModifyPermission = x[Feature.Manage_Users_Edit_Delegate_OtherUsers_OwnRolesAndAttestRoles];
            }
        });
    }

    private loadData() {
        this.userService.getUserCompanyRoleDelegateHistoryForUser(this.userId).then(x => {
            this.records = x;
            this.gridHandler.gridAg.setData(this.records);
        })
    }

    // EVENTS

    private addRow() {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/User/Users/Directives/DelegateHistory/DelegateHistoryDialog.html"),
            controller: DelegateHistoryDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                userId: () => { return this.userId }
            }
        }
        this.$uibModal.open(options).result.then(result => {
            if (result && result.targetUser) {
                this.save(result.targetUser);
            }
        });
    }

    private save(targetUser: UserCompanyRoleDelegateHistoryUserDTO) {
        this.progress.startSaveProgress((completion) => {
            this.userService.saveUserCompanyRoleDelegateHistory(targetUser, this.userId).then(result => {
                if (result.success) {
                    completion.completed();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null).then(data => {
            this.loadData();
        }, error => {
        });
    }

    private deleteRow(row: UserCompanyRoleDelegateHistoryGridDTO) {
        this.progress.startDeleteProgress((completion) => {
            this.userService.deleteUserCompanyRoleDelegateHistory(row.userCompanyRoleDelegateHistoryHeadId).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null, this.terms["manage.user.user.delegatehistory.delete.warning"]).then(x => {
            this.loadData();
        });
    }

    // HELP-METHODS

    private get userIsMySelf(): boolean {
        return this.userId === CoreUtility.userId;
    }
}
