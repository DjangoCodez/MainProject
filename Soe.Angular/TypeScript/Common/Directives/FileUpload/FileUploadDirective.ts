import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SoeEntityType, SoeEntityImageType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IMessagingService } from "../../../Core/Services/MessagingService";

export class FileUploadDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('FileUpload', 'FileUpload.html'),
            scope: {
                url: "@",
                callback: "&",
                entity: "@",
                imageType: "@",
                recordId: "=?",
                singleFile: "@",
                showDelete: "=?",
                showRoles: "=?",
                rolesMandatory: "=?",
                showMessageGroups: "=?",
                readOnly: '=',
                returnArray: "=?",
                onDelete: '&',
                toolbarmode: '=?',
                parentGuid: '=?',
                showTransferCheckBox: '=?',
                showDistributionCheckBox: '=?',
                showReload: '=?',
                onReload: '&'
            },
            restrict: 'E',
            replace: true,
            controller: FileUploadController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class FileUploadController {
    private url: string;
    private callback: any;
    private entity: SoeEntityType;
    private imageType: SoeEntityImageType;
    private recordId: number;
    private singleFile: any;
    private showDelete: boolean;
    private showRoles: boolean;
    private showMessageGroups: boolean;
    private rolesMandatory: boolean;
    private readOnly: boolean;
    private returnArray: boolean;
    private onDelete: Function;
    private toolbarmode: boolean;
    private _transferAll: boolean;
    private _distributeAll: boolean;
    private parentGuid: any;
    private showTransferCheckBox: boolean;
    private showDistributionCheckBox: boolean;
    private showReload: boolean;
    private onReload: Function;

    get transferAll(): boolean {
        return this._transferAll;
    }
    set transferAll(value: boolean) {
        this._transferAll = value;
        this.messagingService.publish(Constants.EVENT_UPDATE_TRANSFER_ALL, { guid: this.parentGuid, value: value });
    }

    get distributeAll(): boolean {
        return this._distributeAll;
    }
    set distributeAll(value: boolean) {
        this._distributeAll = value;
        this.messagingService.publish(Constants.EVENT_UPDATE_DISTRIBUTE_ALL, { guid: this.parentGuid, value: value });
    }

    public get showSelectAllDistribute() {
        return this.entity === SoeEntityType.Offer || this.entity === SoeEntityType.Order || this.entity === SoeEntityType.CustomerInvoice || this.entity === SoeEntityType.Contract;
    }

    public get showSelectAlltransfer() {
        return this.entity === SoeEntityType.Offer || this.entity === SoeEntityType.Order || this.entity === SoeEntityType.Contract;
    }

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private messagingService: IMessagingService,
        private $scope: any) {
    }

    public showUploadDialog() {
        this.translationService.translate("core.fileupload.fileupload").then((term) => {
            let url;
            if (this.returnArray) {
                url = CoreUtility.apiPrefix + `${Constants.WEBAPI_CORE_FILES_UPLOAD_GETARRAY}`;
            } else if (this.url) {
                url = CoreUtility.apiPrefix + this.url;
            }

            let modal = this.notificationService.showFileUploadEx(term, { url: url, allowMultipleFiles: !this.singleFile, showRoles: this.showRoles, rolesMandatory: this.rolesMandatory, showMessageGroups: this.showMessageGroups, entity: this.entity, imageType: this.imageType, recordId: this.recordId });

            modal.result.then(result => {
                if (this.singleFile) {
                    this.fileUploaded(result.result);
                } else {
                    result.result.forEach(file => {
                        this.fileUploaded(file);
                    });
                }
            });
        });
    }

    private fileUploaded(file: any) {
        if (file.success) {
            if (file.stringValue === "image") {
                var image = file.value;
                image.fileType = file.stringValue;
                image.isDeleted = false;
                this.callback({ result: image });
            } else {
                if (this.returnArray == true) {
                    this.callback({
                        result: {
                            id: file.integerValue,
                            fileName: file.stringValue,
                            description: file.stringValue,
                            array: file.value,
                        }
                    });
                } else {
                    this.callback({
                        result: {
                            id: file.integerValue,
                            fileName: file.stringValue,
                            description: file.stringValue
                        }
                    });
                }
            }
        }
    }

    private delete() {
        if (this.onDelete)
            this.onDelete();
    }

    private reload() {
        if (this.onReload)
            this.onReload();
    }
}
