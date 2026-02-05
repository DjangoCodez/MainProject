import {
  AfterViewInit,
  Binding,
  Component,
  EnvironmentInjector,
  ViewChild,
  ViewContainerRef,
  inject,
  input,
  inputBinding,
  outputBinding,
} from '@angular/core';
import { IToolbarItemConfig } from '../models/toolbar';

import {
  ToolbarButtonAction,
  ToolbarButtonComponent,
} from '../toolbar-button/toolbar-button.component';
import {
  ToolbarCheckboxAction,
  ToolbarCheckboxComponent,
} from '../toolbar-checkbox/toolbar-checkbox.component';
import { ToolbarService } from '../services/toolbar.service';
import {
  ToolbarDatepickerAction,
  ToolbarDatepickerComponent,
} from '../toolbar-datepicker/toolbar-datepicker.component';
import {
  ToolbarSelectAction,
  ToolbarSelectComponent,
} from '../toolbar-select/toolbar-select.component';
import {
  ToolbarMenuButtonAction,
  ToolbarMenuButtonComponent,
} from '../toolbar-menu-button/toolbar-menu-button.component';
import {
  ToolbarDaterangepickerAction,
  ToolbarDaterangepickerComponent,
} from '../toolbar-daterangepicker/toolbar-daterangepicker.component';
import { ToolbarLabelComponent } from '../toolbar-label/toolbar-label.component';

@Component({
  selector: 'soe-toolbar-item-group',
  imports: [],
  templateUrl: './toolbar-item-group.component.html',
  styleUrl: './toolbar-item-group.component.scss',
})
export class ToolbarItemGroupComponent implements AfterViewInit {
  alignLeft = input(false);
  items = input<IToolbarItemConfig[]>([]);

  @ViewChild('groupContainer', { read: ViewContainerRef })
  groupContainer!: ViewContainerRef;

  envInjector = inject(EnvironmentInjector);

  toolbarService = inject(ToolbarService);

  ngAfterViewInit(): void {
    this.items().forEach(item => {
      // Setup bindings
      const bindings: Binding[] = this.setInputBindings(item);
      bindings.push(...this.setOutputBindings(item));

      // Create component
      const comp = this.groupContainer.createComponent(item.component, {
        environmentInjector: this.envInjector,
        bindings: bindings,
      });
    });
  }

