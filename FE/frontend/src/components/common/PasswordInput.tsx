import { forwardRef, useState, type InputHTMLAttributes } from 'react';
import clsx from 'clsx';

interface PasswordInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label:  string;
  error?: string;
}

export const PasswordInput = forwardRef<HTMLInputElement, PasswordInputProps>(
  ({ label, error, className, id, ...rest }, ref) => {
    const [visible, setVisible] = useState(false);
    const inputId = id ?? `pw-${label.replace(/\s+/g, '-').toLowerCase()}`;

    return (
      <div className="flex flex-col gap-1 mb-4">
        <label htmlFor={inputId} className="text-sm font-medium text-gray-700">
          {label}
        </label>
        <div className="relative">
          <input
            ref={ref}
            id={inputId}
            type={visible ? 'text' : 'password'}
            aria-invalid={!!error}
            className={clsx(
              'w-full px-3 py-2 pr-10 border rounded-md outline-none transition',
              'focus:ring-2 focus:ring-blue-500',
              error ? 'border-red-500' : 'border-gray-300',
              className
            )}
            {...rest}
          />
          <button
            type="button"
            onClick={() => setVisible(v => !v)}
            aria-label={visible ? 'Ascunde parola' : 'Arată parola'}
            className="absolute right-2 top-1/2 -translate-y-1/2 text-sm text-gray-600 hover:text-gray-900"
          >
            {visible ? '🙈' : '👁'}
          </button>
        </div>
        {error && <span role="alert" className="text-xs text-red-600">{error}</span>}
      </div>
    );
  }
);
PasswordInput.displayName = 'PasswordInput';
