import { useQuery } from '@tanstack/react-query'
import type { Category, NewsSource, Language, Filters } from '../types'

interface FiltersProps {
  filters: Filters
  onFiltersChange: (filters: Filters) => void
}

export default function Filters({ filters, onFiltersChange }: FiltersProps) {
  const { data: categories } = useQuery<Category[]>({
    queryKey: ['categories'],
    queryFn: async () => {
      const response = await fetch('/api/filters/categories')
      if (!response.ok) throw new Error('Failed to fetch categories')
      return response.json()
    }
  })

  const { data: sources } = useQuery<NewsSource[]>({
    queryKey: ['sources'],
    queryFn: async () => {
      const response = await fetch('/api/filters/sources')
      if (!response.ok) throw new Error('Failed to fetch sources')
      return response.json()
    }
  })

  const { data: languages } = useQuery<Language[]>({
    queryKey: ['languages'],
    queryFn: async () => {
      const response = await fetch('/api/filters/languages')
      if (!response.ok) throw new Error('Failed to fetch languages')
      return response.json()
    }
  })

  return (
    <div className="p-4 space-y-8">
      <div>
        <h3 className="text-base font-semibold mb-3 text-gray-900 dark:text-white">Language</h3>
        <div className="space-y-1">
          {languages?.map(lang => (
            <button
              key={lang.id}
              onClick={() => onFiltersChange({ ...filters, language: lang.id })}
              className={`w-full text-left px-2.5 py-1.5 rounded-md text-sm transition-colors ${
                filters.language === lang.id
                  ? 'bg-blue-50 dark:bg-blue-900/50 text-blue-700 dark:text-blue-200 font-medium'
                  : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800'
              }`}
            >
              {lang.name}
            </button>
          ))}
        </div>
      </div>

      <div>
        <h3 className="text-base font-semibold mb-3 text-gray-900 dark:text-white">Categories</h3>
        <div className="space-y-1">
          {categories?.map(category => (
            <button
              key={category.id}
              onClick={() => onFiltersChange({ ...filters, categoryId: category.id })}
              className={`w-full text-left px-2.5 py-1.5 rounded-md text-sm transition-colors ${
                filters.categoryId === category.id
                  ? 'bg-blue-50 dark:bg-blue-900/50 text-blue-700 dark:text-blue-200 font-medium'
                  : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800'
              }`}
            >
              {category.local}
            </button>
          ))}
        </div>
      </div>

      <div>
        <h3 className="text-base font-semibold mb-3 text-gray-900 dark:text-white">Sources</h3>
        <div className="space-y-1">
          {sources?.map(source => (
            <button
              key={source.id}
              onClick={() => onFiltersChange({ ...filters, sourceId: source.id })}
              className={`w-full text-left px-2.5 py-1.5 rounded-md text-sm transition-colors ${
                filters.sourceId === source.id
                  ? 'bg-blue-50 dark:bg-blue-900/50 text-blue-700 dark:text-blue-200 font-medium'
                  : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800'
              }`}
            >
              {source.name}
            </button>
          ))}
        </div>
      </div>

      {(filters.language || filters.categoryId || filters.sourceId) && (
        <button
          onClick={() => onFiltersChange({})}
          className="w-full px-2.5 py-1.5 text-sm text-red-600 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300 transition-colors"
        >
          Clear filters
        </button>
      )}
    </div>
  )
} 