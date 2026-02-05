export class FieldOptionsController {
    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $scope: ng.IScope,
        private field: any,
        private uniqueValues: string[]) {
    }

    private save() {
        this.field.isConfigured = true;
        this.$uibModalInstance.close(this.field);
    }

    private close() {
        this.$uibModalInstance.close();
    }
    private okValid() {
    }
}