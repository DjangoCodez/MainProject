import { IUserSelectionDTO, IUserSelectionAccessDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_ReportUserSelectionAccessType, UserSelectionType } from "../../Util/CommonEnumerations";
import { BoolSelectionDTO, DateRangeSelectionDTO, DateSelectionDTO, DatesSelectionDTO, IdListSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "./ReportDataSelectionDTO";
import { ReportDataSelectionDTO } from "./ReportDTOs";

export class UserSelectionDTO implements IUserSelectionDTO {
    access: UserSelectionAccessDTO[];
    actorCompanyId: number;
    description: string;
    name: string;
    selections: ReportDataSelectionDTO[];
    state: SoeEntityState;
    type: UserSelectionType;
    userSelectionId: number;
    userId: number;

    public setTypes() {
        if (this.access) {
            this.access = this.access.map(a => {
                let aObj = new UserSelectionAccessDTO();
                angular.extend(aObj, a);
                return aObj;
            });
        } else {
            this.access = [];
        }
    }

    public setSelectionTypes() {
        if (this.selections) {
            this.selections = this.selections.map(s => {
                let sObj;
                switch (s.typeName) {
                    //case 'GeneralReportSelectionDTO':
                    //    sObj = new GeneralReportSelectionDTO((<GeneralReportSelectionDTO>s).exportType);
                    //    break;
                    case 'BoolSelectionDTO':
                        sObj = new BoolSelectionDTO((<BoolSelectionDTO>s).value);
                        break;
                    case 'TextSelectionDTO':
                        sObj = new TextSelectionDTO((<TextSelectionDTO>s).text);
                        break;
                    case 'DateSelectionDTO':
                        sObj = new DateSelectionDTO((<DateSelectionDTO>s).date, (<DateSelectionDTO>s).id);
                        break;
                    case 'DatesSelectionDTO':
                        sObj = new DatesSelectionDTO((<DatesSelectionDTO>s).dates);
                        break;
                    case 'DateRangeSelectionDTO':
                        sObj = new DateRangeSelectionDTO((<DateRangeSelectionDTO>s).rangeType, (<DateRangeSelectionDTO>s).from, (<DateRangeSelectionDTO>s).to, (<DateRangeSelectionDTO>s).useMinMaxIfEmpty, (<DateRangeSelectionDTO>s).id);
                        break;
                    case 'IdSelectionDTO':
                        sObj = new IdSelectionDTO((<IdSelectionDTO>s).id);
                        break;
                    case 'IdListSelectionDTO':
                        sObj = new IdListSelectionDTO((<IdListSelectionDTO>s).ids);
                        break;
                    //case 'MatrixColumnsSelectionDTO':
                    //    let columns: MatrixColumnSelectionDTO[] = [];
                    //    _.forEach((<MatrixColumnsSelectionDTO>s).columns, col => {
                    //        columns.push(new MatrixColumnSelectionDTO(col.field, col.sort, col.title, col.options));
                    //    });

                    //    sObj = new MatrixColumnsSelectionDTO(columns);
                    //    sObj.analysisMode = (<MatrixColumnsSelectionDTO>s).analysisMode;
                    //    sObj.insightId = (<MatrixColumnsSelectionDTO>s).insightId;
                    //    sObj.insightName = (<MatrixColumnsSelectionDTO>s).insightName;
                    //    sObj.chartType = (<MatrixColumnsSelectionDTO>s).chartType;
                    //    sObj.valueType = (<MatrixColumnsSelectionDTO>s).valueType;
                    //    break;
                    //case 'EmployeeSelectionDTO':
                    //    sObj = new EmployeeSelectionDTO((<EmployeeSelectionDTO>s).employeeIds, (<EmployeeSelectionDTO>s).accountIds, (<EmployeeSelectionDTO>s).categoryIds, (<EmployeeSelectionDTO>s).employeeGroupIds, (<EmployeeSelectionDTO>s).payrollGroupIds, (<EmployeeSelectionDTO>s).vacationGroupIds, (<EmployeeSelectionDTO>s).includeInactive, (<EmployeeSelectionDTO>s).onlyInactive, (<EmployeeSelectionDTO>s).includeEnded, (<EmployeeSelectionDTO>s).accountingType);
                    //    break;
                    //case 'PayrollProductRowSelectionDTO':
                    //    sObj = new PayrollProductRowSelectionDTO(s.key, (<PayrollProductRowSelectionDTO>s).sysPayrollTypeLevel1, (<PayrollProductRowSelectionDTO>s).sysPayrollTypeLevel2, (<PayrollProductRowSelectionDTO>s).sysPayrollTypeLevel3, (<PayrollProductRowSelectionDTO>s).sysPayrollTypeLevel4, (<PayrollProductRowSelectionDTO>s).payrollProductIds);
                    //    break;
                    //case 'UserDataSelectionDTO':
                    //    sObj = new UserDataSelectionDTO((<UserDataSelectionDTO>s).ids, (<UserDataSelectionDTO>s).includeInactive);
                    //    break;
                    default:
                        sObj = new ReportDataSelectionDTO();
                        break;
                }

                angular.extend(sObj, s);
                return sObj;
            });
        }
    }
}

export class UserSelectionAccessDTO implements IUserSelectionAccessDTO {
    created: Date;
    createdBy: string;
    messageGroupId: number;
    modified: Date;
    modifiedBy: string;
    roleId: number;
    state: SoeEntityState;
    type: TermGroup_ReportUserSelectionAccessType;
    userSelectionAccessId: number;
    userSelectionId: number;
}
