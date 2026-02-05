import { IdSelectionDTO, TextSelectionDTO, YearAndPeriodSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";

export class JournalRangeSelection {
    public static component(): ng.IComponentOptions {
        return {
            controller: JournalRangeSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/JournalRangeSelection/JournalRangeSelectionView.html",
            bindings: {
                onVoucherSeriesSelectedFrom: "&",
                onVoucherSeriesSelectedTo: "&",
                onVoucherNumberChangedFrom: "&",
                onVoucherNumberChangedTo: "&",
                voucherSeriesFrom: "=",
                voucherSeriesTo: "=",
                voucherNumberFrom: "=",
                voucherNumberTo: "=",
                voucherSeriesList: "<"
            }
        };
    }

    public static componentKey = "journalRangeSelection";

    private voucherSeriesFrom: SmallGenericType;
    private voucherSeriesTo: SmallGenericType;
    private voucherNumberFrom: TextSelectionDTO;
    private voucherNumberTo: TextSelectionDTO;
    private userSelectionInput: YearAndPeriodSelectionDTO;
    private onVoucherSeriesSelectedFrom: (_: { selection: IdSelectionDTO }) => void = angular.noop;
    private onVoucherSeriesSelectedTo: (_: { selection: IdSelectionDTO }) => void = angular.noop;
    private onVoucherNumberChangedFrom: (_: { selection: TextSelectionDTO }) => void = angular.noop;
    private onVoucherNumberChangedTo: (_: { selection: TextSelectionDTO }) => void = angular.noop;

    private voucherSeriesList: SmallGenericType[];

    //@ngInject
    constructor(private $scope: ng.IScope, private $timeout: ng.ITimeoutService) {
    }

    public $onInit() {
    }

    private voucherSeriesChangedFrom(selection: SmallGenericType) {
        if (selection) {
            const selections = new IdSelectionDTO(selection.id);

            if (!this.voucherSeriesTo || this.voucherSeriesTo.id == 0) { //Do not remove 'voucherSeriesTo.id == 0'. Need to check the empty selection. /Dinindu
                this.voucherSeriesTo = selection;
                this.voucherSeriesChangedTo(selection);
            }
            this.onVoucherSeriesSelectedFrom({ selection: selections });
        }
    }

    private voucherSeriesChangedTo(selection: SmallGenericType) {
        if (selection) {
            const selections = new IdSelectionDTO(selection.id);
            this.onVoucherSeriesSelectedTo({ selection: selections });
        }
    }

    private voucherNumberChangedFrom(selection: TextSelectionDTO) {
        if (!this.voucherNumberTo || !this.voucherNumberTo.text) {
            this.voucherNumberTo = new TextSelectionDTO(selection.text);
        }
        this.onVoucherNumberChangedFrom({ selection: selection });
    }

    private voucherNumberChangedTo(selection: TextSelectionDTO) {
        this.onVoucherNumberChangedTo({ selection: selection });
    }
}
