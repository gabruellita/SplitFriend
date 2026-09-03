import { forwardRef, type InputHTMLAttributes } from 'react';
import clsx from 'clsx';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label:   string;
  error?:  string;
  helper?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, helper, className, id, ...rest }, ref) => {
    const inputId = id ?? `input-${label.replace(/\s+/g, '-').toLowerCase()}`;

    return (
      <div className="flex flex-col gap-1 mb-4">
        <label htmlFor={inputId} className="text-sm font-medium text-gray-700">
          {label}
        </label>
        <input
          ref={ref}
          id={inputId}
          aria-invalid={!!error}
          aria-describedby={error ? `${inputId}-err` : undefined}
          className={clsx(
            'px-3 py-2 border rounded-md outline-none transition',
            'focus:ring-2 focus:ring-blue-500',
            error ? 'border-red-500' : 'border-gray-300',
            className
          )}
          {...rest}
        />
        {error && (
          <span id={`${inputId}-err`} role="alert" className="text-xs text-red-600">
            {error}
          </span>
        )}
        {!error && helper && <span className="text-xs text-gray-500">{helper}</span>}
      </div>
    );
  }
);
Input.displayName = 'Input';
