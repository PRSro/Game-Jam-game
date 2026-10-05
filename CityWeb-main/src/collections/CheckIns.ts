import type { CollectionConfig } from 'payload'

export const CheckIns: CollectionConfig = {
  slug: 'check-ins',
  admin: { useAsTitle: 'id' },
  fields: [
    { name: 'person', type: 'relationship', relationTo: 'persons' },
    { name: 'event', type: 'relationship', relationTo: 'events' },
    { name: 'place', type: 'relationship', relationTo: 'places' },
    { name: 'checkedInAt', type: 'date', defaultValue: () => new Date() },
    { name: 'openToChat', type: 'checkbox', defaultValue: false },
    { name: 'optIn', type: 'checkbox', defaultValue: false },
  ],
}
