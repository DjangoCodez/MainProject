import { ICoreService } from "../../Core/Services/CoreService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { GridControllerBase2Ag } from "../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../Core/Handlers/ToolbarFactory";
import { Feature, SoeEntityType, TermGroup } from "../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../Core/Handlers/ControllerFlowHandler";
import { ToolBarButton, ToolBarUtility } from "../../Util/ToolBarUtility";
import { IconLibrary } from "../../Util/Enumerations";
import { ExtraFieldDialogController } from "./Dialogs/ExtraFieldDialogController";
import { ExtraFieldDTO } from "../Models/ExtraFieldDTO";
import { Constants } from "../../Util/Constants";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Filters

    private hasEditPermission: boolean;

    private isForAccountDim: boolean;
    private fieldTypes: any[];

    //@ngInject
    constructor(
        private $uibModal,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "common.extrafields.extrafields", progressHandlerFactory, messagingHandlerFactory);

        this.isForAccountDim = (soeConfig.entity == SoeEntityType.Account);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.loadFieldTypes())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start(this.getPermissions());
    }

    private getPermissions(): any[] {
        const features = [];

        features.push({ feature: soeConfig.feature, loadReadPermissions: true, loadModifyPermissions: true });

        switch (soeConfig.entity) {
            case SoeEntityType.InvoiceProduct:
                features.push({ feature: Feature.Billing_Product_Products_ExtraFields_Edit, loadReadPermissions: true, loadModifyPermissions: true });
                break;
            case SoeEntityType.Supplier:
                features.push({ feature: Feature.Common_ExtraFields_Supplier_Edit, loadReadPermissions: true, loadModifyPermissions: true });
                break;
            case SoeEntityType.Customer:
                features.push({ feature: Feature.Common_ExtraFields_Customer_Edit, loadReadPermissions: true, loadModifyPermissions: true });
                break;
            case SoeEntityType.Employee:
                features.push({ feature: Feature.Common_ExtraFields_Employee_Edit, loadReadPermissions: true, loadModifyPermissions: true });
                break;
            case SoeEntityType.Account:
                features.push({ feature: Feature.Common_ExtraFields_Account_Edit, loadReadPermissions: true, loadModifyPermissions: true });
                break;
        }

        return features;
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[soeConfig.feature].readPermission;
        this.modifyPermission = response[soeConfig.feature].modifyPermission;

        switch (soeConfig.entity) {
            case SoeEntityType.InvoiceProduct:
                this.hasEditPermission = response[Feature.Billing_Product_Products_ExtraFields_Edit].modifyPermission;
                break;
            case SoeEntityType.Supplier:
                this.hasEditPermission = response[Feature.Common_ExtraFields_Supplier_Edit].modifyPermission;
                break;
            case SoeEntityType.Customer:
                this.hasEditPermission = response[Feature.Common_ExtraFields_Customer_Edit].modifyPermission;
                break;
            case SoeEntityType.Employee:
                this.hasEditPermission = response[Feature.Common_ExtraFields_Employee_Edit].modifyPermission;
                break;
            case SoeEntityType.Account:
                this.hasEditPermission = response[Feature.Common_ExtraFields_Account_Edit].modifyPermission;
                break;
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
        if (this.toolbar && this.hasEditPermission) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.extrafields.createextrafield", "common.extrafields.createextrafield", IconLibrary.FontAwesome, "fa-plus", () => {
                this.edit(null);
            })));
        }
    }

    private loadFieldTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ExtraFieldTypes, false, false).then((x) => {
            this.fieldTypes = [];
            _.forEach(_.filter(x, (y) => y.id < 7), (row) => {
                this.fieldTypes.push({ id: row.id, value: row.name });
            });
        });
    }

    private setupGrid() {
        // Columns
        const keys: string[] = [
            "common.appellation",
            "common.extrafields.fieldtype",
            "common.accountdim",
            "core.edit",
            "core.delete"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("text", terms["common.appellation"], null, true);
            this.gridAg.addColumnText("typeName", terms["common.extrafields.fieldtype"], null, true);
            if (this.isForAccountDim) {
                this.gridAg.addColumnText("accountDimName", terms["common.accountdim"], null, true);
            }
            if (this.hasEditPermission) {
                this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), null, () => this.hasEditPermission);
                this.gridAg.addColumnDelete(terms["core.delete"], this.delete.bind(this), false, (row) => row && !row.hasRecords);
            }

            this.gridAg.finalizeInitGrid("common.extrafields.extrafields", true)
        });
    }

    private loadGridData(useCache = false, hideProgress = false) {
        if (hideProgress) {
            this.load(useCache);
        } else {
            this.progress.startLoadingProgress([() => this.load(useCache)]);
        }
    }

    private load(useCache = false): ng.IPromise<any> {
        return this.coreService.getExtraFieldsGrid(soeConfig.entity, true, null, null, useCache).then(x => {
            _.forEach(x, (y) => {
                const type = _.find(this.fieldTypes, (t) => t.id === y.type);
                if (type)
                    y.typeName = type.value;
            });
            return x;
        }).then(data => {
            this.setData(data);
        });
    }

    public edit(row) {
        if (row) {
            this.coreService.getExtraField(row.extraFieldId).then(x => {
                this.showEditDialog(x);
            });
        }
        else {
            this.showEditDialog(undefined);
        }
    }

    private showEditDialog(field) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/ExtraFields/Dialogs/Views/ExtraFieldDialog.html"),
            controller: ExtraFieldDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                entity: () => { return soeConfig.entity },
                extraField: () => { return field }
            }
        }

        this.$uibModal.open(options).result.then((extraField: any) => {
            if (extraField) {
                this.progress.startSaveProgress((completion) => {
                    this.coreService.saveExtraField(extraField).then((result) => {
                        if (result.success) {
                            completion.completed(Constants.EVENT_EDIT_SAVED, extraField);
                        } else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, this.guid)
                    .then(data => {
                        this.loadGridData(false);
                    }, error => {

                    });
            }
        });
    }

    private delete(row) {
        this.progress.startDeleteProgress((completion) => {
            this.coreService.deleteExtraField(row.extraFieldId).then((result) => {
                if (result.success) {
                    completion.completed(row, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.loadGridData(false, true);
        })
    }
}