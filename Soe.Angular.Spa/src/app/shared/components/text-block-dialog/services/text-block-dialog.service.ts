import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import {
  TextBlockModel,
  TextblockDTO,
} from '../models/text-block-dialog.model';
import {
  deleteTextBlock,
  getTextBlock,
  getTextBlocks,
  saveTextBlock,
} from '@shared/services/generated-service-endpoints/core/TextBlock.endpoints';

@Injectable({
  providedIn: 'root',
})
export class TextBlockDialogService {
  constructor(private http: SoeHttpClient) {}

  getAll(entity: number): Observable<TextblockDTO[]> {
    return this.http.get<TextblockDTO[]>(getTextBlocks(entity));
  }
  get(textBlockId: number): Observable<TextblockDTO> {
    return this.http.get<TextblockDTO>(getTextBlock(textBlockId));
  }
  save(model: TextBlockModel): Observable<any> {
    return this.http.post(saveTextBlock(), model);
  }
  delete(textBlockId: number): Observable<any> {
    return this.http.delete(deleteTextBlock(textBlockId));
  }
}
