import axios from 'axios'
import type { Summary } from '../types'

const API_URL = 'http://localhost:5102/api'

export async function fetchSummaries(page: number) {
  try {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: '20',
    })

    const { data } = await axios.get<Summary[]>(`${API_URL}/summaries`, { params })
    return data
  } catch (error) {
    console.error('Error fetching summaries:', error)
    throw error
  }
}

export async function fetchSummaryById(id: string): Promise<Summary> {
  const response = await fetch(`/api/summaries/${id}`)
  if (!response.ok) {
    throw new Error('Failed to fetch summary')
  }
  return response.json()
} 