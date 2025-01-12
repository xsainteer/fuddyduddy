import { useParams, Link, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { useEffect } from 'react'
import { fetchSummaryById } from '../api/summaries'
import NewsCard from '../components/NewsCard'
import { useLocalization } from '../hooks/useLocalization'
import { useLayout } from '../contexts/LayoutContext'

export default function SummaryPage() {
  const { id } = useParams<{ id: string }>()
  const { t, language } = useLocalization()
  const navigate = useNavigate()
  const { setShowSidePanels } = useLayout()
  
  // Hide side panels when component mounts, show them when unmounts
  useEffect(() => {
    setShowSidePanels(false)
    return () => setShowSidePanels(true)
  }, [setShowSidePanels])

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
        <p className="text-gray-500 dark:text-gray-400 mb-4">{t.common.error}</p>
        <Link 
          to={`/${language.toLowerCase()}/feed`}
          className="text-blue-500 hover:text-blue-600 dark:text-blue-400 dark:hover:text-blue-300"
        >
          ← {t.common.backToFeed}
        </Link>
      </div>
    )
  }

  return (
    <>
      <div className="mb-4">
        <button
          onClick={() => navigate(-1)}
          className="inline-flex items-center text-blue-500 hover:text-blue-600 dark:text-blue-400 dark:hover:text-blue-300"
        >
          <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          {t.common.back}
        </button>
      </div>
      <NewsCard summary={summary} />
    </>
  )
} 