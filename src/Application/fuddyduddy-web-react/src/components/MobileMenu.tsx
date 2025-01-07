import { useEffect, type ReactNode } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { useLocalization } from '../hooks/useLocalization'

interface MobileMenuProps {
  isOpen: boolean
  onClose: () => void
  children: ReactNode
}

export default function MobileMenu({ isOpen, onClose, children }: MobileMenuProps) {
  const { t } = useLocalization()
  const location = useLocation()
  const isNotHomePage = location.pathname !== '/'

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
            <h2 className="text-xl font-bold text-gray-900 dark:text-white">{t.header.title}</h2>
            <p className="text-sm text-gray-600 dark:text-gray-400">{t.header.subtitle}</p>
          </div>

          {/* Navigation */}
          <nav className="p-4 border-b dark:border-gray-800">
            {isNotHomePage && (
              <Link
                to="/"
                className="block px-3 py-2 rounded-lg text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
                onClick={onClose}
              >
                {t.common.backToFeed}
              </Link>
            )}
            <Link
              to="/about"
              className="block px-3 py-2 rounded-lg text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
              onClick={onClose}
            >
              {t.header.about}
            </Link>
            <Link
              to="/digests"
              className="block px-3 py-2 rounded-lg text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
              onClick={onClose}
            >
              {t.header.digests}
            </Link>
          </nav>

          {/* Filters */}
          <div className="flex-1 overflow-y-auto">
            {children}
          </div>
        </div>
      </div>
    </>
  )
} 