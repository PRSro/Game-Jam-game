import type { CollectionConfig } from 'payload'

export const Events: CollectionConfig = {
  slug: 'events',
  admin: {
    useAsTitle: 'title',
  },
  fields: [
    {
      name: 'title',
      type: 'text',
      required: true,
    },
    {
      name: 'description',
      type: 'richText',
    },
    {
      name: 'place',
      type: 'relationship',
      relationTo: 'places',
      required: true,
    },
    {
      name: 'startDate',
      type: 'date',
      required: true,
      admin: {
        date: {
          pickerAppearance: 'dayAndTime',
        },
      },
    },
    {
      name: 'endDate',
      type: 'date',
      admin: {
        date: {
          pickerAppearance: 'dayAndTime',
        },
      },
    },
    {
      name: 'isFree',
      type: 'checkbox',
      defaultValue: true,
    },
    {
      name: 'category',
      type: 'text',
    },
    {
      name: 'neighborhoodTags',
      type: 'array',
      fields: [
        {
          name: 'tag',
          type: 'text',
        },
      ],
    },
    {
      name: 'imageUrl',
      type: 'text',
      label: 'Image URL (hotlink from source if permitted)',
    },
    {
      name: 'sourceUrl',
      type: 'text',
      label: 'Source URL (attribution)',
    },
    {
      name: 'source',
      type: 'select',
      options: [
        { label: 'Manual', value: 'manual' },
        { label: 'B365', value: 'b365' },
        { label: 'Meetup', value: 'meetup' },
        { label: 'Eventbrite', value: 'eventbrite' },
        { label: 'iaBilet', value: 'iabilet' },
        { label: 'Other', value: 'other' },
      ],
      defaultValue: 'manual',
    },
    {
      name: 'unverified',
      type: 'checkbox',
      defaultValue: false,
    },
  ],
}
