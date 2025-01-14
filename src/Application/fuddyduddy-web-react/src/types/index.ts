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
  language: string
}

export interface Category {
  id: number
  name: string
  local: string
}

export interface NewsSource {
  id: number
  name: string
  domain: string
}

export interface Language {
  id: string
  name: string
  local: string
}

export interface Filters {
  categoryId?: number
  sourceId?: number
  language?: string
}

export interface DigestSummary {
  id: string
  title: string
  generatedAt: string
}

export interface DigestReference {
  title: string
  url: string
  reason: string
  source: string
  category: string
}

export interface Digest {
  id: string
  title: string
  content: string
  generatedAt: string
  periodStart: string
  periodEnd: string
  references: DigestReference[]
} 