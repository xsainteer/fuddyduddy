import { useEffect } from 'react'
import { useLocation } from 'react-router-dom'
import { useLanguage } from '../contexts/LanguageContext'
import { metaTranslations } from '../translations/meta'

export default function MetaTags() {
  const { language } = useLanguage()
  const location = useLocation()
  const meta = metaTranslations[language]
  const domain = 'fuddy-duddy.org'

  useEffect(() => {
    // Update basic meta tags
    document.title = meta.title
    document.querySelector('meta[name="description"]')?.setAttribute('content', meta.description)
    document.querySelector('meta[name="keywords"]')?.setAttribute('content', meta.keywords)
    document.querySelector('meta[name="language"]')?.setAttribute('content', language === 'EN' ? 'English' : 'Russian')

    // Update Open Graph meta tags
    document.querySelector('meta[property="og:title"]')?.setAttribute('content', meta.title)
    document.querySelector('meta[property="og:description"]')?.setAttribute('content', meta.description)
    document.querySelector('meta[property="og:url"]')?.setAttribute('content', `https://${domain}${location.pathname}`)
    document.querySelector('meta[property="og:image"]')?.setAttribute('content', meta.image)

    // Update Twitter meta tags
    document.querySelector('meta[property="twitter:title"]')?.setAttribute('content', meta.title)
    document.querySelector('meta[property="twitter:description"]')?.setAttribute('content', meta.description)
    document.querySelector('meta[property="twitter:url"]')?.setAttribute('content', `https://${domain}${location.pathname}`)
    document.querySelector('meta[property="twitter:image"]')?.setAttribute('content', meta.image)
  }, [language, location.pathname, meta])

  return null
}
