import type { CollectionConfig } from 'payload'

export const Neighborhoods: CollectionConfig = {
  slug: 'neighborhoods',
  admin: { useAsTitle: 'name' },
  fields: [
    { name: 'name', type: 'text', required: true },
    { name: 'sector', type: 'text' },
    { name: 'description', type: 'textarea' },
    { name: 'stats', type: 'json' },
  ],
}
