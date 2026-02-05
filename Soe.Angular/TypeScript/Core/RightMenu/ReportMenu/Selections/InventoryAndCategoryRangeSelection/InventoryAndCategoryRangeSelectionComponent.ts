import { AccountDTO } from "../../../../../Common/Models/AccountDTO";
import { IdSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { SoeCategoryType, TermGroup } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../Services/CoreService";
import { IReportDataService } from "../../ReportDataService";

interface MultiSelectViewModel {
    id: string;
    label: string;
}

export class InventoryAndCategoryRangeSelection {
    public static component(): ng.IComponentOptions {
        return {
            controller: InventoryAndCategoryRangeSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/InventoryAndCategoryRangeSelection/InventoryAndCategoryRangeSelectionView.html",
            bindings: {
                onInventoryFromSelected: "&",
                onInventoryToSelected: "&",
                onInventoryAccountFromSelected : "&",
                onInventoryAccountToSelected : "&",
                onCategoryFromSelected: "&",
                onCategoryToSelected: "&",
                onPrognoseTypeChanged: "&",
                selectedFromInventory: "=",
                selectedToInventory: "=",
                selectedFromCategory: "=",
                selectedToCategory: "=",
                selectedFromInventoryAccount: "=",
                selectedToInventoryAccount: "=",
                selectedPrognoseType: "=",
                canVisibleInventoryAccount:"="
            }
        };
    }

    public static componentKey = "inventoryAndCategoryRangeSelection";

    private selectedFromCategory: IdSelectionDTO;    
    private selectedFromInventory: IdSelectionDTO;
    private selectedFromInventoryAccount: IdSelectionDTO;
    private selectedToCategory: IdSelectionDTO;    
    private selectedToInventory: IdSelectionDTO;
    private selectedToInventoryAccount: IdSelectionDTO;
    private selectedPrognoseType: IdSelectionDTO;

    private selectableCategories: SmallGenericType[];
    private selectableInventories: SmallGenericType[];
    private selectableInventoryAccounts: SmallGenericType[];
    private selectablePrognoseTypes: SmallGenericType[];

    private canVisibleInventoryAccount: boolean;

    private onInventoryFromSelected: (_: { selection: IdSelectionDTO }) => void = angular.noop;
    private onInventoryToSelected: (_: { selection: IdSelectionDTO }) => void = angular.noop;
    private onCategoryFromSelected: (_: { selection: IdSelectionDTO }) => void = angular.noop;
    private onCategoryToSelected: (_: { selection: IdSelectionDTO }) => void = angular.noop;
    private onInventoryAccountFromSelected: (_: { selection: IdSelectionDTO }) => void = angular.noop;
    private onInventoryAccountToSelected: (_: { selection: IdSelectionDTO }) => void = angular.noop;
    private onPrognoseTypeChanged: (_: { selection: IdSelectionDTO }) => void = angular.noop;

    //@ngInject
    constructor(private $scope: ng.IScope, private $timeout: ng.ITimeoutService, private reportDataService: IReportDataService, private coreService: ICoreService) {
        
    }

    public $onInit() {
        if (!this.canVisibleInventoryAccount) {
            this.canVisibleInventoryAccount = false;
        }
        this.getCategories();
        this.getInventory();
        this.getPrognoseType();
        this.getInventoryAccounts();
    }

    
    public inventoryFromChanged(selection: IdSelectionDTO) {
        if (selection) {
            if (!this.selectedToInventory || !this.selectedToInventory.id) {
                this.selectedToInventory = new IdSelectionDTO(selection.id);
                this.onInventoryToSelected({ selection: new IdSelectionDTO(selection.id) });
            }
            this.onInventoryFromSelected({ selection: new IdSelectionDTO(selection.id) });
        }
    }

    public inventoryToChanged(selection: IdSelectionDTO) {        
        if (selection) {
            this.selectedToInventory.id = selection.id;
            this.onInventoryToSelected({ selection: new IdSelectionDTO(selection.id) });
        }
    }

    public categoryFromChanged(selection: IdSelectionDTO) {
        if (selection) {
            if (!this.selectedToCategory || !this.selectedToCategory.id) {
                this.selectedToCategory = new IdSelectionDTO(selection.id);
                this.onCategoryToSelected({ selection: new IdSelectionDTO(selection.id) });
            }
            this.onCategoryFromSelected({ selection: new IdSelectionDTO(selection.id) });
        }
    }

    public categoryToChanged(selection: IdSelectionDTO) {        
        if (selection) {
            this.selectedToCategory.id = selection.id;
            this.onCategoryToSelected({ selection: new IdSelectionDTO(selection.id) });
        }
    }
    public inventoryAccountFromChanged(selection: IdSelectionDTO) {
        if (selection) {
            if (!this.selectedToInventoryAccount || !this.selectedToInventoryAccount.id) {
                this.selectedToInventoryAccount = new IdSelectionDTO(selection.id);
                this.onInventoryAccountToSelected({ selection: new IdSelectionDTO(selection.id) });
            }
            this.onInventoryAccountFromSelected({ selection: new IdSelectionDTO(selection.id) });
        }
    }

    public inventoryAccountToChanged(selection: IdSelectionDTO) {
        if (selection) {
            this.selectedToInventoryAccount.id = selection.id;
            this.onInventoryAccountToSelected({ selection: new IdSelectionDTO(selection.id) });
        }
    }

    public prognoseTypeChanged(selection: IdSelectionDTO) {       
        if (selection) {
            this.onPrognoseTypeChanged({ selection: new IdSelectionDTO(selection.id) });
        }
    }

    private getCategories() {
        this.selectableCategories = [];
        return this.coreService.getCategories(SoeCategoryType.Inventory, false, false, false, false).then(categories => {
            this.selectableCategories.push(new SmallGenericType(0, ' '));
            categories.forEach(category => {
                this.selectableCategories.push(new SmallGenericType(category.categoryId, category.code + " - " + category.name));
            });
        });
    }

    private getInventory() {
        this.reportDataService.getInventories().then(data => {
            this.selectableInventories = _.sortBy(data, 'inventoryNr');
        });
    }
    private getInventoryAccounts() {       
        this.selectableInventoryAccounts = [];
        this.reportDataService.getInventoryAccounts().then(data => {
            this.reportDataService.getInventorySettingAccounts().then((settingAccounts: AccountDTO[]) => {
                data = _.sortBy(data, 'inventoryAccountNr');
                data.forEach(inventoryAccount => {
                    this.selectableInventoryAccounts.push(new SmallGenericType(inventoryAccount.inventoryAccountId, inventoryAccount.inventoryAccountNr + " " + inventoryAccount.inventoryAccountName));
                });
                settingAccounts.forEach(settingAccount => {
                    if (!this.selectableInventoryAccounts.some(x => x.id == settingAccount.accountId)) {
                        this.selectableInventoryAccounts.push(new SmallGenericType(settingAccount.accountId, settingAccount.numberName));
                    }
                })
                

            });
        });
    }
    private getPrognoseType() {
        return this.coreService.getTermGroupContent(TermGroup.PrognosTypes, false, false, false).then(data => {
            this.selectablePrognoseTypes = _.sortBy(data, 'id');
            if (this.selectablePrognoseTypes.length > 0) {
                this.selectedPrognoseType = new IdSelectionDTO(this.selectablePrognoseTypes[0].id);
            }
        });
    }
    
}
