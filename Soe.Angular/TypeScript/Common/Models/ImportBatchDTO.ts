import { TermGroup_IOType, TermGroup_IOStatus, TermGroup_IOSource, TermGroup_IOImportHeadType } from "../../Util/CommonEnumerations";

export class ImportBatchDTO {

    recordId: number;
    type: TermGroup_IOType;
    typename: string;
    source: TermGroup_IOSource;
    sourceName: string;
    importHeadType: TermGroup_IOImportHeadType;
    importHeadTypeName: string;
    status: TermGroup_IOStatus;
    statusName: string;
    batchId: string;
    created: Date;            
}
