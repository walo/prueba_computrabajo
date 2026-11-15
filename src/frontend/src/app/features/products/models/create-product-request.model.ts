export interface CreateProductRequest {
  readonly sku: string;
  readonly name: string;
  readonly description: string | null;
  readonly categoryId: string;
  readonly price: number;
  readonly initialStock: number;
  readonly minStock: number;
}
