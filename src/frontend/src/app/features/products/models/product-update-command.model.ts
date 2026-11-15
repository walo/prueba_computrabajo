import { UpdateProductRequest } from './update-product-request.model';

export interface ProductUpdateCommand {
  readonly id: string;
  readonly payload: UpdateProductRequest;
}
