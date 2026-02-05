import { HttpClient } from '@angular/common/http';
import { TranslateLoader } from '@ngx-translate/core';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { Observable, Observer } from 'rxjs';

export enum TermType {
  Core,
  Error,
  Common,
  Economy,
  Time,
  Billing,
  Manage,
  AgGrid,
  ClientManagement,
}

type TranslateResponse = { [translationKey: string]: string };

class TermDatabase {
  constructor(
    private http: HttpClient,
    lang: string
  ) {}

  get dbName() {
    return 'terms';
  }

  get storeName() {
    return 'termStore';
  }

  get dbFactory(): IDBFactory {
    return indexedDB;
  }

  private getTermTypeName(type: TermType) {
    switch (type) {
      case TermType.Core:
        return 'Core';
      case TermType.Error:
        return 'Error';
      case TermType.Common:
        return 'Common';
      case TermType.Economy:
        return 'Economy';
      case TermType.Time:
        return 'Time';
      case TermType.Billing:
        return 'Billing';
      case TermType.Manage:
        return 'Manage';
      case TermType.AgGrid:
        return 'AgGrid';
      case TermType.ClientManagement:
        return 'ClientManagement';
      default:
        return 'MISSING';
    }
  }

  private getRowKey(type: TermType) {
    const prefix = this.getTermTypeName(type);
    return `${prefix}_${SoeConfigUtil.language}`;
  }

  public getTerms(types: TermType[]) {
    return (observer: Observer<TranslateResponse>) => {
      const openRequest = this.dbFactory.open(
        `terms`,
        SoeConfigUtil.termVersionNrInt
      );

      openRequest.onsuccess = () => {
        let n = 0;
        let result: TranslateResponse = {};
        const db = openRequest.result;
        const observable = new Observable(this.initLoadTerms(db, types));
        observable.subscribe(val => {
          n++;
          result = { ...result, ...val }; //Translator expects one response

          if (n === types.length) {
            //All types have been resolved, ready to send final version back to translator
            observer.next(result);
            observer.complete();
          }
        });
      };

      openRequest.onupgradeneeded = () => {
        //Triggered when...
        //  1) DB not existing since before
        //  2) soeConfig.termVersionNr is changed
        const db = openRequest.result;
        if (db.objectStoreNames.contains(this.storeName)) {
          db.deleteObjectStore(this.storeName);
        }
        db.createObjectStore(this.storeName);
      };

      openRequest.onerror = error => {
        console.error(error);
      };
    };
  }

  private initLoadTerms(db: IDBDatabase, types: TermType[]) {
    return (observer: Observer<TranslateResponse>) => {
      const checker = (function () {
        //Make sure the observer is closed when done.
        let n = 0;
        return () => {
          n++;
          if (n >= types.length) {
            observer.complete();
          }
        };
      })();

      types.forEach(type => {
        const transaction = db.transaction(this.storeName, 'readonly');
        const termStore = transaction.objectStore(this.storeName);

        //Get term from store
        const action = termStore.get(this.getRowKey(type));

        action.onsuccess = () => {
          if (action.result) {
            observer.next(action.result);
            checker();
          } else {
            //Get termgroup from server
            this.loadTermGroup(db, type, observer, checker);
          }
        };

        action.onerror = error => {
          observer.next({});
          console.error(error);
          checker();
        };
      });
    };
  }

  private loadTermGroup(
    db: IDBDatabase,
    type: TermType,
    result: Observer<any>,
    checkIfDone: () => void
  ) {
    this.loadFromServer(type).subscribe(val => {
      const transaction = db.transaction(this.storeName, 'readwrite');
      const termStore = transaction.objectStore(this.storeName);
      const actionResult = termStore.put(val, this.getRowKey(type));

      actionResult.onsuccess = () => {
        result.next(val);
        checkIfDone();
      };

      actionResult.onerror = error => {
        console.error(error);
        result.next({});
        checkIfDone();
      };
    });
  }

  private loadFromServer(type: TermType) {
    return this.http.get<TranslateResponse>(
      `translation/${SoeConfigUtil.language}/${this.getTermTypeName(type)}`
    );
  }
}

export class CustomTranslateLoader implements TranslateLoader {
  constructor(
    private http: HttpClient,
    private termTypes: TermType[]
  ) {}

  public getTranslation(lang: string): Observable<TranslateResponse> {
    const db = new TermDatabase(this.http, lang);
    return new Observable(db.getTerms(this.termTypes));
  }
}

export function createCommonTranslateLoader(http: HttpClient) {
  return new CustomTranslateLoader(http, [
    TermType.Core,
    TermType.Common,
    TermType.Error,
    TermType.AgGrid,
    TermType.Time,
    TermType.Economy,
    TermType.Manage,
    TermType.Billing,
    TermType.ClientManagement,
  ]);
}
