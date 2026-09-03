import { z } from 'zod';

// Oglindeste Create/UpdateCategoryRequestValidator din BE (Finance Service).
export const categorySchema = z.object({
  name: z
    .string()
    .min(1, 'Numele este obligatoriu.')
    .max(100, 'Numele poate avea maxim 100 de caractere.'),

  kind: z.enum(['INCOME', 'EXPENSE']),

  icon: z
    .string()
    .max(50, 'Iconița poate avea maxim 50 de caractere.')
    .optional(),

  color: z
    .string()
    .max(20, 'Culoarea poate avea maxim 20 de caractere.')
    .optional(),
});

export type CategoryFormValues = z.infer<typeof categorySchema>;
