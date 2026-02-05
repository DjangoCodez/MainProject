import { IIdListSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { IReportDataService } from "../../ReportDataService";

export class VoucherSeriesSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: VoucherSeriesSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/VoucherSeriesSelection/VoucherSeriesSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "voucherSeriesSelection";

    //binding properties
    private labelKey: string;
    private onSelected: (_: { selection: IIdListSelectionDTO }) => void = angular.noop;
    private userSelectionInput: IdListSelectionDTO;

    private selectedVoucherSeriesTypes: SmallGenericType[] = [];
    private voucherSeriesTypes: SmallGenericType[] = [];

    private delaySetSavedUserSelection: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope, private reportDataService: IReportDataService) {
        this.reportDataService.getVoucherSeriesTypes().then(x => {
            _.forEach(_.orderBy(x, 'voucherSeriesTypeNr'), (seriesType: any) => {
                this.voucherSeriesTypes.push(new SmallGenericType(seriesType.voucherSeriesTypeId, seriesType.voucherSeriesTypeNr + ". " + seriesType.name));
            });
        });

        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedVoucherSeriesSelection();
        });
    }

    public $onInit() {
        this.selectedVoucherSeriesTypes = [];

        this.propagateSelection();
    }

    private setSavedVoucherSeriesSelection() {
        if (!this.userSelectionInput)
            return;

        if (this.voucherSeriesTypes.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        this.selectedVoucherSeriesTypes = [];
        if (this.userSelectionInput.ids && this.userSelectionInput.ids.length > 0)
            this.selectedVoucherSeriesTypes = _.filter(this.voucherSeriesTypes, c => _.includes(this.userSelectionInput.ids, c.id));

        this.propagateSelection();
    }

    private propagateSelection() {
        let selection: IIdListSelectionDTO = new IdListSelectionDTO(this.selectedVoucherSeriesTypes.map(c => c.id));

        this.onSelected({ selection: selection });
    }
}