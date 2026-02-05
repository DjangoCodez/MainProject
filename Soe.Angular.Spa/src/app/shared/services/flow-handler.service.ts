import { Injectable, signal } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { forkJoin, Observable, of } from 'rxjs';
import { mergeMap, take, tap } from 'rxjs/operators';
import { MessagingService } from './messaging.service';
import { GridComponent } from '@ui/grid/grid.component';
import { CoreService } from './core.service';
import { Constants } from '@shared/util/client-constants';

type IFlowHandlerStreamsType = Observable<unknown> | Array<Observable<unknown>>;

interface FlowHandlerExecuteInterface {
  parentGuid?: string;
  permission?: Feature;
  additionalModifyPermissions?: Feature[];
  additionalReadPermissions?: Feature[];
  terms?: IFlowHandlerStreamsType;
  companySettings?: IFlowHandlerStreamsType;
  userSettings?: IFlowHandlerStreamsType;
  lookups?: IFlowHandlerStreamsType;
  data?: IFlowHandlerStreamsType;
  skipInitialLoad?: boolean;
  onPermissionsLoaded?: () => void;
  onSettingsLoaded?: () => void;
  setupDefaultToolbar?: () => void;
  setupGrid?: (grid: GridComponent<any>) => void;
  onFinished?: () => void;
}

export type FlowHandlerOptions = {
  parentGuid?: string;
  permission?: Feature;
  additionalModifyPermissions?: Feature[];
  additionalReadPermissions?: Feature[];
  terms?: IFlowHandlerStreamsType;
  companySettings?: IFlowHandlerStreamsType;
  userSettings?: IFlowHandlerStreamsType;
  lookups?: IFlowHandlerStreamsType;
  data?: IFlowHandlerStreamsType;
  skipInitialLoad?: boolean;
  skipDefaultToolbar?: boolean;
  useLegacyToolbar?: boolean;
  onPermissionsLoaded?: () => void;
  onSettingsLoaded?: () => void;
  setupDefaultToolbar?: () => void;
  onGridReadyToDefine?: (grid: GridComponent<any>) => void;
  onFinished?: () => void;
};

export interface IGridMessage {
  data: { grid: GridComponent<unknown>; parentGuid: string };
}

@Injectable()
export class FlowHandlerService {
  options: FlowHandlerOptions = {};

  readPermission = signal(false);
  modifyPermission = signal(false);
  allowFetchGrid = signal(false);

  private hasLoadedPermissions = signal(false);

  private additionalReadPermissions = signal<Partial<Record<Feature, boolean>>>(
    {}
  );
  private additionalModifyPermissions = signal<
    Partial<Record<Feature, boolean>>
  >({});

  constructor(
    private coreService: CoreService,
    private message: MessagingService
  ) {}

  hasReadAccess(feature: Feature) {
    return this.additionalReadPermissions()[feature] || false;
  }

  hasModifyAccess(feature: Feature) {
    return this.additionalModifyPermissions()[feature] || false;
  }

  executeForGrid() {
    this.execute({
      parentGuid: this.options?.parentGuid,
      permission: this.options?.permission,
      additionalReadPermissions: this.options?.additionalReadPermissions,
      additionalModifyPermissions: this.options?.additionalModifyPermissions,
      companySettings: this.options?.companySettings,
      userSettings: this.options?.userSettings,
      terms: this.options?.terms,
      lookups: this.options?.lookups,
      data: this.options?.data,
      skipInitialLoad: this.options?.skipInitialLoad,
      onPermissionsLoaded: this.options?.onPermissionsLoaded,
      onSettingsLoaded: this.options?.onSettingsLoaded,
      setupDefaultToolbar: this.options?.setupDefaultToolbar,
      setupGrid: this.options?.onGridReadyToDefine,
      onFinished: this.options?.onFinished,
    });
  }

  executeForEdit() {
    this.execute({
      parentGuid: this.options?.parentGuid,
      permission: this.options?.permission,
      additionalReadPermissions: this.options?.additionalReadPermissions,
      additionalModifyPermissions: this.options?.additionalModifyPermissions,
      companySettings: this.options?.companySettings,
      userSettings: this.options?.userSettings,
      terms: this.options?.terms,
      lookups: this.options?.lookups,
      data: this.options?.data,
      onPermissionsLoaded: this.options?.onPermissionsLoaded,
      onSettingsLoaded: this.options?.onSettingsLoaded,
      setupDefaultToolbar: this.options?.setupDefaultToolbar,
      onFinished: this.options?.onFinished,
    });
  }

