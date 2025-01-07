import { Link } from 'react-router-dom'
import ThemeToggle from './ThemeToggle'

interface HeaderProps {
  onMobileMenuClick: () => void
}

export default function Header({ onMobileMenuClick }: HeaderProps) {
  return (
    <header className="bg-white/80 dark:bg-gray-800/80 backdrop-blur-sm border-b dark:border-gray-700 sticky top-0 z-10 transition-colors">
      <div className="max-w-7xl mx-auto py-3 px-4">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-4">
            <button
              onClick={onMobileMenuClick}
              className="md:hidden p-2 -ml-2 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>

            <Link to="/" className="block">
              <h1 className="text-xl font-bold text-gray-900 dark:text-white">FuddyDuddy News</h1>
              <p className="text-sm text-gray-600 dark:text-gray-400">Новости Кыргызстана</p>
            </Link>
          </div>

          <div className="flex items-center gap-4">
            <Link 
              to="/digests"
              className="md:hidden text-sm text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white transition-colors"
            >
              Digests
            </Link>

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
  )
} 