


//Available methods for EmailTemplatesController

//get
export const getEmailTemplates = (id?: number) => `V2/Core/EmailTemplates/EmailTemplates/?id=${id}`;

//get
export const getEmailTemplatesByType = (type: number) => `V2/Core/EmailTemplates/EmailTemplates/ByType/${type}`;

//get
export const getEmailTemplate = (emailTemplateId: number) => `V2/Core/EmailTemplates/EmailTemplate/${emailTemplateId}`;

//post, takes args: (emailTemplate: number)
export const saveEmailTemplate = () => `V2/Core/EmailTemplates/EmailTemplate`;

//delete
export const deleteEmailTemplate = (emailTemplateId: number) => `V2/Core/EmailTemplates/EmailTemplate/${emailTemplateId}`;


