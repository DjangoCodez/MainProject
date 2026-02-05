import { ICoreService } from "../../../Core/Services/CoreService";
import { HtmlUtility } from "../../../Util/HtmlUtility";

export class ShowImageController {
    public image: any;
    //@ngInject
    constructor(private $window, private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private coreService: ICoreService, private imageId: number, public description: string) {
        coreService.getImage(imageId).then(image => {
            this.image = image;
        });
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public print() {
        var printWindow = window.open('', this.image.description);
        printWindow.document.write('<html><head><title>' + this.image.description + '</title>');
        printWindow.document.write('</head><body><img style="width: 100%;" src=\'data:image/jpg;base64,');
        printWindow.document.write(this.image.image);
        printWindow.document.write('\' /></body></html>');
        printWindow.document.close();
        (<any>printWindow).print();
    }

    public download() {
        HtmlUtility.openInNewTab(this.$window, `/ajax/downloadTextFile.aspx?table=datastoragerecord&id=${this.image.imageId}&cid=${soeConfig.actorCompanyId}&useedi=${false}`);
    }
}