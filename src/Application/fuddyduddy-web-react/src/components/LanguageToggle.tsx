import { useLanguage } from '../contexts/LanguageContext'

export default function LanguageToggle() {
  const { language, setLanguage } = useLanguage()

  return (
    <button
      onClick={() => setLanguage(language === 'RU' ? 'EN' : 'RU')}
      className="px-2 py-1 text-sm font-medium text-gray-700 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition-colors"
    >
      {language}
    </button>
  )
} 