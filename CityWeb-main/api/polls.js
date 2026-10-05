import pg from 'pg';

const { Pool } = pg;
const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false }
});

export default async function handler(req, res) {
  if (req.method !== 'GET') {
    return res.status(405).json({ message: 'Method not allowed' });
  }

  try {
    const { rows } = await pool.query('SELECT * FROM polls ORDER BY "createdAt" DESC NULLS LAST');
    res.status(200).json({ docs: rows });
  } catch (err) {
    console.error('Error fetching polls:', err);
    res.status(500).json({ error: 'Failed to fetch polls from database' });
  }
}
