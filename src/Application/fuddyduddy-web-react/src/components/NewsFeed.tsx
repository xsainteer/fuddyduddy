import { useInfiniteQuery, InfiniteData } from '@tanstack/react-query'
import { useInView } from 'react-intersection-observer'
import { useEffect } from 'react'
import NewsCard from './NewsCard'
import type { Summary, Filters } from '../types'

interface NewsFeedProps {
  filters: Filters
}

export default function NewsFeed({ filters }: NewsFeedProps) {
  const { ref, inView } = useInView()

  const {
    data,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    status,
  } = useInfiniteQuery<Summary[], Error, InfiniteData<Summary[]>, (string | Filters)[], number>({
    queryKey: ['summaries', filters],
    queryFn: async ({ pageParam }) => {
      const searchParams = new URLSearchParams({
        page: pageParam.toString(),
        pageSize: '20'
      })

      if (filters.language) searchParams.set('language', filters.language)
      if (filters.categoryId) searchParams.set('categoryId', filters.categoryId.toString())
      if (filters.sourceId) searchParams.set('sourceId', filters.sourceId)

      const response = await fetch(`/api/summaries?${searchParams}`)
      if (!response.ok) throw new Error('Network response was not ok')
      const data = await response.json()
      return data as Summary[]
    },
    getNextPageParam: (lastPage, pages) => lastPage.length === 20 ? pages.length : undefined,
    initialPageParam: 0
  })

  useEffect(() => {
    if (inView && hasNextPage && !isFetchingNextPage) {
      fetchNextPage()
    }
  }, [inView, hasNextPage, isFetchingNextPage, fetchNextPage])

  if (status === 'pending') return <div>Loading...</div>

  if (status === 'error') {
    return <div>Error: {error.message}</div>
  }

  const summaries = data?.pages.flat() || []

  if (summaries.length === 0) {
    return (
      <div className="text-center py-8 text-gray-500 dark:text-gray-400">
        No summaries found
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {summaries.map((summary: Summary) => (
        <NewsCard key={summary.id} summary={summary} />
      ))}
      <div ref={ref} className="h-4" />
      {isFetchingNextPage && (
        <div className="text-center py-4 text-gray-500 dark:text-gray-400">
          Loading more...
        </div>
      )}
    </div>
  )
} 