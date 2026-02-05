import { Component, OnInit, inject, signal } from '@angular/core';
import { FilesHelper } from '@shared/components/files-helper/files-helper.component';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  SoeDataStorageRecordType,
  SoeEntityType,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { DateRangeValue } from '@ui/forms/datepicker/daterangepicker/daterangepicker.component';
import {
  IMessageboxComponentResponse,
  MessageboxType,
} from '@ui/dialog/models/messagebox';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { NumberRangeValue } from '@ui/forms/numberbox/numberrange/numberrange.component';
import { TimeRangeValue } from '@ui/forms/timebox/timerange/timerange.component';
import { TimeboxValue } from '@ui/forms/timebox/timebox.component';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { ToolbarButtonAction } from '@ui/toolbar/toolbar-button/toolbar-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { UiComponentsTestForm } from '../../models/ui-components-test-form.model';
import { UiComponentsTestDTO } from '../../models/ui-components-test.model';
import { UiComponentsTestService } from '../../services/ui-components-test.service';
import { EditableGridTestDataDTO } from '../grid-test-components/editable-grid.component';
import { ToolbarCheckboxAction } from '@ui/toolbar/toolbar-checkbox/toolbar-checkbox.component';
import { ToolbarDatepickerAction } from '@ui/toolbar/toolbar-datepicker/toolbar-datepicker.component';
import { ToolbarDaterangepickerAction } from '@ui/toolbar/toolbar-daterangepicker/toolbar-daterangepicker.component';
import { ToolbarSelectAction } from '@ui/toolbar/toolbar-select/toolbar-select.component';

