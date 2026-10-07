// Reglas de negocio del frontend, como funciones PURAS (reciben valores y devuelven
// valores, sin tocar la red ni el DOM). Por eso se testean sin mocks de UI.

export const MENSAJE_DNI = "El DNI debe ser numérico, de 7 u 8 dígitos.";

export function dniValido(dni) {
  return typeof dni === "string" && /^\d{7,8}$/.test(dni);
}

const estaVacio = (texto) => !texto || texto.trim() === "";

// Devuelve { valido, error }: la primera regla que falla explica por qué.
export function validarEmpleado(form) {
  if (estaVacio(form.nombre)) return { valido: false, error: "El nombre es obligatorio." };
  if (estaVacio(form.apellido)) return { valido: false, error: "El apellido es obligatorio." };
  if (!dniValido(form.dni)) return { valido: false, error: MENSAJE_DNI };
  if (estaVacio(form.fechaIngreso)) return { valido: false, error: "La fecha de ingreso es obligatoria." };
  return { valido: true, error: "" };
}

// Chequeo del lado del cliente antes de pegarle a la API: ¿ya existe esa combinación
// empleado + tipo de turno + fecha entre las asignaciones cargadas?
export function existeAsignacion(asignaciones, empleadoId, tipoTurnoId, fecha) {
  if (!empleadoId || !tipoTurnoId) return false;
  return asignaciones.some(
    (a) =>
      String(a.empleadoId) === String(empleadoId) &&
      String(a.tipoTurnoId) === String(tipoTurnoId) &&
      a.fecha === fecha
  );
}

// Total de turnos y horas por empleado, ordenado por nombre.
export function totalHorasPorEmpleado(asignaciones) {
  const porEmpleado = new Map();
  for (const a of asignaciones) {
    const actual = porEmpleado.get(a.empleadoId) || {
      empleadoId: a.empleadoId,
      empleado: a.empleadoNombreCompleto,
      turnos: 0,
      horas: 0,
    };
    actual.turnos += 1;
    actual.horas += a.horasCalculadas;
    porEmpleado.set(a.empleadoId, actual);
  }
  return Array.from(porEmpleado.values()).sort((a, b) => a.empleado.localeCompare(b.empleado));
}

// La dependencia que habla con la API (`obtenerAsignaciones`) entra por parámetro:
// en la app es getAsignaciones; en los tests, un doble.
export async function cargarFilasReporte(desde, hasta, obtenerAsignaciones) {
  if (hasta < desde) {
    throw new Error("La fecha 'hasta' no puede ser anterior a la fecha 'desde'.");
  }
  const asignaciones = await obtenerAsignaciones(desde, hasta);
  return totalHorasPorEmpleado(asignaciones);
}
