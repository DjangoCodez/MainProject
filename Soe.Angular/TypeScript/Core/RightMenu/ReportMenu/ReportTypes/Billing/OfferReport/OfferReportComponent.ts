import { ICommonCustomerService } from "../../../../../../Common/Customer/CommonCustomerService";
import { IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { TermGroup } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { SelectionCollection } from "../../../SelectionCollection";

export class OfferReport {
    selectableSorting: ISmallGenericType[];
    selectableInventoryBalanceList: any[];
    selectableCodeList: any[];
    public static component(): ng.IComponentOptions {
        return {
            controller: OfferReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/OfferReport/OfferReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<",
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "offerReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private selectedSortItem: IdSelectionDTO;
    private actorNrFrom: SmallGenericType;
    private actorNrTo: SmallGenericType;
    private offerNumberFrom: TextSelectionDTO;
    private offerNumberTo: TextSelectionDTO;
    private customerDict: SmallGenericType[];

    //@ngInject
    constructor(private $scope: ng.IScope, private coreService: ICoreService, private commonCustomerService: ICommonCustomerService,) {

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
        this.getSortOrder();
        this.getCustomers();
    }

    private getCustomers() {
        this.customerDict = [];
        this.coreService.getCustomers(true, false, true).then((customers: ISmallGenericType[]) => {
            this.customerDict = customers;
        });
    }

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {
        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null) {
            this.actorNrFrom = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text);
            this.customerIDChangedFrom(this.actorNrFrom);
        }
        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null) {
            this.actorNrTo = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text);
            this.customerIDChangedTo(this.actorNrTo);
        }
        this.offerNumberFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
        this.offerNumberTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);
        this.selectedSortItem = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
    }

    private getSortOrder() {
        this.selectableSorting = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportBillingOfferSortOrder, false, true).then(data => {
            this.selectableSorting = data;
            this.selectedSortItem = new IdSelectionDTO(this.selectableSorting[0].id);
        });
    }

    public onSortOrderSelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER, selection);
    }

    public customerIDChangedFrom(selection: SmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, new TextSelectionDTO(selection.name.split(' ')[0]));

        if (!this.actorNrTo) {
            this.actorNrTo = selection;
            this.customerIDChangedTo(selection);
        }
    }

    public customerIDChangedTo(selection: SmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, new TextSelectionDTO(selection.name.split(' ')[0]));
    }

    public offerNumberChangedFrom(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, selection);

        if (!this.offerNumberTo || !this.offerNumberTo.text) {
            this.offerNumberTo = selection;
        }
    }

    public offerNumberChangedTo(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, selection);
    }

}