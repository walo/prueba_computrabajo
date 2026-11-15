import { Injectable, inject, signal, computed } from '@angular/core';
import { FeatureHttpService, FeatureDto } from './service-http.example';

export interface FeatureFilter {
  search: string;
  status?: string;
}

@Injectable({ providedIn: 'root' })
export class FeatureSignalStore {
  private readonly featureService = inject(FeatureHttpService);

  // Estado reactivo interno encapsulado vía Signals
  private readonly _items = signal<FeatureDto[]>([]);
  private readonly _loading = signal<boolean>(false);
  private readonly _filter = signal<FeatureFilter>({ search: '' });

  // Señales de solo lectura expuestas hacia los componentes
  readonly items = this._items.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly filter = this._filter.asReadonly();

  // Señales computadas derivadas
  readonly totalCount = computed(() => this._items().length);
  readonly filteredItems = computed(() => {
    const query = this._filter().search.toLowerCase().trim();
    if (!query) return this._items();

    return this._items().filter(
      (item) =>
        item.name.toLowerCase().includes(query) ||
        item.code.toLowerCase().includes(query)
    );
  });

  loadAll(): void {
    this._loading.set(true);
    this.featureService.getItems().subscribe({
      next: (response) => {
        this._items.set(response.data);
        this._loading.set(false);
      },
      error: () => this._loading.set(false),
    });
  }

  setFilter(filter: Partial<FeatureFilter>): void {
    this._filter.update((current) => ({ ...current, ...filter }));
  }

  removeItem(id: string): void {
    this._items.update((current) => current.filter((i) => i.id !== id));
  }
}
