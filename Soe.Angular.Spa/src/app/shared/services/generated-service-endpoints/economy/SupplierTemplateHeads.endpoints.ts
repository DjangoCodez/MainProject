


//Available methods for SupplierTemplateHeadsController

//get
export const getAttestWorkFlowTemplateHeadsForCompany = (entity: number) => `V2/Economy/SupplierAttestTemplateHeads/AttestWorkFlow/TemplateHeads/ForCurrentCompany/${entity}`;

//get
export const getAttestWorkFlowTemplateHeadRows = (templateHeadId: number) => `V2/Economy/SupplierAttestTemplateHeads/AttestWorkFlow/TemplateHeads/Rows/${templateHeadId}`;

//get
export const getAttestWorkFlowTemplateHeadRowsWithUser = (templateHeadId: number) => `V2/Economy/SupplierAttestTemplateHeads/AttestWorkFlow/TemplateHeads/Rows/User/${templateHeadId}`;

//get
export const getAttestWorkFlowUsersByAttestTransition = (attestTransitionId: number) => `V2/Economy/SupplierAttestTemplateHeads/AttestWorkFlow/Users/ByAttestTransition/${attestTransitionId}`;

//get
export const getAttestWorkFlowAttestRolesByAttestTransition = (attestTransitionId: number) => `V2/Economy/SupplierAttestTemplateHeads/AttestWorkFlow/AttestRoles/ByAttestTransition/${attestTransitionId}`;

//get
export const getAttestWorkFlowHead = (attestWorkFlowHeadId: number, setTypeName: boolean, loadRows: boolean) => `V2/Economy/SupplierAttestTemplateHeads/AttestWorkFlow/AttestWorkFlowHead/${attestWorkFlowHeadId}/${setTypeName}/${loadRows}`;

//get
export const getAttestWorkFlowHeadFromInvoiceId = (invoiceId: number, setTypeName: boolean, loadTemplate: boolean, loadRows: boolean, loadRemoved: boolean) => `V2/Economy/SupplierAttestTemplateHeads/AttestWorkFlow/HeadFromInvoiceId/${invoiceId}/${setTypeName}/${loadTemplate}/${loadRows}/${loadRemoved}`;

//post, takes args: (invoiceIds: number[])
export const getAttestWorkFlowHeadFromInvoiceIds = () => `V2/Economy/SupplierAttestTemplateHeads/AttestWorkFlow/HeadFromInvoiceIds`;

//get
export const getAttestWorkFlowRowsFromInvoiceId = (invoiceId: number) => `V2/Economy/SupplierAttestTemplateHeads/AttestWorkFlow/RowsFromInvoiceId/${invoiceId}`;


