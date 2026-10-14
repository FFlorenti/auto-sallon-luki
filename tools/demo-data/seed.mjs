import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const here = path.dirname(fileURLToPath(import.meta.url))
const cache = path.join(here, '.cache')
const args = process.argv.slice(2)
const arg = (name, fallback) => (args.includes(name) ? args[args.indexOf(name) + 1] : fallback)

const baseUrl = arg('--url', `http://localhost:${process.env.WEB_PORT ?? 8080}`).replace(/\/$/, '')
const email = arg('--email', process.env.ADMIN_EMAIL)
const password = arg('--password', process.env.ADMIN_PASSWORD)
const force = args.includes('--force')

if (!email || !password) {
  console.error('Admin login missing. Run with:  node --env-file=.env tools/demo-data/seed.mjs')
  process.exit(1)
}

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms))

async function api(method, url, body, token) {
  const isForm = body instanceof FormData
  const response = await fetch(baseUrl + url, {
    method,
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(body && !isForm ? { 'Content-Type': 'application/json' } : {}),
    },
    body: isForm ? body : body ? JSON.stringify(body) : undefined,
  })
  if (!response.ok) {
    throw new Error(`${method} ${url} -> ${response.status} ${await response.text()}`)
  }
  const text = await response.text()
  return text ? JSON.parse(text) : null
}

async function getPhoto(photo) {
  const file = path.join(cache, photo.file)
  if (fs.existsSync(file) && fs.statSync(file).size > 10_000) {
    return fs.readFileSync(file)
  }

  fs.mkdirSync(cache, { recursive: true })
  for (let attempt = 1; attempt <= 5; attempt++) {
    await sleep(1200)
    const response = await fetch(photo.url, { headers: { 'User-Agent': 'AutoLukiDemoSeeder/1.0 (demo data for local development)' } })
    if (response.ok) {
      const bytes = Buffer.from(await response.arrayBuffer())
      fs.writeFileSync(file, bytes)
      return bytes
    }
    await sleep(4000 * attempt)
  }
  throw new Error(`Could not download ${photo.url}`)
}

const cars = JSON.parse(fs.readFileSync(path.join(here, 'cars.json'), 'utf8'))

const { accessToken: token } = await api('POST', '/api/auth/login', { email, password })

const existing = await api('GET', '/api/admin/cars?pageSize=1', null, token)
if (existing.totalCount > 0 && !force) {
  console.error(`The site already has ${existing.totalCount} cars. Nothing was added. Use --force to add the demo cars anyway.`)
  process.exit(1)
}

const brands = await api('GET', '/api/brands')
async function modelId(brandName, modelName) {
  let brand = brands.find((b) => b.name === brandName)
  if (!brand) {
    brand = await api('POST', '/api/admin/brands', { name: brandName }, token)
    brands.push(brand)
  }
  let model = brand.models.find((m) => m.name === modelName)
  if (!model) {
    model = await api('POST', `/api/admin/brands/${brand.id}/models`, { name: modelName }, token)
    brand.models.push(model)
  }
  return model.id
}

for (const car of cars) {
  const { id } = await api('POST', '/api/admin/cars', {
    modelId: await modelId(car.brand, car.model),
    year: car.year,
    price: car.price,
    mileageKm: car.mileageKm,
    fuelType: car.fuelType,
    transmission: car.transmission,
    bodyType: car.bodyType,
    engineSizeCc: car.engineSizeCc,
    powerHp: car.powerHp,
    color: car.color,
    doors: car.doors,
    vin: null,
    isCustomsCleared: car.isCustomsCleared,
    descriptionSq: car.descriptionSq,
    descriptionEn: car.descriptionEn,
  }, token)

  const form = new FormData()
  for (const photo of car.photos) {
    form.append('files', new Blob([await getPhoto(photo)], { type: 'image/jpeg' }), photo.file)
  }
  await api('POST', `/api/admin/cars/${id}/photos`, form, token)

  if (car.status !== 'Available') {
    await api('PATCH', `/api/admin/cars/${id}/status`, { status: car.status }, token)
  }

  console.log(`#${id} ${car.brand} ${car.model} ${car.year} - ${car.photos.length} photos${car.status !== 'Available' ? ` (${car.status})` : ''}`)
}

console.log(`\nDone: ${cars.length} demo cars added to ${baseUrl}`)
