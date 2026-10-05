import type { CollectionConfig } from 'payload'

export const Polls: CollectionConfig = {
  slug: 'polls',
  admin: { useAsTitle: 'question' },
  fields: [
    { name: 'question', type: 'text', required: true },
    { name: 'description', type: 'textarea' },
    { name: 'place', type: 'relationship', relationTo: 'places' },
    { name: 'communityType', type: 'select', options: [
      { label: 'Neighborhood', value: 'neighborhood' },
      { label: 'Sector', value: 'sector' },
      { label: 'City', value: 'city' },
    ]},
    { name: 'communityName', type: 'text' },
    { name: 'startsAt', type: 'date' },
    { name: 'endsAt', type: 'date' },
    { name: 'options', type: 'array', fields: [
      { name: 'option', type: 'text', required: true },
    ]},
  ],
}
