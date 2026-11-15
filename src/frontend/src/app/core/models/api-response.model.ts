import { ApiError } from './api-error.model';

export interface ApiResponse<T> {
  data: T;
  message: string;
  errors: ApiError[];
  isSuccess: boolean;
}
