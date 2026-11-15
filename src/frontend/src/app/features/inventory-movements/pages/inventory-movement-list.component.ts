import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgxPermissionsModule } from 'ngx-permissions';
import { finalize, map } from 'rxjs';
import {
  AdAlertComponent,
  AdBadgeComponent,
  AdButtonComponent,
  AdCardComponent,
  AdIconComponent,
  AdInputComponent,
  AdModalComponent,
  AdSearchBoxComponent,
  AdSelectComponent,
  AdSelectOption,
  AdTableCellDirective,
  AdTableColumn,
  AdTableComponent,
  AdTextareaComponent,
} from '@accionadev/ui-kit';
import { API_PATHS } from '../../../core/constants/api-paths.constants';
import { PERMISSIONS } from '../../../core/constants/permissions.constants';
import { ErrorHandlerService, UiErrorMessage } from '../../../core/services/error-handler.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Product } from '../../products/models/product.model';
import { ProductsService } from '../../products/services/products.service';
import { CreateInventoryMovementRequest } from '../models/create-inventory-movement-request.model';
import { InventoryMovementFilter } from '../models/inventory-movement-filter.model';
import { InventoryMovementType } from '../models/inventory-movement-type.type';
import { InventoryMovement } from '../models/inventory-movement.model';
import { InventoryMovementsService } from '../services/inventory-movements.service';

