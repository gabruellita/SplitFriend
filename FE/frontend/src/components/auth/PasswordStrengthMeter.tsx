interface Props { password: string; }

export const PasswordStrengthMeter: React.FC<Props> = ({ password }) => {
  const calculateStrength = (): number => {
    let score = 0;
    if (password.length >= 8)   score++;
    if (password.length >= 12)  score++;
    if (/[A-Z]/.test(password)) score++;
    if (/\d/.test(password))    score++;
    if (/[\W_]/.test(password)) score++;
    return score;
  };

  const strength = calculateStrength();
  const labels   = ['', 'Foarte slabă', 'Slabă', 'Medie', 'Bună', 'Puternică'];
  const colors   = ['', 'bg-red-500', 'bg-orange-500', 'bg-yellow-500', 'bg-blue-500', 'bg-green-500'];

  if (!password) return null;

  return (
    <div className="mb-3">
      <div className="flex gap-1 h-1">
        {[1, 2, 3, 4, 5].map(i => (
          <div
            key={i}
            className={`flex-1 rounded ${i <= strength ? (colors[strength] ?? '') : 'bg-gray-200'}`}
          />
        ))}
      </div>
      <p className="text-xs mt-1 text-gray-600">Putere: {labels[strength]}</p>
    </div>
  );
};
