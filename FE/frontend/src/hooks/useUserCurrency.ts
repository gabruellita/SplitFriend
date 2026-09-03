import { useAuth } from '@/hooks/useAuth';
import { useCurrencies } from '@/hooks/useCurrencies';

/**
 * Codul monedei preferate a utilizatorului curent (ex. "RON"). Sursă unică
 * pentru „moneda ta" în UI și pentru conversii (useConvert/ConvertedAmount).
 *
 * ⚠️ Atenție: claim-ul JWT `currency` și header-ul `X-User-Currency` conțin
 * ID-ul numeric al monedei (Finance/Statistics îl folosesc ca `currency_id`),
 * NU codul. De aceea rezolvăm codul din `preferredCurrencyId` prin lista de
 * monede, nu din JWT. Întoarce `null` până se încarcă monedele sau dacă nu
 * există user autentificat.
 */
export function useUserCurrency(): string | null {
  const { user } = useAuth();
  const { currencies } = useCurrencies();
  if (!user) return null;
  return currencies.find(c => c.id === user.preferredCurrencyId)?.code ?? null;
}
