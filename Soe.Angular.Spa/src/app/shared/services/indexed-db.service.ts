import { Injectable } from '@angular/core';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

type onSuccessCB = (store: IDBObjectStore) => void;
type onErrorCB = (error: Event) => void;

@Injectable({
  providedIn: 'root',
})
export class IndexedDBService {
  get dbFactory(): IDBFactory {
    return indexedDB;
  }

  get defaultStoreName(): string {
    return 'records';
  }

  get versionedDbName(): string {
    return 'soeVersioned';
  }

  get defaultDbName(): string {
    return 'soe';
  }

  public getRequest(
    versionDependent: boolean,
    onSuccess: onSuccessCB,
    onError: onErrorCB,
    explicitStoreName?: string
  ) {
    return this.createRequest(
      'readonly',
      versionDependent,
      onSuccess,
      onError,
      explicitStoreName
    );
  }

  public addRequest(
    versionDependent: boolean,
    onSuccess: onSuccessCB,
    onError: onErrorCB,
    explicitStoreName?: string
  ) {
    return this.createRequest(
      'readwrite',
      versionDependent,
      onSuccess,
      onError,
      explicitStoreName
    );
  }

  private createRequest(
    type: 'readonly' | 'readwrite',
    versionDependent: boolean,
    onSuccess: onSuccessCB,
    onError: onErrorCB,
    explicitStoreName?: string
  ) {
    const dbName = this.getDbName(versionDependent, explicitStoreName);
    const storeName = this.getStoreName(explicitStoreName);
    const req = this.initOpen(dbName, versionDependent);

    // IndexedDB handle requests in a queue.
    // If the user has multiple tabs open, and on one tab is working on a request that takes a long time,
    // the other tabs will be blocked until the request is completed.
    // To prevent this, we set a timeout on the request.
    const timeoutDurationMS = 1000; // 1000 ms
    const timeoutId = setTimeout(() => {
      req.onerror && req.onerror(new Event('REQUEST-TIMEOUT'));
    }, timeoutDurationMS);

    const clearRequestTimeout = () => {
      clearTimeout(timeoutId);
    };

    req.onupgradeneeded = () => {
      //Triggered when...
      //  1) DB not existing since before
      //  2) soeConfig.termVersionNr is changed
      clearRequestTimeout();
      const db = req.result;
      if (db.objectStoreNames.contains(storeName)) {
        db.deleteObjectStore(storeName);
      }
      db.createObjectStore(storeName);
    };

    req.onsuccess = () => {
      clearRequestTimeout();
      const db = req.result;
      const trans = db.transaction(storeName, type);
      const store = trans.objectStore(storeName);
      onSuccess(store);
    };

    req.onerror = error => {
      clearRequestTimeout();
      onError(error);
    };

    return req;
  }

  //Helpers
  private getStoreName(explicitStoreName?: string) {
    if (explicitStoreName) {
      return explicitStoreName;
    }

    return this.defaultStoreName;
  }

  private getDbName(versionDependent: boolean, explicitStoreName?: string) {
    if (explicitStoreName) {
      return explicitStoreName;
    }

    if (versionDependent) {
      return this.versionedDbName;
    }

    return this.defaultDbName;
  }

  private initOpen(
    storeName: string,
    versionDependent: boolean
  ): IDBOpenDBRequest {
    if (versionDependent) {
      return this.dbFactory.open(storeName, SoeConfigUtil.termVersionNrInt);
    }

    return this.dbFactory.open(storeName);
  }
}
