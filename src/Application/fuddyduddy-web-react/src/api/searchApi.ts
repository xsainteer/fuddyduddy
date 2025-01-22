import { Summary } from '../types'

interface SearchRequest {
  query: string
  limit?: number
  language: string
}

interface SearchResponse {
  summary: Summary
  score: number
}

export const searchSummaries = async (request: SearchRequest): Promise<SearchResponse[]> => {
  const response = await fetch('/api/search/summaries', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    throw new Error('Search request failed')
  }

  return response.json()
} 