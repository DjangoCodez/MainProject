import { Component, input, output, signal } from '@angular/core';
import { MatExpansionModule } from '@angular/material/expansion';
import { TranslatePipe } from '@ngx-translate/core';
import { IconModule } from '@ui/icon/icon.module'
import { LabelComponent } from '@ui/label/label.component';

@Component({
  selector: 'soe-expansion-panel',
  imports: [MatExpansionModule, TranslatePipe, IconModule, LabelComponent],
  templateUrl: './expansion-panel.component.html',
  styleUrls: ['./expansion-panel.component.scss'],
})
export class ExpansionPanelComponent {
  labelKey = input('');
  labelClass = input('');
  secondaryLabelKey = input('');
  secondaryLabelBold = input(false);
  secondaryLabelParantheses = input(true);
  secondaryLabelPrefixKey = input('');
  secondaryLabelPostfixKey = input('');
  description = input('');
  disabled = input(false);
  open = input(false);
  static = input(false);
  borderless = input(false);
  noMargin = input(false);
  noPadding = input(false);
  doPadding = input(false);
  noTopPadding = input(false);
  addTopMargin = input(false);
  noBottomMargin = input(false);
  condensedBody = input(false);

  isOpen = signal(false);

  isOpened = output<boolean>();

  onOpened(): void {
    this.isOpen.set(true);
    this.isOpened.emit(true);
  }

  onClosed(): void {
    this.isOpen.set(false);
    this.isOpened.emit(false);
  }
}
