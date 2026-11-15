import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { API_PATHS } from '../../../core/constants/api-paths.constants';
import { Category } from '../models/category.model';
import { CategoryFilter } from '../models/category-filter.model';
import { CategoryUpdateCommand } from '../models/category-update-command.model';
import { CreateCategoryRequest } from '../models/create-category-request.model';
import { PagedCategoriesResponse } from '../models/paged-categories-response.model';

@Injectable({ providedIn: 'root' })
export class CategoriesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}${API_PATHS.CATEGORIES.BASE}`;

  public getCategories(filter: CategoryFilter): Observable<PagedCategoriesResponse> {
    const params = this.buildFilterParams(filter);

    return this.http.get<PagedCategoriesResponse>(this.baseUrl, { params });
  }

  public getCategoryById(id: string): Observable<Category> {
    return this.http.get<Category>(`${this.baseUrl}/${id}`);
  }

  public createCategory(payload: CreateCategoryRequest): Observable<Category> {
    return this.http.post<Category>(this.baseUrl, payload);
  }

  public updateCategory(command: CategoryUpdateCommand): Observable<Category> {
    return this.http.put<Category>(`${this.baseUrl}/${command.id}`, command.payload);
  }

  public deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  private buildFilterParams(filter: CategoryFilter): HttpParams {
    let params = new HttpParams()
      .set('page', String(filter.page))
      .set('pageSize', String(filter.pageSize));
    if (filter.search) {
      params = params.set('search', filter.search);
    }
    if (filter.isActive !== null && filter.isActive !== undefined) {
      params = params.set('isActive', String(filter.isActive));
    }

    return params;
  }
}