  execute({
    parentGuid = undefined,
    permission = Feature.None,
    additionalReadPermissions = [],
    additionalModifyPermissions = [],
    companySettings = of(true),
    userSettings = of(true),
    terms = of(true),
    lookups = of(true),
    data = of(true),
    skipInitialLoad = false,
    onPermissionsLoaded: onPermissionsLoaded = undefined,
    onSettingsLoaded: onSettingsLoaded = undefined,
    setupDefaultToolbar = undefined,
    setupGrid = undefined,
    onFinished: onFinished = undefined,
  }: FlowHandlerExecuteInterface): void {
    this.loadPermission(permission)
      .pipe(
        // Load additional read and modify permissions
        mergeMap(() =>
          this.getAdditionalReadAndModifyPermissions(
            additionalReadPermissions,
            additionalModifyPermissions
          ).pipe(
            tap(() => {
              if (typeof onPermissionsLoaded !== 'undefined') {
                onPermissionsLoaded();
              }
            })
          )
        ),
        // Load settings
        mergeMap(() =>
          this.loadSettings(terms, companySettings, userSettings).pipe(
            tap(() => {
              if (typeof onSettingsLoaded !== 'undefined') {
                onSettingsLoaded();
              }
            })
          )
        ),
        // Load lookups and data
        mergeMap(() => forkJoin(this.toArray(lookups))),

        // Set up grid listener
        mergeMap(() => {
          this.allowFetchGrid.set(true);
          // If we don't have a grid listener - continue the observable
          if (typeof setupGrid === 'undefined') {
            if (typeof setupDefaultToolbar !== 'undefined')
              setupDefaultToolbar();
            if (typeof onFinished !== 'undefined') onFinished();
            return of(true);
          }

          // Register a listener on GRID READY to trigger the setup grid logic
          // The GUID prevents the flow handler from consuming messages intended for other grids
          return this.message
            .onEvent(Constants.EVENT_GRID_READY, parentGuid)
            .pipe(
              take(2),
              tap((gridMessage: IGridMessage) => {
                if (
                  parentGuid === undefined ||
                  gridMessage.data.grid.parentGuid() === parentGuid
                ) {
                  gridMessage.data.grid.columns = [];
                  setupGrid(gridMessage.data.grid);

                  if (typeof setupDefaultToolbar !== 'undefined')
                    setupDefaultToolbar();
                  if (typeof onFinished !== 'undefined') onFinished();
                }
              })
            );
        }),
        mergeMap(() => data)
      )
      .subscribe();
  }

  public reloadPermission(permission: Feature): Observable<unknown> {
    this.hasLoadedPermissions.set(false);
    return this.loadPermission(permission);
  }

  private loadPermission(permission: Feature): Observable<unknown> {
    // Bail if we already have loaded the permissions
    if (this.hasLoadedPermissions()) return of(true);

    return forkJoin([
      this.coreService.hasModifyPermissions([permission]),
      this.coreService.hasReadOnlyPermissions([permission]),
    ]).pipe(
      mergeMap(([modify, read]) => {
        this.modifyPermission.set(modify[permission]);
        this.readPermission.set(read[permission]);
        this.modifyPermission() && this.message.publishEnableTabAdd();
        this.hasLoadedPermissions.set(true);
        return of(true);
      })
    );
  }

  private getAdditionalReadAndModifyPermissions(
    additionalReadPermissions: Feature[],
    additionalModifyPermissions: Feature[]
  ) {
    const readPermissions =
      additionalReadPermissions.length > 0
        ? this.coreService.hasReadOnlyPermissions(additionalReadPermissions)
        : of([]);
    const modifyPermissions =
      additionalModifyPermissions.length > 0
        ? this.coreService.hasModifyPermissions(additionalModifyPermissions)
        : of([]);
    return forkJoin([readPermissions, modifyPermissions]).pipe(
      tap(([read, modify]) => {
        this.additionalReadPermissions.set(read);
        this.additionalModifyPermissions.set(modify);
      })
    );
  }

  private loadSettings(
    terms: IFlowHandlerStreamsType,
    companySettings: IFlowHandlerStreamsType,
    userSettings: IFlowHandlerStreamsType
  ): Observable<unknown> {
    return forkJoin([terms, companySettings, userSettings]);
  }

  private loadLookupsAndData(
    lookups: IFlowHandlerStreamsType,
    data: IFlowHandlerStreamsType
  ): Observable<unknown> {
    return forkJoin([lookups, data]);
  }

  /**
   * Converts singular observables into arrays to work with forkJoin
   * @param streamOrStreams - one stream or an array of streams
   * @returns the stream or streams in an array
   */
  private toArray(streamOrStreams: IFlowHandlerStreamsType): Observable<any>[] {
    return Array.isArray(streamOrStreams)
      ? [...streamOrStreams]
      : [streamOrStreams];
  }
}
