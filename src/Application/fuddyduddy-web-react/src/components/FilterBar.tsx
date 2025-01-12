import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import type { Category, NewsSource, Filters as FiltersType } from '../types'
import { useLocalization } from '../hooks/useLocalization'

interface FilterBarProps {
  filters: FiltersType
  onFiltersChange: (filters: FiltersType) => void
}

export default function FilterBar({ filters, onFiltersChange }: FilterBarProps) {
  const { t, language: interfaceLanguage } = useLocalization()
  const [isExpanded, setIsExpanded] = useState(false)

  const { data: categories = [] } = useQuery<Category[]>({
    queryKey: ['categories'],
    queryFn: () => fetch('/api/filters/categories').then(res => res.json())
  })

  const { data: sources = [] } = useQuery<NewsSource[]>({
    queryKey: ['sources'],
    queryFn: () => fetch('/api/filters/sources').then(res => res.json())
  })

  const handleCategoryChange = (categoryId: number) => {
    onFiltersChange({
      ...filters,
      categoryId: filters.categoryId === categoryId ? undefined : categoryId
    })
  }

  const handleSourceChange = (sourceId: number) => {
    onFiltersChange({
      ...filters,
      sourceId: filters.sourceId === sourceId ? undefined : sourceId
    })
  }

  const clearFilters = () => {
    onFiltersChange({})
    setIsExpanded(false)
  }

  const activeCategory = categories.find(c => c.id === filters.categoryId)
  const activeSource = sources.find(s => s.id === filters.sourceId)
  const hasActiveFilters = !!(activeCategory || activeSource)

  return (
    <div className="sticky top-16 z-20 bg-white/95 dark:bg-gray-900/95 backdrop-blur-sm border border-gray-200 dark:border-gray-800 shadow-sm transition-all rounded-lg mb-4">
      {/* Collapsed view */}
      <div 
        className={`px-4 py-3 flex items-center justify-between cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-all duration-200 rounded-lg ${
          isExpanded ? 'bg-gray-50 dark:bg-gray-800/50 rounded-b-none' : ''
        }`}
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <div className="flex items-center gap-3 min-w-0">
          <div className="flex items-center gap-2 flex-shrink-0">
            <svg 
              className={`w-5 h-5 text-gray-500 dark:text-gray-400 transition-transform duration-200 ${isExpanded ? 'rotate-180' : ''}`}
              fill="none" 
              stroke="currentColor" 
              viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
            <span className="font-medium text-gray-800 dark:text-gray-200">{t.filters.title}</span>
          </div>
          {hasActiveFilters && (
            <div className="flex items-center gap-2 overflow-x-auto no-scrollbar flex-grow min-w-0 mask-edges">
              {activeCategory && (
                <span className="inline-flex items-center px-2.5 py-1 rounded-full text-sm bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300 border border-blue-100 dark:border-blue-800 whitespace-nowrap">
                  {interfaceLanguage === 'RU' ? activeCategory.local : activeCategory.name}
                </span>
              )}
              {activeSource && (
                <span className="inline-flex items-center px-2.5 py-1 rounded-full text-sm bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300 border border-blue-100 dark:border-blue-800 whitespace-nowrap">
                  {activeSource.name}
                </span>
              )}
            </div>
          )}
        </div>
        {hasActiveFilters && (
          <button
            onClick={(e) => {
              e.stopPropagation()
              clearFilters()
            }}
            className="ml-4 px-2.5 py-1 text-sm text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-700/50 rounded-full transition-colors flex-shrink-0"
          >
            {t.filters.clearAll}
          </button>
        )}
      </div>

      {/* Expanded view */}
      <div 
        className={`overflow-hidden transition-all duration-200 ${
          isExpanded ? 'max-h-[800px] border-t border-gray-200 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-800/25 rounded-b-lg' : 'max-h-0'
        }`}
      >
        <div className="p-4 space-y-6">
          {/* Categories */}
          <div>
            <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">{t.filters.categories}</h3>
            <div className="flex flex-wrap gap-2">
              {categories.map(category => (
                <button
                  key={category.id}
                  onClick={() => handleCategoryChange(category.id)}
                  className={`px-3.5 py-2 rounded-lg text-sm font-medium transition-all duration-200 ${
                    filters.categoryId === category.id
                      ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300 shadow-sm'
                      : 'text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800/50'
                  }`}
                >
                  {interfaceLanguage === 'RU' ? category.local : category.name}
                </button>
              ))}
            </div>
          </div>

          {/* Sources */}
          <div>
            <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">{t.filters.sources}</h3>
            <div className="flex flex-col gap-2">
              {sources.map(source => (
                <button
                  key={source.id}
                  onClick={() => handleSourceChange(source.id)}
                  className={`px-3.5 py-2 rounded-lg text-sm font-medium transition-all duration-200 text-left ${
                    filters.sourceId === source.id
                      ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300 shadow-sm'
                      : 'text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800/50'
                  }`}
                >
                  {source.name}
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
} 