import { z } from 'zod';

export const exportSchema = z.object({
  mode: z.enum(['MONTHS', 'RANGE']),
  months: z.array(z.string().regex(/^\d{4}-\d{2}$/)).optional(),
  range: z.object({ from: z.string().min(1), to: z.string().min(1) }).optional(),
  blocks: z.array(z.enum(['SUMMARY', 'TREND', 'CATEGORIES', 'TRANSACTIONS']))
    .min(1, 'Selectează cel puțin un bloc.'),
  granularity: z.enum(['DAILY', 'WEEKLY', 'MONTHLY']).optional(),
  runningBalanceInStatement: z.boolean().optional(),
  cumulativeTotal: z.boolean().optional(),
})
.refine(v => v.mode !== 'MONTHS' || (v.months && v.months.length > 0), {
  message: 'Selectează cel puțin o lună.', path: ['months'],
})
.refine(v => v.mode !== 'RANGE' || (v.range && v.range.from <= v.range.to), {
  message: 'Data de început trebuie ≤ data de final.', path: ['range'],
});

export type ExportFormValues = z.infer<typeof exportSchema>;
