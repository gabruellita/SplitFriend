-- ════════════════════════════════════════════════════════════════════════════
-- FINANCE SERVICE — DATABASE SCHEMA (PostgreSQL 16)
-- Aceeasi baza `finance_db` ca Identity Service. Ruleaza DUPA scripturile Identity
-- (vezi ordinea mount-urilor din docker-compose.yml), fiindca face FK la `currencies`.
-- Convenții: snake_case, BIGINT GENERATED ALWAYS AS IDENTITY, TIMESTAMPTZ, ENUM nativ.
-- ════════════════════════════════════════════════════════════════════════════

-- ─── ENUM-uri native ────────────────────────────────────────────────────────
CREATE TYPE transaction_kind     AS ENUM ('INCOME', 'EXPENSE');
CREATE TYPE transaction_status   AS ENUM ('POSTED', 'VOIDED');
CREATE TYPE recurrence_frequency AS ENUM ('DAILY', 'WEEKLY', 'MONTHLY', 'YEARLY');

-- ─── TABELĂ: categories (system + custom per user) ──────────────────────────
CREATE TABLE categories (
    id                 BIGINT           GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name               VARCHAR(100)     NOT NULL,
    kind               transaction_kind NOT NULL,
    icon               VARCHAR(50)      NULL,
    color              VARCHAR(20)      NULL,
    created_by_user_id BIGINT           NULL,            -- NULL = categorie system
    is_system          BOOLEAN          NOT NULL DEFAULT FALSE,
    is_active          BOOLEAN          NOT NULL DEFAULT TRUE,
    created_at         TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
    updated_at         TIMESTAMPTZ      NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_categories_name_length CHECK (LENGTH(TRIM(name)) >= 1)
);

CREATE INDEX idx_categories_user   ON categories(created_by_user_id);
CREATE INDEX idx_categories_active ON categories(is_active) WHERE is_active = TRUE;

CREATE TRIGGER set_categories_updated_at
    BEFORE UPDATE ON categories
    FOR EACH ROW
    EXECUTE FUNCTION trigger_set_timestamp();   -- functie definita de Identity (acelasi DB)

-- Seed — categorii system (is_system=TRUE, created_by_user_id=NULL)
INSERT INTO categories (name, kind, icon, color, is_system) VALUES
    ('Salariu',       'INCOME',  '💰', '#16a34a', TRUE),
    ('Alte venituri', 'INCOME',  '➕', '#22c55e', TRUE),
    ('Mâncare',       'EXPENSE', '🍔', '#ef4444', TRUE),
    ('Transport',     'EXPENSE', '🚌', '#f59e0b', TRUE),
    ('Utilități',     'EXPENSE', '💡', '#3b82f6', TRUE),
    ('Divertisment',  'EXPENSE', '🎬', '#a855f7', TRUE);

-- ─── TABELĂ: recurring_transaction_templates ────────────────────────────────
-- Creata INAINTE de `transactions` fiindca transactions.template_id face FK aici.
CREATE TABLE recurring_transaction_templates (
    id             BIGINT               GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id        BIGINT               NOT NULL,        -- fara FK (apartine Identity)
    category_id    BIGINT               NULL REFERENCES categories(id)  ON DELETE SET NULL,
    amount         NUMERIC(18,2)        NOT NULL CHECK (amount > 0),
    currency_id    BIGINT               NOT NULL REFERENCES currencies(id) ON DELETE RESTRICT,
    kind           transaction_kind     NOT NULL,
    description    VARCHAR(500)         NULL,
    frequency      recurrence_frequency NOT NULL,
    interval_count INTEGER              NOT NULL DEFAULT 1 CHECK (interval_count > 0),
    start_date     DATE                 NOT NULL,
    end_date       DATE                 NULL,
    next_run_date  DATE                 NOT NULL,
    is_active      BOOLEAN              NOT NULL DEFAULT TRUE,
    created_at     TIMESTAMPTZ          NOT NULL DEFAULT NOW(),
    updated_at     TIMESTAMPTZ          NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_rtt_end_after_start CHECK (end_date IS NULL OR end_date >= start_date)
);

