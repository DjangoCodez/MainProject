import { CoreUtility } from "../../Util/CoreUtility";

export interface IGoogleMapsService {
    geoCode(address: string): ng.IPromise<any>
    getLocation(address: string): ng.IPromise<any>
}

export class GoogleMapsService implements IGoogleMapsService {

    //@ngInject
    constructor(private $http, private $q: ng.IQService) { }

    // GET
    geoCode(address: string) {

        // TODO: There must be a better way to do this!
        // Google does not like our custom headers, so we need to remove them,
        // After the call we add them again, otherwise everything else breaks down.
        var soeparameters = this.$http.defaults.headers.common['soeparameters'];
        this.$http.defaults.headers.common.Authorization = undefined;
        this.$http.defaults.headers.common['soeparameters'] = undefined;
        address = encodeURIComponent(address);
        var url = 'https://maps.googleapis.com/maps/api/geocode/json';
        url += '?key=' + this.getKey();
        url += '&address=' + address;
        url += '&sensor=false';
        var result = this.$http.get(url);
        this.$http.defaults.headers.common['soeparameters'] = soeparameters;

        return result;
    }

    getLocation(address: string) {
        var deferral = this.$q.defer();

        this.geoCode(address).then((x) => {
            var results = x.data.results;
            if (results && results.length > 0) {
                var result = results[0];
                var location = result.geometry.location;
                deferral.resolve(location);
            } else {
                deferral.resolve(null);
            }
        });

        return deferral.promise;
    }

    getKey() {
        return 'AIzaSyA2W2Y55jpZkxoMMIB2ZB9eV_eU9ERD-YQ';
    }
}