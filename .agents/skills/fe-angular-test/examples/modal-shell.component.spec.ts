import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { ModalShellComponent } from './modal-shell.organism.example';

@Component({
  standalone: true,
  imports: [ModalShellComponent],
  template: `
    <app-modal-shell
      [isOpen]="isOpen"
      [title]="title"
      (close)="onClose()"
    >
      <p id="test-body">Cuerpo de prueba</p>
      <button modal-actions id="test-action">Aceptar</button>
    </app-modal-shell>
  `,
})
class TestHostComponent {
  isOpen = true;
  title = 'Título Modal';
  closed = false;

  onClose(): void {
    this.closed = true;
  }
}

describe('ModalShellComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent, ModalShellComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('debe renderizar el contenido proyectado y el título cuando isOpen es true', () => {
    const titleEl = fixture.nativeElement.querySelector('h3');
    const bodyEl = fixture.nativeElement.querySelector('#test-body');
    const actionEl = fixture.nativeElement.querySelector('#test-action');

    expect(titleEl).toBeTruthy();
    expect(titleEl.textContent.trim()).toBe('Título Modal');
    expect(bodyEl).toBeTruthy();
    expect(actionEl).toBeTruthy();
  });

  it('no debe renderizar nada en el DOM cuando isOpen es false', () => {
    host.isOpen = false;
    fixture.detectChanges();

    const dialog = fixture.nativeElement.querySelector('[role="dialog"]');
    expect(dialog).toBeNull();
  });

  it('debe emitir close al hacer click en el botón de cerrar', () => {
    const closeBtn = fixture.nativeElement.querySelector('button[aria-label="Cerrar modal"]');
    expect(closeBtn).toBeTruthy();

    closeBtn.click();
    expect(host.closed).toBe(true);
  });

  it('debe emitir close al pulsar la tecla Escape', () => {
    const event = new KeyboardEvent('keydown', { key: 'Escape' });
    document.dispatchEvent(event);
    fixture.detectChanges();

    expect(host.closed).toBe(true);
  });
});
