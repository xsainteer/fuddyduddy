import { Link } from 'react-router-dom'

export default function AboutPage() {
  return (
    <div>
      {/* Back button */}
      <div className="mb-8">
        <Link 
          to="/"
          className="inline-flex items-center text-blue-500 hover:text-blue-600 dark:text-blue-400 dark:hover:text-blue-300"
        >
          <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          Back to news feed
        </Link>
      </div>

      <div className="space-y-12">
        {/* English section */}
        <section>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            About FuddyDuddy News
          </h2>
          <div className="space-y-4 text-gray-600 dark:text-gray-300">
            <p>
              FuddyDuddy News is a news aggregation service that collects and processes news from various Kyrgyzstan news agencies. 
              We use artificial intelligence to rephrase titles and create concise summaries of articles, making it easier to stay 
              informed about current events in Kyrgyzstan.
            </p>
            <p>
              All news content belongs to their respective news agencies and is protected by copyright law. Each summary includes 
              proper attribution to the original source and provides a direct link to the original article. We respect the intellectual 
              property rights of news agencies and aim to promote their work through our platform.
            </p>
          </div>
        </section>

        {/* Russian section */}
        <section>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            О FuddyDuddy News
          </h2>
          <div className="space-y-4 text-gray-600 dark:text-gray-300">
            <p>
              FuddyDuddy News — это сервис агрегации новостей, который собирает и обрабатывает новости различных информационных 
              агентств Кыргызстана. Мы используем искусственный интеллект для перефразирования заголовков и создания кратких 
              обзоров статей, чтобы вам было легче оставаться в курсе текущих событий в Кыргызстане.
            </p>
            <p>
              Все новостные материалы принадлежат соответствующим информационным агентствам и защищены законом об авторском праве. 
              Каждый обзор содержит указатель на источник и имеет прямую ссылку на оригинальную статью. Мы уважаем права 
              интеллектуальной собственности информационных агентств и стремимся продвигать их работу через нашу платформу.
            </p>
          </div>
        </section>
      </div>
    </div>
  )
} 