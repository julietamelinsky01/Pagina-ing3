import { describe, expect, it } from 'vitest'
import { avisoDeCarga, nivelDeCarga } from './carga'

// Un test por cada camino que declara nivelDeCarga, con sus bordes (20, 40 y 48 hs).
describe('nivelDeCarga', () => {
  it.each([
    ['nulo', null],
    ['indefinido', undefined],
    ['NaN', NaN],
  ])('devuelve sin-datos si las horas son %s', (_caso, horas) => {
    expect(nivelDeCarga(horas)).toBe('sin-datos')
  })

  it('rechaza horas negativas como inválidas', () => {
    expect(nivelDeCarga(-1)).toBe('invalido')
  })

  it('cero horas es sin-turnos', () => {
    expect(nivelDeCarga(0)).toBe('sin-turnos')
  })

  it.each([
    ['justo debajo de 20 (borde)', 19.99, 'parcial'],
    ['20 exactas ya es normal (borde)', 20, 'normal'],
    ['justo debajo de 40 (borde)', 39.99, 'normal'],
    ['40 exactas ya es completa (borde)', 40, 'completa'],
    ['48 exactas sigue siendo completa (borde)', 48, 'completa'],
    ['48,01 ya es exceso (borde)', 48.01, 'exceso'],
  ])('%s', (_caso, horas, esperado) => {
    expect(nivelDeCarga(horas)).toBe(esperado)
  })
})

describe('avisoDeCarga', () => {
  it('avisa el exceso con las horas exactas', () => {
    expect(avisoDeCarga(52)).toBe('Supera el máximo legal: 52 hs en la semana.')
  })

  it('avisa que la jornada está completa', () => {
    expect(avisoDeCarga(40)).toBe('Jornada completa: no sumar más turnos.')
  })

  it('avisa que todavía no hay turnos', () => {
    expect(avisoDeCarga(0)).toBe('Todavía no tiene turnos esta semana.')
  })

  it.each([
    ['parcial', 10],
    ['normal', 30],
    ['sin datos', null],
  ])('no avisa nada si la carga es %s', (_caso, horas) => {
    expect(avisoDeCarga(horas)).toBe('')
  })
})
