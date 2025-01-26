import type { SearchResult } from '../types'
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'

dayjs.extend(utc)

interface SearchParams {
  query: string
  language: string
  limit: number
  fromDate: Date
  toDate: Date
  categoryIds: number[]
  sourceIds: number[]
}

export async function searchSummaries(params: SearchParams): Promise<SearchResult[]> {
  const response = await fetch('/api/search/summaries', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      query: params.query,
      language: params.language,
      limit: params.limit,
      fromDate: dayjs(params.fromDate).utc().toISOString(),
      toDate: dayjs(params.toDate).utc().toISOString(),
      categoryIds: params.categoryIds,
      sourceIds: params.sourceIds
    })
  })

  if (!response.ok) {
    throw new Error('Failed to search summaries')
  }

  return response.json()
} 