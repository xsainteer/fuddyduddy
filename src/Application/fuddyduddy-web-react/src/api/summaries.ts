import axios from 'axios'
import type { Summary } from '../types'

export async function fetchSummaries(page: number) {
  try {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: '20',
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