@Component({
  selector: 'app-inventory-movement-list',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    ReactiveFormsModule,
    NgxPermissionsModule,
    AdAlertComponent,
    AdBadgeComponent,
    AdButtonComponent,
    AdCardComponent,
    AdIconComponent,
    AdInputComponent,
    AdModalComponent,
    AdSearchBoxComponent,
    AdSelectComponent,
    AdTableCellDirective,
    AdTableComponent,
    AdTextareaComponent,
  ],
  templateUrl: './inventory-movement-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InventoryMovementListComponent implements OnInit {
  private readonly movementsService = inject(InventoryMovementsService);
  private readonly productsService = inject(ProductsService);
  private readonly errorHandler = inject(ErrorHandlerService);
  private readonly notifications = inject(NotificationService);

  public readonly permissions = PERMISSIONS;
  public readonly movements = signal<InventoryMovement[]>([]);
  public readonly activeProducts = signal<Product[]>([]);
  public readonly totalCount = signal(0);
  public readonly page = signal(1);
  public readonly pageSize = signal(10);
  public readonly search = signal('');
  public readonly productFilter = signal<string>('all');
  public readonly typeFilter = signal<string>('all');
  public readonly fromDate = signal('');
  public readonly toDate = signal('');
  public readonly isLoading = signal(false);
  public readonly isModalOpen = signal(false);
  public readonly isSaving = signal(false);
  public readonly modalError = signal<UiErrorMessage | null>(null);
  public readonly pageError = signal<UiErrorMessage | null>(null);

  public readonly columns: AdTableColumn[] = [
    { key: 'createdAtUtc', label: 'Fecha UTC', width: '165px' },
    { key: 'productSku', label: 'Producto' },
    { key: 'type', label: 'Tipo', width: '120px' },
    { key: 'quantity', label: 'Cantidad', align: 'right', width: '105px' },
    { key: 'stockBalance', label: 'Saldo (Ant -> Nvo)', align: 'right', width: '165px' },
    { key: 'reason', label: 'Motivo' },
    { key: 'performedByEmail', label: 'Responsable', width: '185px' },
  ];

  public readonly typeFilterOptions: AdSelectOption[] = [
    { label: 'Todos los tipos', value: 'all' },
    { label: 'Entrada (ENTRY)', value: 'ENTRY' },
    { label: 'Salida (EXIT)', value: 'EXIT' },
  ];

  public readonly movementTypeOptions: AdSelectOption[] = [
    { label: 'Entrada (ENTRY)', value: 'ENTRY' },
    { label: 'Salida (EXIT)', value: 'EXIT' },
  ];

  public readonly productFilterOptions = computed<AdSelectOption[]>(() => [
    { label: 'Todos los productos', value: 'all' },
    ...this.activeProducts().map((product) => ({
      label: `${product.sku} — ${product.name}`,
      value: product.id,
    })),
  ]);

  public readonly activeProductOptions = computed<AdSelectOption[]>(() =>
    this.activeProducts().map((product) => ({
      label: `${product.sku} — ${product.name} (Stock: ${product.stock})`,
      value: product.id,
    }))
  );

  public readonly movementForm = new FormGroup({
    productId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    type: new FormControl<InventoryMovementType>('ENTRY', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    quantity: new FormControl(1, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(1)],
    }),
    reason: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3), Validators.maxLength(300)],
    }),
  });

  private readonly formSnapshot = toSignal(
    this.movementForm.valueChanges.pipe(map(() => this.movementForm.getRawValue())),
    { initialValue: this.movementForm.getRawValue() }
  );

  public readonly selectedProduct = computed<Product | null>(() => {
    const productId = this.formSnapshot().productId;
    if (!productId) {
      return null;
    }

    return this.activeProducts().find((product) => product.id === productId) ?? null;
  });

  public readonly currentStock = computed<number>(() => this.selectedProduct()?.stock ?? 0);

  public readonly projectedStock = computed<number | null>(() => {
    const product = this.selectedProduct();
    if (!product) {
      return null;
    }
    const snapshot = this.formSnapshot();
    const quantity = Math.max(0, Math.trunc(Number(snapshot.quantity) || 0));
    const delta = snapshot.type === 'ENTRY' ? quantity : -quantity;

    return product.stock + delta;
  });

  public readonly isInsufficientStock = computed<boolean>(() => {
    const projected = this.projectedStock();

    return projected !== null && projected < 0;
  });

  public ngOnInit(): void {
    this.loadActiveProducts();
    this.loadMovements();
  }

  public onSearchChange(query: string): void {
    this.search.set(query.trim());
    this.page.set(1);
    this.loadMovements();
  }

  public onProductFilterChange(productId: string): void {
    this.productFilter.set(productId || 'all');
    this.page.set(1);
    this.loadMovements();
  }

  public onTypeFilterChange(type: string): void {
    this.typeFilter.set(type || 'all');
    this.page.set(1);
    this.loadMovements();
  }

  public onFromDateChange(dateText: string): void {
    this.fromDate.set(dateText);
    this.applyDateFilterUpdate(dateText);
  }

  public onToDateChange(dateText: string): void {
    this.toDate.set(dateText);
    this.applyDateFilterUpdate(dateText);
  }

  public onPageChange(nextPage: number): void {
    this.page.set(nextPage);
    this.loadMovements();
  }

  public clearFilters(): void {
    this.search.set('');
    this.productFilter.set('all');
    this.typeFilter.set('all');
    this.fromDate.set('');
    this.toDate.set('');
    this.page.set(1);
    this.pageError.set(null);
    this.loadMovements();
  }

  public openCreateModal(): void {
    this.modalError.set(null);
    this.movementForm.reset({
      productId: this.activeProducts()[0]?.id ?? '',
      type: 'ENTRY',
      quantity: 1,
      reason: '',
    });
    this.isModalOpen.set(true);
  }

  public closeModal(): void {
    this.isModalOpen.set(false);
    this.modalError.set(null);
  }

  public clearModalError(): void {
    this.modalError.set(null);
  }

  public clearPageError(): void {
    this.pageError.set(null);
  }

  public submitForm(): void {
    if (this.movementForm.invalid || this.isSaving()) {
      this.movementForm.markAllAsTouched();

      return;
    }
    this.modalError.set(null);
    this.isSaving.set(true);
    this.executeCreate();
  }

  private applyDateFilterUpdate(dateText: string): void {
    const trimmed = dateText.trim();
    if (trimmed && !this.isValidDateText(trimmed)) {
      return;
    }
    this.page.set(1);
    this.loadMovements();
  }

  private loadActiveProducts(): void {
    this.productsService.getProducts({ page: 1, pageSize: 100, isActive: true }).subscribe({
      next: (response) => this.activeProducts.set([...response.items]),
      error: (error: HttpErrorResponse) => this.pageError.set(this.resolveUiError(error)),
    });
  }

  private loadMovements(): void {
    if (this.hasInvalidDateRange()) {
      this.pageError.set({
        title: 'Rango de fechas inválido',
        message: 'La fecha inicial no puede ser posterior a la fecha final.',
      });

      return;
    }
    this.pageError.set(null);
    this.isLoading.set(true);
    this.movementsService
      .getMovements(this.buildCurrentFilter())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => this.applyPagedResponse(response.items, response.totalCount),
        error: (error: HttpErrorResponse) => this.pageError.set(this.resolveUiError(error)),
      });
  }

  private applyPagedResponse(items: readonly InventoryMovement[], totalCount: number): void {
    this.movements.set([...items]);
    this.totalCount.set(totalCount);
  }

  private executeCreate(): void {
    this.movementsService
      .createMovement(this.buildCreatePayload())
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => this.handleCreateSuccess(),
        error: (error: HttpErrorResponse) => this.modalError.set(this.resolveUiError(error)),
      });
  }

  private handleCreateSuccess(): void {
    this.notifications.success('Movimiento registrado', 'El movimiento de inventario quedó asentado en el Kardex.');
    this.closeModal();
    this.loadActiveProducts();
    this.loadMovements();
  }

  private buildCreatePayload(): CreateInventoryMovementRequest {
    const raw = this.movementForm.getRawValue();

    return {
      productId: raw.productId,
      type: raw.type,
      quantity: Math.trunc(Number(raw.quantity)),
      reason: raw.reason.trim(),
    };
  }

  private buildCurrentFilter(): InventoryMovementFilter {
    const selectedProduct = this.productFilter();

    return {
      page: this.page(),
      pageSize: this.pageSize(),
      search: this.search() || undefined,
      productId: selectedProduct === 'all' ? null : selectedProduct,
      type: this.resolveTypeFilter(),
      fromDateUtc: this.toUtcBoundaryIso(this.fromDate(), false),
      toDateUtc: this.toUtcBoundaryIso(this.toDate(), true),
    };
  }

  private resolveTypeFilter(): InventoryMovementType | null {
    const selectedType = this.typeFilter();
    if (selectedType === 'ENTRY' || selectedType === 'EXIT') {
      return selectedType;
    }

    return null;
  }

  private hasInvalidDateRange(): boolean {
    const fromIso = this.toUtcBoundaryIso(this.fromDate(), false);
    const toIso = this.toUtcBoundaryIso(this.toDate(), true);

    return Boolean(fromIso && toIso && fromIso > toIso);
  }

  private isValidDateText(value: string): boolean {
    return /^\d{4}-\d{2}-\d{2}$/.test(value.trim());
  }

  private toUtcBoundaryIso(dateText: string, endOfDay: boolean): string | null {
    const trimmed = dateText.trim();
    if (!this.isValidDateText(trimmed)) {
      return null;
    }
    const suffix = endOfDay ? 'T23:59:59.999Z' : 'T00:00:00.000Z';
    const parsed = new Date(`${trimmed}${suffix}`);

    return Number.isNaN(parsed.getTime()) ? null : parsed.toISOString();
  }

  private resolveUiError(error: HttpErrorResponse): UiErrorMessage {
    return (
      this.errorHandler.resolveError(error, API_PATHS.INVENTORY_MOVEMENTS.BASE) ?? {
        title: 'Error',
        message: 'No se pudo completar la operación solicitada.',
      }
    );
  }
}
