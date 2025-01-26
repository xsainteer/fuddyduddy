import { useState } from 'react'
import { Dialog } from '@headlessui/react'
import { CalendarIcon, ChevronLeftIcon, ChevronRightIcon } from '@heroicons/react/24/outline'
import { format, addMonths, subMonths, startOfMonth, endOfMonth, eachDayOfInterval, isSameMonth, isSameDay, isToday, startOfDay } from 'date-fns'
import { ru, enUS, Locale } from 'date-fns/locale'

interface DatePickerProps {
  selected: Date
  onChange: (date: Date) => void
  label: string
  minDate?: Date
  maxDate?: Date
  language: 'RU' | 'EN'
}

export default function DatePicker({ selected, onChange, label, minDate, maxDate, language }: DatePickerProps) {
  const [isOpen, setIsOpen] = useState(false)
  const [currentMonth, setCurrentMonth] = useState(startOfMonth(selected))
  const locale: Locale = language === 'RU' ? ru : enUS

  const days = eachDayOfInterval({
    start: startOfMonth(currentMonth),
    end: endOfMonth(currentMonth)
  })

  const previousMonth = () => {
    setCurrentMonth(subMonths(currentMonth, 1))
  }

  const nextMonth = () => {
    setCurrentMonth(addMonths(currentMonth, 1))
  }

  const handleDateSelect = (date: Date) => {
    onChange(startOfDay(date))
    setIsOpen(false)
  }

  const weekDays = Array.from({ length: 7 }, (_, i) => 
    locale.localize?.day(i as 0 | 1 | 2 | 3 | 4 | 5 | 6, { width: 'narrow' }) ?? ['M', 'T', 'W', 'T', 'F', 'S', 'S'][i]
  )

  return (
    <>
      <button
        type="button"
        onClick={() => setIsOpen(true)}
        className="flex items-center gap-2 px-4 py-2 border dark:border-gray-700 
                  rounded-lg bg-white dark:bg-gray-800 text-gray-900 
                  dark:text-gray-100 hover:bg-gray-50 dark:hover:bg-gray-700"
      >
        <CalendarIcon className="h-5 w-5 text-gray-500" />
        <span>{label}: {format(selected, 'dd.MM.yyyy')}</span>
      </button>

      <Dialog
        open={isOpen}
        onClose={() => setIsOpen(false)}
        className="relative z-50"
      >
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Panel className="mx-auto max-w-sm rounded-lg bg-white dark:bg-gray-800 p-6 w-full">
            <Dialog.Title className="text-xl font-bold text-gray-800 dark:text-gray-200 mb-6">
              {label}
            </Dialog.Title>

            {/* Header with month/year and navigation */}
            <div className="flex items-center justify-between mb-4 bg-gray-50 dark:bg-gray-800/50 px-4 py-2 -mx-6 rounded-t-lg">
              <button
                onClick={previousMonth}
                disabled={minDate && isSameMonth(currentMonth, minDate)}
                className="p-1 text-xl font-medium hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg 
                         disabled:opacity-50 text-gray-600 dark:text-gray-400 
                         hover:text-gray-900 dark:hover:text-gray-200 w-8 h-8 flex items-center justify-center"
              >
                <ChevronLeftIcon className="w-5 h-5" />
              </button>
              <div className="text-sm font-medium text-gray-600 dark:text-gray-400">
                {format(currentMonth, 'LLLL yyyy', { locale })}
              </div>
              <button
                onClick={nextMonth}
                disabled={maxDate && isSameMonth(currentMonth, maxDate)}
                className="p-1 text-xl font-medium hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg 
                         disabled:opacity-50 text-gray-600 dark:text-gray-400 
                         hover:text-gray-900 dark:hover:text-gray-200 w-8 h-8 flex items-center justify-center"
              >
                <ChevronRightIcon className="w-5 h-5" />
              </button>
            </div>

            {/* Weekday headers */}
            <div className="grid grid-cols-7 gap-1 mb-2 bg-gray-50 dark:bg-gray-800/50 -mx-6 px-6 py-2">
              {weekDays.map((day, i) => (
                <div
                  key={i}
                  className="w-8 h-8 flex items-center justify-center text-sm text-gray-600 dark:text-gray-400"
                >
                  {day}
                </div>
              ))}
            </div>

            {/* Calendar grid */}
            <div className="grid grid-cols-7 gap-1">
              {days.map((day, dayIdx) => {
                const isSelected = isSameDay(day, selected)
                const isDisabled = (minDate && day < startOfDay(minDate)) || (maxDate && day > startOfDay(maxDate))

                return (
                  <button
                    key={dayIdx}
                    onClick={() => !isDisabled && handleDateSelect(day)}
                    disabled={isDisabled}
                    className={`
                      w-8 h-8 text-sm rounded-lg flex items-center justify-center transition-colors
                      ${isDisabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}
                      ${isSelected
                        ? 'bg-blue-50 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300'
                        : 'text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800/50'
                      }
                      ${isToday(day) && !isSelected ? 'ring-1 ring-blue-500 dark:ring-blue-400' : ''}
                    `}
                  >
                    {format(day, 'd')}
                  </button>
                )
              })}
            </div>

            <div className="mt-6 flex justify-end space-x-4">
              <button
                type="button"
                onClick={() => setIsOpen(false)}
                className="px-4 py-2 text-gray-500 hover:text-gray-700 dark:text-gray-400 
                         dark:hover:text-gray-200"
              >
                {language === 'RU' ? 'Назад' : 'Back'}
              </button>
            </div>
          </Dialog.Panel>
        </div>
      </Dialog>
    </>
  )
} 
