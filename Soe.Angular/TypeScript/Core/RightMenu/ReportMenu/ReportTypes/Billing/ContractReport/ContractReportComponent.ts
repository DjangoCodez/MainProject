import { ICommonCustomerService } from "../../../../../../Common/Customer/CommonCustomerService";
import { IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { TermGroup } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { SelectionCollection } from "../../../SelectionCollection";

export class ContractReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: ContractReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/ContractReport/ContractReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "contractReport";
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private selectableSortOrder: ISmallGenericType[];
    private customerNrFrom: SmallGenericType;
    private customerNrTo: SmallGenericType;
    private invoiceSeqNrFrom: ITextSelectionDTO;
    private invoiceSeqNrTo: ITextSelectionDTO;
    private invoiceSeqNrHandler: boolean = true;
    private userSelectionInputSortOrder: IdSelectionDTO;
    private customerDict: SmallGenericType[];

    //@ngInject
    constructor(private $scope: ng.IScope, private coreService: ICoreService, private commonCustomerService: ICommonCustomerService,) {
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.invoiceSeqNrFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
            this.invoiceSeqNrHandler = true;
            this.invoiceSeqNrTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);

            if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM) != null) {
                this.customerNrFrom = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM).text);
                this.onActorNumberFromChanged(this.customerNrFrom);
            }

            if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO) != null) {
                this.customerNrTo = _.find(this.customerDict, d => d.name.split(' ')[0] === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO).text);
                this.onActorNumberToChanged(this.customerNrTo);
            }

            this.userSelectionInputSortOrder = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
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

    private getSortOrder() {
        this.selectableSortOrder = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportBillingContractSortOrder, false, false, false).then(data => {
            this.selectableSortOrder = data;
            this.userSelectionInputSortOrder = new IdSelectionDTO(this.selectableSortOrder[0].id);
        });
    }

    public onSortOrderChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER, selection);
    }

    public onInvoiceSerialNumberFromChanged(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, selection);
        if (!this.invoiceSeqNrTo || !this.invoiceSeqNrTo.text) {
            this.invoiceSeqNrHandler = true;
            this.invoiceSeqNrTo = new TextSelectionDTO(selection.text);
        }
    }

    public onInvoiceSerialNumberToChanged(selection: ITextSelectionDTO) {
        if (this.invoiceSeqNrHandler) {
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, selection);
            this.invoiceSeqNrHandler = false;
            this.invoiceSeqNrTo = new TextSelectionDTO(selection.text);
        } else {
            this.invoiceSeqNrHandler = true;
        }
    }

    public onActorNumberFromChanged(selection: SmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_FROM, new TextSelectionDTO(selection.name.split(' ')[0]));

        if (!this.customerNrTo) {
            this.customerNrTo = selection;
            this.onActorNumberToChanged(selection);
        }
    }

    public onActorNumberToChanged(selection: SmallGenericType) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACTOR_NUMBER_TO, new TextSelectionDTO(selection.name.split(' ')[0]));
    }

}


