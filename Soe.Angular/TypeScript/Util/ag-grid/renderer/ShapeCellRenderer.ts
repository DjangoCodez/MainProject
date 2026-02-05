import { ICellRendererParams, ICellRendererComp } from "./ICellRendererComp";
import { FieldOrPredicate } from "../../SoeGridOptionsAg";
import { ObjectFieldHelper } from "../../ObjectFieldHelper";
import { Constants } from "../../Constants";

export interface IShapeCellRendererParams {
    color?: string;
    colorField?: string;
    isSelect?: boolean;
    showIcon?: FieldOrPredicate;
    toolTip?: string;
    displayField?: string;
    shape: string;
    useGradient: boolean;
    gradientField?: string;
    ignoreTextInFilter?: boolean;
    width?: number;
    showEmptyIcon?: (data) => string;
}

declare type Params = ICellRendererParams & IShapeCellRendererParams;

export class ShapeCellRenderer implements ICellRendererComp {
    private cellElement: HTMLElement;

    public init?(params: any): void {
        this.cellElement = this.buildGuiElement(params);
    }

    public getGui(): HTMLElement {
        return this.cellElement;
    }

    public destroy?(): void;

    public refresh(params: any): boolean {
        return false;
    }

    private buildGuiElement(params: Params): HTMLElement {
        const { eGridCell, data, toolTip, showIcon, shape, useGradient, gradientField, value, isSelect, color, colorField, ignoreTextInFilter, width, displayField, showEmptyIcon } = params;

        if (toolTip) {
            eGridCell.setAttribute("title", toolTip);
        }

        const cellElement = document.createElement("div");

        if (!data) {
            if (value && params.colDef && params.colDef.showRowGroup) {
                return value;
            }
            else {
                cellElement.style.display = "none";
                return cellElement;
            }
        }

        const isVisible = isSelect ? true : (data[gradientField] ? data[gradientField] : ObjectFieldHelper.IsEvaluatedTrue(data, showIcon));

        if (!isVisible) {
            cellElement.style.display = "none";
        }

        if (isSelect) {
            const showShape: boolean = data[gradientField] || data[colorField];
            const shapeSpan = document.createElement('span');
            if (showShape)
                shapeSpan.innerHTML = ShapeCellRenderer.getShapeTemplate(data, shape, useGradient, gradientField, data[colorField], width);

            cellElement.appendChild(shapeSpan);

            if (!ignoreTextInFilter) {
                const textSpan = document.createElement('span');
                textSpan.setAttribute('style', 'margin-left: ' + (showShape ? '10' : '30') + 'px; vertical-align: top');
                textSpan.innerText = data[displayField];
                cellElement.appendChild(textSpan);
            }
        }
        else {
            let colorToUse = color;
            if (!colorToUse && colorField) {
                colorToUse = data[colorField];
            }
            if (colorToUse || useGradient || !showEmptyIcon) {
                cellElement.innerHTML = ShapeCellRenderer.getShapeTemplate(data, shape, useGradient, gradientField, colorToUse ? colorToUse : value, width);
            }
            else if (showEmptyIcon) {
                const iconToShow = showEmptyIcon(data);
                if (iconToShow) {
                    cellElement.innerHTML = '<span><i class="' + iconToShow + '"></i></span>';
                    cellElement.style.marginLeft = "7px";
                    cellElement.style.display = "";
                }
            }
        }
        return cellElement;
    }

    public static getShapeTemplate(data: any, shape: string, useGradient: boolean, gradientField: string, color: string, width?: number): string {
        if (!width)
            width = 12;

        if (!color)
            color = 'transparent';

        let template = '<svg class="shape-svg" width="{0}">'.format(width.toString());

        //Add gradient
        if (useGradient && data[gradientField]) {
            template += '<defs>' +
                '<linearGradient id="grad1" x1="0%" y1="50%" x2="50%" y2="0%" >' +
                '<stop offset="0%" style="stop-color:rgb(51,255,0);stop-opacity:1" />' +
                '<stop offset="100%" style="stop-color:rgb(255,0,0);stop-opacity:1" />' +
                '</linearGradient></defs>' +
                '<circle class="shape" cx="6" cy="6" r="6" fill="url(#grad1)" />'
        } else {
            switch (shape) {
                case Constants.SHAPE_CIRCLE:
                    template += '<circle class="shape" cx="6" cy="6" r="6"';
                    break;
                case Constants.SHAPE_SQUARE:
                    template += '<rect class="shape" width="{0}" height="12"'.format(width.toString());
                    break;
                case Constants.SHAPE_TRIANGLE_DOWN:
                    template += '<polygon class="shape" points="0,1 12,1 6,11"';
                    break;
                case Constants.SHAPE_TRIANGLE_LEFT:
                    template += '<polygon class="shape" points="11,0 1,6 11,12"';
                    break;
                case Constants.SHAPE_TRIANGLE_RIGHT:
                    template += '<polygon class="shape" points="1,0 11,6 1,12"';
                    break;
                case Constants.SHAPE_TRIANGLE_UP:
                    template += '<polygon class="shape" points="6,1 0,11 12,11"';
                    break;
                default:
                    // Default to square
                    template += '<rect class="shape" width="{0}" height="12"'.format(width.toString());
            }

            template += ' style="fill: ' + color + ';';
            if (!color)
                template += 'display: none;';
            template += '" />';
        }

        template += '</svg>';

        return template;
    }
}