import { PlanningShiftDTO } from './shift.model';

export class SpWeekSlot {
  start: Date;
  text = '';
  nbrOfDaysInWeek = 7; // Used for displaying correct width for partial weeks

  constructor(start: Date, text = '', nbrOfDaysInWeek = 7) {
    this.start = start;
    this.text = text;
    this.nbrOfDaysInWeek = nbrOfDaysInWeek;
  }
}

export class SpDaySlot {
  start: Date;
  text = '';
  tooltip = '';
  isToday = false;
  isSaturday = false;
  isSunday = false;
  needTime = 0;
  netTime = 0;
  factorTime = 0;
  workTime = 0;
  grossTime = 0;
  cost = 0;
  costIncEmpTaxAndSuppCharge = 0;

  constructor(start: Date, text = '') {
    this.start = start;
    this.text = text;

    this.isToday = start.isToday();
    this.isSaturday = start.isSaturday();
    this.isSunday = start.isSunday();
  }

  clearTimeAndCosts(): void {
    this.tooltip = '';
    this.needTime = 0;
    this.netTime = 0;
    this.factorTime = 0;
    this.workTime = 0;
    this.grossTime = 0;
    this.cost = 0;
    this.costIncEmpTaxAndSuppCharge = 0;
  }
}

export class SpSumDaySlot extends SpDaySlot {
  hourSlots: SpSumHourSlot[] = [];
  halfHourSlots: SpSumHalfHourSlot[] = [];
  quarterHourSlots: SpSumQuarterHourSlot[] = [];

  constructor(start: Date, text = '') {
    super(start, text);
  }
}

export class SpEmployeeDaySlot extends SpDaySlot {
  employeeId = 0;
  employeeInfo = '';
  shifts: PlanningShiftDTO[] = [];
  filteredShifts: PlanningShiftDTO[] = [];
  hourSlots: SpEmployeeHourSlot[] = [];
  halfHourSlots: SpEmployeeHalfHourSlot[] = [];
  quarterHourSlots: SpEmployeeQuarterHourSlot[] = [];
  isSelected = false;

  // Time and costs
  absenceTime = 0;

  // Availability
  isFullyAvailable = false;
  isFullyUnavailable = false;
  isPartiallyAvailable = false;
  isPartiallyUnavailable = false;
  comment = '';
  availableText = '';
  unavailableText = '';
  availabilityTooltip = '';

  get hasAvailability(): boolean {
    return this.isFullyAvailable || this.isPartiallyAvailable;
  }
  get hasUnavailability(): boolean {
    return this.isFullyUnavailable || this.isPartiallyUnavailable;
  }

  constructor(start: Date, employeeId: number, employeeInfo: string) {
    super(start);

    this.employeeId = employeeId;
    this.employeeInfo = employeeInfo;
  }

  setFilteredShifts(showHiddenShifts: boolean): void {
    this.filteredShifts = this.shifts.filter(
      shift => shift.isVisible || showHiddenShifts
    );
  }

  clearTimeAndCosts(): void {
    super.clearTimeAndCosts();
    this.absenceTime = 0;
  }
}

export class SpHourSlot {
  start: Date;
  text = '';
  tooltip = '';
  plannedMinutes = 0;
  nbrOfShifts = 0;

  constructor(start: Date, text = '') {
    this.start = start;
    this.text = text;
  }

  clearTimeAndCosts(): void {
    this.tooltip = '';
    this.plannedMinutes = 0;
    this.nbrOfShifts = 0;
  }
}

export class SpSumHourSlot extends SpHourSlot {
  constructor(start: Date, text = '') {
    super(start, text);
  }
}

export class SpEmployeeHourSlot extends SpHourSlot {
  employeeId = 0;
  employeeInfo = '';
  isSelected = false;

  // Availability
  isFullyAvailable = false;
  isFullyUnavailable = false;
  isPartiallyAvailable = false;
  isPartiallyUnavailable = false;
  comment = '';
  availableText = '';
  unavailableText = '';
  availabilityTooltip = '';

  get hasAvailability(): boolean {
    return this.isFullyAvailable || this.isPartiallyAvailable;
  }
  get hasUnavailability(): boolean {
    return this.isFullyUnavailable || this.isPartiallyUnavailable;
  }

  constructor(start: Date, employeeId: number, employeeInfo: string) {
    super(start);

    this.employeeId = employeeId;
    this.employeeInfo = employeeInfo;
  }
}

export class SpHalfHourSlot {
  start: Date;
  text = '';
  tooltip = '';
  plannedMinutes = 0;
  nbrOfShifts = 0;

  get isEndOfHour(): boolean {
    return this.start.getMinutes() === 30;
  }

  constructor(start: Date, text = '') {
    this.start = start;
    this.text = text;
  }

  clearTimeAndCosts(): void {
    this.tooltip = '';
    this.plannedMinutes = 0;
    this.nbrOfShifts = 0;
  }
}

export class SpSumHalfHourSlot extends SpHalfHourSlot {
  constructor(start: Date, text = '') {
    super(start, text);
  }
}

export class SpEmployeeHalfHourSlot extends SpHalfHourSlot {
  employeeId = 0;
  employeeInfo = '';
  isSelected = false;

  // Availability
  isFullyAvailable = false;
  isFullyUnavailable = false;
  isPartiallyAvailable = false;
  isPartiallyUnavailable = false;
  comment = '';
  availableText = '';
  unavailableText = '';
  availabilityTooltip = '';

  get hasAvailability(): boolean {
    return this.isFullyAvailable || this.isPartiallyAvailable;
  }
  get hasUnavailability(): boolean {
    return this.isFullyUnavailable || this.isPartiallyUnavailable;
  }

  constructor(start: Date, employeeId: number, employeeInfo: string) {
    super(start);

    this.employeeId = employeeId;
    this.employeeInfo = employeeInfo;
  }
}

export class SpQuarterHourSlot {
  start: Date;
  text = '';
  tooltip = '';
  plannedMinutes = 0;
  nbrOfShifts = 0;

  get isEndOfHour(): boolean {
    return this.start.getMinutes() === 45;
  }

  constructor(start: Date, text = '') {
    this.start = start;
    this.text = text;
  }

  clearTimeAndCosts(): void {
    this.tooltip = '';
    this.plannedMinutes = 0;
    this.nbrOfShifts = 0;
  }
}

export class SpSumQuarterHourSlot extends SpQuarterHourSlot {
  constructor(start: Date, text = '') {
    super(start, text);
  }
}

export class SpEmployeeQuarterHourSlot extends SpQuarterHourSlot {
  employeeId = 0;
  employeeInfo = '';
  isSelected = false;

  // Availability
  isFullyAvailable = false;
  isFullyUnavailable = false;
  isPartiallyAvailable = false;
  isPartiallyUnavailable = false;
  comment = '';
  availableText = '';
  unavailableText = '';
  availabilityTooltip = '';

  get hasAvailability(): boolean {
    return this.isFullyAvailable || this.isPartiallyAvailable;
  }
  get hasUnavailability(): boolean {
    return this.isFullyUnavailable || this.isPartiallyUnavailable;
  }

  constructor(start: Date, employeeId: number, employeeInfo: string) {
    super(start);

    this.employeeId = employeeId;
    this.employeeInfo = employeeInfo;
  }
}
