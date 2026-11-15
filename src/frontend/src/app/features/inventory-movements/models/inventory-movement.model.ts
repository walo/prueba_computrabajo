import { InventoryMovementType } from './inventory-movement-type.type';

export interface InventoryMovement {
  readonly id: string;
  readonly productId: string;
  readonly productSku: string;
  readonly productName: string;
  readonly type: InventoryMovementType;
  readonly quantity: number;
  readonly previousStock: number;
  readonly newStock: number;
  readonly reason: string;
  readonly performedByUserId: string;
  readonly performedByEmail: string;
  readonly createdAtUtc: string;
}
