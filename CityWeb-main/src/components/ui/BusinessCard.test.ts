import { describe, expect, it, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createMemoryHistory, createRouter } from 'vue-router'
import BusinessCard from './BusinessCard.vue'
import { businesses } from '../../data/demo'
import { setLocale } from '../../i18n'

async function mountCard(businessIndex = 0) {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/evenimente', component: { render: () => null } }]
  })
  const wrapper = mount(BusinessCard, {
    props: { business: businesses[businessIndex] },
    global: { plugins: [router] }
  })
  return wrapper
}

afterEach(() => setLocale('ro'))

describe('BusinessCard', () => {
  it('renders business information correctly', async () => {
    const wrapper = await mountCard(0)
    const text = wrapper.text()

    expect(text).toContain('Cafenea de Specialitate Floreasca')
    expect(text).toContain('Verificat')
    expect(text).toContain('floreasca')
    expect(text).toContain('Cafenea')
  })

  it('renders outgoing graph connections for events, jobs, and polls', async () => {
    const wrapper = await mountCard(0)
    const text = wrapper.text()

    expect(text).toContain('1 evenimente')
    expect(text).toContain('1 joburi')
    expect(text).toContain('1 sondaje')
  })

  it('renders localized content when locale is set to en', async () => {
    setLocale('en')
    const wrapper = await mountCard(0)
    const text = wrapper.text()

    expect(text).toContain('Floreasca Specialty Coffee')
    expect(text).toContain('Coffee Shop')
    expect(text).toContain('View details →')
  })
})
