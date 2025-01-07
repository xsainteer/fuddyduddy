import type { DigestSummary } from '../types'

interface DigestsProps {
  className?: string
}

export default function Digests({ className = '' }: DigestsProps) {
  // Placeholder data until we implement the digests feature
  const placeholderDigests: DigestSummary[] = [
    {
      id: '1',
      title: 'Morning Digest',
      generatedAt: new Date().toISOString()
    },
    {
      id: '2',
      title: 'Afternoon Digest',
      generatedAt: new Date(Date.now() - 3600000).toISOString()
    }
  ]

  return (
    <div className={`p-4 space-y-4 ${className}`}>
      <h2 className="text-xl font-bold text-gray-800 dark:text-gray-200">Latest Digests</h2>
      <div className="space-y-3">
        {placeholderDigests.map(digest => (
          <div
            key={digest.id}
            className="p-3 rounded-lg border dark:border-gray-800 bg-white dark:bg-gray-900 hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer transition-colors"
          >
            <h3 className="font-medium text-gray-800 dark:text-gray-200">{digest.title}</h3>
            <p className="text-sm text-gray-500 dark:text-gray-400">
              {new Date(digest.generatedAt).toLocaleTimeString()}
            </p>
          </div>
        ))}
      </div>
      <p className="text-sm text-gray-500 dark:text-gray-400 italic">
        Digests are generated every hour
      </p>
    </div>
  )
} 