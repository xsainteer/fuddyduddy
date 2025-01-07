import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'

export type Language = 'EN' | 'RU'

interface LanguageContextType {
  language: Language
  setLanguage: (language: Language) => void
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined)

function getDefaultLanguage(): Language {
  // First check localStorage
  const saved = localStorage.getItem('language')
  if (saved === 'EN' || saved === 'RU') {
    return saved
  }

  // Then check browser language
  const browserLang = navigator.language.toLowerCase()
  // If it starts with 'en', use English, otherwise Russian
  return browserLang.startsWith('en') ? 'EN' : 'RU'
}

export function LanguageProvider({ children }: { children: ReactNode }) {
  const [language, setLanguage] = useState<Language>(getDefaultLanguage)

  useEffect(() => {
    localStorage.setItem('language', language)
  }, [language])

  return (
    <LanguageContext.Provider value={{ language, setLanguage }}>
      {children}
    </LanguageContext.Provider>
  )
}

export function useLanguage() {
  const context = useContext(LanguageContext)
  if (context === undefined) {
    throw new Error('useLanguage must be used within a LanguageProvider')
  }
  return context
} 