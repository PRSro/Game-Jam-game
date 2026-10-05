import { describe, expect, it, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createMemoryHistory, createRouter } from 'vue-router'
import BusinessDetailPage from './BusinessDetailPage.vue'
import { setLocale } from '../i18n'

async function mountPage(businessId = 'biz-1') {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/afaceri', component: { render: () => null } },
      { path: '/afaceri/:id', component: BusinessDetailPage }
    ]
  })
  await router.push(`/afaceri/${businessId}`)
  await router.isReady()

  const wrapper = mount(BusinessDetailPage, {
    global: { plugins: [router] }
  })
  return wrapper
}

afterEach(() => setLocale('ro'))

describe('BusinessDetailPage', () => {
  it('renders business header, category, and address for valid business', async () => {
    const wrapper = await mountPage('biz-1')
    const text = wrapper.text()

    expect(text).toContain('Cafenea de Specialitate Floreasca')
    expect(text).toContain('Afacere Verificată')
    expect(text).toContain('floreasca')
    expect(text).toContain('Calea Floreasca nr. 42, București')
  })

  it('renders Facebook profile link button', async () => {
    const wrapper = await mountPage('biz-1')
    const fbBtn = wrapper.find('#business-facebook-btn')

    expect(fbBtn.exists()).toBe(true)
    expect(fbBtn.attributes('href')).toBe('https://facebook.com/floreascacoffee.demo')
  })

  it('renders graph connections for events, jobs, and polls', async () => {
    const wrapper = await mountPage('biz-1')
    const text = wrapper.text()

    expect(text).toContain('Evenimente Găzduite')
    expect(text).toContain('Locuri de Muncă Disponibile')
  })

  it('renders fallback error state for non-existent business ID', async () => {
    const wrapper = await mountPage('biz-nonexistent')
    const notFound = wrapper.find('#business-not-found')

    expect(notFound.exists()).toBe(true)
    expect(notFound.text()).toContain('Afacerea nu a fost găsită')
  })
})
