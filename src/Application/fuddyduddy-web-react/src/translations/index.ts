import type { Language } from '../contexts/LanguageContext'

interface Translation {
  header: {
    title: string
    subtitle: string
    about: string
    digests: string
  }
  filters: {
    title: string
    categories: string
    sources: string
    language: string
    clearAll: string
  }
  digests: {
    title: string
    generatedEveryHour: string
  }
  about: {
    title: string
    description: string
    disclaimer: string
  }
  common: {
    readMore: string
    share: string
    loading: string
    error: string
    retry: string
    originalSource: string
    newSummariesAvailable: string
    loadMore: string
    backToFeed: string
  }
}

const translations: Record<Language, Translation> = {
  EN: {
    header: {
      title: 'FuddyDuddy News',
      subtitle: 'News from Kyrgyzstan',
      about: 'About',
      digests: 'Digests'
    },
    filters: {
      title: 'Filters',
      categories: 'Categories',
      sources: 'Sources',
      language: 'Language',
      clearAll: 'Clear all'
    },
    digests: {
      title: 'Latest Digests',
      generatedEveryHour: 'Digests are generated every hour'
    },
    about: {
      title: 'About FuddyDuddy News',
      description: 'FuddyDuddy News aggregates news from various Kyrgyzstan news agencies, using AI to rephrase titles and summarize articles for better readability.',
      disclaimer: 'All news content belongs to respective news agencies. Each summary contains a reference to the source and a link to the original article.'
    },
    common: {
      readMore: 'Read full article',
      share: 'Share',
      loading: 'Loading...',
      error: 'Something went wrong',
      retry: 'Try again',
      originalSource: 'Original source',
      newSummariesAvailable: 'New summaries available. Click to refresh.',
      loadMore: 'Load more',
      backToFeed: 'Back to news feed'
    }
  },
  RU: {
    header: {
      title: 'FuddyDuddy News',
      subtitle: 'Новости Кыргызстана',
      about: 'О проекте',
      digests: 'Дайджесты'
    },
    filters: {
      title: 'Фильтры',
      categories: 'Категории',
      sources: 'Источники',
      language: 'Язык',
      clearAll: 'Очистить все'
    },
    digests: {
      title: 'Последние дайджесты',
      generatedEveryHour: 'Дайджесты генерируются каждый час'
    },
    about: {
      title: 'О FuddyDuddy News',
      description: 'FuddyDuddy News агрегирует новости из различных информационных агентств Кыргызстана, используя ИИ для перефразирования заголовков и создания кратких обзоров статей.',
      disclaimer: 'Все новости принадлежат соответствующим информационным агентствам. Каждый обзор содержит ссылку на источник и оригинальную статью.'
    },
    common: {
      readMore: 'Читать полностью',
      share: 'Поделиться',
      loading: 'Загрузка...',
      error: 'Что-то пошло не так',
      retry: 'Попробовать снова',
      originalSource: 'Оригинальный источник',
      newSummariesAvailable: 'Доступны новые новости. Нажмите, чтобы обновить.',
      loadMore: 'Загрузить еще',
      backToFeed: 'Вернуться к ленте новостей'
    }
  }
}

export function useTranslations(language: Language) {
  return translations[language]
} 