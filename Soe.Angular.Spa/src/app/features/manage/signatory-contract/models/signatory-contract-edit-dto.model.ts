import { ISignatoryContractDTO } from "@shared/models/generated-interfaces/SignatoryContractDTO";
import { ISignatoryContractRecipientDTO } from "@shared/models/generated-interfaces/SignatoryContractRecipientDTO";


export class SignatoryContractDTO implements ISignatoryContractDTO {
    signatoryContractId: number;
    actorCompanyId: number;
    parentSignatoryContractId?: number;
    signedByUserId: number;
    signedByUserName: string;
    recipientUserId: number;
    recipientUserName: string;
    recipients: ISignatoryContractRecipientDTO[];
    creationMethodType: number;
    canPropagate: boolean;
    revokedBy: string;
    revokedReason: string;
    created: Date;
    createdBy: string;
    revokedAtUTC?: Date;
    revokedAt?: Date;
    requiredAuthenticationMethodType: number;
    permissionTypes: number[];
    permissionNames: string[];
    permissions: string;
    subContracts: ISignatoryContractDTO[];

    constructor() {
        this.signatoryContractId = 0;
        this.actorCompanyId = 0;
        this.parentSignatoryContractId = undefined;
        this.signedByUserId = 0;
        this.signedByUserName = '';
        this.recipientUserId = 0;
        this.recipientUserName = '';
        this.recipients = [];
        this.creationMethodType = 0;
        this.canPropagate = false;
        this.revokedBy = '';
        this.revokedReason = '';
        this.created = new Date();
        this.createdBy = '';
        this.revokedAtUTC = undefined;
        this.revokedAt = undefined;
        this.requiredAuthenticationMethodType = 0;
        this.permissionTypes = [];
        this.permissionNames = [];
        this.permissions = '';
        this.subContracts = [];
    }
}

