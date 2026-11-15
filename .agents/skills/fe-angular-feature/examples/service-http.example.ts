import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import { ApiResponse } from '@core/models/api-response.model';

export interface FeatureDto {
  id: string;
  code: string;
  name: string;
  status: string;
}

export interface CreateFeatureDto {
  code: string;
  name: string;
}

@Injectable({
  providedIn: 'root',
})
export class FeatureHttpService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/items`;

  getItems(): Observable<ApiResponse<FeatureDto[]>> {
    return this.http.get<ApiResponse<FeatureDto[]>>(this.apiUrl);
  }

  getById(id: string): Observable<ApiResponse<FeatureDto>> {
    return this.http.get<ApiResponse<FeatureDto>>(`${this.apiUrl}/${id}`);
  }

  create(payload: CreateFeatureDto): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(this.apiUrl, payload);
  }

  deleteItem(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }
}
