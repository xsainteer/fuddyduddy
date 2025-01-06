import { useInfiniteQuery, InfiniteData } from '@tanstack/react-query'
import { useIntersection } from '@mantine/hooks'
import { useEffect } from 'react'
import { toast } from 'react-hot-toast'
import { fetchSummaries } from '../api/summaries'
import NewsCard from './NewsCard'
import type { Summary } from '../types'

interface PageData {
  items: Summary[]
  nextPage: number | undefined
  prevPage: number | undefined
}

export default function NewsFeed() {
  // Create two intersection observers - one for top and one for bottom
  const { ref: topRef, entry: topEntry } = useIntersection({
    root: null,
    threshold: 0,
  })

  const { ref: bottomRef, entry: bottomEntry } = useIntersection({
    root: null,
    threshold: 0,
  })

  const {
    data,
    fetchNextPage,
    fetchPreviousPage,
    isFetchingNextPage,
    isFetchingPreviousPage,
    hasNextPage,
    hasPreviousPage,
  } = useInfiniteQuery<PageData, Error, InfiniteData<PageData>, string[], number>({
    queryKey: ['summaries'],
    queryFn: async ({ pageParam }) => {
      const page = pageParam ?? 0
      const data = await fetchSummaries(page)
      return {
        items: data,
        nextPage: data.length === 20 ? page + 1 : undefined,
        prevPage: page > 0 ? page - 1 : undefined,
      }
    },
    getNextPageParam: (lastPage) => lastPage.nextPage,
    getPreviousPageParam: (firstPage) => firstPage.prevPage,
    initialPageParam: 0,
    retry: false,
    staleTime: 1000 * 60
  })

  // Handle bottom scroll
  useEffect(() => {
    if (bottomEntry?.isIntersecting && hasNextPage && !isFetchingNextPage) {
      fetchNextPage()
    }
  }, [bottomEntry, hasNextPage, isFetchingNextPage, fetchNextPage])

  // Handle top scroll
  useEffect(() => {
    if (topEntry?.isIntersecting && hasPreviousPage && !isFetchingPreviousPage) {
      fetchPreviousPage()
    }
  }, [topEntry, hasPreviousPage, isFetchingPreviousPage, fetchPreviousPage])

  return (
    <div className="space-y-3">
      {/* Loading indicator for previous page */}
      {isFetchingPreviousPage && (
        <div className="text-center py-4">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-current border-r-transparent dark:border-white motion-reduce:animate-[spin_1.5s_linear_infinite]" />
        </div>
      )}

      {data?.pages.map((page: PageData, i: number) => (
        <div key={i} className="space-y-3">
          {page.items.map((summary: Summary, index: number) => (
            <div
              key={summary.id}
              ref={(node) => {
                // Add refs for first and last items of each page
                if (i === 0 && index === 0) topRef(node)
                if (i === data.pages.length - 1 && index === page.items.length - 1) bottomRef(node)
              }}
              className="transform transition-all duration-200 hover:-translate-y-0.5"
            >
              <NewsCard 
                summary={summary} 
                isHighlighted={false}
              />
            </div>
          ))}
        </div>
      ))}

      {/* Loading indicator for next page */}
      {isFetchingNextPage && (
        <div className="text-center py-4">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-current border-r-transparent dark:border-white motion-reduce:animate-[spin_1.5s_linear_infinite]" />
        </div>
      )}

      {!hasNextPage && (
        <div className="text-center py-4 text-gray-500 dark:text-gray-400">
          No more news to load
        </div>
      )}
    </div>
  )
} 