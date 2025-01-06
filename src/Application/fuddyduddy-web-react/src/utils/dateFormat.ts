import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import timezone from 'dayjs/plugin/timezone'
import localizedFormat from 'dayjs/plugin/localizedFormat'

// Initialize plugins
dayjs.extend(utc)
dayjs.extend(timezone)
dayjs.extend(localizedFormat)

export function formatDateTime(utcDate: string) {
  return dayjs.utc(utcDate).local().format('HH:mm · MMM D, YYYY')
} 