import { useEffect, useState, useRef } from 'react'
import { Link } from 'react-router-dom'
import type { Digest, Filters } from '../types'
import { useLocalization } from '../hooks/useLocalization'
import { fetchLatestDigests } from '../api/digests'
import DigestSkeleton from './DigestSkeleton'
import { formatDateTime } from '../utils/dateFormat'

interface DigestsProps {
  className?: string
  filters?: Filters
}

function getContentPreview(content: string): string {
  const lines = content.split('\n').filter(line => line.trim())
  const previewLines = lines.slice(0, 3).map(line => {
    if (line.length > 120) {
      return line.slice(0, 120) + '...'
    }
    return line
  })
  
  if (lines.length > 3) {
    return previewLines.join('\n') + '...'
  }
  return previewLines.join('\n')
}

const POLLING_INTERVAL = 60000 // 1 minute
const LAST_VIEWED_KEY = 'lastViewedDigestTimestamp'

export default function Digests({ className = '', filters = {} }: DigestsProps) {
  const { t, language: interfaceLanguage } = useLocalization()
  const [digests, setDigests] = useState<Digest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [lastViewedTimestamp, setLastViewedTimestamp] = useState<number>(
    parseInt(localStorage.getItem(LAST_VIEWED_KEY) || '0')
  )
  const pollingTimeoutRef = useRef<number>()

  const loadDigests = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await fetchLatestDigests(filters.language || 'RU', 5)
      setDigests(data)
    } catch (err) {
      setError(t.errors.failedToLoadDigests)
      console.error('Failed to load digests:', err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadDigests()

    // Set up polling
    pollingTimeoutRef.current = window.setInterval(loadDigests, POLLING_INTERVAL)

    return () => {
      if (pollingTimeoutRef.current) {
        clearInterval(pollingTimeoutRef.current)
      }
    }
  }, [filters.language])

  // Update last viewed timestamp when component mounts or digests change
  useEffect(() => {
    if (digests.length > 0) {
      const latestTimestamp = Math.max(
        ...digests.map(d => new Date(d.generatedAt).getTime())
      )
      localStorage.setItem(LAST_VIEWED_KEY, latestTimestamp.toString())
      setLastViewedTimestamp(latestTimestamp)
    }
  }, [digests])

  return (
    <div className={`p-4 space-y-4 ${className}`}>
      <h2 className="text-xl font-bold text-gray-800 dark:text-gray-200">
        {t.digests.title}
      </h2>

      {loading ? (
        <DigestSkeleton />
      ) : error ? (
        <p className="text-red-500 dark:text-red-400">{error}</p>
      ) : (
        <>
          <div className="space-y-3">
            {digests.map(digest => {
              const isNew = new Date(digest.generatedAt).getTime() > lastViewedTimestamp
              return (
                <Link
                  key={digest.id}
                  to={`/digests/${digest.id}`}
                  className={`block p-3 rounded-lg border dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer transition-colors relative
                    ${isNew ? 'border-blue-500 dark:border-blue-400 bg-blue-50 dark:bg-blue-500/5 animate-highlight-pulse' : 'bg-white dark:bg-gray-900'}`}
                >
                  {isNew && (
                    <span className="absolute -top-2 -right-2 px-2 py-1 text-xs font-medium bg-blue-500 text-white rounded-full shadow-sm">
                      {t.common.new}
                    </span>
                  )}
                  <h3 className="font-medium text-gray-800 dark:text-gray-200 mb-1">
                    {digest.title}
                  </h3>
                  <div className="text-sm text-gray-600 dark:text-gray-400 mb-2 whitespace-pre-line line-clamp-3">
                    {getContentPreview(digest.content)}
                  </div>
                  <p className="text-sm text-gray-500 dark:text-gray-400">
                    {formatDateTime(digest.generatedAt, interfaceLanguage)}
                  </p>
                </Link>
              )
            })}
          </div>

          {digests.length > 0 && (
            <Link
              to="/digests"
              className="block text-sm text-blue-600 dark:text-blue-400 hover:underline"
            >
              {t.digests.viewAll}
            </Link>
          )}
        </>
      )}

      <p className="text-sm text-gray-500 dark:text-gray-400 italic">
        {t.digests.generatedEveryHour}
      </p>
    </div>
  )
} 