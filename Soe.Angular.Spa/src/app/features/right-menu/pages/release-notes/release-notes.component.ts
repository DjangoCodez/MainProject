import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
    selector: 'soe-release-notes',
    templateUrl: './release-notes.component.html',
    styleUrls: ['./release-notes.component.scss'],
    standalone: false
})
export class ReleaseNotesComponent {
  constructor(private router: ActivatedRoute) {
    // TODO: Add logic to fetch metadata.
    const { c, module, feature } = this.router.snapshot.queryParams;
  }
}
