import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useLocalization } from '../hooks/useLocalization'
import { fetchLatestDigests } from '../api/digests'
import type { Digest, Filters } from '../types'

const PAGE_SIZE = 20

interface DigestsPageProps {
  filters: Filters
}

export default function DigestsPage({ filters }: DigestsPageProps) {
  const { t } = useLocalization()
  const [digests, setDigests] = useState<Digest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [hasMore, setHasMore] = useState(true)

  useEffect(() => {
    const loadInitialDigests = async () => {
      try {
        setLoading(true)
        setError(null)
        const data = await fetchLatestDigests(filters.language || 'RU', PAGE_SIZE)
        setDigests(data)
        setHasMore(data.length === PAGE_SIZE)
      } catch (err) {
        setError(t.errors.failedToLoadDigests)
        console.error('Failed to load digests:', err)
      } finally {
        setLoading(false)
      }
    }

    loadInitialDigests()
  }, [filters.language, t.errors.failedToLoadDigests])

  const loadMore = async () => {
    try {
      setLoading(true)
      const data = await fetchLatestDigests(filters.language || 'RU', PAGE_SIZE, digests.length)
      setDigests(prev => [...prev, ...data])
      setHasMore(data.length === PAGE_SIZE)
    } catch (err) {
      setError(t.errors.failedToLoadDigests)
      console.error('Failed to load more digests:', err)
    } finally {
      setLoading(false)
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
        {t.common.backToFeed}
      </Link>

      <div className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
        <h1 className="text-2xl font-bold text-gray-800 dark:text-gray-200 mb-6">
          {t.digests.title}
        </h1>

        <div className="space-y-4">
          {digests.map(digest => (
            <Link
              key={digest.id}
              to={`/digests/${digest.id}`}
              className="block p-4 rounded-lg border dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
            >
              <h2 className="text-lg font-medium text-gray-800 dark:text-gray-200 mb-2">
                {digest.title}
              </h2>
              <p className="text-sm text-gray-500 dark:text-gray-400">
                {new Date(digest.generatedAt).toLocaleString()}
              </p>
            </Link>
          ))}

          {loading && (
            <div className="animate-pulse space-y-4">
              {[...Array(3)].map((_, i) => (
                <div
                  key={i}
                  className="p-4 rounded-lg border dark:border-gray-800"
                >
                  <div className="h-6 bg-gray-200 dark:bg-gray-800 rounded w-3/4 mb-2" />
                  <div className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-1/4" />
                </div>
              ))}
            </div>
          )}

          {hasMore && !loading && (
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