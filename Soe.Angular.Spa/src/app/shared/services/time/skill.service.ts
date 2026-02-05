import { inject, Injectable } from '@angular/core';
import { SoeHttpClient } from '../http.service';
import {
  employeeHasShiftTypeSkills,
  employeePostHasShiftTypeSkills,
  getEmployeePostSkills,
  getEmployeeSkills,
  matchEmployeesByShiftTypeSkills,
} from '../generated-service-endpoints/time/Skill.endpoints';
import { IEmployeeSkillDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { map, Observable, of, combineLatest, switchMap } from 'rxjs';
import { CoreService } from '../core.service';
import { CompanySettingType } from '@shared/models/generated-interfaces/Enumerations';
import { SettingsUtil } from '@shared/util/settings-util';
import { TranslateService } from '@ngx-translate/core';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';

@Injectable({
  providedIn: 'root',
})
export class SkillService {
  constructor(private http: SoeHttpClient) {}

  private readonly coreService = inject(CoreService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly translate = inject(TranslateService);

  // EmployeeSkills

  getEmployeeSkills(employeeId: number): Observable<IEmployeeSkillDTO[]> {
    return this.http.get<IEmployeeSkillDTO[]>(getEmployeeSkills(employeeId));
  }

  employeeHasShiftTypeSkills(
    employeeId: number,
    shiftTypeId: number,
    date: Date
  ): Observable<boolean> {
    return this.http.get<boolean>(
      employeeHasShiftTypeSkills(
        employeeId,
        shiftTypeId,
        date.toDateTimeString()
      )
    );
  }

  matchEmployeesByShiftTypeSkills(shiftTypeId: number): Observable<number[]> {
    return this.http.get<number[]>(
      matchEmployeesByShiftTypeSkills(shiftTypeId)
    );
  }

  // EmployeePostSkills

  getEmployeePostSkills(
    employeePostId: number
  ): Observable<IEmployeeSkillDTO[]> {
    return this.http.get<IEmployeeSkillDTO[]>(
      getEmployeePostSkills(employeePostId)
    );
  }

  employeePostHasShiftTypeSkills(
    employeePostId: number,
    shiftTypeId: number,
    date: Date
  ): Observable<boolean> {
    return this.http.get<boolean>(
      employeePostHasShiftTypeSkills(
        employeePostId,
        shiftTypeId,
        date.toDateTimeString()
      )
    );
  }

  showValidateSkillsResult(
    passed: boolean,
    shiftUndefinedText: string
  ): Observable<boolean> {
    if (passed) return of(true);

    return combineLatest([
      this.getOverrideSetting(),
      this.translate.get([
        'common.obs',
        'time.schedule.planning.editshift.missingskills',
        'time.schedule.planning.editshift.missingskillsoverride',
      ]),
    ]).pipe(
      switchMap(([canOverride, terms]) => {
        let message =
          terms['time.schedule.planning.editshift.missingskills'].format(
            shiftUndefinedText
          );

        // No override possible -> show error and return false
        if (!canOverride) {
          this.messageboxService.show(terms['common.obs'], message, {
            type: 'forbidden',
          });
          return of(false);
        }

        // Override possible -> ask user
        message +=
          '\n' +
          terms['time.schedule.planning.editshift.missingskillsoverride'];

        return this.messageboxService
          .question(terms['common.obs'], message, {
            type: 'warning',
            buttons: 'okCancel',
          })
          .afterClosed()
          .pipe(map(res => !!res.result));
      })
    );
  }

  private getOverrideSetting(): Observable<boolean> {
    return this.coreService
      .getCompanySettings([CompanySettingType.TimeSkillCantBeOverridden])
      .pipe(
        map(x => {
          return !SettingsUtil.getBoolCompanySetting(
            x,
            CompanySettingType.TimeSkillCantBeOverridden
          );
        })
      );
  }
}
