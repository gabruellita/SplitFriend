import { z } from 'zod';

// Oglindeste CreateTransactionRequestValidator + CreateRecurringTemplateRequestValidator din BE.
export const transactionSchema = z
  .object({
    amount: z
      .number({ error: 'Suma este obligatorie.' })
      .positive('Suma trebuie să fie mai mare ca 0.'),

    kind: z.enum(['INCOME', 'EXPENSE']),

    transactionDate: z
      .string()
      .min(1, 'Data este obligatorie.'),

    categoryId: z
      .number()
      .int()
      .positive()
      .nullable()
      .optional(),

    description: z
      .string()
      .max(500, 'Descrierea poate avea maxim 500 de caractere.')
      .optional(),

    // ─── Recurenta ───────────────────────────────────────
    isRecurring: z.boolean().optional(),

    frequency: z.enum(['DAILY', 'WEEKLY', 'MONTHLY', 'YEARLY']).optional(),

    intervalCount: z
      .number()
      .int()
      .positive('Intervalul trebuie să fie cel puțin 1.')
      .optional(),

    endDate: z.string().optional(),
  })
  .refine(v => !(v.isRecurring ?? false) || !!v.frequency, {
    path: ['frequency'],
    message: 'Alege o frecvență pentru recurență.',
  })
  .refine(v => !(v.isRecurring ?? false) || (v.intervalCount ?? 0) >= 1, {
    path: ['intervalCount'],
    message: 'Intervalul trebuie să fie cel puțin 1.',
  })
  .refine(
    v => !(v.isRecurring ?? false) || !v.endDate || v.endDate >= v.transactionDate,
    { path: ['endDate'], message: 'Data de final trebuie să fie ≥ data de start.' },
  );

export type TransactionFormValues = z.infer<typeof transactionSchema>;
