import { IUserDTO, System, IUserReplacementDTO, IUserSmallDTO, IUserGridDTO, IUserCompanyRoleDelegateHistoryHeadDTO, IUserCompanyRoleDelegateHistoryRowDTO, IUserCompanyRoleDelegateHistoryGridDTO, IUserCompanyRoleDelegateHistoryUserDTO, IUserAttestRoleDTO, IUserCompanyRoleDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, UserReplacementType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { UserAttestRoleDTO, UserCompanyRoleDTO } from "./EmployeeUserDTO";

export class UserDTO implements IUserDTO {
    blockedFromDate: Date;
    changePassword: boolean;
    defaultActorCompanyId: number;
    email: string;
    emailCopy: boolean;
    estatusLoginId: string;
    hasUserVerifiedEmail: boolean;
    idLoginGuid: System.IGuid;
    isAdmin: boolean;
    isMobileUser: boolean;
    isSuperAdmin: boolean;
    langId: number;
    licenseId: number;
    licenseNr: string;
    loginName: string;
    name: string;
    state: SoeEntityState;
    userId: number;

    public fixDates() {
        this.blockedFromDate = CalendarUtility.convertToDate(this.blockedFromDate);
    }
}

export class UserSmallDTO implements IUserSmallDTO {
    allowSupportLogin: boolean;
    attestFlowHasAnswered: boolean;
    attestFlowIsRequired: boolean;
    attestFlowRowId: number;
    attestRoleId: number;
    blockedFromDate: Date;
    categories: string;
    changePassword: boolean;
    defaultActorCompanyId: number;
    defaultRoleName: string;
    email: string;
    hideEditButton: boolean;
    idLoginActive: boolean;
    isSelected: boolean;
    isSelectedForEmail: boolean;
    langId: number;
    licenseId: number;
    licenseNr: string;
    loginName: string;
    main: boolean;
    name: string;
    state: SoeEntityState;
    userId: number;

    public fixDates() {
        this.blockedFromDate = CalendarUtility.convertToDate(this.blockedFromDate);
    }
}

export class UserGridDTO implements IUserGridDTO {

    externalAuthId: string;
    softOneIdLoginName: string;
    defaultActorCompanyId: number;
    defaultRoleName: string;
    email: string;
    idLoginActive: boolean;
    loginName: string;
    name: string;
    state: SoeEntityState;
    userId: number;

    // Extensions
    isActive: boolean;

    public get userInfo(): string {
        return `(${this.loginName}) ${this.name}`;
    }
}

export class UserReplacementDTO implements IUserReplacementDTO {
    actorCompanyId: number;
    originUserId: number;
    replacementUserId: number;
    startDate: Date;
    state: SoeEntityState;
    stopDate: Date;
    type: UserReplacementType;
    userReplacementId: number;

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
    }
}

export class UserCompanyRoleDelegateHistoryHeadDTO implements IUserCompanyRoleDelegateHistoryHeadDTO {
    actorCompanyId: number;
    byUserId: number;
    created: Date;
    createdBy: string;
    fromUserId: number;
    modified: Date;
    modifiedBy: string;
    rows: UserCompanyRoleDelegateHistoryRowDTO[];
    state: SoeEntityState;
    toUserId: number;
    userCompanyRoleDelegateHistoryHeadId: number;
}

export class UserCompanyRoleDelegateHistoryRowDTO implements IUserCompanyRoleDelegateHistoryRowDTO {
    accountId: number;
    attestRoleId: number;
    created: Date;
    createdBy: string;
    dateFrom: Date;
    dateTo: Date;
    modified: Date;
    modifiedBy: string;
    parentId: number;
    roleId: number;
    state: SoeEntityState;
    userCompanyRoleDelegateHistoryHeadId: number;
    userCompanyRoleDelegateHistoryRowId: number;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }
}

export class UserCompanyRoleDelegateHistoryGridDTO implements IUserCompanyRoleDelegateHistoryGridDTO {
    attestRoleNames: string;
    byUserId: number;
    byUserName: string;
    created: Date;
    dateFrom: Date;
    dateTo: Date;
    fromUserId: number;
    fromUserName: string;
    roleNames: string;
    showDelete: boolean;
    state: SoeEntityState;
    toUserId: number;
    toUserName: string;
    userCompanyRoleDelegateHistoryHeadId: number;

    // Extensions
    public get isDeleted(): boolean {
        return this.state === SoeEntityState.Deleted;
    }

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }
}

export class UserCompanyRoleDelegateHistoryUserDTO implements IUserCompanyRoleDelegateHistoryUserDTO {
    loginName: string;
    name: string;
    possibleTargetAttestRoles: UserAttestRoleDTO[];
    possibleTargetRoles: UserCompanyRoleDTO[];
    targetAttestRoles: UserAttestRoleDTO[];
    targetRoles: UserCompanyRoleDTO[];
    userId: number;

    public setTypes() {
        if (this.possibleTargetRoles) {
            this.possibleTargetRoles = this.possibleTargetRoles.map(r => {
                let rObj = new UserCompanyRoleDTO();
                angular.extend(rObj, r);
                rObj.fixDates();
                return rObj;
            });
        } else {
            this.possibleTargetRoles = [];
        }

        if (this.possibleTargetAttestRoles) {
            this.possibleTargetAttestRoles = this.possibleTargetAttestRoles.map(r => {
                let rObj = new UserAttestRoleDTO();
                angular.extend(rObj, r);
                rObj.fixDates();

                if (rObj.children) {
                    rObj.children = rObj.children.map(c => {
                        let cObj = new UserAttestRoleDTO();
                        angular.extend(cObj, c);
                        cObj.fixDates();
                        return cObj;
                    });
                }
                return rObj;
            });
        } else {
            this.possibleTargetAttestRoles = [];
        }

        if (this.targetRoles) {
            this.targetRoles = this.targetRoles.map(r => {
                let rObj = new UserCompanyRoleDTO();
                angular.extend(rObj, r);
                rObj.fixDates();
                return rObj;
            });
        } else {
            this.targetRoles = [];
        }

        if (this.targetAttestRoles) {
            this.targetAttestRoles = this.targetAttestRoles.map(r => {
                let rObj = new UserAttestRoleDTO();
                angular.extend(rObj, r);
                rObj.fixDates();

                if (rObj.children) {
                    rObj.children = rObj.children.map(c => {
                        let cObj = new UserAttestRoleDTO();
                        angular.extend(cObj, c);
                        cObj.fixDates();
                        return cObj;
                    });
                }
                return rObj;
            });
        } else {
            this.targetAttestRoles = [];
        }
    }
}
