import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IUserSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { orderBy } from 'lodash';
import { take, tap } from 'rxjs';
import { UserSmallDTO } from '../../models/select-users-dialog.model';
import { SelectUsersDialogService } from '../../services/select-users-dialog.service';

@Component({
  selector: 'soe-select-users',
  templateUrl: './select-users.component.html',
  styleUrls: ['./select-users.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SelectUsersComponent
  extends GridBaseDirective<IUserSmallDTO>
  implements OnInit
{
  @Input() showParticipant = false;
  @Input() showMain = false;
  @Input() selectedUsers: IUserSmallDTO[] = [];
  @Output() usersChange: EventEmitter<IUserSmallDTO[]> = new EventEmitter<
    IUserSmallDTO[]
  >();

  flowHandler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  selectUsersDialogService = inject(SelectUsersDialogService);
  coreService = inject(CoreService);
  performUserLoad = new Perform<IUserSmallDTO[]>(this.progressService);
  users: IUserSmallDTO[] = [];

  ngOnInit(): void {
    this.startFlow(Feature.Billing_Purchase_Purchase_Edit, 'select-users', {
      skipDefaultToolbar: true,
      skipInitialLoad: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IUserSmallDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.selected',
        'common.username',
        'common.name',
        'common.main',
        'billing.order.selectusers.responsible',
        'billing.order.selectusers.participant',
        'common.categories',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnBool(
          'isSelected',
          this.showParticipant
            ? terms['billing.order.selectusers.participant']
            : terms['common.selected'],
          {
            flex: 1,
            suppressFilter: true,
            minWidth: 40,
            maxWidth: 80,
            enableHiding: true,
            editable: true,
            onClick: this.participantSelected.bind(this),
          }
        );

        if (this.showMain) {
          this.grid.addColumnBool(
            'main',
            terms['billing.order.selectusers.responsible'],
            {
              flex: 1,
              suppressFilter: true,
              enableHiding: true,
              editable: true,
              onClick: this.mainSelected.bind(this),
            }
          );
        }
        this.grid.addColumnText('loginName', terms['common.username'], {
          flex: 1,
          editable: false,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          editable: false,
        });
        this.grid.addColumnText('categories', terms['common.categories'], {
          flex: 1,
          editable: false,
        });
        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid();
        this.loadUsers();
      });
  }
  loadUsers() {
    this.performUserLoad.load(
      this.coreService.getUsers(false, true, false, false, true, false).pipe(
        tap(users => {
          this.users = users;
          this.updateSelectedUsers();
          this.setGridData();
        })
      )
    );
  }

  selectAllChanged(data: boolean) {
    this.users.forEach(user => {
      user.isSelected = data;
    });

    this.setGridData();
  }

  participantSelected(data: boolean, row: IUserSmallDTO) {
    const user = this.users.find((user: IUserSmallDTO) => {
      return user.userId == row.userId;
    });
    if (user) {
      user.isSelected = data;
    }
    this.setGridData();
  }

  mainSelected(data: boolean, row: IUserSmallDTO) {
    if (data) {
      this.users.forEach((user: IUserSmallDTO) => {
        if (user.userId == row.userId) {
          user.main = true;
          user.isSelected = true;
        } else if (user.main) {
          user.main = false;
        }
      });
    }
    this.setGridData();
  }

  updateSelectedUsers() {
    const inActivatedUserTerm = this.translate.instant(
      'billing.order.selectusers.inactivateduser'
    );
    // Set pre-selected
    this.selectedUsers.forEach(selectedUser => {
      const user = this.users.find((user: IUserSmallDTO) => {
        return user.userId == selectedUser.userId;
      });
      if (user) {
        user.isSelected = true;
        // Set main
        if (selectedUser.main) user.main = true;
      } else {
        // user is inactivated and therefore it excists in selectedUsers, but not in users
        const inActivatedUser = new UserSmallDTO();
        inActivatedUser.userId = selectedUser.userId;
        inActivatedUser.main = selectedUser.main;
        inActivatedUser.name = selectedUser.name;
        inActivatedUser.loginName = inActivatedUserTerm;
        inActivatedUser.isSelected = true;

        this.users.push(inActivatedUser);
      }
    });

    this.setGridData();
  }

  setSelectedUsers() {
    this.selectedUsers = [];
    if (this.users) {
      this.users.forEach(user => {
        if (user.isSelected) {
          this.selectedUsers.push(user);
        }
      });
    }
  }

  setGridData() {
    if (this.grid) {
      this.grid.setData(orderBy(this.users, 'name', ['asc']));
    }
    this.setSelectedUsers();
    this.usersChange.emit(this.selectedUsers);
  }
}
