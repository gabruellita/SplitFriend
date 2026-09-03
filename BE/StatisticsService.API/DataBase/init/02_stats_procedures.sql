-- ════════════════════════════════════════════════════════════════════════════
-- STATISTICS SERVICE — PROCEDURI STOCATE (PostgreSQL 16)
-- Read-only (STABLE). Citesc tabela `transactions` din finance_db (domeniul Finance).
-- Montate DUPA scripturile Finance (vezi docker-compose). Apelate prin Dapper cu
-- CommandType.StoredProcedure. ENUM-urile se intorc ::TEXT; se accepta TEXT cu cast ::enum.
-- ════════════════════════════════════════════════════════════════════════════

-- 1. Evolutie venituri vs cheltuieli in timp (granularitate parametrizata)
CREATE OR REPLACE FUNCTION sp_stats_timeseries(
    p_user_id BIGINT, p_from DATE, p_to DATE, p_granularity TEXT)
RETURNS TABLE(bucket DATE, kind TEXT, total NUMERIC)
LANGUAGE sql STABLE AS $$
    SELECT date_trunc(p_granularity, t.transaction_date)::date AS bucket,
           t.kind::TEXT, SUM(t.amount) AS total
    FROM transactions t
    WHERE t.user_id = p_user_id AND t.status = 'POSTED'
      AND (p_from IS NULL OR t.transaction_date >= p_from)
      AND (p_to   IS NULL OR t.transaction_date <= p_to)
    GROUP BY 1, 2
    ORDER BY 1;
$$;

