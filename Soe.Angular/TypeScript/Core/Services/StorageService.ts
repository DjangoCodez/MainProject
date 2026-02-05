import { CoreUtility } from "../../Util/CoreUtility";

declare var store: any;

export interface IStorageService {

    createTermGroupKey(id: number, type: string);
    createCompanyObjectKey(actorCompanyId: string, type: string);
    add(key: string, object: any, expire?: Date);
    fetch(key: string): any;
    remove(key: string);
    clear(predicate?: (key: string) => boolean);
    clearOld(key: string);

}

export class StorageService implements IStorageService {

    constructor() {
        this.clearExpiredItems();
    }

    createTermGroupKey(id: number, type: string) {
        return CoreUtility.termVersionNr + "#" + id.toString() + "#" + type;
    }

    createCompanyObjectKey(actorCompanyId: string, type: string) {
        return CoreUtility.termVersionNr + "#" + actorCompanyId.toString() + "#" + type;
    }

    add(key: string, object: any, expire?: Date) {
        if (!store.enabled)
            return;
        try {
            store.set(key, { value: object, expire: expire });
        } catch (e) {
            if (e.name === 'QuotaExceededError') {
                this.clear();
                return;
            }
            console.error(e);
        }
    }

    fetch(key: string): any {
        if (!store.enabled)
            return null;

        var x = store.get(key);
        if (!x)
            return null;

        if (x.expire && x.expire < new Date())
            return null;

        return x.value;
    }

    remove(key: string) {
        if (!store.enabled)
            return;

        store.remove(key);
    }

    clear(predicate?: (key: string) => boolean) {
        if (!store.enabled)
            return;

        if (predicate) {
            store.forEach((key: string) => {
                if (predicate(key)) {
                    store.remove(key);
                }
            })
        } else {
            store.clear();
        }
    }

    clearOld(key: string) {
        if (!store.enabled)
            return;

        key = key.replace(CoreUtility.termVersionNr, "");

        _.forIn(window.localStorage, (value: string, objKey: string) => {
            if (true === _.includes(objKey, key)) {
                this.remove(objKey);
            }
        });
    }

    clearExpiredItems() {
        var now = new Date();
        for (var key in store.getAll()) {
            var item = store.get(key);
            if (item.expire) {
                if (new Date(Date.parse(item.expire)) < now) {
                    store.remove(key);
                }
            }
        }
    }
}