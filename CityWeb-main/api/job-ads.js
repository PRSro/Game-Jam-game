import pg from 'pg';

const { Pool } = pg;
const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false }
});

export default async function handler(req, res) {
  if (req.method !== 'GET') return res.status(405).json({ message: 'Method not allowed' });
  try {
    const { rows } = await pool.query('SELECT * FROM "job_ads" ORDER BY "createdAt" DESC NULLS LAST');
    res.status(200).json({ docs: rows });
  } catch (_err) {
    try {
      const { rows } = await pool.query('SELECT * FROM "job-ads" ORDER BY "createdAt" DESC NULLS LAST');
      res.status(200).json({ docs: rows });
    } catch (e) {
      console.error(e);
      res.status(500).json({ error: 'Failed to fetch job ads' });
    }
  }
}
