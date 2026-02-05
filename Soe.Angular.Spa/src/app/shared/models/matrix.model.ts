import {
  MatrixDataType,
  TermGroup_MatrixDateFormatOption,
  TermGroup_MatrixGroupAggOption,
} from './generated-interfaces/Enumerations';
import {
  IMatrixDefinitionColumn,
  IMatrixDefinitionColumnOptions,
  IMatrixLayoutColumn,
} from './generated-interfaces/MatrixResult';

export class MatrixDefinitionColumn implements IMatrixDefinitionColumn {
  columnNumber: number;
  key: string;
  matrixDataType: MatrixDataType;
  field: string;
  title: string;
  options: IMatrixDefinitionColumnOptions;
  matrixLayoutColumn: IMatrixLayoutColumn;

  constructor() {
    this.columnNumber = 0;
    this.key = '';
    this.matrixDataType = MatrixDataType.String;
    this.field = '';
    this.title = '';
    this.options = new MatrixDefinitionColumnOptions();
    this.matrixLayoutColumn = new MatrixLayoutColumn();
  }
}

export class MatrixLayoutColumn implements IMatrixLayoutColumn {
  matrixDataType: MatrixDataType;
  field: string;
  title: string;
  sort: number;
  visible: boolean;
  options: IMatrixDefinitionColumnOptions;

  constructor() {
    this.matrixDataType = MatrixDataType.String;
    this.field = '';
    this.title = '';
    this.sort = 0;
    this.visible = true;
    this.options = new MatrixDefinitionColumnOptions();
  }
}

export class MatrixDefinitionColumnOptions
  implements IMatrixDefinitionColumnOptions
{
  aggregate: boolean;
  alignLeft: boolean;
  alignRight: boolean;
  changed: boolean;
  clearZero: boolean;
  dateFormatOption: TermGroup_MatrixDateFormatOption;
  decimals: number;
  groupBy: boolean;
  groupOption: TermGroup_MatrixGroupAggOption;
  hidden: boolean;
  key: string;
  labelPostValue: string;
  minutesToDecimal: boolean;
  minutesToTimeSpan: boolean;
  formatTimeWithSeconds: boolean;
  formatTimeWithDays: boolean;

  constructor() {
    this.changed = false;
    this.hidden = false;
    this.key = '';
    this.alignLeft = false;
    this.alignRight = false;
    this.clearZero = false;
    this.decimals = 0;
    this.minutesToDecimal = false;
    this.minutesToTimeSpan = false;
    this.formatTimeWithSeconds = false;
    this.formatTimeWithDays = false;
    this.dateFormatOption = TermGroup_MatrixDateFormatOption.DateShort;
    this.labelPostValue = '';
    this.groupBy = false;
    this.aggregate = false;
    this.groupOption = TermGroup_MatrixGroupAggOption.Sum;
  }
}
