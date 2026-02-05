import { Component, input } from '@angular/core';
import { LabelComponent } from '@ui/label/label.component';

@Component({
  selector: 'soe-toolbar-label',
  imports: [LabelComponent],
  templateUrl: './toolbar-label.component.html',
  styleUrl: './toolbar-label.component.scss',
})
export class ToolbarLabelComponent {
  key = input('');
  labelKey = input('');
  labelLowercase = input(false);
  labelCentered = input(false);
  secondaryLabelKey = input('');
  secondaryLabelBold = input(false);
  secondaryLabelParantheses = input(true);
  secondaryLabelPrefixKey = input('');
  secondaryLabelPostfixKey = input('');
  secondaryLabelLowercase = input(false);
  labelClass = input('');
  labelValue = input('');
  tooltipKey = input('');
  hidden = input(false);
}
