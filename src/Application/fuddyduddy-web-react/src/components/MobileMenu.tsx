import { useEffect } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useLocalization } from '../hooks/useLocalization'
import { useTheme } from '../contexts/ThemeContext'
import logoTransparent from '../assets/fuddyduddy_logo_transparent.png'
import logoWhite from '../assets/fuddyduddy_logo_white.jpg'

interface MobileMenuProps {
  isOpen: boolean
  onClose: () => void
}

export default function MobileMenu({ isOpen, onClose }: MobileMenuProps) {
  const { t, language } = useLocalization()
  const { theme } = useTheme()
  const location = useLocation()
  const navigate = useNavigate()

  // Close menu when pressing Escape
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }

    if (isOpen) {
      document.addEventListener('keydown', handleEscape)
      // Prevent body scroll when menu is open
      document.body.style.overflow = 'hidden'
    }

    return () => {
      document.removeEventListener('keydown', handleEscape)
      document.body.style.overflow = ''
    }
  }, [isOpen, onClose])

  if (!isOpen) return null

  return (
    <>
      {/* Backdrop */}
      <div 
        className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40 transition-opacity"
        onClick={onClose}
      />

      {/* Menu */}
      <div className={`fixed inset-y-0 left-0 w-80 bg-white dark:bg-gray-900 shadow-xl z-50 transform transition-transform duration-300 ease-in-out ${
        isOpen ? 'translate-x-0' : '-translate-x-full'
      }`}>
        <div className="flex flex-col h-full">
          {/* Header */}
          <div className="p-4 border-b dark:border-gray-800">
            <button
              onClick={onClose}
              className="absolute top-4 right-4 p-2 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
            <div className="flex items-center gap-3">
              <img 
                src={theme === 'dark' ? logoTransparent : logoWhite} 
                alt="FuddyDuddy Logo" 
                className="w-12 h-12 object-contain"
              />
              <div>
                <h2 className="text-xl font-bold text-gray-900 dark:text-white font-['Bebas_Neue']">{t.header.title}</h2>
                <p className="text-sm text-gray-600 dark:text-gray-400">{t.header.subtitle}</p>
              </div>
            </div>
          </div>

          {/* Navigation */}
          <nav className="p-4 border-b dark:border-gray-800">
            <Link
              to={`/${language.toLowerCase()}/feed`}
              className="block px-3 py-2 rounded-lg text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
              onClick={onClose}
            >
              {t.header.feed}
            </Link>
            <Link
              to={`/${language.toLowerCase()}/digests`}
              className="block px-3 py-2 rounded-lg text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
              onClick={onClose}
            >
              {t.header.digests}
            </Link>
            <Link
              to={`/${language.toLowerCase()}/about`}
              className="block px-3 py-2 rounded-lg text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
              onClick={onClose}
            >
              {t.header.about}
            </Link>
          </nav>

          {/* Language switcher */}
          <div className="p-4 border-b dark:border-gray-800">
            <h3 className="px-3 mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
              {t.header.language}
            </h3>
            <div className="flex gap-2 px-3">
              <button
                onClick={() => {
                  const path = location.pathname.replace(`/${language.toLowerCase()}`, '/ru')
                  navigate(path)
                  onClose()
                }}
                className={`px-3 py-1.5 text-sm font-medium rounded-lg transition-colors ${
                  language === 'RU'
                    ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300'
                    : 'text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white hover:bg-gray-50 dark:hover:bg-gray-800/50'
                }`}
              >
                RU
              </button>
              <button
                onClick={() => {
                  const path = location.pathname.replace(`/${language.toLowerCase()}`, '/en')
                  navigate(path)
                  onClose()
                }}
                className={`px-3 py-1.5 text-sm font-medium rounded-lg transition-colors ${
                  language === 'EN'
                    ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300'
                    : 'text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white hover:bg-gray-50 dark:hover:bg-gray-800/50'
                }`}
              >
                EN
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  )
} 