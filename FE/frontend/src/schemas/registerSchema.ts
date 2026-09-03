import { z } from 'zod';

export const registerSchema = z.object({
  email: z
    .string()
    .min(1,   'Email-ul este obligatoriu.')
    .email(   'Formatul email-ului este invalid.')
    .max(256, 'Email-ul este prea lung.'),

  username: z
    .string()
    .min(3,   'Username-ul trebuie să aibă minim 3 caractere.')
    .max(100, 'Username-ul este prea lung.')
    .regex(/^[a-zA-Z0-9_-]+$/, 'Doar litere, cifre, _ și -.'),

  password: z
    .string()
    .min(8,   'Parola trebuie să aibă minim 8 caractere.')
    .max(128, 'Parola este prea lungă.')
    .regex(/[A-Z]/, 'Necesită cel puțin o literă mare.')
    .regex(/[a-z]/, 'Necesită cel puțin o literă mică.')
    .regex(/\d/,    'Necesită cel puțin o cifră.')
    .regex(/[\W_]/, 'Necesită cel puțin un caracter special.'),

  confirmPassword: z.string(),

  firstName: z
    .string()
    .max(100, 'Prenumele este prea lung.')
    .optional(),

  lastName: z
    .string()
    .max(100, 'Numele este prea lung.')
    .optional(),

  // Zod v4: use `error` (replaces v3 `required_error` / `invalid_type_error`)
  preferredCurrencyId: z
    .number({ error: 'Selectează o monedă.' })
    .int('ID monedă invalid.')
    .positive('ID monedă invalid.'),

  acceptTerms: z
    .boolean()
    .refine(v => v === true, 'Trebuie să accepți termenii.'),
})
  .refine(data => data.password === data.confirmPassword, {
    message: 'Parolele nu coincid.',
    path:    ['confirmPassword'],
  });

export type RegisterFormValues = z.infer<typeof registerSchema>;
