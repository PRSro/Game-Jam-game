import { ref, computed } from 'vue'
import { useSoundEffects } from './useSoundEffects'

export interface RadioStation {
  id: string
  name: string
  frequency: string
  streamUrl: string
  tagline: string
  accentColor: string
}

export const RADIO_STATIONS: RadioStation[] = [
  {
    id: 'radio-zu',
    name: 'Radio ZU',
    frequency: '89.0 FM',
    streamUrl: 'https://live.radiozu.ro/stream',
    tagline: 'Hiturile se ascultă la ZU',
    accentColor: '#0095ff'
  },
  {
    id: 'bucuresti-fm',
    name: 'București FM',
    frequency: '98.3 FM',
    streamUrl: 'https://stream2.srr.ro/bucurestifm',
    tagline: 'Aici e Bucureștiul!',
    accentColor: '#e63946'
  },
  {
    id: 'europa-fm',
    name: 'Europa FM',
    frequency: '106.7 FM',
    streamUrl: 'https://astreaming.europafm.ro/europafm_mp3_64k',
    tagline: 'Pe aceeași frecvență cu tine',
    accentColor: '#3a86ff'
  },
  {
    id: 'kiss-fm',
    name: 'Kiss FM',
    frequency: '96.1 FM',
    streamUrl: 'https://live.kissfm.ro/kissfm.aacp',
    tagline: '#1 Radio Station in Romania',
    accentColor: '#ff0055'
  }
]

const currentStationId = ref<string>(RADIO_STATIONS[0].id)
const isPlaying = ref(false)
const isLoading = ref(false)
const isMuted = ref(false)
const volume = ref(0.8)
const errorMessage = ref<string | null>(null)

let audio: HTMLAudioElement | null = null
let timeoutTimer: ReturnType<typeof setTimeout> | null = null

const CONNECT_TIMEOUT_MS = 8000

export function useRadio() {
  const { playTuneSound, playErrorSound, playClickSound } = useSoundEffects()

  const currentStation = computed(() => {
    return RADIO_STATIONS.find(s => s.id === currentStationId.value) || RADIO_STATIONS[0]
  })

  const clearTimeoutTimer = () => {
    if (timeoutTimer) {
      clearTimeout(timeoutTimer)
      timeoutTimer = null
    }
  }

  const stopAudio = () => {
    clearTimeoutTimer()
    if (audio) {
      audio.pause()
      audio.src = ''
      audio.load()
      audio = null
    }
    isPlaying.value = false
    isLoading.value = false
  }

  const handleTimeout = () => {
    if (isLoading.value) {
      stopAudio()
      errorMessage.value = 'Stație indisponibilă momentan'
      playErrorSound()
    }
  }

  const initAudio = () => {
    if (!audio) {
      audio = new Audio()
      audio.preload = 'none'

      audio.addEventListener('playing', () => {
        clearTimeoutTimer()
        isLoading.value = false
        isPlaying.value = true
        errorMessage.value = null
      })

      audio.addEventListener('waiting', () => {
        isLoading.value = true
      })

      audio.addEventListener('error', () => {
        clearTimeoutTimer()
        stopAudio()
        errorMessage.value = 'Stație indisponibilă momentan'
        playErrorSound()
      })
    }

    audio.volume = isMuted.value ? 0 : volume.value
    return audio
  }

  const playStation = async (stationId?: string) => {
    if (stationId && stationId !== currentStationId.value) {
      currentStationId.value = stationId
      playTuneSound()
    } else {
      playClickSound()
    }

    errorMessage.value = null
    const station = RADIO_STATIONS.find(s => s.id === currentStationId.value) || RADIO_STATIONS[0]
    
    const player = initAudio()
    player.src = station.streamUrl
    player.load()

    isLoading.value = true
    isPlaying.value = false

    clearTimeoutTimer()
    timeoutTimer = setTimeout(handleTimeout, CONNECT_TIMEOUT_MS)

    try {
      await player.play()
    } catch {
      clearTimeoutTimer()
      isLoading.value = false
      isPlaying.value = false
      errorMessage.value = 'Stație indisponibilă momentan'
      playErrorSound()
    }
  }

  const togglePlay = () => {
    if (isPlaying.value || isLoading.value) {
      playClickSound()
      stopAudio()
    } else {
      playStation()
    }
  }

  const setStation = (stationId: string) => {
    if (currentStationId.value === stationId && isPlaying.value) return
    playStation(stationId)
  }

  const setVolume = (val: number) => {
    volume.value = Math.max(0, Math.min(1, val))
    if (audio) {
      audio.volume = isMuted.value ? 0 : volume.value
    }
  }

  const toggleMute = () => {
    playClickSound()
    isMuted.value = !isMuted.value
    if (audio) {
      audio.volume = isMuted.value ? 0 : volume.value
    }
  }

  return {
    RADIO_STATIONS,
    currentStationId,
    currentStation,
    isPlaying,
    isLoading,
    isMuted,
    volume,
    errorMessage,
    togglePlay,
    setStation,
    setVolume,
    toggleMute,
    stopAudio
  }
}
