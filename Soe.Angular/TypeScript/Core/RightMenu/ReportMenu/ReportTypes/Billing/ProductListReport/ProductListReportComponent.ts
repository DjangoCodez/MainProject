
import { TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { ISmallGenericType, ITextSelectionDTO, IYearAndPeriodSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";

export class ProductListReport {
    selectableSorting: ISmallGenericType[];
    selectableInventoryBalanceList: any[];
    selectableCodeList: any[];
    public static component(): ng.IComponentOptions {
        return {
            controller: ProductListReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/ProductListReport/ProductListReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<",
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "productListReport";

    private onSelected: (_: { selection: IYearAndPeriodSelectionDTO }) => void = angular.noop;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    
    private articleNumberFrom: TextSelectionDTO;
    private articleNumberTo: TextSelectionDTO;


    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService, private coreService: ICoreService) {
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

    

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {
        this.articleNumberFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
        this.articleNumberTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);
    }

    public articleNumberChangedFrom(selection: ITextSelectionDTO) {
        var selectionFrom = new TextSelectionDTO(selection.text);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, selection);

        if (!this.articleNumberTo || !this.articleNumberTo.text) {
            this.articleNumberTo = selection;
        }
    }

    private articleNumberChangedTo(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, selection);
    }

}

