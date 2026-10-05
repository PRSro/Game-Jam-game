import type { CollectionConfig } from 'payload'

export const Places: CollectionConfig = {
  slug: 'places',
  admin: {
    useAsTitle: 'name',
  },
  fields: [
    {
      name: 'name',
      type: 'text',
      required: true,
    },
    {
      name: 'venueName',
      type: 'text',
      label: 'Venue Name',
    },
    {
      name: 'address',
      type: 'textarea',
    },
    {
      name: 'neighborhood',
      type: 'text',
      label: 'Neighborhood',
    },
    {
      name: 'sector',
      type: 'text',
      label: 'Sector',
    },
    {
      name: 'latitude',
      type: 'number',
      admin: {
        step: 0.000001,
      },
    },
    {
      name: 'longitude',
      type: 'number',
      admin: {
        step: 0.000001,
      },
    },
    {
      name: 'tags',
      type: 'array',
      fields: [
        {
          name: 'tag',
          type: 'text',
        },
      ],
    },
  ],
}
