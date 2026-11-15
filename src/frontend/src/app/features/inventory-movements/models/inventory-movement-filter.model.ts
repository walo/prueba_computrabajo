import { InventoryMovementType } from './inventory-movement-type.type';

export interface InventoryMovementFilter {
  page: number;
  pageSize: number;
  search?: string;
  productId?: string | null;
  type?: InventoryMovementType | null;
  fromDateUtc?: string | null;
  toDateUtc?: string | null;
}
