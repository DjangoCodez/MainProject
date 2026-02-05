import 'jquery';
import 'bootstrap3-typeahead';

import { ICellEditorParams } from "./ICellEditorComp";
import { InputCellEditor } from "./InputCellEditor";
import { ElementHelper } from "../ElementHelper";
import { TypeAheadOptionsAg } from "../../SoeGridOptionsAg";

export interface ITypeAheadCellEditorParams {
    typeAheadOptions: TypeAheadOptionsAg;
    field: string;
}

declare type Params = ICellEditorParams & ITypeAheadCellEditorParams;
/*NOTE: As inhereting from InputCellEditor, we need to implement the custom leave (Enter means Tab). There's a couple of ways a user can interact
1: If the typeahead menu is not opened, enter-keyup triggers a custom leave (similar to InputCellEditor).
2: If the typeahead menu is opened, enter-keyup are handled by typeahead and eventually will run afterSelect.
But the keyup event handler doesn't know whether the menu is opened or not and is triggered before afterSelect is.
Both will call tryGotoNextCell to support both scenarios. tryGotoNextCell is wrapped inside a debounce to prevent the method to be run twice.
It is set to run the trailing is execute if called twice, which should be afterSelect.
*/
export class TypeAheadCellEditor extends InputCellEditor {
    private containerElement: HTMLElement;
    private handleKeyEvent: any;
    private stopEnterHandler: any;

    public init?(params: ICellEditorParams): void {
        super.init(params);
        this.containerElement = this.buildTypeAheadContainerElement(params as any);
    }

    // Return the DOM element of your editor, this is what the grid puts into the DOM
    public getGui(): HTMLElement {
        return this.containerElement;
    }

    // Gets called once by grid after editing is finished
    // if your editor needs to do any cleanup, do it here
    public destroy?(): void {
        ($(this.cellElement) as any).typeahead("destroy");
        this.containerElement.removeEventListener("keydown", this.handleKeyEvent);
        this.cellElement.removeEventListener("keyup", this.stopEnterHandler);
        super.destroy();
    }

    public afterGuiAttached?(): void {
        const { delay, minLength, source, displayField, dataField, useScroll } = (this.params as Params).typeAheadOptions;

        const inputElementWrapper = $(this.cellElement) as any;

        //Fix for version 21. of ag-grid
        const wrapperParent = inputElementWrapper.closest(".ag-root-wrapper");
        if (wrapperParent && wrapperParent.length > 0) {
            wrapperParent[0].style.overflow = "visible";
        }

        //Fix for dialog popups...
        const modalParent = inputElementWrapper.closest(".modal-open");
        if ((modalParent && modalParent.length > 0)) {
            const popupEditor = inputElementWrapper.closest(".ag-popup-editor");
            if (popupEditor && popupEditor.length > 0) {
                popupEditor[0].style.zIndex = "10000";
            }
        }

        inputElementWrapper.typeahead({
            source: (q, p) => {
                const result: any[] = source(q);
                p(result);
            },
            minLength,
            delay,
            updater: (i) => i ? i[dataField] : undefined,
            displayText: (i) => i[displayField],
            afterSelect: () => this.tryGotoNextCell(false),
            items: useScroll ? 1000 : 8,
        });
        
        super.afterGuiAttached();
        inputElementWrapper.typeahead("lookup");
    }

    // Gets called once after initialised.
    // If you return true, the editor will appear in a popup
    isPopup?(): boolean {
        return true;
    }

    protected buildTypeAheadContainerElement(params: Params): HTMLElement {
        const inputElement = this.cellElement;
        const { typeAheadOptions, node, eGridCell } = params;
        const { buttonConfig } = typeAheadOptions;

        const containerElement = document.createElement("div");

        containerElement.setAttribute("name", "root");
        containerElement.style.width = eGridCell.style.width;

        this.handleKeyEvent = (e: KeyboardEvent) => {
            if (!this.isEndingEditingKey(e)) {
                return;
            }

            this.stopEnterHandler(e);

            const inputElementWrapper = $(this.cellElement) as any;
            var inputValue = inputElementWrapper.val();

            inputElementWrapper.typeahead("select");
            const current = inputElementWrapper.typeahead("getActive");

            if (current) {
                if (current[typeAheadOptions.displayField].toLowerCase().indexOf(inputValue.toString().toLowerCase()) >= 0) {
                    const { dataField } = (this.params as Params).typeAheadOptions;
                    this.cellElement.value = current[dataField];
                }
                else {
                    current['noExactMatch'] = true;
                    current['typedText'] = inputValue.toString();
                    this.cellElement.value = inputValue.toString();
                }
            }
            else {
                this.cellElement.value = inputValue.toString();
            }

            var shiftKey = (e && e.shiftKey);
            this.tryGotoNextCell(shiftKey);
        };


        this.stopEnterHandler = (e: KeyboardEvent) => {
            if (!this.isEndingEditingKey(e)) {
                return;
            }
            event.stopPropagation();
            event.stopImmediatePropagation();
            if (event.cancelable) event.preventDefault();
        }

        //Need to remove the super class' keyup handler for enter as it alters the typeahead behavior.
        inputElement.removeEventListener("keydown", this.handleEnterKey);
        inputElement.addEventListener("keyup", this.stopEnterHandler);
        containerElement.addEventListener("keydown", this.handleKeyEvent);

        inputElement.setAttribute("id", "typeahead-editor");
        inputElement.setAttribute("autocomplete", "off");
        inputElement.classList.add("form-control");
        inputElement.style.height = node.rowHeight + "px";

        if (buttonConfig) {
            const groupElement = document.createElement("div");
            const groupButtonElement = document.createElement("span");
            const buttonElement = document.createElement("button");

            groupElement.classList.add("input-group");
            groupButtonElement.classList.add("input-group-btn");

            buttonElement.setAttribute("type", "button");
            buttonElement.classList.add("btn");
            buttonElement.classList.add("btn-default");
            buttonElement.classList.add("no-border-radius");
            buttonElement.classList.add("leave-typeahead-alone");
            ElementHelper.appendConcatClasses(buttonElement, buttonConfig.icon);

            buttonElement.addEventListener("click", buttonConfig.click);

            groupButtonElement.appendChild(buttonElement);
            groupElement.appendChild(inputElement);
            groupElement.appendChild(groupButtonElement);
            containerElement.appendChild(groupElement);
        }
        else {
            containerElement.appendChild(inputElement);
        }
        
        return containerElement;
    }

    //Need a debounce to prevent double calls to tabToNextCell. 
    private tryGotoNextCell = _.debounce((shiftKey: boolean) => {
        const { node, column, typeAheadOptions, api, rowIndex } = (this.params as Params);
        const isAllowedToNavigate = typeAheadOptions.allowNavigationFromTypeAhead(this.getValue(), node.data, column.colDef);
        if (isAllowedToNavigate) {
            this.tabToNextCell(shiftKey);
        }
    }, 100, { leading: false, trailing: true });

    private isEndingEditingKey(e: KeyboardEvent) {
        const key = e.which || e.keyCode;
        return key === $.ui.keyCode.ENTER || key === $.ui.keyCode.TAB;
    }
}