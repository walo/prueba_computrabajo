#!/usr/bin/env node
// Andamiaje y validación de Historias de Usuario e Integración Jira Cloud.
//
//   node .agents/skills/req-user-story-creator/scripts/new-story.mjs create-jira "Título" "Descripción con Gherkin"
//   node .agents/skills/req-user-story-creator/scripts/new-story.mjs new "Título de la funcionalidad" [--key PROJ-12] [--slug slug]
//   node .agents/skills/req-user-story-creator/scripts/new-story.mjs check [KEY|--all]
//   node .agents/skills/req-user-story-creator/scripts/new-story.mjs list
//
// Sin dependencias externas: Node 18+ (Node 22 en el proyecto).

import { readdirSync, existsSync, readFileSync, mkdirSync, statSync } from 'node:fs';
import { join, dirname, basename, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { execSync } from 'node:child_process';

const rojo = (s) => `\x1b[31m${s}\x1b[0m`;
const verde = (s) => `\x1b[32m${s}\x1b[0m`;
const ambar = (s) => `\x1b[33m${s}\x1b[0m`;
const cian = (s) => `\x1b[36m${s}\x1b[0m`;

function raizRepo() {
  let d = process.cwd();
  while (true) {
    if (existsSync(join(d, 'specs')) || existsSync(join(d, '.git'))) return d;
    const padre = dirname(d);
    if (padre === d) break;
    d = padre;
  }
  return resolve(dirname(fileURLToPath(import.meta.url)), '..', '..', '..', '..');
}

const RAIZ = raizRepo();
const SPECS = join(RAIZ, 'specs');

function entradasSpecs() {
  if (!existsSync(SPECS)) {
    mkdirSync(SPECS, { recursive: true });
    return [];
  }
  return readdirSync(SPECS);
}

function slugificar(texto) {
  return texto
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '_')
    .replace(/^_+|_+$/g, '')
    .slice(0, 60);
}

function normalizarKey(key) {
  const trimmed = key.trim();
  if (/^\d{1,3}$/.test(trimmed)) {
    return `US-${trimmed.padStart(3, '0')}`;
  }
  return trimmed.toUpperCase();
}

function listarCarpetasSpecs() {
  return entradasSpecs()
    .map((nombre) => join(SPECS, nombre))
    .filter((ruta) => statSync(ruta).isDirectory());
}

function comandoList() {
  const carpetas = listarCarpetasSpecs();
  console.log(cian(`\n--- Especificaciones Técnicas en specs/ (${carpetas.length}) ---`));
  if (!carpetas.length) {
    console.log(ambar('No hay carpetas de especificación creadas en specs/.'));
    return;
  }
  for (const c of carpetas) {
    const nombre = basename(c);
    const archivos = readdirSync(c);
    const hasInterview = archivos.some((f) => f.includes('interview') && f.endsWith('.md'));
    const hasSpec = archivos.some((f) => f.endsWith('.spec.md'));
    const hasPlan = archivos.some((f) => f.includes('plan') && f.endsWith('.md'));
    const hasTasks = archivos.some((f) => f.includes('tasks') && f.endsWith('.md'));

    console.log(`\n📁 ${verde(nombre)}`);
    console.log(`   Entrevista: ${hasInterview ? verde('✓') : ambar('Pendiente (init)')}`);
    console.log(`   Spec:       ${hasSpec ? verde('✓') : ambar('Pendiente')}`);
    console.log(`   Plan:       ${hasPlan ? verde('✓') : ambar('Pendiente')}`);
    console.log(`   Tasks:      ${hasTasks ? verde('✓') : ambar('Pendiente')}`);
  }
}

