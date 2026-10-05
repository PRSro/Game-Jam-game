import type { CollectionConfig } from 'payload'

export const TrafficDisruptions: CollectionConfig = {
  slug: 'traffic-disruptions',
  admin: { useAsTitle: 'reason' },
  fields: [
    { name: 'reason', type: 'text', required: true },
    { name: 'routeType', type: 'select', options: [
      { label: 'Metro', value: 'metro' },
      { label: 'Bus', value: 'bus' },
      { label: 'Tram', value: 'tram' },
      { label: 'Trolley', value: 'trolley' },
      { label: 'Street', value: 'street' },
    ]},
    { name: 'route', type: 'text' },
    { name: 'street', type: 'text' },
    { name: 'sector', type: 'text' },
    { name: 'startDate', type: 'date' },
    { name: 'endDate', type: 'date' },
    { name: 'source', type: 'text' },
    { name: 'sourceUrl', type: 'text' },
    { name: 'unverified', type: 'checkbox', defaultValue: true },
  ],
}
