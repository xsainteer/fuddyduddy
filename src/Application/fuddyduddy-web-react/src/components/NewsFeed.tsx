import { useInfiniteQuery, InfiniteData, useQueryClient } from '@tanstack/react-query'
import { useIntersection } from '@mantine/hooks'
import { useEffect, useState } from 'react'
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
  const queryClient = useQueryClient()
  const [hasNewSummaries, setHasNewSummaries] = useState(false)

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
    refetch
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

  // Check for new summaries every minute
  useEffect(() => {
    const checkNewSummaries = async () => {
      try {
        const latestData = await fetchSummaries(0)
        const currentFirstId = data?.pages[0]?.items[0]?.id
        
        if (currentFirstId && latestData[0]?.id !== currentFirstId) {
          setHasNewSummaries(true)
        }
      } catch (error) {
        console.error('Error checking for new summaries:', error)
      }
    }

    const interval = setInterval(checkNewSummaries, 60000) // Check every minute
    return () => clearInterval(interval)
  }, [data?.pages])

  // Handle refresh click
  const handleRefresh = async () => {
    setHasNewSummaries(false)
    await queryClient.resetQueries({ queryKey: ['summaries'] })
    await refetch()
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

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
      {/* New summaries notification */}
      {hasNewSummaries && (
        <button
          onClick={handleRefresh}
          className="fixed top-[4.5rem] left-1/2 -translate-x-1/2 px-3 py-1.5 
            bg-gray-900/90 dark:bg-gray-800/90 backdrop-blur-sm
            text-white
            rounded-full border border-gray-700/50
            shadow-lg hover:bg-gray-800/90 dark:hover:bg-gray-700/90 
            transition-all duration-300
            flex items-center gap-2 group
            z-50"
        >
          <svg 
            className="w-3.5 h-3.5 transition-transform group-hover:rotate-180 duration-500" 
            fill="none" 
            stroke="currentColor" 
            viewBox="0 0 24 24"
          >
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          <span className="text-xs font-medium">New summaries available</span>
        </button>
      )}

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