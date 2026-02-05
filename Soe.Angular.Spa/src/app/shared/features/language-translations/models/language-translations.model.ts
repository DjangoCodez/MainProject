import {
  CompTermsRecordType,
  SoeEntityState,
  TermGroup_Languages,
} from '@shared/models/generated-interfaces/Enumerations';

import { ICompTermDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class CompTermDTO implements ICompTermDTO {
  compTermId!: number;
  recordId!: number;
  state: SoeEntityState = 0;
  recordType: CompTermsRecordType = CompTermsRecordType.ExtraField;
  name!: string;
  lang: TermGroup_Languages = TermGroup_Languages.Unknown;
  langName!: string;
  isModified!: boolean;

  constructor(
    compTermId: number,
    recordId: number,
    state: SoeEntityState,
    recordType: CompTermsRecordType,
    name: string,
    lang: TermGroup_Languages,
    langName: string
  ) {
    this.compTermId = compTermId;
    this.recordId = recordId;
    this.state = state;
    this.recordType = recordType;
    this.name = name;
    this.lang = lang;
    this.langName = langName;
  }
}
