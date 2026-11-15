export interface CategoryFilter {
  readonly page: number;
  readonly pageSize: number;
  readonly search?: string;
  readonly isActive?: boolean | null;
}
