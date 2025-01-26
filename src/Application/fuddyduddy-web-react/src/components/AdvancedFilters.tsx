import { useState } from 'react'
import { Dialog } from '@headlessui/react'
import { ChevronDownIcon } from '@heroicons/react/24/outline'
import DatePicker from './DatePicker'
import type { Category, NewsSource } from '../types'
import { useLocalization } from '../hooks/useLocalization'
import { subDays } from 'date-fns'

interface AdvancedFiltersProps {
  filters: {
    fromDate: Date
    toDate: Date
    categoryIds: number[]
    sourceIds: number[]
  }
  onFiltersChange: (filters: any) => void
  limit: number
  onLimitChange: (limit: number) => void
  categories: Category[]
  sources: NewsSource[]
}

export default function AdvancedFilters({
  filters,
  onFiltersChange,
  limit,
  onLimitChange,
  categories,
  sources
}: AdvancedFiltersProps) {
  const { t, language } = useLocalization()
  const [isExpanded, setIsExpanded] = useState(false)
  const [filterModalType, setFilterModalType] = useState<'categories' | 'sources' | null>(null)

  const handleFilterSelection = (type: 'categories' | 'sources', id: number) => {
    const currentIds = type === 'categories' ? filters.categoryIds : filters.sourceIds
    const newIds = currentIds.includes(id)
      ? currentIds.filter(existingId => existingId !== id)
      : [...currentIds, id]
    
    onFiltersChange({
      ...filters,
      [type === 'categories' ? 'categoryIds' : 'sourceIds']: newIds
    })
  }

  const getSelectedItemsText = (type: 'categories' | 'sources') => {
    const selectedIds = type === 'categories' ? filters.categoryIds : filters.sourceIds
    const count = selectedIds.length
    if (count === 0) return type === 'categories' ? t.search.filters.categories.placeholder : t.search.filters.sources.placeholder
    return t.search.filters[type].selected.replace('{count}', count.toString())
  }

  const clearFilters = () => {
    onFiltersChange({
      fromDate: subDays(new Date(), 7),
      toDate: new Date(),
      categoryIds: [],
      sourceIds: []
    })
    onLimitChange(10)
  }

  const hasActiveFilters = filters.categoryIds.length > 0 || filters.sourceIds.length > 0 || limit !== 10

  return (
    <div className="sticky top-16 z-20 bg-white/95 dark:bg-gray-900/95 backdrop-blur-sm border border-gray-200 dark:border-gray-800 shadow-sm transition-all rounded-lg">
      {/* Collapsible header */}
      <div 
        className={`px-4 py-3 flex items-center justify-between cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-all duration-200 rounded-lg ${
          isExpanded ? 'bg-gray-50 dark:bg-gray-800/50 rounded-b-none' : ''
        }`}
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <div className="flex items-center gap-3 min-w-0">
          <div className="flex items-center gap-2 flex-shrink-0">
            <ChevronDownIcon 
              className={`w-5 h-5 text-gray-500 dark:text-gray-400 transition-transform duration-200 ${isExpanded ? 'rotate-180' : ''}`}
            />
            <span className="font-medium text-gray-800 dark:text-gray-200">{t.search.filters.advanced.title}</span>
          </div>
          {hasActiveFilters && (
            <div className="flex items-center gap-2 overflow-x-auto no-scrollbar flex-grow min-w-0 mask-edges">
              {filters.categoryIds.length > 0 && (
                <span className="inline-flex items-center px-2.5 py-1 rounded-full text-sm bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300 border border-blue-100 dark:border-blue-800 whitespace-nowrap">
                  {t.search.filters.categories.selected.replace('{count}', filters.categoryIds.length.toString())}
                </span>
              )}
              {filters.sourceIds.length > 0 && (
                <span className="inline-flex items-center px-2.5 py-1 rounded-full text-sm bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300 border border-blue-100 dark:border-blue-800 whitespace-nowrap">
                  {t.search.filters.sources.selected.replace('{count}', filters.sourceIds.length.toString())}
                </span>
              )}
              {limit !== 10 && (
                <span className="inline-flex items-center px-2.5 py-1 rounded-full text-sm bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300 border border-blue-100 dark:border-blue-800 whitespace-nowrap">
                  {t.search.filters.advanced.resultsLimit}: {limit}
                </span>
              )}
            </div>
          )}
        </div>
        {hasActiveFilters && (
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation()
              clearFilters()
            }}
            className="ml-4 px-2.5 py-1 text-sm text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-700/50 rounded-full transition-colors flex-shrink-0"
          >
            {t.search.filters.clear}
          </button>
        )}
      </div>

      {/* Collapsible content */}
      <div 
        className={`overflow-hidden transition-all duration-200 ${
          isExpanded ? 'max-h-[800px] border-t border-gray-200 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-800/25 rounded-b-lg' : 'max-h-0'
        }`}
      >
        <div className="p-4 space-y-6">
          {/* Date range pickers */}
          <div>
            <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">{t.search.filters.dateRange.title}</h3>
            <div className="flex flex-wrap gap-4">
              <DatePicker
                selected={filters.fromDate}
                onChange={(date) => onFiltersChange({ ...filters, fromDate: date })}
                label={t.search.filters.dateRange.from}
                minDate={new Date(2020, 0, 1)}
                maxDate={filters.toDate}
                language={language}
              />

              <DatePicker
                selected={filters.toDate}
                onChange={(date) => onFiltersChange({ ...filters, toDate: date })}
                label={t.search.filters.dateRange.to}
                minDate={filters.fromDate}
                maxDate={new Date()}
                language={language}
              />
            </div>
          </div>

          {/* Filter buttons */}
          <div>
            <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">{t.search.filters.categories.title}</h3>
            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                onClick={() => setFilterModalType('categories')}
                className="px-3.5 py-2 rounded-lg text-sm font-medium transition-all duration-200
                         text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800/50"
              >
                {getSelectedItemsText('categories')}
              </button>
            </div>
          </div>

          <div>
            <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">{t.search.filters.sources.title}</h3>
            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                onClick={() => setFilterModalType('sources')}
                className="px-3.5 py-2 rounded-lg text-sm font-medium transition-all duration-200
                         text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800/50"
              >
                {getSelectedItemsText('sources')}
              </button>
            </div>
          </div>

          {/* Results limit */}
          <div>
            <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">{t.search.filters.advanced.resultsLimit}</h3>
            <select
              value={limit}
              onChange={(e) => onLimitChange(Number(e.target.value))}
              className="p-2 rounded-lg border dark:border-gray-700 bg-white dark:bg-gray-800 
                       text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500 
                       focus:border-transparent outline-none appearance-none
                       bg-no-repeat bg-right pr-8"
              style={{
                backgroundImage: `url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M6 8l4 4 4-4'/%3e%3c/svg%3e")`,
                backgroundSize: '1.5em 1.5em'
              }}
            >
              {Array.from({ length: 10 }, (_, i) => (i + 1) * 10).map(num => (
                <option key={num} value={num}>{num}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Filter Modals */}
      <Dialog
        open={filterModalType !== null}
        onClose={() => setFilterModalType(null)}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-sm rounded-lg bg-white dark:bg-gray-800 p-6 w-full max-h-[90vh] flex flex-col">
            <Dialog.Title className="text-xl font-bold text-gray-800 dark:text-gray-200 mb-6">
              {filterModalType === 'categories' 
                ? t.search.filters.categories.modalTitle 
                : t.search.filters.sources.modalTitle}
            </Dialog.Title>

            <div className="space-y-2 overflow-y-auto flex-1 -mx-6 px-6">
              {filterModalType === 'categories' ? (
                categories.map(category => (
                  <button
                    key={category.id}
                    onClick={() => handleFilterSelection('categories', category.id)}
                    className={`block w-full text-left px-3 py-2 rounded-lg text-sm transition-colors ${
                      filters.categoryIds.includes(category.id)
                        ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300'
                        : 'text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800/50'
                    }`}
                  >
                    {language === 'RU' ? category.local : category.name}
                  </button>
                ))
              ) : (
                sources.map(source => (
                  <button
                    key={source.id}
                    onClick={() => handleFilterSelection('sources', source.id)}
                    className={`block w-full text-left px-3 py-2 rounded-lg text-sm transition-colors ${
                      filters.sourceIds.includes(source.id)
                        ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300'
                        : 'text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800/50'
                    }`}
                  >
                    {source.name}
                  </button>
                ))
              )}
            </div>

            <div className="mt-6 flex justify-end space-x-4">
              <button
                type="button"
                onClick={() => setFilterModalType(null)}
                className="px-4 py-2 text-gray-500 hover:text-gray-700 dark:text-gray-400 
                         dark:hover:text-gray-200"
              >
                {t.common.back}
              </button>
              <button
                type="button"
                onClick={() => setFilterModalType(null)}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                {t.search.filters.apply}
              </button>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </div>
  )
} 