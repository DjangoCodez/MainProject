import { ISoeGridOptions, SoeGridOptions } from "../../../Util/SoeGridOptions";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";
import { DayTypeDTO } from "../../../Common/Models/DayTypeDTO";

export class SelectDayTypesController {

    protected soeGridOptions: ISoeGridOptions;

    //@ngInject
    constructor(private $uibModalInstance,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private dayTypes: DayTypeDTO[],
        private selectedDayTypes: number[]) {

        this.soeGridOptions = new SoeGridOptions("SelectDayTypes", $timeout, uiGridConstants);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.enableDoubleClick = false;
        this.soeGridOptions.setMinRowsToShow(10);

        // 'Not selected' should always be sorted first
        var notSelected = _.find(this.dayTypes, s => s.dayTypeId === 0);
        if (notSelected)
            notSelected['sortFirst'] = true;

        this.setupGridColumns();
    }

    private setupGridColumns() {
        var keys: string[] = [
            "core.select",
            "common.daytype"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.soeGridOptions.addColumnBool("selected", terms["core.select"], "15%", true, "select");
            this.soeGridOptions.addColumnText("name", terms["common.daytype"], null);

            this.soeGridOptions.setData(this.sort());
        });
    }

    private select(row) {
        this.$timeout(() => {
            if (row.selected === true) {
                if (!_.includes(this.selectedDayTypes, row.dayTypeId))
                    this.selectedDayTypes.push(row.dayTypeId);
            } else {
                if (_.includes(this.selectedDayTypes, row.dayTypeId))
                    this.selectedDayTypes.splice(this.selectedDayTypes.indexOf(row.dayTypeId), 1);
            }
        });
    }

    private sort() {
        // Mark selected
        if (this.selectedDayTypes) {
            _.forEach(this.dayTypes, dayType => {
                dayType['selected'] = (_.includes(this.selectedDayTypes, dayType.dayTypeId));
            });
        }

        // Sort to get selected at the top
        return _.orderBy(this.dayTypes, ['sortFirst', 'name'], ['asc', 'asc'])
    }

    private ok() {
        this.$uibModalInstance.close({ success: true, selectedDayTypes: this.selectedDayTypes });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}