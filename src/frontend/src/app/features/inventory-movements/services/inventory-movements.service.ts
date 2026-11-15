import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { API_PATHS } from '../../../core/constants/api-paths.constants';
import { CreateInventoryMovementRequest } from '../models/create-inventory-movement-request.model';
import { InventoryMovementFilter } from '../models/inventory-movement-filter.model';
import { InventoryMovement } from '../models/inventory-movement.model';
import { PagedInventoryMovementsResponse } from '../models/paged-inventory-movements-response.model';

@Injectable({ providedIn: 'root' })
export class InventoryMovementsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}${API_PATHS.INVENTORY_MOVEMENTS.BASE}`;

  getMovements(filter: InventoryMovementFilter): Observable<PagedInventoryMovementsResponse> {
    const params = this.buildHttpParams(filter);

    return this.http.get<PagedInventoryMovementsResponse>(this.baseUrl, { params });
  }

  getMovementById(id: string): Observable<InventoryMovement> {
    return this.http.get<InventoryMovement>(`${this.baseUrl}/${id}`);
  }

  createMovement(payload: CreateInventoryMovementRequest): Observable<InventoryMovement> {
    return this.http.post<InventoryMovement>(this.baseUrl, payload);
  }

  private buildHttpParams(filter: InventoryMovementFilter): HttpParams {
    const baseParams = new HttpParams()
      .set('page', String(filter.page))
      .set('pageSize', String(filter.pageSize));
    const optionalEntries: ReadonlyArray<readonly [string, string | null | undefined]> = [
      ['search', filter.search?.trim()],
      ['productId', filter.productId],
      ['type', filter.type],
      ['fromDateUtc', filter.fromDateUtc],
      ['toDateUtc', filter.toDateUtc],
    ];

    return optionalEntries
      .filter((entry): entry is readonly [string, string] => Boolean(entry[1]))
      .reduce((params, [key, value]) => params.set(key, value), baseParams);
  }
}
