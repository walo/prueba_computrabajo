import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { API_PATHS } from '../../../core/constants/api-paths.constants';
import { Category } from '../models/category.model';
import { PagedCategoriesResponse } from '../models/paged-categories-response.model';
import { CategoriesService } from './categories.service';

describe('CategoriesService', () => {
  let service: CategoriesService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiUrl}${API_PATHS.CATEGORIES.BASE}`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CategoriesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch paged categories with query params', () => {
    let responseData: PagedCategoriesResponse | undefined;
    const expected: PagedCategoriesResponse = { items: [], page: 2, pageSize: 10, totalCount: 0 };
    service.getCategories({ page: 2, pageSize: 10, search: 'tech', isActive: true }).subscribe((res) => {
      responseData = res;
    });
    const req = httpMock.expectOne(
      `${baseUrl}?page=2&pageSize=10&search=tech&isActive=true`
    );
    expect(req.request.method).toBe('GET');
    req.flush(expected);
    expect(responseData).toEqual(expected);
  });

  it('should fetch category by id', () => {
    let resolved: Category | undefined;
    const sample: Category = {
      id: 'cat-1',
      code: 'elec-01',
      name: 'Electronics',
      description: null,
      isActive: true,
      createdAtUtc: '2026-11-15T00:00:00Z',
      updatedAtUtc: null,
    };
    service.getCategoryById('cat-1').subscribe((res) => {
      resolved = res;
    });
    const req = httpMock.expectOne(`${baseUrl}/cat-1`);
    expect(req.request.method).toBe('GET');
    req.flush(sample);
    expect(resolved).toEqual(sample);
  });

  it('should create category with POST request', () => {
    const payload = { code: 'bev-01', name: 'Beverages', description: 'Drinks' };
    service.createCategory(payload).subscribe();
    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({ ...payload, id: 'cat-2', isActive: true, createdAtUtc: '2026-11-15T00:00:00Z', updatedAtUtc: null });
  });

  it('should update category with PUT request', () => {
    const payload = { code: 'bev-01', name: 'Cold Drinks', description: null, isActive: false };
    service.updateCategory({ id: 'cat-2', payload }).subscribe();
    const req = httpMock.expectOne(`${baseUrl}/cat-2`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    req.flush({ ...payload, id: 'cat-2', createdAtUtc: '2026-11-15T00:00:00Z', updatedAtUtc: '2026-11-15T01:00:00Z' });
  });

  it('should delete category with DELETE request', () => {
    service.deleteCategory('cat-2').subscribe();
    const req = httpMock.expectOne(`${baseUrl}/cat-2`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
