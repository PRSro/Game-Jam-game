import { describe, expect, it, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import RadioPlayer from './RadioPlayer.vue'
import { useRadio } from '../composables/useRadio'

describe('RadioPlayer', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('renders station options for Radio ZU, București FM, Europa FM, and Kiss FM', () => {
    const wrapper = mount(RadioPlayer)
    const text = wrapper.text()

    expect(text).toContain('Radio ZU')
    expect(text).toContain('București FM')
    expect(text).toContain('Europa FM')
    expect(text).toContain('Kiss FM')
  })

  it('allows changing current station', async () => {
    const wrapper = mount(RadioPlayer)
    const bucurestiBtn = wrapper.find('#radio-station-bucuresti-fm')

    expect(bucurestiBtn.exists()).toBe(true)
    await bucurestiBtn.trigger('click')

    const { currentStationId } = useRadio()
    expect(currentStationId.value).toBe('bucuresti-fm')
  })

  it('toggles UI sound effects (SFX)', async () => {
    const wrapper = mount(RadioPlayer)
    const sfxBtn = wrapper.find('#sfx-toggle-btn')

    expect(sfxBtn.exists()).toBe(true)
    await sfxBtn.trigger('click')
    expect(sfxBtn.classes()).toContain('border-control-border')
  })

  it('displays error message when station connection times out or fails', async () => {
    const { errorMessage } = useRadio()
    errorMessage.value = 'Stație indisponibilă momentan'

    const wrapper = mount(RadioPlayer)
    const errorBanner = wrapper.find('#radio-error-banner')

    expect(errorBanner.exists()).toBe(true)
    expect(errorBanner.text()).toContain('Stație indisponibilă momentan')
  })
})
