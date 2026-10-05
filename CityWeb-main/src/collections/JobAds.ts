import type { CollectionConfig } from 'payload'

export const JobAds: CollectionConfig = {
  slug: 'job-ads',
  admin: { useAsTitle: 'title' },
  fields: [
    { name: 'title', type: 'text', required: true },
    { name: 'company', type: 'text' },
    { name: 'business', type: 'relationship', relationTo: 'businesses' },
    { name: 'place', type: 'relationship', relationTo: 'places' },
    { name: 'category', type: 'text' },
    { name: 'jobType', type: 'select', options: [
      { label: 'Full-time', value: 'full-time' },
      { label: 'Part-time', value: 'part-time' },
      { label: 'Internship', value: 'internship' },
      { label: 'Freelance', value: 'freelance' },
    ]},
    { name: 'location', type: 'text' },
    { name: 'applyLink', type: 'text' },
    { name: 'expiresAt', type: 'date' },
  ],
}
