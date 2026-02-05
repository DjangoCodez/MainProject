export class SelectDateController {
    private selectedDate: Date;
    
    //@ngInject
    constructor(
        private $uibModalInstance,
        defaultDate:Date
        
    ) {
        this.selectedDate = defaultDate;
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
                selectedDate: this.selectedDate
            });
        }
        else {
            this.$uibModalInstance.dismiss('cancel');
        }
    }
}