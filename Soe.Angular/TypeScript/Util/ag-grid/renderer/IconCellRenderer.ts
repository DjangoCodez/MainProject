import { ICellRendererParams, ICellRendererComp } from "./ICellRendererComp";
import { FieldOrPredicate, DataCallback } from "../../SoeGridOptionsAg";
import { ElementHelper } from "../ElementHelper";

//TODO: Fix icon callback, is using angular.
export interface IIconCellRendererParams {
    icon?: string;
    toolTip?: string;
    showIcon?: FieldOrPredicate;
    noPointer?: boolean;
    isSubgrid: boolean;
    onClick?: DataCallback;
    getNodeOnClick: boolean;
}

declare type Params = ICellRendererParams & IIconCellRendererParams;

export class IconCellRenderer implements ICellRendererComp {
    private cellElement: HTMLElement;

    public init(params: any): void {
        this.cellElement = this.buildGuiElement(params);
    }

    public getGui(): HTMLElement {
        return this.cellElement;
    }

    public refresh(params: any): boolean {
        return false;
    }

    private buildGuiElement(params: Params) {
        const { icon, isSubgrid } = params;
        const icons = icon ? icon.split("|") : [];

        if (icons.length > 1) {
            return this.buildForMultipleIcons(params, icons);
        }

        return this.buildForSingleIcon(params);
    }

    private buildForSingleIcon(params: Params) {
        const { icon, value, noPointer } = params;
        let el = document.createElement('button');

        el.classList.add('gridCellIcon');
        if (noPointer)
            el.classList.add('nolink');

        ElementHelper.appendConcatClasses(el, icon ? icon : value);

        this.tryApplyTooltip(el, params);
        this.tryApplyShowIcon(el, params);
        this.tryApplyClickEvent(el, params);

        return el;
    }

    private buildForMultipleIcons(params: Params, icons: string[]) {
        const { noPointer } = params;
        let el = document.createElement('button');

        el.classList.add('gridCellIcon');
        if (noPointer)
            el.classList.add('nolink');
        //el.classList.add('stacked');

        this.tryApplyTooltip(el, params);
        this.tryApplyShowIcon(el, params);
        this.tryApplyClickEvent(el, params);

        let eIcons = document.createElement('span');
        eIcons.classList.add('stacked-container', 'fa', 'fa-stack');

        icons.forEach((icon, idx) => {
            var eIcon = document.createElement('i');
            eIcon.classList.add("stacked-primary");
            ElementHelper.appendConcatClasses(eIcon, icon);
            if (idx > 0)
                eIcon.classList.add("stacked-secondary");

            eIcons.appendChild(eIcon);
        });

        el.appendChild(eIcons);

        return el;
    }

    private tryApplyTooltip(el: HTMLElement, params: Params): void {
        const { toolTip } = params;

        if (toolTip) {
            el.setAttribute('title', toolTip);
        }
    }

    private tryApplyShowIcon(el: HTMLElement, params: Params): void {
        const showIcon = params.showIcon;
        const data = params.getNodeOnClick ? params.node : params.data


        let shouldShow = (typeof showIcon === "string" ? (data) => data[showIcon] : showIcon) || function () { return true };

        if (!shouldShow(data)) {
            el.classList.add("hidden");
        }
    }

    private tryApplyClickEvent(el: HTMLElement, params: Params): void {
        if (!params.onClick) {
            return;
        }

        el.addEventListener("click", (e: any) => {
            var isKeyDown = (e.clientX === 0 && e.clientY === 0 && e.screenY === 0 && e.screenX === 0);
            if (!isKeyDown) {
                params.onClick(params.getNodeOnClick ? params.node : params.data);
                e.stopPropagation();
            }
        });
    }
}