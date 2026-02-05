import { CoreUtility } from "../../Util/CoreUtility";
import Bundle from "../../Common/HtmlEditor/Bundle";

declare var System;

export interface ILazyLoadService {
    loadBundle(bundleName: string) : Promise<any>;
}

export class LazyLoadService implements ILazyLoadService {
    //@ngInject
    constructor(private $ocLazyLoad: oc.ILazyLoad) { }

    loadBundle(bundleName: string): Promise<any> {
        let fileName = bundleName.split('.').slice(1).join('/') + '.js';
        let loadIndividualFiles = CoreUtility.isDebugMode;
        let url = loadIndividualFiles ? CoreUtility.baseUrl + fileName : '/angular/dist/' + bundleName + '.js';

        if (this.$ocLazyLoad.isLoaded(url)) {
            return Promise.resolve();
        }

        return new Promise((resolve, reject) => {
            System.import(url).then(x => {
                if (!loadIndividualFiles) {
                    return System.import(fileName).then(f => {
                        return this.$ocLazyLoad.load(f.default);
                    });
                }

                return this.$ocLazyLoad.load(x.default);
            }).then(() => {
                resolve(null);
            });
        });
    }
}