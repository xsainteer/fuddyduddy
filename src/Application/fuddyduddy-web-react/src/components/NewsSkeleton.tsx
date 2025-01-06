export default function NewsSkeleton() {
  return (
    <div className="animate-pulse">
      <div className="p-4 rounded-xl border dark:border-gray-800 bg-white dark:bg-gray-900">
        {/* Header */}
        <div className="flex items-start justify-between mb-3">
          <div className="w-3/4">
            <div className="h-6 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
            <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-1/2"></div>
          </div>
        </div>

        {/* Content */}
        <div className="space-y-2 mb-3">
          <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded"></div>
          <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-5/6"></div>
          <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-4/6"></div>
        </div>

        {/* Tags */}
        <div className="flex gap-2 mb-4">
          <div className="h-6 w-16 bg-gray-100 dark:bg-gray-800 rounded-full"></div>
          <div className="h-6 w-20 bg-gray-100 dark:bg-gray-800 rounded-full"></div>
          <div className="h-6 w-16 bg-gray-100 dark:bg-gray-800 rounded-full"></div>
        </div>

        {/* Footer */}
        <div className="flex justify-between pt-3 border-t dark:border-gray-800">
          <div className="h-8 w-20 bg-gray-100 dark:bg-gray-800 rounded"></div>
          <div className="h-8 w-32 bg-gray-100 dark:bg-gray-800 rounded"></div>
        </div>
      </div>
    </div>
  )
} 