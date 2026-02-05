import { EditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Feature } from "../../../Util/CommonEnumerations";

export class EditController extends EditControllerBase {

    // Data
    grossProfitCode: any;

    // Filter options                
    accountYearFilterOptions: Array<any> = [];
    accountDimFilterOptions: Array<any> = [];
    accountFilterOptions: Array<any> = [];

    // Current selected
    currentAccountYearId: any;
    currentAccountDimId: any;
    currentAccountId: any;

    //@ngInject
    constructor(
        private grossProfitCodeId: number,
        $uibModal,
        coreService: ICoreService,
        private accountingService: IAccountingService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService) {

        super("Economy.Export.Payments.Edit", Feature.Economy_Export_Payments, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);
    }

    // SETUP

    protected setupLookups() {
        this.lookups = 2;
        this.startLoad();

        this.loadAccountYearDict();
    }

    private setupToolBar() {
        this.setupDefaultToolBar();
    }

    // LOOKUPS

    private load() {
    }

    private loadAccountYearDict() {
        this.accountingService.getAccountYearDict(false).then((x) => {
            _.forEach(x, (y: any) => {
                this.accountYearFilterOptions.push({ id: y.id, name: y.name })
            });
            this.accountYearFilterOptions = this.accountYearFilterOptions.reverse(); //reverse, to set the latest accountyear on top       
            this.currentAccountYearId = this.accountYearFilterOptions[0].id; //set default accountyear                      
            this.lookupLoaded();
        });
    }

    private loadAccounts() {

    }

    // EVENTS

    protected lookupLoaded() {
        super.lookupLoaded();
        if (this.lookups <= 0) {
            this.load();
            this.setupToolBar();
        }
    }

    // ACTIONS

    private save() {
        this.startSave();
    }

    protected delete() {
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
    }

    // VALIDATION

    protected validate() {
    }
}