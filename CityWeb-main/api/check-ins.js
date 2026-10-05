import pg from 'pg';

const { Pool } = pg;
const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false }
});

export default async function handler(req, res) {
  if (req.method !== 'GET') return res.status(405).json({ message: 'Method not allowed' });
  try {
    const { rows } = await pool.query('SELECT * FROM "check-ins" ORDER BY "createdAt" DESC');
    res.status(200).json({ docs: rows });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Failed to fetch check-ins' });
  }
}
