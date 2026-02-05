import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICommonCustomerService } from "../../Customer/CommonCustomerService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { TermGroup } from "../../../Util/CommonEnumerations";
import { ICoreService } from "../../../Core/Services/CoreService";

export class ShowContactAddressInfoController {

    private contactAddressItems: any[];
    private contactAddressTypes: any[];
    //@ngInject
    constructor(
        private $uibModalInstance,
        private translationService: ITranslationService,
        private commonCustomerService: ICommonCustomerService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private contactPersonId: number,
    ) {
        
    }

    private $onInit() {
        this.loadContactAddressTypes().then(() => {
            this.loadContactInfo();
        } )
    }

    private loadContactInfo() {

        this.commonCustomerService.getCustomerContactAddressDict(this.contactPersonId).then((x) => {
            this.contactAddressItems = x;
            for (var i = 0; i < this.contactAddressItems.length; i++) {
                var item = this.contactAddressItems[i];
                item.typeName = this.contactAddressTypes.filter((x) => x.id == item.id)[0].name;
            }
        })
    }

    private loadContactAddressTypes(): ng.IPromise<any>  {
        return this.coreService.getTermGroupContent(TermGroup.SysContactEComType, false, false).then(x => {
            this.contactAddressTypes = x;
        });
    }

    buttonOkClick() {
        this.$uibModalInstance.close('ok');
    }



    
}