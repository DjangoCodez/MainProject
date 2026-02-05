import { Subscriber } from 'rxjs';
import { IndexedDBService } from './indexed-db.service';
import { Injectable } from '@angular/core';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

type Seconds = number;
export interface ICacheOptions {
  /**
   *
   * When the item should be stored in a certain store.
   * If not defined, the store will be decided by whether
   * it's version dependent or not.
   *
   * @default: undefined
   */
  explicitStoreName?: string;
  /**
   * If an item is version dependent, the values will automatically be
   * considered expired when a new version is deployed.
   *
   * @default: true
   */
  versionDependent?: boolean;
  /**
   * Key is used to build the identifier in the database.
   * Should be used when invalidating the items via other requests.
   * Default value is the request's endpoint.
   *
   * Yields db key in the following (approximate) format: actorcompanyid + key
   *
   * @default: endpoint
   */
  key?: string;
  /**
   * When the item should be considered expired.
   * If type is number, date is calculated as current date time + seconds.
   * If undefined, no expiration is set.
   *
   * @default: undefined (never expires by date)
   */
  expires?: Seconds | Date;
}

interface ICacheItem<T> {
  created: Date;
  validUntil?: Date;
  data: T;
}

@Injectable({
  providedIn: 'root',
})
export class CacheService {
  constructor(private db: IndexedDBService) {}

  public getCacheItem<T>(
    subscriber: Subscriber<T | null>,
    endpoint: string,
    options?: ICacheOptions
  ) {
    const { explicitStoreName, versionDependent, key } = {
      ...this.defaultOptions(),
      ...options,
    };
    const cacheKey = this.getCacheKey(endpoint, key);
    const isVersionDependent = this.getVersionDependency(versionDependent);

    const onErrorCb = (error: Event) => {
      this.handleError(error);
      subscriber.next(null);
    };

    const onSuccessCb = (store: IDBObjectStore) => {
      const action = store.get(cacheKey);

      action.onsuccess = () => {
        if (action.result) {
          const item = action.result as ICacheItem<T>;
          //console.log('Found item in cache', item);
          if (!item.validUntil || new Date(item.validUntil) > new Date()) {
            subscriber.next(item.data);
            return;
          }
        }
        subscriber.next(null);
      };

      action.onerror = (error: Event) => {
        this.handleError(error);
        subscriber.next(null);
      };
    };

    this.db.getRequest(
      isVersionDependent,
      onSuccessCb,
      onErrorCb,
      explicitStoreName
    );
  }

  public setCacheItem<T>(value: T, endpoint: string, options?: ICacheOptions) {
    const { explicitStoreName, versionDependent, key, expires } = {
      ...this.defaultOptions(),
      ...options,
    };
    const isVersionDependent = this.getVersionDependency(versionDependent);
    const cacheKey = this.getCacheKey(endpoint, key);
    const validUntil = this.getExpired(expires);
    const cacheItem: ICacheItem<T> = {
      created: new Date(),
      validUntil: validUntil,
      data: value,
    };

    const onErrorCb = (error: Event) => {
      this.handleError(error);
    };

    const onSuccessCb = (store: IDBObjectStore) => {
      const action = store.put(cacheItem, cacheKey);

      action.onsuccess = () => {
        //console.log('Success, added item to cache');
      };

      action.onerror = (error: Event) => {
        this.handleError(error);
      };
    };

    this.db.addRequest(
      isVersionDependent,
      onSuccessCb,
      onErrorCb,
      explicitStoreName
    );
  }

  private handleError(event: Event) {
    if (event.type == 'REQUEST-TIMEOUT') return;
    console.error(event);
  }

  private getExpired(value: Seconds | Date | undefined): Date | undefined {
    if (!value) {
      return undefined;
    }

    if (typeof value === 'number') {
      const date = new Date();
      date.setSeconds(date.getSeconds() + value);
      return date;
    }

    return new Date(value);
  }

  private getCacheKey(endpoint: string, explicitKey?: string) {
    const generateKey = (value: string) => {
      return `${SoeConfigUtil.actorCompanyId}-${value}`;
    };

    if (explicitKey) {
      return generateKey(explicitKey);
    }

    return generateKey(endpoint);
  }

  private getVersionDependency(value?: boolean) {
    if (!value) return false;

    return value;
  }

  private defaultOptions(): ICacheOptions {
    return {
      explicitStoreName: undefined,
      versionDependent: true,
      key: undefined,
      expires: undefined,
    };
  }
}
