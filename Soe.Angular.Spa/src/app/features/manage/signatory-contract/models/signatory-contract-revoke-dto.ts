import { ISignatoryContractRevokeDTO } from "@shared/models/generated-interfaces/SignatoryContractRevokeDTO";

export class SignatoryContractRevokeDTO implements ISignatoryContractRevokeDTO{
    signatoryContractId: number;
    revokedReason: string;

    constructor() {
        this.signatoryContractId = 0;
        this.revokedReason = '';
    }
}
