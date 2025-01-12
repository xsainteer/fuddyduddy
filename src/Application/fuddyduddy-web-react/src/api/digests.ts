import axios from 'axios'
import type { Digest } from '../types'

export const digestKeys = {
  all: ['digests'] as const,
  latest: (language: string, pageSize: number = 10, page: number = 0) => 
    [...digestKeys.all, 'latest', language, pageSize, page] as const,
  detail: (id: string) => [...digestKeys.all, 'detail', id] as const,
}

export async function fetchLatestDigests(language: string = 'RU', pageSize: number = 10, page: number = 0) {
  try {
    const params = new URLSearchParams({
      language,
      pageSize: pageSize.toString(),
      page: page.toString()
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