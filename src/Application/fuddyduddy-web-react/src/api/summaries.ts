import axios from 'axios'
import type { Summary } from '../types'

export const summaryKeys = {
  all: ['summaries'] as const,
  latest: (language: string, pageSize: number = 10, page: number = 0) => 
    [...summaryKeys.all, 'latest', language, pageSize, page] as const,
  detail: (id: string) => [...summaryKeys.all, 'detail', id] as const,
}

export async function fetchLatestSummaries(language: string = 'RU', pageSize: number = 10, page: number = 0) {
  try {
    const params = new URLSearchParams({
      language,
      pageSize: pageSize.toString(),
      page: page.toString()
    })

    const { data } = await axios.get<Summary[]>(`/api/summaries`, { params })
    return data
  } catch (error) {
    console.error('Error fetching summaries:', error)
    throw error
  }
}

export async function fetchSummaryById(id: string): Promise<Summary> {
  try {
    const { data } = await axios.get<Summary>(`/api/summaries/${id}`)
    return data
  } catch (error) {
    console.error('Error fetching summary:', error)
    throw error
  }
} 