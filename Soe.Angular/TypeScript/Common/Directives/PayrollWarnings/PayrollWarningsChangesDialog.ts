import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";

export class PayrollWarningsChangesDialogController {
    public progress: IProgressHandler;
    private

    //@ngInject
    constructor(private $uibModalInstance,
        private $uibModal,
        private $q: ng.IQService,
        private terms: { [index: string]: string; },
        private changes: any) {
       
    }

    // SETUP

   
    getAttestState(value: string, fieldType: number) {
        if (fieldType !== 3)
            return value;

        if(value=="0")
            return this.terms["core.yes"];
        else
            return this.terms["core.no"];
    }

    private ok() {
        
        this.$uibModalInstance.dismiss();
    }

}