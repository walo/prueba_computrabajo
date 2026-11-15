import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { FeatureHttpService, FeatureDto } from './service-http.example';
import { environment } from '@env/environment';
import { ApiResponse } from '@core/models/api-response.model';

describe('FeatureHttpService', () => {
  let service: FeatureHttpService;
  let httpMock: HttpTestingController;

  const mockResponse: ApiResponse<FeatureDto[]> = {
    isSuccess: true,
    message: 'Operación exitosa',
    errors: [],
    data: [
      { id: '1', name: 'Item 1' },
      { id: '2', name: 'Item 2' },
    ],
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        FeatureHttpService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(FeatureHttpService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('debe crearse correctamente', () => {
    expect(service).toBeTruthy();
  });

  it('debe obtener la lista de elementos mediante GET', () => {
    service.getItems().subscribe((res) => {
      expect(res.isSuccess).toBe(true);
      expect(res.data.length).toBe(2);
      expect(res.data[0].name).toBe('Item 1');
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/items`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('debe enviar la petición DELETE con el identificador del recurso', () => {
    const itemId = 'item-123';

    service.deleteItem(itemId).subscribe((res) => {
      expect(res.isSuccess).toBe(true);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/items/${itemId}`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ isSuccess: true, message: 'Eliminado', errors: [], data: null });
  });
});
