import { PayrollPriceTypeSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { TermGroup } from "../../../../../Util/CommonEnumerations";
import { CoreService } from "../../../../Services/CoreService";
import { PayrollPriceTypeDTO } from "../../../../../Common/Models/PayrollPriceTypeDTOs";
import { IPayrollPriceTypeSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IReportDataService } from "../../ReportDataService";

export class PayrollPriceTypeSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PayrollPriceTypeSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/PayrollPriceTypeSelection/PayrollPriceTypeSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                labelKey2: "@",
                hideLabel: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "payrollPriceTypeSelection";

    //binding properties
    private labelKey: string;
    private labelKey2: string;
    private onSelected: (_: { selection: IPayrollPriceTypeSelectionDTO }) => void = angular.noop;
    private userSelectionInput: PayrollPriceTypeSelectionDTO;
    private selectedPayrollPriceTypes: SmallGenericType[] = [];
    private payrollPriceTypeList: SmallGenericType[] = [];
    private payrollPriceTypeCodes: SmallGenericType[] = [];
    private selectedPayrollPriceTypeCodes: SmallGenericType[] = [];
    private payrollPriceTypes: PayrollPriceTypeDTO[] = [];
    private delaySetSavedUserSelection: boolean = false;
    private isLoaded: boolean = false;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private coreService: CoreService,
        private reportDataService: IReportDataService,
        private $timeout: ng.ITimeoutService) {
        this.reportDataService.getPayrollPriceTypes().then((x) => {
            _.forEach(x, (row) => {
                this.payrollPriceTypes = x;
                this.payrollPriceTypeCodes.push(new SmallGenericType(row.payrollPriceTypeId, row.code));
            });
            if (this.delaySetSavedUserSelection)
                this.setSavedUserSelection();

        });

        this.coreService.getTermGroupContent(TermGroup.PayrollPriceTypes, false, false).then((x) => {
            _.forEach(x, (row) => {
                this.payrollPriceTypeList.push(new SmallGenericType(row.id, row.name));
            });
         });
        

        this.$scope.$watch(() => this.userSelectionInput, () => {
                this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.propagateSelection();
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        if (this.payrollPriceTypeCodes.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        this.selectedPayrollPriceTypeCodes = [];
        this.selectedPayrollPriceTypes = [];

        if (this.userSelectionInput.typeIds && this.userSelectionInput.typeIds.length > 0)
            this.selectedPayrollPriceTypes = _.filter(this.payrollPriceTypeList, c => _.includes(this.userSelectionInput.typeIds, c.id));

        if (this.userSelectionInput.ids && this.userSelectionInput.ids.length > 0)
            this.selectedPayrollPriceTypeCodes = _.filter(this.payrollPriceTypeCodes, c => _.includes(this.userSelectionInput.ids, c.id));
        
        this.propagateSelection();
    }

    private propagateSelection() {
        this.$timeout(() => {
            if (this.userSelectionInput && this.userSelectionInput.ids && this.userSelectionInput.ids.length > 0 && this.payrollPriceTypeCodes.length > 0 && this.selectedPayrollPriceTypeCodes.length === 0 && !this.isLoaded) {
                this.selectedPayrollPriceTypeCodes = _.filter(this.payrollPriceTypeCodes, c => _.includes(this.userSelectionInput.ids, c.id));
                this.isLoaded = true;
            }
        });
        this.payrollPriceTypeCodes = [];
        _.forEach(this.payrollPriceTypes, (x) => {
            if (this.selectedPayrollPriceTypes.length == 0 || this.selectedPayrollPriceTypes.find(p => p.id === x.type)) {
                this.payrollPriceTypeCodes.push(new SmallGenericType(x.payrollPriceTypeId, x.code));
            }
        });

        let selection: IPayrollPriceTypeSelectionDTO = new PayrollPriceTypeSelectionDTO(this.selectedPayrollPriceTypeCodes.map(c => c.id), this.selectedPayrollPriceTypes.map(c => c.id));
         this.onSelected({ selection: selection });
    }
}