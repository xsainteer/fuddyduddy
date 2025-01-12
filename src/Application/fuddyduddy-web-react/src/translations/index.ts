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
    description: string
    generatedEveryHour: string
    viewAll: string
  }
  about: {
    title: string
    description: string
    disclaimer: string
    howItWorks: {
      title: string
      step1: string
      step2: string
      step3: string
    }
    legal: {
      title: string
      copyright: string
      disclaimer: string
      aiDisclaimer: string
      responsibility: string
      contentWarning: string
      optOut: string
    }
    privacy: {
      title: string
      dataCollection: string
      cookies: string
      thirdParty: string
    }
    contact: {
      title: string
      description: string
      email: string
    }
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
    back: string
    new: string
  }
  errors: {
    failedToLoadDigests: string
  }
}

const translations: Record<Language, Translation> = {
  EN: {
    header: {
      title: 'FuddyDuddy News',
      subtitle: 'KYRGYZSTAN NEWS',
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
      description: 'A digest is a short summary of the most important news in Kyrgyzstan over the past hour. Our AI reads multiple news sources, identifies key events, and combines them into one easy-to-read overview with links to original articles.',
      generatedEveryHour: 'Digests are generated every hour',
      viewAll: 'View all digests'
    },
    about: {
      title: 'About FuddyDuddy News',
      description: 'FuddyDuddy News is an AI-powered platform that analyzes news materials from Kyrgyzstan and shares impressions about them in the form of brief reviews, always with a link to the original source.',
      disclaimer: 'All rights to news materials belong to their respective news agencies. Each of our reviews contains a direct link to the original article.',
      howItWorks: {
        title: 'How It Works',
        step1: 'Our system familiarizes itself with news materials through official channels of news agencies.',
        step2: 'Artificial Intelligence forms its own impression of the read material, creating a brief review-reflection.',
        step3: 'Each review is accompanied by a link to the original article, so you can read the source material.'
      },
      legal: {
        title: 'Legal Information',
        copyright: 'All rights to news materials belong to their rightful owners. We do not copy or distribute original content.',
        disclaimer: 'FuddyDuddy News is not a news aggregator. We offer AI-generated impressions of news materials without claiming authorship of the original content.',
        aiDisclaimer: 'Our reviews are AI\'s subjective perception of news. They are not copies, translations, or official representations of the original articles.',
        responsibility: 'FuddyDuddy News is not responsible for the content of original articles. Always refer to primary sources for official information.',
        contentWarning: 'Please note that due to the automated nature of our service, AI-generated reviews may contain inaccuracies or subjective interpretations. Always verify information in official sources.',
        optOut: 'If you represent a news agency and would like your materials not to be analyzed by our service, please contact us at the email below.'
      },
      privacy: {
        title: 'Privacy Information',
        dataCollection: 'We collect minimal data necessary for the service to function, such as your language preference and theme settings.',
        cookies: 'We use essential cookies to remember your preferences and improve your browsing experience.',
        thirdParty: 'Links to original articles will take you to third-party news websites, which have their own privacy policies and terms of service.'
      },
      contact: {
        title: 'Contact Us',
        description: 'For all inquiries, including requests from news agencies, please contact:',
        email: 'hey@fuddy-duddy.org'
      }
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
      backToFeed: 'Back to feed',
      back: 'Back',
      new: 'New'
    },
    errors: {
      failedToLoadDigests: 'Failed to load digests'
    }
  },
  RU: {
    header: {
      title: 'FuddyDuddy News',
      subtitle: 'НОВОСТИ КЫРГЫЗСТАНА',
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
      description: 'Дайджест — это краткий обзор самых важных новостей Кыргызстана за последний час. Наш ИИ читает множество новостных источников, выделяет ключевые события и объединяет их в один удобный обзор со ссылками на оригинальные статьи.',
      generatedEveryHour: 'Дайджесты генерируются каждый час',
      viewAll: 'Смотреть все дайджесты'
    },
    about: {
      title: 'О FuddyDuddy News',
      description: 'FuddyDuddy News — это AI-powered платформа, которая анализирует новостные материалы Кыргызстана и делится впечатлениями о них в форме кратких обзоров, всегда со ссылкой на оригинальный источник.',
      disclaimer: 'Все права на новостные материалы принадлежат соответствующим информационным агентствам. Каждый наш обзор содержит прямую ссылку на оригинальную статью.',
      howItWorks: {
        title: 'Как это работает',
        step1: 'Наша система знакомится с новостными материалами через официальные каналы информационных агентств.',
        step2: 'Искусственный интеллект формирует собственное впечатление о прочитанном материале, создавая краткий обзор-рефлексию.',
        step3: 'Каждый обзор сопровождается ссылкой на оригинальную статью, чтобы вы могли ознакомиться с первоисточником.'
      },
      legal: {
        title: 'Правовая информация',
        copyright: 'Все права на новостные материалы принадлежат их законным владельцам. Мы не копируем и не распространяем оригинальный контент.',
        disclaimer: 'FuddyDuddy News не является новостным агрегатором. Мы предлагаем AI-generated впечатления о новостных материалах, не претендуя на авторство оригинального контента.',
        aiDisclaimer: 'Наши обзоры — это субъективное восприятие новостей искусственным интеллектом. Они не являются копией, переводом или официальным представлением оригинальных статей.',
        responsibility: 'FuddyDuddy News не несет ответственности за содержание оригинальных статей. Для получения официальной информации всегда обращайтесь к первоисточникам.',
        contentWarning: 'Обратите внимание, что из-за автоматизированного характера нашего сервиса, AI-generated обзоры могут содержать неточности или субъективные интерпретации. Всегда проверяйте информацию в официальных источниках.',
        optOut: 'Если вы представляете информационное агентство и хотите, чтобы материалы вашего издания не анализировались нашим сервисом, пожалуйста, свяжитесь с нами по указанному ниже email.'
      },
      privacy: {
        title: 'Информация о конфиденциальности',
        dataCollection: 'Мы собираем минимальные данные, необходимые для работы сервиса, такие как ваши языковые предпочтения и настройки темы.',
        cookies: 'Мы используем необходимые файлы cookie для сохранения ваших предпочтений и улучшения работы с сайтом.',
        thirdParty: 'Ссылки на оригинальные статьи ведут на сторонние новостные сайты, которые имеют свою политику конфиденциальности и условия использования.'
      },
      contact: {
        title: 'Связаться с нами',
        description: 'По всем вопросам, включая запросы от информационных агентств, пожалуйста, обращайтесь:',
        email: 'hey@fuddy-duddy.org'
      }
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
      backToFeed: 'Лента новостей',
      back: 'Назад',
      new: 'Новое'
    },
    errors: {
      failedToLoadDigests: 'Не удалось загрузить дайджесты'
    }
  }
}

export function useTranslations(language: Language) {
  return translations[language]
} 