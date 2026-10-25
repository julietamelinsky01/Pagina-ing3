// Etiqueta legible de un turno según su hora de inicio.
// (Sin tests a propósito: es el segundo Pull Request del TP5, el que queda frenado y abierto.)
export function etiquetaDeTurno(horaInicio) {
  if (!horaInicio) {
    return 'Sin horario'
  }
  const hora = Number(horaInicio.split(':')[0])
  if (Number.isNaN(hora) || hora < 0 || hora > 23) {
    return 'Horario inválido'
  }
  if (hora < 6) {
    return 'Madrugada'
  }
  if (hora < 12) {
    return 'Mañana'
  }
  if (hora < 20) {
    return 'Tarde'
  }
  return 'Noche'
}
