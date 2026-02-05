import { Injectable } from '@angular/core';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';
import { SoeRecalculateTimeHeadAction } from '@shared/models/generated-interfaces/Enumerations';
import { IRecalculateTimeHeadDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  cancelRecalculateTimeHead,
  cancelRecalculateTimeRecord,
  getRecalculateTimeHead,
  getRecalculateTimeHeadId,
  getRecalculateTimeHeads,
  setRecalculateTimeHeadToProcessed,
} from '@shared/services/generated-service-endpoints/time/RecalculateTime.endpoints';
import { Guid } from '@shared/util/string-util';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PlacementsRecalculateStatusDialogService {
  constructor(private http: SoeHttpClient) {}

  private defaultAdditionalProps = {
    recalculateAction: SoeRecalculateTimeHeadAction.Placement,
    loadRecords: true,
    showHistory: false,
    setExtensionNames: true,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      recalculateAction?: SoeRecalculateTimeHeadAction;
      loadRecords?: boolean;
      showHistory?: boolean;
      setExtensionNames?: boolean;
      dateFrom?: Date;
      dateTo?: Date;
      limitNbrOfHeads?: number;
    }
  ): Observable<IRecalculateTimeHeadDTO[]> {
    const recalculateAction =
      additionalProps?.recalculateAction ??
      this.defaultAdditionalProps.recalculateAction;
    const loadRecords =
      additionalProps?.loadRecords ?? this.defaultAdditionalProps.loadRecords;
    const showHistory =
      additionalProps?.showHistory ?? this.defaultAdditionalProps.showHistory;
    const setExtensionNames =
      additionalProps?.setExtensionNames ??
      this.defaultAdditionalProps.setExtensionNames;

    const limitNbrOfHeads = additionalProps?.limitNbrOfHeads ?? undefined;

    let dateFromString: string = '';
    if (additionalProps?.dateFrom)
      dateFromString = additionalProps.dateFrom.toDateTimeString();

    let dateToString = '';
    if (additionalProps?.dateTo)
      dateToString = additionalProps.dateTo.toDateTimeString();

    return this.http.get<IRecalculateTimeHeadDTO[]>(
      getRecalculateTimeHeads(
        recalculateAction,
        loadRecords,
        showHistory,
        setExtensionNames,
        dateFromString,
        dateToString,
        limitNbrOfHeads
      )
    );
  }

  getRecalculateTimeHeadId(key: Guid): Observable<number> {
    return this.http.get<number>(getRecalculateTimeHeadId(key.toString()));
  }

  getRecalculateTimeHead(
    recalculateTimeHeadId: number,
    loadRecords: boolean,
    setExtensionNames: boolean
  ): Observable<IRecalculateTimeHeadDTO> {
    return this.http.get<IRecalculateTimeHeadDTO>(
      getRecalculateTimeHead(
        recalculateTimeHeadId,
        loadRecords,
        setExtensionNames
      )
    );
  }

  setRecalculateTimeHeadToProcessed(
    recalculateTimeHeadId: number
  ): Observable<IActionResult> {
    return this.http.post<IActionResult>(setRecalculateTimeHeadToProcessed(), {
      id: recalculateTimeHeadId,
    });
  }

  cancelRecalculateTimeHead(
    recalculateTimeHeadId: number
  ): Observable<IActionResult> {
    return this.http.delete<IActionResult>(
      cancelRecalculateTimeHead(recalculateTimeHeadId)
    );
  }

  cancelRecalculateTimeRecord(
    recalculateTimeRecordId: number
  ): Observable<IActionResult> {
    return this.http.delete<IActionResult>(
      cancelRecalculateTimeRecord(recalculateTimeRecordId)
    );
  }
}
