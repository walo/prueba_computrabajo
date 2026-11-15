import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import {
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
import { Category } from '../models/category.model';
import { CategoryFilter } from '../models/category-filter.model';
import { CategoriesService } from '../services/categories.service';

@Component({
  selector: 'app-categories-page',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgxPermissionsModule,
    AdCardComponent,
    AdSearchBoxComponent,
    AdSelectComponent,
    AdButtonComponent,
    AdIconComponent,
    AdTableComponent,
    AdTableCellDirective,
    AdBadgeComponent,
    AdTableActionsComponent,
    AdModalComponent,
    AdInputComponent,
    AdTextareaComponent,
    AdSwitchComponent,
  ],
  templateUrl: './categories-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoriesPageComponent implements OnInit {
  private readonly categoriesService = inject(CategoriesService);
  private readonly confirmService = inject(AdConfirmService);
  private readonly notifications = inject(NotificationService);
  private readonly permissionsService = inject(NgxPermissionsService);
  private readonly permissionsMap = toSignal(this.permissionsService.permissions$, {
    initialValue: this.permissionsService.getPermissions(),
  });

  public readonly permissions = PERMISSIONS;
  public readonly items = signal<readonly Category[]>([]);
  public readonly totalCount = signal<number>(0);
  public readonly pageIndex = signal<number>(1);
  public readonly pageSize = signal<number>(10);
  public readonly searchQuery = signal<string>('');
  public readonly statusFilter = signal<string>('all');
  public readonly isLoading = signal<boolean>(false);
  public readonly isModalOpen = signal<boolean>(false);
  public readonly isSubmitting = signal<boolean>(false);
  public readonly editingCategory = signal<Category | null>(null);

  public readonly columns: AdTableColumn[] = [
    { key: 'code', label: 'Código', width: '160px' },
    { key: 'name', label: 'Nombre' },
    { key: 'description', label: 'Descripción' },
    { key: 'isActive', label: 'Estado', align: 'center', width: '140px' },
    { key: 'actions', label: 'Acciones', align: 'right', width: '120px' },
  ];

  public readonly statusOptions: AdSelectOption[] = [
    { label: 'Todos los estados', value: 'all' },
    { label: 'Activas', value: 'active' },
    { label: 'Inactivas', value: 'inactive' },
  ];

  public readonly rowActions = computed<AdTableAction[]>(() => {
    const granted = this.permissionsMap();
    const actions: AdTableAction[] = [];
    if (granted[PERMISSIONS.CATEGORIES.UPDATE]) {
      actions.push({ id: 'edit', label: 'Editar', icon: 'pencil', variant: 'ghost' });
    }
    if (granted[PERMISSIONS.CATEGORIES.DELETE]) {
      actions.push({ id: 'delete', label: 'Eliminar', icon: 'trash', danger: true });
    }

    return actions;
  });

  public readonly categoryForm = new FormGroup({
    code: new FormControl('', {
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
      validators: [Validators.required, Validators.minLength(2), Validators.maxLength(100)],
    }),
    description: new FormControl<string | null>(null, {
      validators: [Validators.maxLength(500)],
    }),
    isActive: new FormControl(true, { nonNullable: true }),
  });

  public ngOnInit(): void {
    this.loadCategories();
  }

  public onSearchChange(query: string): void {
    this.searchQuery.set(query.trim());
    this.pageIndex.set(1);
    this.loadCategories();
  }

  public onStatusFilterChange(status: string): void {
    this.statusFilter.set(status || 'all');
    this.pageIndex.set(1);
    this.loadCategories();
  }

  public onPageChange(page: number): void {
    this.pageIndex.set(page);
    this.loadCategories();
  }

  public openCreateModal(): void {
    this.editingCategory.set(null);
    this.categoryForm.reset({ code: '', name: '', description: null, isActive: true });
    this.isModalOpen.set(true);
  }

  public openEditModal(category: Category): void {
    this.editingCategory.set(category);
    this.categoryForm.reset({
      code: category.code,
      name: category.name,
      description: category.description,
      isActive: category.isActive,
    });
    this.isModalOpen.set(true);
  }

  public closeModal(): void {
    this.isModalOpen.set(false);
    this.editingCategory.set(null);
  }

  public onModalOpenChange(open: boolean): void {
    if (!open) {
      this.closeModal();
    }
  }

  public submitForm(): void {
    if (this.categoryForm.invalid || this.isSubmitting()) {
      this.categoryForm.markAllAsTouched();

      return;
    }
    this.isSubmitting.set(true);
    const current = this.editingCategory();
    if (current) {
      this.executeUpdate(current.id);

      return;
    }
    this.executeCreate();
  }

  public handleRowAction(event: AdTableActionEvent): void {
    const category = event.row as Category;
    if (event.actionId === 'edit') {
      this.openEditModal(category);
    }
    if (event.actionId === 'delete') {
      void this.confirmDelete(category);
    }
  }

  public async confirmDelete(category: Category): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Eliminar categoría',
      message: `¿Deseas eliminar la categoría "${category.name}"? Esta acción no se puede deshacer.`,
      confirmText: 'Eliminar',
      cancelText: 'Cancelar',
      variant: 'danger',
      icon: 'trash',
    });
    if (!confirmed) {
      return;
    }
    this.isLoading.set(true);
    this.categoriesService
      .deleteCategory(category.id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.notifications.success('Categoría eliminada', `La categoría "${category.name}" fue eliminada.`);
          this.loadCategories();
        },
      });
  }

  private loadCategories(): void {
    this.isLoading.set(true);
    const filter = this.buildCurrentFilter();
    this.categoriesService
      .getCategories(filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.items.set(response.items);
          this.totalCount.set(response.totalCount);
        },
      });
  }

  private buildCurrentFilter(): CategoryFilter {
    const search = this.searchQuery() || undefined;
    const isActive = this.resolveStatusFilter();

    return {
      page: this.pageIndex(),
      pageSize: this.pageSize(),
      search,
      isActive,
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
    const raw = this.categoryForm.getRawValue();
    this.categoriesService
      .createCategory({ code: raw.code, name: raw.name, description: raw.description })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.notifications.success('Categoría creada', 'La categoría fue registrada correctamente.');
          this.closeModal();
          this.loadCategories();
        },
      });
  }

  private executeUpdate(categoryId: string): void {
    const raw = this.categoryForm.getRawValue();
    this.categoriesService
      .updateCategory({
        id: categoryId,
        payload: {
          code: raw.code,
          name: raw.name,
          description: raw.description,
          isActive: raw.isActive,
        },
      })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.notifications.success('Categoría actualizada', 'Los cambios fueron guardados.');
          this.closeModal();
          this.loadCategories();
        },
      });
  }
}
