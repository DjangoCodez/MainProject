import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { filter } from 'rxjs/operators';
import { GridComponent } from '@ui/grid/grid.component';
import { Constants } from '@shared/util/client-constants';

interface IMessage<T = any> {
  topic: string;
  data: T;
  guid?: string;
}

@Injectable({ providedIn: 'root' })
export class MessagingService {
  private subject$ = new Subject<IMessage>();

  constructor() {
    this.subject$ = new Subject<IMessage>();
  }

  publish<T = any>(event: string, data: T, guid?: string): void {
    this.subject$.next({ topic: event, data: data, guid });
  }

  onEvent<T = any>(event: string, guid?: string): Observable<IMessage<T>> {
    return this.subject$.pipe(
      filter(m => {
        const matchGridIfNotNull = !guid || guid === m.guid;
        return m.topic == event && matchGridIfNotNull;
      })
    );
  }

  publishEnableTabByKey(key: string, enabled: boolean): void {
    this.publish(Constants.EVENT_ENABLE_TAB, { key: key, enabled: enabled });
  }

  publishEnableTabByIndex(index: number, enabled: boolean): void {
    this.publish(Constants.EVENT_ENABLE_TAB, {
      index: index,
      enabled: enabled,
    });
  }

  publishEnableTabAdd(): void {
    this.publish(Constants.EVENT_ENABLE_TAB_ADD, null);
  }

  publishGridReady(grid: GridComponent<any>, guid: string): void {
    this.publish(Constants.EVENT_GRID_READY, { grid }, guid);
  }
}
