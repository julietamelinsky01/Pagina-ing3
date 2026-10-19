import { expect, test } from '@playwright/test'

// E2E: un navegador de verdad usa la app DESPLEGADA en QA (front + api + base reales).
// Sin mocks de red: si el navegador no le pega a la api, no es end-to-end.

// Las credenciales viven en el entorno (secrets del environment `qa`), nunca en el código.
// Si faltan, el test FALLA con un mensaje claro: no se saltea.
function credenciales () {
  const username = process.env.E2E_USER
  const password = process.env.E2E_PASSWORD
  if (!username || !password) {
    throw new Error('Faltan E2E_USER y/o E2E_PASSWORD: sin credenciales de QA no se puede ingresar.')
  }
  return { username, password }
}

async function ingresar (page) {
  const { username, password } = credenciales()
  await page.goto('/login')
  await page.getByLabel('Usuario').fill(username)
  await page.getByLabel('Contraseña').fill(password)
  await page.getByRole('button', { name: 'Ingresar' }).click()
  await expect(page.getByRole('heading', { name: 'Empleados' })).toBeVisible()
}

// DNI de 8 dígitos que no se repite entre corridas (sale de la hora): el DNI es único en la base.
const dniUnico = () => String(Date.now()).slice(-8)

test('ingresar lleva a la lista de empleados y la sesión sobrevive a recargar', async ({ page }) => {
  await ingresar(page)
  await expect(page.getByText(credenciales().username, { exact: true })).toBeVisible()   // la barra muestra quién ingresó
  await page.reload()
  await expect(page.getByRole('heading', { name: 'Empleados' })).toBeVisible()           // sigue adentro: no lo mandó al login
})

test('crear un empleado lo muestra en la lista, y darlo de baja lo saca de los activos', async ({ page }) => {
  await ingresar(page)
  const nombre = `e2e ${Date.now()}`                              // único: no choca con otra corrida
  await page.getByRole('button', { name: 'Nuevo empleado' }).click()
  await page.getByLabel('Nombre').fill(nombre)
  await page.getByLabel('Apellido').fill('Prueba')
  await page.getByLabel('DNI').fill(dniUnico())
  await page.getByLabel('Fecha de ingreso').fill('2024-01-15')
  await page.getByRole('button', { name: 'Guardar' }).click()

  const fila = page.getByRole('row').filter({ hasText: nombre })
  await expect(fila).toBeVisible()                                // el dato que ESTE test creó, sin sleeps

  page.once('dialog', dialogo => dialogo.accept())                // la baja pide confirmar con un confirm()
  await fila.getByRole('button', { name: 'Dar de baja' }).click()
  await expect(page.getByText(nombre)).toHaveCount(0)             // y la baja, comprobada: ya no está entre los activos
})

test('un DNI inválido muestra el error, no deja guardar y no crea nada', async ({ page }) => {
  await ingresar(page)
  const nombre = `e2e invalido ${Date.now()}`
  await page.getByRole('button', { name: 'Nuevo empleado' }).click()
  await page.getByLabel('Nombre').fill(nombre)
  await page.getByLabel('Apellido').fill('Prueba')
  await page.getByLabel('DNI').fill('12')                         // 2 dígitos: la regla pide 7 u 8
  await page.getByLabel('Fecha de ingreso').fill('2024-01-15')

  await expect(page.getByText('El DNI debe ser numérico, de 7 u 8 dígitos.')).toBeVisible()   // el usuario ve el error…
  await expect(page.getByRole('button', { name: 'Guardar' })).toBeDisabled()                  // …no puede guardar…

  await page.getByRole('button', { name: 'Cancelar' }).click()
  await expect(page.getByText(nombre)).toHaveCount(0)             // …y en la lista no apareció nada
})
