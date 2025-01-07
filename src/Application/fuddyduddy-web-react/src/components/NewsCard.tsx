import { Link } from 'react-router-dom'
import { formatDateTime } from '../utils/dateFormat'
import ShareButton from './ShareButton'
import type { Summary } from '../types'

interface NewsCardProps {
  summary: Summary
  isHighlighted?: boolean
}

export default function NewsCard({ summary }: NewsCardProps) {
  return (
    <article className="p-4 rounded-xl transition-all duration-300 border dark:border-gray-800 bg-white dark:bg-gray-900 hover:bg-gray-50 dark:hover:bg-gray-800/50 dark:text-gray-100">
      {/* Header */}
      <div className="flex items-start justify-between mb-3">
        <div>
          <Link to={`/summary/${summary.id}`}>
            <h2 className="font-bold text-lg leading-tight mb-1 hover:underline">
              {summary.title}
            </h2>
          </Link>
          <div className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
            <span>{formatDateTime(summary.generatedAt)}</span>
            <span>·</span>
            <span className="text-blue-600 dark:text-blue-400">{summary.source}</span>
            <span>·</span>
            <a 
              href={summary.newsArticleUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="hover:text-blue-500 hover:underline"
            >
              Original source
            </a>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="mb-3">
        <p className="text-gray-600 dark:text-gray-300 whitespace-pre-wrap">
          {summary.article}
        </p>
      </div>

      {/* Category */}
      <div className="flex flex-wrap gap-1.5">
        <span 
          className="px-2 py-0.5 text-sm bg-blue-100 dark:bg-blue-900 
                   text-blue-600 dark:text-blue-300 rounded-full
                   transition-colors"
        >
          {summary.categoryLocal}
        </span>
      </div>

      {/* Engagement buttons */}
      <div className="flex items-center justify-between mt-4 pt-3 border-t dark:border-gray-800">
        <ShareButton title={summary.title} url={summary.id} />
        <a 
          href={summary.newsArticleUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="flex items-center gap-2 text-gray-500 dark:text-gray-400 hover:text-blue-500 dark:hover:text-blue-400"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
          </svg>
          <span className="text-sm">Read full article</span>
        </a>
      </div>
    </article>
  )
} 