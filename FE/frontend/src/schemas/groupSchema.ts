// src/schemas/groupSchema.ts
import { z } from 'zod';

export const groupSchema = z.object({
  name:        z.string().trim().min(2, 'Minim 2 caractere').max(120, 'Maxim 120 caractere'),
  description: z.string().trim().max(500, 'Maxim 500 caractere').optional().or(z.literal('')),
  currencyId:  z.number({ message: 'Alege o monedă' }).int().positive('Alege o monedă'),
});

export type GroupFormValues = z.infer<typeof groupSchema>;
