import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeCodeRankingDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ITimeCodeRankingRowForm {
  validationHandler: ValidationHandler;
  element?: ITimeCodeRankingDTO;
}

export class TimeCodeRankingRowForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeCodeRankingRowForm) {
    super(validationHandler, {
      timeCodeRankingId: new SoeTextFormControl(
        element?.timeCodeRankingId || 0,
        {
          isIdField: true,
        }
      ),
      leftTimeCodeId: new SoeSelectFormControl(
        element?.leftTimeCodeId || 0,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'time.time.timecode.timecoderanking.unsocialhourscode'
      ),
      operatorType: new SoeSelectFormControl(element?.operatorType || 0, {}),
      leftTimeCodeName: new SoeTextFormControl(element?.leftTimeCodeName || ''),
      rightTimeCodeNames: new SoeTextFormControl(
        element?.rightTimeCodeNames || ([] as string[])
      ),
      rightTimeCodeIds: new SoeSelectFormControl(
        element?.rightTimeCodeIds || [],
        { required: true },
        'time.time.timecode.timecodes'
      ),
      operatorTypeInv: new SoeSelectFormControl('', { disabled: true }),
      rightTimeCodeIdsInv: new SoeTextFormControl([], {
        disabled: true,
      }),
    });
  }

  customPatchValue(element: ITimeCodeRankingDTO) {
    this.patchValue(element);
  }
}
