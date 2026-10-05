import type { CollectionConfig } from 'payload'

export const Connections: CollectionConfig = {
  slug: 'connections',
  admin: {
    useAsTitle: 'type',
  },
  fields: [
    {
      name: 'type',
      type: 'select',
      options: [
        { label: 'Event at Place', value: 'event_place' },
        { label: 'Person attended Event', value: 'person_event' },
        { label: 'Person from Neighborhood', value: 'person_neighborhood' },
        { label: 'Job at Place', value: 'job_place' },
        { label: 'Traffic affects Event', value: 'traffic_event' },
        { label: 'Poll in Place/Community', value: 'poll_place' },
        { label: 'Business at Place', value: 'business_place' },
        { label: 'Other', value: 'other' },
      ],
      required: true,
    },
    {
      name: 'sourceType',
      type: 'select',
      options: [
        { label: 'Event', value: 'events' },
        { label: 'Person', value: 'persons' },
        { label: 'Place', value: 'places' },
        { label: 'JobAd', value: 'job-ads' },
        { label: 'TrafficDisruption', value: 'traffic-disruptions' },
        { label: 'Poll', value: 'polls' },
        { label: 'Business', value: 'businesses' },
        { label: 'Neighborhood', value: 'neighborhoods' },
      ],
      required: true,
    },
    {
      name: 'source',
      type: 'relationship',
      relationTo: ['events', 'places', 'persons', 'businesses', 'job-ads', 'traffic-disruptions', 'polls', 'neighborhoods'],
      required: true,
    },
    {
      name: 'targetType',
      type: 'select',
      options: [
        { label: 'Event', value: 'events' },
        { label: 'Person', value: 'persons' },
        { label: 'Place', value: 'places' },
        { label: 'JobAd', value: 'job-ads' },
        { label: 'TrafficDisruption', value: 'traffic-disruptions' },
        { label: 'Poll', value: 'polls' },
        { label: 'Business', value: 'businesses' },
        { label: 'Neighborhood', value: 'neighborhoods' },
      ],
      required: true,
    },
    {
      name: 'target',
      type: 'relationship',
      relationTo: ['events', 'places', 'persons', 'businesses', 'job-ads', 'traffic-disruptions', 'polls', 'neighborhoods'],
      required: true,
    },
    {
      name: 'weight',
      type: 'number',
      defaultValue: 1,
    },
    {
      name: 'metadata',
      type: 'json',
    },
  ],
}
