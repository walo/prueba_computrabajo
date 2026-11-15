import { UpdateCategoryRequest } from './update-category-request.model';

export interface CategoryUpdateCommand {
  readonly id: string;
  readonly payload: UpdateCategoryRequest;
}
