-- ════════════════════════════════════════════════════════════════════════════
-- IDENTITY SERVICE — DATABASE SCHEMA (PostgreSQL 16)
-- Toate ID-urile sunt BIGINT GENERATED ALWAYS AS IDENTITY. Niciun UUID.
-- Convenție: snake_case pentru tabele și coloane (standard PostgreSQL).
-- ════════════════════════════════════════════════════════════════════════════

-- ─── ENUM nativ pentru status utilizator ────────────────────────────────────
CREATE TYPE user_status AS ENUM ('ACTIVE', 'INACTIVE', 'PENDING');

-- ─── TABELĂ: currencies (nomenclator monede ISO 4217) ───────────────────────
CREATE TABLE currencies (
    id         BIGINT       GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code       VARCHAR(3)   NOT NULL UNIQUE,         -- ISO 4217: RON, USD, EUR
    name       VARCHAR(100) NOT NULL,
    symbol     VARCHAR(10)  NOT NULL,
    is_active  BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_currencies_code_length CHECK (LENGTH(TRIM(code)) = 3),
    CONSTRAINT chk_currencies_code_upper  CHECK (code = UPPER(code))
);

CREATE INDEX idx_currencies_active ON currencies(is_active) WHERE is_active = TRUE;

-- Date inițiale (seed) — 5 monede principale
INSERT INTO currencies (code, name, symbol) VALUES
    ('RON', 'Leu Românesc',   'lei'),
    ('EUR', 'Euro',           '€'),
    ('USD', 'Dolar American', '$'),
    ('GBP', 'Liră Sterlină',  '£'),
    ('CHF', 'Franc Elvețian', 'CHF');

-- ─── TABELĂ: users ──────────────────────────────────────────────────────────
CREATE TABLE users (
    id                        BIGINT       GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email                     VARCHAR(256) NOT NULL UNIQUE,
    username                  VARCHAR(100) NOT NULL UNIQUE,
    password_hash             VARCHAR(512) NOT NULL,            -- BCrypt hash
    first_name                VARCHAR(100) NULL,
    last_name                 VARCHAR(100) NULL,
    status                    user_status  NOT NULL DEFAULT 'PENDING',
    preferred_currency_id     BIGINT       NOT NULL,
    email_confirmation_token  VARCHAR(512) NULL,
    email_confirmed_at        TIMESTAMPTZ  NULL,
    last_login_at             TIMESTAMPTZ  NULL,
    failed_login_attempts     INTEGER      NOT NULL DEFAULT 0,
    created_at                TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at                TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_users_currency
        FOREIGN KEY (preferred_currency_id)
        REFERENCES currencies(id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,

    CONSTRAINT chk_users_email_format
        CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),

    CONSTRAINT chk_users_email_lowercase
        CHECK (email = LOWER(email)),

    CONSTRAINT chk_users_username_length
        CHECK (LENGTH(TRIM(username)) >= 3)
);

-- Indecși pentru performanță
CREATE INDEX idx_users_email    ON users(email);
CREATE INDEX idx_users_status   ON users(status) WHERE status = 'ACTIVE';
CREATE INDEX idx_users_currency ON users(preferred_currency_id);
CREATE INDEX idx_users_token    ON users(email_confirmation_token)
    WHERE email_confirmation_token IS NOT NULL;

-- ─── TRIGGER: auto-update updated_at ────────────────────────────────────────
CREATE OR REPLACE FUNCTION trigger_set_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER set_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION trigger_set_timestamp();

-- ─── SECVENȚĂ: familie de refresh tokens (reuse-detection) ───────────────────
CREATE SEQUENCE IF NOT EXISTS refresh_token_family_seq;

-- ─── TABELĂ: refresh_tokens (JWT refresh flow) ──────────────────────────────
CREATE TABLE refresh_tokens (
    id         BIGINT       GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id    BIGINT       NOT NULL,
    token      VARCHAR(512) NOT NULL UNIQUE,
    family_id  BIGINT       NOT NULL,
    expires_at TIMESTAMPTZ  NOT NULL,
    revoked_at TIMESTAMPTZ  NULL,
    created_at TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_refresh_tokens_user
        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_active  ON refresh_tokens(token)
    WHERE revoked_at IS NULL;
CREATE INDEX idx_refresh_tokens_family  ON refresh_tokens(family_id);

-- ─── Comentarii documentare ──────────────────────────────────────────────────
COMMENT ON TABLE  users                       IS 'Utilizatori sistem cu autentificare JWT';
COMMENT ON COLUMN users.status                IS 'PENDING=email neconfirmat, ACTIVE=funcțional, INACTIVE=dezactivat';
COMMENT ON COLUMN users.preferred_currency_id IS 'FK catre moneda preferata pentru afisarea tranzactiilor';
COMMENT ON TABLE  currencies                  IS 'Nomenclator monede ISO 4217';
COMMENT ON TABLE  refresh_tokens              IS 'Token-uri JWT refresh — rotation policy';

-- ─── TABELĂ: password_reset_tokens (forgot-password flow) ───────────────────
CREATE TABLE IF NOT EXISTS password_reset_tokens (
    id         BIGINT       GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id    BIGINT       NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token      VARCHAR(512) UNIQUE NOT NULL,
    expires_at TIMESTAMPTZ  NOT NULL,
    used_at    TIMESTAMPTZ  NULL,
    created_at TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_password_reset_active
    ON password_reset_tokens (token) WHERE used_at IS NULL;

COMMENT ON TABLE password_reset_tokens IS 'Token-uri reset parola — single-use, expira in 1h';
