import { CoreUtility } from "../CoreUtility";
import { ISoeGridOptionsAg, MenuItem } from "../SoeGridOptionsAg";
import { ICoreService } from "../../Core/Services/CoreService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { INotificationService } from "../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../Enumerations";

export interface IMenuItem {
    name: string, // name of menu item
    disabled?: boolean, // if item should be enabled / disabled
    shortcut?: string, // shortcut (just display text, saying the shortcut here does nothing)
    action?: () => void, // function that gets executed when item is chosen
    checked?: boolean, // set to true to provide a check beside the option
    icon?: HTMLElement | string, // the icon to display beside the icon, either a DOM element or HTML string
    subMenu?: IMenuItem[] // if this menu is a sub menu, contains a list of sub menu item definitions
}

export class GridMenuBuilder {
    constructor(
        private soeGridOptions: ISoeGridOptionsAg,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private notificationService: INotificationService) {

    }

    buildDefaultMenu(addPdfExportOption = false) {

        const keys: string[] = [
            "core.uigrid.gridmenu.clear_all_filters",
            "core.uigrid.export",
            "core.uigrid.gridmenu.export_all_excel",
            "core.uigrid.gridmenu.export_all_csv",
            "core.uigrid.gridmenu.export_filtered_excel",
            "core.uigrid.gridmenu.export_filtered_csv",
            "core.uigrid.gridmenu.reset_columns",
            "core.aggrid.sizecolumntofit",
            "core.uigrid.gridstate",
            "core.uigrid.savedefaultstate",
            "core.uigrid.deletedefaultstate",
            "core.uigrid.savestate",
            "core.uigrid.defaultstate",
            "core.uigrid.deletestate"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.gridmenu.clear_all_filters"], icon: "<span class='fal fa-filter-slash' style='width: 100%; text-align: center; margin-top: 4px;' />", action: () => this.soeGridOptions.clearFilters() });
            this.soeGridOptions.addGridMenuItem("separator");
            this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.export"] + ":", disabled: true });
            this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.gridmenu.export_all_excel"], icon: "<span class='fal fa-file-excel' style='width: 100%; text-align: center; margin-top: 4px;' />", action: () => this.soeGridOptions.exportRows("excel", true) });
            this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.gridmenu.export_filtered_excel"], icon: "<span class='fal fa-file-excel' style='width: 100%; text-align: center; margin-top: 4px;' />", action: () => this.soeGridOptions.exportRows("excel") });
            this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.gridmenu.export_all_csv"], icon: "<span class='fal fa-file-csv' style='width: 100%; text-align: center; margin-top: 4px;' />", action: () => this.soeGridOptions.exportRows("csv", true) });
            this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.gridmenu.export_filtered_csv"], icon: "<span class='fal fa-file-csv' style='width: 100%; text-align: center; margin-top: 4px;' />", action: () => this.soeGridOptions.exportRows("csv") });

            if (addPdfExportOption) {
                this.soeGridOptions.addGridMenuItem({ name: "Exportera all data som Pdf", icon: "<span class='fal fa-file-pdf' style='width: 100%; text-align: center; margin-top: 4px;' />", action: () => this.soeGridOptions.exportRows("pdf") });
            }

            this.soeGridOptions.addGridMenuItem("separator");
            this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.gridstate"] + ":", disabled: true });
            if (CoreUtility.isSupportAdmin) {
                this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.savedefaultstate"], icon: "<span class='fal fa-save' style='width: 100%; text-align: center; margin-top: 4px;' />", action: () => { this.saveDefaultState() } });
                this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.deletedefaultstate"], icon: "<span class='fal fa-times iconDelete' style='width: 100%; text-align: center; margin-top: 4px;' />", action: () => { this.deleteDefaultState() } });
            }
            this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.savestate"], icon: "<span class='fal fa-save' style='width: 100%; text-align: center; margin-top: 4px;' />", action: () => this.soeGridOptions.saveState((name, data) => this.coreService.saveUserGridState(name, data)).then(() => this.soeGridOptions.restoreState((name) => this.coreService.getUserGridState(name), false)) });
            this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.defaultstate"], icon: "<span class='fal fa-columns' style='width: 100%; text-align: center; margin-top: 4px;' />", action: () => this.soeGridOptions.restoreDefaultState((name) => this.coreService.getSysGridState(name)) });
            this.soeGridOptions.addGridMenuItem({ name: terms["core.uigrid.deletestate"], icon: "<span class='fal fa-undo iconDelete' style='width: 100%; text-align: center; margin-top: 2px;' />", action: () => this.soeGridOptions.deleteState((name) => this.coreService.deleteUserGridState(name), (name) => this.coreService.getSysGridState(name)) });

            this.soeGridOptions.customCreateDefaultColumnMenu = (agGridDefaultItems?: string[]): MenuItem[] => {
                const defaultColumnsToKeep = {
                    "pinSubMenu": true,
                    "autoSizeAll": true
                };
                return _.concat((agGridDefaultItems || []).filter(s => defaultColumnsToKeep[s]) as MenuItem[], [
                    { name: terms["core.aggrid.sizecolumntofit"] || "Size columns to fit", action: () => this.soeGridOptions.sizeColumnToFit() } as IMenuItem
                ]);
            };
        });
    }

    protected saveDefaultState() {
        const keys: string[] = [
            "core.warning",
            "core.uigrid.savedefaultstatewarning",
            "core.enterpassword",
            "core.wrongpassword"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialogEx(terms["core.warning"], terms["core.uigrid.savedefaultstatewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, { showTextBox: true, textBoxLabel: terms["core.enterpassword"] });
            modal.result.then(result => {
                if (result.result) {
                    if (result.textBoxValue === 'Fiskpinne36!') {
                        this.soeGridOptions.saveDefaultState((name, data) => this.coreService.saveSysGridState(name, data));
                    } else {
                        this.notificationService.showDialogEx(terms["core.warning"], terms["core.wrongpassword"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                    }
                }
            });
        });
    };

    protected deleteDefaultState() {
        const keys: string[] = [
            "core.warning",
            "core.uigrid.deletedefaultstatewarning",
            "core.enterpassword",
            "core.wrongpassword"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialogEx(terms["core.warning"], terms["core.uigrid.deletedefaultstatewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, { showTextBox: true, textBoxLabel: terms["core.enterpassword"] });
            modal.result.then(val => {
                modal.result.then(result => {
                    if (result.result) {
                        if (result.textBoxValue === 'Fiskpinne36!') {
                            this.soeGridOptions.deleteDefaultState((name) => this.coreService.deleteSysGridState(name));
                        } else {
                            this.notificationService.showDialogEx(terms["core.warning"], terms["core.wrongpassword"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                        }
                    }
                });
            });
        });
    };
}