import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import type { Summary, Filters } from '../types'
import { useLocalization } from '../hooks/useLocalization'
import { fetchLatestSummaries, summaryKeys } from '../api/summaries'
import { formatDateTime } from '../utils/dateFormat'

interface NewsPanelProps {
  className?: string
  filters?: Filters
}

const POLLING_INTERVAL = 60000 // 1 minute
const LAST_VIEWED_KEY = 'lastViewedSummaryTimestamp'

export default function NewsPanel({ className = '', filters = {} }: NewsPanelProps) {
  const { t, language: interfaceLanguage } = useLocalization()
  const [lastViewedTimestamp, setLastViewedTimestamp] = useState<number>(
    parseInt(localStorage.getItem(LAST_VIEWED_KEY) || '0')
  )

  const { data: summaries, isLoading, error } = useQuery<Summary[]>({
    queryKey: summaryKeys.latest(filters.language || 'RU', 10),
    queryFn: () => fetchLatestSummaries(filters.language || 'RU', 10),
    refetchInterval: POLLING_INTERVAL,
    staleTime: POLLING_INTERVAL / 2,
  })

  // Update last viewed timestamp when summaries change
  useEffect(() => {
    if (summaries?.length) {
      const latestTimestamp = Math.max(
        ...summaries.map(d => new Date(d.generatedAt).getTime())
      )
      localStorage.setItem(LAST_VIEWED_KEY, latestTimestamp.toString())
      setLastViewedTimestamp(latestTimestamp)
    }
  }, [summaries])

  return (
    <div className={`p-4 space-y-4 ${className}`}>
      <h2 className="text-xl font-bold text-gray-800 dark:text-gray-200">
        {t.common.latestNews}
      </h2>

      {isLoading ? (
        <div className="animate-pulse space-y-3">
          {[...Array(5)].map((_, i) => (
            <div
              key={i}
              className="p-3 rounded-lg border dark:border-gray-800 bg-white dark:bg-gray-900"
            >
              <div className="h-5 bg-gray-200 dark:bg-gray-800 rounded w-3/4 mb-2" />
              <div className="space-y-1 mb-2">
                <div className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-full" />
                <div className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-5/6" />
              </div>
              <div className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-1/4" />
            </div>
          ))}
        </div>
      ) : error ? (
        <p className="text-red-500 dark:text-red-400">{t.errors.failedToLoadSummaries}</p>
      ) : (
        <>
          <div className="space-y-3">
            {summaries?.map(summary => {
              const isNew = new Date(summary.generatedAt).getTime() > lastViewedTimestamp
              return (
                <Link
                  key={summary.id}
                  to={`/${filters.language?.toLowerCase()}/summary/${summary.id}`}
                  className={`block p-3 rounded-lg border dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer transition-colors relative
                    ${isNew ? 'border-blue-500 dark:border-blue-400 bg-blue-50 dark:bg-blue-500/5 animate-highlight-pulse' : 'bg-white dark:bg-gray-900'}`}
                >
                  {isNew && (
                    <span className="absolute -top-2 -right-2 px-2 py-1 text-xs font-medium bg-blue-500 text-white rounded-full shadow-sm">
                      {t.common.new}
                    </span>
                  )}
                  <h3 className="font-medium text-gray-800 dark:text-gray-200 mb-1">
                    {summary.title}
                  </h3>
                  <div className="text-sm text-gray-600 dark:text-gray-400 mb-2 line-clamp-3">
                    {summary.article}
                  </div>
                  <p className="text-sm text-gray-500 dark:text-gray-400">
                    {formatDateTime(summary.generatedAt, interfaceLanguage)}
                  </p>
                </Link>
              )
            })}
          </div>

          {(summaries && summaries.length > 0) && (
            <Link
              to={`/${filters.language?.toLowerCase()}/feed`}
              className="block text-sm text-blue-600 dark:text-blue-400 hover:underline"
            >
              {t.common.viewAllNews}
            </Link>
          )}
        </>
      )}
    </div>
  )
} 