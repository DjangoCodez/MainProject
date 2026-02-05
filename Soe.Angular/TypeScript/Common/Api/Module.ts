import '../../Core/Module';
import { ApiService } from './ApiService';

angular.module("Soe.Common.Api.Module", ['Soe.Core'])
    .service("apiService", ApiService)
