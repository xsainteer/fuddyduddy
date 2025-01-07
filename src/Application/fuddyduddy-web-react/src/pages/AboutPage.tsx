import { useLocalization } from '../hooks/useLocalization'

export default function AboutPage() {
  const { t } = useLocalization()

  return (
    <div className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6 space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
        {t.about.title}
      </h1>
      <p className="text-gray-700 dark:text-gray-300">
        {t.about.description}
      </p>
      <p className="text-gray-600 dark:text-gray-400 text-sm">
        {t.about.disclaimer}
      </p>
    </div>
  )
} 