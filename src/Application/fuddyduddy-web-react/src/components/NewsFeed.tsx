import { useEffect, useState, useCallback } from 'react'
import { useInfiniteQuery } from '@tanstack/react-query'
import type { Summary, Filters } from '../types'
import NewsCard from './NewsCard'
import { useLocalization } from '../hooks/useLocalization'
import UpdateNotification from './UpdateNotification'

interface PageData {
  items: Summary[]
  hasMore: boolean
}

interface NewsFeedProps {
  filters: Filters
}

const PAGE_SIZE = 10
const SCROLL_THRESHOLD = 300

export default function NewsFeed({ filters }: NewsFeedProps) {
  const { t } = useLocalization()
  const [showScrollTop, setShowScrollTop] = useState(false)

  const {
    data,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    refetch,
    isLoading
  } = useInfiniteQuery({
    queryKey: ['summaries', filters],
    queryFn: async ({ pageParam = 0 }) => {
      const searchParams = new URLSearchParams()
      searchParams.append('page', pageParam.toString())
      if (filters.categoryId) searchParams.append('categoryId', filters.categoryId.toString())
      if (filters.sourceId) searchParams.append('sourceId', filters.sourceId.toString())
      if (filters.language) searchParams.append('language', filters.language)

      const response = await fetch(`/api/summaries?${searchParams.toString()}`)
      if (!response.ok) throw new Error('Network response was not ok')
      const summaries = await response.json() as Summary[]
      
      // Store the latest summary ID when loading the first page
      if (pageParam === 0 && summaries.length > 0) {
        localStorage.setItem('latestSummaryId', summaries[0].id)
      }
      
      return {
        items: summaries,
        hasMore: summaries.length >= PAGE_SIZE
      }
    },
    getNextPageParam: (lastPage: PageData, allPages) => 
      lastPage.hasMore ? allPages.length : undefined,
    initialPageParam: 0
  })

  // Infinite scroll handler
  const handleScroll = useCallback(() => {
    setShowScrollTop(window.scrollY > SCROLL_THRESHOLD)

    if (window.innerHeight + window.scrollY >= document.documentElement.scrollHeight - 1000) {
      if (hasNextPage && !isFetchingNextPage) {
        fetchNextPage()
      }
    }
  }, [hasNextPage, isFetchingNextPage, fetchNextPage])

  useEffect(() => {
    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [handleScroll])

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  if (error) {
    return (
      <div className="text-center py-8">
        <p className="text-red-500 dark:text-red-400 mb-4">{t.common.error}</p>
        <button
          onClick={() => refetch()}
          className="text-blue-500 hover:text-blue-600 dark:text-blue-400 dark:hover:text-blue-300"
        >
          {t.common.retry}
        </button>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="text-center py-8">
        <p>{t.common.loading}</p>
      </div>
    )
  }

  if (!data?.pages?.length || !data.pages[0]?.items?.length) {
    return (
      <div className="text-center py-8">
        <p className="text-gray-600 dark:text-gray-400">No summaries found</p>
      </div>
    )
  }

  return (
    <>
      <UpdateNotification filters={filters} onRefresh={refetch} />

      <div className="space-y-4">
        {data.pages.map((page, i) => (
          <div key={i} className="space-y-4">
            {page.items.map((summary: Summary) => (
              <NewsCard key={summary.id} summary={summary} />
            ))}
          </div>
        ))}

        {isFetchingNextPage && (
          <div className="text-center py-4">
            <p>{t.common.loading}</p>
          </div>
        )}

        {showScrollTop && (
          <button
            onClick={scrollToTop}
            className="fixed bottom-4 right-4 p-3 bg-blue-600 dark:bg-blue-500 text-white rounded-full 
                     shadow-lg hover:bg-blue-700 dark:hover:bg-blue-600 transition-colors"
            aria-label="Scroll to top"
          >
            <svg
              className="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M5 10l7-7m0 0l7 7m-7-7v18"
              />
            </svg>
          </button>
        )}
      </div>
    </>
  )
} 