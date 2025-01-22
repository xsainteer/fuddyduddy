import { useEffect } from 'react'
import { BrowserRouter, Routes, Route, Navigate, useLocation, useNavigate } from 'react-router-dom'
import Layout from './components/Layout'
import HomePage from './pages/HomePage'
import DigestsPage from './pages/DigestsPage'
import DigestPage from './pages/DigestPage'
import SummaryPage from './pages/SummaryPage'
import AboutPage from './pages/AboutPage'
import SearchPage from './pages/SearchPage'
import { ThemeProvider } from './contexts/ThemeContext'
import { LanguageProvider, useLanguage } from './contexts/LanguageContext'
import { LayoutProvider } from './contexts/LayoutContext'

function LanguageRedirect() {
  const navigate = useNavigate()
  const location = useLocation()
  const { language } = useLanguage()

  useEffect(() => {
    // Only redirect if we're at the root
    if (location.pathname === '/') {
      navigate(`/${language.toLowerCase()}/digests${location.search}${location.hash}`, { replace: true })
    }
  }, [navigate, location, language])

  return null
}

export default function App() {
  return (
    <BrowserRouter>
      <ThemeProvider>
        <LanguageProvider>
          <LayoutProvider>
            <Routes>
              {/* Root redirect */}
              <Route path="/" element={<LanguageRedirect />} />

              {/* Language-specific routes */}
              <Route path="/:lang" element={<Layout />}>
                <Route index element={<Navigate to="digests" replace />} />
                <Route path="feed" element={<HomePage />} />
                <Route path="digests" element={<DigestsPage />} />
                <Route path="digests/:id" element={<DigestPage />} />
                <Route path="summary/:id" element={<SummaryPage />} />
                <Route path="about" element={<AboutPage />} />
                <Route path="search" element={<SearchPage />} />
              </Route>

              {/* Catch-all redirect to root for language detection */}
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </LayoutProvider>
        </LanguageProvider>
      </ThemeProvider>
    </BrowserRouter>
  )
} 