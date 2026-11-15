// src/environments/environment.ts (Desarrollo local por defecto)
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1',
  appName: 'Sistema de Gestión',
  appSubtitle: 'Enterprise Portal',
  /** Ruta pública al isotipo del cliente en public/ (SVG o PNG transparente >= 128x128px). Vacío = usa logoIcon */
  logoUrl: '',
  /** Fallback Lucide de <ad-icon> compartido en Login, AppShell y vistas de plataforma */
  logoIcon: 'layers',
  companyName: 'AccionaDev Technologies',
  version: '1.0.0',
};

// src/environments/environment.prod.ts (Producción)
// export const environment = {
//   production: true,
//   apiUrl: 'https://api.tudominio.com/api/v1',
//   appName: 'Sistema de Gestión',
//   appSubtitle: 'Enterprise Portal',
//   logoUrl: '',
//   logoIcon: 'layers',
//   companyName: 'AccionaDev Technologies',
//   version: '1.0.0',
// };

/**
 * CONFIGURACIÓN OBLIGATORIA EN angular.json (fileReplacements):
 * "configurations": {
 *   "production": {
 *     "fileReplacements": [
 *       {
 *         "replace": "src/environments/environment.ts",
 *         "with": "src/environments/environment.prod.ts"
 *       }
 *     ]
 *   }
 * }
 */
