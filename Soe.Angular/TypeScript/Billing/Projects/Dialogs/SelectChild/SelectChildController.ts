import { ProjectTimeMatrixSaveRowDTO } from "../../../../Common/Models/ProjectTimeMatrixDTO";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { CalendarUtility } from "../../../../Util/CalendarUtility";

export class SelectChildController {

    private selectedRow: ProjectTimeMatrixSaveRowDTO;
    private modified: boolean = false;
    //@ngInject
    constructor(
        private $uibModalInstance,
        private title: string,
        private weekFrom: Date,
        private childs: SmallGenericType[],
        private rows: ProjectTimeMatrixSaveRowDTO[]
    ) {
        
    }

    private weekDay2Date(weekDay: number): string {
        return CalendarUtility.toFormattedDate(this.weekFrom.addDays(weekDay-1));
    }

    private childchanging(row: ProjectTimeMatrixSaveRowDTO) {
        this.modified = row.isModified = true;
    }

    buttonCancelClick() {
        this.close(false);
    }

    buttonOkClick() {
        this.close(true);
    }
  
    close(ok: boolean) {
        if (ok) {
            this.$uibModalInstance.close({
                rows: this.rows,
                modified: this.modified
            });
        }
        else {
            this.$uibModalInstance.dismiss('cancel');
        }
    }
}