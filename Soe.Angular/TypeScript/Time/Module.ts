import '../Core/Module';

import { TermPartsLoaderProvider } from "../Core/Services/TermPartsLoader";

// TODO: Make sure removing dependency to Common does not cause problems. Move Common directives in here in a needed basis
angular.module("Soe.Time", ['Soe.Core']) 
    .config(/*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('time');
    });
