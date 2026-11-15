export interface UpdateProductRequest {
  readonly sku: string;
  readonly name: string;
  readonly description: string | null;
  readonly categoryId: string;
  readonly price: number;
  readonly minStock: number;
  readonly isActive: boolean;
}
