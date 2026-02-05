import { Component, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'soe-core-styles',
  templateUrl: './core-styles.component.html',
  styleUrls: [
    './core-styles.component.scss',
    './core-styles.component.colors.scss',
    './core-styles.component.core.scss',
  ],
  encapsulation: ViewEncapsulation.None,
  standalone: false,
})
export class CoreStylesComponent {}
