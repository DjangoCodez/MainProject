import { Component } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { SelectUsersDialogData } from '../../models/select-users-dialog.model';
import { IUserSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Component({
  templateUrl: './select-users-dialog.component.html',
  styleUrls: ['./select-users-dialog.component.scss'],
  providers: [FlowHandlerService],
  standalone: false,
})
export class SelectUsersDialogComponent extends DialogComponent<SelectUsersDialogData> {
  selectedUsers: IUserSmallDTO[] = [];
  showMain = false;
  showParticipant = false;
  constructor() {
    super();
    this.setDialogParam();
  }

  setDialogParam() {
    if (this.data) {
      if (this.data.selectedUsers) {
        this.selectedUsers = this.data.selectedUsers;
      }
      if (this.data.showMain) {
        this.showMain = this.data.showMain;
      }
      if (this.data.showParticipant) {
        this.showParticipant = this.data.showParticipant;
      }
    }
  }
  onUsersChange(users: IUserSmallDTO[]) {
    this.selectedUsers = users.sort((a, b) =>
      a.main < b.main ? 1 : a.main === b.main ? 0 : -1
    );
  }

  cancel() {
    this.dialogRef.close(false);
  }

  protected selectUsers(): void {
    this.dialogRef.close(this.selectedUsers);
  }
}
