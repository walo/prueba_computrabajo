#!/usr/bin/env node
// Helper CLI para la habilidad req-user-story-interviewer (Entrevistas y Refinamiento SDD).
//
// Uso:
//   node .agent/skills/req-user-story-interviewer/scripts/interview-helper.mjs find US-056
//   node .agent/skills/req-user-story-interviewer/scripts/interview-helper.mjs init US-056
//   node .agent/skills/req-user-story-interviewer/scripts/interview-helper.mjs check US-056
//
// Sin dependencias externas: Node 18+ (Node 22 en el proyecto).

import { readdirSync, existsSync, readFileSync, writeFileSync, statSync } from 'node:fs';
import { join, dirname, basename, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROLES_VALIDOS = ['admin', 'manager', 'user', 'auditor', 'platform_admin', 'client_admin', 'super_admin', 'guest'];
const ROLES_INVENTADOS = [];


const SECCIONES_OBLIGATORIAS = [
  { patron: /^##\s*1\.\s*Resumen Ejecutivo/mi, nombre: '1. Resumen Ejecutivo y Objetivos Clarificados' },
  { patron: /^##\s*2\.\s*Registro de Preguntas/mi, nombre: '2. Registro de Preguntas y Resoluciones' },
  { patron: /^##\s*3\.\s*Matriz de Decisiones Técnicas/mi, nombre: '3. Matriz de Decisiones Técnicas por Stack' },
  { patron: /^##\s*4\.\s*Criterios de Aceptación Refinados/mi, nombre: '4. Criterios de Aceptación Refinados (Gherkin)' },
  { patron: /^##\s*5\.\s*Checklist de Preparación/mi, nombre: '5. Checklist de Preparación para SDD' },
];

const rojo = (s) => `\x1b[31m${s}\x1b[0m`;
const verde = (s) => `\x1b[32m${s}\x1b[0m`;
const ambar = (s) => `\x1b[33m${s}\x1b[0m`;
const cian = (s) => `\x1b[36m${s}\x1b[0m`;

function raizRepo() {
  let d = process.cwd();
  while (true) {
    if (existsSync(join(d, 'specs')) && existsSync(join(d, 'AGENTS.md'))) return d;
    const padre = dirname(d);
    if (padre === d) break;
    d = padre;
  }
  return resolve(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', '..');
}

const RAIZ = raizRepo();
const SPECS = join(RAIZ, 'specs');

function normalizarId(id) {
  const trimmed = id.trim();
  if (/^\d{1,3}$/.test(trimmed)) {
    return `US-${trimmed.padStart(3, '0')}`;
  }
  return trimmed.toUpperCase();
}

function buscarHistoria(idBuscado) {
  const codigo = normalizarId(idBuscado);
  if (!existsSync(SPECS)) {
    console.error(rojo(`No existe el directorio specs/ en ${RAIZ}`));
    process.exit(2);
  }

  const entradas = readdirSync(SPECS);
  for (const entrada of entradas) {
    const rutaCompleta = join(SPECS, entrada);
    if (!statSync(rutaCompleta).isDirectory()) continue;

    if (entrada.startsWith(codigo + '_') || entrada === codigo) {
      const archivos = readdirSync(rutaCompleta);
      const archivoHistoria = archivos.find((f) => f.startsWith(codigo) && f.endsWith('.md') && !f.includes('interview') && !f.includes('plan') && !f.includes('tasks') && !f.includes('spec'));
      const archivoInterview = archivos.find((f) => f.includes('interview') && f.endsWith('.md'));

      return {
        codigo,
        carpeta: entrada,
        rutaCarpeta: rutaCompleta,
        archivoHistoria: archivoHistoria ? join(rutaCompleta, archivoHistoria) : null,
        nombreHistoria: archivoHistoria || null,
        archivoInterview: archivoInterview ? join(rutaCompleta, archivoInterview) : null,
        nombreInterview: archivoInterview || null,
      };
    }
  }

  return null;
}

function comandoFind(args) {
  const id = args[0];
  if (!id) {
    console.error(rojo('Error: Debes especificar el código de historia o Jira Issue. Ejemplo: interview-helper.mjs find PROJ-12'));
    process.exit(1);
  }

  const info = buscarHistoria(id);
  if (!info) {
    console.error(rojo(`No se encontró ninguna carpeta para ${id} en ${SPECS}`));
    process.exit(1);
  }

  console.log(cian(`\n--- Historia / Feature Encontrada: ${info.codigo} ---`));
  console.log(`Carpeta: ${info.carpeta}`);
  console.log(`Ruta: ${info.rutaCarpeta}`);
  console.log(`Historia / Jira: ${info.archivoHistoria ? verde(info.nombreHistoria) : verde('Centralizada en Jira Cloud')}`);
  console.log(`Archivo de Entrevista: ${info.archivoInterview ? verde(info.nombreInterview) : ambar('Pendiente de crear (usa `init`)')}`);
}

function comandoInit(args) {
  const id = args[0];
  if (!id) {
    console.error(rojo('Error: Debes especificar el código de historia. Ejemplo: interview-helper.mjs init US-056'));
    process.exit(1);
  }

  const info = buscarHistoria(id);
  if (!info) {
    console.error(rojo(`No se encontró ninguna carpeta para la historia ${id} en ${SPECS}`));
    process.exit(1);
  }

  const rutaPlantilla = resolve(dirname(fileURLToPath(import.meta.url)), '..', 'resources', 'interview_template.md');
  if (!existsSync(rutaPlantilla)) {
    console.error(rojo(`No se encontró la plantilla de entrevista en ${rutaPlantilla}`));
    process.exit(2);
  }

  const destino = join(info.rutaCarpeta, `${info.codigo}-interview.md`);
  if (existsSync(destino)) {
    console.log(ambar(`El archivo de entrevista ya existe en:\n  ${destino}`));
    console.log(`Si deseas sobreescribirlo, elimínalo primero.`);
    return;
  }

  let contenido = readFileSync(rutaPlantilla, 'utf8');

  // Intentar leer metadatos de la historia
  let tituloHistoria = info.carpeta.replace(new RegExp(`^${info.codigo}_`), '').replace(/_/g, ' ');
  let nombreArchivoHistoria = info.nombreHistoria || `${info.carpeta}.md`;

  if (info.archivoHistoria && existsSync(info.archivoHistoria)) {
    const textoHistoria = readFileSync(info.archivoHistoria, 'utf8');
    const mTitulo = /^#\s*Historia de Usuario:\s*(.+)$/m.exec(textoHistoria);
    if (mTitulo) tituloHistoria = mTitulo[1].trim();
  }

  const fechaHoy = new Date().toISOString().slice(0, 10);
  contenido = contenido
    .replace(/\[Título de la Historia\]/g, tituloHistoria)
    .replace(/US-\[NÚMERO\]/g, info.codigo)
    .replace(/\[KEY\]/g, info.codigo)
    .replace(/\[AAAA-MM-DD\]/g, fechaHoy);

  writeFileSync(destino, contenido, 'utf8');
  console.log(verde(`\n✓ Archivo de entrevista inicializado con éxito:`));
  console.log(`  ${destino}`);
  console.log(cian(`\nSiguiente paso: Conducir la entrevista con el usuario y completar los bloques en el documento.`));
}

function comandoCheck(args) {
  const id = args[0];
  if (!id) {
    console.error(rojo('Error: Debes especificar el código de historia. Ejemplo: interview-helper.mjs check US-056'));
    process.exit(1);
  }

  const info = buscarHistoria(id);
  if (!info) {
    console.error(rojo(`No se encontró ninguna carpeta para la historia ${id} en ${SPECS}`));
    process.exit(1);
  }

  const archivo = info.archivoInterview || join(info.rutaCarpeta, `${info.codigo}-interview.md`);
  if (!existsSync(archivo)) {
    console.error(rojo(`No existe el archivo de entrevista para ${info.codigo}:\n  ${archivo}`));
    console.error(`Inicialízalo con: node .agent/skills/req-user-story-interviewer/scripts/interview-helper.mjs init ${info.codigo}`);
    process.exit(1);
  }

  const contenido = readFileSync(archivo, 'utf8');
  const errores = [];
  const advertencias = [];

  // 1. Validar secciones obligatorias
  for (const s of SECCIONES_OBLIGATORIAS) {
    if (!s.patron.test(contenido)) {
      errores.push(`Falta la sección obligatoria: "${s.nombre}"`);
    }
  }

  // 2. Comprobar placeholders sin rellenar [...]
  // Ignoramos código en bloque y texto en backticks
  const sinCodigo = contenido
    .replace(/```[\s\S]*?```/g, '')
    .replace(/`[^`\n]+`/g, '')
    .replace(/\[!NOTE\]/g, '')
    .replace(/\[!IMPORTANT\]/g, '')
    .replace(/\[!WARNING\]/g, '')
    .replace(/\[!TIP\]/g, '')
    .replace(/\[!CAUTION\]/g, '');

  const placeholders = [...sinCodigo.matchAll(/\[([^[\]\n]{3,60})\]/g)]
    .map((m) => m[1].trim())
    .filter((txt) => !txt.startsWith('x') && !txt.startsWith(' ') && !txt.startsWith('http') && !txt.endsWith('.md') && !txt.startsWith('US-'));

  if (placeholders.length > 0) {
    const lista = placeholders.slice(0, 5).join(', ');
    const mas = placeholders.length > 5 ? ` (y ${placeholders.length - 5} más)` : '';
    errores.push(`Contiene ${placeholders.length} marcador(es) de plantilla sin rellenar: [${lista}]${mas}`);
  }

  // 3. Validar roles
  for (const rol of ROLES_INVENTADOS) {
    const re = new RegExp(`\\b${rol}\\b`, 'i');
    if (re.test(sinCodigo)) {
      errores.push(`Uso de rol no válido en el schema: "${rol}". Usa solo: ${ROLES_VALIDOS.join(', ')}`);
    }
  }

  // 4. Validar multi-tenancy (company_id, tenant_id o client_id)
  const hasTenant = /\b(company_id|tenant_id|client_id)\b/i.test(sinCodigo);
  if (!hasTenant) {
    advertencias.push('No se menciona explícitamente identificador de tenancy ("company_id", "tenant_id" o "client_id") para proyectos multi-tenant.');
  }


  // 5. Validar escenarios Gherkin (mínimo 3)
  const dados = (sinCodigo.match(/\*\*Dado\*\*/gi) || []).length;
  const cuandos = (sinCodigo.match(/\*\*Cuando\*\*/gi) || []).length;
  const entonces = (sinCodigo.match(/\*\*Entonces\*\*/gi) || []).length;
  const numGherkin = Math.min(dados, cuandos, entonces);

  if (numGherkin < 3) {
    errores.push(`Se requieren al menos 3 escenarios Gherkin completos (Dado/Cuando/Entonces). Encontrados: ${numGherkin}`);
  }

  // Reporte final
  console.log(cian(`\n--- Auditoría de Entrevista: ${info.codigo} ---`));
  console.log(`Archivo: ${archivo}`);

  if (advertencias.length) {
    console.log(ambar('\nAdvertencias:'));
    advertencias.forEach((a) => console.log(ambar(`  ⚠ ${a}`)));
  }

  if (errores.length) {
    console.log(rojo('\nErrores detectados:'));
    errores.forEach((e) => console.log(rojo(`  ✗ ${e}`)));
    console.log(rojo(`\nResultado: RECHAZADO (${errores.length} error(es)). Corrige el documento antes de pasar a la especificación técnica.`));
    process.exit(1);
  } else {
    console.log(verde('\n✓ Entrevista validada con éxito. Cumple con todos los estándares técnicos y de negocio.'));
    console.log(verde('  Lista para generar la especificación técnica con req-technical-spec-generator.'));
    process.exit(0);
  }
}

// Router CLI
const [, , comando, ...resto] = process.argv;

switch (comando) {
  case 'find':
    comandoFind(resto);
    break;
  case 'init':
    comandoInit(resto);
    break;
  case 'check':
    comandoCheck(resto);
    break;
  default:
    console.log(`\nUso de interview-helper.mjs:
  node .agent/skills/req-user-story-interviewer/scripts/interview-helper.mjs find <US-XXX>
  node .agent/skills/req-user-story-interviewer/scripts/interview-helper.mjs init <US-XXX>
  node .agent/skills/req-user-story-interviewer/scripts/interview-helper.mjs check <US-XXX>
`);
    process.exit(1);
}
