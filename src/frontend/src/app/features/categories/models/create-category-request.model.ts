export interface CreateCategoryRequest {
  readonly code: string;
  readonly name: string;
  readonly description: string | null;
}
