import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { formatDateTime } from '../utils/dateFormat'
import ShareButton from './ShareButton'
import type { Summary, SimilarSummary } from '../types'
import { useLocalization } from '../hooks/useLocalization'
import { loadSimilarities } from '../api/summaries'

const truncateTitle = (title: string, maxLength: number = 100): string => {
  if (title.length <= maxLength) return title;
  return title.slice(0, maxLength).trim() + '...'
}

interface NewsCardProps {
  summary: Summary
  isHighlighted?: boolean
  score?: number
}

export default function NewsCard({ summary, score }: NewsCardProps) {
  const { t, language } = useLocalization()
  const [isExpanded, setIsExpanded] = useState(false)

  // Split article into lines and determine if it needs expansion
  const lines = summary.article.split('\n')
  const hasMoreLines = lines.length > 10
  const displayedLines = isExpanded ? lines : lines.slice(0, 10)


  // Get similarities to display
  const sortedSimilarities = summary.similarities?.sort(
    (a, b) => new Date(b.generatedAt).getTime() - new Date(a.generatedAt).getTime()
  ) || []
  
  const [offset, setOffset] = useState(0);

  const [displayedSimilarities, setDisplayedSimilarities] = useState(sortedSimilarities);
  
  const [preloadedSimilarities, setPreloadedSimilarities] = useState<SimilarSummary[]>([]);
  
  const [hasMoreSimilarities, setHasMoreSimilarities] = useState(false);
  
  async function requestMoreSimilarities() {
    const newSimilarities = await loadSimilarities(summary.id, offset.toString());
    if(newSimilarities.length > 0)
    {
      setPreloadedSimilarities(newSimilarities);
      
      setOffset(prevOffset => prevOffset + 3);
      
      setHasMoreSimilarities(preloadedSimilarities.length > 0);
      
      return true;
    }
    else
    {
      return false;
    }
  }
    
  async function loadMoreSimilarities() {
    setDisplayedSimilarities([
        ...displayedSimilarities,
        ...preloadedSimilarities
    ]);
    
    const areSummariesLeft = await requestMoreSimilarities();
    if(!areSummariesLeft)
    {
      setHasMoreSimilarities(false);
    }
  }
  
  useEffect(() => {
    const fetchSimilarities = async () => {
      await requestMoreSimilarities();
    }
    
    fetchSimilarities();
  }, [summary.id]);
  
  return (
    <article className="p-4 rounded-xl transition-all duration-300 border dark:border-gray-800 bg-white dark:bg-gray-900 hover:bg-gray-50 dark:hover:bg-gray-800/50 dark:text-gray-100">
      {/* Header */}
      <div className="flex items-start justify-between mb-3">
        <div>
          <Link to={`/${language.toLowerCase()}/summary/${summary.id}`}>
            <h2 className="font-bold text-lg leading-tight mb-1 hover:underline">
              {summary.title}
            </h2>
          </Link>
          <div className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
            <span>{formatDateTime(summary.generatedAt, language)}</span>
            <span>·</span>
            <span className="text-blue-600 dark:text-blue-400">{summary.source}</span>
            <span>·</span>
            <a 
              href={summary.newsArticleUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="hover:text-blue-500 hover:underline"
            >
              {t.common.originalSource}
            </a>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="mb-3">
        <div className="text-gray-600 dark:text-gray-300 whitespace-pre-line">
          {displayedLines.join('\n')}
          {hasMoreLines && !isExpanded && (
            <div className="mt-2">
              <button
                onClick={() => setIsExpanded(true)}
                className="text-blue-600 dark:text-blue-400 hover:underline text-sm"
              >
                {t.common.readMore}
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Similarities */}
      {summary.similarities && summary.similarities.length > 0 && (
        <div className="mt-3 pt-3 border-t dark:border-gray-800">
          <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-1.5 flex items-center gap-2">
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
            {t.common.similarSummaries}
          </h3>
          <div className="space-y-1.5">
            {displayedSimilarities.map(similar => (
              <div 
                key={similar.id} 
                className="hover:bg-gray-50 dark:hover:bg-gray-800/50 p-1.5 -mx-2 rounded-lg transition-colors"
              >
                <Link 
                  to={`/${language.toLowerCase()}/summary/${similar.newsSummaryId}`}
                  className="block text-sm text-gray-900 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400"
                >
                  {truncateTitle(similar.title)}
                </Link>
                <div className="flex items-center gap-2 mt-0.5 text-xs text-gray-500 dark:text-gray-400">
                  <span className="text-blue-600 dark:text-blue-400">{similar.source}</span>
                  <span>·</span>
                  <span>{formatDateTime(similar.generatedAt, language)}</span>
                </div>
              </div>
            ))}
            {hasMoreSimilarities && (
              <button
                onClick={async () => await loadMoreSimilarities()}
                className="text-blue-600 dark:text-blue-400 hover:underline text-xs mt-1"
              >
                {t.common.showMore}
              </button>
            )}
          </div>
        </div>
      )}

      {/* Category and Score */}
      <div className="flex flex-wrap items-center gap-1.5 mt-3">
        <span 
          className="px-2 py-0.5 text-sm bg-blue-100 dark:bg-blue-900 
                   text-blue-600 dark:text-blue-300 rounded-full
                   transition-colors"
        >
          {language === 'RU' ? summary.categoryLocal : summary.category}
        </span>
        {score !== undefined && (
          <span className="text-xs text-gray-500 dark:text-gray-400">
            {score.toFixed(3)} {t.search.sort.score.toLowerCase()}
          </span>
        )}
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
          <span className="text-sm">{t.common.readMore}</span>
        </a>
      </div>
    </article>
  )
} 