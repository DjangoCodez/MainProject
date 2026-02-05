export interface IShortCutService {
    UseTabIndexWhenTabAsEnter: boolean;
    bindShortCut($scope: ng.IScope, shortCut: string, callback: ShortCutCallback);
    bindSave($scope: ng.IScope, saveCallback: ShortCutCallback);
    bindSaveAndClose($scope: ng.IScope, saveCallback: ShortCutCallback);
    bindPrint($scope: ng.IScope, saveCallback: ShortCutCallback);
    bindNext($scope: ng.IScope, saveCallback: ShortCutCallback);
    bindNew($scope: ng.IScope, saveCallback: ShortCutCallback);
    bindEnterAsTab($scope: ng.IScope);
    bindEnterCloseDialog($scope: ng.IScope, saveCallback: ShortCutCallback);
}

export type ShortCutCallback = ((event, hotkey) => void) | ((event, hotkey) => boolean);

export class ShortCutService implements IShortCutService {

    public EndOfTabsWithIndex = false;
    public UseTabIndexWhenTabAsEnter = false;

    //@ngInject
    constructor(private hotkeys: any) {
        
    }

    public bindShortCut($scope: ng.IScope, shortCut: string, callback: ShortCutCallback) {
        this.hotkeys.bindTo($scope)
            .add({
                combo: shortCut,
                callback: (event, hotkey) => {
                    if (!callback(event, hotkey)) { event.preventDefault(); }; },
                allowIn: ['INPUT', 'SELECT', 'TEXTAREA']
            })
    }

    public bindSave($scope: ng.IScope, saveCallback: ShortCutCallback) {
        this.bindShortCut($scope, "ctrl+s", saveCallback)
    }

    public bindSaveAndClose($scope: ng.IScope, saveCallback: ShortCutCallback) {
        this.bindShortCut($scope, "ctrl+enter", saveCallback)
    }

    public bindPrint($scope: ng.IScope, saveCallback: ShortCutCallback) {
        this.bindShortCut($scope, "ctrl+p", saveCallback)
    }

    public bindNext($scope: ng.IScope, saveCallback: ShortCutCallback) {
        this.bindShortCut($scope, "ctrl+h", saveCallback)
    }

    public bindNew($scope: ng.IScope, saveCallback: ShortCutCallback) {
        this.bindShortCut($scope, "ctrl+r", saveCallback)
    }

    public bindEnterAsTab($scope: ng.IScope) {
        this.bindShortCut($scope, "enter", (event, hotkey) => this.tabWithEnter(event, hotkey))
    }

    public bindEnterCloseDialog($scope: ng.IScope, saveCallback: ShortCutCallback) {
        this.bindShortCut($scope, "enter", saveCallback)
    }

    private tabWithEnter(event, hotkey) {
        const hasClass = function (elem, className) {
            return _.includes(elem.classList, className)
        }

        const isReadOnly = function (elem) {
            return $(elem).prop('readonly');
        }

        const isButton = function (elem: any, allowWhenTabIndex: boolean, allowMainButton: boolean): boolean {
            if (elem.type === "button") {
                if (allowMainButton) {
                    //var isMainProp = $(elem).attr('main-button')
                    //console.log("isMainProp", isMainProp);
                    //return !(isMainProp && isMainProp === 'true');
                    return !hasClass(elem, "ngSoeMainButton");
                }
                else {
                    return (allowWhenTabIndex) ? (!(elem.tabIndex > 0)) : true;
                }
            }
            return false;
        }

        const isHidden = function (elem: any): boolean {
            return (elem.type === "hidden");
        }

        const hasClosedAccordionParent = function (elem: any): boolean {
            const parentAccordion = elem.closest(".soe-accordion");
            if (parentAccordion) {
                return !hasClass(parentAccordion, 'panel-open'); 
                //var openProp = $(parentAccordion).attr('is-open');
                //console.log("openProp", openProp);
                //return openProp && openProp !== 'true';
            }

            return false;
        };

        const getNextElement = function (elem: any, shortCutService: ShortCutService):any {

            if (shortCutService.EndOfTabsWithIndex && elem.tabIndex === 1) {
                shortCutService.EndOfTabsWithIndex = false;
            }

            if (shortCutService.UseTabIndexWhenTabAsEnter && !shortCutService.EndOfTabsWithIndex) {
                let next = $(":tabbable[tabindex=" + (elem.tabIndex + 1) + "]");
                if (!shortCutService.EndOfTabsWithIndex && (next && next.length > 0)) {
                    return next[0];
                }
                else {
                    shortCutService.EndOfTabsWithIndex = true;
                    return $(":input")[$(":input").index(elem) + 1];
                }
            }
            else {
                if ($(elem).is('textarea')) {
                    if (elem.value) {
                        // If typing text, enter will add new line
                        //elem.value += '\r\n';
                        return undefined;
                    } else {
                        // If no text, use enter as tab (increase by 2 to skip shadow-textarea)
                        return $(":input")[$(":input").index(elem) + 2];
                    }
                } else {
                    let next = $(":input")[$(":input").index(elem) + 1];
                    return $(":input")[$(":input").index(elem) + 1];
                }
            }
        }

        var elem = event.srcElement;

        //button should execute its normal function
        if ( isButton(elem,false,false) )
        {
            return true;
        }

        var next = getNextElement(elem, this);
        if (next === undefined) {
            return true;
        }

        //console.log("next.tabIndex", next.tabIndex, next.disabled );

        // Skip some elements
        let i = 0;
        while (i < 150 && (next.tabIndex < 0 || next.disabled || isReadOnly(next) ||
            isButton(next, this.UseTabIndexWhenTabAsEnter, !this.UseTabIndexWhenTabAsEnter) ||
            hasClass(next, 'skip-tab-with-enter') ||
            isHidden(next) ||
            hasClosedAccordionParent(next)
            ))
        {
            next = getNextElement(next, this);
            i++;    // Just to prevent infinite loop
        }

        if (next) {
            next.focus();
        }
    }
   
}
