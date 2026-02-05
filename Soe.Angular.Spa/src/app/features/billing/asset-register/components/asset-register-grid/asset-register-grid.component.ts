import { Component } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-asset-register-grid',
  templateUrl: './asset-register-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AssetRegisterGridComponent {}
