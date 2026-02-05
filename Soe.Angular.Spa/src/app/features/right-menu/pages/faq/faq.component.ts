import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
    selector: 'soe-faq',
    templateUrl: './faq.component.html',
    styleUrls: ['./faq.component.scss'],
    standalone: false
})
export class FaqComponent {
  constructor(private router: ActivatedRoute) {
    // TODO: Add logic to fetch metadata.
    const { c, module, feature } = this.router.snapshot.queryParams;
  }
}
