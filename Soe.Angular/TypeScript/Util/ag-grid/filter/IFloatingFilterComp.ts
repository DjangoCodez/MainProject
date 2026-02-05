export interface IFloatingFilterComp {
    // mandatory methods

    // The init(params) method is called on the floating filter once. See below for details on the parameters.
    init(params: IFloatingFilterParams): void;

    // This is a method that ag-Grid will call every time the model from the associated rich filter
    // for this floating filter changes. Typically this would be used so that you can refresh your UI and show
    // on it a visual representation of the latest model for the filter as it is being updated somewhere else.
    onParentModelChanged(parentModel: any)

    // Returns the dom html element for this floating filter.
    getGui(): HTMLElement;

    // optional methods

    // Gets called when the floating filter is destroyed.
    // Like column headers, the floating filter life span is only when the column is visible,
    // so gets destroyed if column is made not visible or when user scrolls column out of
    // view with horizontal scrolling.
    destroy?(): void;
}

export interface IFloatingFilterParams {
     // The column this filter is for
    column: any;

    // The params object passed to the filter. This is to allow the
    // floating filter access to the configuration of the parent filter.
    // For example, the provided filters use debounceMs from the parent
    // filter params.
    filterParams: any,

    // This is a shortcut to invoke getModel on the parent parent filter.
    // If the parent filter doesn't exist (filters are lazy created as needed)
    // then returns null rather than calling getModel() on the parent filter.
    currentParentModel(): any;

    // Boolean flag to indicate if the button in the floating filter that
    // opens the parent filter in a popup should be displayed
    suppressFilterButton: boolean;

    // Gets a reference to the parent filter. The result is returned returned
    // async via a callback as the parent filter may not exist yet. If it does
    // not exist, it is created and asynchronously returned (ag-Grid itself
    // does not create component asynchronously, however if providing a framework
    // provided filter eg React, this might be).
    //
    // The floating filter can then call any method it likes on the parent filter.
    // The parent filter will typically provide it's own method for the floating
    // filter to call to set the filter. Eg if creating customer filter A, then
    // it should have a method your floating floating A can call to set the state
    // when the user updates via the floating filter.
    parentFilterInstance: (callback: (filterInstance: IFloatingFilterComp) => void) => void;

    // The grid API
    api: any;
}