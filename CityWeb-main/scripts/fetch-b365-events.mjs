#!/usr/bin/env node
import fs from 'fs/promises'
import path from 'path'
import { fileURLToPath } from 'url'

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)
const outDir = path.join(__dirname, '..', 'seed', 'b365-scraped')
const outFile = path.join(outDir, `b365-events-${Date.now()}.json`)

const USER_AGENT = 'Piața-Research/0.1 (+https://github.com/PRSro/CityWeb)'
const BASE = 'https://b365.ro'

async function checkRobots() {
  try {
    const res = await fetch(new URL('/robots.txt', BASE), {
      headers: { 'User-Agent': USER_AGENT },
      signal: AbortSignal.timeout(5000)
    })
    if (res.ok) {
      const txt = await res.text()
      return { ok: true, disallowsAll: txt.includes('Disallow: /'), text: txt }
    }
  } catch (e) {
    return { ok: false, error: e.message }
  }
  return { ok: false }
}

async function _sleep(ms) {
  return new Promise(r => setTimeout(r, ms))
}

async function main() {
  await fs.mkdir(outDir, { recursive: true })
  const robots = await checkRobots()
  
  // WARNING: b365.ro/robots.txt disallows /. Only enable with explicit approval and legal review.
  // This is a minimal, rate-limited, field-limited fetcher.
  const ENABLE_FETCH = false // keep disabled by default
  
  const result = {
    source: 'b365.ro',
    fetchedAt: new Date().toISOString(),
    status: 'skipped',
    reason: 'Safe mode: ENABLE_FETCH=false. Set to true only after LEGAL-REVIEW and approval.',
    robots,
    items: [],
    notes: [
      'UNVERIFIED - not imported to DB',
      'Do not copy full descriptions/images',
      'Store only title/time/place/url',
      'Respect robots.txt and rate limits (>=1-2s delay)',
      'Check LEGAL-REVIEW before enabling actual scraping',
      'Cache responses; never store raw full pages'
    ]
  }
  
  if (ENABLE_FETCH) {
    // Placeholder - implement minimal parsing only if approved
    result.status = 'conceptual-enabled'
    result.reason = 'Fetch enabled conceptually; implement parsing carefully'
  }
  
  await fs.writeFile(outFile, JSON.stringify(result, null, 2))
  console.log('Wrote:', outFile)
}

main().catch(err => {
  console.error(err)
  process.exit(1)
})
