import { IEmailTemplateDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class EmailTemplateDTO implements IEmailTemplateDTO {
  emailTemplateId!: number;
  actorCompanyId!: number;
  type!: number;
  name!: string;
  subject!: string;
  body!: string;
  typename!: string;
  bodyIsHTML!: boolean;
}
