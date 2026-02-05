import '../../Core/Module';

import 'angular-google-maps';
import 'angular-simple-logger';

import { GoogleMapsService } from './GoogleMapsService';

angular.module("Soe.Common.GoogleMaps", ['Soe.Core', 'uiGmapgoogle-maps', 'nemLogging'])
    .service("googleMapsService", GoogleMapsService)
    .config((uiGmapGoogleMapApiProvider) => {
        // Google Maps (xe_browser_key)
        uiGmapGoogleMapApiProvider.configure({
            transport: 'auto',
            key: 'AIzaSyA2W2Y55jpZkxoMMIB2ZB9eV_eU9ERD-YQ',
            v: 'quarterly',
            libraries: 'weather,geometry,visualization,places'
        });
    });
