import { SelectionCollection } from "../../../Core/RightMenu/ReportMenu/SelectionCollection";
import { ICoreService } from "../../../Core/Services/CoreService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { TermGroup_ReportUserSelectionAccessType, UserSelectionType } from "../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { SmallGenericType } from "../../Models/SmallGenericType";
import { UserSelectionDTO } from "../../Models/UserSelectionDTOs";
import { SaveUserSelectionDialogController } from "./SaveUserSelectionDialogController";

export class UserSelection {
    public static component(): ng.IComponentOptions {
        return {
            controller: UserSelection,
            templateUrl: soeConfig.baseUrl + "Common/Components/UserSelection/UserSelectionView.html",
            bindings: {
                selectionType: "=",
                selections: "=",
                selectedUserSelectionId: "=",
                savePublicPermission: "=",
                onSelectionsLoaded: "&",
                onSelected: "&",
                onInitSave: "&"
            }
        };
    }

    public static componentKey = "userSelection";

    // Terms
    private terms: { [index: string]: string; };

    // Permissions

    // Binding properties
    private selectionType: UserSelectionType;
    private selections: SelectionCollection;
    private selectedUserSelectionId: number;
    private savePublicPermission: boolean;
    private onSelectionsLoaded: (_: { userSelections: SmallGenericType[] }) => void = angular.noop;
    private onSelected: (_: { userSelection: UserSelectionDTO }) => void = angular.noop;
    private onInitSave: () => void = angular.noop;

    // Saved user columns selections
    private userSelections: SmallGenericType[] = [];
    private selectedUserSelection: UserSelectionDTO;

    private get selectionIsPrivate(): boolean {
        return this.selectedUserSelection && !!this.selectedUserSelection.userId;
    }

    private get canSaveSelection(): boolean {
        return this.savePublicPermission || this.selectionIsPrivate || !this.selectedUserSelectionId;
    }

    private get canCopySelection(): boolean {
        return this.selectedUserSelectionId > 0;
    }

    private get canDeleteSelection(): boolean {
        return this.selectedUserSelectionId > 0 && (this.savePublicPermission || this.selectionIsPrivate);
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
        private coreService: ICoreService) {

        this.$scope.$watch(() => this.selectionType, (newVal, oldVal) => {
            this.loadUserSelections();
        });

        this.$scope.$watch(() => this.selectedUserSelectionId, (newVal, oldVal) => {
            if (this.selectedUserSelectionId)
                this.loadUserSelection(this.selectedUserSelectionId);
            else
                this.selectedUserSelection = null;
        });
    }

    // SETUP

    public $onInit() {
        this.$q.all([
            this.loadTerms()
        ]).then(() => {
            this.loadUserSelections();
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

    private loadUserSelections(): ng.IPromise<any> {
        return this.coreService.getUserSelections(this.selectionType).then(x => {
            this.userSelections = x;
            this.userSelections.splice(0, 0, new SmallGenericType(0, this.terms["core.reportmenu.selection.new"]));
            this.selectedUserSelectionId = 0;
            this.selectedUserSelection = null;

            this.onSelectionsLoaded({ userSelections: this.userSelections });
        });
    }

    private loadUserSelection(userSelectionId: number): ng.IPromise<any> {
        return this.coreService.getUserSelection(userSelectionId).then(x => {
            if (x?.selections) {
                this.selectedUserSelection = x;
                this.selectedUserSelectionId = userSelectionId;

                this.onSelected({ userSelection: this.selectedUserSelection });
            }
        });
    }

    // ACTIONS

    private saveUserSelection(selection: UserSelectionDTO) {
        this.onInitSave();

        if (!selection)
            this.selectedUserSelectionId = 0;

        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Components/UserSelection/SaveUserSelectionDialog.html"),
            controller: SaveUserSelectionDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                translationService: () => { return this.translationService },
                savePublicPermission: () => { return this.savePublicPermission },
                selection: () => { return selection },
            }
        });

        modal.result.then(result => {
            if (result.selection) {
                selection = result.selection;
                selection.userId = (result.accessType === TermGroup_ReportUserSelectionAccessType.Private ? CoreUtility.userId : null);
                selection.type = this.selectionType;
                selection.selections = this.selections.materialize();

                this.coreService.saveUserSelection(selection).then(res => {
                    if (res.success) {
                        this.loadUserSelections().then(() => {
                            this.loadUserSelection(res.integerValue);
                        });
                    } else {
                        this.notificationService.showDialogEx(this.terms["core.reportmenu.selection.save.error"], res.errorMessage, SOEMessageBoxImage.Error);
                    }
                });
            }
        });
    }

    private deleteUserSelection() {
        if (!this.selectedUserSelection)
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
                    this.coreService.deleteUserSelection(this.selectedUserSelectionId).then(result => {
                        if (result.success) {
                            this.loadUserSelections();
                        } else {
                            this.notificationService.showDialogEx(this.terms["core.reportmenu.selection.delete.error"], result.errorMessage, SOEMessageBoxImage.Error);
                        }
                    });
                }
            });
        });
    }
}