function comandoNew(args) {
  const titulo = args.find((a) => !a.startsWith('--'));
  if (!titulo) {
    console.error(rojo('Error: Falta el título de la funcionalidad. Ej: new "Gestión de Cuotas" --key PROJ-12'));
    process.exit(2);
  }

  const opcion = (nombre, porDefecto) => {
    const i = args.indexOf(`--${nombre}`);
    return i !== -1 && args[i + 1] ? args[i + 1] : porDefecto;
  };

  const keyDada = opcion('key', null);
  let keyFinal = keyDada ? normalizarKey(keyDada) : null;

  if (!keyFinal) {
    const carpetas = entradasSpecs().filter((n) => statSync(join(SPECS, n)).isDirectory());
    let max = 0;
    for (const c of carpetas) {
      const m = /^US-(\d{3})/i.exec(c);
      if (m) max = Math.max(max, Number(m[1]));
    }
    keyFinal = `US-${String(max + 1).padStart(3, '0')}`;
  }

  const slug = slugificar(opcion('slug', titulo));
  const carpeta = join(SPECS, `${keyFinal}_${slug}`);

  if (existsSync(carpeta)) {
    console.error(rojo(`Error: Ya existe la carpeta ${carpeta}`));
    process.exit(1);
  }

  mkdirSync(carpeta, { recursive: true });

  console.log(verde(`\n✔ Carpeta técnica andamiada en Git:`));
  console.log(`  ${carpeta}`);
  console.log(cian(`\nSiguiente paso (Refinamiento con el usuario):`));
  console.log(`  node .agents/skills/req-user-story-interviewer/scripts/interview-helper.mjs init ${keyFinal}`);
}

function comandoCreateJira(args) {
  const [titulo, descripcion] = args.filter((a) => !a.startsWith('--'));
  if (!titulo || !descripcion) {
    console.error(rojo('Error: Se requieren título y descripción para crear la historia en Jira.'));
    console.error('Uso: node new-story.mjs create-jira "Título" "Descripción con Gherkin"');
    process.exit(2);
  }

  const rutaJiraClient = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..', 'req-jira', 'jira_client.js');
  if (!existsSync(rutaJiraClient)) {
    console.error(rojo(`No se encontró jira_client.js en ${rutaJiraClient}`));
    process.exit(2);
  }

  console.log(cian(`\nConectando con Jira Cloud para registrar la Historia de Usuario...`));

  try {
    const comandoCmd = `node "${rutaJiraClient}" create "${titulo.replace(/"/g, '\\"')}" "${descripcion.replace(/"/g, '\\"')}" "Story"`;
    const resultadoRaw = execSync(comandoCmd, { encoding: 'utf8' }).trim();

    let jsonRes;
    try {
      jsonRes = JSON.parse(resultadoRaw);
    } catch {
      console.log(resultadoRaw);
      console.error(rojo('Error al parsear respuesta JSON de Jira.'));
      process.exit(1);
    }

    if (jsonRes.status !== 'success') {
      console.error(rojo(`Error en creación de Jira: ${JSON.stringify(jsonRes)}`));
      process.exit(1);
    }

    const key = jsonRes.key;
    const url = jsonRes.url;
    const slug = slugificar(titulo);
    const carpeta = join(SPECS, `${key}_${slug}`);

    mkdirSync(carpeta, { recursive: true });

    console.log(verde(`\n🎉 Historia creada exitosamente en Jira Cloud!`));
    console.log(`📌 Issue Key: ${key}`);
    console.log(`🔗 URL: ${url}`);
    console.log(verde(`✔ Carpeta técnica andamiada en Git: specs/${key}_${slug}/`));
    console.log(cian(`\nSiguiente paso (Refinamiento y entrevista técnica):`));
    console.log(`  node .agents/skills/req-user-story-interviewer/scripts/interview-helper.mjs init ${key}`);
  } catch (err) {
    console.error(rojo(`\nError ejecutando jira_client.js: ${err.message}`));
    process.exit(1);
  }
}

