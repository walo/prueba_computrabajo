import { Category } from './category.model';

export interface PagedCategoriesResponse {
  readonly items: readonly Category[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
}
