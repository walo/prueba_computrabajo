import { HttpErrorResponse } from '@angular/common/http';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { NgxPermissionsModule, NgxPermissionsService } from 'ngx-permissions';
import { of, throwError } from 'rxjs';
import { PERMISSIONS } from '../../../core/constants/permissions.constants';
import { NotificationService } from '../../../core/services/notification.service';
import { Product } from '../../products/models/product.model';
import { ProductsService } from '../../products/services/products.service';
import { InventoryMovement } from '../models/inventory-movement.model';
import { InventoryMovementsService } from '../services/inventory-movements.service';
import { InventoryMovementListComponent } from './inventory-movement-list.component';

const sampleProduct: Product = {
  id: 'prod-1',
  sku: 'LAP-001',
  name: 'Laptop Workstation',
  description: 'Workstation',
  categoryId: 'cat-1',
  categoryCode: 'ELEC',
  categoryName: 'Electronics',
  price: 1299.99,
  stock: 10,
  minStock: 2,
  isLowStock: false,
  isActive: true,
  createdAtUtc: '2026-11-15T10:00:00Z',
  updatedAtUtc: null,
};

const sampleMovement: InventoryMovement = {
  id: 'mov-1',
  productId: 'prod-1',
  productSku: 'LAP-001',
  productName: 'Laptop Workstation',
  type: 'ENTRY',
  quantity: 10,
  previousStock: 0,
  newStock: 10,
  reason: 'Initial inventory restock',
  performedByUserId: 'usr-1',
  performedByEmail: 'admin@redarbor.test',
  createdAtUtc: '2026-11-15T10:05:00Z',
};

