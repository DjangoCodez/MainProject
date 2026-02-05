import { SoeEntityType } from '../../Util/CommonEnumerations'
import { IFilesLookupDTO } from "../../Scripts/TypeLite.Net4";
import { ImportFileDTO } from './ImportFileDTO';

export class FilesLookupDTO implements IFilesLookupDTO {
  constructor(
    public entity: SoeEntityType,
    public files: ImportFileDTO[]
  ) { }
}
