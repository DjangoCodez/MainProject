import { IUserAgentClientInfoDTO } from "../../Scripts/TypeLite.Net4";

export class UserAgentClientInfoDTO implements IUserAgentClientInfoDTO {
    data: string;
    deviceBrand: string;
    deviceFamily: string;
    deviceModel: string;
    osFamily: string;
    osVersion: string;
    userAgentFamily: string;
    userAgentVersion: string;

    // Extensions

    public get hasData(): boolean {
        return this.data && this.data.length > 0;
    }

    public get deviceString(): string {
        return "{0} {1} {2}".format(this.deviceBrand, this.deviceFamily, this.deviceModel).trim();
    }

    public get osString(): string {
        return "{0} {1}".format(this.osFamily, this.osVersion);
    }

    public get uaString(): string {
        return "{0} {1}".format(this.userAgentFamily, this.userAgentVersion);
    }
}