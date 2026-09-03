export interface ApiValidationError {
  errors: Record<string, string[]>;
}

export interface ApiSimpleError {
  error: string;
}

export type ApiError = ApiValidationError | ApiSimpleError;

export interface ApiResponse<T> {
  data:  T;
  error: null;
}

export interface ApiErrorResponse {
  data:  null;
  error: ApiError;
}

export type ApiResult<T> = ApiResponse<T> | ApiErrorResponse;
