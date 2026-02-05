import { ISoeGridOptions, SoeGridOptions } from "../../../../../../Util/SoeGridOptions";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { ShiftDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";

export class EditShiftDebugController {

    private soeGridOptions: ISoeGridOptions;

    //@ngInject
    constructor(private $uibModalInstance,
        translationService: ITranslationService,
        private shifts: ShiftDTO[],
        $timeout: ng.ITimeoutService,
        uiGridConstants: uiGrid.IUiGridConstants) {

        this.soeGridOptions = new SoeGridOptions("", $timeout, uiGridConstants);

        let properties: string[] = [];
        // Create a column for each property
        for (let property in shifts[0]) {
            if (shifts[0].hasOwnProperty(property))
                properties.push(property);
        }

        _.forEach(_.sortBy(properties), prop => {
            this.soeGridOptions.addColumnText(prop, prop, "200");
        });

        this.soeGridOptions.setData(shifts);
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}
