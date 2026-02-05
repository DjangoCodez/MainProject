import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ProductRowsContainers, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { TextBlockDTO } from "../../Models/InvoiceDTO";
import { CompTermDTO } from "../../Models/CompTermDTO";
import { SimpleTextEditorDialogMode, CompTermsRecordType, TermGroup } from "../../../Util/CommonEnumerations";

export class TextBlockDialogController {

    type: number;
    entity: number;
    langId: number;
    editPermission: boolean;
    terms: { [index: string]: string; };
    container: ProductRowsContainers;
    mode: SimpleTextEditorDialogMode;
    textBlocks: TextBlockDTO[];
    currentTextBlock: TextBlockDTO;
    templates: any[];
    languages: any[];
    maxTextLength: number = null;
    originalTextExists: boolean;

    // Flags
    showOk = false;
    showTitle = false;
    showTemplatesSelect = false;
    showTemplatesButtons = false;
    showTranslations = false;
    textareaDisabled = false;


    // Translations
    compTermRecordType: number;
    compTermRows: CompTermDTO[];

    //Validation
    saveTemplateValid = false;
    deleteTemplateValid = false;
    okValid = false;

    private _text: any;
    get text() {
        return this._text;
    }
    set text(item: any) {
        this._text = item;
        if ((item && item.length > 0)  || this.originalTextExists) {
            this.okValid = true;
            if (this.title && this.title.length > 0)
                this.saveTemplateValid = true;
            else
                this.saveTemplateValid = false
        }
        else {
            this.okValid = false
        }
    }

    private _title: any;
    get title() {
        return this._title;
    }
    set title(item: any) {
        this._title = item;
        if (item && item.length > 0 && this.text && this.text.length > 0)
            this.saveTemplateValid = true;
        else
            this.saveTemplateValid = false
    }

    private textboxTitle: string;

