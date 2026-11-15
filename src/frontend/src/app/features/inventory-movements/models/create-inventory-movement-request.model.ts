import { InventoryMovementType } from './inventory-movement-type.type';

export interface CreateInventoryMovementRequest {
  productId: string;
  type: InventoryMovementType;
  quantity: number;
  reason: string;
}