function validarCarpeta(carpeta) {
  const errores = [];
  const avisos = [];
  const nombre = basename(carpeta);
  const archivos = readdirSync(carpeta);

  const m = /^([A-Z0-9]+(?:-\d+)?)_([\p{Ll}\d_]+)$/u.exec(nombre);
  if (!m) {
    errores.push(`Nombre de carpeta inválido: "${nombre}". Formato esperado: <KEY>_<slug> (ej: PROJ-12_cuotas).`);
    return { errores, avisos };
  }

  const key = m[1];
  const interviewFile = archivos.find((f) => f.includes('interview') && f.endsWith('.md'));
  const specFile = archivos.find((f) => f.endsWith('.spec.md'));
  const planFile = archivos.find((f) => f.includes('plan') && f.endsWith('.md'));
  const tasksFile = archivos.find((f) => f.includes('tasks') && f.endsWith('.md'));

  if (!interviewFile) {
    avisos.push(`Falta el archivo de entrevista: ${key}-interview.md (inicialízalo con interview-helper init ${key}).`);
  }

  if (!specFile) {
    avisos.push(`Falta la especificación técnica: ${key}.spec.md (genérala con req-technical-spec-generator).`);
  } else {
    const specContenido = readFileSync(join(carpeta, specFile), 'utf8');
    if (!specContenido.includes(key)) {
      errores.push(`La especificación ${specFile} no referencia el Issue Key "${key}".`);
    }
    if (!/\[HasPermission\(/i.test(specContenido)) {
      errores.push(`La especificación ${specFile} no define permisos PBAC obligatorios con [HasPermission].`);
    }
  }

  if (!planFile) {
    avisos.push(`Falta el plan de arquitectura: ${key}-plan.md.`);
  }

  if (!tasksFile) {
    avisos.push(`Falta el desglose de tareas: ${key}-tasks.md.`);
  }

  return { errores, avisos, carpeta };
}

function comandoCheck(args) {
  let carpetas = [];
  if (args.includes('--all')) {
    carpetas = listarCarpetasSpecs();
  } else {
    const objetivo = args.find((a) => !a.startsWith('--'));
    if (!objetivo) {
      console.error(rojo('Indica una clave de historia: check PROJ-12 | check specs/PROJ-12_slug | check --all'));
      process.exit(2);
    }

    const key = normalizarKey(objetivo);
    const todas = listarCarpetasSpecs();
    const encontradas = todas.filter((c) => basename(c).startsWith(key + '_') || basename(c) === key);

    if (!encontradas.length) {
      console.error(rojo(`No se encontró ninguna carpeta en specs/ para ${objetivo}.`));
      process.exit(1);
    }
    carpetas = encontradas;
  }

  let fallos = 0;
  for (const carpeta of carpetas) {
    const { errores, avisos } = validarCarpeta(carpeta);
    const nombre = basename(carpeta);
    if (errores.length) {
      fallos++;
      console.log(rojo(`✘ ${nombre}`));
      for (const e of errores) console.log(`    ${rojo('·')} ${e}`);
    } else {
      console.log(verde(`✔ ${nombre}`));
    }
    for (const a of avisos) console.log(`    ${ambar('⚠')} ${a}`);
  }

  console.log(`\n${carpetas.length - fallos}/${carpetas.length} especificaciones válidas.`);
  process.exit(fallos ? 1 : 0);
}

// Router CLI
const [, , comando, ...args] = process.argv;
switch (comando) {
  case 'create-jira':
    comandoCreateJira(args);
    break;
  case 'new':
    comandoNew(args);
    break;
  case 'check':
    comandoCheck(args);
    break;
  case 'list':
    comandoList();
    break;
  default:
    console.log(`
Uso de new-story.mjs:
  node new-story.mjs create-jira "<Título>" "<Descripción con Gherkin>"
  node new-story.mjs new "<Título>" [--key PROJ-12] [--slug slug]
  node new-story.mjs check <KEY|--all>
  node new-story.mjs list
`);
    process.exit(comando ? 2 : 0);
}
