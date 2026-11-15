import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { API_PATHS } from '../../../core/constants/api-paths.constants';
import { InventoryMovementsService } from './inventory-movements.service';

describe('InventoryMovementsService', () => {
  let service: InventoryMovementsService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiUrl}${API_PATHS.INVENTORY_MOVEMENTS.BASE}`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(InventoryMovementsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should request paged inventory movements with combined filter query params', () => {
    service
      .getMovements({
        page: 2,
        pageSize: 15,
        search: 'restock',
        productId: 'prod-1',
        type: 'ENTRY',
        fromDateUtc: '2026-11-01T00:00:00Z',
        toDateUtc: '2026-11-15T23:59:59Z',
      })
      .subscribe();
    const req = httpMock.expectOne((request) => request.url === baseUrl);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('15');
    expect(req.request.params.get('search')).toBe('restock');
    expect(req.request.params.get('productId')).toBe('prod-1');
    expect(req.request.params.get('type')).toBe('ENTRY');
    expect(req.request.params.get('fromDateUtc')).toBe('2026-11-01T00:00:00Z');
    expect(req.request.params.get('toDateUtc')).toBe('2026-11-15T23:59:59Z');
    req.flush({ items: [], page: 2, pageSize: 15, totalCount: 0 });
  });

  it('should omit null and blank optional query params when requesting movements', () => {
    service
      .getMovements({
        page: 1,
        pageSize: 10,
        search: '   ',
        productId: null,
        type: null,
        fromDateUtc: null,
        toDateUtc: null,
      })
      .subscribe();
    const req = httpMock.expectOne((request) => request.url === baseUrl);
    expect(req.request.params.has('search')).toBe(false);
    expect(req.request.params.has('productId')).toBe(false);
    expect(req.request.params.has('type')).toBe(false);
    expect(req.request.params.has('fromDateUtc')).toBe(false);
    expect(req.request.params.has('toDateUtc')).toBe(false);
    req.flush({ items: [], page: 1, pageSize: 10, totalCount: 0 });
  });

  it('should fetch a single inventory movement by id via GET', () => {
    service.getMovementById('mov-1').subscribe();
    const req = httpMock.expectOne(`${baseUrl}/mov-1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should create inventory movement via POST', () => {
    const payload = {
      productId: 'prod-1',
      type: 'EXIT' as const,
      quantity: 3,
      reason: 'Dispatch order #42',
    };
    service.createMovement(payload).subscribe();
    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });
});
