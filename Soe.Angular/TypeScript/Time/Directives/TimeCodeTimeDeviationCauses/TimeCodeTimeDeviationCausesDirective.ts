import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ITimeService } from "../../Time/TimeService";
import { SoeTimeCodeType } from "../../../Util/CommonEnumerations";
import { IQService } from "angular";
import { TimeCodeTimeDeviationCausesDialogController } from "./TimeCodeTimeDeviationCausesDialogController";
import { TimeCodeBreakTimeCodeDeviationCauseDTO } from "../../../Common/Models/TimeCode";

export class TimeCodeTimeDeviationCausesDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl("Time/Directives/TimeCodeTimeDeviationCauses/TimeCodeTimeDeviationCauses.html"),
            scope: {
                rows: '=',
                readOnly: '=?',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: TimeCodeTimeDeviationCausesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class TimeCodeTimeDeviationCausesController {

    // Init parameters
    private rows: TimeCodeBreakTimeCodeDeviationCauseDTO[];
    private readOnly: boolean;
    private onChange: Function;

    // Data
    private timeDeviationCauses: ISmallGenericType[] = [];
    private timeCodes: ISmallGenericType[] = [];

    // Properties
    private selectedRow: TimeCodeBreakTimeCodeDeviationCauseDTO;

    //@ngInject
    constructor(
        private $uibModal,
        private $scope: ng.IScope,
        private $q: IQService,
        private timeService: ITimeService,
        private urlHelperService: IUrlHelperService) {
    }

    public $onInit() {
        this.$q.all([
            this.loadTimeDeviationCauses(),
            this.loadTimeCodes()]).then(() => {
                this.setupWatchers();
            });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.rows, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.selectedRow = this.rows && this.rows.length > 0 ? this.rows[0] : null;
                this.setTimeDeviationCauseNames();
                this.setTimeCodeNames();
            }
        });
    }

    // SERVICE CALLS

    private loadTimeDeviationCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCausesDict(false, false).then(x => {
            this.timeDeviationCauses = x;
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.timeService.getTimeCodesDict(SoeTimeCodeType.None, false, false).then(x => {
            this.timeCodes = x;
        });
    }

    // EVENTS

    private edit(row: TimeCodeBreakTimeCodeDeviationCauseDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/TimeCodeTimeDeviationCauses/TimeCodeTimeDeviationCausesDialog.html"),
            controller: TimeCodeTimeDeviationCausesDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                timeDeviationCauses: () => { return this.timeDeviationCauses },
                timeCodes: () => { return this.timeCodes },
                row: () => { return row },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.row) {
                if (!row) {
                    // Add new
                    row = new TimeCodeBreakTimeCodeDeviationCauseDTO();
                    if (!this.rows)
                        this.rows = [];
                    this.rows.push(row);
                }

                // Update fields
                row.timeCodeDeviationCauseId = result.row.timeCodeDeviationCauseId;
                row.timeCodeId = result.row.timeCodeId;
                this.setTimeDeviationCauseName(row);
                this.setTimeCodeName(row);
                this.selectedRow = row;

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private delete(row: TimeCodeBreakTimeCodeDeviationCauseDTO) {
        _.pull(this.rows, row);

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private setTimeDeviationCauseNames() {
        _.forEach(this.rows, row => {
            this.setTimeDeviationCauseName(row);
        });
    }

    private setTimeDeviationCauseName(row: TimeCodeBreakTimeCodeDeviationCauseDTO) {
        let cause = _.find(this.timeDeviationCauses, t => t.id === row.timeCodeDeviationCauseId);
        row.timeDeviationCauseName = cause ? cause.name : '';
    }

    private setTimeCodeNames() {
        _.forEach(this.rows, row => {
            this.setTimeCodeName(row);
        });
    }

    private setTimeCodeName(row: TimeCodeBreakTimeCodeDeviationCauseDTO) {
        let code = _.find(this.timeCodes, t => t.id === row.timeCodeId);
        row.timeCodeName = code ? code.name : '';
    }
}