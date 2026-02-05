import { AccountFilterSelectionDTO, AccountIntervalSelectionDTO, BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";
import { NamedFilterRange } from "../../../Selections/AccountRangeSelection/AccountRangeSelectionComponent";

export class FixedAssetReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: FixedAssetReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/FixedAssetReport/FixedAssetReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "fixedAssetReport";


    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private projectReportTitle = "";

    private showRange: boolean;

    private selectedDateRange: DateRangeSelectionDTO;
    private dateIsSelectedDTO: BoolSelectionDTO;
    private accountPeriodFrom: AccountIntervalSelectionDTO;
    private accountPeriodTo: AccountIntervalSelectionDTO;

    private selectedFromCategory: IdSelectionDTO;
    private selectedFromInventory: IdSelectionDTO;
    private selectedFromInventoryAccount: IdSelectionDTO;
    private selectedToCategory: IdSelectionDTO;
    private selectedToInventory: IdSelectionDTO;
    private selectedToInventoryAccount: IdSelectionDTO;
    private selectedPrognoseType: IdSelectionDTO;




    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;
           
            this.setIntervals(newVal);

            this.selectedFromCategory = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CATEGORY_FROM);
            this.selectedToCategory = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CATEGORY_TO);
            this.selectedFromInventory = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_FROM);
            this.selectedToInventory = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_TO);
            this.selectedFromInventoryAccount = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_ACCOUNT_FROM);
            this.selectedToInventoryAccount = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_ACCOUNT_TO);
            this.selectedPrognoseType = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_PROGNOSE_TYPE);
        });

        const keys = [
            "billing.project.central.projectreports",
            "common.report.daterangeselection",
            "common.report.seperatereport",
            "common.report.accountselection",
            "common.report.distributionreport",
            "common.report.standardselection"
        ]

        this.translationService.translateMany(keys).then(terms => {
            this.projectReportTitle = terms["billing.project.central.projectreports"];
        });
    }

    private setIntervals(savedValues: ReportUserSelectionDTO) {
        
        const accountPeriodFrom = savedValues.getIntervalFromSelection();
        if (accountPeriodFrom) {
            this.accountPeriodFrom = accountPeriodFrom;
        }
        const accountPeriodTo = savedValues.getIntervalToSelection();
        if (accountPeriodTo) {
            this.accountPeriodTo = accountPeriodTo;
        }

        const dateSelectionChoosen = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_SELECTION_CHOOSEN);
        this.dateIsSelectedDTO = dateSelectionChoosen;
        this.showRange = !dateSelectionChoosen.value;
        if (!this.showRange) {
            this.selectedDateRange = savedValues.getDateRangeSelection();
        }
    }

    public onDateSelectionSelected(selection: IBoolSelectionDTO) {
        this.showRange = !selection.value;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_SELECTION_CHOOSEN, selection);
    }

    public intervalFromChanged(selection: AccountIntervalSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_PERIOD_FROM, selection);
    }

    public intervalToChanged(selection: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_PERIOD_TO, selection)
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public onInventoryFromSelected(selection: IdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_FROM, selection);
    }

    public onInventoryToSelected(selection: IdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_TO, selection)
    }

    public onCategoryFromSelected(selection: IdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CATEGORY_FROM, selection);
    }

    public onInventoryAccountFromSelected(selection: IdSelectionDTO) {        
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_ACCOUNT_FROM, selection);
    }

    public onInventoryAccountToSelected(selection: IdSelectionDTO) {        
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_ACCOUNT_TO, selection);
    }

    public onCategoryToSelected(selection: IdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CATEGORY_TO, selection)
    }

    public onPrognoseTypeChanged(selection: IdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROGNOSE_TYPE, selection)
    }

    
}
