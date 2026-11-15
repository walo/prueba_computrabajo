import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { API_PATHS } from '../../../core/constants/api-paths.constants';
import { ProductsService } from './products.service';

describe('ProductsService', () => {
  let service: ProductsService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiUrl}${API_PATHS.PRODUCTS.BASE}`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ProductsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should request paged products with combined filter query params', () => {
    service
      .getProducts({
        page: 2,
        pageSize: 10,
        search: 'lap',
        categoryId: 'cat-1',
        isActive: true,
        lowStockOnly: true,
      })
      .subscribe();
    const req = httpMock.expectOne((request) => request.url === baseUrl);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('10');
    expect(req.request.params.get('search')).toBe('lap');
    expect(req.request.params.get('categoryId')).toBe('cat-1');
    expect(req.request.params.get('isActive')).toBe('true');
    expect(req.request.params.get('lowStockOnly')).toBe('true');
    req.flush({ items: [], page: 2, pageSize: 10, totalCount: 0 });
  });

  it('should create product via POST', () => {
    service
      .createProduct({
        sku: 'SKU-1',
        name: 'Laptop',
        description: null,
        categoryId: 'cat-1',
        price: 1200,
        initialStock: 5,
        minStock: 2,
      })
      .subscribe();
    const postReq = httpMock.expectOne(baseUrl);
    expect(postReq.request.method).toBe('POST');
    postReq.flush({});
  });

  it('should update product via PUT', () => {
    service
      .updateProduct({
        id: 'prod-1',
        payload: {
          sku: 'SKU-1',
          name: 'Laptop Pro',
          description: 'Updated',
          categoryId: 'cat-1',
          price: 1400,
          minStock: 3,
          isActive: false,
        },
      })
      .subscribe();
    const putReq = httpMock.expectOne(`${baseUrl}/prod-1`);
    expect(putReq.request.method).toBe('PUT');
    putReq.flush({});
  });

  it('should delete product via DELETE', () => {
    service.deleteProduct('prod-1').subscribe();
    const deleteReq = httpMock.expectOne(`${baseUrl}/prod-1`);
    expect(deleteReq.request.method).toBe('DELETE');
    deleteReq.flush(null);
  });
});
