/**
 * Normaliza y formatea listas de texto para secciones del Perfil y Solicitud.
 * 
 * Reglas:
 * 1. Separar únicamente por comas (,), guiones/viñetas (- / •), saltos de línea y la conjunción ' y '.
 * 2. NUNCA separar por la conjunción ' o ' (ej. "ERP financiero o sistema contable" permanece como un único elemento).
 * 3. Eliminar guiones, viñetas, caracteres especiales o numeraciones al inicio y final.
 * 4. Capitalizar la primera letra conservando siglas y mayúsculas originales (ej. ERP, SAP, Power BI).
 * 5. Filtrar elementos vacíos, nulos o duplicados.
 */
export function normalizeProfileList(val: any): string[] {
  if (!val) return [];

  let rawItems: string[] = [];
  if (Array.isArray(val)) {
    rawItems = val.map(x => String(x));
  } else {
    rawItems = [String(val)];
  }

  const result: string[] = [];
  const seen = new Set<string>();

  for (const raw of rawItems) {
    if (!raw) continue;

    // Normalizar saltos de línea, viñetas, guiones de separación, comas y la conjunción " y "
    // IMPORTANTE: NO incluir reemplazo por la conjunción " o " / " O "
    let normalized = String(raw)
      .replace(/\r\n/g, '\n')
      .replace(/[•▪▫‣⁃–—*]/g, '\n')
      .replace(/->/g, '\n')
      .replace(/=>/g, '\n')
      .replace(/;/g, '\n')
      .replace(/,/g, '\n')
      .replace(/\s+-\s+/g, '\n') // Guion rodeado de espacios: "item1 - item2"
      .replace(/\s+y\s+/gi, '\n'); // Conector " y "

    const lines = normalized.split('\n');

    for (let line of lines) {
      if (!line) continue;

      // Eliminar guiones, viñetas, caracteres especiales o números/puntos al inicio
      line = line.replace(/^[\s\-\•\▪\▫\‣\⁃\–\—\*\>\d\.\)]+/, '').trim();

      // Eliminar punto final si no es puntos suspensivos
      if (line.endsWith('.') && !line.endsWith('..')) {
        line = line.substring(0, line.length - 1).trim();
      }

      if (!line) continue;

      const lowerItem = line.toLowerCase();
      if (
        lowerItem === 'no especificado' ||
        lowerItem === 'null' ||
        lowerItem === 'undefined' ||
        lowerItem === '—' ||
        lowerItem === '-'
      ) {
        continue;
      }

      // Capitalizar la primera letra conservando siglas y mayúsculas originales
      const formattedLine = line.charAt(0).toUpperCase() + line.slice(1);

      // Evitar elementos duplicados
      if (!seen.has(lowerItem)) {
        seen.add(lowerItem);
        result.push(formattedLine);
      }
    }
  }

  return result;
}
