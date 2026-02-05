import { ITranslationService } from "../Services/TranslationService";
import { INotificationService } from "../Services/NotificationService";
import { IFileUploaderFactory } from "../Services/fileuploaderfactory";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../Util/Enumerations";
import { ISmallGenericType } from "../../Scripts/TypeLite.Net4";
import { ICoreService } from "../Services/CoreService";
import { StringUtility } from "../../Util/StringUtility";
import { CoreUtility } from "../../Util/CoreUtility";

// https://github.com/nervgh/angular-file-upload

export class FileUploadController {

    private maxSize: number = 20 * 1024 * 1024; // 20 MB
    private baseUrl: string;
    private uploader: any;
    private uploaderCreated: boolean = false;

    private selectedOption: number;
    private isChecked: boolean = false;
    private selectedDate: Date;

    private selectableRoles: ISmallGenericType[] = [];
    private selectedRoles: ISmallGenericType[] = [];

    private selectableMessageGroups: ISmallGenericType[] = [];
    private selectedMessageGroups: ISmallGenericType[] = [];

    //@ngInject
    constructor(private $uibModalInstance,
        fileUploaderFactory: IFileUploaderFactory,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        url: string,
        private title: string,
        private showDropZone: boolean,
        private showQueue: boolean,
        private allowMultipleFiles: boolean,
        private noMaxSize: boolean,
        private showRoles: boolean,
        private rolesMandatory: boolean,
        private showMessageGroups: boolean,
        private showSelect?: boolean,
        private selectLabel?: string,
        private selectOptions?: ISmallGenericType[],
        private defaultOption?: number,
        private showDate?: boolean,
        private dateLabel?: string,
        private defaultDate?: Date,
        private showCheckBox?: boolean,
        private checkBoxLabel?: string,
        private defaultChecked?: boolean) {

        this.uploader = fileUploaderFactory.create(url);
        this.baseUrl = url;
        this.uploaderCreated = true;

        // Limit file size
        this.uploader.onAfterAddingFile = ((item) => {
            if (!noMaxSize && item.file.size > this.maxSize) {
                this.fileTooLarge(item, this.uploader);
            }
        });
        this.uploader.onBeforeUploadItem = ((item) => {
            if (!noMaxSize && item.file.size > this.maxSize) {
                this.fileTooLarge(item, this.uploader);
            } else {
                if (this.baseUrl.endsWith('/'))
                    this.baseUrl = this.baseUrl.left(this.baseUrl.length - 1);

                item.url = this.baseUrl;
                if (this.showSelect)
                    item.url += '/' + this.selectedOption;
                if (this.showDate)
                    item.url += '/' + (this.selectedDate ? this.selectedDate.toDateTimeString() : 'null');
                if (this.showCheckBox)
                    item.url += '/' + this.isChecked;
                if (this.showRoles || this.showMessageGroups) {
                    item.url += '/' + ((this.showRoles && this.selectedRoles.length > 0) ? this.selectedRoles.map(r => r.id).join(',') : '0');
                    item.url += '/' + ((this.showMessageGroups && this.selectedMessageGroups.length > 0) ? this.selectedMessageGroups.map(g => g.id).join(',') : '0');
                }
            }
        });

        if (!allowMultipleFiles) {
            this.uploader.queueLimit = 1;
            this.uploader.autoUpload = !this.showRoles;

            this.uploader.onSuccessItem = ((item, response, status, headers) => {
                this.$uibModalInstance.close({ result: response, options: this.getReturnOptions() });
            });
            this.uploader.onErrorItem = ((item, response, status, headers) => {
            });
        } else {
            var successResponses = [];

            this.uploader.onSuccessItem = ((item, response, status, headers) => {
                successResponses.push(response);
            });

            this.uploader.onCompleteAll = (() => {
                this.$uibModalInstance.close({ result: successResponses, options: this.getReturnOptions() });
            });
        }

        if (this.showSelect && this.selectOptions && this.selectOptions.length > 0 && this.defaultOption)
            this.selectedOption = this.defaultOption;

        if (this.showDate && this.defaultDate)
            this.selectedDate = this.defaultDate;

        if (this.showCheckBox && this.defaultChecked)
            this.isChecked = true;

        if (this.showRoles)
            this.loadRoles();
        if (this.showMessageGroups)
            this.loadMessageGroups();
    }

    private loadRoles(): ng.IPromise<any> {
        return this.coreService.getCompanyRolesDict(false, false).then(x => {
            this.selectableRoles = x;
            if (this.rolesMandatory)
                this.selectedRoles = _.filter(this.selectableRoles, r => r.id === CoreUtility.roleId);
        });
    }

    private loadMessageGroups(): ng.IPromise<any> {
        return this.coreService.getMessageGroupsDict(false).then(x => {
            this.selectableMessageGroups = x;
        });
    }

    private fileTooLarge(item, uploader) {
        // Cancel upload
        uploader.cancelItem(item);
        uploader.removeFromQueue(item);

        // Show message to user
        const keys = [
            "core.fileupload.filetoolarge.title",
            "core.fileupload.filetoolarge.message"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var size: number = item.file.size;
            size = (size / 1024 / 1024).round(1);
            var maxMB = (this.maxSize / 1024 / 1024).round(0);
            this.notificationService.showDialog(terms["core.fileupload.filetoolarge.title"], terms["core.fileupload.filetoolarge.message"].format(size.toString(), maxMB.toString()), SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        });
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private getReturnOptions() {
        return { selectedOption: this.selectedOption, selectedDate: this.selectedDate, isChecked: this.isChecked, selectedRoles: this.selectedRoles, selectedMessageGroups: this.selectedMessageGroups };
    }
}