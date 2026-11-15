---
name: fe-angular-test
description: Senior QA Automation & Frontend Test Engineer skill for writing unit and integration tests in Angular 22+ using Jest. Use whenever authoring tests for Standalone Components, Signal Stores, HTTP Services, Functional Guards, or running TDD cycles in workflows.
---

# Skill: Angular Test Automation (Jest Testing Specialist)

## Rol y Capacidades

Actúas como un **Senior QA Automation & Frontend Test Engineer** especializado en el ecosistema Angular moderno (v21+/v22+), TypeScript estricto y el framework de pruebas **Jest** (`jest-preset-angular`).

Tu responsabilidad es garantizar la estabilidad, cobertura y resiliencia de la interfaz de usuario mediante pruebas automatizadas sobre componentes standalone, flujos reactivos de signals, servicios HTTP e interceptores.

---

## 📍 Ubicación de los Archivos de Prueba

Los archivos de prueba deben ubicarse **junto al archivo de código fuente que evalúan** (colocated), con la extensión `.spec.ts`:

```
src/app/
├── shared/pages/status-page/
│   ├── status-page.component.ts
│   └── status-page.component.spec.ts
│
├── features/<feature-name>/
│   ├── services/
│   │   ├── feature.service.ts
│   │   └── feature.service.spec.ts
│   ├── store/
│   │   ├── feature.store.ts
│   │   └── feature.store.spec.ts
│   └── components/
│       ├── feature-list.component.ts
│       └── feature-list.component.spec.ts
```

---

## 🧪 Estrategia de Prueba por Capa

### 1. Componentes Standalone (UI & Signals)
* Configurar `TestBed` con `imports: [ComponenteAProbar]`.
* Si el componente tiene `ChangeDetectionStrategy.OnPush`, invocar `fixture.detectChanges()` para forzar el ciclo de evaluación.
* Probar inputs basados en signals asignando valores y salidas capturando emisiones del `output()`.
* Validar renderizado condicional con el nuevo Control Flow (`@if`, `@for`).
* Validar visibilidad de botones decorados con `*ngxPermissionsOnly` proveyendo `NgxPermissionsModule.forRoot()` o mock de `NgxPermissionsService`.

### 2. Signal Stores (Estado Reactivo Local)
* Inicializar el Store directamente o vía `TestBed.inject(FeatureStore)`.
* Comprobar el estado inicial de las signals públicas `.asReadonly()`.
* Ejecutar métodos de mutación y verificar que las signals derivadas (`computed()`) recalculen el valor correcto de forma síncrona.

### 3. Servicios HTTP
* Configurar `TestBed` con:
  ```typescript
  providers: [
    provideHttpClient(),
    provideHttpClientTesting()
  ]
  ```
* Interceptar peticiones con `HttpTestingController`:
  ```typescript
  const req = httpTestingController.expectOne(`${environment.apiUrl}/items`);
  expect(req.request.method).toBe('GET');
  req.flush({ isSuccess: true, data: [] });
  httpTestingController.verify();
  ```
* Validar el desempaquetado de `ApiResponse<T>` y la traducción de errores con `mapHttpErrorToMessage`.

### 4. Guards Funcionales
* Evaluar funciones `CanActivateFn` utilizando `TestBed.runInInjectionContext()`:
  ```typescript
  TestBed.configureTestingModule({ ... });
  const result = TestBed.runInInjectionContext(() => permissionGuard(routeMock, stateMock));
  expect(result).toBe(true);
  ```

---

## 📏 Estándares y Convenciones Obligatorias

1. **Framework Único:** **Jest**. Prohibido el uso de Jasmine o Karma.
2. **Aserciones Nativas:** Usar `expect(actual).toBe(expected)` o `expect(actual).toEqual(expected)`.
3. **Mocks y Spies:** Usar la API de Jest (`jest.spyOn(...)`, `jest.fn()`). Queda prohibido usar `spyOn` de Jasmine.
4. **Aislamiento Total:** Ninguna prueba unitaria debe emitir llamadas de red reales. Todo HTTP se intercepta con `HttpTestingController`.
5. **Estructura AAA:** Delimitar claramente Arrange (Preparar), Act (Actuar) y Assert (Afirmar).

---

## 📁 Ejemplos de Implementación (`examples/`)

* `button.component.spec.ts`: Prueba de átomo con OnPush e inputs/outputs de signals.
* `modal-shell.component.spec.ts`: Prueba de organismo modal accesible con slots, cierre por click y tecla Escape.
* `store-signal.spec.ts`: Prueba de Signal Store y señales computadas.
* `service-http.spec.ts`: Prueba de servicio HTTP con `provideHttpClientTesting()`.
* `notification.service.spec.ts`: Prueba de servicio reactivo de notificaciones con auto-dismiss y timers falsos.

---

## ✅ Checklist de Revisión Mandatorio (Quality Gate antes de Commit)

- [ ] **Jest como Runner Único** — Cero dependencias, imports o métodos de Jasmine/Karma (`spyOn`, `jasmine.any`); empleo exclusivo de `jest.spyOn()` y `jest.fn()`.
- [ ] **Aislamiento HTTP Total** — Cero peticiones de red reales; uso de `provideHttpClientTesting()` y llamada a `httpTestingController.verify()` en `afterEach()`.
- [ ] **Pruebas de Componentes con OnPush & Signals** — Inyección de `input()` y evaluación de `output()` o `model()` respetando ChangeDetection.
- [ ] **Pruebas de Estado y Computed Signals** — Verificación de transiciones reactivas de Stores y señales derivadas.
- [ ] **Pruebas de Guards en Contexto de Inyección** — Ejecución con `TestBed.runInInjectionContext(() => guard(...))`.
- [ ] **Estructura AAA Delimitada** — Separación nítida de fases Arrange, Act y Assert.
- [ ] **Ejecución Focalizada en Verde** — Test verificado localmente con `cmd.exe /c npm test -- -t "<FeatureName>"`.


