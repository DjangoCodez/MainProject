import { RightMenuControllerBase, RightMenuType } from "../RightMenuControllerBase";
import { IUrlHelperService } from "../../Services/UrlHelperService";
import { Feature, SoeModule, TermGroup_Languages } from "../../../Util/CommonEnumerations";
import { SysHelpDTO } from "../../Models/SysHelpDTO";
import { IMessagingService } from "../../Services/MessagingService";
import { ICoreService } from "../../Services/CoreService";
import { ILazyLoadService } from "../../Services/LazyLoadService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { EditDialogController } from "./EditDialogController";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { Constants } from "../../../Util/Constants";

export class HelpMenuContentFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService, $compile): ng.IDirective {
        return {
            template: "<div></div>",
            replace: true,
            scope: {
                content: "=",
                link: "&"
            },
            restrict: 'E',
            link: (scope: any, element) => {
                scope.linkClicked = (id) => scope.link({ feature: id });
                scope.$watch("content", (newValue, oldValue) => {
                    if (newValue && newValue !== oldValue) {
                        element.html(newValue);
                        $compile(element.contents())(scope);
                    }
                });
            }
        };
    }
}

export class HelpMenuDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('/Core/RightMenu/HelpMenu/HelpMenu.html'),
            scope: {
                positionIndex: "@",
                feature: "@",
                soeModule: "@"
            },
            restrict: 'E',
            replace: true,
            controller: HelpMenuController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class HelpMenuController extends RightMenuControllerBase {

    // Init parameters
    feature: Feature;
    soeModule: SoeModule;

    loading: boolean;
    editEnabled: boolean;
    text: string;
    content: SysHelpDTO[] = [];

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        messagingService: IMessagingService,
        private coreService: ICoreService,
        private $uibModal,
        private urlHelperService: IUrlHelperService,
        private $scope: ng.IScope,
        private $sce: ng.ISCEService,
        private $window,
        private lazyLoadService: ILazyLoadService) {
        super($timeout, messagingService, RightMenuType.Help);
    }

    public $onInit() {
        this.setTopPosition();

        this.editEnabled = CoreUtility.isSupportAdmin;

        this.messagingService.subscribe(Constants.EVENT_TOGGLE_HELP_MENU, (data: any) => {
            this.toggleShowMenu();
        });

        this.messagingService.subscribe(Constants.EVENT_SHOW_HELP_MENU, (data: any) => {
            if (!this.showMenu)
                this.toggleShowMenu();

            if (data && data.feature) {
                this.feature = data.feature;
                this.getHelp();
            }
        });
    }

    private getHelp() {
        if (this.content.length === 0) {
            this.coreService.getHelp(this.feature).then((h: SysHelpDTO) => {
                let help = h;
                if (!help) {
                    help = new SysHelpDTO();
                    help.sysFeatureId = this.feature;
                }
                this.content.push(help);
            });
        }
    }

    public linkClicked(feature: Feature) {
        this.loading = true;
        this.coreService.getHelp(feature)
            .then(help => {
                this.content.push(help);
                this.loading = false;
            });
    }

    public back() {
        if (this.content.length > 1)
            this.content.pop();
        this.$scope.$apply();
    }

    public edit() {
        this.lazyLoadService.loadBundle("Soe.Common.HtmlEditor.Bundle").then(() => {
            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Core/RightMenu/HelpMenu/EditDialog.html"),
                controller: EditDialogController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                windowClass: 'modal-wide',
                resolve: {
                    help: () => this.getCurrent()
                }
            });

            modal.result.then(help => {
                let content = this.getCurrent();
                content.text = help.text;
                content.title = help.title;
                content.sysHelpId = help.sysHelpId;
            });
        })
    }

    private getCurrent() {
        return this.content[this.content.length - 1];
    }

    private openCustomerCenter() {
        let lang: string;
        if (CoreUtility.languageId == TermGroup_Languages.Swedish)
            lang = "se";
        else if (CoreUtility.languageId == TermGroup_Languages.Finnish)
            lang = "fi";
        else if (CoreUtility.languageId == TermGroup_Languages.English)
            lang = "en";
        else if (CoreUtility.languageId == TermGroup_Languages.Norwegian)
            lang = "no";
        else if (CoreUtility.languageId == TermGroup_Languages.Danish)
            lang = "dk";
        else
            lang = "se";

        HtmlUtility.openInNewTab(this.$window, "https://online.superoffice.com/cust12308/CS/scripts/customer.fcgi?customerLang=" + lang);
    }

    private openTeamViewer() {
        HtmlUtility.openInNewTab(this.$window, "https://get.teamviewer.com/s1support");
    }

    private openReleaseNotes() {
        HtmlUtility.openInNewTab(this.$window, `/soe/common/rightmenu/helpmenu/releasenotes/?module=${this.soeModule}&feature=${this.feature}`);
    }

    private openFaq() {
        HtmlUtility.openInNewTab(this.$window, `/soe/common/rightmenu/helpmenu/faq/?module=${this.soeModule}&feature=${this.feature}`);
    }
}
