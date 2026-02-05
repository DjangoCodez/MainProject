import { CoreUtility } from "./CoreUtility";

export class TinyMCEUtility {

    public static setupDefaultOptions() {
        return {
            baseURL: soeConfig.baseUrl,
            language_url: this.getTinyMCELanguageUrl(),
            language: this.getTinyMCELanguage(),
            plugins: 'paste lists preview link',
            toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | bullist numlist preview | link',
            link_context_toolbar: true,
            link_title: false,
            target_list: false,
            default_link_target: "_blank",
            link_assume_external_targets: true,
            paste_data_images: true,
            valid_elements: '*[*]',
            helpTitles: { title: " " },
            menu: {
                edit: { title: 'Edit', items: 'undo redo | cut copy paste pastetext | selectall' },
                insert: { title: 'Insert', items: 'template hr' },
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
            },
            branding: false,
            browser_spellcheck: true,
            contextmenu: false,
        };
    }

    public static setupDefaultReadOnlyOptions() {
        return {
            baseURL: soeConfig.baseUrl,
            plugins: 'link',
            toolbar: '',
            link_context_toolbar: true,
            menubar: '',
            statusbar: false,
            branding: false,
            browser_spellcheck: false,
            contextmenu: false,
            editorReadOnly: true,
            content_style: ".mce-content-body  { font-family:'Roboto Condensed', Verdana, Arial, Helvetica, Sans-serif; font-size: 12px; }",
            setup: function (editor) {
                editor.on('init', (event) => {

                    // Make links clickable in readonly editor
                    Array.from(editor.getDoc().querySelectorAll('a')).forEach(el => {
                        el['addEventListener']('click', () => {
                            const href = el['getAttribute']('href');
                            let target = el['getAttribute']('target');
                            if (!target)
                                target = '_blank';

                            if (target !== '_blank') {
                                document.location.href = href;
                            } else {
                                const link = document.createElement('a');
                                link.href = href;
                                link.target = target;
                                link.rel = 'noopener';
                                document.body.appendChild(link);
                                link.click();
                                document.body.removeChild(link);
                            }
                        });
                    });
                });
                editor.on('focus', (event) => {
                    event.target.setMode('readonly');
                });
            },
        };
    }

    public static getTinyMCELanguageUrl(): string {
        var url: string;
        if (soeConfig.baseUrl.startsWithCaseInsensitive('/angular/build')) {
            // Local (build)
            url = "/angular/js/tinymce/";
        } else {
            // Published (dist)
            url = soeConfig.baseUrl + "langs/";
        }

        var lang: string = CoreUtility.language.replace("-", "_") + ".js";

        url += lang;

        return url;
    }

    public static getTinyMCELanguage(): string {
        return CoreUtility.language.replace("-", "_");
    }
}