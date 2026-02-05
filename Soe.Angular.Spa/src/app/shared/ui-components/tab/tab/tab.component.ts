import { Component, input, model, signal } from '@angular/core';

@Component({
  selector: 'soe-tab',
  templateUrl: './tab.component.html',
  styleUrls: ['./tab.component.scss'],
})
export class TabComponent {
  key = input<string | undefined>(undefined);
  label = input('');
  disabled = model(false);
  closable = input(true);
  isDirty = input(false);
  isNew = input(false);

  isActive = signal(false);
  doubleClickCount = signal(0);
}
