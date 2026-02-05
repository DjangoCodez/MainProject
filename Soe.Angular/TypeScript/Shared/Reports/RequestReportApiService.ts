import { IHttpService } from "../../Core/Services/httpservice";
import { IMessagingService } from "../../Core/Services/MessagingService";
import { INotificationService } from "../../Core/Services/NotificationService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IDownloadFileDTO, IReportPrintDTO } from "../../Scripts/TypeLite.Net4";
import { Constants } from "../../Util/Constants";
import {
  SOEMessageBoxButtons,
  SOEMessageBoxImage,
} from "../../Util/Enumerations";
import { ExportUtility } from "../../Util/ExportUtility";

export interface IRequestReportApiService {
  // GET
  get(url: string, queue?: boolean): ng.IPromise<IDownloadFileDTO>;

  // POST
  post(
    url: string,
    value: IReportPrintDTO,
    queue?: boolean,
    download?: boolean
  ): ng.IPromise<IDownloadFileDTO>;
}

export class RequestReportApiService implements IRequestReportApiService {
  //@ngInject
  constructor(
    private readonly httpService: IHttpService,
    private readonly messagingService: IMessagingService,
    private readonly notificationService: INotificationService,
    private readonly translationService: ITranslationService
  ) {}

  get(url: string, queue: boolean = true): ng.IPromise<IDownloadFileDTO> {
    url += `?queue=${queue}`;

    return this.httpService
      .get(url, false)
      .then((response: IDownloadFileDTO) => {
        this.handleResponse(response, queue);
        return response;
      });
  }

  // POST

  post(
    url: string,
    value: IReportPrintDTO,
    queue: boolean = true,
    download: boolean = false
  ): ng.IPromise<IDownloadFileDTO> {
    value.queue = queue;
    return this.httpService
      .post(url, value)
      .then((response: IDownloadFileDTO) => {
        this.handleResponse(response, queue, download);
        return response;
      });
  }

  private handleResponse(
    fileDataResult: IDownloadFileDTO,
      queue: boolean,
      download: boolean = false
  ): void {
    if (
      fileDataResult.success &&
      fileDataResult.content &&
      fileDataResult.fileName
    ) {
        if (download) {
            ExportUtility.OpenFile(
                fileDataResult.content,
                fileDataResult.fileName,
                fileDataResult.fileType
            );
        }
        else { 
            ExportUtility.DownloadFile(
                fileDataResult.content,
                fileDataResult.fileName,
                fileDataResult.fileType
            );
        }
        return;
    } else if (fileDataResult.success && queue) {
      this.messagingService.publish(Constants.EVENT_SHOW_REPORT_MENU, {});
    } else if (!fileDataResult.success && fileDataResult.errorMessage) {
      const keys: string[] = ["core.error"];

      this.translationService.translateMany(keys).then((terms) => {
        this.notificationService.showDialog(
          terms["core.error"],
          fileDataResult.errorMessage,
          SOEMessageBoxImage.Error,
          SOEMessageBoxButtons.OK
        );
      });
    }
  }
}
