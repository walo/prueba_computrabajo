import { TestBed } from '@angular/core/testing';
import { NotificationService } from './notification.service.example';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    jest.useFakeTimers();
    TestBed.configureTestingModule({
      providers: [NotificationService],
    });
    service = TestBed.inject(NotificationService);
  });

  afterEach(() => {
    jest.clearAllTimers();
    jest.useRealTimers();
  });

  it('debe inicializarse con un listado vacío de notificaciones', () => {
    expect(service.toasts()).toEqual([]);
  });

  it('debe agregar una notificación exitosa al invocar success()', () => {
    service.success('Operación completada con éxito');

    const toasts = service.toasts();
    expect(toasts.length).toBe(1);
    expect(toasts[0].type).toBe('success');
    expect(toasts[0].message).toBe('Operación completada con éxito');
    expect(toasts[0].id).toBeDefined();
  });

  it('debe agregar una notificación de error al invocar error()', () => {
    service.error('Falló la conexión con el servidor');

    const toasts = service.toasts();
    expect(toasts.length).toBe(1);
    expect(toasts[0].type).toBe('error');
    expect(toasts[0].message).toBe('Falló la conexión con el servidor');
  });

  it('debe eliminar la notificación manualmente al invocar dismiss()', () => {
    service.info('Mensaje informativo', 0); // duration 0 = no auto dismiss
    const id = service.toasts()[0].id;

    service.dismiss(id);
    expect(service.toasts().length).toBe(0);
  });

  it('debe auto-eliminar la notificación transcurrido el tiempo configurado', () => {
    service.show('Mensaje temporal', 'info', 3000);
    expect(service.toasts().length).toBe(1);

    jest.advanceTimersByTime(3000);

    expect(service.toasts().length).toBe(0);
  });
});
