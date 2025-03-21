import { useLocalization } from '../hooks/useLocalization'
import { Link } from 'react-router-dom'

export default function AboutPage() {
  const { t, language } = useLocalization()

  return (
    <>
      <Link
        to={`/${language.toLowerCase()}/feed`}
        className="inline-block mb-4 text-blue-600 dark:text-blue-400 hover:underline"
      >
        {t.common.backToFeed}
      </Link>

      <div className="space-y-8">
        {/* Main About Section */}
        <section className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
          <h1 className="text-2xl font-bold text-gray-800 dark:text-gray-200 mb-4">
            {t.about.title}
          </h1>
          <div className="prose dark:prose-invert max-w-none space-y-4">
            <p className="text-gray-600 dark:text-gray-400">
              {t.about.description}
            </p>
            <p className="text-gray-600 dark:text-gray-400">
              {t.about.disclaimer}
            </p>
          </div>
        </section>

        {/* How It Works */}
        <section className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
          <h2 className="text-xl font-bold text-gray-800 dark:text-gray-200 mb-4">
            {t.about.howItWorks.title}
          </h2>
          <div className="space-y-4">
            <div className="flex gap-4 items-start">
              <div className="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center flex-shrink-0">
                <span className="text-blue-600 dark:text-blue-400 font-medium">1</span>
              </div>
              <p className="text-gray-600 dark:text-gray-400">{t.about.howItWorks.step1}</p>
            </div>
            <div className="flex gap-4 items-start">
              <div className="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center flex-shrink-0">
                <span className="text-blue-600 dark:text-blue-400 font-medium">2</span>
              </div>
              <p className="text-gray-600 dark:text-gray-400">{t.about.howItWorks.step2}</p>
            </div>
            <div className="flex gap-4 items-start">
              <div className="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center flex-shrink-0">
                <span className="text-blue-600 dark:text-blue-400 font-medium">3</span>
              </div>
              <p className="text-gray-600 dark:text-gray-400">{t.about.howItWorks.step3}</p>
            </div>
          </div>
        </section>

        {/* Legal Information */}
        <section className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
          <h2 className="text-xl font-bold text-gray-800 dark:text-gray-200 mb-4">
            {t.about.legal.title}
          </h2>
          <div className="space-y-4">
            <div className="p-4 bg-yellow-50 dark:bg-yellow-900/30 border border-yellow-200 dark:border-yellow-700 rounded-lg">
              <p className="text-yellow-800 dark:text-yellow-200">
                {t.about.legal.contentWarning}
              </p>
            </div>
            <p className="text-gray-600 dark:text-gray-400">
              {t.about.legal.copyright}
            </p>
            <p className="text-gray-600 dark:text-gray-400">
              {t.about.legal.disclaimer}
            </p>
            <p className="text-gray-600 dark:text-gray-400">
              {t.about.legal.aiDisclaimer}
            </p>
            <div className="mt-6 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
              <p className="text-sm text-gray-500 dark:text-gray-400 italic">
                {t.about.legal.responsibility}
              </p>
            </div>
            <p className="text-gray-600 dark:text-gray-400">
              {t.about.legal.optOut}
            </p>
          </div>
        </section>

        {/* Privacy Information */}
        <section className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
          <h2 className="text-xl font-bold text-gray-800 dark:text-gray-200 mb-4">
            {t.about.privacy.title}
          </h2>
          <div className="space-y-4">
            <p className="text-gray-600 dark:text-gray-400">
              {t.about.privacy.dataCollection}
            </p>
            <p className="text-gray-600 dark:text-gray-400">
              {t.about.privacy.cookies}
            </p>
            <p className="text-gray-600 dark:text-gray-400">
              {t.about.privacy.thirdParty}
            </p>
          </div>
        </section>

        {/* Contact Information */}
        <section className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
          <h2 className="text-xl font-bold text-gray-800 dark:text-gray-200 mb-4">
            {t.about.contact.title}
          </h2>
          <div className="space-y-4">
            <p className="text-gray-600 dark:text-gray-400">
              {t.about.contact.description}
            </p>
            <a
              href={`mailto:${t.about.contact.email}`}
              className="inline-block text-blue-600 dark:text-blue-400 hover:underline"
            >
              {t.about.contact.email}
            </a>
          </div>
        </section>

        {/* Open Source Information */}
        <section className="bg-white dark:bg-gray-900 rounded-xl shadow-sm p-6">
          <h2 className="text-xl font-bold text-gray-800 dark:text-gray-200 mb-4">
            {t.about.openSource.title}
          </h2>
          <div className="space-y-4">
            <p className="text-gray-600 dark:text-gray-400">
              {t.about.openSource.description}
            </p>
            <a
              href={t.about.openSource.githubLink}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center text-blue-600 dark:text-blue-400 hover:underline"
            >
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="mr-2">
                <path d="M9 19c-5 1.5-5-2.5-7-3m14 6v-3.87a3.37 3.37 0 0 0-.94-2.61c3.14-.35 6.44-1.54 6.44-7A5.44 5.44 0 0 0 20 4.77 5.07 5.07 0 0 0 19.91 1S18.73.65 16 2.48a13.38 13.38 0 0 0-7 0C6.27.65 5.09 1 5.09 1A5.07 5.07 0 0 0 5 4.77a5.44 5.44 0 0 0-1.5 3.78c0 5.42 3.3 6.61 6.44 7A3.37 3.37 0 0 0 9 18.13V22"></path>
              </svg>
              {t.about.openSource.viewOnGithub}
            </a>
          </div>
        </section>
      </div>
    </>
  )
} 