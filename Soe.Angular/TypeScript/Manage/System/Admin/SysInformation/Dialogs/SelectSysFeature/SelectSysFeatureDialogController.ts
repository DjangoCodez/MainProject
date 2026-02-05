import { ISmallGenericType } from "../../../../../../Scripts/TypeLite.Net4";

export class SelectSysFeatureDialogController {

    private filter: string;
    private showOnlySelected: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private selectableSysFeatures: ISmallGenericType[],
        private selectedSysFeatures: ISmallGenericType[]) {

        if (this.selectedSysFeatures.length > 0) {
            _.forEach(this.selectedSysFeatures, feature => {
                let sysFeature = _.find(this.selectableSysFeatures, s => s.id === feature.id);
                if (sysFeature)
                    sysFeature['selected'] = true;
            });
            this.showOnlySelected = true;
        }
    }

    private get filteredSysFeatures(): ISmallGenericType[] {
        return _.filter(this.selectableSysFeatures, s => (!this.showOnlySelected || s['selected']) && (!this.filter || (_.includes(s.name.toLocaleLowerCase(), this.filter.toLocaleLowerCase()) || _.includes(s.id.toString(), this.filter))));
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        this.$uibModalInstance.close({ selectedSysFeatures: _.filter(this.selectableSysFeatures, s => s['selected']) });
    }
}