describe('InventoryMovementListComponent', () => {
  let mockMovementsService: { getMovements: jest.Mock; createMovement: jest.Mock };
  let mockProductsService: { getProducts: jest.Mock };
  let mockNotifications: { success: jest.Mock; error: jest.Mock };
  let permissionsService: NgxPermissionsService;

  beforeEach(async () => {
    mockMovementsService = {
      getMovements: jest.fn().mockReturnValue(of({ items: [sampleMovement], page: 1, pageSize: 10, totalCount: 1 })),
      createMovement: jest.fn(),
    };
    mockProductsService = {
      getProducts: jest.fn().mockReturnValue(of({ items: [sampleProduct], page: 1, pageSize: 100, totalCount: 1 })),
    };
    mockNotifications = { success: jest.fn(), error: jest.fn() };
    await TestBed.configureTestingModule({
      imports: [InventoryMovementListComponent, NgxPermissionsModule.forRoot()],
      providers: [
        provideZonelessChangeDetection(),
        { provide: InventoryMovementsService, useValue: mockMovementsService },
        { provide: ProductsService, useValue: mockProductsService },
        { provide: NotificationService, useValue: mockNotifications },
      ],
    }).compileComponents();
    permissionsService = TestBed.inject(NgxPermissionsService);
    permissionsService.loadPermissions([PERMISSIONS.INVENTORY_MOVEMENTS.READ, PERMISSIONS.INVENTORY_MOVEMENTS.CREATE]);
  });

  it('should load initial kardex movements and active products on init', () => {
    const fixture = TestBed.createComponent(InventoryMovementListComponent);
    fixture.detectChanges();
    expect(mockProductsService.getProducts).toHaveBeenCalledWith({ page: 1, pageSize: 100, isActive: true });
    expect(mockMovementsService.getMovements).toHaveBeenCalledWith({
      page: 1,
      pageSize: 10,
      search: undefined,
      productId: null,
      type: null,
      fromDateUtc: null,
      toDateUtc: null,
    });
    expect(fixture.componentInstance.movements()).toHaveLength(1);
    expect(fixture.componentInstance.totalCount()).toBe(1);
  });

  it('should apply search, product, type, and UTC date range filters and reset them', () => {
    const fixture = TestBed.createComponent(InventoryMovementListComponent);
    fixture.detectChanges();
    mockMovementsService.getMovements.mockClear();
    fixture.componentInstance.onSearchChange('LAP');
    fixture.componentInstance.onProductFilterChange('prod-1');
    fixture.componentInstance.onTypeFilterChange('EXIT');
    fixture.componentInstance.onFromDateChange('2026-11-01');
    fixture.componentInstance.onToDateChange('2026-11-15');
    expect(mockMovementsService.getMovements).toHaveBeenLastCalledWith({
      page: 1,
      pageSize: 10,
      search: 'LAP',
      productId: 'prod-1',
      type: 'EXIT',
      fromDateUtc: '2026-11-01T00:00:00.000Z',
      toDateUtc: '2026-11-15T23:59:59.999Z',
    });
    fixture.componentInstance.clearFilters();
    expect(mockMovementsService.getMovements).toHaveBeenLastCalledWith({
      page: 1,
      pageSize: 10,
      search: undefined,
      productId: null,
      type: null,
      fromDateUtc: null,
      toDateUtc: null,
    });
  });

  it('should compute reactive stock preview for entry and detect insufficient stock on exit', () => {
    const fixture = TestBed.createComponent(InventoryMovementListComponent);
    fixture.detectChanges();
    fixture.componentInstance.openCreateModal();
    fixture.componentInstance.movementForm.patchValue({ productId: 'prod-1', type: 'ENTRY', quantity: 5 });
    expect(fixture.componentInstance.currentStock()).toBe(10);
    expect(fixture.componentInstance.projectedStock()).toBe(15);
    expect(fixture.componentInstance.isInsufficientStock()).toBe(false);
    fixture.componentInstance.movementForm.patchValue({ type: 'EXIT', quantity: 14 });
    expect(fixture.componentInstance.projectedStock()).toBe(-4);
    expect(fixture.componentInstance.isInsufficientStock()).toBe(true);
  });

  it('should submit ENTRY movement, refresh products and movements, and notify success', () => {
    mockMovementsService.createMovement.mockReturnValue(of(sampleMovement));
    const fixture = TestBed.createComponent(InventoryMovementListComponent);
    fixture.detectChanges();
    fixture.componentInstance.openCreateModal();
    fixture.componentInstance.movementForm.setValue({
      productId: 'prod-1',
      type: 'ENTRY',
      quantity: 4,
      reason: 'Supplier delivery #104',
    });
    fixture.componentInstance.submitForm();
    expect(mockMovementsService.createMovement).toHaveBeenCalledWith({
      productId: 'prod-1',
      type: 'ENTRY',
      quantity: 4,
      reason: 'Supplier delivery #104',
    });
    expect(mockNotifications.success).toHaveBeenCalled();
    expect(fixture.componentInstance.isModalOpen()).toBe(false);
    expect(mockProductsService.getProducts).toHaveBeenCalledTimes(2);
  });

  it('should translate backend InsufficientStock conflict into modalError alert', () => {
    const conflictError = new HttpErrorResponse({
      status: 409,
      error: { code: 'InventoryMovement.InsufficientStock' },
    });
    mockMovementsService.createMovement.mockReturnValue(throwError(() => conflictError));
    const fixture = TestBed.createComponent(InventoryMovementListComponent);
    fixture.detectChanges();
    fixture.componentInstance.openCreateModal();
    fixture.componentInstance.movementForm.setValue({
      productId: 'prod-1',
      type: 'EXIT',
      quantity: 99,
      reason: 'Manual sale dispatch',
    });
    fixture.componentInstance.submitForm();
    expect(fixture.componentInstance.modalError()?.message).toContain('stock disponible es insuficiente');
    expect(fixture.componentInstance.isModalOpen()).toBe(true);
  });

  it('should hide create button when user lacks Create permission and render zero row actions', () => {
    permissionsService.loadPermissions([PERMISSIONS.INVENTORY_MOVEMENTS.READ]);
    const fixture = TestBed.createComponent(InventoryMovementListComponent);
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).not.toContain('Registrar Movimiento');
    expect(text).not.toContain('Editar');
    expect(text).not.toContain('Eliminar');
  });
});
