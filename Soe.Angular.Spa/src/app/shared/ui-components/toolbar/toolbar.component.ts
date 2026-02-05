import {
  Component,
  input,
  output,
  ElementRef,
  signal,
  AfterViewInit,
  viewChild,
} from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SoeFormGroup } from '@shared/extensions';
import { IconModule } from '@ui/icon/icon.module';
import {
  NavigatorRecordConfig,
  RecordNavigatorComponent,
} from '@ui/record-navigator/record-navigator.component';
import { ToolbarGroups, ToolbarItemGroupConfig } from './models/toolbar';
import { ToolbarItemGroupComponent } from './toolbar-item-group/toolbar-item-group.component';
import { TranslatePipe } from '@ngx-translate/core';
import { BrowserUtil } from '@shared/util/browser-util';

@Component({
  selector: 'soe-toolbar',
  imports: [
    RecordNavigatorComponent,
    IconModule,
    ToolbarItemGroupComponent,
    TranslatePipe,
  ],
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss'],
})
export class ToolbarComponent implements AfterViewInit {
  itemGroups = input<ToolbarItemGroupConfig[]>([]);
  toolbarGroups = input<ToolbarGroups[]>([]);
  noPadding = input(false);
  noTopBottomPadding = input(false);
  noMargin = input(false);
  noBorder = input(false);

  leftContent = viewChild<ElementRef>('leftContent');
  hasLeftContent = signal<boolean>(true);
  rightContent = viewChild<ElementRef>('rightContent');
  hasRightContent = signal<boolean>(true);

  // Record navigator
  recordConfig = input<NavigatorRecordConfig>(new NavigatorRecordConfig());
  form = input<SoeFormGroup | undefined>(undefined);
  navigatorRecordChanged = output<SmallGenericType>();

  ngAfterViewInit(): void {
    const hasLeftContent = BrowserUtil.elementHasContent(this.leftContent());
    const hasRightContent = BrowserUtil.elementHasContent(this.rightContent());
    setTimeout(() => {
      this.hasLeftContent.set(hasLeftContent);
      this.hasRightContent.set(hasRightContent);
    });
  }

  recordChanged(record: SmallGenericType): void {
    this.navigatorRecordChanged.emit(record);
  }
}
