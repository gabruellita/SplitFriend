import { type ButtonHTMLAttributes } from 'react';
import clsx from 'clsx';
import { Spinner } from './Spinner';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?:   'primary' | 'secondary' | 'danger';
  loading?:   boolean;
  fullWidth?: boolean;
}

export const Button: React.FC<ButtonProps> = ({
  variant   = 'primary',
  loading   = false,
  fullWidth = false,
  disabled,
  children,
  className,
  ...rest
}) => {
  const variantClasses = {
    primary:   'bg-blue-600 hover:bg-blue-700 text-white',
    secondary: 'bg-gray-200 hover:bg-gray-300 text-gray-800',
    danger:    'bg-red-600 hover:bg-red-700 text-white',
  };

  return (
    <button
      disabled={disabled || loading}
      className={clsx(
        'px-4 py-2 rounded-md font-medium transition flex items-center justify-center gap-2',
        'disabled:opacity-50 disabled:cursor-not-allowed',
        fullWidth && 'w-full',
        variantClasses[variant],
        className
      )}
      {...rest}
    >
      {loading && <Spinner size="sm" />}
      {children}
    </button>
  );
};
