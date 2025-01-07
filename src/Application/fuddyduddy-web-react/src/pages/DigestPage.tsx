import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useLocalization } from '../hooks/useLocalization'
import { fetchDigestById } from '../api/digests'
import type { Digest } from '../types'

export default function DigestPage() {
  const { id } = useParams<{ id: string }>()
  const { t } = useLocalization()
  const [digest, setDigest] = useState<Digest | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

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
          <Link to="/" className="text-blue-600 dark:text-blue-400 hover:underline">
            {t.common.backToFeed}
          </Link>
        </div>
      </div>
    )
  }

  return (
    <>
      <Link
        to="/"
        className="inline-block mb-4 text-blue-600 dark:text-blue-400 hover:underline"
      >
        {t.common.backToFeed}
      </Link>

      <div className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6 space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800 dark:text-gray-200 mb-2">
            {digest.title}
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            {new Date(digest.generatedAt).toLocaleString()}
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
                  <a
                    href={ref.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="font-medium text-blue-600 dark:text-blue-400 hover:underline"
                  >
                    {ref.title}
                  </a>
                  {ref.reason && (
                    <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
                      {ref.reason}
                    </p>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </>
  )
} 