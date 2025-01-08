import axios from 'axios'
import type { Digest } from '../types'

export async function fetchLatestDigests(language: string = 'RU', count: number = 5, skip: number = 0) {
  try {
    const params = new URLSearchParams({
      language,
      count: count.toString(),
      skip: skip.toString()
    })

    const { data } = await axios.get<Digest[]>(`/api/digests`, { params })
    return data
  } catch (error) {
    console.error('Error fetching digests:', error)
    throw error
  }
}

export async function fetchDigestById(id: string): Promise<Digest> {
  try {
    const { data } = await axios.get<Digest>(`/api/digests/${id}`)
    return data
  } catch (error) {
    console.error('Error fetching digest:', error)
    throw error
  }
} 