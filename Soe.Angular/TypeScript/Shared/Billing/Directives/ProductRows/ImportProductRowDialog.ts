import { ICoreService } from "../../../../Core/Services/CoreService";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { TermGroup, TermGroup_Country, TermGroup_InvoiceRowImportType } from "../../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { HtmlUtility } from "../../../../Util/HtmlUtility";

export class ImportProductRowController {
    private bytes: any;
    private filename: string;
    private importType: ISmallGenericType[] = [];
    private typeId: TermGroup_InvoiceRowImportType;

    //@ngInject
    constructor(private $uibModalInstance, private coreService: ICoreService, private $q: ng.IQService, private $window) {
        this.$q.all([
            this.loadFileTypes(),
        ]);
    }

    private loadFileTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceRowImportType, false, false).then((x) => {
            this.importType = [];
            _.forEach(x, (row) => {
                if (row.id === TermGroup_InvoiceRowImportType.Jcad) {
                    if (CoreUtility.sysCountryId === TermGroup_Country.FI) {
                        this.importType.push({ id: row.id, name: row.name });
                    }
                } else {
                    this.importType.push({ id: row.id, name: row.name });
                }
            });
        });
    }

    fileUploaded(result: any) {
        if (result) {
            this.bytes = result.array;
            this.filename = result.fileName;
        }
    }

    buttonOkClick() {
        this.$uibModalInstance.close({ bytes: this.bytes, typeId: this.typeId });
    }

    buttonDownloadTemplateClick() {
        return this.coreService.getExcelProductRowsTemplate().then((x) => {
            HtmlUtility.openInNewTab(this.$window, x.href);
        });
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}