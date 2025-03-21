import { useState } from 'react'
import { useLocalization } from '../hooks/useLocalization'
import { searchSummaries } from '../api/searchApi'
import NewsCard from '../components/NewsCard'
import { useQuery } from '@tanstack/react-query'
import type { Summary, Category, NewsSource } from '../types'
import { subDays } from 'date-fns'
import AdvancedFilters from '../components/AdvancedFilters'

type SortField = 'time' | 'score'
type SortDirection = 'asc' | 'desc'

interface SearchResult {
  summary: Summary
  score: number
}

interface SearchFilters {
  fromDate: Date
  toDate: Date
  categoryIds: number[]
  sourceIds: number[]
}

export default function SearchPage() {
  const { t, language } = useLocalization()
  const [query, setQuery] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [results, setResults] = useState<SearchResult[]>([])
  const [error, setError] = useState<string | null>(null)
  const [sortField, setSortField] = useState<SortField>('score')
  const [sortDirection, setSortDirection] = useState<SortDirection>('desc')
  const [limit, setLimit] = useState(10)
  
  // Initialize filters with last 7 days as default
  const [filters, setFilters] = useState<SearchFilters>({
    fromDate: subDays(new Date(), 7),
    toDate: new Date(),
    categoryIds: [],
    sourceIds: []
  })

  // Fetch categories and sources
  const { data: categories = [] } = useQuery<Category[]>({
    queryKey: ['categories'],
    queryFn: () => fetch('/api/filters/categories').then(res => res.json())
  })

  const { data: sources = [] } = useQuery<NewsSource[]>({
    queryKey: ['sources'],
    queryFn: () => fetch('/api/filters/sources').then(res => res.json())
  })

  const sortResults = (items: SearchResult[]) => {
    return [...items].sort((a, b) => {
      const multiplier = sortDirection === 'asc' ? 1 : -1
      
      if (sortField === 'time') {
        return multiplier * (new Date(a.summary.generatedAt).getTime() - new Date(b.summary.generatedAt).getTime())
      }
      
      return multiplier * (a.score - b.score)
    })
  }

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!query.trim()) return

    setIsLoading(true)
    setError(null)

    try {
      const searchResults = await searchSummaries({
        query: query.trim(),
        language,
        limit,
        fromDate: filters.fromDate,
        toDate: filters.toDate,
        categoryIds: filters.categoryIds,
        sourceIds: filters.sourceIds
      })
      setResults(sortResults(searchResults))
    } catch (err) {
      setError(t.errors.searchFailed)
      setResults([])
    } finally {
      setIsLoading(false)
    }
  }

  const handleSort = (field: SortField) => {
    if (field === sortField) {
      setSortDirection(prev => prev === 'asc' ? 'desc' : 'asc')
    } else {
      setSortField(field)
      setSortDirection('desc')
    }
  }

  const sortedResults = sortResults(results)

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
            <li>{t.search.experimental.tips.tryDifferent}</li>
          </ul>
        </div>
      </div>

      {/* Search Form with Filters */}
      <form onSubmit={handleSearch} className="mb-8 space-y-4">
        {/* Search Input and Button */}
        <div className="flex gap-4">
          <input
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder={t.search.placeholder}
            className="flex-1 p-2 rounded-lg border dark:border-gray-700 bg-white dark:bg-gray-800 
                     text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500 
                     focus:border-transparent outline-none"
          />
          <button
            type="submit"
            disabled={isLoading || !query.trim()}
            className="px-3 py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400
                     text-white rounded-lg font-medium transition-colors
                     disabled:cursor-not-allowed"
          >
            {isLoading ? t.common.loading : t.search.button}
          </button>
        </div>

        {/* Advanced Filters */}
        <AdvancedFilters
          filters={filters}
          onFiltersChange={setFilters}
          limit={limit}
          onLimitChange={setLimit}
          categories={categories}
          sources={sources}
        />
      </form>

      {/* Error Message */}
      {error && (
        <div className="mb-8 p-4 bg-red-50 dark:bg-red-900/30 text-red-600 dark:text-red-400 rounded-lg">
          {error}
        </div>
      )}

      {/* Sort Controls */}
      {results.length > 0 && (
        <div className="mb-4 flex items-center gap-2">
          <span className="text-sm text-gray-500 dark:text-gray-400">{t.search.sort.label}</span>
          <button
            onClick={() => handleSort('time')}
            className={`px-3 py-1.5 text-sm rounded-lg transition-colors flex items-center gap-1
                     ${sortField === 'time'
                       ? 'bg-blue-100 dark:bg-blue-900/50 text-blue-700 dark:text-blue-300'
                       : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800'
                     }`}
          >
            {t.search.sort.time}
            {sortField === 'time' && (
              <svg className={`w-4 h-4 transition-transform ${sortDirection === 'desc' ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
              </svg>
            )}
          </button>
          <button
            onClick={() => handleSort('score')}
            className={`px-3 py-1.5 text-sm rounded-lg transition-colors flex items-center gap-1
                     ${sortField === 'score'
                       ? 'bg-blue-100 dark:bg-blue-900/50 text-blue-700 dark:text-blue-300'
                       : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800'
                     }`}
          >
            {t.search.sort.score}
            {sortField === 'score' && (
              <svg className={`w-4 h-4 transition-transform ${sortDirection === 'desc' ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
              </svg>
            )}
          </button>
        </div>
      )}

      {/* Results */}
      <div className="space-y-6">
        {sortedResults.map((result) => (
          <NewsCard 
            key={result.summary.id} 
            summary={result.summary}
            score={result.score}
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