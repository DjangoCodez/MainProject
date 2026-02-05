import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  DestroyRef,
  ElementRef,
  OnInit,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { ShortcutService } from '@core/services/shortcut.service';
import {
  IconName,
  IconPrefix,
  IconProp,
} from '@fortawesome/fontawesome-svg-core';
import { TranslatePipe } from '@ngx-translate/core';
import { IconUtil } from '@shared/util/icon-util';
import { IconModule } from '@ui/icon/icon.module';

export type SaveButtonBehaviour = 'save' | 'standard' | 'primary' | 'danger';

@Component({
  selector: 'soe-save-button',
  templateUrl: './save-button.component.html',
  styleUrls: [
    '../../../styles/shared-styles/shared-button-styles.scss',
    '../button/button.component.scss', // same styles as button component
  ],
  imports: [CommonModule, IconModule, TranslatePipe],
})
export class SaveButtonComponent implements OnInit, AfterViewInit {
  behaviour = input<SaveButtonBehaviour>('save');
  caption = input('core.save');
  tooltip = input('');
  iconPrefix = input<IconPrefix>('fal');
  iconName = input<IconName | undefined>();
  iconClass = input('');
  inline = input(false);
  disabled = input(false);
  dirty = input<boolean | undefined>(undefined);
  invalid = input(false);
  invalidTooltip = input('error.unabletosave_tooltip');
  disableKeyboardSave = input(false);
  inProgress = input(false);

  action = output<Event>();
  validationErrorsAction = output<Event>();

  isDisabled = computed(() => {
    return this.dirty() === false || this.inProgress() || this.disabled();
  });

  typeClass = computed(() => {
    return this.getTypeClass();
  });

  icon = signal<IconProp | undefined>(undefined);

  private shortcutService = inject(ShortcutService);

  constructor(
    private element: ElementRef,
    private destroyRef: DestroyRef
  ) {}

  ngOnInit() {
    if (this.iconName())
      this.icon.set(IconUtil.createIcon(this.iconPrefix(), this.iconName()));
  }

  ngAfterViewInit(): void {
    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['Control', 's'],
      e => this.handleKeyboardSave(e),
      false
    );
  }

  onAction(action: Event): void {
    this.action.emit(action);
  }

  onValidationErrorsAction(action: Event): void {
    this.validationErrorsAction.emit(action);
  }

  private handleKeyboardSave(e: KeyboardEvent) {
    if (
      this.dirty() &&
      !this.disableKeyboardSave() &&
      !this.disabled() &&
      !this.invalid() &&
      !this.inProgress()
    ) {
      this.onAction(e);
    }
  }

  private getTypeClass(): string {
    switch (this.behaviour()) {
      case 'standard':
        return 'btn btn-secondary';
      case 'save':
        return 'btn btn-primary';
      case 'danger':
        return 'btn btn-danger';
      default:
        return 'btn btn-primary'; // Default if no match
    }
  }
}
