import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import type { Digest, Filters } from '../types'
import { useLocalization } from '../hooks/useLocalization'
import { fetchLatestDigests } from '../api/digests'
import DigestSkeleton from './DigestSkeleton'

interface DigestsProps {
  className?: string
  filters?: Filters
}

function getContentPreview(content: string): string {
  const lines = content.split('\n').filter(line => line.trim())
  const previewLines = lines.slice(0, 3).map(line => {
    if (line.length > 120) {
      return line.slice(0, 120) + '...'
    }
    return line
  })
  
  if (lines.length > 3) {
    return previewLines.join('\n') + '...'
  }
  return previewLines.join('\n')
}

export default function Digests({ className = '', filters = {} }: DigestsProps) {
  const { t } = useLocalization()
  const [digests, setDigests] = useState<Digest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadDigests = async () => {
      try {
        setLoading(true)
        setError(null)
        const data = await fetchLatestDigests(filters.language || 'RU', 5)
        setDigests(data)
      } catch (err) {
        setError(t.errors.failedToLoadDigests)
        console.error('Failed to load digests:', err)
      } finally {
        setLoading(false)
      }
    }

    loadDigests()
  }, [filters.language, t.errors.failedToLoadDigests])

  return (
    <div className={`p-4 space-y-4 ${className}`}>
      <h2 className="text-xl font-bold text-gray-800 dark:text-gray-200">
        {t.digests.title}
      </h2>

      {loading ? (
        <DigestSkeleton />
      ) : error ? (
        <p className="text-red-500 dark:text-red-400">{error}</p>
      ) : (
        <>
          <div className="space-y-3">
            {digests.map(digest => (
              <Link
                key={digest.id}
                to={`/digests/${digest.id}`}
                className="block p-3 rounded-lg border dark:border-gray-800 bg-white dark:bg-gray-900 hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer transition-colors"
              >
                <h3 className="font-medium text-gray-800 dark:text-gray-200 mb-1">
                  {digest.title}
                </h3>
                <div className="text-sm text-gray-600 dark:text-gray-400 mb-2 whitespace-pre-line line-clamp-3">
                  {getContentPreview(digest.content)}
                </div>
                <p className="text-sm text-gray-500 dark:text-gray-400">
                  {new Date(digest.generatedAt).toLocaleString()}
                </p>
              </Link>
            ))}
          </div>

          {digests.length > 0 && (
            <Link
              to="/digests"
              className="block text-sm text-blue-600 dark:text-blue-400 hover:underline"
            >
              {t.digests.viewAll}
            </Link>
          )}
        </>
      )}

      <p className="text-sm text-gray-500 dark:text-gray-400 italic">
        {t.digests.generatedEveryHour}
      </p>
    </div>
  )
} 