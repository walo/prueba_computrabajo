export interface ProductFilter {
  readonly page: number;
  readonly pageSize: number;
  readonly search?: string;
  readonly categoryId?: string | null;
  readonly isActive?: boolean | null;
  readonly lowStockOnly?: boolean | null;
}
