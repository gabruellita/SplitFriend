-- ════════════════════════════════════════════════════════════════════════════
-- FINANCE SERVICE — PROCEDURI STOCATE (PostgreSQL 16)
-- Apelate exclusiv prin Dapper cu CommandType.StoredProcedure.
-- ENUM-urile se intorc ::TEXT in SELECT si se accepta ca TEXT cu cast ::enum la INSERT.
-- ════════════════════════════════════════════════════════════════════════════

-- ─── CategoryRepository ─────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION sp_get_categories(p_user_id BIGINT)
RETURNS TABLE(
    id                 BIGINT,
    name               VARCHAR(100),
    kind               TEXT,
    icon               VARCHAR(50),
    color              VARCHAR(20),
    created_by_user_id BIGINT,
    is_system          BOOLEAN,
    is_active          BOOLEAN,
    created_at         TIMESTAMPTZ,
    updated_at         TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT id, name, kind::TEXT, icon, color, created_by_user_id,
           is_system, is_active, created_at, updated_at
    FROM categories
    WHERE is_active = TRUE
      AND (is_system = TRUE OR created_by_user_id = p_user_id)
    ORDER BY kind, name;
$$;

CREATE OR REPLACE FUNCTION sp_get_category_by_id(p_id BIGINT, p_user_id BIGINT)
RETURNS TABLE(
    id                 BIGINT,
    name               VARCHAR(100),
    kind               TEXT,
    icon               VARCHAR(50),
    color              VARCHAR(20),
    created_by_user_id BIGINT,
    is_system          BOOLEAN,
    is_active          BOOLEAN,
    created_at         TIMESTAMPTZ,
    updated_at         TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT id, name, kind::TEXT, icon, color, created_by_user_id,
           is_system, is_active, created_at, updated_at
    FROM categories
    WHERE id = p_id
      AND (is_system = TRUE OR created_by_user_id = p_user_id)
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION sp_create_category(
    p_name    VARCHAR,
    p_kind    TEXT,
    p_icon    VARCHAR,
    p_color   VARCHAR,
    p_user_id BIGINT
)
RETURNS BIGINT
LANGUAGE sql AS $$
    INSERT INTO categories (name, kind, icon, color, created_by_user_id, is_system)
    VALUES (p_name, p_kind::transaction_kind, p_icon, p_color, p_user_id, FALSE)
    RETURNING id;
$$;

CREATE OR REPLACE FUNCTION sp_update_category(
    p_id      BIGINT,
    p_user_id BIGINT,
    p_name    VARCHAR,
    p_icon    VARCHAR,
    p_color   VARCHAR
)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE categories
    SET name = p_name, icon = p_icon, color = p_color
    WHERE id = p_id AND created_by_user_id = p_user_id AND is_system = FALSE;
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

CREATE OR REPLACE FUNCTION sp_deactivate_category(p_id BIGINT, p_user_id BIGINT)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE categories
    SET is_active = FALSE
    WHERE id = p_id AND created_by_user_id = p_user_id AND is_system = FALSE;
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

CREATE OR REPLACE FUNCTION sp_category_valid_for_user(p_id BIGINT, p_user_id BIGINT, p_kind TEXT)
RETURNS BOOLEAN
LANGUAGE sql STABLE AS $$
    SELECT EXISTS(
        SELECT 1 FROM categories
        WHERE id = p_id
          AND is_active = TRUE
          AND (is_system = TRUE OR created_by_user_id = p_user_id)
          AND kind = p_kind::transaction_kind
    );
$$;

-- ─── TransactionRepository ──────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION sp_create_transaction(
    p_user_id          BIGINT,
    p_category_id      BIGINT,
    p_amount           NUMERIC,
    p_currency_id      BIGINT,
    p_kind             TEXT,
    p_description      VARCHAR,
    p_transaction_date DATE,
    p_template_id      BIGINT
)
RETURNS BIGINT
LANGUAGE sql AS $$
    INSERT INTO transactions
        (user_id, category_id, amount, currency_id, kind, description, transaction_date, template_id)
    VALUES
        (p_user_id, p_category_id, p_amount, p_currency_id, p_kind::transaction_kind,
         p_description, p_transaction_date, p_template_id)
    RETURNING id;
$$;

CREATE OR REPLACE FUNCTION sp_get_transactions(
    p_user_id     BIGINT,
    p_from        DATE,
    p_to          DATE,
    p_category_id BIGINT,
    p_kind        TEXT
)
RETURNS TABLE(
    id               BIGINT,
    user_id          BIGINT,
    category_id      BIGINT,
    category_name    VARCHAR(100),
    amount           NUMERIC(18,2),
    currency_id      BIGINT,
    currency_code    VARCHAR(3),
    kind             TEXT,
    description      VARCHAR(500),
    transaction_date DATE,
    status           TEXT,
    template_id      BIGINT,
    created_at       TIMESTAMPTZ,
    updated_at       TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT t.id, t.user_id, t.category_id, c.name AS category_name,
           t.amount, t.currency_id, cur.code AS currency_code,
           t.kind::TEXT, t.description, t.transaction_date, t.status::TEXT,
           t.template_id, t.created_at, t.updated_at
    FROM transactions t
    LEFT JOIN categories c   ON c.id   = t.category_id
    LEFT JOIN currencies cur ON cur.id = t.currency_id
    WHERE t.user_id = p_user_id
      AND t.status = 'POSTED'
      AND (p_from        IS NULL OR t.transaction_date >= p_from)
      AND (p_to          IS NULL OR t.transaction_date <= p_to)
      AND (p_category_id IS NULL OR t.category_id = p_category_id)
      AND (p_kind        IS NULL OR t.kind = p_kind::transaction_kind)
    ORDER BY t.transaction_date DESC, t.id DESC;
$$;

CREATE OR REPLACE FUNCTION sp_get_transaction_by_id(p_id BIGINT, p_user_id BIGINT)
RETURNS TABLE(
    id               BIGINT,
    user_id          BIGINT,
    category_id      BIGINT,
    category_name    VARCHAR(100),
    amount           NUMERIC(18,2),
    currency_id      BIGINT,
    currency_code    VARCHAR(3),
    kind             TEXT,
    description      VARCHAR(500),
    transaction_date DATE,
    status           TEXT,
    template_id      BIGINT,
    created_at       TIMESTAMPTZ,
    updated_at       TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT t.id, t.user_id, t.category_id, c.name AS category_name,
           t.amount, t.currency_id, cur.code AS currency_code,
           t.kind::TEXT, t.description, t.transaction_date, t.status::TEXT,
           t.template_id, t.created_at, t.updated_at
    FROM transactions t
    LEFT JOIN categories c   ON c.id   = t.category_id
    LEFT JOIN currencies cur ON cur.id = t.currency_id
    WHERE t.id = p_id AND t.user_id = p_user_id
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION sp_update_transaction(
    p_id               BIGINT,
    p_user_id          BIGINT,
    p_category_id      BIGINT,
    p_amount           NUMERIC,
    p_currency_id      BIGINT,
    p_kind             TEXT,
    p_description      VARCHAR,
    p_transaction_date DATE
)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE transactions
    SET category_id      = p_category_id,
        amount           = p_amount,
        currency_id      = p_currency_id,
        kind             = p_kind::transaction_kind,
        description      = p_description,
        transaction_date = p_transaction_date
    WHERE id = p_id AND user_id = p_user_id AND status = 'POSTED';
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

CREATE OR REPLACE FUNCTION sp_void_transaction(p_id BIGINT, p_user_id BIGINT)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE transactions
    SET status = 'VOIDED'
    WHERE id = p_id AND user_id = p_user_id AND status = 'POSTED';
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

CREATE OR REPLACE FUNCTION sp_get_summary(p_user_id BIGINT, p_from DATE, p_to DATE)
RETURNS TABLE(
    kind              TEXT,
    category_id       BIGINT,
    category_name     VARCHAR(100),
    total_amount      NUMERIC,
    transaction_count BIGINT
)
LANGUAGE sql STABLE AS $$
    SELECT t.kind::TEXT, t.category_id, c.name AS category_name,
           SUM(t.amount) AS total_amount, COUNT(*) AS transaction_count
    FROM transactions t
    LEFT JOIN categories c ON c.id = t.category_id
    WHERE t.user_id = p_user_id
      AND t.status = 'POSTED'
      AND (p_from IS NULL OR t.transaction_date >= p_from)
      AND (p_to   IS NULL OR t.transaction_date <= p_to)
    GROUP BY t.kind, t.category_id, c.name
    ORDER BY t.kind, total_amount DESC;
$$;

-- ─── RecurringTemplateRepository ────────────────────────────────────────────

CREATE OR REPLACE FUNCTION sp_create_recurring_template(
    p_user_id        BIGINT,
    p_category_id    BIGINT,
    p_amount         NUMERIC,
    p_currency_id    BIGINT,
    p_kind           TEXT,
    p_description    VARCHAR,
    p_frequency      TEXT,
    p_interval_count INTEGER,
    p_start_date     DATE,
    p_end_date       DATE,
    p_next_run_date  DATE
)
RETURNS BIGINT
LANGUAGE sql AS $$
    INSERT INTO recurring_transaction_templates
        (user_id, category_id, amount, currency_id, kind, description,
         frequency, interval_count, start_date, end_date, next_run_date)
    VALUES
        (p_user_id, p_category_id, p_amount, p_currency_id, p_kind::transaction_kind, p_description,
         p_frequency::recurrence_frequency, p_interval_count, p_start_date, p_end_date, p_next_run_date)
    RETURNING id;
$$;

CREATE OR REPLACE FUNCTION sp_get_recurring_templates(p_user_id BIGINT)
RETURNS TABLE(
    id             BIGINT,
    user_id        BIGINT,
    category_id    BIGINT,
    category_name  VARCHAR(100),
    amount         NUMERIC(18,2),
    currency_id    BIGINT,
    currency_code  VARCHAR(3),
    kind           TEXT,
    description    VARCHAR(500),
    frequency      TEXT,
    interval_count INTEGER,
    start_date     DATE,
    end_date       DATE,
    next_run_date  DATE,
    is_active      BOOLEAN,
    created_at     TIMESTAMPTZ,
    updated_at     TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT r.id, r.user_id, r.category_id, c.name AS category_name,
           r.amount, r.currency_id, cur.code AS currency_code,
           r.kind::TEXT, r.description, r.frequency::TEXT, r.interval_count,
           r.start_date, r.end_date, r.next_run_date, r.is_active,
           r.created_at, r.updated_at
    FROM recurring_transaction_templates r
    LEFT JOIN categories c   ON c.id   = r.category_id
    LEFT JOIN currencies cur ON cur.id = r.currency_id
    WHERE r.user_id = p_user_id
    ORDER BY r.is_active DESC, r.next_run_date;
$$;

CREATE OR REPLACE FUNCTION sp_get_recurring_template_by_id(p_id BIGINT, p_user_id BIGINT)
RETURNS TABLE(
    id             BIGINT,
    user_id        BIGINT,
    category_id    BIGINT,
    category_name  VARCHAR(100),
    amount         NUMERIC(18,2),
    currency_id    BIGINT,
    currency_code  VARCHAR(3),
    kind           TEXT,
    description    VARCHAR(500),
    frequency      TEXT,
    interval_count INTEGER,
    start_date     DATE,
    end_date       DATE,
    next_run_date  DATE,
    is_active      BOOLEAN,
    created_at     TIMESTAMPTZ,
    updated_at     TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT r.id, r.user_id, r.category_id, c.name AS category_name,
           r.amount, r.currency_id, cur.code AS currency_code,
           r.kind::TEXT, r.description, r.frequency::TEXT, r.interval_count,
           r.start_date, r.end_date, r.next_run_date, r.is_active,
           r.created_at, r.updated_at
    FROM recurring_transaction_templates r
    LEFT JOIN categories c   ON c.id   = r.category_id
    LEFT JOIN currencies cur ON cur.id = r.currency_id
    WHERE r.id = p_id AND r.user_id = p_user_id
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION sp_update_recurring_template(
    p_id             BIGINT,
    p_user_id        BIGINT,
    p_category_id    BIGINT,
    p_amount         NUMERIC,
    p_currency_id    BIGINT,
    p_kind           TEXT,
    p_description    VARCHAR,
    p_frequency      TEXT,
    p_interval_count INTEGER,
    p_end_date       DATE
)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE recurring_transaction_templates
    SET category_id    = p_category_id,
        amount         = p_amount,
        currency_id    = p_currency_id,
        kind           = p_kind::transaction_kind,
        description    = p_description,
        frequency      = p_frequency::recurrence_frequency,
        interval_count = p_interval_count,
        end_date       = p_end_date
    WHERE id = p_id AND user_id = p_user_id;
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

CREATE OR REPLACE FUNCTION sp_deactivate_recurring_template(p_id BIGINT, p_user_id BIGINT)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE recurring_transaction_templates
    SET is_active = FALSE
    WHERE id = p_id AND user_id = p_user_id;
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

CREATE OR REPLACE FUNCTION sp_get_due_templates(p_user_id BIGINT, p_run_date DATE)
RETURNS TABLE(
    id             BIGINT,
    user_id        BIGINT,
    category_id    BIGINT,
    category_name  VARCHAR(100),
    amount         NUMERIC(18,2),
    currency_id    BIGINT,
    currency_code  VARCHAR(3),
    kind           TEXT,
    description    VARCHAR(500),
    frequency      TEXT,
    interval_count INTEGER,
    start_date     DATE,
    end_date       DATE,
    next_run_date  DATE,
    is_active      BOOLEAN,
    created_at     TIMESTAMPTZ,
    updated_at     TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT r.id, r.user_id, r.category_id, NULL::VARCHAR(100) AS category_name,
           r.amount, r.currency_id, NULL::VARCHAR(3) AS currency_code,
           r.kind::TEXT, r.description, r.frequency::TEXT, r.interval_count,
           r.start_date, r.end_date, r.next_run_date, r.is_active,
           r.created_at, r.updated_at
    FROM recurring_transaction_templates r
    WHERE r.user_id = p_user_id
      AND r.is_active = TRUE
      AND r.next_run_date <= p_run_date
    ORDER BY r.next_run_date;
$$;

-- Varianta globala (toti userii) pentru job-ul de fundal. FOR UPDATE SKIP LOCKED
-- previne dubla-generare daca job-ul si run-due (login) ruleaza in paralel.
-- Nota: LANGUAGE plpgsql + RETURN QUERY fiindca FOR UPDATE nu e permis intr-o functie SQL pura.
CREATE OR REPLACE FUNCTION sp_get_all_due_templates(p_run_date DATE)
RETURNS TABLE(
    id             BIGINT,
    user_id        BIGINT,
    category_id    BIGINT,
    category_name  VARCHAR(100),
    amount         NUMERIC(18,2),
    currency_id    BIGINT,
    currency_code  VARCHAR(3),
    kind           TEXT,
    description    VARCHAR(500),
    frequency      TEXT,
    interval_count INTEGER,
    start_date     DATE,
    end_date       DATE,
    next_run_date  DATE,
    is_active      BOOLEAN,
    created_at     TIMESTAMPTZ,
    updated_at     TIMESTAMPTZ
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.user_id, r.category_id, NULL::VARCHAR(100) AS category_name,
           r.amount, r.currency_id, NULL::VARCHAR(3) AS currency_code,
           r.kind::TEXT, r.description, r.frequency::TEXT, r.interval_count,
           r.start_date, r.end_date, r.next_run_date, r.is_active,
           r.created_at, r.updated_at
    FROM recurring_transaction_templates r
    WHERE r.is_active = TRUE
      AND r.next_run_date <= p_run_date
    ORDER BY r.user_id, r.next_run_date
    FOR UPDATE OF r SKIP LOCKED;
END;
$$;

CREATE OR REPLACE FUNCTION sp_advance_template(
    p_id            BIGINT,
    p_next_run_date DATE,
    p_is_active     BOOLEAN
)
RETURNS VOID
LANGUAGE sql AS $$
    UPDATE recurring_transaction_templates
    SET next_run_date = p_next_run_date,
        is_active     = p_is_active
    WHERE id = p_id;
$$;

-- ════════════════════════════════════════════════════════════════════════════
-- SPLIT BILL — GRUPURI & MEMBERSHIP
-- ════════════════════════════════════════════════════════════════════════════

-- Creeaza grup + inscrie owner-ul ca membru ACTIVE, intr-o singura tranzactie.
CREATE OR REPLACE FUNCTION sp_create_group(
    p_name        VARCHAR,
    p_description VARCHAR,
    p_currency_id BIGINT,
    p_owner_id    BIGINT
)
RETURNS BIGINT
LANGUAGE plpgsql AS $$
DECLARE v_group_id BIGINT;
BEGIN
    INSERT INTO groups (name, description, currency_id, owner_user_id)
    VALUES (p_name, p_description, p_currency_id, p_owner_id)
    RETURNING id INTO v_group_id;

    INSERT INTO group_members (group_id, user_id, role, status, joined_at)
    VALUES (v_group_id, p_owner_id, 'OWNER', 'ACTIVE', NOW());

    RETURN v_group_id;
END;
$$;

-- Grupurile in care userul e membru ACTIVE, cu cod moneda, nr membri si rolul lui.
CREATE OR REPLACE FUNCTION sp_get_groups(p_user_id BIGINT)
RETURNS TABLE(
    id            BIGINT,
    name          VARCHAR(100),
    description   VARCHAR(500),
    currency_id   BIGINT,
    currency_code VARCHAR(3),
    owner_user_id BIGINT,
    status        TEXT,
    member_count  BIGINT,
    my_role       TEXT,
    created_at    TIMESTAMPTZ,
    updated_at    TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT g.id, g.name, g.description, g.currency_id, cur.code AS currency_code,
           g.owner_user_id, g.status::TEXT,
           (SELECT COUNT(*) FROM group_members m2 WHERE m2.group_id = g.id AND m2.status = 'ACTIVE') AS member_count,
           gm.role::TEXT AS my_role,
           g.created_at, g.updated_at
    FROM groups g
    JOIN group_members gm ON gm.group_id = g.id AND gm.user_id = p_user_id AND gm.status = 'ACTIVE'
    LEFT JOIN currencies cur ON cur.id = g.currency_id
    ORDER BY g.created_at DESC;
$$;

-- Un grup dupa id, doar daca userul e membru ACTIVE (altfel 0 randuri → 404 in service).
CREATE OR REPLACE FUNCTION sp_get_group_by_id(p_id BIGINT, p_user_id BIGINT)
RETURNS TABLE(
    id            BIGINT,
    name          VARCHAR(100),
    description   VARCHAR(500),
    currency_id   BIGINT,
    currency_code VARCHAR(3),
    owner_user_id BIGINT,
    status        TEXT,
    member_count  BIGINT,
    my_role       TEXT,
    created_at    TIMESTAMPTZ,
    updated_at    TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT g.id, g.name, g.description, g.currency_id, cur.code AS currency_code,
           g.owner_user_id, g.status::TEXT,
           (SELECT COUNT(*) FROM group_members m2 WHERE m2.group_id = g.id AND m2.status = 'ACTIVE') AS member_count,
           gm.role::TEXT AS my_role,
           g.created_at, g.updated_at
    FROM groups g
    JOIN group_members gm ON gm.group_id = g.id AND gm.user_id = p_user_id AND gm.status = 'ACTIVE'
    LEFT JOIN currencies cur ON cur.id = g.currency_id
    WHERE g.id = p_id
    LIMIT 1;
$$;

-- Rolul userului in grup ('OWNER'/'MEMBER') daca e ACTIVE, altfel NULL.
CREATE OR REPLACE FUNCTION sp_get_group_role(p_group_id BIGINT, p_user_id BIGINT)
RETURNS TEXT
LANGUAGE sql STABLE AS $$
    SELECT role::TEXT FROM group_members
    WHERE group_id = p_group_id AND user_id = p_user_id AND status = 'ACTIVE'
    LIMIT 1;
$$;

-- Boolean apartenenta ACTIVE (folosit si de Chat Service ulterior).
CREATE OR REPLACE FUNCTION sp_is_group_member(p_group_id BIGINT, p_user_id BIGINT)
RETURNS BOOLEAN
LANGUAGE sql STABLE AS $$
    SELECT EXISTS(
        SELECT 1 FROM group_members
        WHERE group_id = p_group_id AND user_id = p_user_id AND status = 'ACTIVE'
    );
$$;

CREATE OR REPLACE FUNCTION sp_update_group(
    p_id          BIGINT,
    p_owner_id    BIGINT,
    p_name        VARCHAR,
    p_description VARCHAR
)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE groups SET name = p_name, description = p_description
    WHERE id = p_id AND owner_user_id = p_owner_id AND status = 'ACTIVE';
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

CREATE OR REPLACE FUNCTION sp_archive_group(p_id BIGINT, p_owner_id BIGINT)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE groups SET status = 'ARCHIVED'
    WHERE id = p_id AND owner_user_id = p_owner_id AND status = 'ACTIVE';
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

-- Membrii grupului cu date din `users` (aceeasi baza finance_db).
CREATE OR REPLACE FUNCTION sp_get_group_members(p_group_id BIGINT)
RETURNS TABLE(
    user_id    BIGINT,
    email      VARCHAR(256),
    username   VARCHAR(100),
    first_name VARCHAR(100),
    last_name  VARCHAR(100),
    role       TEXT,
    status     TEXT,
    joined_at  TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT gm.user_id, u.email, u.username, u.first_name, u.last_name,
           gm.role::TEXT, gm.status::TEXT, gm.joined_at
    FROM group_members gm
    LEFT JOIN users u ON u.id = gm.user_id
    WHERE gm.group_id = p_group_id AND gm.status IN ('ACTIVE','INVITED')
    ORDER BY gm.role, u.username;
$$;

-- ─── SPLIT BILL — INVITAȚII ─────────────────────────────────────────────────

-- Cauta id-ul unui user dupa email (lowercase). NULL daca nu exista cont.
CREATE OR REPLACE FUNCTION sp_find_user_id_by_email(p_email VARCHAR)
RETURNS BIGINT
LANGUAGE sql STABLE AS $$
    SELECT id FROM users WHERE email = LOWER(TRIM(p_email)) LIMIT 1;
$$;

-- Statusul membership-ului userului in grup (NULL daca nu exista rand deloc).
CREATE OR REPLACE FUNCTION sp_get_member_status(p_group_id BIGINT, p_user_id BIGINT)
RETURNS TEXT
LANGUAGE sql STABLE AS $$
    SELECT status::TEXT FROM group_members
    WHERE group_id = p_group_id AND user_id = p_user_id
    LIMIT 1;
$$;

-- Invita un user EXISTENT: insereaza (sau reactiveaza la INVITED) randul de membership.
CREATE OR REPLACE FUNCTION sp_invite_existing_user(p_group_id BIGINT, p_user_id BIGINT)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    INSERT INTO group_members (group_id, user_id, role, status)
    VALUES (p_group_id, p_user_id, 'MEMBER', 'INVITED')
    ON CONFLICT (group_id, user_id) DO UPDATE
        SET status     = 'INVITED',
            invited_at = NOW(),
            left_at    = NULL
        WHERE group_members.status IN ('LEFT', 'REMOVED');
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

-- Creeaza o invitatie pending pentru un email FARA cont. Intoarce id-ul invitatiei.
CREATE OR REPLACE FUNCTION sp_create_pending_invitation(
    p_group_id   BIGINT,
    p_email      VARCHAR,
    p_token      VARCHAR,
    p_expires_at TIMESTAMPTZ
)
RETURNS BIGINT
LANGUAGE sql AS $$
    INSERT INTO pending_invitations (group_id, email, token, expires_at)
    VALUES (p_group_id, LOWER(TRIM(p_email)), p_token, p_expires_at)
    RETURNING id;
$$;

-- Accepta invitatia: INVITED → ACTIVE. Intoarce nr randuri (0 daca nu era INVITED).
CREATE OR REPLACE FUNCTION sp_accept_invitation(p_group_id BIGINT, p_user_id BIGINT)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE group_members
    SET status = 'ACTIVE', joined_at = NOW()
    WHERE group_id = p_group_id AND user_id = p_user_id AND status = 'INVITED';
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

-- Pleaca din grup: ACTIVE → LEFT. Intoarce nr randuri.
CREATE OR REPLACE FUNCTION sp_leave_group(p_group_id BIGINT, p_user_id BIGINT)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE group_members
    SET status = 'LEFT', left_at = NOW()
    WHERE group_id = p_group_id AND user_id = p_user_id AND status = 'ACTIVE' AND role <> 'OWNER';
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

-- Suma neachitata a userului in grup (owed − paid pe split-uri ne-CANCELED).
CREATE OR REPLACE FUNCTION sp_get_user_unsettled(p_group_id BIGINT, p_user_id BIGINT)
RETURNS NUMERIC
LANGUAGE sql STABLE AS $$
    SELECT COALESCE(SUM(es.owed_amount - es.paid_amount), 0)
    FROM expense_splits es
    JOIN group_expenses ge ON ge.id = es.group_expense_id
    WHERE ge.group_id = p_group_id AND es.user_id = p_user_id
      AND ge.status <> 'CANCELED';
$$;

-- ─── SPLIT BILL — CHELTUIELI ────────────────────────────────────────────────

-- Creeaza cheltuiala + split-uri + tranzactie personala DOAR pentru platitor, tranzactional.
-- Debitorii nu primesc tranzactie personala la creare — aceasta se genereaza la plata, in moneda lor.
-- p_splits: JSONB array, ex: '[{"user_id":1,"owed_amount":50.00},{"user_id":2,"owed_amount":50.00}]'
CREATE OR REPLACE FUNCTION sp_create_group_expense(
    p_group_id     BIGINT,
    p_paid_by      BIGINT,
    p_title        VARCHAR,
    p_amount       NUMERIC,
    p_currency_id  BIGINT,
    p_split_type   TEXT,
    p_expense_date DATE,
    p_splits       JSONB
)
RETURNS BIGINT
LANGUAGE plpgsql AS $$
DECLARE
    v_expense_id BIGINT;
    v_split      JSONB;
    v_user_id    BIGINT;
    v_owed       NUMERIC(18,2);
    v_tx_id      BIGINT;
BEGIN
    INSERT INTO group_expenses (group_id, paid_by_user_id, title, amount, currency_id, split_type, expense_date)
    VALUES (p_group_id, p_paid_by, p_title, p_amount, p_currency_id, p_split_type::split_type, p_expense_date)
    RETURNING id INTO v_expense_id;

    FOR v_split IN SELECT * FROM jsonb_array_elements(p_splits)
    LOOP
        v_user_id := (v_split->>'user_id')::BIGINT;
        v_owed    := (v_split->>'owed_amount')::NUMERIC;

        IF v_user_id = p_paid_by THEN
            -- Platitorul: tranzactie personala pe cota lui, in moneda cheltuielii (= moneda lui). Split deja achitat.
            INSERT INTO transactions (user_id, category_id, amount, currency_id, kind, description, transaction_date, status)
            VALUES (v_user_id, NULL, v_owed, p_currency_id, 'EXPENSE', p_title, p_expense_date, 'POSTED')
            RETURNING id INTO v_tx_id;

            INSERT INTO expense_splits (group_expense_id, user_id, owed_amount, paid_amount, personal_transaction_id)
            VALUES (v_expense_id, v_user_id, v_owed, v_owed, v_tx_id);
        ELSE
            -- Debitorii: NICIO tranzactie personala inca (se creeaza la plata, in moneda lor).
            INSERT INTO expense_splits (group_expense_id, user_id, owed_amount, paid_amount, personal_transaction_id)
            VALUES (v_expense_id, v_user_id, v_owed, 0, NULL);
        END IF;
    END LOOP;

    RETURN v_expense_id;
END;
$$;

CREATE OR REPLACE FUNCTION sp_get_group_expenses(p_group_id BIGINT)
RETURNS TABLE(
    id              BIGINT,
    group_id        BIGINT,
    paid_by_user_id BIGINT,
    title           VARCHAR(200),
    amount          NUMERIC(18,2),
    currency_id     BIGINT,
    currency_code   VARCHAR(3),
    split_type      TEXT,
    status          TEXT,
    expense_date    DATE,
    created_at      TIMESTAMPTZ,
    updated_at      TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT ge.id, ge.group_id, ge.paid_by_user_id, ge.title, ge.amount,
           ge.currency_id, cur.code AS currency_code, ge.split_type::TEXT, ge.status::TEXT,
           ge.expense_date, ge.created_at, ge.updated_at
    FROM group_expenses ge
    LEFT JOIN currencies cur ON cur.id = ge.currency_id
    WHERE ge.group_id = p_group_id
    ORDER BY ge.created_at DESC;
$$;

CREATE OR REPLACE FUNCTION sp_get_group_expense_by_id(p_id BIGINT, p_group_id BIGINT)
RETURNS TABLE(
    id              BIGINT,
    group_id        BIGINT,
    paid_by_user_id BIGINT,
    title           VARCHAR(200),
    amount          NUMERIC(18,2),
    currency_id     BIGINT,
    currency_code   VARCHAR(3),
    split_type      TEXT,
    status          TEXT,
    expense_date    DATE,
    created_at      TIMESTAMPTZ,
    updated_at      TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT ge.id, ge.group_id, ge.paid_by_user_id, ge.title, ge.amount,
           ge.currency_id, cur.code AS currency_code, ge.split_type::TEXT, ge.status::TEXT,
           ge.expense_date, ge.created_at, ge.updated_at
    FROM group_expenses ge
    LEFT JOIN currencies cur ON cur.id = ge.currency_id
    WHERE ge.id = p_id AND ge.group_id = p_group_id
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION sp_get_expense_splits(p_expense_id BIGINT)
RETURNS TABLE(
    user_id     BIGINT,
    owed_amount NUMERIC(18,2),
    paid_amount NUMERIC(18,2),
    is_settled  BOOLEAN
)
LANGUAGE sql STABLE AS $$
    SELECT user_id, owed_amount, paid_amount, is_settled
    FROM expense_splits
    WHERE group_expense_id = p_expense_id
    ORDER BY user_id;
$$;

-- Anuleaza cheltuiala: status CANCELED + VOID la tranzactiile personale legate.
CREATE OR REPLACE FUNCTION sp_cancel_group_expense(p_id BIGINT, p_group_id BIGINT)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE group_expenses SET status = 'CANCELED'
    WHERE id = p_id AND group_id = p_group_id AND status <> 'CANCELED';
    GET DIAGNOSTICS v_rows = ROW_COUNT;

    IF v_rows > 0 THEN
        UPDATE transactions SET status = 'VOIDED'
        WHERE id IN (
            SELECT personal_transaction_id FROM expense_splits
            WHERE group_expense_id = p_id AND personal_transaction_id IS NOT NULL
        );
    END IF;

    RETURN v_rows;
END;
$$;

-- ─── SPLIT BILL — SOLDURI ───────────────────────────────────────────────────

-- Solduri per (user, moneda) — multi-valuta: fiecare moneda are propriul rand de sold.
-- net_amount > 0: userul a platit mai mult decat datoreaza (creditor net in moneda respectiva).
-- net_amount < 0: userul datoreaza mai mult decat a platit (debitor net in moneda respectiva).
CREATE OR REPLACE FUNCTION sp_get_group_balances(p_group_id BIGINT)
RETURNS TABLE(
    user_id       BIGINT,
    username      VARCHAR(100),
    currency_id   BIGINT,
    currency_code VARCHAR(3),
    net_amount    NUMERIC(18,2)
)
LANGUAGE sql STABLE AS $$
    WITH members AS (
        SELECT gm.user_id, u.username
        FROM group_members gm
        LEFT JOIN users u ON u.id = gm.user_id
        WHERE gm.group_id = p_group_id AND gm.status = 'ACTIVE'
    ),
    paid AS (
        SELECT paid_by_user_id AS user_id, currency_id, SUM(amount) AS total
        FROM group_expenses
        WHERE group_id = p_group_id AND status <> 'CANCELED'
        GROUP BY paid_by_user_id, currency_id
    ),
    owed AS (
        SELECT es.user_id, ge.currency_id, SUM(es.owed_amount) AS total
        FROM expense_splits es
        JOIN group_expenses ge ON ge.id = es.group_expense_id
        WHERE ge.group_id = p_group_id AND ge.status <> 'CANCELED'
        GROUP BY es.user_id, ge.currency_id
    ),
    pay_made AS (
        SELECT from_user_id AS user_id, currency_id, SUM(amount) AS total
        FROM payments WHERE group_id = p_group_id GROUP BY from_user_id, currency_id
    ),
    pay_recv AS (
        SELECT to_user_id AS user_id, currency_id, SUM(amount) AS total
        FROM payments WHERE group_id = p_group_id GROUP BY to_user_id, currency_id
    ),
    keys AS (
        SELECT user_id, currency_id FROM paid
        UNION SELECT user_id, currency_id FROM owed
        UNION SELECT user_id, currency_id FROM pay_made
        UNION SELECT user_id, currency_id FROM pay_recv
    )
    SELECT k.user_id, m.username, k.currency_id, c.code,
           ( COALESCE(p.total,0) - COALESCE(o.total,0)
           + COALESCE(pm.total,0) - COALESCE(pr.total,0) )::NUMERIC(18,2) AS net_amount
    FROM keys k
    JOIN members m        ON m.user_id   = k.user_id
    JOIN currencies c     ON c.id        = k.currency_id
    LEFT JOIN paid p      ON p.user_id   = k.user_id AND p.currency_id   = k.currency_id
    LEFT JOIN owed o      ON o.user_id   = k.user_id AND o.currency_id   = k.currency_id
    LEFT JOIN pay_made pm ON pm.user_id  = k.user_id AND pm.currency_id  = k.currency_id
    LEFT JOIN pay_recv pr ON pr.user_id  = k.user_id AND pr.currency_id  = k.currency_id
    WHERE ( COALESCE(p.total,0) - COALESCE(o.total,0)
          + COALESCE(pm.total,0) - COALESCE(pr.total,0) ) <> 0
    ORDER BY k.user_id, k.currency_id;
$$;

-- ─── SPLIT BILL — SETTLE-UP (PLĂȚI + ALOCARE FIFO) ──────────────────────────

-- Returneaza moneda principala a creditorului + suma ramasa de achitat pentru perechea (from→to).
-- Folosit de frontend pentru a sti in ce moneda sa trimita plata si cursul implicit.
CREATE OR REPLACE FUNCTION sp_get_creditor_currency(
    p_group_id  BIGINT,
    p_from_user BIGINT,
    p_to_user   BIGINT
)
RETURNS TABLE(currency_id BIGINT, currency_code VARCHAR(3), remaining_owed NUMERIC(18,2))
LANGUAGE sql STABLE AS $$
    SELECT ge.currency_id, c.code,
           COALESCE(SUM(es.owed_amount - es.paid_amount), 0)::NUMERIC(18,2)
    FROM expense_splits es
    JOIN group_expenses ge ON ge.id = es.group_expense_id
    JOIN currencies c      ON c.id = ge.currency_id
    WHERE ge.group_id = p_group_id
      AND ge.status <> 'CANCELED'
      AND ge.paid_by_user_id = p_to_user
      AND es.user_id = p_from_user
      AND es.is_settled = FALSE
    GROUP BY ge.currency_id, c.code
    ORDER BY ge.currency_id
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION sp_create_payment(
    p_group_id             BIGINT,
    p_from_user            BIGINT,
    p_to_user              BIGINT,
    p_amount               NUMERIC,
    p_currency_id          BIGINT,
    p_original_amount      NUMERIC,
    p_original_currency_id BIGINT,
    p_exchange_rate        NUMERIC,
    p_rate_date            DATE,
    p_method               VARCHAR
)
RETURNS BIGINT
LANGUAGE plpgsql AS $$
DECLARE
    v_payment_id BIGINT;
    v_remaining  NUMERIC(18,2) := p_amount;
    v_split      RECORD;
    v_alloc      NUMERIC(18,2);
    v_tx_id      BIGINT;
BEGIN
    -- Tranzactia personala a debitorului: EXPENSE in moneda LUI, la cursul de azi.
    INSERT INTO transactions (user_id, category_id, amount, currency_id, kind, description, transaction_date, status)
    VALUES (p_from_user, NULL, p_original_amount, p_original_currency_id, 'EXPENSE',
            'Decontare grup', p_rate_date, 'POSTED')
    RETURNING id INTO v_tx_id;

    INSERT INTO payments (group_id, from_user_id, to_user_id, amount, currency_id,
                          original_amount, original_currency_id, exchange_rate, rate_date,
                          personal_transaction_id, payment_method)
    VALUES (p_group_id, p_from_user, p_to_user, p_amount, p_currency_id,
            p_original_amount, p_original_currency_id, p_exchange_rate, p_rate_date,
            v_tx_id, p_method)
    RETURNING id INTO v_payment_id;

    -- Split-urile lui `from`, neachitate, pe cheltuieli platite de `to`, FIFO (cronologic).
    FOR v_split IN
        SELECT es.id, (es.owed_amount - es.paid_amount) AS remaining_owed
        FROM expense_splits es
        JOIN group_expenses ge ON ge.id = es.group_expense_id
        WHERE ge.group_id = p_group_id
          AND ge.status <> 'CANCELED'
          AND ge.paid_by_user_id = p_to_user
          AND es.user_id = p_from_user
          AND es.is_settled = FALSE
          AND ge.currency_id = p_currency_id   -- aloca doar pe split-uri in moneda creditorului (p_amount e in moneda asta)
        ORDER BY ge.created_at, es.id
    LOOP
        EXIT WHEN v_remaining <= 0;
        v_alloc := LEAST(v_remaining, v_split.remaining_owed);
        IF v_alloc > 0 THEN
            INSERT INTO payment_allocations (payment_id, expense_split_id, allocated_amount)
            VALUES (v_payment_id, v_split.id, v_alloc);   -- triggerul creste paid_amount + auto-settle
            v_remaining := v_remaining - v_alloc;
        END IF;
    END LOOP;

    RETURN v_payment_id;
END;
$$;

CREATE OR REPLACE FUNCTION sp_get_payments(p_group_id BIGINT)
RETURNS TABLE(
    id                     BIGINT,
    group_id               BIGINT,
    from_user_id           BIGINT,
    to_user_id             BIGINT,
    amount                 NUMERIC(18,2),
    currency_id            BIGINT,
    currency_code          VARCHAR(3),
    original_amount        NUMERIC(18,2),
    original_currency_id   BIGINT,
    original_currency_code VARCHAR(3),
    exchange_rate          NUMERIC(18,8),
    rate_date              DATE,
    payment_method         VARCHAR(50),
    paid_at                TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT p.id, p.group_id, p.from_user_id, p.to_user_id, p.amount,
           p.currency_id, c1.code, p.original_amount, p.original_currency_id, c2.code,
           p.exchange_rate, p.rate_date, p.payment_method, p.paid_at
    FROM payments p
    LEFT JOIN currencies c1 ON c1.id = p.currency_id
    LEFT JOIN currencies c2 ON c2.id = p.original_currency_id
    WHERE p.group_id = p_group_id
    ORDER BY p.paid_at DESC;
$$;

-- ─── CurrencyLookupRepository ────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION sp_get_currency_code(p_id BIGINT)
RETURNS VARCHAR(3)
LANGUAGE sql STABLE AS $$ SELECT code FROM currencies WHERE id = p_id LIMIT 1; $$;
