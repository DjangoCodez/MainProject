import { ValidationHandler } from '@shared/handlers';
import { SysLogDTO } from './support-logs.model';
import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';

interface ISupportLogsForm {
  validationHandler: ValidationHandler;
  element: SysLogDTO | undefined;
}

export class SupportLogsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISupportLogsForm) {
    super(validationHandler, {
      sysLogId: new SoeTextFormControl(element?.sysLogId || 0, {
        isIdField: true,
        isNameField: true,
      }),
      date: new SoeTextFormControl(element?.date || ''),
      dateStr: new SoeTextFormControl(element?.dateStr || ''),
      level: new SoeTextFormControl(element?.level || ''),
      message: new SoeTextFormControl(element?.message || ''),
      exception: new SoeTextFormControl(element?.exception || ''),
      licenseId: new SoeTextFormControl(element?.licenseId || ''),
      licenseNr: new SoeTextFormControl(element?.licenseNr || ''),
      actorCompanyId: new SoeTextFormControl(element?.actorCompanyId || ''),
      companyName: new SoeTextFormControl(element?.companyName || ''),
      roleId: new SoeTextFormControl(element?.roleId || ''),
      roleName: new SoeTextFormControl(element?.roleName || ''),
      userId: new SoeTextFormControl(element?.userId || ''),
      loginName: new SoeTextFormControl(element?.loginName || ''),
      taskWatchLogId: new SoeTextFormControl(element?.taskWatchLogId || ''),
      taskWatchLogStart: new SoeTextFormControl(
        element?.taskWatchLogStart || ''
      ),
      taskWatchLogStop: new SoeTextFormControl(element?.taskWatchLogStop || ''),
      taskWatchLogName: new SoeTextFormControl(element?.taskWatchLogName || ''),
      taskWatchLogParameters: new SoeTextFormControl(
        element?.taskWatchLogParameters || ''
      ),
      recorId: new SoeTextFormControl(element?.recorId || ''),
      application: new SoeTextFormControl(element?.application || ''),
      from: new SoeTextFormControl(element?.from || ''),
      hostName: new SoeTextFormControl(element?.hostName || ''),
      ipNr: new SoeTextFormControl(element?.ipNr || ''),
      lineNumber: new SoeTextFormControl(element?.lineNumber || ''),
      logClass: new SoeTextFormControl(element?.logClass || ''),
      logger: new SoeTextFormControl(element?.logger || ''),
      referUri: new SoeTextFormControl(element?.referUri || ''),
      requestUri: new SoeTextFormControl(element?.requestUri || ''),
      session: new SoeTextFormControl(element?.session || ''),
      source: new SoeTextFormControl(element?.source || ''),
      targetSite: new SoeTextFormControl(element?.targetSite || ''),
      thread: new SoeTextFormControl(element?.thread || ''),
    });
  }
}
