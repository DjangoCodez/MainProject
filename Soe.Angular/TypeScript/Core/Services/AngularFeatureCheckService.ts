import { Feature } from "../../Util/CommonEnumerations";
import { CoreUtility } from "../../Util/CoreUtility";

export interface IAngularFeatureCheckService {
    shouldUseAngularSpa(feature: Feature): boolean;
}

export class AngularFeatureCheckService implements IAngularFeatureCheckService {
    public shouldUseAngularSpa(feature: Feature): boolean {

        const actorCompanyId = CoreUtility.actorCompanyId;
        const allowedCompanyIds = [3057691, 3057665, 3057716, 29984];

        if (allowedCompanyIds.includes(actorCompanyId) && feature === Feature.Billing_Project_Edit_Budget) {
            return true;
        }
        
        return false;
    }
}