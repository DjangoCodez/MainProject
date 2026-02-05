export class ClientGridDTO {
  sysMultiCompanyMappingId!: number;
  tcLicenseNr!: string;
  tcLicenseName!: string;
  tcName!: string;
  created!: Date;
  createdBy!: string;
}

export class SysMultiCompanyConnectionRequest {
  sysMultiCompanyConnectionRequestId!: number;
  code!: string;
  expiresAtUTC?: Date;
}

export class SysMultiCompanyConnectionRequestStatus {
  sysMultiCompanyConnectionRequestId!: number;
  linkedCompanyName!: string;
}
