import { IconName } from '@fortawesome/pro-light-svg-icons';

export type MessageboxSize = 'sm' | 'md' | 'lg' | 'xl' | 'fullscreen';
export type MessageboxType =
  | 'information'
  | 'warning'
  | 'error'
  | 'success'
  | 'question'
  | 'questionAbort'
  | 'forbidden'
  | 'progress'
  | 'custom';
export type MessageBoxButtons =
  | 'none'
  | 'ok'
  | 'okCancel'
  | 'yesNo'
  | 'yesNoCancel';

export interface IMessageboxComponentResponse {
  result?: boolean;
  textValue: string;
  numberValue: number;
  checkboxValue: boolean;
  dateValue?: Date;
  data: MessageboxData;
}

export interface MessageboxData {
  size?: MessageboxSize;
  title: string;
  hideCloseButton: boolean;
  enableCloseProgress: boolean;

  type: MessageboxType;
  customIconName: IconName;
  iconClass: string;

  text: string;
  hiddenText: string;

  showInputText: boolean;
  inputTextLabel: string;
  inputTextValue: string;
  inputTextRows: number;
  isPassword: boolean;

  showInputNumber: boolean;
  inputNumberLabel: string;
  inputNumberValue: number;
  inputNumberDecimals: number;
  inputNumberShowArrows: boolean;

  showInputCheckbox: boolean;
  inputCheckboxLabel: string;
  inputCheckboxValue: boolean;

  showInputDate: boolean;
  inputDateLabel: string;
  inputDateValue?: Date;

  buttons: MessageBoxButtons;
  buttonOkLabelKey: string;
  buttonYesLabelKey: string;
  buttonNoLabelKey: string;
  buttonCancelLabelKey: string;
}
