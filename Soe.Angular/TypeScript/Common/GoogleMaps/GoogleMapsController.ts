import { ICoreService } from "../../Core/Services/CoreService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { IGoogleMapsService } from "./GoogleMapsService";

export class GoogleMapsController {

    map;
    options;
    searchbox;
    marker;

    locationFound: boolean = true;

    //@ngInject
    constructor(
        $http,
        private $uibModalInstance,
        private $window: ng.IWindowService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private googleMapsService: IGoogleMapsService,
        private title: string,
        private zoom: number,
        private latitude: number,
        private longitude: number,
        private search: string,
        uiGmapGoogleMapApi) {

        this.map = {
            "center": { "latitude": 0, "longitude": 0 },
        };

        if (this.latitude !== 0 && this.longitude !== 0)
            this.title = "N " + this.latitude + "  E " + this.longitude;
        else if (this.search)
            this.title = this.search;

        uiGmapGoogleMapApi.then(maps => {
            this.options = {
                fullscreenControl: true,
                scrollwheel: true
            };

            if (this.latitude !== 0 && this.longitude !== 0)
                this.showMarker(this.latitude, this.longitude, this.title);
            else {
                this.googleMapsService.geoCode(this.search).then((x) => {
                    var results = x.data.results;
                    if (results && results.length > 0) {
                        var result = results[0];
                        var location = result.geometry.location;
                        this.showMarker(location.lat, location.lng, result.formatted_address);
                    } else {
                        this.locationFound = false;
                    }
                });
            }
            //var events = {
            //    places_changed: function (searchBox) {
            //var place = searchBox.getPlaces();
            //if (!place || place == 'undefined' || place.length == 0) {
            //    //console.log('no place data :(');
            //    return;
            //}

            //this.map = {
            //    "center": {
            //        "latitude": place[0].geometry.location.lat(),
            //        "longitude": place[0].geometry.location.lng()
            //    },
            //    "zoom": 18
            //};
            //this.marker = {
            //    id: 0,
            //    coords: {
            //        latitude: place[0].geometry.location.lat(),
            //        longitude: place[0].geometry.location.lng()
            //    }
            //};
            //    }
            //}
            //this.searchbox = {
            //    template: this.urlHelperService.getCommonViewUrl("Dialogs/GoogleMaps", "searchbox.tpl.html"),
            //    events: events
            //};
        });
    }

    showMarker(latitude: number, longitude: number, title: string) {
        this.map = {
            center: { latitude: latitude, longitude: longitude },
            zoom: this.zoom,
        };
        this.marker = {
            id: 1,
            coords: { latitude: latitude, longitude: longitude },
            options: {
                title: title,
                clickable: false
            }
        }
    }

    close() {
        this.$uibModalInstance.dismiss('cancel');
    }
}