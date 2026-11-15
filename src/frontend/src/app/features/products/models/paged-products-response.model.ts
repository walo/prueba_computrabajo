import { Product } from './product.model';

export interface PagedProductsResponse {
  readonly items: readonly Product[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
}
