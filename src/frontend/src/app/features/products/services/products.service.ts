import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { API_PATHS } from '../../../core/constants/api-paths.constants';
import { CreateProductRequest } from '../models/create-product-request.model';
import { PagedProductsResponse } from '../models/paged-products-response.model';
import { Product } from '../models/product.model';
import { ProductFilter } from '../models/product-filter.model';
import { ProductUpdateCommand } from '../models/product-update-command.model';

@Injectable({ providedIn: 'root' })
export class ProductsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}${API_PATHS.PRODUCTS.BASE}`;

  public getProducts(filter: ProductFilter): Observable<PagedProductsResponse> {
    const params = this.buildFilterParams(filter);

    return this.http.get<PagedProductsResponse>(this.baseUrl, { params });
  }

  public getProductById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${id}`);
  }

  public createProduct(payload: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, payload);
  }

  public updateProduct(command: ProductUpdateCommand): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/${command.id}`, command.payload);
  }

  public deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  private buildFilterParams(filter: ProductFilter): HttpParams {
    let params = new HttpParams()
      .set('page', String(filter.page))
      .set('pageSize', String(filter.pageSize));
    params = this.appendTextAndCategoryParams(params, filter);

    return this.appendBooleanParams(params, filter);
  }

  private appendTextAndCategoryParams(params: HttpParams, filter: ProductFilter): HttpParams {
    let next = params;
    if (filter.search) {
      next = next.set('search', filter.search);
    }
    if (filter.categoryId) {
      next = next.set('categoryId', filter.categoryId);
    }

    return next;
  }

  private appendBooleanParams(params: HttpParams, filter: ProductFilter): HttpParams {
    let next = params;
    if (filter.isActive !== null && filter.isActive !== undefined) {
      next = next.set('isActive', String(filter.isActive));
    }
    if (filter.lowStockOnly === true) {
      next = next.set('lowStockOnly', 'true');
    }

    return next;
  }
}
