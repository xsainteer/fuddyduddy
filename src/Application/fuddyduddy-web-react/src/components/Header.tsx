import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useLanguage, type Language } from '../contexts/LanguageContext'
import { useTheme } from '../contexts/ThemeContext'
import logoTransparent from '../assets/fuddyduddy_logo_transparent.png'
import logoWhite from '../assets/fuddyduddy_logo_white.jpg'

interface HeaderProps {
  onMobileMenuClick: () => void
}

export default function Header({ onMobileMenuClick }: HeaderProps) {
  const { t, language, setLanguage } = useLanguage()
  const { theme, toggleTheme } = useTheme()
  const location = useLocation()
  const navigate = useNavigate()

  const handleLanguageChange = (newLang: Language) => {
    if (newLang === language) return
    
    // Update language in context
    setLanguage(newLang)
    
    // Update URL to reflect new language
    const currentPath = location.pathname
    const langPrefix = `/${language.toLowerCase()}`
    const pathWithoutLang = currentPath.startsWith(langPrefix) 
      ? currentPath.slice(langPrefix.length) || '/'
      : currentPath
    
    navigate(`/${newLang.toLowerCase()}${pathWithoutLang}${location.search}${location.hash}`)
  }

  return (
    <header className="bg-white/80 dark:bg-gray-800/80 backdrop-blur-sm border-b dark:border-gray-700 sticky top-0 z-10 transition-colors">
      <div className="max-w-7xl mx-auto px-4">
        <div className="flex items-center justify-between h-16">
          {/* Logo and title */}
          <div className="flex items-center">
            <Link to={`/${language.toLowerCase()}/feed`} className="flex items-center gap-3">
              <img 
                src={theme === 'dark' ? logoTransparent : logoWhite} 
                alt="FuddyDuddy Logo" 
                className="w-12 h-12 object-contain"
              />
              <div>
                <h1 className="text-xl font-bold text-gray-900 dark:text-white font-['Bebas_Neue']">{t.header.title}</h1>
                <p className="text-sm text-gray-600 dark:text-gray-400">{t.header.subtitle}</p>
              </div>
            </Link>
          </div>

          {/* Navigation */}
          <nav className="hidden md:flex items-center space-x-6">
            <Link
              to={`/${language.toLowerCase()}/feed`}
              className="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white"
            >
              {t.header.feed}
            </Link>
            <Link
              to={`/${language.toLowerCase()}/digests`}
              className="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white"
            >
              {t.header.digests}
            </Link>
            <Link
              to={`/${language.toLowerCase()}/about`}
              className="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white"
            >
              {t.header.about}
            </Link>
          </nav>

          {/* Right side controls */}
          <div className="flex items-center gap-2 border-l dark:border-gray-700 pl-4">
            {/* Language switcher */}
            <div className="hidden md:flex items-center space-x-2">
              <button
                onClick={() => handleLanguageChange('RU')}
                className={`px-2 py-1 text-sm rounded ${
                  language === 'RU'
                    ? 'bg-blue-100 dark:bg-blue-900 text-blue-600 dark:text-blue-300'
                    : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'
                }`}
              >
                RU
              </button>
              <button
                onClick={() => handleLanguageChange('EN')}
                className={`px-2 py-1 text-sm rounded ${
                  language === 'EN'
                    ? 'bg-blue-100 dark:bg-blue-900 text-blue-600 dark:text-blue-300'
                    : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'
                }`}
              >
                EN
              </button>
            </div>

            {/* Theme toggle */}
            <button
              onClick={toggleTheme}
              className="p-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white"
              aria-label="Toggle theme"
            >
              {theme === 'dark' ? (
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"
                  />
                </svg>
              ) : (
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"
                  />
                </svg>
              )}
            </button>

            {/* Mobile menu button */}
            <button
              onClick={onMobileMenuClick}
              className="md:hidden p-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white"
              aria-label="Open menu"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M4 6h16M4 12h16M4 18h16"
                />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </header>
  )
} 