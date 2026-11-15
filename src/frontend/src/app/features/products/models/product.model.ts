export interface Product {
  readonly id: string;
  readonly sku: string;
  readonly name: string;
  readonly description: string | null;
  readonly categoryId: string;
  readonly categoryCode: string;
  readonly categoryName: string;
  readonly price: number;
  readonly stock: number;
  readonly minStock: number;
  readonly isLowStock: boolean;
  readonly isActive: boolean;
  readonly createdAtUtc: string;
  readonly updatedAtUtc: string | null;
}
