import dayjs from 'dayjs'
import 'dayjs/locale/ru'
import utc from 'dayjs/plugin/utc'
import localizedFormat from 'dayjs/plugin/localizedFormat'
import { useTranslations } from '../translations'
import type { Language } from '../contexts/LanguageContext'

// Initialize plugins
dayjs.extend(utc)
dayjs.extend(localizedFormat)

export function formatDateTime(utcDate: string, language: Language) {
  // Set locale based on interface language
  dayjs.locale(language.toLowerCase())
  const t = useTranslations(language)
  let local = dayjs.utc(utcDate).local()
  let now = dayjs()

  // Today
  if (local.isSame(now, 'day')) {
    return `${t.dates.today} ${local.format('HH:mm')}`
  }

  // Yesterday
  if (local.isSame(now.subtract(1, 'day'), 'day')) {
    return `${t.dates.yesterday} ${local.format('HH:mm')}`
  }

  // This year
  if (local.isSame(now, 'year')) {
    return local.format('D MMMM HH:mm')
  }

  // Different year
  return local.format('D MMMM YYYY, HH:mm')
} 