import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ButtonComponent } from './button.component';

describe('ButtonComponent', () => {
  let component: ButtonComponent;
  let fixture: ComponentFixture<ButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('debe crearse correctamente', () => {
    expect(component).toBeTruthy();
  });

  it('debe aplicar la clase de variante primary por defecto', () => {
    const buttonElement = fixture.nativeElement.querySelector('button');
    expect(buttonElement.className).toContain('bg-primary-700');
  });

  it('debe deshabilitar el botón y mostrar el spinner cuando loading es true', () => {
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();

    const buttonElement = fixture.nativeElement.querySelector('button');
    expect(buttonElement.disabled).toBe(true);
    expect(buttonElement.querySelector('.animate-spin')).toBeTruthy();
  });

  it('debe emitir el evento clicked al hacer click cuando no está deshabilitado', () => {
    const clickedSpy = jest.fn();
    component.clicked.subscribe(clickedSpy);

    const buttonElement = fixture.nativeElement.querySelector('button');
    buttonElement.click();

    expect(clickedSpy).toHaveBeenCalledTimes(1);
  });

  it('no debe emitir clicked si el botón está deshabilitado', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();

    const clickedSpy = jest.fn();
    component.clicked.subscribe(clickedSpy);

    const buttonElement = fixture.nativeElement.querySelector('button');
    buttonElement.click();

    expect(clickedSpy).not.toHaveBeenCalled();
  });
});
