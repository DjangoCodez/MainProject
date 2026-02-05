import { ISoeGridOptions, SoeGridOptions } from "../../../Util/SoeGridOptions";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";

export class SelectDayOfWeeksController {

    protected soeGridOptions: ISoeGridOptions;

    //@ngInject
    constructor(private $uibModalInstance,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private dayOfWeeks: SmallGenericType[],
        private selectedDayOfWeeks: number[]) {

        this.soeGridOptions = new SoeGridOptions("SelectDayOfWeeks", $timeout, uiGridConstants);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.enableDoubleClick = false;
        this.soeGridOptions.setMinRowsToShow(10);

        this.setupGridColumns();
    }

    private setupGridColumns() {
        var keys: string[] = [
            "core.select",
            "common.weekday"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.soeGridOptions.addColumnBool("selected", terms["core.select"], "15%", true, "select");
            this.soeGridOptions.addColumnText("name", terms["common.weekday"], null);

            this.soeGridOptions.setData(this.sort());
        });
    }

    private select(row) {
        this.$timeout(() => {
            if (row.selected === true) {
                if (!_.includes(this.selectedDayOfWeeks, row.id))
                    this.selectedDayOfWeeks.push(row.id);
            } else {
                if (_.includes(this.selectedDayOfWeeks, row.id))
                    this.selectedDayOfWeeks.splice(this.selectedDayOfWeeks.indexOf(row.id), 1);
            }
        });
    }

    private sort() {
        // Mark selected
        if (this.selectedDayOfWeeks) {
            _.forEach(this.dayOfWeeks, dayOfWeek => {
                dayOfWeek['selected'] = (_.includes(this.selectedDayOfWeeks, dayOfWeek.id));
            });
        }

        return this.dayOfWeeks;
    }

    private ok() {
        this.$uibModalInstance.close({ success: true, selectedDayOfWeeks: this.selectedDayOfWeeks });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}
