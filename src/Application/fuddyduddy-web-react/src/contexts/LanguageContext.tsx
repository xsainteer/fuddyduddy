import { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { useTranslations } from '../translations'

export type Language = 'EN' | 'RU'

interface LanguageContextType {
  language: Language
  setLanguage: (lang: Language) => void
  t: ReturnType<typeof useTranslations>
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined)

function getInitialLanguage(): Language {
  // Try to get language from localStorage
  const savedLang = localStorage.getItem('language') as Language
  if (savedLang && ['EN', 'RU'].includes(savedLang)) {
    return savedLang
  }

  // Try to get language from browser
  const browserLang = navigator.language.split('-')[0].toUpperCase()
  if (browserLang === 'RU') {
    return 'RU'
  }

  // Default to English
  return 'EN'
}

export function LanguageProvider({ children }: { children: ReactNode }) {
  const [language, setLanguage] = useState<Language>(getInitialLanguage)
  const translations = useTranslations(language)

  useEffect(() => {
    localStorage.setItem('language', language)
  }, [language])

  return (
    <LanguageContext.Provider value={{ language, setLanguage, t: translations }}>
      {children}
    </LanguageContext.Provider>
  )
}

export function useLanguage() {
  const context = useContext(LanguageContext)
  if (!context) {
    throw new Error('useLanguage must be used within a LanguageProvider')
  }
  return context
} 