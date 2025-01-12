import { createContext, useContext, useState, ReactNode } from 'react'

interface LayoutContextType {
  showSidePanels: boolean
  setShowSidePanels: (show: boolean) => void
}

const LayoutContext = createContext<LayoutContextType | undefined>(undefined)

interface LayoutProviderProps {
  children: ReactNode
}

export function LayoutProvider({ children }: LayoutProviderProps) {
  const [showSidePanels, setShowSidePanels] = useState(true)

  return (
    <LayoutContext.Provider value={{ showSidePanels, setShowSidePanels }}>
      {children}
    </LayoutContext.Provider>
  )
}

export function useLayout() {
  const context = useContext(LayoutContext)
  if (context === undefined) {
    throw new Error('useLayout must be used within a LayoutProvider')
  }
  return context
} 