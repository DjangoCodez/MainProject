import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SignatoryContractAuthenticationMethodType, TermGroup_SignatoryContractPermissionType } from "../../../Util/CommonEnumerations";
import { AuthenticationDetailsDTO, AuthenticationResponseDTO } from "../../Models/SignatoryContractDTO";

export class SignatoryContractAuthenticationController {
    private hasPermission = true;
    private showPassword = false;
    private showCode = false;

    private username: string;
    private password: string;
    private code: string;

    private authenticationDetails: AuthenticationDetailsDTO = null;
    private message: string = null;
    private permissionLabel: string = "";
    private isBusy = false;
    private couldNotAuth = false;

    //@ngInject
    constructor(
        private permissionType: TermGroup_SignatoryContractPermissionType,
        private $uibModalInstance,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private progressService: IProgressHandler
    ) {
        this.onInit();
    }

    public onInit() {
        this.isBusy = true;
        this.progressService.startLoadingProgress([() => {
            return this.coreService.signatoryContractAuthorize(this.permissionType).then((result) => {
                this.isBusy = false;
                this.permissionLabel = `[${result.permissionLabel}]`;
                if (!result.hasPermission) {
                    return this.hasPermission = false;
                }
                if (result.isAuthorized) {
                    return this.onSuccess();
                }
                if (result.isAuthenticationRequired) {
                    this.authenticationDetails = result.authenticationDetails;
                    return this.setupAuthenticate();
                }
            });
        }])
    }

    private setupAuthenticate() {
        if (!this.authenticationDetails) return;

        switch (this.authenticationDetails.authenticationMethodType) {
            case SignatoryContractAuthenticationMethodType.Password:
                this.showPassword = true;
                break;
            case SignatoryContractAuthenticationMethodType.PasswordSMSCode:
                this.showPassword = true;
                this.showCode = true;
                break;
        }

        this.message = this.authenticationDetails.message;
        this.couldNotAuth = this.authenticationDetails.authenticationRequestId === 0;
    }

    public submit() {
        this.isBusy = true;
        const auth = new AuthenticationResponseDTO(this.authenticationDetails.authenticationRequestId, {
            username: this.username,
            password: this.password,
            code: this.code
        })
        this.progressService.startLoadingProgress([() => {
            return this.coreService.signatoryContractAuthenticate(auth).then((result) => {
                this.isBusy = false;
                if (result.success) {
                    return this.onSuccess();
                }

                this.message = result.message;
            })
        }])
    }

    public cancel() {
        this.$uibModalInstance.close({
            success: false,
        });
    }

    public onSuccess() {
        this.$uibModalInstance.close({
            success: true,
        });
    }
}