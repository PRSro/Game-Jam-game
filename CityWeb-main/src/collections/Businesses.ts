import type { CollectionConfig } from 'payload'

export const Businesses: CollectionConfig = {
  slug: 'businesses',
  admin: { useAsTitle: 'name' },
  fields: [
    { name: 'name', type: 'text', required: true },
    { name: 'category', type: 'text' },
    { name: 'place', type: 'relationship', relationTo: 'places' },
    { name: 'description', type: 'textarea' },
    { name: 'tags', type: 'array', fields: [{ name: 'tag', type: 'text' }] },
    { name: 'verified', type: 'checkbox', defaultValue: false },
  ],
}
