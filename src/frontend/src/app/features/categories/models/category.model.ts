export interface Category {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly description: string | null;
  readonly isActive: boolean;
  readonly createdAtUtc: string;
  readonly updatedAtUtc: string | null;
}
