import { useEffect, useState } from 'react'
import { useParams, Link, useNavigate } from 'react-router-dom'
import { useLocalization } from '../hooks/useLocalization'
import { useLayout } from '../contexts/LayoutContext'
import { fetchDigestById } from '../api/digests'
import type { Digest } from '../types'
import { formatDateTime } from '../utils/dateFormat'

export default function DigestPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { t, language: interfaceLanguage } = useLocalization()
  const { setShowSidePanels } = useLayout()
  const [digest, setDigest] = useState<Digest | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Hide side panels when component mounts, show them when unmounts
  useEffect(() => {
    setShowSidePanels(false)
    return () => setShowSidePanels(true)
  }, [setShowSidePanels])

  useEffect(() => {
    const loadDigest = async () => {
      if (!id) return

      try {
        setLoading(true)
        setError(null)
        const data = await fetchDigestById(id)
        setDigest(data)
      } catch (err) {
        setError(t.errors.failedToLoadDigests)
        console.error('Failed to load digest:', err)
      } finally {
        setLoading(false)
      }
    }

    loadDigest()
  }, [id, t.errors.failedToLoadDigests])

  if (loading) {
    return (
      <div className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
        <div className="animate-pulse space-y-4">
          <div className="h-8 bg-gray-200 dark:bg-gray-800 rounded w-3/4" />
          <div className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-1/4 mb-8" />
          <div className="space-y-2">
            {[...Array(3)].map((_, i) => (
              <div key={i} className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-full" />
            ))}
          </div>
        </div>
      </div>
    )
  }

  if (error || !digest) {
    return (
      <div className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
        <div className="text-center py-8">
          <p className="text-red-500 dark:text-red-400 mb-4">{error || t.common.error}</p>
          <button
            onClick={() => navigate(-1)}
            className="text-blue-600 dark:text-blue-400 hover:underline"
          >
            {t.common.backToFeed}
          </button>
        </div>
      </div>
    )
  }

  return (
    <>
      <div className="flex items-center justify-between mb-4">
        <button
          onClick={() => navigate(-1)}
          className="text-blue-600 dark:text-blue-400 hover:underline"
        >
          ← {t.common.back}
        </button>
        <Link
          to={`/${interfaceLanguage.toLowerCase()}/feed`}
          className="text-blue-600 dark:text-blue-400 hover:underline"
        >
          {t.common.backToFeed}
        </Link>
      </div>

      <div className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6 space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800 dark:text-gray-200 mb-2">
            {digest.title}
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            {formatDateTime(digest.generatedAt, interfaceLanguage)}
          </p>
        </div>

        <div className="text-gray-800 dark:text-gray-200 space-y-4">
          {digest.content.split('\n').map((paragraph, i) => (
            <p key={i} className="leading-relaxed">{paragraph}</p>
          ))}
        </div>

        {digest.references.length > 0 && (
          <div>
            <h2 className="text-xl font-bold text-gray-800 dark:text-gray-200 mb-4">
              {t.common.originalSource}
            </h2>
            <div className="space-y-3">
              {digest.references.map((ref, i) => (
                <div key={i} className="p-4 rounded-lg border dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50">
                  <div className="flex flex-col gap-2">
                    <a
                      href={ref.url}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="font-medium text-blue-600 dark:text-blue-400 hover:underline"
                    >
                      {ref.title}
                    </a>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-medium bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-gray-200 rounded-full">
                        {ref.category}
                      </span>
                      <span className="px-2 py-0.5 text-xs font-medium bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-gray-200 rounded-full">
                        {ref.source}
                      </span>
                    </div>
                    {ref.reason && (
                      <p className="text-sm text-gray-600 dark:text-gray-400">
                        {ref.reason}
                      </p>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </>
  )
} 