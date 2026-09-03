-- ════════════════════════════════════════════════════════════════════════════
-- CHAT SERVICE — SCHEMA (PostgreSQL 16, aceeasi baza finance_db)
-- Ruleaza DUPA Finance (06_*), fiindca reutilizeaza sp_is_group_member + group_members.
-- ════════════════════════════════════════════════════════════════════════════

CREATE TABLE chat_messages (
    id                  BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    group_id            BIGINT      NOT NULL,        -- fara FK (decuplat de Finance)
    sender_user_id      BIGINT      NOT NULL,
    content             TEXT        NOT NULL,
    reply_to_message_id BIGINT      NULL REFERENCES chat_messages(id) ON DELETE SET NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    edited_at           TIMESTAMPTZ NULL,
    deleted_at          TIMESTAMPTZ NULL
);

CREATE INDEX idx_chat_messages_group ON chat_messages(group_id, created_at DESC);

-- Insereaza un mesaj si intoarce randul complet (pentru broadcast).
CREATE OR REPLACE FUNCTION sp_chat_insert_message(
    p_group_id BIGINT,
    p_sender   BIGINT,
    p_content  TEXT,
    p_reply_to BIGINT
)
RETURNS TABLE(
    id                  BIGINT,
    group_id            BIGINT,
    sender_user_id      BIGINT,
    content             TEXT,
    reply_to_message_id BIGINT,
    created_at          TIMESTAMPTZ,
    edited_at           TIMESTAMPTZ,
    deleted_at          TIMESTAMPTZ
)
LANGUAGE sql AS $$
    INSERT INTO chat_messages (group_id, sender_user_id, content, reply_to_message_id)
    VALUES (p_group_id, p_sender, p_content, p_reply_to)
    RETURNING id, group_id, sender_user_id, content, reply_to_message_id,
              created_at, edited_at, deleted_at;
$$;

-- Istoric paginat: mesajele dinaintea lui p_before_id (sau cele mai noi daca NULL).
CREATE OR REPLACE FUNCTION sp_chat_get_messages(
    p_group_id  BIGINT,
    p_before_id BIGINT,
    p_limit     INTEGER
)
RETURNS TABLE(
    id                  BIGINT,
    group_id            BIGINT,
    sender_user_id      BIGINT,
    content             TEXT,
    reply_to_message_id BIGINT,
    created_at          TIMESTAMPTZ,
    edited_at           TIMESTAMPTZ,
    deleted_at          TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT id, group_id, sender_user_id, content, reply_to_message_id,
           created_at, edited_at, deleted_at
    FROM chat_messages
    WHERE group_id = p_group_id
      AND (p_before_id IS NULL OR id < p_before_id)
    ORDER BY id DESC
    LIMIT p_limit;
$$;

-- Un mesaj dupa id (pentru broadcast dupa edit).
CREATE OR REPLACE FUNCTION sp_chat_get_message_by_id(p_id BIGINT)
RETURNS TABLE(
    id                  BIGINT,
    group_id            BIGINT,
    sender_user_id      BIGINT,
    content             TEXT,
    reply_to_message_id BIGINT,
    created_at          TIMESTAMPTZ,
    edited_at           TIMESTAMPTZ,
    deleted_at          TIMESTAMPTZ
)
LANGUAGE sql STABLE AS $$
    SELECT id, group_id, sender_user_id, content, reply_to_message_id,
           created_at, edited_at, deleted_at
    FROM chat_messages WHERE id = p_id LIMIT 1;
$$;

-- Editare (doar autorul, mesaj ne-sters). Intoarce nr randuri.
CREATE OR REPLACE FUNCTION sp_chat_edit_message(p_id BIGINT, p_sender BIGINT, p_content TEXT)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE chat_messages
    SET content = p_content, edited_at = NOW()
    WHERE id = p_id AND sender_user_id = p_sender AND deleted_at IS NULL;
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

-- Stergere soft (doar autorul). Intoarce nr randuri.
CREATE OR REPLACE FUNCTION sp_chat_delete_message(p_id BIGINT, p_sender BIGINT)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE v_rows INTEGER;
BEGIN
    UPDATE chat_messages
    SET deleted_at = NOW()
    WHERE id = p_id AND sender_user_id = p_sender AND deleted_at IS NULL;
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RETURN v_rows;
END;
$$;

-- Id-urile membrilor ACTIVE ai grupului (pentru contoare necitite). Reutilizeaza group_members.
CREATE OR REPLACE FUNCTION sp_chat_get_member_ids(p_group_id BIGINT)
RETURNS TABLE(user_id BIGINT)
LANGUAGE sql STABLE AS $$
    SELECT user_id FROM group_members
    WHERE group_id = p_group_id AND status = 'ACTIVE';
$$;
