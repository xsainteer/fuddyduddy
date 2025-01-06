import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { fetchSummaryById } from '../api/summaries'
import NewsCard from '../components/NewsCard'

export default function SummaryPage() {
  const { id } = useParams<{ id: string }>()
  
  const { data: summary, isLoading, error } = useQuery({
    queryKey: ['summary', id],
    queryFn: () => fetchSummaryById(id!),
    enabled: !!id,
  })

  if (isLoading) {
    return (
      <div className="flex justify-center py-8">
        <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-current border-r-transparent dark:border-white" />
      </div>
    )
  }

  if (error || !summary) {
    return (
      <div className="text-center py-8">
        <p className="text-gray-500 dark:text-gray-400 mb-4">Summary not found</p>
        <Link 
          to="/"
          className="text-blue-500 hover:text-blue-600 dark:text-blue-400 dark:hover:text-blue-300"
        >
          ← Back to news feed
        </Link>
      </div>
    )
  }

  return (
    <div>
      <div className="mb-4">
        <Link 
          to="/"
          className="inline-flex items-center text-blue-500 hover:text-blue-600 dark:text-blue-400 dark:hover:text-blue-300"
        >
          <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          Back to news feed
        </Link>
      </div>
      <NewsCard summary={summary} isHighlighted={false} />
    </div>
  )
} 