import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ChecklistExtendedRowDTO } from "../../../../../Common/Models/checklistdto";
import { ToolBarUtility, ToolBarButtonGroup } from "../../../../../Util/ToolBarUtility";
import { IFileUploadDTO, ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { FilesHelper } from "../../../../../Common/Files/FilesHelper";
import { SoeEntityImageType, SoeEntityType, TermGroup_ChecklistRowType } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ImageDTO } from "../../../../../Common/Models/ImageDTO";

export class EditChecklistRowDialogController {

    private title: string;
    private question: string;
    private showNavigationButtons: boolean = true;
    private yesNoDict: ISmallGenericType[] = [];
    private questionLabel: string;
    private rowsLen: number;
    private disabled: boolean = true;
    private readonly filesHelper: FilesHelper;
    
    protected navigationMenuButtons = new Array<ToolBarButtonGroup>();
    private terms: { [index: string]: string; };

    private edit: ng.IFormController;

    //@ngInject
    constructor(private $scope: ng.IScope,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private row: ChecklistExtendedRowDTO,
        private rows: Array<ChecklistExtendedRowDTO>, 
        private index: number,
        private rowsCtrl: any,
        private questionsTerm: string,
        private readOnly: boolean
        ) {
        // Setup

        this.filesHelper = new FilesHelper(this.coreService, this.$q, undefined, false, SoeEntityType.ChecklistHeadRecord, SoeEntityImageType.ChecklistHeadRecord, () => this.row.rowRecordId, null, true);
        this.setup();
        this.setupNavigationGroup();
    }

    private loadTerms(): ng.IPromise<any> {
        // Columns
        const keys: string[] = [
            "core.question",
            "core.yes",
            "core.no"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.questionsTerm = terms["core.question"];
            this.yesNoDict.push({ id: 0, name: "" });
            this.yesNoDict.push({ id: 1, name: this.terms["core.yes"] });
            this.yesNoDict.push({ id: 2, name: this.terms["core.no"] });
        });
    }

    protected setup() {
        this.loadTerms().then(() => {
            this.rowsLen = this.rows.length
            this.setTitle();
            this.question = this.row.text;
            if (this.row.fileUploads && this.row.fileUploads.length > 0) {
                this.filesHelper.files = this.parseToImageDTO(this.row.fileUploads);
                this.filesHelper.filesLoaded = true;
            }
            else
                this.filesHelper.loadFiles(true);
        })
    }

    protected parseToImageDTO(images: IFileUploadDTO[]): ImageDTO[] {
        const imageDTOs: ImageDTO[] = [];
        _.forEach(images, (img) => {
            const dto = new ImageDTO();

            dto.imageId = dto["id"] = img.imageId ?? img.id;
            dto.fileName = img.fileName;
            dto.description = img.description;
            dto.includeWhenDistributed = img.includeWhenDistributed;
            dto.includeWhenTransfered = img.includeWhenTransfered;
            dto.invoiceAttachmentId = img.invoiceAttachmentId;
            dto.dataStorageRecordType = img.dataStorageRecordType;
            dto.sourceType = img.sourceType; 
            dto.created = new Date();
            dto.isModified = true;
            dto.isAdded = true;
            dto.canDelete = true;
            dto.setFileFormat();

            imageDTOs.push(dto);
        });
        return imageDTOs;
    }
       
    protected setupNavigationGroup() {
        const group = ToolBarUtility.createNavigationGroup(
            () => {
                if (this.rowsLen > 0) {
                    this.row = this.rows[0];
                    this.index = 0;
                    this.setTitle();
                }
            },
            () => {
                if (this.index - 1 < 0) {
                    this.index = this.rowsLen - 1;
                }
                else {
                    this.index -= 1;
                }

                this.row = this.rows[this.index];
                this.setTitle();
            },
            () => {
                if (this.index + 1 > this.rowsLen - 1) {
                    this.index = 0;
                }
                else
                {
                    this.index += 1;
                }

                this.row = this.rows[this.index];
                this.setTitle();
            },
            () => {
                if (this.rowsLen > 0) {
                    this.index = this.rowsLen - 1
                    this.row = this.rows[this.index];
                    this.setTitle();
                }
            },
            null,
            null
        );

        this.navigationMenuButtons.push(group);
    }

    private setTitle() {
        this.title = this.questionsTerm + ' ' + this.row.rowNr + '/' + this.rowsLen;
        this.isMandatory();
    }

    private isMandatory() {
        if (this.row.mandatory) {
            this.questionLabel = this.questionsTerm + '*'
        }
        else
        {
            this.questionLabel = this.questionsTerm
        }
    }

    private cancel() {
        this.$uibModalInstance.dismiss();
    }

    private close() {
        let hasModified = false;
        if (this.row.type === TermGroup_ChecklistRowType.Image) {
            this.$scope.$broadcast('stopEditing', {
                functionComplete: () => {
                    this.row.fileUploads = this.filesHelper.getAsDTOs(true);
                    this.row.boolData = this.filesHelper.nbrOfFiles > 0;
                    hasModified = this.row.fileUploads.length > 0;
                    if (hasModified) {
                        this.row.isModified = true;
                    }
                    this.$uibModalInstance.close({ modified: hasModified, row: this.row });
                }
            });
        }
        else {
            this.$uibModalInstance.close({ modified: true, row: this.row });
        }
        
    }
}