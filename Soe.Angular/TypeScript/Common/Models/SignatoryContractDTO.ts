import { SignatoryContractAuthenticationMethodType, TermGroup_SignatoryContractPermissionType } from "../../Util/CommonEnumerations";

export class GetPermissionResultDTO
{
    permissionType: TermGroup_SignatoryContractPermissionType = TermGroup_SignatoryContractPermissionType.Unknown;
    permissionLabel = ""
    hasPermission: boolean = false;
    isAuthorized: boolean = false;
    isAuthenticated?: boolean | null = null;
    isAuthenticationRequired?: boolean | null = null;
    authenticationDetails?: AuthenticationDetailsDTO | null = null;
}

export class AuthenticationDetailsDTO
{
    authenticationRequestId: number;
    authenticationMethodType: SignatoryContractAuthenticationMethodType;
    message: string;
    validUntilUTC: Date;

    constructor(
        authenticationRequestId: number,
        authenticationMethodType: SignatoryContractAuthenticationMethodType,
        message: string,
        validUntilUTC: Date
    )
    {
        this.authenticationRequestId = authenticationRequestId;
        this.authenticationMethodType = authenticationMethodType;
        this.message = message;
        this.validUntilUTC = validUntilUTC;
    }
}

export class AuthenticationResponseDTO
{
    signatoryContractAuthenticationRequestId: number;
    username?: string | null = null;
    password?: string | null = null;
    code?: string | null = null;

    constructor(signatoryContractAuthenticationRequestId: number, params: {
        username?: string | null,
        password?: string | null,
        code?: string | null
    }) {
        this.signatoryContractAuthenticationRequestId = signatoryContractAuthenticationRequestId;
        this.username = params.username;
        this.password = params.password;
        this.code = params.code;
    }
}

export class AuthenticationResultDTO
{
    success: boolean;
    message: string;

    constructor(success: boolean, message: string)
    {
        this.success = success;
        this.message = message;
    }
}