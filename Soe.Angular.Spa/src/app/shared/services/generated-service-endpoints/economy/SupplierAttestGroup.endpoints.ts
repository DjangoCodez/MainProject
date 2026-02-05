


//Available methods for SupplierAttestGroupController

//get
export const getAttestWorkFlowGroupsDict = (addEmptyRow: boolean) => `V2/Economy/SupplierAttestGroup/AttestWorkFlow/AttestGroup/?addEmptyRow=${addEmptyRow}`;

//get
export const getAttestWorkFlowGroups = (addEmptyRow: boolean, attestWorkFlowHeadId?: number) => `V2/Economy/SupplierAttestGroup/AttestWorkFlow/AttestGroups/?addEmptyRow=${addEmptyRow}&attestWorkFlowHeadId=${attestWorkFlowHeadId}`;

//get
export const getAttestWorkFlowGroup = (id: number) => `V2/Economy/SupplierAttestGroup/AttestWorkFlow/AttestGroup/ById/${id}`;

//post, takes args: (model: number)
export const getAttestGroupSuggestion = () => `V2/Economy/SupplierAttestGroup/AttestWorkFlow/AttestGroup/Suggestion`;

//post, takes args: (head: number)
export const saveAttestWorkFlow = () => `V2/Economy/SupplierAttestGroup/AttestWorkFlow/AttestGroup/SaveAttestWorkFlow`;

//post, takes args: (model: number)
export const saveAttestWorkFlowMultiple = () => `V2/Economy/SupplierAttestGroup/AttestWorkFlow/AttestGroup/SaveAttestWorkFlowMultiple`;

//post, takes args: (model: number)
export const saveAttestWorkFlowForInvoices = () => `V2/Economy/SupplierAttestGroup/AttestWorkFlow/AttestGroup/SaveAttestWorkFlowForInvoices`;

//delete
export const deleteAttestWorkFlow = (attestWorkFlowHeadId: number) => `V2/Economy/SupplierAttestGroup/AttestWorkFlow/AttestGroup/DeleteAttestWorkFlow/${attestWorkFlowHeadId}`;

//delete
export const deleteAttestWorkFlows = (attestWorkFlowHeadIds: string) => `V2/Economy/SupplierAttestGroup/AttestWorkFlow/AttestGroup/DeleteAttestWorkFlows/${encodeURIComponent(attestWorkFlowHeadIds)}`;


