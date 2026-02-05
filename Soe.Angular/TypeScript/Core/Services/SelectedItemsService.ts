import {ISoeCellValueChanged } from "../../Util/SoeGridOptionsAg";

export interface ISelectedItemsService {
    setup($scope: ng.IScope, idField:string, saveCallback: (items: number[]) => void);
    CellChanged: (params: ISoeCellValueChanged) => void;
    SelectedItemsExist: () => boolean;
    Save: () => void;
}

export class SelectedItemsService implements ISelectedItemsService {
    private localSaveCallback: (items: number[]) => void;
    private selectedItems: number[] = [];
    private localIdField: string;
    private $scope: ng.IScope;
    //@ngInject
    constructor() {
    }

    public setup($scope: ng.IScope, idField: string, saveCallback: (items: number[]) => void) {
        this.localSaveCallback = saveCallback;
        this.localIdField = idField;
        this.$scope = $scope;
    }
    
    public CellChanged(params: ISoeCellValueChanged) {
        this.selectItem(params.data[this.localIdField]);
        this.$scope.$applyAsync();
    }

    public SelectedItemsExist(): boolean {
        return (this.selectedItems.length !== 0);
    }

    public Save() {
        this.localSaveCallback(this.selectedItems)
        this.selectedItems = [];
    }

    private selectItem(id: number) {
        // If item exists, remove it (it has been clicked twice and returned to original state),
        // otherwise add it.
        if (_.includes(this.selectedItems, id)) {
            this.selectedItems.splice(this.selectedItems.indexOf(id), 1);
        } else {
            this.selectedItems.push(id);
        }
    }
}