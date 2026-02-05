import { IconLibrary } from "./Enumerations";

export class ToolBarUtility {
    static createGroup(button?: ToolBarButton): ToolBarButtonGroup {
        var group = new ToolBarButtonGroup();
        if (button)
            group.buttons.push(button);

        return group;
    }

    static createSortGroup(sortFirst = () => { }, sortUp = () => { }, sortDown = () => { }, sortLast = () => { }, disabled = () => { }, hidden = () => { }): ToolBarButtonGroup {
        var group = new ToolBarButtonGroup();
        group.buttons.push(new ToolBarButton("", "core.sortfirst", IconLibrary.FontAwesome, "fa-angle-double-up far", sortFirst, disabled, hidden));
        group.buttons.push(new ToolBarButton("", "core.sortup", IconLibrary.FontAwesome, "fa-angle-up far", sortUp, disabled, hidden));
        group.buttons.push(new ToolBarButton("", "core.sortdown", IconLibrary.FontAwesome, "fa-angle-down far", sortDown, disabled, hidden));
        group.buttons.push(new ToolBarButton("", "core.sortlast", IconLibrary.FontAwesome, "fa-angle-double-down far", sortLast, disabled, hidden));
        return group;
    }

    static createNavigationGroup(navigateFirst = () => { }, navigateLeft = () => { }, navigateRight = () => { }, navigateLast = () => { }, disabled = () => { }, hidden = () => { }): ToolBarButtonGroup {
        var group = new ToolBarButtonGroup();
        group.buttons.push(new ToolBarButton("", "core.navigatefirst", IconLibrary.FontAwesome, "fa-angle-double-left far", navigateFirst, disabled, hidden));
        group.buttons.push(new ToolBarButton("", "core.navigateleft", IconLibrary.FontAwesome, "fa-angle-left far", navigateLeft, disabled, hidden));
        group.buttons.push(new ToolBarButton("", "core.navigateright", IconLibrary.FontAwesome, "fa-angle-right far", navigateRight, disabled, hidden));
        group.buttons.push(new ToolBarButton("", "core.navigatelast", IconLibrary.FontAwesome, "fa-angle-double-right far", navigateLast, disabled, hidden));
        return group;
    }

    static createHelpButton(click = () => { }, disabled = () => { }, hidden = () => { }): ToolBarButton {
        return new ToolBarButton("", "core.help", IconLibrary.FontAwesome, "fa-question", click, disabled, hidden);
    }

    static createClearFiltersButton(click = () => { }, disabled = () => { }, hidden = () => { }): ToolBarButton {
        return new ToolBarButton("", "core.uigrid.gridmenu.clear_all_filters", IconLibrary.FontAwesome, "fa-filter-slash", click, disabled, hidden);
    }

    static createReloadDataButton(click = () => { }, disabled = () => { }, hidden = () => { }): ToolBarButton {
        return new ToolBarButton("", "core.reload_data", IconLibrary.FontAwesome, "fa-sync", click, disabled, hidden);
    }

    static createSaveButton(click = () => { }, disabled = () => { }, hidden = () => { }): ToolBarButton {
        return new ToolBarButton("", "core.save", IconLibrary.FontAwesome, "fa-save", click, disabled, hidden);
    }

    static createCopyButton(click = () => { }, disabled = () => { }, hidden = () => { }): ToolBarButton {
        return new ToolBarButton("", "core.copy", IconLibrary.FontAwesome, "fa-clone", click, disabled, hidden);
    }
}

export class ToolBarButtonGroup {
    public buttons = new Array<ToolBarButton>();

    public deleteButton(idString: string) {
        this.buttons = this.buttons.filter(button => button.idString !== idString);
    }
}

export class ToolBarButton {

    public library: string;
    public idString: string;

    constructor(
        public labelKey: string,
        public titleKey: string,
        public iconLibrary: IconLibrary,
        public iconClass: string,
        public click = () => { },
        public disabled = () => { },
        public hidden = () => { },
        public options?: ToolBarButtonOptions) {

        if (iconLibrary == IconLibrary.Glyphicon) {
            this.library = "glyphicon";
        } else if (iconLibrary == IconLibrary.FontAwesome) {
            this.library = "fal";
        }

        if (!this.options)
            this.options = {};

        if (!this.options.buttonClass)
            this.options.buttonClass = 'btn-default';

    }
}
export class ToolBarButtonOptions {
    buttonClass?: string;
    labelValue?: string;
    newTabLink?: string;
}