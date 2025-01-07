import { Routes, Route } from 'react-router-dom'
import { useState } from 'react'
import Header from './components/Header'
import NewsFeed from './components/NewsFeed'
import AboutPage from './pages/AboutPage'
import SummaryPage from './pages/SummaryPage'
import DigestPage from './pages/DigestPage'
import DigestsPage from './pages/DigestsPage'
import Filters from './components/Filters'
import Digests from './components/Digests'
import MobileMenu from './components/MobileMenu'
import { LanguageProvider } from './contexts/LanguageContext'
import type { Filters as FiltersType } from './types'

export default function App() {
  const [filters, setFilters] = useState<FiltersType>({})
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false)

  return (
    <LanguageProvider>
      <div className="min-h-screen bg-gray-50 dark:bg-gray-950">
        <Header onMobileMenuClick={() => setIsMobileMenuOpen(true)} />
        
        {/* Mobile menu */}
        <MobileMenu 
          isOpen={isMobileMenuOpen} 
          onClose={() => setIsMobileMenuOpen(false)}
        >
          <Filters filters={filters} onFiltersChange={setFilters} />
        </MobileMenu>

        <div className="max-w-7xl mx-auto px-4 py-4 grid grid-cols-12 gap-6">
          {/* Left sidebar - Filters */}
          <div className="hidden md:block md:col-span-3 lg:col-span-3 bg-white dark:bg-gray-900 rounded-xl shadow-sm">
            <Filters filters={filters} onFiltersChange={setFilters} />
          </div>

          {/* Main content */}
          <main className="col-span-12 md:col-span-6 lg:col-span-6">
            <Routes>
              <Route path="/" element={<NewsFeed filters={filters} />} />
              <Route path="/about" element={<AboutPage />} />
              <Route path="/summary/:id" element={<SummaryPage />} />
              <Route path="/digests" element={<DigestsPage filters={filters} />} />
              <Route path="/digests/:id" element={<DigestPage />} />
            </Routes>
          </main>

          {/* Right sidebar - Digests */}
          <div className="hidden md:block md:col-span-3 lg:col-span-3 bg-white dark:bg-gray-900 rounded-xl shadow-sm">
            <Digests filters={filters} />
          </div>
        </div>
      </div>
    </LanguageProvider>
  )
} 