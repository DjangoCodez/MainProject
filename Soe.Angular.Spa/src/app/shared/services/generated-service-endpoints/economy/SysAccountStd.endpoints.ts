


//Available methods for SysAccountStdController

//get
export const getSysAccountStd = (sysAccountStdTypeId: number, accountNr: string) => `V2/Economy/Account/SysAccountStd/${sysAccountStdTypeId}/${encodeURIComponent(accountNr)}`;

//get
export const copySysAccountStd = (sysAccountStdId: number) => `V2/Economy/Account/SysAccountStd/Copy/${sysAccountStdId}`;


