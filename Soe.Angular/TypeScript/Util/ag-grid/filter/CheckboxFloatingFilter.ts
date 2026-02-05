import { IFloatingFilterComp, IFloatingFilterParams } from "./IFloatingFilterComp";

export class CheckboxFloatingFilter implements IFloatingFilterComp {
    private checkStates = {
        checked: "0",
        unchecked: "1",
        indeterminate: "2"
    };

    private containerElement: HTMLElement;
    private checkboxElement: HTMLInputElement;
    private params: IFloatingFilterParams;

    public init(params: IFloatingFilterParams): void {
        this.params = params
        this.containerElement = document.createElement("div");
        this.checkboxElement = document.createElement("input");
        this.checkboxElement.setAttribute("type", "checkbox");
        this.checkboxElement.style.width = "13px";
        this.checkboxElement.style.height = "13px";
        this.checkboxElement.addEventListener("click", this.onCheckboxChanged);

        this.containerElement.appendChild(this.checkboxElement);
        this.containerElement.style.textAlign = "center";
        
        this.resetCheckbox(params['setChecked']);
    }

    public onParentModelChanged(parentModel: any) {
        if (_.isNil(parentModel) || parentModel.values.length != 1) {
            this.resetCheckbox();
            return;
        }

        this.checkboxElement.checked = parentModel.values[0] === "true";
        this.checkboxElement.indeterminate = false;
        this.checkboxElement.readOnly = false;
    }

    public getGui(): HTMLElement {
        return this.containerElement;
    }

    public destroy?(): void {
        this.checkboxElement.removeEventListener("click", this.onCheckboxChanged);
    }

    private resetCheckbox(setChecked?: boolean) {
        if (setChecked) {
            this.checkboxElement.checked = true;
            this.checkboxElement.readOnly = false;
            this.checkboxElement.indeterminate = false;
        }
        else {
            this.checkboxElement.checked = false;
            this.checkboxElement.readOnly = true;
            this.checkboxElement.indeterminate = true;
        }
    }

    private onCheckboxChanged = (e: Event) => {
        const target = e.target as HTMLInputElement;
        if (target.readOnly) {
            target.checked = true;
            target.readOnly = false;
            target.indeterminate = false;
        }
        else if (target.checked) {
            target.checked = false;
            target.readOnly = true;
            target.indeterminate = true;
        }
        else {
            target.checked = false;
            target.readOnly = false;
            target.indeterminate = false;
        }
        
        this.params.parentFilterInstance((instance:any) => {
            var model = {
                type: 'set',
                values: target.indeterminate ? null : [target.checked.toString()]
            };
            instance.setModel(model);
            this.params.api.onFilterChanged();
        });
    };
}