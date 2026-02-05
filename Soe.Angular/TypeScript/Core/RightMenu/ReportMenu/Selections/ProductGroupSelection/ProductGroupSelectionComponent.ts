import { IReportDataService } from "../../ReportDataService";
import { ISelectableTimePeriodDTO, } from "../../../../../Scripts/TypeLite.Net4";
import { IdListSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { TimePeriodHeadDTO } from "../../../../../Common/Models/TimePeriodHeadDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { ProjectDTO } from "../../../../../Common/Models/ProjectDTO";
import { SelectionCollection } from "../../SelectionCollection";
import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { Constants } from "../../../../../Util/Constants";

export class ProductGroupSelection {

    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: ProductGroupSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/ProductGroupSelection/ProductGroupSelectionView.html",
            bindings: {
                userSelection: "=",
                onProductGroupSelectedFrom: "&",
                onProductGroupSelectedTo: "&",
            }
        };

        return options;
    }

    public static componentKey = "productGroupSelection";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private selectableProductGroups: any[];
    //private rangeProductGroupFilters: NamedFilterRange[];

    private productGroupFrom: SmallGenericType;
    private productGroupTo: SmallGenericType;

    private onProductGroupSelectedFrom: (_: { selection: any }) => void = angular.noop;
    private onProductGroupSelectedTo: (_: { selection: any }) => void = angular.noop;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private reportDataService: IReportDataService,) {

        this.$scope.$watch(() => this.selections, (newVal, oldVal) => {
            if (!newVal)
                return;
        });

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.setSavedUserFilters(newVal);
        });
    }

    public $onInit() {
        this.getProductGroupsList();
    }

    private getProductGroupsList() {
        this.selectableProductGroups = [];
        return this.reportDataService.getProductGroups(true).then(data => {
            _.orderBy(data, "code").forEach(pg => {
                this.selectableProductGroups.push({ id: pg.productGroupId, name: pg.code + " " + pg.name, value: pg.code });
            });
        });
    }

    private productGroupsChangedFrom(selection: any) {
        if (!selection || !this.selectableProductGroups)
            return;

        if (!this.productGroupTo || !this.productGroupTo.id || this.productGroupTo.id === 0) {
            this.productGroupTo = selection;
            this.productGroupsChangedTo(selection);
        }

        this.onProductGroupSelectedFrom({ selection: selection });
    }

    private productGroupsChangedTo(selection: any) {
        if (!selection)
            return;
        
        this.onProductGroupSelectedTo({ selection: selection });
    }

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {
        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_PRODUCT_GROUPS_FROM) != null) {
            this.productGroupFrom = _.find(this.selectableProductGroups, d => d.value === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_PRODUCT_GROUPS_FROM).text);
            this.onProductGroupSelectedFrom({ selection: this.productGroupFrom });
        }

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_PRODUCT_GROUPS_TO) != null) {
            this.productGroupTo = _.find(this.selectableProductGroups, d => d.value === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_PRODUCT_GROUPS_TO).text);
            this.onProductGroupSelectedTo({ selection: this.productGroupTo });
        }
    }
}