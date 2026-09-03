import { axiosClient } from './axiosClient';
import { API_ENDPOINTS } from './endpoints';
import type { TransactionKind } from '@/types/finance.types';
import type {
  Granularity,
  MoMGranularity,
  TimeseriesPoint,
  CategorySlice,
  TopCategory,
  CalendarDay,
  HistogramBucket,
  SavingsRatePoint,
  RunningBalancePoint,
  MoMPoint,
  ParetoSlice,
  WeekdayPoint,
  RecurringSplitPoint,
} from '@/types/statistics.types';

const S = API_ENDPOINTS.STATISTICS;

export const statisticsApi = {
  getTimeseries: async (from?: string, to?: string, granularity?: Granularity): Promise<TimeseriesPoint[]> => {
    const { data } = await axiosClient.get<TimeseriesPoint[]>(S.TIMESERIES, { params: { from, to, granularity } });
    return data;
  },

  getCategoryBreakdown: async (from?: string, to?: string, kind?: TransactionKind): Promise<CategorySlice[]> => {
    const { data } = await axiosClient.get<CategorySlice[]>(S.CATEGORY_BREAKDOWN, { params: { from, to, kind } });
    return data;
  },

  getTopCategories: async (from?: string, to?: string, kind?: TransactionKind, limit?: number): Promise<TopCategory[]> => {
    const { data } = await axiosClient.get<TopCategory[]>(S.TOP_CATEGORIES, { params: { from, to, kind, limit } });
    return data;
  },

  getCalendar: async (from?: string, to?: string): Promise<CalendarDay[]> => {
    const { data } = await axiosClient.get<CalendarDay[]>(S.CALENDAR, { params: { from, to } });
    return data;
  },

  getHistogram: async (from?: string, to?: string, max?: number, buckets?: number): Promise<HistogramBucket[]> => {
    const { data } = await axiosClient.get<HistogramBucket[]>(S.HISTOGRAM, { params: { from, to, max, buckets } });
    return data;
  },

  getSavingsRate: async (from?: string, to?: string): Promise<SavingsRatePoint[]> => {
    const { data } = await axiosClient.get<SavingsRatePoint[]>(S.SAVINGS_RATE, { params: { from, to } });
    return data;
  },

  getRunningBalance: async (from?: string, to?: string): Promise<RunningBalancePoint[]> => {
    const { data } = await axiosClient.get<RunningBalancePoint[]>(S.RUNNING_BALANCE, { params: { from, to } });
    return data;
  },

  getMoM: async (from?: string, to?: string, kind?: TransactionKind, granularity?: MoMGranularity): Promise<MoMPoint[]> => {
    const { data } = await axiosClient.get<MoMPoint[]>(S.MOM, { params: { from, to, kind, granularity } });
    return data;
  },

  getPareto: async (from?: string, to?: string): Promise<ParetoSlice[]> => {
    const { data } = await axiosClient.get<ParetoSlice[]>(S.PARETO, { params: { from, to } });
    return data;
  },

  getWeekday: async (from?: string, to?: string, kind?: TransactionKind): Promise<WeekdayPoint[]> => {
    const { data } = await axiosClient.get<WeekdayPoint[]>(S.WEEKDAY, { params: { from, to, kind } });
    return data;
  },

  getRecurringSplit: async (from?: string, to?: string, kind?: TransactionKind): Promise<RecurringSplitPoint[]> => {
    const { data } = await axiosClient.get<RecurringSplitPoint[]>(S.RECURRING_SPLIT, { params: { from, to, kind } });
    return data;
  },
};
