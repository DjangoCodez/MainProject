import { ISysTermDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, PostChange } from "../../Util/CommonEnumerations";

export class SysTermDTO implements ISysTermDTO {
    created: Date;
    createdBy: string;
    langId: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    postChange: PostChange;
    sysTermGroupId: number;
    sysTermId: number;
    translationKey: string;

    //Extensions
    isModified: boolean;
}
