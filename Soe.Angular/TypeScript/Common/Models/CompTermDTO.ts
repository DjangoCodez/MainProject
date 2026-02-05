import { ICompTermDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, CompTermsRecordType, TermGroup_Languages } from "../../Util/CommonEnumerations";


export class CompTermDTO implements ICompTermDTO {
    compTermId: number;
    recordId: number;
    state: SoeEntityState;
    recordType: CompTermsRecordType;
    name: string;
    lang: TermGroup_Languages;
    langName: string;
}
