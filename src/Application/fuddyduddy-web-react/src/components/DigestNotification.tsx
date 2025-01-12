import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { useLocalization } from '../hooks/useLocalization'
import { fetchLatestDigests, digestKeys } from '../api/digests'
import type { Filters } from '../types'

interface DigestNotificationProps {
  filters: Filters
}

const POLLING_INTERVAL = 60000 // 1 minute
const LAST_CHECKED_KEY = 'lastCheckedDigestTimestamp'

export default function DigestNotification({ filters }: DigestNotificationProps) {
  const { t } = useLocalization()
  const navigate = useNavigate()
  const [hasNewDigest, setHasNewDigest] = useState(false)
  const [isVisible, setIsVisible] = useState(false)

  const { data: digests } = useQuery({
    queryKey: digestKeys.latest(filters.language || 'RU', 1),
    queryFn: () => fetchLatestDigests(filters.language || 'RU', 1),
    refetchInterval: POLLING_INTERVAL,
    staleTime: POLLING_INTERVAL / 2,
  })

  useEffect(() => {
    if (digests?.length) {
      const lastChecked = parseInt(localStorage.getItem(LAST_CHECKED_KEY) || '0')
      const latestDigestTime = new Date(digests[0].generatedAt).getTime()
      
      if (latestDigestTime > lastChecked) {
        setHasNewDigest(true)
        setIsVisible(true)
      }
    }
  }, [digests])

  const handleClick = () => {
    if (hasNewDigest) {
      const now = new Date().getTime()
      localStorage.setItem(LAST_CHECKED_KEY, now.toString())
      setHasNewDigest(false)
      setIsVisible(false)
      navigate('/digests')
    }
  }

  const handleClose = (e: React.MouseEvent) => {
    e.stopPropagation()
    setIsVisible(false)
  }

  if (!isVisible || !hasNewDigest) return null

  return (
    <div 
      onClick={handleClick}
      className="fixed bottom-20 left-4 right-4 md:hidden bg-blue-600 text-white rounded-lg shadow-lg p-4 cursor-pointer animate-slide-up z-40"
    >
      <div className="flex items-center justify-between">
        <span className="font-medium">{t.digests.newDigestAvailable}</span>
        <button
          onClick={handleClose}
          className="p-1 hover:bg-blue-700 rounded-full transition-colors"
          aria-label="Close notification"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
    </div>
  )
} 