  private setInputBindings(item: IToolbarItemConfig): Binding[] {
    const bindings: Binding[] = [];

    bindings.push(inputBinding('key', () => item.key));

    //if (item.component !== ToolbarLabelComponent)
    bindings.push(...this.setCommonInputBindings(item));

    switch (item.component) {
      case ToolbarButtonComponent:
        bindings.push(...this.setCommonButtonInputBindings(item));

        if (item.buttonBehaviour)
          bindings.push(inputBinding('behaviour', item.buttonBehaviour));
        break;
      case ToolbarMenuButtonComponent:
        bindings.push(...this.setCommonButtonInputBindings(item));

        if (item.menuButtonBehaviour)
          bindings.push(inputBinding('behaviour', item.menuButtonBehaviour));
        if (item.variant) bindings.push(inputBinding('variant', item.variant));
        if (item.insideGroup)
          bindings.push(inputBinding('insideGroup', item.insideGroup));
        if (item.dropUp) bindings.push(inputBinding('dropUp', item.dropUp));
        if (item.dropLeft)
          bindings.push(inputBinding('dropLeft', item.dropLeft));
        if (item.hideDropdownArrow)
          bindings.push(
            inputBinding('hideDropdownArrow', item.hideDropdownArrow)
          );
        if (item.list) bindings.push(inputBinding('list', item.list));
        if (item.selectedListItem)
          bindings.push(
            inputBinding('selectedListItem', item.selectedListItem)
          );
        if (item.showSelectedItemIcon)
          bindings.push(
            inputBinding(
              'showSelectedItemIcon',
              item.showSelectedItemIcon || false
            )
          );
        if (item.initialSelectedItemId)
          bindings.push(
            inputBinding('initialSelectedItemId', item.initialSelectedItemId)
          );
        if (item.unselectItemAfterSelect)
          bindings.push(
            inputBinding(
              'unselectItemAfterSelect',
              item.unselectItemAfterSelect || false
            )
          );
        break;
      case ToolbarCheckboxComponent:
        bindings.push(...this.setCommonLabelInputBindings(item));

        if (item.checkboxBehaviour)
          bindings.push(inputBinding('behaviour', item.checkboxBehaviour));
        if (item.checked !== undefined)
          bindings.push(inputBinding('checked', item.checked));
        break;
      case ToolbarDatepickerComponent:
        bindings.push(...this.setCommonLabelInputBindings(item));
        bindings.push(...this.setCommonDatepickerInputBindings(item));

        if (item.initialDate)
          bindings.push(inputBinding('initialDate', item.initialDate));
        break;
      case ToolbarDaterangepickerComponent:
        bindings.push(...this.setCommonDatepickerInputBindings(item));

        if (item.labelKeyFrom)
          bindings.push(inputBinding('labelKeyFrom', item.labelKeyFrom));
        if (item.secondaryLabelKeyFrom)
          bindings.push(
            inputBinding('secondaryLabelKeyFrom', item.secondaryLabelKeyFrom)
          );
        if (item.secondaryLabelBoldFrom)
          bindings.push(
            inputBinding('secondaryLabelBoldFrom', item.secondaryLabelBoldFrom)
          );
        if (item.secondaryLabelParanthesesFrom)
          bindings.push(
            inputBinding(
              'secondaryLabelParanthesesFrom',
              item.secondaryLabelParanthesesFrom
            )
          );
        if (item.secondaryLabelPrefixKeyFrom)
          bindings.push(
            inputBinding(
              'secondaryLabelPrefixKeyFrom',
              item.secondaryLabelPrefixKeyFrom
            )
          );
        if (item.secondaryLabelPostfixKeyFrom)
          bindings.push(
            inputBinding(
              'secondaryLabelPostfixKeyFrom',
              item.secondaryLabelPostfixKeyFrom
            )
          );
        if (item.lastInPeriodFrom)
          bindings.push(
            inputBinding('lastInPeriodFrom', item.lastInPeriodFrom)
          );
        if (item.labelKeyTo)
          bindings.push(inputBinding('labelKeyTo', item.labelKeyTo));
        if (item.secondaryLabelKeyTo)
          bindings.push(
            inputBinding('secondaryLabelKeyTo', item.secondaryLabelKeyTo)
          );
        if (item.secondaryLabelBoldTo)
          bindings.push(
            inputBinding('secondaryLabelBoldTo', item.secondaryLabelBoldTo)
          );
        if (item.secondaryLabelParanthesesTo)
          bindings.push(
            inputBinding(
              'secondaryLabelParanthesesTo',
              item.secondaryLabelParanthesesTo
            )
          );
        if (item.secondaryLabelPrefixKeyTo)
          bindings.push(
            inputBinding(
              'secondaryLabelPrefixKeyTo',
              item.secondaryLabelPrefixKeyTo
            )
          );
        if (item.secondaryLabelPostfixKeyTo)
          bindings.push(
            inputBinding(
              'secondaryLabelPostfixKeyTo',
              item.secondaryLabelPostfixKeyTo
            )
          );
        if (item.lastInPeriodTo)
          bindings.push(inputBinding('lastInPeriodTo', item.lastInPeriodTo));
        if (item.description)
          bindings.push(inputBinding('description', item.description));
        if (item.separatorDash)
          bindings.push(inputBinding('separatorDash', item.separatorDash));
        if (item.autoAdjustRange)
          bindings.push(inputBinding('autoAdjustRange', item.autoAdjustRange));
        if (item.initialDates)
          bindings.push(inputBinding('initialDates', item.initialDates));
        if (item.deltaDays)
          bindings.push(inputBinding('deltaDays', item.deltaDays));
        if (item.offsetDaysOnStep)
          bindings.push(
            inputBinding('offsetDaysOnStep', item.offsetDaysOnStep)
          );
        break;
      case ToolbarLabelComponent:
        bindings.push(...this.setCommonLabelInputBindings(item));

        if (item.labelLowercase)
          bindings.push(inputBinding('labelLowercase', item.labelLowercase));
        if (item.labelCentered)
          bindings.push(inputBinding('labelCentered', item.labelCentered));
        if (item.labelClass)
          bindings.push(inputBinding('labelClass', item.labelClass));
        if (item.labelValue)
          bindings.push(inputBinding('labelValue', item.labelValue));
        if (item.tooltipKey)
          bindings.push(inputBinding('tooltipKey', item.tooltipKey));
        break;
      case ToolbarSelectComponent:
        bindings.push(...this.setCommonLabelInputBindings(item));

        if (item.width) bindings.push(inputBinding('width', item.width));
        if (item.items) bindings.push(inputBinding('items', item.items));
        if (item.optionIdField)
          bindings.push(inputBinding('optionIdField', item.optionIdField));
        if (item.optionNameField)
          bindings.push(inputBinding('optionNameField', item.optionNameField));
        if (item.selectedItem)
          bindings.push(inputBinding('selectedItem', item.selectedItem));
        if (item.selectedId)
          bindings.push(inputBinding('selectedId', item.selectedId));
        if (item.initialSelectedId)
          bindings.push(
            inputBinding('initialSelectedId', item.initialSelectedId)
          );
        break;
    }

    return bindings;
  }

