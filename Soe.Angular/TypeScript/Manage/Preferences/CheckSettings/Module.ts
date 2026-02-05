import '../../Module';

import { PreferencesService } from "../PreferencesService";

angular.module("Soe.Manage.Preferences.CheckSettings.Module", ['Soe.Manage'])
    .service("preferencesService", PreferencesService);
