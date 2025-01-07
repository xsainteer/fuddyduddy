import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Routes, Route, Link } from 'react-router-dom'
import { ThemeProvider } from './contexts/ThemeContext'
import { Toaster } from 'react-hot-toast'
import NewsFeed from './components/NewsFeed'
import SummaryPage from './pages/SummaryPage'
import AboutPage from './pages/AboutPage'
import ThemeToggle from './components/ThemeToggle'
import ScrollToTop from './components/ScrollToTop'

const queryClient = new QueryClient()

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <div className="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors">
          <Toaster position="bottom-center" />
          <header className="bg-white/80 dark:bg-gray-800/80 backdrop-blur-sm border-b dark:border-gray-700 sticky top-0 z-10 transition-colors">
            <div className="max-w-2xl mx-auto py-3 px-4">
              <div className="flex justify-between items-center">
                <div>
                  <Link to="/" className="block">
                    <h1 className="text-xl font-bold text-gray-900 dark:text-white">FuddyDuddy News</h1>
                    <p className="text-sm text-gray-600 dark:text-gray-400">Новости Кыргызстана</p>
                  </Link>
                </div>
                <div className="flex items-center gap-4">
                  <Link 
                    to="/about"
                    className="text-sm text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white transition-colors"
                  >
                    About
                  </Link>
                  <ThemeToggle />
                </div>
              </div>
            </div>
          </header>
          <main className="max-w-2xl mx-auto px-4 py-4">
            <Routes>
              <Route path="/" element={<NewsFeed />} />
              <Route path="/summary/:id" element={<SummaryPage />} />
              <Route path="/about" element={<AboutPage />} />
            </Routes>
          </main>
          <ScrollToTop />
        </div>
      </ThemeProvider>
    </QueryClientProvider>
  )
}

export default App 