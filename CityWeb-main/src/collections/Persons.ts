import type { CollectionConfig } from 'payload'

export const Persons: CollectionConfig = {
  slug: 'persons',
  admin: {
    useAsTitle: 'displayName',
  },
  fields: [
    {
      name: 'displayName',
      type: 'text',
      required: true,
    },
    {
      name: 'neighborhood',
      type: 'text',
    },
    {
      name: 'commuteType',
      type: 'select',
      options: [
        { label: 'Metro', value: 'metro' },
        { label: 'Bus', value: 'bus' },
        { label: 'Tram', value: 'tram' },
        { label: 'Trolley', value: 'trolley' },
        { label: 'Other', value: 'other' },
      ],
    },
    {
      name: 'commuteRoute',
      type: 'text',
    },
    {
      name: 'interests',
      type: 'array',
      fields: [
        { name: 'interest', type: 'text' },
      ],
    },
    {
      name: 'intent',
      type: 'select',
      options: [
        { label: 'Newcomer', value: 'newcomer' },
        { label: 'Jobseeker', value: 'jobseeker' },
        { label: 'Hiring', value: 'hiring' },
        { label: 'Meeting', value: 'meeting' },
        { label: 'Curious', value: 'curious' },
      ],
    },
  ],
}
