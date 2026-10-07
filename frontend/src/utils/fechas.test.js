import { describe, expect, it } from 'vitest'
import { NOMBRES_DIA, aISO, diasDeLaSemana, lunesDeLaSemana, sumarDias } from './fechas'

describe('aISO', () => {
  it('completa con ceros el mes y el día', () => {
    expect(aISO(new Date(2026, 0, 5))).toBe('2026-01-05')
  })
})

describe('lunesDeLaSemana', () => {
  // 14/9/2026 es lunes. El borde que importa: el domingo pertenece a la semana que EMPEZÓ el lunes anterior.
  it.each([
    ['un lunes es su propio lunes', '2026-09-14', '2026-09-14'],
    ['un miércoles', '2026-09-16', '2026-09-14'],
    ['un sábado', '2026-09-19', '2026-09-14'],
    ['un domingo (borde)', '2026-09-20', '2026-09-14'],
    ['el lunes siguiente (borde)', '2026-09-21', '2026-09-21'],
  ])('%s', (_caso, entrada, esperado) => {
    expect(aISO(lunesDeLaSemana(entrada))).toBe(esperado)
  })
})

describe('sumarDias', () => {
  it.each([
    ['dentro del mismo mes', '2026-09-14', 6, '2026-09-20'],
    ['cruzando de mes', '2026-09-28', 5, '2026-10-03'],
    ['cruzando de año', '2026-12-30', 3, '2027-01-02'],
    ['restando días', '2026-09-01', -1, '2026-08-31'],
  ])('suma %s', (_caso, fecha, dias, esperado) => {
    expect(sumarDias(fecha, dias)).toBe(esperado)
  })
})

describe('diasDeLaSemana', () => {
  it('devuelve siete días consecutivos a partir del lunes', () => {
    const dias = diasDeLaSemana(lunesDeLaSemana('2026-09-16')).map(aISO)

    expect(dias).toHaveLength(7)
    expect(dias[0]).toBe('2026-09-14')
    expect(dias[6]).toBe('2026-09-20')
    expect(NOMBRES_DIA).toHaveLength(7)
  })
})
