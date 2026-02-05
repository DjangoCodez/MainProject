import { GeneralReportSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { Feature, ReportUserSelectionType, SoeModule, TermGroup_ReportUserSelectionAccessType } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { ICoreService } from "../../../../Services/CoreService";
import { INotificationService } from "../../../../Services/NotificationService";
import { IReportService } from "../../../../Services/reportservice";
import { ITranslationService } from "../../../../Services/TranslationService";
import { IUrlHelperService } from "../../../../Services/UrlHelperService";
import { SelectionCollection } from "../../SelectionCollection";
import { SaveSelectionDialogController } from "./SaveSelectionDialogController";

export class UserSelections {
    public static component(): ng.IComponentOptions {
        return {
            controller: UserSelections,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Components/UserSelections/UserSelectionsView.html",
            bindings: {
                module: "<",
                selectionType: "=",
                selections: "=",
                selectedExportType: "=",
                reportId: "<",
                showIncludeColumnsSelection: "=",
                onSelected: "&"
            }
        };
    }

    public static componentKey = "userSelections";

    // Terms
    private terms: { [index: string]: string; };

    // Permissions
    private savePublicPermission: boolean = false;

    // Binding properties
    private module: SoeModule;
    private selectionType: ReportUserSelectionType;
    private selections: SelectionCollection;
    private selectedExportType: number;
    private reportId: number;
    private showIncludeColumnsSelection: boolean;
    private onSelected: (_: { reportUserSelection: ReportUserSelectionDTO }) => void = angular.noop;

    // Saved user columns selections
    private reportUserSelections: SmallGenericType[] = [];
    private selectedReportUserSelectionId: number;
    private selectedReportUserSelection: ReportUserSelectionDTO;

    private get selectionIsPrivate(): boolean {
        return this.selectedReportUserSelection && !!this.selectedReportUserSelection.userId;
    }

    private get canSaveSelection(): boolean {
        return this.savePublicPermission || this.selectionIsPrivate || !this.selectedReportUserSelectionId;
    }

    private get canCopySelection(): boolean {
        return this.selectedReportUserSelectionId > 0;
    }

    private get canDeleteSelection(): boolean {
        return this.selectedReportUserSelectionId > 0 && (this.savePublicPermission || this.selectionIsPrivate);
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModal: ng.ui.bootstrap.IModalService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private reportService: IReportService) {

        this.$scope.$watchGroup([() => this.reportId, () => this.selectionType], () => {
            this.loadReportUserSelections();
        });
    }

    // SETUP

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions()
        ]).then(() => {
            this.loadReportUserSelections();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.reportmenu.selection.new",
            "core.reportmenu.selection.save",
            "core.reportmenu.selection.save.error",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const features: number[] = [];

        if (this.module === SoeModule.Economy)
            features.push(Feature.Economy_Distribution_Reports_SavePublicSelections);
        else if (this.module === SoeModule.Billing)
            features.push(Feature.Billing_Distribution_Reports_SavePublicSelections);
        else if (this.module === SoeModule.Time)
            features.push(Feature.Time_Distribution_Reports_SavePublicSelections);

        return this.coreService.hasModifyPermissions(features).then((x) => {
            if (this.module === SoeModule.Economy)
                this.savePublicPermission = x[Feature.Economy_Distribution_Reports_SavePublicSelections];
            else if (this.module === SoeModule.Billing)
                this.savePublicPermission = x[Feature.Billing_Distribution_Reports_SavePublicSelections];
            else if (this.module === SoeModule.Time)
                this.savePublicPermission = x[Feature.Time_Distribution_Reports_SavePublicSelections];
        });
    }

    private loadReportUserSelections(): ng.IPromise<any> {
        return this.reportService.getReportUserSelections(this.reportId, this.selectionType).then(x => {
            this.reportUserSelections = x;
            this.reportUserSelections.splice(0, 0, new SmallGenericType(0, this.terms["core.reportmenu.selection.new"]));
            this.selectedReportUserSelectionId = 0;
            this.selectedReportUserSelection = null;
        });
    }

    private loadReportUserSelection(reportUserSelectionId: number): ng.IPromise<any> {
        return this.reportService.getReportUserSelection(reportUserSelectionId).then(x => {
            this.selectedReportUserSelection = x;
            this.selectedReportUserSelectionId = reportUserSelectionId;

            this.onSelected({ reportUserSelection: this.selectedReportUserSelection });
        });
    }

    private loadReportUserSelectionFromReportPrintout(reportPrintoutId: number): ng.IPromise<any> {
        return this.reportService.getReportSelectionFromReportPrintout(reportPrintoutId).then(x => {
            this.selectedReportUserSelection = x;
            this.selectedReportUserSelectionId = 0;
        });
    }

    // EVENTS

    private reportUserSelectionChanged() {
        this.$timeout(() => {
            this.loadReportUserSelection(this.selectedReportUserSelectionId);
        });
    }

    // ACTIONS

    private saveReportUserSelection(selection: ReportUserSelectionDTO) {
        if (!selection)
            this.selectedReportUserSelectionId = 0;

        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Core/RightMenu/ReportMenu/Components/UserSelections/SaveSelectionDialog.html"),
            controller: SaveSelectionDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                translationService: () => { return this.translationService },
                module: () => { return this.module },
                showIncludeColumnsSelection: () => { return this.showIncludeColumnsSelection },
                savePublicPermission: () => { return this.savePublicPermission },
                selection: () => { return selection },
            }
        });

        modal.result.then(result => {
            if (result.selection) {
                selection = result.selection;
                selection.reportId = this.reportId;
                selection.userId = (result.accessType === TermGroup_ReportUserSelectionAccessType.Private ? CoreUtility.userId : null);
                selection.type = this.selectionType;
                selection.selections = this.selections.materialize();

                let general: GeneralReportSelectionDTO = selection.getGeneralReportSelection();
                if (!general) {
                    general = new GeneralReportSelectionDTO(this.selectedExportType);
                    selection.selections.push(general);
                } else {
                    general.exportType = this.selectedExportType;
                }

                if (this.selectionType === ReportUserSelectionType.DataSelection) {
                    if (!result.includeColumnsSelection)
                        selection.selections = selection.selections.filter(s => s.key !== Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS);
                } else if (this.selectionType === ReportUserSelectionType.AnalysisColumnSelection || this.selectionType === ReportUserSelectionType.InsightsColumnSelection) {
                    selection.selections = selection.selections.filter(s => s.key === Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS);
                }

                this.reportService.saveReportUserSelection(selection).then(res => {
                    if (res.success) {
                        this.loadReportUserSelections().then(() => {
                            this.loadReportUserSelection(res.integerValue);
                        });
                    } else {
                        this.notificationService.showDialogEx(this.terms["core.reportmenu.selection.save.error"], res.errorMessage, SOEMessageBoxImage.Error);
                    }
                });
            }
        });
    }

    private deleteReportUserSelection() {
        if (!this.selectedReportUserSelection)
            return;

        const keys: string[] = [];
        if (this.selectionIsPrivate) {
            keys.push("core.warning");
            keys.push("core.deletewarning");
        } else {
            keys.push("core.reportmenu.selection.deletepublicwarning.title");
            keys.push("core.reportmenu.selection.deletepublicwarning.message");
        }

        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialogEx(this.selectionIsPrivate ? terms["core.warning"] : terms["core.reportmenu.selection.deletepublicwarning.title"], this.selectionIsPrivate ? terms["core.deletewarning"] : terms["core.reportmenu.selection.deletepublicwarning.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.reportService.deleteReportUserSelection(this.selectedReportUserSelectionId).then(result => {
                        if (result.success) {
                            this.loadReportUserSelections();
                        } else {
                            this.notificationService.showDialogEx(this.terms["core.reportmenu.selection.delete.error"], result.errorMessage, SOEMessageBoxImage.Error);
                        }
                    });
                }
            });
        });
    }
}