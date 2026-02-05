import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TimeCodeDialogController } from "./TimeCodeDialogController";
import { ITimeService } from "../../../../Time/TimeService";
import { TimeAccumulatorTimeCodeDTO } from "../../../../../Common/Models/TimeAccumulatorDTOs";
import { SoeTimeCodeType } from "../../../../../Util/CommonEnumerations";

export class TimeCodesDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Time/TimeAccumulators/Directives/TimeCodes/TimeCodes.html'),
            scope: {
                timeCodes: '=',
                readOnly: '=',
                onChange: '&',
            },
            restrict: 'E',
            replace: true,
            controller: TimeCodesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class TimeCodesController {

    // Init parameters
    private timeCodes: TimeAccumulatorTimeCodeDTO[];
    private readOnly: boolean;
    private onChange: Function;

    // Data
    private selectedTimeCode: TimeAccumulatorTimeCodeDTO;
    private allTimeCodes: ISmallGenericType[];

    //@ngInject
    constructor(
        private $uibModal,
        private $scope: ng.IScope,
        private timeService: ITimeService,
        private urlHelperService: IUrlHelperService) {

        this.loadTimeCodes().then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.timeCodes, (newVal, oldVal) => {
            this.selectedTimeCode = this.timeCodes && this.timeCodes.length > 0 ? this.timeCodes[0] : null;
            this.setTimeCodeNames();
        });
    }

    // SERVICE CALLS

    private loadTimeCodes(): ng.IPromise<any> {
        return this.timeService.getTimeCodesDict(SoeTimeCodeType.None, true, false).then(x => {
            this.allTimeCodes = x;
        });
    }

    // EVENTS

    private editTimeCode(timeCode: TimeAccumulatorTimeCodeDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeAccumulators/Directives/TimeCodes/TimeCodeDialog.html"),
            controller: TimeCodeDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                timeCodes: () => { return this.allTimeCodes },
                timeCode: () => { return timeCode },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.timeCode) {
                if (!timeCode) {
                    // Add new
                    timeCode = new TimeAccumulatorTimeCodeDTO();
                    if (!this.timeCodes)
                        this.timeCodes = [];
                    this.timeCodes.push(timeCode);
                }

                // Update fields
                timeCode.timeCodeId = result.timeCode.timeCodeId;
                timeCode.factor = result.timeCode.factor;
                timeCode.importDefault = result.timeCode.importDefault;
                this.setTimeCodeName(timeCode);
                this.selectedTimeCode = timeCode;

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deleteTimeCode(timeCode: TimeAccumulatorTimeCodeDTO) {
        _.pull(this.timeCodes, timeCode);

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private setTimeCodeNames() {
        _.forEach(this.timeCodes, timeCode => {
            this.setTimeCodeName(timeCode);
        });
    }

    private setTimeCodeName(timeCode: TimeAccumulatorTimeCodeDTO) {
        let tc = _.find(this.allTimeCodes, t => t.id === timeCode.timeCodeId);
        timeCode.timeCodeName = tc ? tc.name : '';
    }
}