import { expect, test } from '@playwright/test'

// Integración AMPLIA: le habla a la api de QA ya desplegada, con su base de verdad detrás.
// Sin navegador y sin ningún doble en el medio. La api tiene su propia dirección (NO es la del
// front) y va sin barra al final.
const API = process.env.API_BASE_URL || 'http://localhost:8080'

// Las credenciales viven en el entorno (secrets del environment `qa`), nunca en el código.
// Si faltan, el test FALLA con un mensaje claro: no se saltea (un test que se saltea solo no prueba nada).
function credenciales () {
  const username = process.env.E2E_USER
  const password = process.env.E2E_PASSWORD
  if (!username || !password) {
    throw new Error('Faltan E2E_USER y/o E2E_PASSWORD: sin credenciales de QA la api no deja pasar.')
  }
  return { username, password }
}

async function entrar (request) {
  const r = await request.post(`${API}/api/auth/login`, { data: credenciales() })
  expect(r.status(), 'login contra la api de QA').toBe(200)
  const { token } = await r.json()
  return { Authorization: `Bearer ${token}` }
}

// DNI de 8 dígitos que no se repite entre corridas (sale de la hora): el DNI es único en la base.
const dniUnico = () => String(Date.now()).slice(-8)

const nuevoEmpleado = (dni, nombre) => ({
  nombre,
  apellido: 'Prueba',
  dni,
  fechaIngreso: '2024-01-15',
})

async function listar (request, headers, query = '') {
  const r = await request.get(`${API}/api/empleados${query}`, { headers })
  expect(r.status()).toBe(200)
  return await r.json()
}

// La app no borra empleados de verdad: «dar de baja» es el borrado lógico. Es la forma en que
// estos tests limpian lo que crean (el empleado sale de la lista de activos).
async function darDeBaja (request, headers, id) {
  const r = await request.delete(`${API}/api/empleados/${id}`, { headers })
  expect(r.status()).toBe(200)
}

test('el alta guarda en la base de verdad y la baja lo saca de los activos', async ({ request }) => {
  const headers = await entrar(request)
  const nombre = `api ${Date.now()}`                           // único: no choca con otra corrida
  const alta = await request.post(`${API}/api/empleados`, { headers, data: nuevoEmpleado(dniUnico(), nombre) })
  expect(alta.status()).toBe(201)                              // la api dice «creado»
  const creado = await alta.json()

  const activos = await listar(request, headers, '?activo=true')   // y la BASE lo devuelve: no es un doble
  expect(activos.some(e => e.id === creado.id && e.nombre === nombre)).toBe(true)

  await darDeBaja(request, headers, creado.id)

  const despues = await listar(request, headers, '?activo=true')   // y la baja, comprobada
  expect(despues.some(e => e.id === creado.id)).toBe(false)
  const ficha = await request.get(`${API}/api/empleados/${creado.id}`, { headers })
  expect((await ficha.json()).activo).toBe(false)               // sigue en la base, pero inactivo
})

test('un DNI inválido lo rechaza la api, y no crea nada', async ({ request }) => {
  const headers = await entrar(request)
  const antes = await listar(request, headers)

  const alta = await request.post(`${API}/api/empleados`, {
    headers,
    data: nuevoEmpleado('12', `api invalido ${Date.now()}`),    // 2 dígitos: la regla pide 7 u 8
  })
  expect(alta.status()).toBe(400)                              // el contrato del error

  const despues = await listar(request, headers)
  expect(despues.length).toBe(antes.length)                     // y de verdad no se creó nada
})

test('un DNI repetido lo rechaza la base con conflicto, y no crea un segundo empleado', async ({ request }) => {
  const headers = await entrar(request)
  const dni = dniUnico()
  const primero = await request.post(`${API}/api/empleados`, { headers, data: nuevoEmpleado(dni, `api dup ${Date.now()}`) })
  expect(primero.status()).toBe(201)
  const { id } = await primero.json()

  try {
    const antes = await listar(request, headers)
    const repetido = await request.post(`${API}/api/empleados`, { headers, data: nuevoEmpleado(dni, `api otro ${Date.now()}`) })
    expect(repetido.status()).toBe(409)                         // el DNI ya existe: la regla se prueba contra la base real
    const despues = await listar(request, headers)
    expect(despues.length).toBe(antes.length)                   // y no apareció un segundo
  } finally {
    await darDeBaja(request, headers, id)                       // limpia lo que creó, pase lo que pase
  }
})
