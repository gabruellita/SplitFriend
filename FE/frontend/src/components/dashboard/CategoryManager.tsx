import { useState } from 'react';
import { Plus, Trash2, Lock } from 'lucide-react';
import type { Category, TransactionKind } from '@/types/finance.types';
import { useCategories } from '@/hooks/useCategories';
import { Button } from '@/components/common/Button';
import { Alert } from '@/components/common/Alert';
import { ConfirmDialog } from './ConfirmDialog';

interface CategoryManagerProps {
  kind: TransactionKind;
  /** Notifica parintele ca lista s-a schimbat (ca sa-si reincarce dropdown-ul). */
  onChange?: () => void;
}

export const CategoryManager: React.FC<CategoryManagerProps> = ({ kind, onChange }) => {
  const { categories, isLoading, error, createCategory, deleteCategory } = useCategories(kind);
  const [name, setName] = useState('');
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [toDelete, setToDelete] = useState<Category | null>(null);
  const [deleting, setDeleting] = useState(false);

  const add = async () => {
    const trimmed = name.trim();
    if (!trimmed) return;
    setSaving(true);
    setFormError(null);
    try {
      await createCategory({ name: trimmed, kind });
      setName('');
      onChange?.();
    } catch {
      setFormError('Nu s-a putut crea categoria. Verifică dacă numele e unic.');
    } finally {
      setSaving(false);
    }
  };

  const confirmDelete = async () => {
    if (!toDelete) return;
    setDeleting(true);
    try {
      await deleteCategory(toDelete.id);
      setToDelete(null);
      onChange?.();
    } finally {
      setDeleting(false);
    }
  };

  return (
    <div>
      {error && <Alert type="error" message={error} />}
      {formError && <Alert type="error" message={formError} onClose={() => setFormError(null)} />}

      <div className="mb-4 flex gap-2">
        <input
          type="text"
          value={name}
          onChange={e => setName(e.target.value)}
          onKeyDown={e => { if (e.key === 'Enter') void add(); }}
          placeholder="Nume categorie nouă"
          maxLength={100}
          className="flex-1 rounded-md border border-slate-300 px-3 py-2 text-sm outline-none transition focus:ring-2 focus:ring-brand-500"
        />
        <Button onClick={() => void add()} loading={saving} disabled={!name.trim()}>
          <Plus className="h-4 w-4" /> Adaugă
        </Button>
      </div>

      {isLoading ? (
        <p className="text-sm text-slate-500">Se încarcă…</p>
      ) : categories.length === 0 ? (
        <p className="text-sm text-slate-500">Nicio categorie.</p>
      ) : (
        <ul className="divide-y divide-slate-900/5">
          {categories.map(c => (
            <li key={c.id} className="flex items-center justify-between py-2 text-sm">
              <span className="text-slate-800">{c.name}</span>
              {c.isSystem ? (
                <span className="inline-flex items-center gap-1 text-xs text-slate-400">
                  <Lock className="h-3.5 w-3.5" /> sistem
                </span>
              ) : (
                <button type="button" onClick={() => setToDelete(c)} aria-label={`Șterge ${c.name}`}
                  className="rounded-lg p-1.5 text-slate-500 hover:bg-rose-50 hover:text-rose-600 cursor-pointer">
                  <Trash2 className="h-4 w-4" />
                </button>
              )}
            </li>
          ))}
        </ul>
      )}

      <ConfirmDialog
        open={!!toDelete}
        title="Ștergere categorie"
        message={`Sigur vrei să ștergi categoria „${toDelete?.name}"? Tranzacțiile existente rămân, dar fără această categorie.`}
        loading={deleting}
        onConfirm={() => void confirmDelete()}
        onCancel={() => setToDelete(null)}
      />
    </div>
  );
};
