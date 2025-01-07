export interface Summary {
  id: string
  title: string
  article: string
  category: string
  categoryLocal: string
  generatedAt: string
  reason: string | null
  newsArticleTitle: string
  newsArticleUrl: string
  source: string
}

export interface Category {
  id: number
  name: string
  local: string
}

export interface NewsSource {
  id: string
  name: string
  domain: string
}

export interface Language {
  id: string
  name: string
}

export interface Filters {
  language?: string
  categoryId?: number
  sourceId?: string
}

export interface DigestSummary {
  id: string
  title: string
  generatedAt: string
} 