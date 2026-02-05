import {
  ISupplierExtendedGridDTO,
  ISupplierGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export type SupplierExtendedGridDTO = ISupplierExtendedGridDTO &
  ISupplierGridDTO & { categoriesArray?: string[] };
