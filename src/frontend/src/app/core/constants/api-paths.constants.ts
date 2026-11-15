export const API_PATHS = {
  AUTH: {
    LOGIN: '/auth/login',
    REFRESH: '/auth/refresh',
    LOGOUT: '/auth/logout',
    ME: '/auth/me',
  },
  USERS: {
    BASE: '/users',
  },
  CATEGORIES: {
    BASE: '/categories',
  },
  PRODUCTS: {
    BASE: '/products',
  },
  INVENTORY_MOVEMENTS: {
    BASE: '/inventory-movements',
  },
} as const;
