import { ITooltipComp } from '@ag-grid-community/core/dist/cjs/main';

class ProductImageTooltipComponent {
    params: any;
    eGui: HTMLDivElement;
    imageHeight = 200;
    imageWidth = 200;
    constructor(parent, event, params) {
        this.params = params;
        this.createAndAppendElement(parent);
        this.positionElement(event);
    }

    createAndAppendElement(parent) {
        const parentDiv = document.createElement('div');
        parentDiv.classList.add('ag-theme-belham');
        parentDiv.classList.add('ag-popup');
        parentDiv.classList.add('product-image-tooltip');

        if (this.params.data.externalId) {
            parentDiv.style.display = "none";
            const img = document.createElement('img');
            const show = this.show.bind(this);
            img.src = `https://www.rskdatabasen.se/thumb/w/id/${this.params.data.externalId}/BILD/${this.imageHeight}/${this.imageWidth}`;
            img.addEventListener('load', (event) => {
                show();
            })
            parentDiv.appendChild(img);
        }
        else {
            const div = document.createElement('div');
            div.innerHTML = this.params.data.name;
            parentDiv.appendChild(div);
        }

        this.eGui = parentDiv;
        parent.appendChild(this.eGui);
    }

    show() {
        this.eGui.style.display = 'block';
    }

    positionElement(event) {
        const gridCell = this.params.eGridCell;
        if (gridCell) {
            const { x, y, width, height } = gridCell.getBoundingClientRect();
            this.eGui.style.left = `${x + width - 5}px`;
            this.eGui.style.top = `${y - this.imageHeight - 5}px`;
        } else {
            this.eGui.style.left = `${event.x}px`;
            this.eGui.style.top = `${event.y}px`;
        }
    }

    destroy() {
        this.params = null;
        this.eGui.parentElement.removeChild(this.eGui);
    }
}

export class ProductImageCellRenderer implements ITooltipComp {
    params: any;
    eGui: HTMLDivElement;
    component: any;
    init(params) {
        this.params = params;
        const { data } = params; 
        this.eGui = document.createElement('div');
        if (params.data.type === 2 && data.imageUrl && data.imageUrl.length > 0) {
            this.eGui.innerHTML = `<div><span><img style="height: 40px; width: 40px" src="https://www.rskdatabasen.se/thumb/w/id/${data.externalId }/BILD/100/100" /></span></div>`;
        } else {
            this.eGui.innerHTML = '<div></div>';
        }

        this.addListeners();
    }

    addListeners() {
        this.onMouseEnter = this.onMouseEnter.bind(this);
        this.onMouseOut = this.onMouseOut.bind(this);

        this.eGui.addEventListener('mouseenter', this.onMouseEnter);
        this.eGui.addEventListener('mouseout', this.onMouseOut);
    }

    removeListeners() {
        this.eGui.removeEventListener('mouseenter', this.onMouseEnter);
        this.eGui.removeEventListener('mouseout', this.onMouseOut);
    }

    onMouseEnter(e) {
        if (this.component) { return; }

        this.component = this.renderImageViewerComponent(e);
    }

    onMouseOut(e) {
        this.removeImageViewerComponent();
    }

    renderImageViewerComponent(e) {
        const ivc = new ProductImageTooltipComponent(document.body, e, this.params);

        return ivc;
    }

    removeImageViewerComponent() {
        if (this.component) {
            this.component.destroy();
            this.component = null;
        }
    }

    getGui() {
        return this.eGui;
    }

    destroy() {
        this.removeListeners();
        this.removeImageViewerComponent();
    }
}