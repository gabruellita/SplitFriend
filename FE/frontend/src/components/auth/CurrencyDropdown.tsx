import { forwardRef, type SelectHTMLAttributes } from 'react';
import clsx from 'clsx';
import { useCurrencies } from '@/hooks/useCurrencies';

interface CurrencyDropdownProps extends Omit<SelectHTMLAttributes<HTMLSelectElement>, 'children'> {
  label:  string;
  error?: string;
}

export const CurrencyDropdown = forwardRef<HTMLSelectElement, CurrencyDropdownProps>(
  ({ label, error, className, id, ...rest }, ref) => {
    const { currencies, isLoading, error: loadError } = useCurrencies();
    const selectId = id ?? 'currency-dropdown';

    return (
      <div className="flex flex-col gap-1 mb-4">
        <label htmlFor={selectId} className="text-sm font-medium text-gray-700">
          {label}
        </label>
        <select
          ref={ref}
          id={selectId}
          disabled={isLoading || !!loadError}
          aria-invalid={!!error}
          className={clsx(
            'px-3 py-2 border rounded-md outline-none focus:ring-2 focus:ring-blue-500',
            error ? 'border-red-500' : 'border-gray-300',
            className
          )}
          {...rest}
        >
          <option value="">
            {isLoading ? 'Se încarcă monedele...' : '-- Alege moneda preferată --'}
          </option>
          {currencies.map(c => (
            <option key={c.id} value={c.id}>
              {c.symbol} — {c.name} ({c.code})
            </option>
          ))}
        </select>
        {error     && <span role="alert" className="text-xs text-red-600">{error}</span>}
        {loadError && <span role="alert" className="text-xs text-red-600">{loadError}</span>}
      </div>
    );
  }
);
CurrencyDropdown.displayName = 'CurrencyDropdown';
