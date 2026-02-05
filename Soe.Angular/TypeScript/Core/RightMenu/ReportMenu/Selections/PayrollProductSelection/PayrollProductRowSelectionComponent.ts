import { IReportDataService } from "../../ReportDataService";
import { IPayrollProductGridDTO, ISelectablePayrollTypeDTO } from "../../../../../Scripts/TypeLite.Net4";
import { PayrollProductRowSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { ITranslationService } from "../../../../Services/TranslationService";

interface ISelectablePayrollTypeViewModel {
    name: string,
    id: number,
    parentId: number
}

interface ISelectablePayrollProductViewModel {
    id: number;
    label: string;
}

interface ISelectablePayrollTypeCollection {
    id: number;
    selected?: ISelectablePayrollTypeViewModel;
    available: ISelectablePayrollTypeViewModel[];
}

type payrollProductPredicate = (p: IPayrollProductGridDTO) => boolean;

export class PayrollProductRowSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PayrollProductRowSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/PayrollProductSelection/PayrollProductRowSelectionView.html",
            bindings: {
                onSelected: "&",
                onDeleted: "&",
                canDelete: "<",
                labelKey: "@",
                hidelabel: "<",
                hideProducts: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "payrollProductRowSelection";
    private static deselecter: ISelectablePayrollTypeViewModel = { id: -1, parentId: -1, name: "" };
    private static rootLevelParentId: number = 0;
    private static emptySelectablePayrollTypesLevel: number = -1;

    //bindings properties
    private onSelected: (_: { selection: PayrollProductRowSelectionDTO }) => void = angular.noop;
    private onDeleted: () => void = angular.noop;
    private userSelectionInput: PayrollProductRowSelectionDTO;

    private allSysPayrollTypes: _.Dictionary<ISelectablePayrollTypeDTO[]>;
    private allPayrollProducts: IPayrollProductGridDTO[] = [];
    private selectablePayrollTypes: ISelectablePayrollTypeCollection[] = [];
    private currentlySelectablePayrollProducts: ISelectablePayrollProductViewModel[] = [];
    private selectedPayrollProducts: ISelectablePayrollProductViewModel[] = [];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private reportDataService: IReportDataService,
        private translationService: ITranslationService) {

        this.reportDataService.getPayrollProducts().then(payrollProducts => {
            this.allPayrollProducts = payrollProducts;
        });

        this.reportDataService.getPayrollTypes().then(payrollTypes => {
            this.allSysPayrollTypes = _.groupBy(payrollTypes, v => v.parentSysTermId);

            this.buildFirstLevelSelectablePayrollTypes();
            if (this.userSelectionInput)
                this.setSavedUserSelection();
        });
    }

    public $onInit() {
        PayrollProductRowSelection.deselecter.name = this.translationService.translateInstant("common.all");
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        if (this.userSelectionInput.sysPayrollTypeLevel1)
            this.selectPayrollTypeAtLevel(0, this.userSelectionInput.sysPayrollTypeLevel1);
        if (this.userSelectionInput.sysPayrollTypeLevel2)
            this.selectPayrollTypeAtLevel(1, this.userSelectionInput.sysPayrollTypeLevel2);
        if (this.userSelectionInput.sysPayrollTypeLevel3)
            this.selectPayrollTypeAtLevel(2, this.userSelectionInput.sysPayrollTypeLevel3);
        if (this.userSelectionInput.sysPayrollTypeLevel4)
            this.selectPayrollTypeAtLevel(3, this.userSelectionInput.sysPayrollTypeLevel4);
        if (this.userSelectionInput.payrollProductIds.length > 0) {
            this.populateSelectablePayrollProducts();
            this.selectedPayrollProducts = _.filter(this.currentlySelectablePayrollProducts, p => _.includes(this.userSelectionInput.payrollProductIds, p.id));
            this.propagateSelection();
        }
    }

    private buildFirstLevelSelectablePayrollTypes() {
        this.buildSelectablePayrollTypesFromLevel(PayrollProductRowSelection.emptySelectablePayrollTypesLevel, PayrollProductRowSelection.rootLevelParentId);
    }

    private buildSelectablePayrollTypesFromLevel(levelIndex: number, parentId: number) {
        const nextLevelIndex = levelIndex + 1;
        this.selectablePayrollTypes.splice(nextLevelIndex);

        const payrollTypesForNextLevel = this.allSysPayrollTypes[parentId];
        if (!payrollTypesForNextLevel) {
            return;
        }

        const selectablePayrollTypesForLevel = payrollTypesForNextLevel.map(v => <ISelectablePayrollTypeViewModel>{ id: v.id, parentId: v.sysTermId, name: v.name });
        selectablePayrollTypesForLevel.unshift(PayrollProductRowSelection.deselecter);

        this.selectablePayrollTypes.push({
            id: nextLevelIndex,
            available: selectablePayrollTypesForLevel,
            selected: PayrollProductRowSelection.deselecter
        });
    }

    private selectPayrollTypeAtLevel(levelIndex: number, selectablePayrollTypeIndex: number) {
        const payrollTypesAtLevel = this.selectablePayrollTypes[levelIndex];

        if (!payrollTypesAtLevel) {
            console.warn("Selectable payroll types was not found at specified level.", levelIndex, selectablePayrollTypeIndex);
            return;
        }

        const selectedFilter = payrollTypesAtLevel.available.find(s => s.id === selectablePayrollTypeIndex);
        payrollTypesAtLevel.selected = selectedFilter;

        this.buildSelectablePayrollTypesFromLevel(levelIndex, selectedFilter.parentId);
        this.selectedPayrollProducts = [];
        this.propagateSelection();
    }

    private populateSelectablePayrollProducts() {
        this.currentlySelectablePayrollProducts = [];
        let predicates: payrollProductPredicate[] = [];

        this.applyForSelectedPayrollTypeLevel((prop, selected) => {
            predicates.push(p => {
                return p[prop] === selected.id;
            });
        });

        this.currentlySelectablePayrollProducts = this.allPayrollProducts
            .filter(p => predicates.every(predicate => predicate(p)))
            .map(p => <ISelectablePayrollProductViewModel>{ id: p.productId, label: "{0} - {1}".format(p.number, p.name) });
    }

    private propagateSelection() {
        const selection = new PayrollProductRowSelectionDTO('', 0, 0, 0, 0, []);

        this.applyForSelectedPayrollTypeLevel((prop, selected) => {
            selection[prop] = selected.id;
        });

        selection.payrollProductIds = this.selectedPayrollProducts.map(p => p.id);

        this.onSelected({ selection });
    }

    private removeSelection() {
        this.onDeleted();
    }

    private clearSelection() {
        this.buildFirstLevelSelectablePayrollTypes();
        this.selectedPayrollProducts = [];
        this.propagateSelection();
    }

    private applyForSelectedPayrollTypeLevel(callback: (typeLevelPropertyName: string, selectedPayrollType: ISelectablePayrollTypeViewModel) => void) {
        this.selectablePayrollTypes.forEach((f, i) => {
            if (f.selected && f.selected.id > 0) {
                const typeLevelProperty: string = "sysPayrollTypeLevel" + (i + 1);
                callback(typeLevelProperty, f.selected);
            }
        });
    }
}