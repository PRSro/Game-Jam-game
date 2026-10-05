import { createHash } from "node:crypto";

// Source list UNVERIFIED: could not be fetched during development (no outbound
// network access), so selectors, JSON-LD presence and robots.txt policies are
// assumptions. Verify each entry before trusting the feed. LEGAL-REVIEW: only
// public listing pages and schema.org metadata are read, no paywalled or
// member-only content, no competitor markup is copied verbatim.
const SOURCES = [
  {
    id: "tinct",
    name: "TINCT",
    url: "https://tinct.media/events",
  },
  {
    id: "rules-city",
    name: "bucharest.rules.city",
    url: "https://bucharest.rules.city/events",
  },
  {
    id: "bucharest-ro",
    name: "Bucharest.ro (primărie)",
    url: "https://www.bucharest.ro/events",
  },
];

const UA =
  "PiațaBot/0.1 (Bucharest living map; +https://github.com/; contact: UNVERIFIED)";

const TIMEOUT_MS = 8000;
const GAP_MS = 400;
const MAX_BODY_BYTES = 3_000_000;

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

const firstOf = (...vals) =>
  vals.find((v) => typeof v === "string" && v.trim());

const stripHtml = (html) =>
  firstOf(html) === undefined
    ? undefined
    : html
        .replace(/<[^>]*>/g, " ")
        .replace(/&nbsp;/g, " ")
        .replace(/&amp;/g, "&")
        .replace(/&quot;/g, '"')
        .replace(/&#39;/g, "'")
        .replace(/\s+/g, " ")
        .trim();

const typesOf = (node) => {
  const t = node?.["@type"];
  if (!t) return [];
  return (Array.isArray(t) ? t : [t]).map((x) => String(x).toLowerCase());
};

const collectJsonLd = (html) => {
  const out = [];
  const re =
    /<script[^>]+type=["']application\/ld\+json["'][^>]*>([\s\S]*?)<\/script>/gi;
  let match;
  while ((match = re.exec(html)) !== null) {
    const raw = match[1].trim();
    if (!raw) continue;
    try {
      out.push(JSON.parse(raw));
    } catch {
      // Malformed block (common with trailing commas or HTML entities); skip it.
    }
  }
  return out;
};

const walk = (node, sink) => {
  if (Array.isArray(node)) {
    node.forEach((n) => walk(n, sink));
    return;
  }
  if (!node || typeof node !== "object") return;

  if (node["@graph"]) walk(node["@graph"], sink);

  const types = typesOf(node);
  if (types.includes("event")) {
    sink.push(node);
  } else if (types.includes("eventseries") && Array.isArray(node.events)) {
    walk(node.events, sink);
  }
};

const placeOf = (location) => {
  if (!location) return { name: undefined, address: undefined };
  if (typeof location === "string")
    return { name: location, address: undefined };
  const addr =
    typeof location.address === "string" ? location.address : undefined;
  return { name: firstOf(location.name), address: addr };
};

const imageOf = (image) => {
  const raw = Array.isArray(image) ? image[0] : image;
  if (!raw) return undefined;
  if (typeof raw === "string") return raw;
  return firstOf(raw.url, raw.contentUrl);
};

const normalize = (node, source) => {
  const title = firstOf(node.name, node.headline);
  const start = firstOf(node.startDate, node.start);
  if (!title || !start) return null;

  const url = firstOf(node.url, node["@id"]);
  const { name: placeName, address } = placeOf(node.location);

  return {
    id: createHash("sha1")
      .update(`${source.id}|${url ?? ""}|${title}|${start}`)
      .digest("hex")
      .slice(0, 16),
    title,
    start,
    end: firstOf(node.endDate, node.end) ?? null,
    description: stripHtml(firstOf(node.description))?.slice(0, 400) ?? null,
    location: placeName ?? null,
    address: address ?? null,
    image: imageOf(node.image) ?? null,
    url: url ?? null,
    organizer: firstOf(node.organizer?.name, node.organizer) ?? null,
    isFree: node.isAccessibleForFree === true,
    source: { id: source.id, name: source.name },
  };
};

const fetchSource = async (source) => {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), TIMEOUT_MS);
  try {
    const response = await fetch(source.url, {
      signal: controller.signal,
      redirect: "follow",
      headers: {
        "User-Agent": UA,
        Accept: "text/html,application/xhtml+xml",
        "Accept-Language": "ro-RO,ro;q=0.9,en;q=0.8",
      },
    });

    if (!response.ok) {
      return {
        ...source,
        ok: false,
        count: 0,
        error: `HTTP ${response.status}`,
      };
    }

    const html = await response.text();
    if (html.length > MAX_BODY_BYTES) {
      return { ...source, ok: false, count: 0, error: "body too large" };
    }

    const raw = [];
    collectJsonLd(html).forEach((block) => walk(block, raw));

    const docs = raw.map((node) => normalize(node, source)).filter(Boolean);

    return { ...source, ok: true, count: docs.length, events: docs };
  } catch (err) {
    return {
      ...source,
      ok: false,
      count: 0,
      error:
        err?.name === "AbortError" ? "timeout" : String(err?.message ?? err),
    };
  } finally {
    clearTimeout(timer);
  }
};

export default async function handler(req, res) {
  if (req.method !== "GET") {
    res.setHeader("Allow", "GET");
    return res.status(405).json({ error: "Method not allowed" });
  }

  const days = Math.min(
    Math.max(Number.parseInt(String(req.query?.days ?? "60"), 10) || 60, 1),
    120,
  );
  const now = Date.now();
  const horizon = now + days * 86_400_000;
  // Grace window so events that just started still appear.
  const grace = now - 6 * 3_600_000;

  const results = [];
  for (const source of SOURCES) {
    results.push(await fetchSource(source));
    await sleep(GAP_MS);
  }

  const seen = new Set();
  const docs = [];
  let skippedPast = 0;
  let skippedFar = 0;
  let skippedBadDate = 0;

  for (const result of results) {
    for (const doc of result.events ?? []) {
      const start = Date.parse(doc.start);
      if (Number.isNaN(start)) {
        skippedBadDate += 1;
        continue;
      }
      if (start < grace) {
        skippedPast += 1;
        continue;
      }
      if (start > horizon) {
        skippedFar += 1;
        continue;
      }
      const dedupeKey = `${doc.url ?? doc.title}|${doc.start}`;
      if (seen.has(dedupeKey)) continue;
      seen.add(dedupeKey);
      docs.push(doc);
    }
  }

  docs.sort((a, b) => Date.parse(a.start) - Date.parse(b.start));

  const sources = results.map(({ events: _unused, ...rest }) => rest);
  const okCount = sources.filter((s) => s.ok).length;

  res.setHeader(
    "Cache-Control",
    "public, s-maxage=900, stale-while-revalidate=3600",
  );
  return res.status(200).json({
    docs,
    meta: {
      fetchedAt: new Date(now).toISOString(),
      windowDays: days,
      total: docs.length,
      sourcesOk: `${okCount}/${sources.length}`,
      sources,
      skipped: {
        past: skippedPast,
        beyondWindow: skippedFar,
        unparseableDate: skippedBadDate,
      },
    },
  });
}
