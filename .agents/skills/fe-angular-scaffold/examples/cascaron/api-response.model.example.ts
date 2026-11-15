/**
 * core/models/api-response.model.ts
 * Contrato de respuesta estándar del backend. Debe coincidir EXACTAMENTE con ApiResponse<T> de .NET.
 */

export interface ApiError {
  code?: string;        // Clave para buscar en error-messages.json (ej. "Entity.NotFound")
  description: string;  // Mensaje técnico del backend
}

export interface ApiResponse<T> {
  data: T;              // Payload de la respuesta exitosa
  message: string;      // Mensaje descriptivo de la operación
  errors: ApiError[];   // Lista de errores (vacía en caso de éxito)
  isSuccess: boolean;   // true = éxito, false = error de dominio controlado
}
