export interface IUrlHelperServiceProvider {
    setPath(path: string, subPath?: string): void;
    $get(): IUrlHelperService;
}

export class UrlHelperServiceProvider implements ng.IServiceProvider {
    private path: string;
    private subPath: string;

    setPath(path: string, subPath?: string) {
        this.path = path;
        this.subPath = subPath;
    }

    $get() {

        return new UrlHelperService(this.path, this.subPath);
    }
}

export interface IUrlHelperService {
    getUrl(path: string): string;
    getGlobalUrl(path: string): string;
    getCoreTemplateUrl(path: string): string;
    getCoreViewUrl(path: string): string;
    getCoreComponent(path: string): string;
    getCoreDirectiveUrl(mod: string, view: string): string;
    getViewUrl(path: string): string;
    getCommonViewUrl(mod: string, view: string): string;
    getCommonDirectiveUrl(mod: string, view: string): string;
    getDirectiveViewUrl(view: string);
    getWidgetUrl(path: string, view: string);
}

export class UrlHelperService implements IUrlHelperService {

    constructor(private path: string, private subPath?: string) {
        if (this.subPath && this.subPath[0] != '/')
            this.subPath = '/' + this.subPath;

        if (this.subPath && this.subPath[this.subPath.length - 1] != '/')
            this.subPath += '/';

        if (this.path && this.path[this.path.length - 1] == '/')
            this.path = this.path.substr(0, this.path.length - 1);
    }

    getUrl(path: string) {
        return this.path + this.subPath + path;
    }

    getGlobalUrl(path: string) {
        return this.path + '/' + path;
    }

    getCoreTemplateUrl(path: string) {
        return this.path + "/Core/Templates/" + path;
    }

    getCoreViewUrl(path: string) {
        return this.path + "/Core/Views/" + path;
    }

    getCoreComponent(path: string) {
        return this.path + "/Core/Views/Components/" + path;
    }

    getCoreDirectiveUrl(mod: string, view: string) {
        return this.path + "/Core/Directives/" + mod + "/" + view;
    }

    getCommonViewUrl(mod: string, view: string) {
        return this.path + "/Common/" + mod + "/Views/" + view;
    }

    getCommonDirectiveUrl(mod: string, view: string) {
        return this.path + "/Common/Directives/" + mod + "/" + view;
    }

    getViewUrl(view: string) {
        return this.path + this.subPath + "Views/" + view;
    }

    getDirectiveViewUrl(view: string) {
        return this.path + this.subPath + "Directives/" + view;
    }

    getWidgetUrl(path: string, view: string) {
        return this.path + this.subPath + "Widgets/" + path + "/" + view;
    }

}