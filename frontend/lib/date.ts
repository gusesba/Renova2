export function getTodayDateInputValue() {
  const now = new Date();
  const timezoneOffset = now.getTimezoneOffset() * 60_000;
  return new Date(now.getTime() - timezoneOffset).toISOString().slice(0, 10);
}

export function getPreviousMonthLastDateInputValue() {
  const now = new Date();
  const previousMonthLastDay = new Date(now.getFullYear(), now.getMonth(), 0);
  const timezoneOffset = previousMonthLastDay.getTimezoneOffset() * 60_000;
  return new Date(previousMonthLastDay.getTime() - timezoneOffset).toISOString().slice(0, 10);
}
