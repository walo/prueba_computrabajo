import { importProvidersFrom, provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AdConfirmService } from '@accionadev/ui-kit';
import { NgxPermissionsModule, NgxPermissionsService } from 'ngx-permissions';
import { of } from 'rxjs';
import { PERMISSIONS } from '../../../core/constants/permissions.constants';
import { NotificationService } from '../../../core/services/notification.service';
import { Category } from '../../categories/models/category.model';
import { CategoriesService } from '../../categories/services/categories.service';
import { Product } from '../models/product.model';
import { ProductsService } from '../services/products.service';
import { ProductListComponent } from './product-list.component';

describe('ProductListComponent', () => {
  const sampleCategory: Category = {
    id: 'cat-1',
    code: 'elec',
    name: 'Electronics',
    description: null,
    isActive: true,
    createdAtUtc: '2026-11-15T00:00:00Z',
    updatedAtUtc: null,
  };
  const sampleProduct: Product = {
    id: 'prod-1',
    sku: 'LAP-001',
    name: 'Laptop Pro 14',
    description: 'Workstation',
    categoryId: 'cat-1',
    categoryCode: 'elec',
    categoryName: 'Electronics',
    price: 1299.99,
    stock: 2,
    minStock: 5,
    isLowStock: true,
    isActive: true,
    createdAtUtc: '2026-11-15T00:00:00Z',
    updatedAtUtc: null,
  };
  const mockProductsService = {
    getProducts: jest.fn(),
    createProduct: jest.fn(),
    updateProduct: jest.fn(),
    deleteProduct: jest.fn(),
  };
  const mockCategoriesService = {
    getCategories: jest.fn(),
  };
  const mockConfirmService = {
    confirm: jest.fn(),
  };
  const mockNotifications = {
    success: jest.fn(),
    error: jest.fn(),
  };

  beforeEach(() => {
    jest.clearAllMocks();
    mockCategoriesService.getCategories.mockReturnValue(
      of({ items: [sampleCategory], page: 1, pageSize: 100, totalCount: 1 })
    );
    mockProductsService.getProducts.mockReturnValue(
      of({ items: [sampleProduct], page: 1, pageSize: 10, totalCount: 1 })
    );
    TestBed.configureTestingModule({
      imports: [ProductListComponent],
      providers: [
        provideZonelessChangeDetection(),
        importProvidersFrom(NgxPermissionsModule.forRoot()),
        { provide: ProductsService, useValue: mockProductsService },
        { provide: CategoriesService, useValue: mockCategoriesService },
        { provide: AdConfirmService, useValue: mockConfirmService },
        { provide: NotificationService, useValue: mockNotifications },
      ],
    });
    const permissions = TestBed.inject(NgxPermissionsService);
    permissions.loadPermissions([
      PERMISSIONS.PRODUCTS.READ,
      PERMISSIONS.PRODUCTS.CREATE,
      PERMISSIONS.PRODUCTS.UPDATE,
      PERMISSIONS.PRODUCTS.DELETE,
    ]);
  });

  it('should load active categories and products on initialization', () => {
    const fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();
    expect(mockCategoriesService.getCategories).toHaveBeenCalledWith({
      page: 1,
      pageSize: 100,
      isActive: true,
    });
    expect(mockProductsService.getProducts).toHaveBeenCalledWith({
      page: 1,
      pageSize: 10,
      search: undefined,
      categoryId: null,
      isActive: null,
      lowStockOnly: false,
    });
    expect(fixture.componentInstance.items()).toEqual([sampleProduct]);
  });

  it('should reload products when search, category, status, or lowStock filters change', () => {
    const fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();
    fixture.componentInstance.onSearchChange('lap');
    fixture.componentInstance.onCategoryFilterChange('cat-1');
    fixture.componentInstance.onStatusFilterChange('active');
    fixture.componentInstance.onLowStockFilterChange(true);
    expect(mockProductsService.getProducts).toHaveBeenLastCalledWith({
      page: 1,
      pageSize: 10,
      search: 'lap',
      categoryId: 'cat-1',
      isActive: true,
      lowStockOnly: true,
    });
  });

  it('should open create modal and submit valid product with initialStock', () => {
    mockProductsService.createProduct.mockReturnValue(of(sampleProduct));
    const fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();
    fixture.componentInstance.openCreateModal();
    fixture.componentInstance.productForm.setValue({
      sku: 'MOU-01',
      name: 'Optical Mouse',
      description: 'Wireless',
      categoryId: 'cat-1',
      price: 39.5,
      initialStock: 12,
      minStock: 3,
      isActive: true,
    });
    fixture.componentInstance.submitForm();
    expect(mockProductsService.createProduct).toHaveBeenCalledWith({
      sku: 'MOU-01',
      name: 'Optical Mouse',
      description: 'Wireless',
      categoryId: 'cat-1',
      price: 39.5,
      initialStock: 12,
      minStock: 3,
    });
    expect(mockNotifications.success).toHaveBeenCalled();
  });

  it('should open edit modal and submit update without overwriting stock', () => {
    mockProductsService.updateProduct.mockReturnValue(of(sampleProduct));
    const fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();
    fixture.componentInstance.openEditModal(sampleProduct);
    fixture.componentInstance.productForm.patchValue({ name: 'Laptop Pro 16', minStock: 8, isActive: false });
    fixture.componentInstance.submitForm();
    expect(mockProductsService.updateProduct).toHaveBeenCalledWith({
      id: 'prod-1',
      payload: {
        sku: 'LAP-001',
        name: 'Laptop Pro 16',
        description: 'Workstation',
        categoryId: 'cat-1',
        price: 1299.99,
        minStock: 8,
        isActive: false,
      },
    });
    expect(mockNotifications.success).toHaveBeenCalled();
  });

  it('should confirm and delete product when confirmed', async () => {
    mockConfirmService.confirm.mockResolvedValue(true);
    mockProductsService.deleteProduct.mockReturnValue(of(void 0));
    const fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();
    await fixture.componentInstance.confirmDelete(sampleProduct);
    expect(mockConfirmService.confirm).toHaveBeenCalled();
    expect(mockProductsService.deleteProduct).toHaveBeenCalledWith('prod-1');
    expect(mockNotifications.success).toHaveBeenCalled();
  });
});
