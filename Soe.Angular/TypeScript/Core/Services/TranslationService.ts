import { CoreUtility } from '../../Util/CoreUtility';

export interface ITranslationService {
    translate(key: string): ng.IPromise<string>;
    translate(key: string, replacements: any): ng.IPromise<string>;
    translateMany(keys: string[]): ng.IPromise<{ [index: string]: string }>;
    translateMany(keys: string[], replacements: any): ng.IPromise<{ [index: string]: string }>;
    translateInstant(key: string): string;
    refresh(): ng.IPromise<void>;
}

export class TranslationService implements ITranslationService {
    //@ngInject
    constructor(private $translate: ng.translate.ITranslateService, private $q: ng.IQService) { }

    public translate(key: string): ng.IPromise<string>;
    public translate(key: string, replacements: any = null): ng.IPromise<string> {
        var deferral = this.$q.defer<string>();
        var promise: ng.IPromise<string>;

        if (key) {
            this.$translate.use(CoreUtility.language);

            if (replacements) {
                promise = this.$translate(key, replacements);
            } else {
                promise = this.$translate(key);
            }

            promise.then(value => {
                deferral.resolve(value);
            }).catch(err => {
                deferral.resolve(key);
            });
        } else {
            deferral.resolve('');
        }

        return deferral.promise;
    }

    public translateMany(keys: string[]): ng.IPromise<{ [index: string]: string }>;
    public translateMany(keys: string[], replacements: any = null): ng.IPromise<{ [index: string]: string }> {
        var deferral = this.$q.defer<{ [index: string]: string }>();
        var promise: ng.IPromise<{ [index: string]: string }>;

        this.$translate.use(CoreUtility.language);

        if (replacements) {
            promise = this.$translate(keys, replacements);
        } else {
            promise = this.$translate(keys);
        }

        promise.then(value => {
            deferral.resolve(value);
        }).catch(err => {
            var ret: { [index: string]: string } = {};
            _.forEach(keys, x => {
                ret[x] = x;
            });
            deferral.resolve(ret);
        });

        return deferral.promise;
    }

    public translateInstant(key: string): string {
        this.$translate.use(CoreUtility.language);
        return this.$translate.instant(key);
    }

    public refresh() {
        return this.$translate.refresh(CoreUtility.language);
    }
}