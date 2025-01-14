const domain = 'fuddy-duddy.org'

export const metaTranslations = {
  EN: {
    title: 'FuddyDuddy News - Kyrgyzstan News',
    description: 'Latest news from Kyrgyzstan. AI-powered news aggregation service that collects and summarizes news from various Kyrgyzstan news agencies.',
    keywords: 'Kyrgyzstan, news, AI, artificial intelligence, news aggregator, kyrgyzstan news, bishkek news',
    image: `https://${domain}/fulllogo_with_slogan_en.png`
  },
  RU: {
    title: 'FuddyDuddy News - Новости Кыргызстана',
    description: 'Актуальные новости Кыргызстана. AI-powered сервис агрегации новостей, который собирает и обобщает новости из различных информационных агентств Кыргызстана.',
    keywords: 'Кыргызстан, новости, AI, искусственный интеллект, агрегатор новостей, новости кыргызстана, бишкек новости',
    image: `https://${domain}/fulllogo_with_slogan_ru.png`
  }
} as const
