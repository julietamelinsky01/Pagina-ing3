// Clasifica la carga semanal de un empleado a partir de sus horas totales.
// (Tiene varios caminos adentro y —a propósito, para la demostración del TP5— ni un solo test.)
export function nivelDeCarga(horasSemanales) {
  if (horasSemanales == null || Number.isNaN(horasSemanales)) {
    return 'sin-datos'
  }
  if (horasSemanales < 0) {
    return 'invalido'
  }
  if (horasSemanales === 0) {
    return 'sin-turnos'
  }
  if (horasSemanales < 20) {
    return 'parcial'
  }
  if (horasSemanales < 40) {
    return 'normal'
  }
  if (horasSemanales <= 48) {
    return 'completa'
  }
  return 'exceso'
}

export function avisoDeCarga(horasSemanales) {
  const nivel = nivelDeCarga(horasSemanales)
  if (nivel === 'exceso') {
    return `Supera el máximo legal: ${horasSemanales} hs en la semana.`
  }
  if (nivel === 'completa') {
    return 'Jornada completa: no sumar más turnos.'
  }
  if (nivel === 'sin-turnos') {
    return 'Todavía no tiene turnos esta semana.'
  }
  return ''
}
