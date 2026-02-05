import {
  AfterContentInit,
  Component,
  ContentChildren,
  DestroyRef,
  ElementRef,
  OnChanges,
  OnDestroy,
  QueryList,
  SimpleChanges,
  computed,
  effect,
  inject,
  input,
  model,
  output,
  signal,
} from '@angular/core';
import { faPlus, faTimes, faAsterisk } from '@fortawesome/pro-light-svg-icons';
import { MessagingService } from '@shared/services/messaging.service';
import { fromEvent, of, Subject, Subscription } from 'rxjs';
import { debounceTime, delay, take, takeUntil } from 'rxjs/operators';
import { TabComponent } from '../tab/tab.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { IconModule } from '@ui/icon/icon.module';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ShortcutService } from '@core/services/shortcut.service';
import { Constants } from '@shared/util/client-constants';
import { SharedModule } from '@shared/shared.module';
import { UnsavedChangesDirective } from '@shared/directives/unsaved-changes/unsaved-changes.directive';

@Component({
  selector: 'soe-tab-group',
  imports: [SharedModule, UnsavedChangesDirective, IconModule],
  templateUrl: './tab-group.component.html',
  styleUrls: ['./tab-group.component.scss'],
})
export class TabGroupComponent
  implements AfterContentInit, OnChanges, OnDestroy
{
  activeIndex = model(0);
  hideAdd = input(false);
  hideCloseAll = input(false);
  preventMultipleNewTabs = input(false);
  disableKeyboardNew = input(false);

  preventNewTabs = computed<boolean>(() => {
    if (!this.preventMultipleNewTabs()) {
      return false;
    } else {
      return this.tabs.some(tab => tab.isNew() || false);
    }
  });

  tabIndexChanged = output<number>();
  tabAdded = output();
  tabRemoved = output<number>();
  allTabsRemoved = output();
  tabDblClicked = output<number>();

  @ContentChildren(TabComponent) tabs!: QueryList<TabComponent>;

  private messageboxService = inject(MessageboxService);

  subscriptionEnableTab$!: Subscription;
  subscriptionEnableTabAdd$!: Subscription;

  hasAddPermission = signal(false);
  disabledCloseAll = signal(true);

  private wasDeleteAction = false;
  private destroy$ = new Subject();
  private prevTabsLength = 0;
  private tabHistory: number[] = [];

  readonly faPlus = faPlus;
  readonly faTimes = faTimes;
  readonly faAsterisk = faAsterisk;

  hasScrollbar = signal<boolean>(false);

  private shortcutService = inject(ShortcutService);

  constructor(
    private messagingService: MessagingService,
    private element: ElementRef,
    private destroyRef: DestroyRef
  ) {
    this.setUpSubscriptions();

    effect(() => {
      const index = this.activeIndex();
      if (index !== this.peekTabHistory()) {
        this.addTabToHistory(index);
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    const { activeIndex } = changes;
    if (activeIndex) this.tabs && this.updateTabIndex();
  }

  ngAfterContentInit(): void {
    this.updateTabIndex();

    this.tabs.changes.pipe(takeUntil(this.destroy$)).subscribe(() => {
      of(true)
        .pipe(delay(0), take(1))
        .subscribe(() => {
          if (this.wasLastTabRemoved()) {
            this.tabWasDeleted(this.activeIndex());
            this.activeIndex.set(this.popTabHistory());
          } else {
            this.activeIndex.set(this.tabs.length - 1);
          }

          this.updateTabIndex();
          this.wasDeleteAction = false;

          this.disabledCloseAll.set(
            Array.from(this.tabs).every(tab => !tab.closable())
          );
          this.prevTabsLength = this.tabs.length;
          this.setHasScrollbar();
        });
    });

    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['Control', 'e'],
      () => this.handleKeyboardNew(),
      false
    );
  }

  wasLastTabRemoved(): boolean {
    return this.wasDeleteAction || this.tabs.length < this.prevTabsLength;
  }

  setUpSubscriptions() {
    fromEvent(window, 'resize')
      .pipe(takeUntil(this.destroy$), debounceTime(200))
      .subscribe(() => this.setHasScrollbar());

    this.subscriptionEnableTab$ = this.messagingService
      .onEvent(Constants.EVENT_ENABLE_TAB)
      .subscribe(message => {
        const key: string | undefined = message.data.key;
        const index: number | undefined = message.data.index;
        const enabled: boolean = message.data.enabled;

        if (key === undefined && index === undefined) {
          console.warn(
            'EnableTab message missing both key and index:',
            message
          );
          return;
        }

        const tab = key
          ? this.getTabByKey(key)
          : this.getTabByIndex(index || 0);
        if (tab) tab.disabled.set(!enabled);
      });

    this.subscriptionEnableTabAdd$ = this.messagingService
      .onEvent(Constants.EVENT_ENABLE_TAB_ADD)
      .subscribe(message => {
        let enabled: boolean = true;
        if (message && message.data != null)
          enabled = message.data.enabled || false;

        setTimeout(() => {
          if (!this.hideAdd()) this.hasAddPermission.set(enabled);
        }, 50);
      });
  }

  private getTabByIndex(index: number): TabComponent | undefined {
    return this.tabs && this.tabs.length > index
      ? this.tabs.get(index)
      : undefined;
  }

  private getTabByKey(key: string): TabComponent | undefined {
    return this.tabs && this.tabs.length > 0
      ? this.tabs.find(tab => tab.key() === key)
      : undefined;
  }

  selectTab(index: number): void {
    this.activeIndex.set(index);
    this.updateTabIndex();
  }

  addTab(): void {
    this.tabAdded.emit();
  }

  removeAllTabs(): void {
    const onConfirmCloseAllTabs = () => {
      this.clearTabHistory();
      this.activeIndex.set(0);
      this.allTabsRemoved.emit();
    };

    if (!this.tabs.some(tab => !!tab.isDirty())) {
      onConfirmCloseAllTabs();
      return;
    }

    const mb = this.messageboxService.warning(
      'core.warning',
      'core.confirmonclosetabs'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) onConfirmCloseAllTabs();
    });
  }

  removeTab($event: Event, index: number): void {
    const onConfirmCloseTab = () => {
      this.wasDeleteAction = true;
      $event.stopPropagation();
      this.tabRemoved.emit(index);
    };

    if (!this.tabs.get(index)?.isDirty()) {
      onConfirmCloseTab();
      return;
    }

    const mb = this.messageboxService.warning(
      'core.warning',
      'core.confirmonclosetab'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) onConfirmCloseTab();
    });
  }

  updateTabIndex(): void {
    setTimeout(() => {
      this.tabs.forEach((tab: TabComponent, index: number) => {
        tab.isActive.set(index === this.activeIndex());
      });
      this.tabIndexChanged.emit(this.activeIndex());
    });
  }

  tabDoubleClick(tab: TabComponent) {
    tab.doubleClickCount.set(tab.doubleClickCount() + 1);
    if (tab.doubleClickCount() >= 2) {
      tab.doubleClickCount.set(0);
      this.tabDblClicked.emit(this.activeIndex());
    }
  }

  handleKeyboardNew() {
    if (
      this.hasAddPermission() &&
      !this.disableKeyboardNew() &&
      !this.preventNewTabs()
    ) {
      this.addTab();
    }
  }

  private setHasScrollbar() {
    const tabsElem = document.getElementsByClassName('soe-tabs');
    this.hasScrollbar.set(
      tabsElem.length > 0
        ? tabsElem[0].scrollWidth > tabsElem[0].clientWidth
        : false
    );
  }

  private popTabHistory(): number {
    return this.tabHistory.pop() || 0;
  }
  private tabWasDeleted(index: number) {
    const newTabs = [];
    for (const i of this.tabHistory) {
      if (i > index) newTabs.push(i - 1);
      else if (i !== index) newTabs.push(i);
    }
    this.tabHistory = newTabs;
  }
  private peekTabHistory(): number {
    return this.tabHistory.length > 0
      ? this.tabHistory[this.tabHistory.length - 1]
      : 0;
  }
  private addTabToHistory(index: number): void {
    this.tabHistory.push(index);
    if (this.tabHistory.length > 50) {
      this.tabHistory.shift();
    }
  }
  private clearTabHistory(): void {
    this.tabHistory = [];
  }

  ngOnDestroy(): void {
    this.subscriptionEnableTab$?.unsubscribe();
    this.subscriptionEnableTabAdd$.unsubscribe();
    this.destroy$.complete();
    this.destroy$.unsubscribe();
  }
}
