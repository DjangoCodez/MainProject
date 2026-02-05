import { ValidatorFn, ValidationErrors } from '@angular/forms';
import { TermGroup_AccountDistributionTriggerType } from '@shared/models/generated-interfaces/Enumerations';
import { IAccountDistributionRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class AccountDistributionValidators {
  static periodAccountingPeriod(): ValidatorFn {
    return (form): ValidationErrors | null => {
      const periodvalue = form.get('numberOfTimes')?.value;
      const triggerType = form.get('triggerType')?.value;

      if (
        triggerType === TermGroup_AccountDistributionTriggerType.Registration
      ) {
        if (
          periodvalue === null ||
          periodvalue === undefined ||
          periodvalue <= 0
        ) {
          const error: ValidationErrors = {};
          error['custom'] = {
            translationKey:
              'economy.accounting.accountdistribution.periodvalue',
          };
          return error;
        }
      }
      return null;
    };
  }

  static periodAccountingRowsDiff(): ValidatorFn {
    return (form): ValidationErrors | null => {
      const rows = form.get('rows')?.value as (
        | IAccountDistributionRowDTO
        | undefined
      )[];
      let same: number = 0;
      let opposit: number = 0;

      rows.forEach(row => {
        same += row?.sameBalance ?? 0;
        opposit += row?.oppositeBalance ?? 0;
      });

      if (same - opposit !== 0) {
        const error: ValidationErrors = {};
        error['custom'] = {
          translationKey: 'economy.accounting.accountdistribution.diffinrows',
        };
        return error;
      }
      return null;
    };
  }
}
