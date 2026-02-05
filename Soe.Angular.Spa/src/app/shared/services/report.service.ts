import { Injectable } from '@angular/core';
import { SoeHttpClient } from './http.service';
import { Observable } from 'rxjs';
import { saveIntSetting } from './generated-service-endpoints/core/Settings.endpoints';
import {
  getCompanySettingReportId,
  getReports,
  getReportsDict,
  getReportsForTypes,
  getSettingOrStandardReport,
  getStandardReport,
} from './generated-service-endpoints/report/ReportV2.endpoints';

import {
  GetPurchasePrintUrlModel,
  ReportViewDTO,
} from '@shared/components/select-report-dialog/models/select-report-dialog.model';
import {
  getPurchasePrintUrl,
  productListPrintUrl,
  stockInventoryPrintUrl,
} from './generated-service-endpoints/report/ReportPrint.endpoints';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SettingMainType } from '@shared/models/generated-interfaces/Enumerations';
import { IReportDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  constructor(private http: SoeHttpClient) {}

  saveIntSetting(model: any): Observable<any> {
    return this.http.post<any>(saveIntSetting(), model);
  }

  getReportsForType(model: any): Observable<ReportViewDTO[]> {
    return this.http.post(getReportsForTypes(), model);
  }

  getPurchasePrintUrl(model: GetPurchasePrintUrlModel): Observable<string> {
    return this.http.post(getPurchasePrintUrl(), model);
  }

  getProductListReportUrl(
    productIds: number[],
    reportId: number,
    sysReportTemplateTypeId: number
  ): Observable<string> {
    return this.http.post(productListPrintUrl(), {
      productIds: productIds,
      reportId: reportId,
      sysReportTemplateTypeId: sysReportTemplateTypeId,
    });
  }

  getStandardReportDTO(
    settingMainType: number,
    settingType: number,
    reportTemplateType: number
  ): Observable<IReportDTO> {
    return this.http.get<IReportDTO>(
      getStandardReport(settingMainType, settingType, reportTemplateType)
    );
  }

  getCompanySettingReportId(
    settingMainType: number,
    settingType: number,
    reportTemplateType: number
  ): Observable<number> {
    return this.http.get<number>(
      getCompanySettingReportId(
        settingMainType,
        settingType,
        reportTemplateType
      )
    );
  }

  getSettingOrStandardReportId(
    settingMainType: SettingMainType,
    settingType: number,
    reportTemplateType: number,
    reportType: number
  ): Observable<number> {
    return this.http.get<number>(
      getSettingOrStandardReport(
        settingMainType,
        settingType,
        reportTemplateType,
        reportType
      )
    );
  }

  getStockInventoryPrintUrl(
    stockInventoryIds: number[],
    reportId: number
  ): Observable<string> {
    return this.http.post(stockInventoryPrintUrl(), {
      stockInventoryIds: stockInventoryIds,
      reportId: reportId,
    });
  }

  getReportsDict(
    sysReportTemplateTypeId: number,
    onlyOriginal: boolean,
    onlyStandard: boolean,
    addEmptyRow: boolean,
    useRole: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getReportsDict(
        sysReportTemplateTypeId,
        onlyOriginal,
        onlyStandard,
        addEmptyRow,
        useRole
      )
    );
  }

  getReports(
    actorCompanyId: number,
    sysReportTemplateTypeId: number,
    onlyOriginal: boolean,
    onlyStandard: boolean,
    addEmptyRow: boolean,
    useRole: boolean
  ) {
    return this.http.get<ISmallGenericType[]>(
      getReports(
        actorCompanyId,
        sysReportTemplateTypeId,
        onlyOriginal,
        onlyStandard,
        addEmptyRow,
        useRole
      )
    );
  }
}
