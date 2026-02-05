import { ITimeService } from "../../../../Time/TimeService";
import { TimeStampAdditionDTO, TimeStampEntryDTO, TimeStampEntryExtendedDTO } from "../../../../../Common/Models/TimeStampDTOs";
import { TimeStampAdditionType } from "../../../../../Util/CommonEnumerations";

export class TimeStampAdditionsDialogController {

    private rows: TimeStampEntryExtendedDTO[] = [];
    private accRows: TimeStampEntryExtendedDTO[] = [];
    private additions: TimeStampAdditionDTO[] = [];

    //@ngInject
    constructor(private $uibModalInstance,
        private $q: ng.IQService,
        private timeService: ITimeService,
        private timeStamp: TimeStampEntryDTO,
        private isMyTime: boolean) {

        // Create a copy of the rows to be able to cancel
        if (this.timeStamp.extended) {
            this.rows = this.timeStamp.extended.filter(e => e.timeCodeId || e.timeScheduleTypeId).map(x => {
                let obj = new TimeStampEntryExtendedDTO();
                angular.extend(obj, x);
                return obj;
            });

            // Store account rows in a separate collection
            // Accounts are not visible or editable here,
            // but needs to be sent down to server again when saving to prevent deletion
            this.accRows = this.timeStamp.extended.filter(e => e.accountId).map(x => {
                let obj = new TimeStampEntryExtendedDTO();
                angular.extend(obj, x);
                return obj;
            });
        } else {
            this.rows = [];
            this.accRows = [];
        }

        this.$q.all([
            this.loadTimeStampAdditions()
        ]).then(() => {
            this.setTypes();
        });
    }

    // SETUP

    private setTypes() {
        this.rows.forEach(row => {
            let addition: TimeStampAdditionDTO;
            if (row.timeScheduleTypeId) {
                addition = this.additions.find(a => a.id === row.timeScheduleTypeId);
            } else if (row.timeCodeId) {
                addition = this.additions.find(a => a.id === row.timeCodeId);
            }
            row.addition = addition;
        });
    }

    // SERVICE CALLS

    private loadTimeStampAdditions(): ng.IPromise<any> {
        return this.timeService.getTimeStampAdditions(this.isMyTime).then(x => {
            this.additions = x;
        });
    }

    // HELP-METHODS

    private get disableOk():boolean {
        return this.rows.filter(r => !r.addition).length > 0;
    }

    // EVENTS

    private additionChanged(row: TimeStampEntryExtendedDTO, addition: TimeStampAdditionDTO) {
        row.addition = addition;
        switch (addition.type) {
            case TimeStampAdditionType.TimeScheduleType:
                row.timeScheduleTypeId = addition.id;
                row.timeCodeId = null;
                row.quantity = null;
                break;
            case TimeStampAdditionType.TimeCodeConstantValue:
                row.timeScheduleTypeId = null;
                row.timeCodeId = addition.id;
                row.quantity = addition.fixedQuantity;
                break;
            case TimeStampAdditionType.TimeCodeVariableValue:
                row.timeScheduleTypeId = null;
                row.timeCodeId = addition.id;
                row.quantity = null;
                break;
        }
    }

    private addRow() {
        let row = new TimeStampEntryExtendedDTO();
        this.rows.push(row);
    }

    private deleteRow(row: TimeStampEntryExtendedDTO) {
        _.pull(this.rows, row);
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        if (this.rows) {
            this.timeStamp.extended = this.rows.map(x => {
                let obj = new TimeStampEntryExtendedDTO();
                angular.extend(obj, x);
                return obj;
            });
        } else {
            this.timeStamp.extended = [];
        }

        // Add account rows stored initially
        if (this.accRows) {
            this.timeStamp.extended = this.timeStamp.extended.concat(this.accRows.map(x => {
                let obj = new TimeStampEntryExtendedDTO();
                angular.extend(obj, x);
                return obj;
            }));
        }

        this.$uibModalInstance.close(this.timeStamp);
    }
}