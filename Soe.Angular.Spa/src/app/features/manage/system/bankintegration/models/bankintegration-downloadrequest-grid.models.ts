import {
  ISoeBankerDownloadFileDTO,
  ISoeBankerDownloadRequestDTO,
} from '@shared/models/generated-interfaces/BankIntegrationDTOs';

export interface ISoeBankerDownloadRequestGridDTO
  extends ISoeBankerDownloadRequestDTO {
  statusMessage: string;

  filesLoaded: boolean;
  downloadFiles: ISoeBankerDownloadFileDTO[];
}
