import { useOutletContext } from 'react-router-dom'
import NewsFeed from '../components/NewsFeed'
import FilterBar from '../components/FilterBar'
import type { Filters } from '../types'

type ContextType = {
  filters: Filters
  setFilters: (filters: Filters) => void
  showFilters: boolean
}

export default function HomePage() {
  const { filters, setFilters, showFilters } = useOutletContext<ContextType>()

  return (
    <>
      {showFilters && <FilterBar filters={filters} onFiltersChange={setFilters} />}
      <NewsFeed filters={filters} />
    </>
  )
} 