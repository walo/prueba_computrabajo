export const PERMISSIONS = {
  USERS: {
    READ: 'Users.Read',
    CREATE: 'Users.Create',
    UPDATE: 'Users.Update',
    DELETE: 'Users.Delete',
  },
  ROLES: {
    READ: 'Roles.Read',
    CREATE: 'Roles.Create',
    UPDATE: 'Roles.Update',
    DELETE: 'Roles.Delete',
  },
  CATEGORIES: {
    READ: 'Categories.Read',
    CREATE: 'Categories.Create',
    UPDATE: 'Categories.Update',
    DELETE: 'Categories.Delete',
  },
  PRODUCTS: {
    READ: 'Products.Read',
    CREATE: 'Products.Create',
    UPDATE: 'Products.Update',
    DELETE: 'Products.Delete',
  },
  INVENTORY_MOVEMENTS: {
    READ: 'InventoryMovements.Read',
    CREATE: 'InventoryMovements.Create',
  },
} as const;
