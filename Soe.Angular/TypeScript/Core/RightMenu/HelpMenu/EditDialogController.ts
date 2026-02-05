import { SysHelpDTO } from "../../Models/SysHelpDTO";
import { ICoreService } from "../../Services/CoreService";
import { TinyMCEUtility } from "../../../Util/TinyMCEUtility";

export class EditDialogController {
    public model: SysHelpDTO;
    public tinyMceOptions: any;
    //@ngInject
    constructor(
        private help: SysHelpDTO,
        private coreService: ICoreService,
        private $uibModalInstance) {

        coreService.getHelpTitles().then(h => {
            this.tinyMceOptions = {
                ctrl: this,
                baseURL: soeConfig.baseUrl,
                language_url: TinyMCEUtility.getTinyMCELanguageUrl(),
                language: TinyMCEUtility.getTinyMCELanguage(),
                plugins: 'link image code paste help_link lists preview fullscreen',
                toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | image link | code | help_link | collapse |  bullist numlist preview fullscreen',
                paste_data_images: true,
                valid_elements: '*[*]',
                helpTitles: h.map(a => { return { text: a.title, value: a.feature } }),
                menu: {
                    edit: { title: 'Edit', items: 'undo redo | cut copy paste pastetext | selectall' },
                    insert: { title: 'Insert', items: 'link media | template hr' },
                    view: { title: 'View', items: 'visualaid' },
                    format: { title: 'Format', items: 'bold italic underline strikethrough superscript subscript | formats | removeformat' },
                    table: { title: 'Table', items: 'inserttable tableprops deletetable | cell row column' },
                    tools: { title: 'Tools', items: 'spellchecker code' }
                },
                content_style: ".help-toggle { background-color:#FFCCCF;} .help-toggle > a {font-size: 18px;} .help-toggle-content{padding-left: 10px; background-color: #CCFCFF; } .mce-content-body  { font-family:'Roboto Condensed', Verdana, Arial, Helvetica, Sans-serif; font-size: 12px; }",
                verify_html: false,
                apply_source_formatting: false,
                setup: function (editor) {
                    editor.on('init', (event) => {
                        event.target.focus();
                    });
                    editor.on('PostProcess', (ed) => {
                        // replace <p></p> with <br />
                        ed.content = ed.content.replace(/(<p><\/p>)/gi, '<br />');
                    });
                    editor.on('KeyDown', function (e) {
                        // Escape close modal
                        if (e.keyCode == 27)
                            this.settings.ctrl.buttonCancelClick();
                    });
                },
                branding: false,
                browser_spellcheck: true,
                contextmenu: false,
            };
            this.model = angular.copy(this.help);
        });
    }

    public ok() {
        this.coreService.saveHelp(this.model).then(result => {
            this.model.sysHelpId = result.integerValue;
            this.$uibModalInstance.close(this.model);
        });
    }

    public cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}