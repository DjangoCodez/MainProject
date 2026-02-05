import { Signal, Type } from '@angular/core';
import { SoeFormGroup } from '@shared/extensions';
import { BehaviorSubject } from 'rxjs';
import {
  ActionTaken,
  CopyActionTaken,
  SetNewRefOnTab,
} from '@shared/directives/edit-base/edit-base.directive';
import { NavigatorRecordConfig } from '@ui/record-navigator/record-navigator.component';

export interface MultiTabWrapperEdit {
  ref: string;
  isNew: boolean;
  label: string;
  hideCloseTab?: boolean;
  component: Type<unknown>;
  gridIndex: number;
  inputs: {
    form?: SoeFormGroup;
    ref?: string;
    actionTakenSignal?: Signal<ActionTaken | undefined>;
    copyActionTakenSignal?: Signal<CopyActionTaken | undefined>;
    openEditInNewTabSignal?: Signal<OpenEditInNewTab | undefined>;
    setNewRefOnTabSignal?: Signal<SetNewRefOnTab | undefined>;
    addOptionId?: number;
  };
  recordConfig: NavigatorRecordConfig;
}

export interface MultiTabConfig {
  key?: string;
  gridComponent?: Type<unknown>;
  editComponent?: Type<unknown>;
  FormClass?: any;
  gridTabLabel?: string;
  editTabLabel?: string;
  createTabLabel?: string;
  rowData?: BehaviorSubject<unknown[]>;
  exportFilenameKey?: string;
  recordConfig?: NavigatorRecordConfig;
  hideForCreateTabMenu?: boolean;
  passGridDataOnAdd?: boolean;
  additionalGridProps?: any;
  disabled?: boolean;
  addOptions?: Array<{id: number, label: string}>;
}

export interface TabWrapperRowEdited<TGrid> {
  gridIndex: number;
  row: TGrid;
  rows: TGrid[];
  filteredRows: TGrid[];
  additionalProps: TabWrapperRowEditedAdditionalProps;
}

export type TabWrapperRowEditedAdditionalProps =
  | Partial<{
      editComponent: Type<unknown>;
      editLabel: string;
      label: string;
      gridData: any;
      gridIndex: number;
    }>
  | any;

export interface TabWrapperGridDataLoaded<TGrid> {
  gridIndex: number;
  rows: TGrid[];
}

export interface OpenEditInNewTab {
  id: number;
  additionalProps: OpenEditInNewTabAdditionalProps;
}

export type OpenEditInNewTabAdditionalProps =
  | Partial<{
      editComponent: Type<unknown>;
      FormClass: SoeFormGroup;
      editTabLabel: string;
      data: any;
    }>
  | any;
