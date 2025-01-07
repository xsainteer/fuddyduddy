import { useLanguage } from '../contexts/LanguageContext'
import { useTranslations } from '../translations'

export function useLocalization() {
  const { language, setLanguage } = useLanguage()
  const translations = useTranslations(language)

  return {
    language,
    setLanguage,
    t: translations
  }
} 