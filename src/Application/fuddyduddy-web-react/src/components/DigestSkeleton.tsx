export default function DigestSkeleton() {
  return (
    <div className="animate-pulse space-y-3">
      {[...Array(5)].map((_, i) => (
        <div
          key={i}
          className="p-3 rounded-lg border dark:border-gray-800 bg-white dark:bg-gray-900"
        >
          <div className="h-5 bg-gray-200 dark:bg-gray-800 rounded w-3/4 mb-2" />
          <div className="space-y-1 mb-2">
            <div className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-full" />
            <div className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-5/6" />
            <div className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-4/6" />
          </div>
          <div className="h-4 bg-gray-200 dark:bg-gray-800 rounded w-1/4" />
        </div>
      ))}
    </div>
  )
} 