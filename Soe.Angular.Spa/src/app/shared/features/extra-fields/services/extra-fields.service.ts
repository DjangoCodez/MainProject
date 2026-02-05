import { inject, Injectable } from '@angular/core';
import {
  Feature,
  SoeEntityType,
  TermGroup,
  TermGroup_ExtraFieldType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getExtraField,
  getExtraFieldGridDTOs,
  saveExtraField,
  deleteExtraField,
  getExtraFieldRecord,
  getExtraFieldsWitRecords,
  saveExtraFieldRecords,
  getExtraFieldsDict,
  getSysExtraFields,
} from '@shared/services/generated-service-endpoints/core/ExtraField.endpoints';
import { Observable, of, tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Perform } from '@shared/util/perform.class';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IAccountDimSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  IExtraFieldDTO,
  IExtraFieldGridDTO,
  IExtraFieldRecordDTO,
} from '@shared/models/generated-interfaces/ExtraFieldDTO';
import { IExtraFieldRecordsModel } from '@shared/models/generated-interfaces/CoreModels';
import { ISysExtraFieldDTO } from '@shared/models/generated-interfaces/SysExtraFieldDTO';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ExtraFieldsService {
  constructor(
    private http: SoeHttpClient,
    private coreService: CoreService
  ) {}

  // Cached data
  progressService = inject(ProgressService);
  performFieldTypes = new Perform<SmallGenericType[]>(this.progressService);
  performAccountDims = new Perform<IAccountDimSmallDTO[]>(this.progressService);

  getGridAdditionalProps = {
    entity: 0,
    loadRecords: false,
    connectedEntity: 0,
    connectedRecordId: 0,
    useCache: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      entity: number;
      loadRecords: boolean;
      connectedEntity: number;
      connectedRecordId: number;
      useCache: boolean;
    }
  ): Observable<IExtraFieldGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IExtraFieldGridDTO[]>(
      getExtraFieldGridDTOs(
        this.getGridAdditionalProps.entity,
        this.getGridAdditionalProps.loadRecords,
        this.getGridAdditionalProps.connectedEntity,
        this.getGridAdditionalProps.connectedRecordId,
        id
      ),
      { useCache: this.getGridAdditionalProps.useCache }
    );
  }

  get(extraFieldId: number): Observable<IExtraFieldDTO> {
    return this.http.get<IExtraFieldDTO>(getExtraField(extraFieldId));
  }

  getDict(
    entity: number,
    connectedEntity: number,
    connectedRecordId: number,
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getExtraFieldsDict(
        entity,
        connectedEntity,
        connectedRecordId,
        addEmptyRow
      )
    );
  }

  save(model: IExtraFieldDTO): Observable<BackendResponse> {
    return this.http.post(saveExtraField(), model);
  }

  delete(extraFieldId: number): Observable<BackendResponse> {
    return this.http.delete(deleteExtraField(extraFieldId));
  }

  loadFieldTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.ExtraFieldTypes, false, false, true)
      .pipe(
        tap(types => {
          this.performFieldTypes.data = types.filter(
            x => x.id !== TermGroup_ExtraFieldType.MultiChoice
          );
        })
      );
  }

  loadAccountDims(): Observable<IAccountDimSmallDTO[]> {
    return this.coreService
      .getAccountDimsSmall(
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false
      )
      .pipe(tap(dims => (this.performAccountDims.data = dims)));
  }

  getExtraFieldRecord(
    extraFieldId: number,
    recordId: number,
    entity: number
  ): Observable<IExtraFieldRecordDTO> {
    return this.http.get<IExtraFieldRecordDTO>(
      getExtraFieldRecord(extraFieldId, recordId, entity)
    );
  }

  getExtraFieldWithRecords(
    recordId: number,
    entity: number,
    langId: number,
    connectedEntity: number,
    connectedRecordId: number
  ): Observable<IExtraFieldRecordDTO[]> {
    return this.http.get<IExtraFieldRecordDTO[]>(
      getExtraFieldsWitRecords(
        recordId,
        entity,
        langId,
        connectedEntity,
        connectedRecordId
      )
    );
  }

  saveExtraFieldRecord(
    model: IExtraFieldRecordsModel
  ): Observable<BackendResponse> {
    return this.http.post(saveExtraFieldRecords(), model);
  }

  getPermission(entity: SoeEntityType): Feature {
    switch (entity) {
      case SoeEntityType.InvoiceProduct:
        return Feature.Billing_Product_Products_ExtraFields_Edit;
      case SoeEntityType.Supplier:
        return Feature.Common_ExtraFields_Supplier_Edit;
      case SoeEntityType.Customer:
        return Feature.Common_ExtraFields_Customer_Edit;
      case SoeEntityType.Employee:
        return Feature.Common_ExtraFields_Employee_Edit;
      case SoeEntityType.Account:
        return Feature.Common_ExtraFields_Account_Edit;
      case SoeEntityType.PayrollProductSetting:
        return Feature.Common_ExtraFields_PayrollProductSetting;
    }
    return Feature.None;
  }

  getValidEntityTypes(
    entityTypes: SoeEntityType[]
  ): Observable<SmallGenericType[]> {
    const permissionEntityMap: any = {};
    const permissions: Feature[] = [];
    const validEntityTypes: SmallGenericType[] = [];
    const terms: any = {};

    validEntityTypes.push({ id: 0, name: '' } as SmallGenericType);

    entityTypes.forEach(entityType => {
      const permission = this.getPermission(entityType);
      permissions.push(permission);
      permissionEntityMap[permission] = entityType;
    });

    // get terms for all entity types
    if (permissions.length > 0) {
      this.coreService
        .getTermGroupContent(TermGroup.SoeEntityType, false, false)
        .pipe(
          tap(x => {
            x.filter(term => entityTypes.includes(term.id)).forEach(term => {
              terms[term.id] = term.name;
            });
          })
        )
        .subscribe();
    }

    // check permissions on each entity type
    this.coreService
      .hasReadOnlyPermissions(permissions)
      .pipe(
        tap((result: any) => {
          permissions.forEach(permission => {
            if (result[permission]) {
              const mappedEntityType = permissionEntityMap[permission];
              validEntityTypes.push({
                id: mappedEntityType,
                name: terms[mappedEntityType],
              } as SmallGenericType);
            }
          });
        })
      )
      .subscribe();

    return of(validEntityTypes);
  }

  getSysExtraFields(
    entityType: SoeEntityType
  ): Observable<ISysExtraFieldDTO[]> {
    return this.http.get<ISysExtraFieldDTO[]>(getSysExtraFields(entityType));
  }
}
