import clsx from 'clsx';

interface AlertProps {
  type:     'success' | 'error' | 'warning' | 'info';
  message:  string;
  onClose?: () => void;
}

export const Alert: React.FC<AlertProps> = ({ type, message, onClose }) => {
  const classes = {
    success: 'bg-green-50  border-green-300  text-green-800',
    error:   'bg-red-50    border-red-300    text-red-800',
    warning: 'bg-yellow-50 border-yellow-300 text-yellow-800',
    info:    'bg-blue-50   border-blue-300   text-blue-800',
  };

  return (
    <div
      role="alert"
      className={clsx('border rounded-md p-3 mb-4 flex justify-between items-start', classes[type])}
    >
      <span className="text-sm">{message}</span>
      {onClose && (
        <button onClick={onClose} aria-label="Închide" className="ml-2 text-lg leading-none">
          ×
        </button>
      )}
    </div>
  );
};
