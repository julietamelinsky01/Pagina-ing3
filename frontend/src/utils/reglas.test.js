import { describe, expect, it, vi } from 'vitest'
import {
  MENSAJE_DNI,
  cargarFilasReporte,
  dniValido,
  existeAsignacion,
  totalHorasPorEmpleado,
  validarEmpleado,
} from './reglas'

describe('dniValido', () => {
  it.each([
    ['7 dígitos', '1234567'],
    ['8 dígitos', '12345678'],
  ])('acepta un DNI de %s', (_caso, dni) => {
    expect(dniValido(dni)).toBe(true)
  })

  // Parametrizado: un comportamiento ("se rechaza") con varios datos inválidos y bordes
  it.each([
    ['6 dígitos (uno menos que el mínimo)', '123456'],
    ['9 dígitos (uno más que el máximo)', '123456789'],
    ['con letras', '1234abcd'],
    ['con puntos', '12.345.678'],
    ['vacío', ''],
    ['nulo', null],
  ])('rechaza un DNI %s', (_caso, dni) => {
    expect(dniValido(dni)).toBe(false)
  })
})

describe('validarEmpleado', () => {
  const completo = { nombre: 'Ana', apellido: 'Pérez', dni: '12345678', fechaIngreso: '2024-01-15' }

  it('acepta un empleado con todos los campos', () => {
    expect(validarEmpleado(completo)).toEqual({ valido: true, error: '' })
  })

  // Caso de error: cada campo faltante se rechaza Y el mensaje dice cuál falta
  it.each([
    ['nombre', { nombre: '   ' }, 'El nombre es obligatorio.'],
    ['apellido', { apellido: '' }, 'El apellido es obligatorio.'],
    ['DNI', { dni: '123' }, MENSAJE_DNI],
    ['fecha de ingreso', { fechaIngreso: '' }, 'La fecha de ingreso es obligatoria.'],
  ])('rechaza si falta el %s y lo explica', (_campo, cambio, mensaje) => {
    const resultado = validarEmpleado({ ...completo, ...cambio })

    expect(resultado.valido).toBe(false)
    expect(resultado.error).toBe(mensaje)
  })
})

describe('existeAsignacion', () => {
  const asignaciones = [{ empleadoId: 1, tipoTurnoId: 2, fecha: '2026-09-14' }]

  it('detecta la misma combinación empleado + turno + fecha', () => {
    expect(existeAsignacion(asignaciones, 1, 2, '2026-09-14')).toBe(true)
  })

  it('compara los ids como texto: el <select> los entrega como string', () => {
    expect(existeAsignacion(asignaciones, '1', '2', '2026-09-14')).toBe(true)
  })

  it.each([
    ['otro día', 1, 2, '2026-09-15'],
    ['otro turno', 1, 3, '2026-09-14'],
    ['otro empleado', 9, 2, '2026-09-14'],
    ['todavía sin elegir empleado', '', 2, '2026-09-14'],
    ['todavía sin elegir turno', 1, '', '2026-09-14'],
  ])('no hay duplicado si es %s', (_caso, empleadoId, tipoTurnoId, fecha) => {
    expect(existeAsignacion(asignaciones, empleadoId, tipoTurnoId, fecha)).toBe(false)
  })
})

describe('totalHorasPorEmpleado', () => {
  it('suma turnos y horas por empleado y ordena por nombre', () => {
    const asignaciones = [
      { empleadoId: 2, empleadoNombreCompleto: 'Zoe Díaz', horasCalculadas: 8 },
      { empleadoId: 1, empleadoNombreCompleto: 'Ana Pérez', horasCalculadas: 8 },
      { empleadoId: 1, empleadoNombreCompleto: 'Ana Pérez', horasCalculadas: 6.5 },
    ]

    const filas = totalHorasPorEmpleado(asignaciones)

    expect(filas.map((f) => f.empleado)).toEqual(['Ana Pérez', 'Zoe Díaz'])
    expect(filas[0]).toMatchObject({ turnos: 2, horas: 14.5 })
    expect(filas[1]).toMatchObject({ turnos: 1, horas: 8 })
  })

  it('devuelve una lista vacía si no hay asignaciones', () => {
    expect(totalHorasPorEmpleado([])).toEqual([])
  })
})

describe('cargarFilasReporte', () => {
  // Acá vi.fn() hace de STUB (contesta con datos fijos) y de MOCK (el test verifica cómo se lo usó)
  it('le pide a la API el rango exacto, una sola vez, y devuelve las filas armadas', async () => {
    const obtenerAsignaciones = vi
      .fn()
      .mockResolvedValue([{ empleadoId: 1, empleadoNombreCompleto: 'Ana Pérez', horasCalculadas: 8 }])

    const filas = await cargarFilasReporte('2026-09-14', '2026-09-20', obtenerAsignaciones)

    expect(obtenerAsignaciones).toHaveBeenCalledTimes(1)
    expect(obtenerAsignaciones).toHaveBeenCalledWith('2026-09-14', '2026-09-20')
    expect(filas).toEqual([{ empleadoId: 1, empleado: 'Ana Pérez', turnos: 1, horas: 8 }])
  })

  it('rechaza un rango invertido sin siquiera llamar a la API', async () => {
    const obtenerAsignaciones = vi.fn()

    await expect(cargarFilasReporte('2026-09-20', '2026-09-14', obtenerAsignaciones)).rejects.toThrow(
      "La fecha 'hasta' no puede ser anterior a la fecha 'desde'."
    )
    expect(obtenerAsignaciones).not.toHaveBeenCalled()
  })

  it('acepta un rango de un solo día (borde: desde == hasta)', async () => {
    const obtenerAsignaciones = vi.fn().mockResolvedValue([])

    await expect(cargarFilasReporte('2026-09-14', '2026-09-14', obtenerAsignaciones)).resolves.toEqual([])
    expect(obtenerAsignaciones).toHaveBeenCalledTimes(1)
  })

  it('propaga el error de la API en vez de tragárselo', async () => {
    const obtenerAsignaciones = vi.fn().mockRejectedValue(new Error('Sin conexión'))

    await expect(cargarFilasReporte('2026-09-14', '2026-09-20', obtenerAsignaciones)).rejects.toThrow(
      'Sin conexión'
    )
  })
})
