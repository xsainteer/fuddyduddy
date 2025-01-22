import { useState } from 'react'
import { useLocalization } from '../hooks/useLocalization'
import { searchSummaries } from '../api/searchApi'
import NewsCard from '../components/NewsCard'
import type { Summary } from '../types'

interface SearchResult {
  summary: Summary
  score: number
}

export default function SearchPage() {
  const { t, language } = useLocalization()
  const [query, setQuery] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [results, setResults] = useState<SearchResult[]>([])
  const [error, setError] = useState<string | null>(null)

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!query.trim()) return

    setIsLoading(true)
    setError(null)

    try {
      const searchResults = await searchSummaries({
        query: query.trim(),
        language: language
      })
      setResults(searchResults)
    } catch (err) {
      setError(t.errors.searchFailed)
      setResults([])
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Experimental Feature Warning */}
      <div className="mb-8 p-4 bg-yellow-50 dark:bg-yellow-900/30 border border-yellow-200 dark:border-yellow-900 rounded-lg">
        <h2 className="text-lg font-semibold text-yellow-800 dark:text-yellow-200 mb-2">
          {t.search.experimental.title}
        </h2>
        <p className="text-yellow-700 dark:text-yellow-300 mb-4">
          {t.search.experimental.description}
        </p>
        <div className="text-yellow-600 dark:text-yellow-400">
          <p className="font-medium mb-2">{t.search.experimental.tips.title}</p>
          <ul className="list-disc list-inside space-y-1 text-sm">
            <li>{t.search.experimental.tips.useNaturalLanguage}</li>
            <li>{t.search.experimental.tips.beSpecific}</li>
            <li>{t.search.experimental.tips.includeContext}</li>
            <li>{t.search.experimental.tips.tryDifferent}</li>
          </ul>
        </div>
      </div>

      {/* Search Form */}
      <form onSubmit={handleSearch} className="mb-8">
        <div className="flex gap-4">
          <input
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder={t.search.placeholder}
            className="flex-1 p-3 rounded-lg border dark:border-gray-700 bg-white dark:bg-gray-800 
                     text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500 
                     focus:border-transparent outline-none"
          />
          <button
            type="submit"
            disabled={isLoading || !query.trim()}
            className="px-6 py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400
                     text-white rounded-lg font-medium transition-colors
                     disabled:cursor-not-allowed"
          >
            {isLoading ? t.common.loading : t.search.button}
          </button>
        </div>
      </form>

      {/* Error Message */}
      {error && (
        <div className="mb-8 p-4 bg-red-50 dark:bg-red-900/30 text-red-600 dark:text-red-400 rounded-lg">
          {error}
        </div>
      )}

      {/* Results */}
      <div className="space-y-6">
        {results.map((result) => (
          <NewsCard 
            key={result.summary.id} 
            summary={result.summary}
          />
        ))}
        {results.length === 0 && query && !isLoading && !error && (
          <div className="text-center text-gray-500 dark:text-gray-400 py-8">
            {t.search.noResults}
          </div>
        )}
      </div>
    </div>
  )
} 