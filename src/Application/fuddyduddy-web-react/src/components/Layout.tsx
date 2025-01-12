import { useState, useEffect } from 'react'
import { Outlet, useParams, useNavigate, useLocation } from 'react-router-dom'
import Header from './Header'
import Digests from './Digests'
import NewsPanel from './NewsPanel'
import MobileMenu from './MobileMenu'
import { useLanguage, type Language } from '../contexts/LanguageContext'
import { useLayout } from '../contexts/LayoutContext'
import type { Filters as FiltersType } from '../types'

const SUPPORTED_LANGUAGES = ['en', 'ru'] as const
type SupportedLanguage = typeof SUPPORTED_LANGUAGES[number]

function isValidLanguage(lang: string | undefined): lang is SupportedLanguage {
  return !!lang && SUPPORTED_LANGUAGES.includes(lang.toLowerCase() as SupportedLanguage)
}

export default function Layout() {
  const [filters, setFilters] = useState<FiltersType>({})
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false)
  const { lang } = useParams<{ lang: string }>()
  const { language, setLanguage } = useLanguage()
  const navigate = useNavigate()
  const location = useLocation()
  const { setShowSidePanels } = useLayout()
  const showSidePanels = !location.pathname.includes('/digests/') && !location.pathname.includes('/summary/')
  const isDigestsPage = location.pathname.includes('/digests')

  // Handle language from URL
  useEffect(() => {
    if (!lang) return

    const normalizedLang = lang.toUpperCase() as Language
    if (isValidLanguage(lang) && normalizedLang !== language) {
      setLanguage(normalizedLang)
    } else if (!isValidLanguage(lang)) {
      // Redirect to current language if URL language is invalid
      navigate(`/${language.toLowerCase()}${location.pathname.replace(`/${lang}`, '')}`, { replace: true })
    }
  }, [lang, language, setLanguage, navigate, location])

  // Update filters with current language
  useEffect(() => {
    setFilters(prev => ({
      ...prev,
      language
    }))
  }, [language])

  useEffect(() => {
    setShowSidePanels(showSidePanels)
  }, [showSidePanels, setShowSidePanels])

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-950">
      <Header onMobileMenuClick={() => setIsMobileMenuOpen(true)} />
      <MobileMenu isOpen={isMobileMenuOpen} onClose={() => setIsMobileMenuOpen(false)} />
      
      <div className="max-w-7xl mx-auto px-4 py-6 grid grid-cols-12 gap-6">
        {/* Main content */}
        <main className={`col-span-12 ${showSidePanels ? 'lg:col-span-9' : 'lg:col-span-12 max-w-4xl mx-auto w-full'}`}>
          <Outlet context={{ filters, setFilters, showFilters: showSidePanels }} />
        </main>

        {/* Right sidebar - Digests or News */}
        {showSidePanels && (
          <aside className="hidden lg:block lg:col-span-3">
            <div className="sticky top-24">
              <div className="bg-white dark:bg-gray-900 rounded-xl shadow-sm max-h-[calc(100vh-120px)] overflow-y-auto no-scrollbar">
                {isDigestsPage ? (
                  <NewsPanel filters={filters} />
                ) : (
                  <Digests filters={filters} />
                )}
              </div>
            </div>
          </aside>
        )}
      </div>
    </div>
  )
} 