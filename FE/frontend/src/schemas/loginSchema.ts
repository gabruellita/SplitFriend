import { z } from 'zod';

export const loginSchema = z.object({
  email: z
    .string()
    .min(1, 'Email-ul este obligatoriu.')
    .email('Formatul email-ului este invalid.')
    .max(256, 'Email-ul este prea lung.'),

  password: z
    .string()
    .min(1, 'Parola este obligatorie.')
    .min(8, 'Parola trebuie să aibă minim 8 caractere.'),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