@Component({
  templateUrl: './ui-components-test.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class UiComponentsTestComponent<T>
  extends EditBaseDirective<
    UiComponentsTestDTO,
    UiComponentsTestService,
    UiComponentsTestForm
  >
  implements OnInit
{
  validationHandler = inject(ValidationHandler);
  service = inject(UiComponentsTestService);
  toasterService = inject(ToasterService);
  toolbarService = inject(ToolbarService);

  expansionPanelEnabled = signal<boolean>(false);
  expansionPanelDescription = signal<string>('Disabled');
  enableExpansionPanelButtonCaption = signal<string>('Enable panel');
  expansionPanelBorderlessLabel = signal<string>('Show more');

  autoadjustNumberrange = signal<boolean>(false);
  autoadjustTimerange = signal<boolean>(false);
  autoadjustTimerange2 = signal<boolean>(false);

  toolbarSelectItems: SmallGenericType[] = [];
  toolbarMenuButtonItems: MenuButtonItem[] = [];
  menuButtonList: MenuButtonItem[] = [];
  menuButtonList2: MenuButtonItem[] = [];
  splitButtonList: MenuButtonItem[] = [];

  private toolbarLabelLabelKey = signal<string>(
    'This is the initial label that can be updated'
  );
  private toolbarLabelTooltipKey = signal<string>('This is a label tooltip V2');
  private toolbarLabelLabelClass = signal<string>('ok-color');
  private toolbarCheckboxDisabled = signal<boolean>(true);
  private toolbarDatepickerDisabled = signal<boolean>(true);
  private toolbarDaterangepickerDisabled = signal<boolean>(true);
  private toolbarSelectDisabled = signal<boolean>(true);

  minDate = new Date().addMonths(-1);
  maxDate = new Date().addMonths(1);
  dates: Date[] = [];

  initialDaterangepickerDates: DateRangeValue = [
    DateUtil.getToday(),
    DateUtil.getToday().addDays(7),
  ];

  filesHelper!: FilesHelper;
  fileRecordId = 0;

  selectItems: any[] = [];
  multiSelectItems: any[] = [];
  multiSelectItems2: any[] = [];
  autocompleteItems: { id: number; name: string }[] = [];
  navigatorItems: Date[] = [];

  monthRangeFrom = <string>'';
  monthRangeTo = <string>'';

  sliderValue = signal(50);
  sliderValueStart = signal(25);
  sliderValueEnd = signal(75);

  constructor() {
    super();
    this.filesHelper = new FilesHelper(
      true,
      SoeEntityType.Voucher,
      SoeDataStorageRecordType.UploadedFile,
      Feature.Manage_Preferences_Registry_SchoolHoliday,
      this.performLoadData
    );
  }

  ngOnInit() {
    this.filesHelper.recordId.set(this.fileRecordId);
    this.form = this.createForm();
    this.createData();
    this.setDefaultValues();
    this.setupToolbar();
    this.loadFiles();
  }

  loadFiles() {
    if (this.filesHelper.filesLoaded()) {
      this.filesHelper.loadFiles(true);
    }
  }

  createForm(element?: UiComponentsTestDTO): UiComponentsTestForm {
    return new UiComponentsTestForm({
      validationHandler: this.validationHandler,
      element,
    });
  }

  private createData() {
    this.toolbarSelectItems.push({ id: 1, name: 'Item 1' });
    this.toolbarSelectItems.push({ id: 2, name: 'Item 2' });
    this.toolbarSelectItems.push({ id: 3, name: 'Item 3' });
    this.toolbarSelectItems.push({ id: 4, name: 'Item 4' });
    this.toolbarSelectItems.push({ id: 5, name: 'Item 5' });

    this.toolbarMenuButtonItems.push({ type: 'header', label: 'Sort by' });
    this.toolbarMenuButtonItems.push({
      id: 1,
      label: 'Number',
      icon: ['fal', 'sort-numeric-down'],
    });
    this.toolbarMenuButtonItems.push({
      id: 2,
      label: 'Name',
      icon: ['fal', 'sort-alpha-down'],
    });

    this.menuButtonList.push({
      id: 1,
      label: 'Edit',
      icon: ['fal', 'pencil'],
    });
    this.menuButtonList.push({
      id: 2,
      label: 'Save',
      icon: ['fal', 'floppy-disk'],
    });
    this.menuButtonList.push({ type: 'divider' });
    this.menuButtonList.push({ type: 'header', label: 'Group header 1' });
    this.menuButtonList.push({ id: 3, label: 'Sub Item 1' });
    this.menuButtonList.push({ id: 4, label: 'Sub Item 2' });
    this.menuButtonList.push({ id: 5, label: 'Sub Item 3' });
    this.menuButtonList.push({ type: 'header', label: 'Group header 2' });
    this.menuButtonList.push({
      id: 6,
      label: 'Cut',
      icon: ['fal', 'cut'],
    });
    this.menuButtonList.push({
      id: 7,
      label: 'Copy',
      icon: ['fal', 'copy'],
    });
    this.menuButtonList.push({
      id: 8,
      label: 'Paste',
      icon: ['fal', 'paste'],
    });

    this.menuButtonList2.push({ type: 'header', label: 'Sort by' });
    this.menuButtonList2.push({
      id: 1,
      label: 'Number',
      icon: ['fal', 'sort-numeric-down'],
    });
    this.menuButtonList2.push({
      id: 2,
      label: 'Name',
      icon: ['fal', 'sort-alpha-down'],
    });

    this.splitButtonList.push({ id: 1, label: 'Item 1' });
    this.splitButtonList.push({ id: 2, label: 'Item 2' });
    this.splitButtonList.push({ id: 3, label: 'Item 3' });
    this.splitButtonList.push({ type: 'divider' });
    this.splitButtonList.push({ id: 4, label: 'Item 4' });
    this.splitButtonList.push({ type: 'header', label: 'Group header' });
    this.splitButtonList.push({
      id: 5,
      label: 'Item 5',
      icon: ['fal', 'star'],
    });

    this.selectItems.push({ id: 1, name: 'Item 1' });
    this.selectItems.push({ id: 2, name: 'Item 2' });
    this.selectItems.push({ id: 3, name: 'Item 3' });
    this.selectItems.push({ id: 4, name: 'Item 4' });
    this.selectItems.push({ id: 5, name: 'Item 5' });

    this.multiSelectItems.push({ id: 1, name: 'Item 1' });
    this.multiSelectItems.push({ id: 2, name: 'Item 2' });
    this.multiSelectItems.push({ id: 3, name: 'Item 3' });
    this.multiSelectItems.push({ id: 4, name: 'Item 4' });
    this.multiSelectItems.push({ id: 5, name: 'Item 5' });
    this.multiSelectItems.push({ id: 6, name: 'Item 6' });
    this.multiSelectItems.push({ id: 7, name: 'Item 7' });
    this.multiSelectItems.push({ id: 8, name: 'Item 8' });
    this.multiSelectItems.push({ id: 9, name: 'Item 9' });
    this.multiSelectItems.push({ id: 10, name: 'Item 10' });

    const dayNames = DateUtil.getDayOfWeekNames(true);
    dayNames.forEach(dayName => {
      this.multiSelectItems2.push({
        id: dayName.id,
        name: `${dayName.name} (${dayName.id})`,
      });
    });

    this.autocompleteItems.push({ id: 1, name: 'Item A' });
    this.autocompleteItems.push({ id: 2, name: 'Item AA' });
    this.autocompleteItems.push({ id: 3, name: 'Item AAA' });
    this.autocompleteItems.push({ id: 4, name: 'Item B' });
    this.autocompleteItems.push({ id: 5, name: 'Item BB' });
    this.autocompleteItems.push({ id: 6, name: 'Item BBB' });

    this.navigatorItems.push(new Date('2023-12-01'));
    this.navigatorItems.push(new Date('2023-12-02'));
    this.navigatorItems.push(new Date('2023-12-03'));
    this.navigatorItems.push(new Date('2023-12-04'));
    this.navigatorItems.push(new Date('2023-12-05'));
  }

  private setDefaultValues() {
    const dto: UiComponentsTestDTO = new UiComponentsTestDTO();
    dto.check = true;
    dto.color = '#72acf8';
    dto.date = new Date();
    dto.dates = [];
    dto.daterange = [new Date(), new Date().addDays(3)];
    dto.menuSelectId = 1;
    dto.menu2SelectId = 1;
    dto.num = 23.18;
    dto.num2 = 356786;
    dto.numberrange = [1, 10];
    dto.radio = '1';
    dto.radio2 = '1';
    dto.radio3 = '1';
    dto.radio4 = '1';
    dto.selectId = 1;
    dto.swtch = true;
    dto.textarea = 'Large amount of text';
    dto.text = 'Text';
    dto.text2 = 'Text 2';
    dto.text3 = 'Password';
    dto.time = new Date();
    dto.timerange = [60, 120]; // 1 hour to 2 hours
    dto.timerange2 = [
      new Date('2025-01-01 10:00:00'),
      new Date('2025-01-01 12:00:00'),
    ];
    dto.duration = 250;

    dto.created = new Date().addDays(-2);
    dto.createdBy = 'SoftOne';
    dto.modified = new Date().addHours(-1).addMinutes(-20);
    dto.modifiedBy = 'SoftOne';
    dto.rows = [
      {
        id: 1,
        city: 'Stockholm',
        name2: 'Text1',
        timeFrom: new Date('2025-01-01 10:00:00'),
        timeTo: new Date('2025-01-01 12:00:00'),
        length: 120,
        itemId: 1,
        typeId: 2,
        isDefault: false,
        date: new Date('2025-01-01 10:00:00'),
        number: 1,
      } as EditableGridTestDataDTO,
      {
        id: 2,
        city: 'Söderhamn',
        name2: 'Text2',
        timeFrom: new Date('2025-01-01 08:00:00'),
        timeTo: new Date('2025-01-01 12:00:00'),
        length: 240,
        itemId: 2,
        typeId: 1,
        isDefault: true,
        date: new Date('2025-01-01 08:00:00'),
        number: 2,
      } as EditableGridTestDataDTO,
    ];

    this.form?.customPatchValue(dto);
  }

  private setupToolbar(): void {
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarButton('leftArrow', {
          iconName: signal('arrow-left'),
          onAction: () => this.onToolbarButtonLeftArrowClick(),
        }),
        this.toolbarService.createToolbarButton('rightArrow', {
          iconName: signal('arrow-right'),
          onAction: () => this.onToolbarButtonRightArrowClick(),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarLabel('label', {
          labelKey: this.toolbarLabelLabelKey,
          tooltipKey: this.toolbarLabelTooltipKey,
          labelClass: this.toolbarLabelLabelClass,
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('disable', {
          buttonBehaviour: signal('primary'),
          iconName: signal('pen-slash'),
          caption: signal('Disable'),
          tooltip: signal('Disable form'),
          onAction: () => this.onToolbarButtonDisableClick(),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarCheckbox('checkbox', {
          labelKey: signal('Check me'),
          disabled: this.toolbarCheckboxDisabled,
          onValueChanged: event => {
            this.onToolbarCheckboxClicked(
              (event as ToolbarCheckboxAction).value
            );
          },
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarSelect('select', {
          labelKey: signal('Select'),
          disabled: this.toolbarSelectDisabled,
          items: signal(this.toolbarSelectItems),
          onValueChanged: event =>
            this.onToolbarSelectClicked((event as ToolbarSelectAction).value),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarDatepicker('datepicker', {
          labelKey: signal('Select date'),
          disabled: this.toolbarDatepickerDisabled,
          onValueChanged: event =>
            this.onToolbarDatepickerClicked(
              (event as ToolbarDatepickerAction)?.value
            ),
        }),
      ],
    });

    setTimeout(() => {
      console.log('Enable some toolbar items');

      this.toolbarLabelLabelKey.set('This is an updated label');
      this.toolbarLabelTooltipKey.set('This is an updated label tooltip');
      this.toolbarLabelLabelClass.set('error-color');

      this.toolbarCheckboxDisabled.set(false);
      this.toolbarDatepickerDisabled.set(false);
      this.toolbarDaterangepickerDisabled.set(false);
      this.toolbarSelectDisabled.set(false);
    }, 5000);

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('button0', {
          iconName: signal('circle-0'),
          caption: signal('button0'),
          onAction: event => this.onToolbarButtonNumberClick(event),
        }),
        this.toolbarService.createToolbarButton('button1', {
          iconName: signal('circle-1'),
          caption: signal('button1'),
          onAction: event => this.onToolbarButtonNumberClick(event),
        }),
        this.toolbarService.createToolbarButton('button2', {
          iconName: signal('circle-2'),
          caption: signal('button2'),
          onAction: event => this.onToolbarButtonNumberClick(event),
        }),
        this.toolbarService.createToolbarButton('button3', {
          iconName: signal('circle-3'),
          caption: signal('button3'),
          onAction: event => this.onToolbarButtonNumberClick(event),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarMenuButton('menuButton', {
          variant: signal('menu'),
          menuButtonBehaviour: signal('secondary'),
          tooltip: signal('Menu button'),
          iconName: signal('arrow-down-short-wide'),
          showSelectedItemIcon: signal(true),
          hideDropdownArrow: signal(true),
          list: signal(this.toolbarMenuButtonItems),
          onItemSelected: event => this.onToolbarMenuButtonSelected(event),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarDaterangepicker('daterangepicker', {
          showArrows: signal(true),
          separatorDash: signal(true),
          initialDates: signal([
            DateUtil.getToday(),
            DateUtil.getToday().addDays(7),
          ]),
          disabled: signal(true),
          onValueChanged: event =>
            this.onToolbarDaterangepickerChanged(
              (event as ToolbarDaterangepickerAction)?.value
            ),
        }),
      ],
    });
  }

  private onToolbarButtonLeftArrowClick() {
    this.messageboxService.information(
      'Toolbar button clicked',
      'Arrow left button was clicked'
    );
    this.form?.enable();
  }

  private onToolbarButtonRightArrowClick() {
    this.messageboxService.information(
      'Toolbar button clicked',
      'Arrow right button was clicked'
    );
    this.form?.enable();
  }

  private onToolbarButtonDisableClick() {
    this.form?.disable();
  }

  private onToolbarButtonNumberClick(action: ToolbarButtonAction) {
    this.messageboxService.information(
      'Toolbar number button clicked',
      action?.key
        ? `Button with key '${action.key}' was pressed`
        : 'Button had no key, but the whole event is also passed (look in console)'
    );

    console.log(action);
  }

  private onToolbarMenuButtonSelected(event: any) {
    console.log(event);
  }

  private onToolbarCheckboxClicked(value: boolean) {
    this.messageboxService.information(
      'Toolbar checkbox clicked',
      `Checkbox value changed to: ${value}`
    );
  }

  private onToolbarDatepickerClicked(value: Date | undefined) {
    this.messageboxService.information(
      'Toolbar datepicker clicked',
      `Date changed to: ${value?.toFormattedDate()}`
    );
  }

  private onToolbarDaterangepickerChanged(value: DateRangeValue | undefined) {
    this.messageboxService.information(
      'Toolbar daterangepicker clicked',
      `Date range changed to: ${value?.[0]?.toFormattedDate()} - ${value?.[1]?.toFormattedDate()}`
    );
  }

  private onToolbarSelectClicked(value: number) {
    this.messageboxService.information(
      'Toolbar select clicked',
      `Selected value: ${this.toolbarSelectItems.find(x => x.id === value)?.name} (${value})`
    );
  }

  // ACTIONS

  enableExpansionPanel() {
    this.expansionPanelEnabled.set(!this.expansionPanelEnabled());
    this.expansionPanelDescription.set(
      this.expansionPanelEnabled() ? 'Enabled' : 'Disabled'
    );
    this.enableExpansionPanelButtonCaption.set(
      this.expansionPanelEnabled() ? 'Disable panel' : 'Enable panel'
    );
  }

  expansionPanelBorderlessOpened(open: boolean) {
    this.expansionPanelBorderlessLabel.set(open ? 'Show less' : 'Show more');
  }

  checkboxChanged(value: boolean) {
    console.log('Value changed to:', value);
  }

  colorChanged(hexValue: string) {
    console.log('Color changed to:', hexValue);
  }

  dateChanged(value: Date | undefined) {
    console.log('Date changed to:', value);
  }

  daterangeChanged(range: [Date | undefined, Date | undefined] | undefined) {
    console.log('Date range changed to:', range);
    console.log('test-form:', this.form);
  }

  daterangeValidityChanged(event: any) {
    console.log('Date range valid changed to:', event);
  }

  monthrangeChanged(range: [Date | undefined, Date | undefined] | undefined) {
    console.log('Month range changed to:', range);
    this.monthRangeFrom =
      range && DateUtil.isValidDate(<Date>range[0])
        ? DateUtil.format(<Date>range[0], DateUtil.dateFnsLanguageDateFormats)
        : '';

    this.monthRangeTo =
      range && DateUtil.isValidDate(<Date>range[1])
        ? DateUtil.format(<Date>range[1], DateUtil.dateFnsLanguageDateFormats)
        : '';
  }

  autoadjustNumberRangeChanged(value: boolean) {
    this.autoadjustNumberrange.set(value);
  }

  autoadjustTimeRangeChanged(value: boolean) {
    this.autoadjustTimerange.set(value);
  }

  autoadjustTimeRange2Changed(value: boolean) {
    this.autoadjustTimerange2.set(value);
  }

  openValidationErrors() {
    this.validationHandler.showFormValidationErrors(this.form as SoeFormGroup);
  }

  menuButtonSelected(selected: MenuButtonItem): void {
    this.messageboxService.information(
      'Menu button selected',
      `Item '${selected.label}' selected`
    );
  }

  menuButton2Selected(selected: MenuButtonItem): void {
    this.messageboxService.information(
      'Menu button selected',
      `Item '${selected.label}' selected`
    );
  }

  splitButtonSelected(selected: MenuButtonItem): void {
    if (selected) {
      this.messageboxService.information(
        'Split button selected',
        `Item '${selected.label}' selected`
      );
    }
  }

  numberboxChanged(value: number) {
    console.log('Number changed to:', value);
  }

  numberrangeChanged(range: NumberRangeValue | undefined) {
    console.log(range);
  }

  radioChanged(value: any) {
    console.log('Radio changed to:', value);
  }

  selectChanged(value: number) {
    console.log('Select changed to:', value);
  }

  multiSelectChanged(value: number[]) {
    console.log('MultiSelect changed to:', value);
  }

  switchChanged(value: boolean) {
    console.log('Switch changed to:', value);
  }

  timeChanged(value: TimeboxValue) {
    console.log('Time changed to:', value);
  }

  timeRangeChanged(value: TimeRangeValue | undefined) {
    console.log('Time range changed to:', value);
  }

  durationChanged(value: TimeboxValue) {
    console.log('Duration changed to:', value);
  }

  autocompleteChanged(value: { id: number; name: string }) {
    console.log('Autocomplete changed to:', value);
  }

  openInfoToaster() {
    this.toasterService.info(
      'Toaster of type information with a close button',
      'Information toaster',
      { closeButton: true }
    );
  }

  openWarningToaster() {
    this.toasterService.warning('Toaster of type warning', 'Warning toaster');
  }

  openErrorToaster() {
    this.toasterService.error('Toaster of type error', 'Error toaster');
  }

  openSuccessToaster() {
    this.toasterService.success('Toaster of type success', 'Success toaster');
  }

  openMessagebox(type: MessageboxType) {
    switch (type) {
      case 'information':
        this.messageboxService.information(
          'Information',
          'Message box of type information'
        );
        break;
      case 'warning':
        this.messageboxService.warning(
          'Warning',
          'Message box of type warning'
        );
        break;
      case 'error':
        this.messageboxService.error('Error', 'Message box of type error');
        break;
      case 'success':
        this.messageboxService.success(
          'Success',
          'Message box of type success'
        );
        break;
      case 'question':
        this.messageboxService.question(
          'Question',
          'Message box of type question with Yes/No buttons'
        );
        break;
      case 'questionAbort':
        this.messageboxService.questionAbort(
          'Question with abort',
          'Message box of type question with Yes/No/Cancel buttons'
        );
        break;
      case 'forbidden':
        this.messageboxService.error(
          'Forbidden',
          'Message box of type forbidden',
          {
            type: 'forbidden',
          }
        );
        break;
      case 'progress':
        const mb = this.messageboxService.progress(
          'Progress',
          'Message box of type progress with no buttons.\n\nClose button is also hidden.\nThis message box will automatically close after five seconds.\n\nIf you want to be able to dismiss the progress dialog, you can set the option enableCloseProgress.',
          { enableCloseProgress: false }
        );
        setTimeout(() => {
          mb.close();
        }, 5000);
        break;
      case 'custom':
        this.messageboxService.show(
          'Custom',
          'Message box of type custom.\nNote that the close button in the header is hidden.',
          {
            customIconName: 'space-station-moon-construction',
            hideCloseButton: true,
          }
        );
        break;
    }
  }

  openMessageboxHiddenText() {
    this.messageboxService.information(
      'Hidden text',
      'This message box contains hidden text.\n\nDouble click twice on the icon to reveal it.',
      {
        customIconName: 'user-secret',
        iconClass: 'text-color',
        hiddenText:
          'Hidden text can be useful to provide for more information in error messages or for other debugging purposes.',
      }
    );
  }

  openMessageboxTextInput(rows: number) {
    const mb = this.messageboxService.show(
      rows === 1 ? 'Text input' : 'Text area input',
      rows === 1
        ? 'Message box with a text input field'
        : 'Message box with a text area input field.\nIf inputTextRows is specified and is greater than 1, a text area input control will be displayed instead of the ordinary text input.',
      {
        customIconName: 'input-text',
        showInputText: true,
        inputTextLabel: rows === 1 ? 'Favorite color' : 'Tell me your story...',
        inputTextValue: rows === 1 ? 'Red' : '',
        inputTextRows: rows,
      }
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.data.inputTextRows === 1) {
        this.messageboxService.information(
          'Answer',
          'Your favorite color is ' + response.textValue.toLocaleLowerCase()
        );
      } else {
        this.messageboxService.error('No way!', 'You are kidding me!');
      }
    });
  }

  openMessageboxNumberInput() {
    const mb = this.messageboxService.show(
      'Number input',
      'Message box with a number input field',
      {
        customIconName: 'hashtag',
        showInputNumber: true,
        inputNumberLabel: 'What is the secret number?',
        inputNumberValue: 42,
        inputNumberDecimals: 2,
        inputNumberShowArrows: true,
      }
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      this.messageboxService.information(
        'Answer',
        `You entered the number ${response.numberValue}, that is unfortunately wrong...`
      );
    });
  }

  openMessageboxCheckboxInput() {
    const mb = this.messageboxService.show(
      'Checkbox input',
      'Message box with a checkbox input field',
      {
        customIconName: 'square-check',
        showInputCheckbox: true,
        inputCheckboxLabel: 'Do it or not?',
        inputCheckboxValue: false,
      }
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      this.messageboxService.information(
        'Answer',
        response.checkboxValue ? 'You are brave!' : 'Chicken!'
      );
    });
  }

  openMessageboxDateInput() {
    const mb = this.messageboxService.show(
      'Date input',
      'Message box with a date input field',
      {
        customIconName: 'calendar-days',
        showInputDate: true,
        inputDateLabel: 'When?',
        inputDateValue: new Date(),
      }
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      this.messageboxService.information(
        'Answer',
        'You´re up at ' + response.dateValue?.toFormattedDate()
      );
    });
  }

  showAttachedFile(file: AttachedFile) {
    console.log(file);
  }

  onSliderChanged(value: number) {
    this.sliderValue.set(value);
    console.log('Slider changed to:', value);
  }

  onSliderRangeChanged(range: { start: number; end: number }) {
    this.sliderValueStart.set(range.start);
    this.sliderValueEnd.set(range.end);
    console.log('Slider range changed to:', range.start, range.end);
  }
}
