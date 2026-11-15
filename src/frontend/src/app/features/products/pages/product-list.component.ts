import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  AdAlertComponent,
  AdBadgeComponent,
  AdButtonComponent,
  AdCardComponent,
  AdConfirmService,
  AdIconComponent,
  AdInputComponent,
  AdModalComponent,
  AdSearchBoxComponent,
  AdSelectComponent,
  AdSelectOption,
  AdSwitchComponent,
  AdTableAction,
  AdTableActionEvent,
  AdTableActionsComponent,
  AdTableCellDirective,
  AdTableColumn,
  AdTableComponent,
  AdTextareaComponent,
} from '@accionadev/ui-kit';
import { NgxPermissionsModule, NgxPermissionsService } from 'ngx-permissions';
import { finalize } from 'rxjs';
import { PERMISSIONS } from '../../../core/constants/permissions.constants';
import { NotificationService } from '../../../core/services/notification.service';
import { Category } from '../../categories/models/category.model';
import { CategoriesService } from '../../categories/services/categories.service';
import { Product } from '../models/product.model';
import { ProductFilter } from '../models/product-filter.model';
import { ProductsService } from '../services/products.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgxPermissionsModule,
    AdCardComponent,
    AdAlertComponent,
    AdSearchBoxComponent,
    AdSelectComponent,
    AdSwitchComponent,
    AdButtonComponent,
    AdIconComponent,
    AdTableComponent,
    AdTableCellDirective,
    AdBadgeComponent,
    AdTableActionsComponent,
    AdModalComponent,
    AdInputComponent,
    AdTextareaComponent,
  ],
  templateUrl: './product-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductListComponent implements OnInit {
  private readonly productsService = inject(ProductsService);
  private readonly categoriesService = inject(CategoriesService);
  private readonly confirmService = inject(AdConfirmService);
  private readonly notifications = inject(NotificationService);
  private readonly permissionsService = inject(NgxPermissionsService);
  private readonly permissionsMap = toSignal(this.permissionsService.permissions$, {
    initialValue: this.permissionsService.getPermissions(),
  });

  public readonly permissions = PERMISSIONS;
  public readonly items = signal<readonly Product[]>([]);
  public readonly activeCategories = signal<readonly Category[]>([]);
  public readonly totalCount = signal<number>(0);
  public readonly pageIndex = signal<number>(1);
  public readonly pageSize = signal<number>(10);
  public readonly searchQuery = signal<string>('');
  public readonly categoryFilter = signal<string>('all');
  public readonly statusFilter = signal<string>('all');
  public readonly lowStockOnly = signal<boolean>(false);
  public readonly isLoading = signal<boolean>(false);
  public readonly isModalOpen = signal<boolean>(false);
  public readonly isSubmitting = signal<boolean>(false);
  public readonly editingProduct = signal<Product | null>(null);

  public readonly columns: AdTableColumn[] = [
    { key: 'sku', label: 'SKU', width: '130px' },
    { key: 'name', label: 'Producto' },
    { key: 'categoryName', label: 'Categoría', width: '160px' },
    { key: 'price', label: 'Precio', align: 'right', width: '120px' },
    { key: 'stock', label: 'Stock / Mín.', align: 'center', width: '150px' },
    { key: 'isActive', label: 'Estado', align: 'center', width: '120px' },
    { key: 'actions', label: 'Acciones', align: 'right', width: '110px' },
  ];

  public readonly statusOptions: AdSelectOption[] = [
    { label: 'Todos los estados', value: 'all' },
    { label: 'Activos', value: 'active' },
    { label: 'Inactivos', value: 'inactive' },
  ];

  public readonly filterCategoryOptions = computed<AdSelectOption[]>(() => [
    { label: 'Todas las categorías', value: 'all' },
    ...this.activeCategories().map((category) => ({ label: category.name, value: category.id })),
  ]);

  public readonly formCategoryOptions = computed<AdSelectOption[]>(() => {
    const baseOptions: AdSelectOption[] = this.activeCategories().map((category) => ({
      label: `${category.code} · ${category.name}`,
      value: category.id,
    }));
    const current = this.editingProduct();
    if (current && !baseOptions.some((option) => option.value === current.categoryId)) {
      return [
        { label: `${current.categoryCode} · ${current.categoryName} (Inactiva)`, value: current.categoryId },
        ...baseOptions,
      ];
    }

    return baseOptions;
  });

  public readonly rowActions = computed<AdTableAction[]>(() => {
    const granted = this.permissionsMap();
    const actions: AdTableAction[] = [];
    if (granted[PERMISSIONS.PRODUCTS.UPDATE]) {
      actions.push({ id: 'edit', label: 'Editar', icon: 'pencil', variant: 'ghost' });
    }
    if (granted[PERMISSIONS.PRODUCTS.DELETE]) {
      actions.push({ id: 'delete', label: 'Eliminar', icon: 'trash', danger: true });
    }

    return actions;
  });

  public readonly productForm = new FormGroup({
    sku: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(30),
        Validators.pattern(/^[a-zA-Z0-9_-]+$/),
      ],
    }),
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(2), Validators.maxLength(150)],
    }),
    description: new FormControl<string | null>(null, {
      validators: [Validators.maxLength(500)],
    }),
    categoryId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    price: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0)],
    }),
    initialStock: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0)],
    }),
    minStock: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0)],
    }),
    isActive: new FormControl(true, { nonNullable: true }),
  });

  public ngOnInit(): void {
    this.loadActiveCategories();
    this.loadProducts();
  }

  public onSearchChange(query: string): void {
    this.searchQuery.set(query.trim());
    this.pageIndex.set(1);
    this.loadProducts();
  }

  public onCategoryFilterChange(categoryId: string): void {
    this.categoryFilter.set(categoryId || 'all');
    this.pageIndex.set(1);
    this.loadProducts();
  }

  public onStatusFilterChange(status: string): void {
    this.statusFilter.set(status || 'all');
    this.pageIndex.set(1);
    this.loadProducts();
  }

  public onLowStockFilterChange(checked: boolean): void {
    this.lowStockOnly.set(checked);
    this.pageIndex.set(1);
    this.loadProducts();
  }

  public onPageChange(page: number): void {
    this.pageIndex.set(page);
    this.loadProducts();
  }

  public openCreateModal(): void {
    this.editingProduct.set(null);
    this.productForm.reset({
      sku: '',
      name: '',
      description: null,
      categoryId: this.activeCategories()[0]?.id ?? '',
      price: 0,
      initialStock: 0,
      minStock: 0,
      isActive: true,
    });
    this.isModalOpen.set(true);
  }

  public openEditModal(product: Product): void {
    this.editingProduct.set(product);
    this.productForm.reset({
      sku: product.sku,
      name: product.name,
      description: product.description,
      categoryId: product.categoryId,
      price: product.price,
      initialStock: product.stock,
      minStock: product.minStock,
      isActive: product.isActive,
    });
    this.isModalOpen.set(true);
  }

  public closeModal(): void {
    this.isModalOpen.set(false);
    this.editingProduct.set(null);
  }

  public onModalOpenChange(open: boolean): void {
    if (!open) {
      this.closeModal();
    }
  }

  public submitForm(): void {
    if (this.productForm.invalid || this.isSubmitting()) {
      this.productForm.markAllAsTouched();

      return;
    }
    this.isSubmitting.set(true);
    const current = this.editingProduct();
    if (current) {
      this.executeUpdate(current.id);

      return;
    }
    this.executeCreate();
  }

  public handleRowAction(event: AdTableActionEvent): void {
    const product = event.row as Product;
    if (event.actionId === 'edit') {
      this.openEditModal(product);
    }
    if (event.actionId === 'delete') {
      void this.confirmDelete(product);
    }
  }

  public async confirmDelete(product: Product): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Eliminar producto',
      message: `¿Deseas eliminar el producto "${product.name}" (${product.sku})? Esta acción no se puede deshacer.`,
      confirmText: 'Eliminar',
      cancelText: 'Cancelar',
      variant: 'danger',
      icon: 'trash',
    });
    if (!confirmed) {
      return;
    }
    this.isLoading.set(true);
    this.productsService
      .deleteProduct(product.id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.notifications.success('Producto eliminado', `El producto "${product.sku}" fue eliminado.`);
          this.loadProducts();
        },
      });
  }

  public formatPrice(price: number): string {
    return `$${Number(price).toFixed(2)}`;
  }

  private loadActiveCategories(): void {
    this.categoriesService
      .getCategories({ page: 1, pageSize: 100, isActive: true })
      .subscribe({
        next: (response) => this.activeCategories.set(response.items),
      });
  }

  private loadProducts(): void {
    this.isLoading.set(true);
    const filter = this.buildCurrentFilter();
    this.productsService
      .getProducts(filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.items.set(response.items);
          this.totalCount.set(response.totalCount);
        },
      });
  }

  private buildCurrentFilter(): ProductFilter {
    const selectedCategory = this.categoryFilter();

    return {
      page: this.pageIndex(),
      pageSize: this.pageSize(),
      search: this.searchQuery() || undefined,
      categoryId: selectedCategory === 'all' ? null : selectedCategory,
      isActive: this.resolveStatusFilter(),
      lowStockOnly: this.lowStockOnly(),
    };
  }

  private resolveStatusFilter(): boolean | null {
    const status = this.statusFilter();
    if (status === 'active') {
      return true;
    }
    if (status === 'inactive') {
      return false;
    }

    return null;
  }

  private executeCreate(): void {
    const raw = this.productForm.getRawValue();
    this.productsService
      .createProduct({
        sku: raw.sku,
        name: raw.name,
        description: raw.description,
        categoryId: raw.categoryId,
        price: Number(raw.price),
        initialStock: Math.trunc(Number(raw.initialStock)),
        minStock: Math.trunc(Number(raw.minStock)),
      })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.notifications.success('Producto creado', 'El producto fue registrado correctamente.');
          this.closeModal();
          this.loadProducts();
        },
      });
  }

  private executeUpdate(productId: string): void {
    const raw = this.productForm.getRawValue();
    this.productsService
      .updateProduct({
        id: productId,
        payload: {
          sku: raw.sku,
          name: raw.name,
          description: raw.description,
          categoryId: raw.categoryId,
          price: Number(raw.price),
          minStock: Math.trunc(Number(raw.minStock)),
          isActive: raw.isActive,
        },
      })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.notifications.success('Producto actualizado', 'Los cambios del producto fueron guardados.');
          this.closeModal();
          this.loadProducts();
        },
      });
  }
}
