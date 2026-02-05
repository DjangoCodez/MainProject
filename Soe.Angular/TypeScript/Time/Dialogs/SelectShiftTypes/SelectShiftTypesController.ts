import { ISoeGridOptions, SoeGridOptions } from "../../../Util/SoeGridOptions";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";
import { ShiftTypeDTO } from "../../../Common/Models/ShiftTypeDTO";

export class SelectShiftTypesController {

    protected soeGridOptions: ISoeGridOptions;

    private selectAllShiftTypes: boolean = false;

    //@ngInject
    constructor(private $uibModalInstance,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private shiftTypes: ShiftTypeDTO[],
        private selectedShiftTypes: number[]) {

        this.soeGridOptions = new SoeGridOptions("SelectShiftTypes", $timeout, uiGridConstants);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.enableDoubleClick = false;
        this.soeGridOptions.setMinRowsToShow(10);

        // 'Not selected' should always be sorted first
        var notSelected = _.find(this.shiftTypes, s => s.shiftTypeId === 0);
        if (notSelected)
            notSelected['sortFirst'] = true;

        this.setupGridColumns();

        if (this.selectedShiftTypes.length === this.shiftTypes.length)
            this.selectAllShiftTypes = true;
    }

    private setupGridColumns() {
        var keys: string[] = [
            "core.select",
            "common.shifttype"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.soeGridOptions.addColumnBool("selected", terms["core.select"], "60", true, "select");
            this.soeGridOptions.addColumnText("name", terms["common.shifttype"], null);

            this.soeGridOptions.setData(this.sort());
        });
    }

    private selectAll() {
        this.$timeout(() => {
            _.forEach(this.shiftTypes, shiftType => {
                shiftType['selected'] = this.selectAllShiftTypes;
                this.select(shiftType);
            });
        });
    }

    private select(row) {
        this.$timeout(() => {
            if (row.selected === true) {
                if (!_.includes(this.selectedShiftTypes, row.shiftTypeId))
                    this.selectedShiftTypes.push(row.shiftTypeId);
            } else {
                if (_.includes(this.selectedShiftTypes, row.shiftTypeId))
                    this.selectedShiftTypes.splice(this.selectedShiftTypes.indexOf(row.shiftTypeId), 1);
            }
        });
    }

    private sort() {
        // Mark selected
        if (this.selectedShiftTypes) {
            _.forEach(this.shiftTypes, shiftType => {
                shiftType['selected'] = (_.includes(this.selectedShiftTypes, shiftType.shiftTypeId));
            });
        }

        return _.orderBy(this.shiftTypes, ['sortFirst', 'name'], ['asc', 'asc'])
    }

    private ok() {
        this.$uibModalInstance.close({ success: true, selectedShiftTypes: this.selectedShiftTypes });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}
