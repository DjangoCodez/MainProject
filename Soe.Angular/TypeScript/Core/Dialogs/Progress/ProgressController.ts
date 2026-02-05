import { ICoreService } from "../../Services/CoreService";
import { ITranslationService } from "../../Services/TranslationService";
import { IUrlHelperService } from "../../Services/UrlHelperService";

export class ProgressController {

    private focusValue: string;

    private html: string;

    //@ngInject
    constructor(
        $http,
        $sce,
        private $scope: ng.IScope,
        private $uibModalInstance,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        public metadata,
        private progressParent: any) {

        this.$uibModalInstance.rendered.then(() => {
            this.focusValue = "ok";
        });

        if ((progressParent) && (!progressParent.progressModalBusy)) {
            this.$scope.$applyAsync(() => this.close());
        }

        this.$scope.$watch(() => this.metadata.html, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.html = this.metadata.html ? $sce.trustAsHtml(this.metadata.html) : '';
        });
    }

    close() {
        this.progressParent.progressModalBusy = false;
        this.$uibModalInstance.close();
    }

    abort() {
        this.progressParent.progressModalBusy = false;
        this.$uibModalInstance.dismiss();
    }
}