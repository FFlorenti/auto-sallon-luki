import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import '@fontsource-variable/inter'
import '@fontsource-variable/saira/wdth.css'
import en from './en.json'
import sq from './sq.json'

export const LANGUAGES = ['sq', 'en'] as const
export type Language = (typeof LANGUAGES)[number]

const STORAGE_KEY = 'autoluki.lang'

function initialLanguage(): Language {
  try {
    const saved = localStorage.getItem(STORAGE_KEY)
    if (saved === 'sq' || saved === 'en') return saved
  } catch {
  }
  return navigator.language.startsWith('en') ? 'en' : 'sq'
}

void i18n.use(initReactI18next).init({
  resources: { en: { translation: en }, sq: { translation: sq } },
  lng: initialLanguage(),
  fallbackLng: 'sq',
  interpolation: { escapeValue: false },
})

i18n.on('languageChanged', (lng) => {
  document.documentElement.lang = lng
  try {
    localStorage.setItem(STORAGE_KEY, lng)
  } catch {
  }
})
document.documentElement.lang = i18n.language

export default i18n
