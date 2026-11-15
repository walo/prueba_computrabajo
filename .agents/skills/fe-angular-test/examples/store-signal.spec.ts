import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { FeatureSignalStore } from './signal-store.example';
import { FeatureHttpService, FeatureDto } from './service-http.example';

describe('FeatureSignalStore', () => {
  let store: FeatureSignalStore;
  let serviceMock: jest.Mocked<FeatureHttpService>;

  const mockItems: FeatureDto[] = [
    { id: '1', name: 'Alpha' },
    { id: '2', name: 'Beta' },
  ];

  beforeEach(() => {
    serviceMock = {
      getItems: jest.fn().mockReturnValue(of({ isSuccess: true, data: mockItems })),
      deleteItem: jest.fn().mockReturnValue(of({ isSuccess: true, data: null })),
    } as unknown as jest.Mocked<FeatureHttpService>;

    TestBed.configureTestingModule({
      providers: [
        FeatureSignalStore,
        { provide: FeatureHttpService, useValue: serviceMock },
      ],
    });

    store = TestBed.inject(FeatureSignalStore);
  });

  it('debe inicializarse con estado vacío y loading en false', () => {
    expect(store.items()).toEqual([]);
    expect(store.loading()).toBe(false);
    expect(store.totalCount()).toBe(0);
  });

  it('debe cargar los items y actualizar las signals reactivas al ejecutar loadAll()', () => {
    store.loadAll();

    expect(serviceMock.getItems).toHaveBeenCalledTimes(1);
    expect(store.items()).toEqual(mockItems);
    expect(store.totalCount()).toBe(2);
    expect(store.loading()).toBe(false);
  });

  it('debe filtrar correctamente los items mediante la signal computada filteredItems', () => {
    store.loadAll();

    store.setFilter({ search: 'Alpha' });
    expect(store.filteredItems().length).toBe(1);
    expect(store.filteredItems()[0].name).toBe('Alpha');

    store.setFilter({ search: 'Inexistente' });
    expect(store.filteredItems().length).toBe(0);
  });

  it('debe eliminar un item del estado local mediante removeItem()', () => {
    store.loadAll();
    expect(store.items().length).toBe(2);

    store.removeItem('1');
    expect(store.items().length).toBe(1);
    expect(store.items()[0].id).toBe('2');
  });
});
