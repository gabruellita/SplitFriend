import { z } from 'zod';

// Aceleași reguli de parolă ca în registerSchema, pentru consistență.
const password = z
  .string()
  .min(8,   'Parola trebuie să aibă minim 8 caractere.')
  .max(128, 'Parola este prea lungă.')
  .regex(/[A-Z]/,   'Necesită cel puțin o literă mare.')
  .regex(/[a-z]/,   'Necesită cel puțin o literă mică.')
  .regex(/\d/,      'Necesită cel puțin o cifră.')
  .regex(/[\W_]/,   'Necesită cel puțin un caracter special.');

export const profileSchema = z.object({
  firstName:           z.string().max(100).optional().or(z.literal('')),
  lastName:            z.string().max(100).optional().or(z.literal('')),
  preferredCurrencyId: z.number().int().positive(),
});
export type ProfileForm = z.infer<typeof profileSchema>;

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Obligatoriu'),
    newPassword:     password,
    confirm:         z.string(),
  })
  .refine(d => d.newPassword === d.confirm, {
    path:    ['confirm'],
    message: 'Parolele nu coincid',
  })
  .refine(d => d.newPassword !== d.currentPassword, {
    path:    ['newPassword'],
    message: 'Alege o parolă diferită',
  });
export type ChangePasswordForm = z.infer<typeof changePasswordSchema>;

export const forgotPasswordSchema = z.object({
  email: z.string().email('Email invalid'),
});
export type ForgotPasswordForm = z.infer<typeof forgotPasswordSchema>;

export const resetPasswordSchema = z
  .object({
    newPassword: password,
    confirm:     z.string(),
  })
  .refine(d => d.newPassword === d.confirm, {
    path:    ['confirm'],
    message: 'Parolele nu coincid',
  });
export type ResetPasswordForm = z.infer<typeof resetPasswordSchema>;
