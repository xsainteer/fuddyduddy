import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import timezone from 'dayjs/plugin/timezone'
import localizedFormat from 'dayjs/plugin/localizedFormat'
import 'dayjs/locale/ru'

// Initialize plugins
dayjs.extend(utc)
dayjs.extend(timezone)
dayjs.extend(localizedFormat)

export function formatDateTime(utcDate: string, language: string = 'EN') {
  // Set locale based on interface language
  dayjs.locale(language.toLowerCase())
  return dayjs.utc(utcDate).local().format('HH:mm · D MMM, YYYY')
} 