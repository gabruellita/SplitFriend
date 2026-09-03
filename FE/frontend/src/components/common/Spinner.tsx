import clsx from 'clsx';

interface SpinnerProps {
  size?: 'sm' | 'md' | 'lg';
}

const sizeClasses = {
  sm: 'h-4 w-4 border-2',
  md: 'h-8 w-8 border-2',
  lg: 'h-12 w-12 border-4',
};

export const Spinner: React.FC<SpinnerProps> = ({ size = 'md' }) => (
  <div className="flex items-center justify-center">
    <div
      role="status"
      aria-label="Se încarcă..."
      className={clsx(
        'animate-spin rounded-full border-gray-300 border-t-blue-600',
        sizeClasses[size]
      )}
    />
  </div>
);
