import '../Module';

import { ExportService } from "./ExportService";

angular.module("Soe.Economy.Export", ['Soe.Economy', 'Soe.Economy.Common.Module'])
    .service("exportService", ExportService);

