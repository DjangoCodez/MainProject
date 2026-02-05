
import { Component, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SoeFormGroup } from '@shared/extensions';
import { ButtonComponent } from '@ui/button/button/button.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';

@Component({
  selector: 'soe-dialog-footer',
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    SaveButtonComponent
],
  templateUrl: './dialog-footer.component.html',
  styleUrl: './dialog-footer.component.scss',
})
export class DialogFooterComponent {
  form = input.required<SoeFormGroup>();

  hideCancel = input(false);
  cancelLabelKey = input('core.cancel');
  hideOk = input(false);
  okLabelKey = input('core.ok');
  okDisabled = input(false);

  cancelled = output<void>();
  committed = output<void>();

  onCancelled() {
    this.cancelled.emit();
  }

  onCommitted() {
    this.committed.emit();
  }
}