CREATE INDEX idx_rtt_due ON recurring_transaction_templates(user_id, next_run_date)
    WHERE is_active = TRUE;

CREATE TRIGGER set_rtt_updated_at
    BEFORE UPDATE ON recurring_transaction_templates
    FOR EACH ROW
    EXECUTE FUNCTION trigger_set_timestamp();

-- ─── TABELĂ: transactions ───────────────────────────────────────────────────
CREATE TABLE transactions (
    id               BIGINT             GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id          BIGINT             NOT NULL,        -- fara FK (apartine Identity)
    category_id      BIGINT             NULL REFERENCES categories(id) ON DELETE SET NULL,
    amount           NUMERIC(18,2)      NOT NULL CHECK (amount > 0),
    currency_id      BIGINT             NOT NULL REFERENCES currencies(id) ON DELETE RESTRICT,
    kind             transaction_kind   NOT NULL,
    description      VARCHAR(500)       NULL,
    transaction_date DATE               NOT NULL,
    status           transaction_status NOT NULL DEFAULT 'POSTED',
    template_id      BIGINT             NULL REFERENCES recurring_transaction_templates(id) ON DELETE SET NULL,
    created_at       TIMESTAMPTZ        NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ        NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_transactions_user_date ON transactions(user_id, transaction_date DESC);
CREATE INDEX idx_transactions_category  ON transactions(category_id);
CREATE INDEX idx_transactions_template  ON transactions(template_id) WHERE template_id IS NOT NULL;

CREATE TRIGGER set_transactions_updated_at
    BEFORE UPDATE ON transactions
    FOR EACH ROW
    EXECUTE FUNCTION trigger_set_timestamp();

-- ─── Comentarii documentare ─────────────────────────────────────────────────
COMMENT ON TABLE categories                      IS 'Categorii tranzacții: system (seed) + custom per user';
COMMENT ON TABLE transactions                    IS 'Tranzacții personale (venituri/cheltuieli), single sau generate din template';
COMMENT ON TABLE recurring_transaction_templates IS 'Template-uri recurente; genereaza tranzactii prin endpoint manual run-due';
COMMENT ON COLUMN transactions.template_id       IS 'NULL = tranzactie single; setat = generata dintr-un template recurent';
COMMENT ON COLUMN transactions.status            IS 'POSTED=activa, VOIDED=anulata (soft delete)';

-- ════════════════════════════════════════════════════════════════════════════
-- SPLIT BILL — GRUPURI, MEMBRI, INVITAȚII, CHELTUIELI, PLĂȚI
-- ════════════════════════════════════════════════════════════════════════════

-- ─── ENUM-uri native ────────────────────────────────────────────────────────
CREATE TYPE group_status   AS ENUM ('ACTIVE', 'ARCHIVED');
CREATE TYPE group_role     AS ENUM ('OWNER', 'MEMBER');
CREATE TYPE member_status  AS ENUM ('INVITED', 'ACTIVE', 'LEFT', 'REMOVED');
CREATE TYPE split_type     AS ENUM ('EQUAL', 'EXACT', 'PERCENT', 'SHARES');
CREATE TYPE expense_status AS ENUM ('OPEN', 'SETTLED', 'CANCELED');

-- ─── TABELĂ: groups ─────────────────────────────────────────────────────────
CREATE TABLE groups (
    id            BIGINT       GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name          VARCHAR(100) NOT NULL,
    description   VARCHAR(500) NULL,
    currency_id   BIGINT       NOT NULL REFERENCES currencies(id) ON DELETE RESTRICT,
    owner_user_id BIGINT       NOT NULL,        -- fara FK (apartine Identity)
    status        group_status NOT NULL DEFAULT 'ACTIVE',
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_groups_name_length CHECK (LENGTH(TRIM(name)) >= 1)
);

CREATE INDEX idx_groups_owner ON groups(owner_user_id);

CREATE TRIGGER set_groups_updated_at
    BEFORE UPDATE ON groups
    FOR EACH ROW
    EXECUTE FUNCTION trigger_set_timestamp();

-- ─── TABELĂ: group_members ──────────────────────────────────────────────────
CREATE TABLE group_members (
    id         BIGINT        GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    group_id   BIGINT        NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    user_id    BIGINT        NOT NULL,          -- fara FK (apartine Identity)
    role       group_role    NOT NULL DEFAULT 'MEMBER',
    status     member_status NOT NULL DEFAULT 'INVITED',
    invited_at TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    joined_at  TIMESTAMPTZ   NULL,
    left_at    TIMESTAMPTZ   NULL,

    CONSTRAINT uq_group_members UNIQUE (group_id, user_id)
);

CREATE INDEX idx_group_members_user ON group_members(user_id) WHERE status = 'ACTIVE';

-- ─── TABELĂ: pending_invitations (invitati FARA cont inca) ───────────────────
CREATE TABLE pending_invitations (
    id          BIGINT       GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    group_id    BIGINT       NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    email       VARCHAR(256) NOT NULL,
    token       VARCHAR(512) NOT NULL UNIQUE,
    expires_at  TIMESTAMPTZ  NOT NULL,
    accepted_at TIMESTAMPTZ  NULL,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_pending_inv_email ON pending_invitations(email) WHERE accepted_at IS NULL;

-- ─── TABELĂ: group_expenses ─────────────────────────────────────────────────
CREATE TABLE group_expenses (
    id              BIGINT         GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    group_id        BIGINT         NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    paid_by_user_id BIGINT         NOT NULL,
    title           VARCHAR(200)   NOT NULL,
    amount          NUMERIC(18,2)  NOT NULL CHECK (amount > 0),
    currency_id     BIGINT         NOT NULL REFERENCES currencies(id) ON DELETE RESTRICT,
    split_type      split_type     NOT NULL,
    status          expense_status NOT NULL DEFAULT 'OPEN',
    expense_date    DATE           NOT NULL,
    created_at      TIMESTAMPTZ    NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ    NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_group_expenses_group ON group_expenses(group_id, created_at DESC);

CREATE TRIGGER set_group_expenses_updated_at
    BEFORE UPDATE ON group_expenses
    FOR EACH ROW
    EXECUTE FUNCTION trigger_set_timestamp();

-- ─── TABELĂ: expense_splits ─────────────────────────────────────────────────
CREATE TABLE expense_splits (
    id                      BIGINT        GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    group_expense_id        BIGINT        NOT NULL REFERENCES group_expenses(id) ON DELETE CASCADE,
    user_id                 BIGINT        NOT NULL,
    owed_amount             NUMERIC(18,2) NOT NULL CHECK (owed_amount >= 0),
    paid_amount             NUMERIC(18,2) NOT NULL DEFAULT 0 CHECK (paid_amount <= owed_amount),
    is_settled              BOOLEAN       NOT NULL DEFAULT FALSE,
    personal_transaction_id BIGINT        NULL REFERENCES transactions(id) ON DELETE SET NULL,

    CONSTRAINT uq_expense_splits UNIQUE (group_expense_id, user_id)
);

CREATE INDEX idx_expense_splits_user ON expense_splits(user_id);

-- ─── TABELĂ: payments (ledger settle-up) ────────────────────────────────────
CREATE TABLE payments (
    id                      BIGINT        GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    group_id                BIGINT        NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    from_user_id            BIGINT        NOT NULL,
    to_user_id              BIGINT        NOT NULL,
    amount                  NUMERIC(18,2) NOT NULL CHECK (amount > 0),   -- in moneda creditorului (alocare FIFO)
    currency_id             BIGINT        NOT NULL REFERENCES currencies(id) ON DELETE RESTRICT, -- moneda creditorului
    original_amount         NUMERIC(18,2) NOT NULL,                      -- cat a dat platitorul in moneda lui
    original_currency_id    BIGINT        NOT NULL REFERENCES currencies(id) ON DELETE RESTRICT, -- moneda platitorului
    exchange_rate           NUMERIC(18,8) NOT NULL,                      -- curs creditor→platitor (1.0 daca aceeasi moneda)
    rate_date               DATE          NOT NULL,
    personal_transaction_id BIGINT        NULL REFERENCES transactions(id) ON DELETE SET NULL,
    payment_method          VARCHAR(50)   NULL,
    paid_at                 TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    created_at              TIMESTAMPTZ   NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_payments_distinct CHECK (from_user_id <> to_user_id)
);

CREATE INDEX idx_payments_group ON payments(group_id);

-- ─── TABELĂ: payment_allocations (N–N payments ↔ expense_splits, FIFO) ───────
CREATE TABLE payment_allocations (
    id               BIGINT        GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    payment_id       BIGINT        NOT NULL REFERENCES payments(id) ON DELETE CASCADE,
    expense_split_id BIGINT        NOT NULL REFERENCES expense_splits(id) ON DELETE CASCADE,
    allocated_amount NUMERIC(18,2) NOT NULL CHECK (allocated_amount > 0)
);

CREATE INDEX idx_payment_alloc_split ON payment_allocations(expense_split_id);

-- ════════════════════════════════════════════════════════════════════════════
-- SPLIT BILL — TRIGGERE
-- ════════════════════════════════════════════════════════════════════════════

-- 1. Auto-settle pe split: is_settled = TRUE cand paid_amount >= owed_amount.
CREATE OR REPLACE FUNCTION trg_fn_expense_split_auto_settle()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    NEW.is_settled := (NEW.paid_amount >= NEW.owed_amount);
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_expense_split_auto_settle
    BEFORE INSERT OR UPDATE OF paid_amount, owed_amount ON expense_splits
    FOR EACH ROW
    EXECUTE FUNCTION trg_fn_expense_split_auto_settle();

-- 2. Auto-settle pe cheltuiala: status='SETTLED' cand TOATE split-urile ei sunt settled.
CREATE OR REPLACE FUNCTION trg_fn_group_expense_auto_settle()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE v_unsettled INTEGER;
BEGIN
    SELECT COUNT(*) INTO v_unsettled
    FROM expense_splits
    WHERE group_expense_id = NEW.group_expense_id AND is_settled = FALSE;

    UPDATE group_expenses
    SET status = (CASE WHEN v_unsettled = 0 THEN 'SETTLED' ELSE 'OPEN' END)::expense_status
    WHERE id = NEW.group_expense_id AND status <> 'CANCELED';

    RETURN NULL;  -- AFTER trigger
END;
$$;

-- Nota: se declanseaza pe ORICE UPDATE (nu doar OF is_settled), fiindca plata
-- actualizeaza paid_amount, iar is_settled e schimbat de un trigger BEFORE — caz
-- in care `UPDATE OF is_settled` NU s-ar declansa (Postgres se uita la clauza SET).
CREATE TRIGGER trg_group_expense_auto_settle
    AFTER INSERT OR UPDATE ON expense_splits
    FOR EACH ROW
    EXECUTE FUNCTION trg_fn_group_expense_auto_settle();

-- ── Multi-valuta: cheltuiala se inregistreaza in moneda platitorului, nu a grupului.
-- Triggerul trg_group_expense_currency_match a fost ELIMINAT (nu mai fortam expense.currency = group.currency).
-- (Pe un volum existent: DROP TRIGGER IF EXISTS trg_group_expense_currency_match ON group_expenses;
--                        DROP FUNCTION IF EXISTS trg_fn_group_expense_currency_match;)

-- 4. La alocarea unei plati: creste paid_amount pe split-ul tinta.
CREATE OR REPLACE FUNCTION trg_fn_payment_apply_allocations()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    UPDATE expense_splits
    SET paid_amount = paid_amount + NEW.allocated_amount
    WHERE id = NEW.expense_split_id;
    RETURN NULL;  -- AFTER trigger
END;
$$;

CREATE TRIGGER trg_payment_apply_allocations
    AFTER INSERT ON payment_allocations
    FOR EACH ROW
    EXECUTE FUNCTION trg_fn_payment_apply_allocations();
