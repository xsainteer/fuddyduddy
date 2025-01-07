import { useQuery } from '@tanstack/react-query'
import type { Category, NewsSource, Language, Filters as FiltersType } from '../types'
import { useLocalization } from '../hooks/useLocalization'

interface FiltersProps {
  filters: FiltersType
  onFiltersChange: (filters: FiltersType) => void
}

export default function Filters({ filters, onFiltersChange }: FiltersProps) {
  const { t, language: interfaceLanguage } = useLocalization()
  
  const { data: categories = [] } = useQuery<Category[]>({
    queryKey: ['categories'],
    queryFn: () => fetch('/api/filters/categories').then(res => res.json())
  })

  const { data: sources = [] } = useQuery<NewsSource[]>({
    queryKey: ['sources'],
    queryFn: () => fetch('/api/filters/sources').then(res => res.json())
  })

  const { data: languages = [] } = useQuery<Language[]>({
    queryKey: ['languages'],
    queryFn: () => fetch('/api/filters/languages').then(res => res.json())
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

  const handleLanguageChange = (language: string) => {
    onFiltersChange({
      ...filters,
      language: filters.language === language ? undefined : language
    })
  }

  const clearFilters = () => {
    onFiltersChange({})
  }

  return (
    <div className="p-4 space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-bold text-gray-800 dark:text-gray-200">{t.filters.title}</h2>
        {(filters.categoryId || filters.sourceId || filters.language) && (
          <button
            onClick={clearFilters}
            className="text-sm text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
          >
            {t.filters.clearAll}
          </button>
        )}
      </div>

      {/* Language */}
      <div>
        <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">{t.filters.language}</h3>
        <div className="space-y-2">
          {languages.map(lang => (
            <button
              key={lang.id}
              onClick={() => handleLanguageChange(lang.id)}
              className={`block w-full text-left px-3 py-2 rounded-lg text-sm transition-colors ${
                filters.language === lang.id
                  ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300'
                  : 'text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800/50'
              }`}
            >
              {interfaceLanguage === 'RU' ? lang.local : lang.name}
            </button>
          ))}
        </div>
      </div>

      {/* Categories */}
      <div>
        <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">{t.filters.categories}</h3>
        <div className="space-y-2">
          {categories.map(category => (
            <button
              key={category.id}
              onClick={() => handleCategoryChange(category.id)}
              className={`block w-full text-left px-3 py-2 rounded-lg text-sm transition-colors ${
                filters.categoryId === category.id
                  ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300'
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
        <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">{t.filters.sources}</h3>
        <div className="space-y-2">
          {sources.map(source => (
            <button
              key={source.id}
              onClick={() => handleSourceChange(source.id)}
              className={`block w-full text-left px-3 py-2 rounded-lg text-sm transition-colors ${
                filters.sourceId === source.id
                  ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300'
                  : 'text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800/50'
              }`}
            >
              {source.name}
            </button>
          ))}
        </div>
      </div>
    </div>
  )
} 