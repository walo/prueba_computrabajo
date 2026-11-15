export interface ApiError {
  code?: string;
  description: string;
}

export interface ApiResponse<T> {
  data: T;
  message: string;
  errors: ApiError[];
  isSuccess: boolean;
}
