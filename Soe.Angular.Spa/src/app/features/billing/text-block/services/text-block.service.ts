import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteTextBlock,
  getTextBlock,
  getTextBlocks,
  saveTextBlock,
} from '@shared/services/generated-service-endpoints/core/TextBlock.endpoints';
import { map, Observable } from 'rxjs';
import { TextBlockGridDTO, TextBlockModel } from '../models/text-block.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class TextBlockService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps: {
    entity: number;
    useCache: boolean;
    cacheExpireTime?: number;
  } = {
    entity: 0,
    useCache: false,
    cacheExpireTime: undefined,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      entity: number;
      useCache: boolean;
      cacheExpireTime?: number;
    }
  ): Observable<TextBlockGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<TextBlockGridDTO[]>(
      getTextBlocks(this.getGridAdditionalProps.entity, id),
      {
        useCache: this.getGridAdditionalProps.useCache,
        cacheOptions: { expires: this.getGridAdditionalProps.cacheExpireTime },
      }
    );
  }

  get(
    textblockId: number,
    useCache = false,
    cacheExpireTime?: number
  ): Observable<TextBlockGridDTO> {
    return this.http
      .get<TextBlockGridDTO>(getTextBlock(textblockId), {
        useCache,
        cacheOptions: { expires: cacheExpireTime },
      })
      .pipe(
        map(data => {
          const obj = new TextBlockGridDTO();
          Object.assign(obj, data);
          return data;
        })
      );
  }

  save(model: TextBlockModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveTextBlock(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteTextBlock(id));
  }
}
