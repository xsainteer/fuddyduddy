import { useEffect, useState } from 'react'
import { useLocalization } from '../hooks/useLocalization'
import type { Filters } from '../types'

interface UpdateNotificationProps {
  filters: Filters
  onRefresh: () => void
}

export default function UpdateNotification({ filters, onRefresh }: UpdateNotificationProps) {
  const { t } = useLocalization()
  const [hasNewSummaries, setHasNewSummaries] = useState(false)

  useEffect(() => {
    const checkNewSummaries = async () => {
      try {
        const searchParams = new URLSearchParams()
        searchParams.append('page', '0')
        if (filters.categoryId) searchParams.append('categoryId', filters.categoryId.toString())
        if (filters.sourceId) searchParams.append('sourceId', filters.sourceId.toString())
        if (filters.language) searchParams.append('language', filters.language)

        const response = await fetch(`/api/summaries?${searchParams.toString()}`)
        if (!response.ok) return
        
        const summaries = await response.json()
        const latestSummaryId = localStorage.getItem('latestSummaryId')
        
        if (latestSummaryId && summaries[0]?.id !== latestSummaryId) {
          setHasNewSummaries(true)
        }
      } catch (error) {
        console.error('Error checking for new summaries:', error)
      }
    }

    const interval = setInterval(checkNewSummaries, 60000) // Check every minute
    return () => clearInterval(interval)
  }, [filters])

  const handleRefresh = () => {
    setHasNewSummaries(false)
    onRefresh()
  }

  if (!hasNewSummaries) return null

  return (
    <div className="fixed top-0 left-0 right-0 z-50 animate-slide-down">
      <div className="max-w-7xl mx-auto px-4">
        <button
          onClick={handleRefresh}
          className="w-full py-3 px-4 bg-blue-600 text-white dark:bg-blue-500 
                   rounded-b-lg shadow-lg hover:bg-blue-700 dark:hover:bg-blue-600 
                   transition-colors text-center font-medium"
        >
          {t.common.newSummariesAvailable}
        </button>
      </div>
    </div>
  )
} 