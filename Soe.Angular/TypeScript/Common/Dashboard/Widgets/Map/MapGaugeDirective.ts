import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { Feature } from "../../../../Util/CommonEnumerations";
import { IGoogleMapsService } from "../../../GoogleMaps/GoogleMapsService";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class MapGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('Map', 'MapGauge.html'), MapGaugeController);
    }
}

class MapGaugeController extends WidgetControllerBase {

    map;
    options;
    searchbox;
    //marker;
    markers: any[] = [];

    private showOrdersOnMapPermission: boolean = false;

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private googleMapsService: IGoogleMapsService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private uiGmapGoogleMapApi) {
        super($timeout, $q, uiGridConstants);
    }

    $onInit() {
        this.markers = [];
        this.map = {
            "center": { "latitude": 0, "longitude": 0 },
        };

        this.options = {
            fullscreenControl: true,
            scrollwheel: true
        };
    }

    protected setup(): ng.IPromise<any> {
        this.widgetCss = 'col-sm-6';
        var keys: string[] = ['common.dashboard.map.title'
        ];

        return this.$q.all([
            this.translationService.translateMany(keys).then(terms => {
                this.widgetTitle = terms["common.dashboard.map.title"];
            }),
            this.loadPermissions()
        ]);
    }

    private loadPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];

        featureIds.push(Feature.Billing_Order_ShowOnMap);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.showOrdersOnMapPermission = x[Feature.Billing_Order_ShowOnMap];
        });
    }

    protected load() {
        super.load();
        this.markers = [];
        //set center...
        this.coreService.getMapStartAddress().then((address) => {
            if (address) {
                this.getAddressLocation(address).then((location) => {
                    this.setMapCenter(location.lat, location.lng, 15);
                });
            }
        })

        //Load 
        this.coreService.getMapLocations(CalendarUtility.getDateToday().addDays(-7)).then((locations) => {
            for (var i = 0; i < locations.length; i++) {
                var location = locations[i];
                this.addMarker(location.latitude, location.longitude, location.name, "yellow-dot.png");
            }
        });

        if (this.showOrdersOnMapPermission) {
            this.coreService.getMapPlannedOrders(CalendarUtility.getDateToday()).then((orders) => {
                //console.log("orders", orders);

                for (var i = 0; i < orders.length; i++) {
                    var order = orders[i];
                    this.getAddressLocation(order.address).then((location) => {
                        this.addMarker(location.lat, location.lng, order.customerName + "\n" + order.address, "blue-dot.png");
                    });
                }
            });
        }

        this.loadComplete(0);
    }

    private getAddressLocation(address: string): ng.IPromise<any> {
        return this.googleMapsService.getLocation(address);
    }

    private setMapCenter(latitude: number, longitude: number, zoom: number) {
        this.map = {
            center: { latitude: latitude, longitude: longitude },
            zoom: zoom,
        };
    }

    private addMarker(latitude: number, longitude: number, title: string, googleIcon: string) {
        var icon = googleIcon ? icon = "https://maps.google.com/mapfiles/ms/icons/" + googleIcon : "";
        let marker = {
            id: this.markers.length + 1,
            latitude: latitude,
            longitude: longitude,
            options: {
                title: title,
                icon: icon,
                //labelContent: title,
                //labelAnchor: '22 0',
                //labelClass: 'marker-labels',
                //labelVisible: true
            }
        };
        this.markers.push(marker);
    }
}