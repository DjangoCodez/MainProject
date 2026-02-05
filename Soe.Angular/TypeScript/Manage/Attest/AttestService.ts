import { ICoreService } from "../../Core/Services/CoreService";
import { IHttpService } from "../../Core/Services/HttpService";
import { SoeModule, TermGroup_AttestEntity, SoeTimeCodeType, TermGroup_InvoiceProductVatType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { AttestStateDTO } from "../../Common/Models/AttestStateDTO";
import { SmallGenericType } from "../../Common/Models/SmallGenericType";
import { AttestRuleHeadDTO, AttestRuleRowDTO, AttestRuleHeadGridDTO } from "../../Common/Models/AttestRuleHeadDTO";
import { IAttestRoleDTO, IUpdateAttestRoleModel } from "../../Scripts/TypeLite.Net4";

export interface IAttestService {

    // GET
    getAttestRole(attestRoleId: number): ng.IPromise<IAttestRoleDTO>
    getAttestRoles(module: SoeModule, includeInactive): ng.IPromise<any>;
    getAttestRuleHeads(module: SoeModule, onlyActive: boolean, loadEmployeeGroups: boolean, loadRows: boolean): ng.IPromise<AttestRuleHeadDTO[]>;
    getAttestRuleHeadsGrid(module: SoeModule): ng.IPromise<AttestRuleHeadGridDTO[]>;
    getAttestRuleHead(attestRuleHeadId: number, loadEmployeeGroups: boolean, loadRows: boolean): ng.IPromise<AttestRuleHeadDTO>;
    getAttestTransitionGridDTOs(entity: TermGroup_AttestEntity, module: SoeModule, setEntityName: boolean): ng.IPromise<any>;
    getAttestStatesGenericList(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean, addMultipleRow: boolean): ng.IPromise<any>;
    getAttestStates(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean): ng.IPromise<AttestStateDTO[]>;
    getAttestEntitiesGenericList(addEmptyRow: boolean, skipUnknown: boolean, module: SoeModule): ng.IPromise<any>;
    getAttestWorkFlowTemplates(entity: TermGroup_AttestEntity): ng.IPromise<any>;
    getAttestWorkFlowTemplate(attestWorkFlowTemplateId: number): ng.IPromise<any>;
    getAttestTransitions(entity: TermGroup_AttestEntity, module: SoeModule, setEntityName: boolean): ng.IPromise<any>;
    getAttestWorkFlowTemplateRows(attestWorkFlowTemplateId: number): ng.IPromise<any>;
    getDayTypesDict(addEmptyRow: boolean): ng.IPromise<SmallGenericType[]>
    getTimeCodesDict(timeCodeType: SoeTimeCodeType, addEmptyRow: boolean, concatCodeAndName: boolean): ng.IPromise<SmallGenericType[]>;
    getPayrollProductsDict(addEmptyRow: boolean, concatNumberAndName: boolean, useCache: boolean): ng.IPromise<SmallGenericType[]>
    getInvoiceProductsDict(invoiceProductVatType: TermGroup_InvoiceProductVatType, addEmptyRow: boolean): ng.IPromise<SmallGenericType[]>;

    // POST
    saveAttestRole(attestRole: IAttestRoleDTO): ng.IPromise<any>;
    saveAttestWorkFlowTemplate(attestTemplate): ng.IPromise<any>;
    saveAttestWorkFlowTemplateRows(attestTemplateRows, attestTemplateId): ng.IPromise<any>;
    saveAttestRule(attestRule: AttestRuleHeadDTO): ng.IPromise<any>;
    updateAttestRuleState(attestRuleHeads: any[]): ng.IPromise<any>;
    updateAttestRoleState(attestRoleState: IUpdateAttestRoleModel): ng.IPromise<any>;

    // DELETE
    deleteAttestRole(attestRoleId: number): ng.IPromise<any>;
    deleteAttestWorkFlowTemplate(attestTemplateId: number): ng.IPromise<any>;
    deleteAttestRule(attestRuleHeadId: number): ng.IPromise<any>;
}

export class AttestService implements IAttestService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getAttestRoles(module: SoeModule, includeInactive: boolean = false) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE + "?module=" + module + "&includeInactive=" + includeInactive, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getAttestRole(attestRoleId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE + attestRoleId, false);
    }

    getAttestRuleHeads(module: SoeModule, onlyActive: boolean, loadEmployeeGroups: boolean, loadRows: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_RULE + "?module=" + module + "&onlyActive=" + onlyActive + "&loadEmployeeGroups=" + loadEmployeeGroups + "&loadRows=" + loadRows, false, Constants.WEBAPI_ACCEPT_DTO).then(x => {
            return x.map(y => {
                let obj = new AttestRuleHeadDTO();
                angular.extend(obj, y);

                if (y.attestRuleRows) {
                    obj.attestRuleRows = y.attestRuleRows.map(r => {
                        let rObj = new AttestRuleRowDTO();
                        angular.extend(rObj, r);
                        return rObj;
                    });
                } else {
                    obj.attestRuleRows = [];
                }

                return obj;
            });
        });
    }

    getAttestRuleHeadsGrid(module: SoeModule) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_RULE + "?module=" + module, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new AttestRuleHeadGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getAttestRuleHead(attestRuleHeadId: number, loadEmployeeGroups: boolean, loadRows: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_RULE + attestRuleHeadId + "/" + loadEmployeeGroups + "/" + loadRows, false).then(x => {
            let obj = new AttestRuleHeadDTO();
            angular.extend(obj, x);

            if (x.attestRuleRows) {
                obj.attestRuleRows = x.attestRuleRows.map(r => {
                    let rObj = new AttestRuleRowDTO();
                    angular.extend(rObj, r);
                    return rObj;
                });
            } else {
                obj.attestRuleRows = [];
            }

            return obj;
        });
    }

    getAttestTransitionGridDTOs(entity: TermGroup_AttestEntity, module: SoeModule, setEntityName: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_TRANSITION + entity + "/" + module + "/" + setEntityName, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getAttestTransitions(entity: TermGroup_AttestEntity, module: SoeModule, setEntityName: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_TRANSITION + entity + "/" + module + "/" + setEntityName, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getAttestStatesGenericList(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean, addMultipleRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE_GL + entity + "/" + module + "/" + addEmptyRow + "/" + addMultipleRow, false);
    }

    getAttestStates(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE + entity + "/" + module + "/" + addEmptyRow, false);
    }

    getAttestEntitiesGenericList(addEmptyRow: boolean, skipUnknown: boolean, module: SoeModule) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ENTITY_GL + addEmptyRow + "/" + skipUnknown + "/" + module, false);
    }
    getAttestWorkFlowTemplates(entity: TermGroup_AttestEntity): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE_LIST + entity, false);
    }

    getAttestWorkFlowTemplate(attestWorkFlowTemplateId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE + attestWorkFlowTemplateId, false);
    }

    getAttestWorkFlowTemplateRows(attestWorkFlowTemplateId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE_ATTEST_WORK_FLOW_TEMPLATE_ROWS + attestWorkFlowTemplateId, false);
    }

    getDayTypesDict(addEmptyRow: boolean): ng.IPromise<SmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_DAY_TYPE + "?addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeCodesDict(timeCodeType: SoeTimeCodeType, addEmptyRow: boolean, concatCodeAndName: boolean): ng.IPromise<SmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE + "?timeCodeType=" + timeCodeType + "&addEmptyRow=" + addEmptyRow + "&concatCodeAndName=" + concatCodeAndName, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getPayrollProductsDict(addEmptyRow: boolean, concatNumberAndName: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT + "?addEmptyRow=" + addEmptyRow + "&concatNumberAndName=" + concatNumberAndName, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getInvoiceProductsDict(invoiceProductVatType: TermGroup_InvoiceProductVatType, addEmptyRow: boolean): ng.IPromise<SmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_INVOICE_PRODUCTS + invoiceProductVatType + "/" + addEmptyRow, false);
    }


    // POST
    saveAttestRole(attestRole: IAttestRoleDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE, attestRole);
    }
    saveAttestWorkFlowTemplate(attestTemplate): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE, attestTemplate);
    }

    saveAttestWorkFlowTemplateRows(attestTemplateRows, attestTemplateId): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE_ATTEST_WORK_FLOW_TEMPLATE_ROWS + attestTemplateId, attestTemplateRows);
    }

    saveAttestRule(attestRule: AttestRuleHeadDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_RULE, attestRule);
    }

    updateAttestRuleState(attestRuleHeads: any[]) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_RULE_STATE, attestRuleHeads);
    }
    updateAttestRoleState(attestRoleState: IUpdateAttestRoleModel) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE_UPDATE_STATE, attestRoleState);
    }

    // DELETE
    deleteAttestWorkFlowTemplate(attestTemplateId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE + attestTemplateId);
    }

    deleteAttestRule(attestRuleHeadId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_RULE + attestRuleHeadId);
    }
    deleteAttestRole(attestRoleId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE + attestRoleId);
    }
}
