/**
 * Catálogo centralizado de permisos del Frontend (PBAC).
 * Debe mantenerse sincronizado con Permissions.cs del Backend (.NET).
 */
export const Permissions = {
  Dashboard: {
    View: 'Dashboard.View',
  },
  Features: {
    Read: 'Features.Read',
    Create: 'Features.Create',
    Update: 'Features.Update',
    Delete: 'Features.Delete',
  },
  Users: {
    Read: 'Users.Read',
    Create: 'Users.Create',
    Update: 'Users.Update',
    Delete: 'Users.Delete',
  },
} as const;

export type PermissionKey =
  | typeof Permissions.Dashboard[keyof typeof Permissions.Dashboard]
  | typeof Permissions.Features[keyof typeof Permissions.Features]
  | typeof Permissions.Users[keyof typeof Permissions.Users];
