import { importProvidersFrom, provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AdConfirmService } from '@accionadev/ui-kit';
import { NgxPermissionsModule, NgxPermissionsService } from 'ngx-permissions';
import { of } from 'rxjs';
import { PERMISSIONS } from '../../../core/constants/permissions.constants';
import { NotificationService } from '../../../core/services/notification.service';
import { Category } from '../models/category.model';
import { CategoriesService } from '../services/categories.service';
import { CategoriesPageComponent } from './categories-page.component';

describe('CategoriesPageComponent', () => {
  const sampleCategory: Category = {
    id: 'cat-1',
    code: 'elec-01',
    name: 'Electronics',
    description: 'Hardware and devices',
    isActive: true,
    createdAtUtc: '2026-11-15T00:00:00Z',
    updatedAtUtc: null,
  };
  const mockCategoriesService = {
    getCategories: jest.fn(),
    createCategory: jest.fn(),
    updateCategory: jest.fn(),
    deleteCategory: jest.fn(),
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
      of({ items: [sampleCategory], page: 1, pageSize: 10, totalCount: 1 })
    );
    TestBed.configureTestingModule({
      imports: [CategoriesPageComponent],
      providers: [
        provideZonelessChangeDetection(),
        importProvidersFrom(NgxPermissionsModule.forRoot()),
        { provide: CategoriesService, useValue: mockCategoriesService },
        { provide: AdConfirmService, useValue: mockConfirmService },
        { provide: NotificationService, useValue: mockNotifications },
      ],
    });
    const permissions = TestBed.inject(NgxPermissionsService);
    permissions.loadPermissions([
      PERMISSIONS.CATEGORIES.READ,
      PERMISSIONS.CATEGORIES.CREATE,
      PERMISSIONS.CATEGORIES.UPDATE,
      PERMISSIONS.CATEGORIES.DELETE,
    ]);
  });

  it('should load categories on initialization', () => {
    const fixture = TestBed.createComponent(CategoriesPageComponent);
    fixture.detectChanges();
    expect(mockCategoriesService.getCategories).toHaveBeenCalledWith({
      page: 1,
      pageSize: 10,
      search: undefined,
      isActive: null,
    });
    expect(fixture.componentInstance.items()).toEqual([sampleCategory]);
  });

  it('should reload categories when search or status filter changes', () => {
    const fixture = TestBed.createComponent(CategoriesPageComponent);
    fixture.detectChanges();
    fixture.componentInstance.onSearchChange('elec');
    expect(mockCategoriesService.getCategories).toHaveBeenLastCalledWith({
      page: 1,
      pageSize: 10,
      search: 'elec',
      isActive: null,
    });
    fixture.componentInstance.onStatusFilterChange('inactive');
    expect(mockCategoriesService.getCategories).toHaveBeenLastCalledWith({
      page: 1,
      pageSize: 10,
      search: 'elec',
      isActive: false,
    });
  });

  it('should open create modal and submit valid category', () => {
    mockCategoriesService.createCategory.mockReturnValue(of(sampleCategory));
    const fixture = TestBed.createComponent(CategoriesPageComponent);
    fixture.detectChanges();
    fixture.componentInstance.openCreateModal();
    fixture.componentInstance.categoryForm.setValue({
      code: 'BEV-01',
      name: 'Beverages',
      description: 'Cold drinks',
      isActive: true,
    });
    fixture.componentInstance.submitForm();
    expect(mockCategoriesService.createCategory).toHaveBeenCalledWith({
      code: 'BEV-01',
      name: 'Beverages',
      description: 'Cold drinks',
    });
    expect(mockNotifications.success).toHaveBeenCalled();
  });

  it('should open edit modal and call updateCategory on submit', () => {
    mockCategoriesService.updateCategory.mockReturnValue(of(sampleCategory));
    const fixture = TestBed.createComponent(CategoriesPageComponent);
    fixture.detectChanges();
    fixture.componentInstance.openEditModal(sampleCategory);
    fixture.componentInstance.categoryForm.patchValue({ name: 'Electronics Pro', isActive: false });
    fixture.componentInstance.submitForm();
    expect(mockCategoriesService.updateCategory).toHaveBeenCalledWith({
      id: 'cat-1',
      payload: {
        code: 'elec-01',
        name: 'Electronics Pro',
        description: 'Hardware and devices',
        isActive: false,
      },
    });
    expect(mockNotifications.success).toHaveBeenCalled();
  });

  it('should delete category after user confirmation', async () => {
    mockConfirmService.confirm.mockResolvedValue(true);
    mockCategoriesService.deleteCategory.mockReturnValue(of(void 0));
    const fixture = TestBed.createComponent(CategoriesPageComponent);
    fixture.detectChanges();
    await fixture.componentInstance.confirmDelete(sampleCategory);
    expect(mockConfirmService.confirm).toHaveBeenCalled();
    expect(mockCategoriesService.deleteCategory).toHaveBeenCalledWith('cat-1');
    expect(mockNotifications.success).toHaveBeenCalled();
  });

  it('should not delete category when confirmation is cancelled', async () => {
    mockConfirmService.confirm.mockResolvedValue(false);
    const fixture = TestBed.createComponent(CategoriesPageComponent);
    fixture.detectChanges();
    await fixture.componentInstance.confirmDelete(sampleCategory);
    expect(mockConfirmService.confirm).toHaveBeenCalled();
    expect(mockCategoriesService.deleteCategory).not.toHaveBeenCalled();
  });
});
