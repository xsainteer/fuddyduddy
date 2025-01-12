import { useEffect, useState, useCallback, useRef } from 'react'
import { Link } from 'react-router-dom'
import { useLocalization } from '../hooks/useLocalization'
import { useLayout } from '../contexts/LayoutContext'
import { fetchLatestDigests } from '../api/digests'
import type { Digest, Filters } from '../types'
import { formatDateTime } from '../utils/dateFormat'

const PAGE_SIZE = 24
const LAST_VIEWED_KEY = 'lastViewedDigestTimestamp'

interface DigestsPageProps {
  filters: Filters
}

export default function DigestsPage({ filters }: DigestsPageProps) {
  const { t, language: interfaceLanguage } = useLocalization()
  const { setShowSidePanels } = useLayout()
  const [digests, setDigests] = useState<Digest[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [hasMore, setHasMore] = useState(true)
  const [page, setPage] = useState(0)
  const isLoadingRef = useRef(false)
  const [lastViewedTimestamp, setLastViewedTimestamp] = useState<number>(
    parseInt(localStorage.getItem(LAST_VIEWED_KEY) || '0')
  )

  // Hide side panels when component mounts, show them when unmounts
  useEffect(() => {
    setShowSidePanels(false)
    return () => setShowSidePanels(true)
  }, [setShowSidePanels])

  // Reset state when language changes
  useEffect(() => {
    setDigests([])
    setPage(0)
    setHasMore(true)
    setLoading(false)
    isLoadingRef.current = false
  }, [filters.language])

  // Update last viewed timestamp when new digests are loaded
  useEffect(() => {
    if (digests.length > 0) {
      const latestTimestamp = Math.max(
        ...digests.map(d => new Date(d.generatedAt).getTime())
      )
      localStorage.setItem(LAST_VIEWED_KEY, latestTimestamp.toString())
      setLastViewedTimestamp(latestTimestamp)
    }
  }, [digests])

  const loadDigests = useCallback(async (pageToLoad: number) => {
    if (isLoadingRef.current) return
    
    try {
      isLoadingRef.current = true
      setLoading(true)
      setError(null)
      
      const data = await fetchLatestDigests(filters.language || 'RU', PAGE_SIZE, pageToLoad)
      
      setDigests(prev => {
        if (pageToLoad === 0) return data
        return [...prev, ...data]
      })
      
      setHasMore(data.length === PAGE_SIZE)
    } catch (err) {
      setError(t.errors.failedToLoadDigests)
      console.error('Failed to load digests:', err)
    } finally {
      setLoading(false)
      isLoadingRef.current = false
    }
  }, [filters.language, t.errors.failedToLoadDigests])

  // Initial load
  useEffect(() => {
    loadDigests(0)
  }, [loadDigests])

  const loadMore = () => {
    if (!isLoadingRef.current && hasMore) {
      const nextPage = page + 1
      setPage(nextPage)
      loadDigests(nextPage)
    }
  }

  if (error && digests.length === 0) {
    return (
      <div className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
        <div className="text-center py-8">
          <p className="text-red-500 dark:text-red-400 mb-4">{error}</p>
          <Link to="/" className="text-blue-600 dark:text-blue-400 hover:underline">
            {t.common.backToFeed}
          </Link>
        </div>
      </div>
    )
  }

  return (
    <>
      <Link
        to="/"
        className="inline-block mb-4 text-blue-600 dark:text-blue-400 hover:underline"
      >
        ← {t.common.backToFeed}
      </Link>

      <div className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
        <h1 className="text-2xl font-bold text-gray-800 dark:text-gray-200 mb-4">
          {t.digests.title}
        </h1>

        <div className="prose dark:prose-invert max-w-none mb-8">
          <p className="text-gray-600 dark:text-gray-400">
            {t.digests.description}
          </p>
        </div>

        <div className="space-y-4">
          {digests.map(digest => {
            const uniqueSources = Array.from(new Set(digest.references.map(ref => ref.source)))
              .filter(Boolean)
              .slice(0, 3)

            const isNew = new Date(digest.generatedAt).getTime() > lastViewedTimestamp

            return (
              <Link
                key={digest.id}
                to={`/digests/${digest.id}`}
                className={`block p-4 rounded-lg border dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors relative ${
                  isNew ? 'border-blue-500 dark:border-blue-400' : ''
                }`}
              >
                {isNew && (
                  <span className="absolute -top-2 -right-2 px-2 py-1 text-xs font-medium bg-blue-500 text-white rounded-full">
                    {t.common.new}
                  </span>
                )}
                <h2 className="text-lg font-medium text-gray-800 dark:text-gray-200 mb-2">
                  {digest.title}
                </h2>
                <div className="text-sm text-gray-600 dark:text-gray-400 mb-2 line-clamp-3">
                  {digest.content.split('\n')[0]}
                </div>
                {uniqueSources.length > 0 && (
                  <div className="flex flex-wrap gap-2 mb-2">
                    {uniqueSources.map((source, index) => (
                      <span
                        key={index}
                        className="inline-flex items-center px-2 py-1 rounded text-xs bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400"
                      >
                        {source}
                      </span>
                    ))}
                    {digest.references.length > uniqueSources.length && (
                      <span className="inline-flex items-center px-2 py-1 rounded text-xs bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400">
                        +{digest.references.length - uniqueSources.length}
                      </span>
                    )}
                  </div>
                )}
                <p className="text-sm text-gray-500 dark:text-gray-400">
                  {formatDateTime(digest.generatedAt, interfaceLanguage)}
                </p>
              </Link>
            )
          })}

          {loading && (
            <div className="animate-pulse space-y-4">
              {[...Array(3)].map((_, i) => (
                <div
                  key={i}
                  className="p-4 rounded-lg border dark:border-gray-800"
                >
                  <div className="h-6 bg-gray-200 dark:bg-gray-800 rounded w-3/4 mb-2" />
                  <div className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-full mb-2" />
                  <div className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-1/4" />
                </div>
              ))}
            </div>
          )}

          {hasMore && !loading && digests.length > 0 && (
            <button
              onClick={loadMore}
              className="w-full p-4 text-center text-blue-600 dark:text-blue-400 hover:underline"
            >
              {t.common.loadMore}
            </button>
          )}
        </div>
      </div>
    </>
  )
} 