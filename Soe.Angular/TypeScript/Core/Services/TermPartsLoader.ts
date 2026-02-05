import { IHttpService } from "./HttpService";
import { IStorageService } from "./StorageService";

export interface ITermPartsLoaderProvider {
    addPart(partName: string);
    setVersion(version: string);
}

export class TermPartsLoaderProvider {
    private parts = [];
    private version = "0";

    addPart(partName: string) {
        this.parts.push(partName);
    }
    setVersion(version: string) {
        this.version = version;
    }

    //@ngInject
    $get(httpService: IHttpService, $q: ng.IQService, storageService: IStorageService) {
        var service = (options) => {
            var deferred = $q.defer();

            var table = {};
            var promises = new Array<any>();
            _.forEach(this.parts, part => {
                var key = this.version + "#term." + part + "." + options.key + ".";
                var data = storageService.fetch(key);
                if (!data) {
                    var url = options.urlTemplate.replace(/\{part\}/g, part).replace(/\{lang\}/g, options.key);
                    promises.push(httpService.get(url, false).then(x => {
                        storageService.add(key, x);
                        _.extend(table, x);
                    }));
                } else {
                    _.extend(table, data);
                }
            });

            if (promises.length > 0) {
                $q.all(promises).then(() => {
                    deferred.resolve(table);
                });
            } else {
                deferred.resolve(table);
            }

            return deferred.promise;
        };

        service['clearCachedTerms'] = () => {
            storageService.clear((key) => {
                return key.contains('#term.') && !key.startsWithCaseInsensitive(this.version);
            });
        };

        return service;
    }
}