-- 2. Breakdown pe categorii (acopera #2 cheltuieli + #3 venituri prin p_kind)
CREATE OR REPLACE FUNCTION sp_stats_category_breakdown(
    p_user_id BIGINT, p_from DATE, p_to DATE, p_kind TEXT)
RETURNS TABLE(category_id BIGINT, category_name VARCHAR(100), total NUMERIC, cnt BIGINT)
LANGUAGE sql STABLE AS $$
    SELECT t.category_id, c.name AS category_name, SUM(t.amount) AS total, COUNT(*) AS cnt
    FROM transactions t
    LEFT JOIN categories c ON c.id = t.category_id
    WHERE t.user_id = p_user_id AND t.status = 'POSTED' AND t.kind = p_kind::transaction_kind
      AND (p_from IS NULL OR t.transaction_date >= p_from)
      AND (p_to   IS NULL OR t.transaction_date <= p_to)
    GROUP BY t.category_id, c.name
    ORDER BY total DESC;
$$;

-- 3. Top N categorii + procent din total (window: SUM() OVER ())
CREATE OR REPLACE FUNCTION sp_stats_top_categories(
    p_user_id BIGINT, p_from DATE, p_to DATE, p_kind TEXT, p_limit INTEGER)
RETURNS TABLE(category_name VARCHAR(100), total NUMERIC, pct NUMERIC)
LANGUAGE sql STABLE AS $$
    SELECT c.name, SUM(t.amount) AS total,
           ROUND(100.0 * SUM(t.amount) / NULLIF(SUM(SUM(t.amount)) OVER (), 0), 1) AS pct
    FROM transactions t
    LEFT JOIN categories c ON c.id = t.category_id
    WHERE t.user_id = p_user_id AND t.status = 'POSTED' AND t.kind = p_kind::transaction_kind
      AND (p_from IS NULL OR t.transaction_date >= p_from)
      AND (p_to   IS NULL OR t.transaction_date <= p_to)
    GROUP BY c.id, c.name
    ORDER BY total DESC
    LIMIT p_limit;
$$;

-- 4. Heatmap calendar (generate_series + LEFT JOIN: include zilele fara tranzactii)
CREATE OR REPLACE FUNCTION sp_stats_calendar(p_user_id BIGINT, p_from DATE, p_to DATE)
RETURNS TABLE(zi DATE, cnt BIGINT, total NUMERIC)
LANGUAGE sql STABLE AS $$
    SELECT d::date AS zi, COUNT(t.id) AS cnt, COALESCE(SUM(t.amount), 0) AS total
    FROM generate_series(p_from, p_to, '1 day') d
    LEFT JOIN transactions t
      ON t.transaction_date = d::date AND t.user_id = p_user_id AND t.status = 'POSTED'
    GROUP BY d
    ORDER BY d;
$$;

-- 5. Histogram sume tranzactii (width_bucket)
CREATE OR REPLACE FUNCTION sp_stats_histogram(
    p_user_id BIGINT, p_from DATE, p_to DATE, p_max NUMERIC, p_buckets INTEGER)
RETURNS TABLE(bucket INTEGER, cnt BIGINT)
LANGUAGE sql STABLE AS $$
    -- LEAST(..., p_buckets): valorile >= p_max ar cadea altfel in bucket-ul p_buckets+1
    -- (overflow); le pliem in ultimul bucket ca sa pastram exact p_buckets intervale.
    SELECT LEAST(width_bucket(amount, 0, p_max, p_buckets), p_buckets) AS bucket, COUNT(*) AS cnt
    FROM transactions
    WHERE user_id = p_user_id AND status = 'POSTED' AND kind = 'EXPENSE'::transaction_kind
      AND (p_from IS NULL OR transaction_date >= p_from)
      AND (p_to   IS NULL OR transaction_date <= p_to)
    GROUP BY bucket
    ORDER BY bucket;
$$;

-- 6. Rata de economisire pe luna (FILTER + NULLIF)
CREATE OR REPLACE FUNCTION sp_stats_savings_rate(p_user_id BIGINT, p_from DATE, p_to DATE)
RETURNS TABLE(luna DATE, venituri NUMERIC, cheltuieli NUMERIC, rata NUMERIC)
LANGUAGE sql STABLE AS $$
    SELECT date_trunc('month', transaction_date)::date AS luna,
           COALESCE(SUM(amount) FILTER (WHERE kind = 'INCOME'), 0)  AS venituri,
           COALESCE(SUM(amount) FILTER (WHERE kind = 'EXPENSE'), 0) AS cheltuieli,
           ROUND(
             (SUM(amount) FILTER (WHERE kind = 'INCOME') - SUM(amount) FILTER (WHERE kind = 'EXPENSE'))
             / NULLIF(SUM(amount) FILTER (WHERE kind = 'INCOME'), 0) * 100, 1) AS rata
    FROM transactions
    WHERE user_id = p_user_id AND status = 'POSTED'
      AND (p_from IS NULL OR transaction_date >= p_from)
      AND (p_to   IS NULL OR transaction_date <= p_to)
    GROUP BY 1
    ORDER BY 1;
$$;

-- 7. Sold cumulativ in timp (window: SUM() OVER (ORDER BY) peste netul zilnic)
CREATE OR REPLACE FUNCTION sp_stats_running_balance(p_user_id BIGINT, p_from DATE, p_to DATE)
RETURNS TABLE(zi DATE, sold_cumulat NUMERIC)
LANGUAGE sql STABLE AS $$
    WITH daily AS (
        SELECT transaction_date AS zi,
               SUM(CASE WHEN kind = 'INCOME' THEN amount ELSE -amount END) AS net_zi
        FROM transactions
        WHERE user_id = p_user_id AND status = 'POSTED'
          AND (p_from IS NULL OR transaction_date >= p_from)
          AND (p_to   IS NULL OR transaction_date <= p_to)
        GROUP BY transaction_date
    )
    SELECT zi, SUM(net_zi) OVER (ORDER BY zi) AS sold_cumulat
    FROM daily
    ORDER BY zi;
$$;

-- 8. MoM / YoY (window: LAG + NULLIF; granularitate month/year)
CREATE OR REPLACE FUNCTION sp_stats_mom(
    p_user_id BIGINT, p_from DATE, p_to DATE, p_kind TEXT, p_granularity TEXT)
RETURNS TABLE(perioada DATE, total NUMERIC, total_anterior NUMERIC, variatie_pct NUMERIC)
LANGUAGE sql STABLE AS $$
    SELECT perioada, total,
           total_anterior,
           ROUND(100.0 * (total - total_anterior) / NULLIF(total_anterior, 0), 1) AS variatie_pct
    FROM (
        SELECT perioada, total,
               LAG(total) OVER (ORDER BY perioada) AS total_anterior
        FROM (
            SELECT date_trunc(p_granularity, transaction_date)::date AS perioada, SUM(amount) AS total
            FROM transactions
            WHERE user_id = p_user_id AND status = 'POSTED' AND kind = p_kind::transaction_kind
              AND (p_from IS NULL OR transaction_date >= p_from)
              AND (p_to   IS NULL OR transaction_date <= p_to)
            GROUP BY 1
        ) m
    ) lagged
    ORDER BY perioada;
$$;

-- 9. Pareto 80/20 (window cumulativ + frame ROWS UNBOUNDED PRECEDING)
CREATE OR REPLACE FUNCTION sp_stats_pareto(p_user_id BIGINT, p_from DATE, p_to DATE)
RETURNS TABLE(category_name VARCHAR(100), total NUMERIC, pct_cumulat NUMERIC)
LANGUAGE sql STABLE AS $$
    SELECT category_name, total,
           ROUND(100.0 * SUM(total) OVER (ORDER BY total DESC ROWS UNBOUNDED PRECEDING)
                 / NULLIF(SUM(total) OVER (), 0), 1) AS pct_cumulat
    FROM (
        SELECT c.name AS category_name, SUM(t.amount) AS total
        FROM transactions t
        LEFT JOIN categories c ON c.id = t.category_id
        WHERE t.user_id = p_user_id AND t.status = 'POSTED' AND t.kind = 'EXPENSE'::transaction_kind
          AND (p_from IS NULL OR t.transaction_date >= p_from)
          AND (p_to   IS NULL OR t.transaction_date <= p_to)
        GROUP BY c.id, c.name
    ) s
    ORDER BY total DESC;
$$;

-- 10. Cheltuieli pe ziua saptamanii (EXTRACT DOW)
CREATE OR REPLACE FUNCTION sp_stats_weekday(
    p_user_id BIGINT, p_from DATE, p_to DATE, p_kind TEXT)
RETURNS TABLE(dow INTEGER, zi TEXT, total NUMERIC, cnt BIGINT)
LANGUAGE sql STABLE AS $$
    SELECT EXTRACT(DOW FROM transaction_date)::int AS dow,
           to_char(transaction_date, 'Dy') AS zi,
           SUM(amount) AS total, COUNT(*) AS cnt
    FROM transactions
    WHERE user_id = p_user_id AND status = 'POSTED' AND kind = p_kind::transaction_kind
      AND (p_from IS NULL OR transaction_date >= p_from)
      AND (p_to   IS NULL OR transaction_date <= p_to)
    GROUP BY 1, 2
    ORDER BY 1;
$$;

-- 11. Recurente vs spontane (filtrare NULL / NOT NULL pe template_id)
CREATE OR REPLACE FUNCTION sp_stats_recurring_split(
    p_user_id BIGINT, p_from DATE, p_to DATE, p_kind TEXT)
RETURNS TABLE(este_recurenta BOOLEAN, total NUMERIC, cnt BIGINT)
LANGUAGE sql STABLE AS $$
    SELECT (template_id IS NOT NULL) AS este_recurenta,
           SUM(amount) AS total, COUNT(*) AS cnt
    FROM transactions
    WHERE user_id = p_user_id AND status = 'POSTED' AND kind = p_kind::transaction_kind
      AND (p_from IS NULL OR transaction_date >= p_from)
      AND (p_to   IS NULL OR transaction_date <= p_to)
    GROUP BY 1;
$$;
