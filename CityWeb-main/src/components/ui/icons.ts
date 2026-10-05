import { defineComponent, h } from 'vue'

/**
 * Content-type and utility icons: 24px line icons, 1.5px stroke, currentColor.
 * No emoji, no filled shapes, no colour baked in - the icon inherits the text
 * colour of its container so it can sit on any token surface.
 */
function createIcon(name: string, paths: string[]) {
  return defineComponent({
    name,
    inheritAttrs: false,
    props: {
      size: { type: Number, default: 24 },
      /** When omitted the icon is decorative and hidden from assistive tech. */
      title: { type: String, default: undefined },
    },
    setup(props, { attrs }) {
      return () =>
        h(
          'svg',
          {
            ...attrs,
            width: props.size,
            height: props.size,
            viewBox: '0 0 24 24',
            fill: 'none',
            stroke: 'currentColor',
            'stroke-width': 1.5,
            'stroke-linecap': 'round',
            'stroke-linejoin': 'round',
            role: props.title ? 'img' : undefined,
            'aria-hidden': props.title ? undefined : 'true',
            focusable: 'false',
          },
          [
            props.title ? h('title', props.title) : null,
            ...paths.map((d) => h('path', { d })),
          ],
        )
    },
  })
}

/* Content types - the five Metrorex line categories. */

export const IconEvenimente = createIcon('IconEvenimente', [
  'M7 2v2M17 2v2M3.5 9h17',
  'M5 4h14a2 2 0 012 2v14a2 2 0 01-2 2H5a2 2 0 01-2-2V6a2 2 0 012-2z',
])

export const IconJoburi = createIcon('IconJoburi', [
  'M4 7h16a1 1 0 011 1v11a1 1 0 01-1 1H4a1 1 0 01-1-1V8a1 1 0 011-1z',
  'M9 7V5a1 1 0 011-1h4a1 1 0 011 1v2',
  'M3 12h18',
])

export const IconTrafic = createIcon('IconTrafic', [
  'M4 6h16v12H4z',
  'M8 10v4M16 10v4',
])

export const IconOameni = createIcon('IconOameni', [
  'M16 20v-1.5a4 4 0 00-4-4H6a4 4 0 00-4 4V20',
  'M9 10.5a3.5 3.5 0 100-7 3.5 3.5 0 000 7z',
  'M22 20v-1.5a4 4 0 00-3-3.87',
  'M16 3.6a4 4 0 010 7.75',
])

export const IconSondaje = createIcon('IconSondaje', [
  'M4 20V10M10 20V4M16 20v-7M22 20H2',
])

/* Utility icons. */

export const IconCalendar = createIcon('IconCalendar', [
  'M7 2v2M17 2v2M3.5 9h17',
  'M5 4h14a2 2 0 012 2v14a2 2 0 01-2 2H5a2 2 0 01-2-2V6a2 2 0 012-2z',
])

export const IconSearch = createIcon('IconSearch', [
  'M10.5 3a7.5 7.5 0 105.3 12.8l4.4 4.4 1.4-1.4-4.4-4.4A7.5 7.5 0 0010.5 3zm0 2a5.5 5.5 0 110 11 5.5 5.5 0 010-11z',
])

export const IconPin = createIcon('IconPin', [
  'M12 21s7-5.6 7-11a7 7 0 10-14 0c0 5.4 7 11 7 11z',
  'M12 12.5a2.5 2.5 0 100-5 2.5 2.5 0 000 5z',
])

export const IconClose = createIcon('IconClose', ['M6 6l12 12M18 6L6 18'])

export const IconCheck = createIcon('IconCheck', ['M4.5 12.5l5 5 10-11'])

export const IconChevronDown = createIcon('IconChevronDown', ['M6 9.5l6 6 6-6'])

export const IconAlert = createIcon('IconAlert', [
  'M12 3.5L21.5 20h-19z',
  'M12 9.5v5M12 17.5h.01',
])

export const IconInfo = createIcon('IconInfo', [
  'M12 21a9 9 0 100-18 9 9 0 000 18z',
  'M12 11v5M12 8h.01',
])

export const IconSun = createIcon('IconSun', [
  'M12 16a4 4 0 100-8 4 4 0 000 8z',
  'M12 2v2M12 20v2M2 12h2M20 12h2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M19.1 4.9l-1.4 1.4M6.3 17.7l-1.4 1.4',
])

export const IconMoon = createIcon('IconMoon', ['M21 12.8A9 9 0 1111.2 3a7 7 0 009.8 9.8z'])