    private _selectedTemplate: any;
    get selectedTemplate() {
        return this._selectedTemplate;
    }
    set selectedTemplate(item: any) {
        this._selectedTemplate = item;
        if (this.selectedTemplate && this.textBlocks) {
            this.currentTextBlock = _.find(this.textBlocks, t => t.textblockId === this.selectedTemplate);
            if (this.currentTextBlock) {
                this.deleteTemplateValid = true;
                this.text = this.currentTextBlock.text;
                this.title = _.filter(this.templates, i => i.id == this.currentTextBlock.textblockId)[0].name;
            }
            else {
                this.deleteTemplateValid = false;
            }
        }
    }

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private $q: ng.IQService,
        text: string,
        editPermission: boolean = false,
        entity: number = 0,
        type: number = 1,
        headline: string = "",
        mode: SimpleTextEditorDialogMode = 0,
        container: ProductRowsContainers = 0,
        langId: number = 0,
        maxTextLength: number = null,
        textboxTitle: string = null) {
        this.text = text ? text : "";
        this.originalTextExists = this.text && this.text.length;
        this.editPermission = editPermission;
        this.title = headline;
        this.entity = entity;
        this.type = type;
        this.mode = mode;
        this.container = container;
        this.langId = langId;
        this.compTermRecordType = CompTermsRecordType.Textblock;
        this.maxTextLength = maxTextLength;
        this.textboxTitle = textboxTitle;

        this.$q.all([
            this.loadTerms(),
            this.loadLanguages(),
            this.loadTextBlocks()
        ]).then(() => {
            this.setupGui()
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.productrows.productnr",
            "billing.productrows.text",
            "billing.order.syswholeseller",
            "billing.productrows.dialogs.changingwholeseller",
            "billing.productrows.dialogs.failedwholesellerchange",
            "common.savetemplate",
            "common.deletetemplate"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadLanguages(): ng.IPromise<any> {
        this.languages = [];
        return this.coreService.getTermGroupContent(TermGroup.Language, false, false).then((x) => {
            this.languages = x;
        });
    }

    private loadTextBlocks(): ng.IPromise<any> {
        this.templates = [];
        return this.coreService.getTextBlocks(this.entity).then((x: TextBlockDTO[]) => {
            if (this.container == ProductRowsContainers.Contract)
                this.textBlocks = _.filter(x, i => i.type === this.type && i.showInContract === true);
            else if (this.container == ProductRowsContainers.Offer)
                this.textBlocks = _.filter(x, i => i.type === this.type && i.showInOffer === true);
            else if (this.container == ProductRowsContainers.Order)
                this.textBlocks = _.filter(x, i => i.type === this.type && i.showInOrder === true);
            else if (this.container == ProductRowsContainers.Invoice)
                this.textBlocks = _.filter(x, i => i.type === this.type && i.showInInvoice === true);
            else if (this.container == ProductRowsContainers.Purchase)
                this.textBlocks = _.filter(x, i => i.type === this.type && i.showInPurchase === true);
            else
                this.textBlocks = _.filter(x, i => i.type === this.type);

            this.templates.push({ id: 0, name: " " });
            _.forEach(this.textBlocks, t => {
                this.templates.push({ id: t.textblockId, name: t.headline });
            });
            this.selectedTemplate = 0;

        }, error => {

        });
    }

    private setupGui() {
        switch (this.mode) {
            case SimpleTextEditorDialogMode.Base:
            case SimpleTextEditorDialogMode.EditInvoiceText:
            case SimpleTextEditorDialogMode.EditInvoiceDescription:
                {
                    this.showOk = this.editPermission;
                    this.showTitle = false;
                    this.showTemplatesSelect = false;
                    this.showTemplatesButtons = false;
                    this.showTranslations = false;

                    break;
                }
            case SimpleTextEditorDialogMode.EditInvoiceRowText:
                {
                    this.showOk = this.editPermission;
                    this.showTitle = true;
                    this.showTemplatesSelect = true;
                    this.showTemplatesButtons = true;
                    this.showTranslations = false;

                    break;
                }
            case SimpleTextEditorDialogMode.NewTextBlock:
                {
                    this.showOk = false;
                    this.showTitle = true;
                    this.showTemplatesSelect = false;
                    this.showTemplatesButtons = true;
                    this.showTranslations = true;

                    //SetSaveMode();
                    break;
                }
            case SimpleTextEditorDialogMode.EditTextBlock:
                {
                    this.showOk = false;
                    this.showTitle = true;
                    this.showTemplatesSelect = false;
                    this.showTemplatesButtons = true;
                    this.showTranslations = true;

                    //SetUpdateMode();
                    break;
                }
            case SimpleTextEditorDialogMode.EditWorkingDescription:
                {
                    this.showOk = this.editPermission;
                    this.showTitle = true;
                    this.showTemplatesSelect = this.editPermission;
                    this.showTemplatesButtons = this.editPermission;
                    this.showTranslations = false;

                    break;
                }
            case SimpleTextEditorDialogMode.AddTextblockToInvoiceRow:
                {
                    //TextArea.Visibility = Visibility.Collapsed;
                    //NumberOfInputCharsLeft.Visibility = Visibility.Collapsed;

                    this.showOk = this.editPermission;
                    this.showTitle = false;
                    this.showTemplatesSelect = true;
                    this.showTemplatesButtons = false;
                    this.showTranslations = true;

                    break;
                }
            case SimpleTextEditorDialogMode.AddSupplierInvoiceBlockReason:
                {
                    this.showOk = true;
                    this.showTitle = false;
                    this.showTemplatesSelect = false;
                    this.showTemplatesButtons = false;
                    this.showTranslations = false;
                    this.textareaDisabled = !this.editPermission;
                    break;
                }
        }
    }

    private saveTextBlock() {

        if (this.currentTextBlock) { //update existing textblock                
            this.currentTextBlock.headline = this.title;
            this.currentTextBlock.text = this.text;
        }
        else { //create new textblock

            //first check if textblock with same name exists
            var existingtemplate = _.filter(this.templates, i => i.name.toLowerCase() == this.title.toLowerCase());

            if (existingtemplate.length > 0) {
                this.showExistingTemplateWarning();
                return;
            }

            this.currentTextBlock = new TextBlockDTO();
            this.currentTextBlock.headline = this.title;
            this.currentTextBlock.text = this.text;
            this.currentTextBlock.type = this.type;
            this.currentTextBlock.showInContract = true;
            this.currentTextBlock.showInOffer = true;
            this.currentTextBlock.showInOrder = true;
            this.currentTextBlock.showInInvoice = true;
        }

        this.coreService.saveTextBlock(this.currentTextBlock, this.entity, this.compTermRows).then((x: TextBlockDTO[]) => {
            this.loadTextBlocks();
            this.currentTextBlock = null;
        }, error => {

        });
    }

    public deleteTextBlock() {
        this.coreService.deleteTextBlock(this.currentTextBlock.textblockId).then((result) => {
            if (!result.success) {
                console.log("result", result);
            }
        }, error => {

        });
    }

    private save() {
        this.$uibModalInstance.close({ text: this.text });
    }

    private close() {
        this.$uibModalInstance.close();
    }

    private showExistingTemplateWarning() {
        const keys: string[] = [
            "core.warning",
            "billing.order.textblock.existingtemplatewarning",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.notificationService.showDialog(terms["core.warning"], terms["billing.order.textblock.existingtemplatewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
        });
    }
}