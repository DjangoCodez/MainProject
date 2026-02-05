import { DateRangeSelectionDTO, IdSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { SoeCategoryType, TermGroup } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { SelectionCollection } from "../../../SelectionCollection";

export class TaxReductionBalanceListReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: TaxReductionBalanceListReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/TaxReductionBalanceListReport/TaxReductionBalanceListReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "taxReductionBalanceListReport";

   
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    
    private projectReportTitle = "";

    private selectableDateRegard: ISmallGenericType[];
    private selectableSortOrder: ISmallGenericType[];
    private selectedDateRange: DateRangeSelectionDTO;
    private taxDeductionTypes: ISmallGenericType[];
    private applicationStatusTypes: ISmallGenericType[];

    private actorNrFrom: ITextSelectionDTO;
    private actorNrTo: ITextSelectionDTO;

    private dateRegard: IdSelectionDTO;
    private sortOrder: IdSelectionDTO;
    private taxDeductionType: IdSelectionDTO;
    private applicationStatusType: IdSelectionDTO;
    //Terms
    private terms: { [index: string]: string };

    //@ngInject
    constructor(private $scope: ng.IScope, private translationService: ITranslationService, private coreService: ICoreService) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;
            
            this.taxDeductionType = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_TAX_DEDUCTION_TYPES);
            this.actorNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM);
            this.actorNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO);
            this.applicationStatusType = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_APPLICATION_STATUS_TYPES);

            this.selectedDateRange = this.userSelection.getDateRangeSelection();

            this.dateRegard = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD);
            this.sortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);

        });
        
        const keys = [
            "billing.project.central.projectreports",
            "common.report.daterangeselection",
            "common.report.seperatereport",
            "common.report.accountselection",
            "common.report.distributionreport",
            "common.report.standardselection",
            "common.all"
        ]

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.projectReportTitle = terms["billing.project.central.projectreports"];
        });
    }

    public $onInit() {
        this.selectedDateRange = new DateRangeSelectionDTO("daterange", null, null);
        this.getSortOrder();
        this.getTaxDeductionTypes();
        this.getApplicationStatusTypes();
    }


    private getTaxDeductionTypes() {
        this.taxDeductionTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.HouseHoldTaxDeductionType, false, false, false).then(data => {
            this.taxDeductionTypes = data.map(item => item.id === 0 ? { ...item, name: this.terms["common.all"] } : item);
            this.taxDeductionTypes.sort((a, b) => a.name.localeCompare(b.name));
            this.taxDeductionType = new IdSelectionDTO(this.taxDeductionTypes[0].id);
        })
    }

    private getApplicationStatusTypes() {
        this.applicationStatusTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.SoeHouseholdClassificationGroup, false, false, false).then(data => {
            this.applicationStatusTypes = data;
            this.applicationStatusType = new IdSelectionDTO(this.applicationStatusTypes[0].id);
        })
    }

    private getSortOrder() {
        this.selectableSortOrder = [];        
        return this.coreService.getTermGroupContent(TermGroup.TaxDeductionBalanceListReportSortOrder, false, false, false).then(data => {
            this.selectableSortOrder = data;           
            this.sortOrder = new IdSelectionDTO(this.selectableSortOrder[0].id);
        });
    }

    public onCustomerGroupChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CUSTOMER_CATEGORY, selection);
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {        
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public onDateRegardChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_REGARD, selection);
    }

    public onSortOrderChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER, selection);
    }

    public onActorNumberFromChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, selection);
    }

    public onActorNumberToChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, selection);
    }

    public onTaxDeductionTypeChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TAX_DEDUCTION_TYPES, selection);
    }

    public onApplicationStatusTypeChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_APPLICATION_STATUS_TYPES, selection);
    }

    
}

