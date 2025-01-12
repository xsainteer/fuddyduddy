import React, { useEffect, useState } from 'react'
import { Link, useOutletContext } from 'react-router-dom'
import { useInfiniteQuery } from '@tanstack/react-query'
import { useLocalization } from '../hooks/useLocalization'
import { useLayout } from '../contexts/LayoutContext'
import { fetchLatestDigests, digestKeys } from '../api/digests'
import type { Digest, Filters } from '../types'
import { formatDateTime } from '../utils/dateFormat'

const PAGE_SIZE = 24
const LAST_VIEWED_KEY = 'lastViewedDigestTimestamp'

interface ContextType {
  filters: Filters
}

export default function DigestsPage() {
  const { filters } = useOutletContext<ContextType>()
  const { t, language: interfaceLanguage } = useLocalization()
  const { setShowSidePanels } = useLayout()
  const [lastViewedTimestamp, setLastViewedTimestamp] = useState<number>(
    parseInt(localStorage.getItem(LAST_VIEWED_KEY) || '0')
  )

  // Hide side panels when component mounts, show them when unmounts
  useEffect(() => {
    setShowSidePanels(false)
    return () => setShowSidePanels(true)
  }, [setShowSidePanels])

  const {
    data,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading,
    error
  } = useInfiniteQuery<Digest[]>({
    queryKey: digestKeys.latest(filters.language || 'RU', PAGE_SIZE),
    queryFn: async ({ pageParam = 0 }) => fetchLatestDigests(filters.language || 'RU', PAGE_SIZE, pageParam as number),
    getNextPageParam: (lastPage: Digest[], allPages: Digest[][]) => 
      lastPage.length === PAGE_SIZE ? allPages.length : undefined,
    initialPageParam: 0,
    staleTime: 60000, // 1 minute
  })

  // Update last viewed timestamp when new digests are loaded
  useEffect(() => {
    if (data?.pages[0]?.length) {
      const latestTimestamp = Math.max(
        ...data.pages[0].map(d => new Date(d.generatedAt).getTime())
      )
      localStorage.setItem(LAST_VIEWED_KEY, latestTimestamp.toString())
      setLastViewedTimestamp(latestTimestamp)
    }
  }, [data?.pages[0]])

  if (error && !data?.pages?.length) {
    return (
      <div className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
        <div className="text-center py-8">
          <p className="text-red-500 dark:text-red-400 mb-4">{t.errors.failedToLoadDigests}</p>
          <Link to={`/${filters.language?.toLowerCase()}`} className="text-blue-600 dark:text-blue-400 hover:underline">
            {t.common.backToFeed}
          </Link>
        </div>
      </div>
    )
  }

  return (
    <>
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
          {data?.pages.map((page, pageIndex) => (
            <React.Fragment key={pageIndex}>
              {page.map(digest => {
                const uniqueSources = Array.from(new Set(digest.references.map(ref => ref.source)))
                  .filter(Boolean)
                  .slice(0, 3)

                const isNew = new Date(digest.generatedAt).getTime() > lastViewedTimestamp

                return (
                  <Link
                    key={digest.id}
                    to={`/${filters.language?.toLowerCase()}/digests/${digest.id}`}
                    className={`block p-4 rounded-lg border dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors relative ${
                      isNew ? 'border-blue-500 dark:border-blue-400 bg-blue-50 dark:bg-blue-500/5 animate-highlight-pulse' : ''
                    }`}
                  >
                    {isNew && (
                      <span className="absolute -top-2 -right-2 px-2 py-1 text-xs font-medium bg-blue-500 text-white rounded-full shadow-sm">
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
            </React.Fragment>
          ))}

          {isLoading && (
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

          {hasNextPage && !isFetchingNextPage && data?.pages?.length > 0 && (
            <button
              onClick={() => fetchNextPage()}
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