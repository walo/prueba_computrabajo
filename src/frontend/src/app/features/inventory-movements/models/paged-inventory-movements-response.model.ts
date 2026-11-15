import { InventoryMovement } from './inventory-movement.model';

export interface PagedInventoryMovementsResponse {
  readonly items: readonly InventoryMovement[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
}
