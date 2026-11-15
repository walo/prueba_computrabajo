export interface UpdateCategoryRequest {
  readonly code: string;
  readonly name: string;
  readonly description: string | null;
  readonly isActive: boolean;
}
