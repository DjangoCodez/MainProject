import { ITimeCodeBreakSmallDTO } from "../../../../../Scripts/TypeLite.Net4";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";

export class SelectBreakTimeCodeController {

    // Properties
    private _selectedBreakTimeCode: ITimeCodeBreakSmallDTO;
    private get selectedBreakTimeCode(): ITimeCodeBreakSmallDTO {
        return this._selectedBreakTimeCode;
    }
    private set selectedBreakTimeCode(item: ITimeCodeBreakSmallDTO) {
        this._selectedBreakTimeCode = item;
        if (this.dragStop) {
            this.breakStopTime = this.breakStartTime.addMinutes(item.defaultMinutes);
        } else {
            this.breakStartTime = this.breakStopTime.addMinutes(-item.defaultMinutes);
        }
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        translationService: ITranslationService,
        private message: string,
        private breakTimeCodes: ITimeCodeBreakSmallDTO[],
        private breakStartTime: Date,
        private breakStopTime: Date,
        private dragStart: boolean,
        private dragStop: boolean) {

        translationService.translate("time.schedule.planning.selectbreaktimecode.info").then(term => {
            this.message = "{0}.\n\n{1}".format(this.message, term);
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.close();
    }

    private close() {
        this.$uibModalInstance.close({ success: true, breakTimeCode: this.selectedBreakTimeCode, breakStartTime: this.breakStartTime });
    }
}