  private setCommonInputBindings(item: IToolbarItemConfig): Binding[] {
    const bindings: Binding[] = [];
    if (item.disabled) bindings.push(inputBinding('disabled', item.disabled));
    if (item.hidden) bindings.push(inputBinding('hidden', item.hidden));
    return bindings;
  }

  private setCommonLabelInputBindings(item: IToolbarItemConfig): Binding[] {
    const bindings: Binding[] = [];
    if (item.labelKey) bindings.push(inputBinding('labelKey', item.labelKey));
    if (item.secondaryLabelKey)
      bindings.push(inputBinding('secondaryLabelKey', item.secondaryLabelKey));
    if (item.secondaryLabelBold)
      bindings.push(
        inputBinding('secondaryLabelBold', item.secondaryLabelBold)
      );
    if (item.secondaryLabelParantheses)
      bindings.push(
        inputBinding(
          'secondaryLabelParantheses',
          item.secondaryLabelParantheses
        )
      );
    if (item.secondaryLabelPrefixKey)
      bindings.push(
        inputBinding('secondaryLabelPrefixKey', item.secondaryLabelPrefixKey)
      );
    if (item.secondaryLabelPostfixKey)
      bindings.push(
        inputBinding('secondaryLabelPostfixKey', item.secondaryLabelPostfixKey)
      );
    return bindings;
  }

  private setCommonButtonInputBindings(item: IToolbarItemConfig): Binding[] {
    const bindings: Binding[] = [];
    if (item.caption) bindings.push(inputBinding('caption', item.caption));
    if (item.tooltip) bindings.push(inputBinding('tooltip', item.tooltip));
    if (item.iconPrefix)
      bindings.push(inputBinding('iconPrefix', item.iconPrefix));
    if (item.iconName) bindings.push(inputBinding('iconName', item.iconName));
    if (item.iconClass)
      bindings.push(inputBinding('iconClass', item.iconClass));
    return bindings;
  }

  private setCommonDatepickerInputBindings(
    item: IToolbarItemConfig
  ): Binding[] {
    const bindings: Binding[] = [];
    if (item.width) bindings.push(inputBinding('width', item.width));
    if (item.view) bindings.push(inputBinding('view', item.view));
    if (item.hideToday)
      bindings.push(inputBinding('hideToday', item.hideToday));
    if (item.hideClear)
      bindings.push(inputBinding('hideClear', item.hideClear));
    if (item.showArrows)
      bindings.push(inputBinding('showArrows', item.showArrows));
    if (item.hideCalendarButton)
      bindings.push(
        inputBinding('hideCalendarButton', item.hideCalendarButton)
      );
    if (item.minDate) bindings.push(inputBinding('minDate', item.minDate));
    if (item.maxDate) bindings.push(inputBinding('maxDate', item.maxDate));
    return bindings;
  }

  private setOutputBindings(item: IToolbarItemConfig): Binding[] {
    const bindings: Binding[] = [];

    switch (item.component) {
      case ToolbarButtonComponent:
        if (item.onAction) {
          bindings.push(
            outputBinding('onAction', (event: ToolbarButtonAction) =>
              item.onAction(event)
            )
          );
        }
        break;
      case ToolbarMenuButtonComponent:
        if (item.onItemSelected) {
          bindings.push(
            outputBinding('onItemSelected', (event: ToolbarMenuButtonAction) =>
              item.onItemSelected(event)
            )
          );
        }
        break;
      case ToolbarCheckboxComponent:
      case ToolbarDatepickerComponent:
      case ToolbarDaterangepickerComponent:
      case ToolbarSelectComponent:
        if (item.onValueChanged) {
          bindings.push(
            outputBinding(
              'onValueChanged',
              (
                event:
                  | ToolbarCheckboxAction
                  | ToolbarDatepickerAction
                  | ToolbarDaterangepickerAction
                  | ToolbarSelectAction
              ) => item.onValueChanged(event)
            )
          );
        }
        break;
    }

    return bindings;
  }
}
