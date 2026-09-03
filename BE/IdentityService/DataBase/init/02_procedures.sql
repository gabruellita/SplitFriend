-- ════════════════════════════════════════════════════════════════════════════
-- IDENTITY SERVICE — PROCEDURI STOCATE (PostgreSQL 16)
-- ════════════════════════════════════════════════════════════════════════════

-- ─── UserRepository ─────────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION sp_get_user_by_email(p_email VARCHAR)
RETURNS TABLE(
    id                       BIGINT,
    email                    VARCHAR(256),
    username                 VARCHAR(100),
    password_hash            VARCHAR(512),
    first_name               VARCHAR(100),
    last_name                VARCHAR(100),
    status                   TEXT,
    preferred_currency_id    BIGINT,
    email_confirmation_token VARCHAR(512),
    email_confirmed_at       TIMESTAMPTZ,
    last_login_at            TIMESTAMPTZ,
    failed_login_attempts    INTEGER,
    created_at               TIMESTAMPTZ,
    updated_at               TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT id, email, username, password_hash, first_name, last_name,
           status::TEXT, preferred_currency_id, email_confirmation_token,
           email_confirmed_at, last_login_at, failed_login_attempts,
           created_at, updated_at
    FROM users
    WHERE email = p_email
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION sp_get_user_by_id(p_id BIGINT)
RETURNS TABLE(
    id                       BIGINT,
    email                    VARCHAR(256),
    username                 VARCHAR(100),
    password_hash            VARCHAR(512),
    first_name               VARCHAR(100),
    last_name                VARCHAR(100),
    status                   TEXT,
    preferred_currency_id    BIGINT,
    email_confirmation_token VARCHAR(512),
    email_confirmed_at       TIMESTAMPTZ,
    last_login_at            TIMESTAMPTZ,
    failed_login_attempts    INTEGER,
    created_at               TIMESTAMPTZ,
    updated_at               TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT id, email, username, password_hash, first_name, last_name,
           status::TEXT, preferred_currency_id, email_confirmation_token,
           email_confirmed_at, last_login_at, failed_login_attempts,
           created_at, updated_at
    FROM users
    WHERE id = p_id
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION sp_user_exists_by_email(p_email VARCHAR)
RETURNS BOOLEAN
LANGUAGE sql STABLE AS $$
    SELECT EXISTS(SELECT 1 FROM users WHERE email = p_email);
$$;

CREATE OR REPLACE FUNCTION sp_user_exists_by_username(p_username VARCHAR)
RETURNS BOOLEAN
LANGUAGE sql STABLE AS $$
    SELECT EXISTS(SELECT 1 FROM users WHERE username = p_username);
$$;

CREATE OR REPLACE FUNCTION sp_create_user(
    p_email                    VARCHAR,
    p_username                 VARCHAR,
    p_password_hash            VARCHAR,
    p_first_name               VARCHAR,
    p_last_name                VARCHAR,
    p_status                   TEXT,
    p_preferred_currency_id    BIGINT,
    p_email_confirmation_token VARCHAR
)
RETURNS BIGINT
LANGUAGE sql AS $$
    INSERT INTO users
        (email, username, password_hash, first_name, last_name,
         status, preferred_currency_id, email_confirmation_token)
    VALUES
        (p_email, p_username, p_password_hash, p_first_name, p_last_name,
         p_status::user_status, p_preferred_currency_id, p_email_confirmation_token)
    RETURNING id;
$$;

CREATE OR REPLACE FUNCTION sp_confirm_email(p_token VARCHAR)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE
    v_rows INTEGER;
BEGIN
    UPDATE users
    SET status                   = 'ACTIVE'::user_status,
        email_confirmed_at       = NOW(),
        email_confirmation_token = NULL
    WHERE email_confirmation_token = p_token
      AND status = 'PENDING'::user_status;
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

CREATE OR REPLACE FUNCTION sp_update_last_login(p_user_id BIGINT)
RETURNS VOID
LANGUAGE sql AS $$
    UPDATE users
    SET last_login_at         = NOW(),
        failed_login_attempts = 0
    WHERE id = p_user_id;
$$;

CREATE OR REPLACE FUNCTION sp_increment_failed_attempts(p_user_id BIGINT)
RETURNS VOID
LANGUAGE sql AS $$
    UPDATE users
    SET failed_login_attempts = failed_login_attempts + 1
    WHERE id = p_user_id;
$$;

-- ─── CurrencyRepository ──────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION sp_get_active_currencies()
RETURNS TABLE(
    id        BIGINT,
    code      VARCHAR(3),
    name      VARCHAR(100),
    symbol    VARCHAR(10),
    is_active BOOLEAN
)
LANGUAGE sql STABLE AS $$
    SELECT id, code, name, symbol, is_active
    FROM currencies
    WHERE is_active = TRUE
    ORDER BY code;
$$;

CREATE OR REPLACE FUNCTION sp_currency_exists_active(p_id BIGINT)
RETURNS BOOLEAN
LANGUAGE sql STABLE AS $$
    SELECT EXISTS(SELECT 1 FROM currencies WHERE id = p_id AND is_active = TRUE);
$$;

-- ─── RefreshTokenRepository ──────────────────────────────────────────────────

-- Genereaza un nou family_id (la login = sesiune/lant nou de tokenuri).
CREATE OR REPLACE FUNCTION sp_next_token_family()
RETURNS BIGINT
LANGUAGE sql AS $$
    SELECT nextval('refresh_token_family_seq');
$$;

CREATE OR REPLACE FUNCTION sp_create_refresh_token(
    p_user_id    BIGINT,
    p_token      VARCHAR,
    p_expires_at TIMESTAMPTZ,
    p_family_id  BIGINT
)
RETURNS VOID
LANGUAGE sql AS $$
    INSERT INTO refresh_tokens (user_id, token, expires_at, family_id)
    VALUES (p_user_id, p_token, p_expires_at, p_family_id);
$$;

-- Intoarce tokenul INCLUSIV daca e revocat — reuse-ul trebuie sa fie detectabil.
CREATE OR REPLACE FUNCTION sp_get_refresh_token_by_token(p_token VARCHAR)
RETURNS TABLE(
    id         BIGINT,
    user_id    BIGINT,
    token      VARCHAR(512),
    family_id  BIGINT,
    expires_at TIMESTAMPTZ,
    revoked_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT id, user_id, token, family_id, expires_at, revoked_at, created_at
    FROM refresh_tokens
    WHERE token = p_token
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION sp_revoke_refresh_token(p_token VARCHAR)
RETURNS VOID
LANGUAGE sql AS $$
    UPDATE refresh_tokens
    SET revoked_at = NOW()
    WHERE token = p_token AND revoked_at IS NULL;
$$;

-- Reuse detectat → revoca intreg lantul (family) de tokenuri active.
CREATE OR REPLACE FUNCTION sp_revoke_token_family(p_family_id BIGINT)
RETURNS VOID
LANGUAGE sql AS $$
    UPDATE refresh_tokens
    SET revoked_at = NOW()
    WHERE family_id = p_family_id AND revoked_at IS NULL;
$$;

CREATE OR REPLACE FUNCTION sp_revoke_all_refresh_tokens_for_user(p_user_id BIGINT)
RETURNS VOID
LANGUAGE sql AS $$
    UPDATE refresh_tokens
    SET revoked_at = NOW()
    WHERE user_id = p_user_id AND revoked_at IS NULL;
$$;

-- ─── ProfileService ─────────────────────────────────────────────────────────

-- Patch partial: parametrii NULL lasa coloana neschimbata (COALESCE).
CREATE OR REPLACE FUNCTION sp_update_user_profile(
    p_id           BIGINT,
    p_first_name   VARCHAR,
    p_last_name    VARCHAR,
    p_currency_id  BIGINT
)
RETURNS TABLE(
    id                    BIGINT,
    email                 VARCHAR(256),
    username              VARCHAR(100),
    first_name            VARCHAR(100),
    last_name             VARCHAR(100),
    status                TEXT,
    preferred_currency_id BIGINT,
    created_at            TIMESTAMPTZ
)
LANGUAGE sql AS $$
    UPDATE users SET
        first_name            = COALESCE(p_first_name,  first_name),
        last_name             = COALESCE(p_last_name,   last_name),
        preferred_currency_id = COALESCE(p_currency_id, preferred_currency_id)
    WHERE id = p_id
    RETURNING id, email, username, first_name, last_name,
              status::TEXT, preferred_currency_id, created_at;
$$;

CREATE OR REPLACE FUNCTION sp_change_password(p_id BIGINT, p_new_hash VARCHAR)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE n INTEGER;
BEGIN
    UPDATE users SET password_hash = p_new_hash WHERE id = p_id;
    GET DIAGNOSTICS n = ROW_COUNT;
    RETURN n;
END;
$$;

-- Profil cu codul monedei (JOIN currencies), pentru GET me.
CREATE OR REPLACE FUNCTION sp_get_user_profile(p_id BIGINT)
RETURNS TABLE(
    id                      BIGINT,
    email                   VARCHAR(256),
    username                VARCHAR(100),
    first_name              VARCHAR(100),
    last_name               VARCHAR(100),
    status                  TEXT,
    preferred_currency_id   BIGINT,
    preferred_currency_code VARCHAR(3),
    created_at              TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT u.id, u.email, u.username, u.first_name, u.last_name,
           u.status::TEXT, u.preferred_currency_id, c.code, u.created_at
    FROM users u
    JOIN currencies c ON c.id = u.preferred_currency_id
    WHERE u.id = p_id;
$$;

-- ─── PasswordReset ──────────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION sp_create_password_reset_token(
    p_user_id BIGINT, p_token VARCHAR, p_expires_at TIMESTAMPTZ)
RETURNS BIGINT
LANGUAGE sql AS $$
    INSERT INTO password_reset_tokens (user_id, token, expires_at)
    VALUES (p_user_id, p_token, p_expires_at)
    RETURNING id;
$$;

-- Întoarce rândul DOAR dacă tokenul există, nu e folosit și nu e expirat.
CREATE OR REPLACE FUNCTION sp_get_active_reset_token(p_token VARCHAR)
RETURNS TABLE(id BIGINT, user_id BIGINT, expires_at TIMESTAMPTZ, used_at TIMESTAMPTZ)
LANGUAGE sql STABLE AS $$
    SELECT id, user_id, expires_at, used_at
    FROM password_reset_tokens
    WHERE token = p_token AND used_at IS NULL AND expires_at > NOW()
    LIMIT 1;
$$;

-- Single-use: marchează folosit; întoarce nr. rânduri afectate (0 = deja folosit/invalid).
CREATE OR REPLACE FUNCTION sp_consume_password_reset_token(p_token VARCHAR)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE n INTEGER;
BEGIN
    UPDATE password_reset_tokens SET used_at = NOW()
    WHERE token = p_token AND used_at IS NULL AND expires_at > NOW();
    GET DIAGNOSTICS n = ROW_COUNT;
    RETURN n;
END;
$$;
