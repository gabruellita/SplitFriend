# -*- coding: utf-8 -*-
"""
Seed de date demo pentru FinanceApp (licență).

Populează finance_db (Postgres din docker-compose, localhost:5433) cu 5 useri demo
cu istoric realist de 10–12 luni: venituri/cheltuieli, categorii custom, template-uri
recurente cu tranzacții generate, plus 2 grupuri Split Bill cu decontări parțiale.

Rulare:
    pip install -r scripts/requirements.txt
    python scripts/seed_demo_data.py

Comportament: șterge întâi userii demo (email @demo.finance) și TOT ce ține de ei,
apoi recreează totul de la zero, într-o singură tranzacție SQL. Conturile reale
(orice alt email) nu sunt atinse. Datele sunt deterministe (seed fix) relativ la
ziua rulării: istoricul se termină mereu "azi".

Split Bill NU se inserează manual: se apelează sp_create_group_expense /
sp_create_payment — aceleași proceduri folosite de Finance Service — ca datele
să fie identice cu cele produse de aplicație (tranzacții personale, alocări FIFO,
auto-settle prin triggere).
"""

import calendar
import json
import random
import sys
from datetime import date, datetime, time, timedelta, timezone
from pathlib import Path

try:
    import bcrypt
    import psycopg2
except ImportError as e:
    sys.exit(f"Lipsește un pachet ({e.name}). Rulează: pip install -r scripts/requirements.txt")

# ── Configurare ───────────────────────────────────────────────────────────────
REPO_ROOT = Path(__file__).resolve().parent.parent
ENV_FILE = REPO_ROOT / "BE" / ".env"

DB_HOST, DB_PORT, DB_NAME, DB_USER = "localhost", 5433, "finance_db", "finance_user"

DEMO_DOMAIN = "@demo.finance"
DEMO_PASSWORD = "Parola123!"
SEED = 42

TODAY = date.today()
MONTHS_RO = ["ianuarie", "februarie", "martie", "aprilie", "mai", "iunie",
             "iulie", "august", "septembrie", "octombrie", "noiembrie", "decembrie"]

# Descrieri realiste pentru zgomotul de zi cu zi
FOOD = ["Cumpărături Mega Image", "Cumpărături Lidl", "Cumpărături Kaufland",
        "Carrefour Express", "Piața Obor", "Glovo — comandă mâncare",
        "Shaormeria Dristor", "Prânz la birou", "Profi City"]
TRANSPORT = ["Călătorie STB", "Încărcare card metrou", "Uber", "Bolt", "Free Now"]
FUN = ["Cinema City AFI", "Ieșire cu prietenii", "Steam", "Bowling", "Bilete concert",
       "Escape room", "Teatru", "Berărie Centrul Vechi"]
HEALTH = ["Farmacia Catena", "Farmacia Tei", "Analize Synevo", "Consult medical", "Dentist"]


def load_db_password() -> str:
    if not ENV_FILE.exists():
        sys.exit(f"Nu găsesc {ENV_FILE} (trebuie să conțină DB_PASSWORD).")
    for line in ENV_FILE.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if line.startswith("DB_PASSWORD="):
            return line.split("=", 1)[1].strip()
    sys.exit(f"DB_PASSWORD lipsește din {ENV_FILE}.")


# ── Utilitare de calendar ────────────────────────────────────────────────────
def month_start(d: date) -> date:
    return d.replace(day=1)


def add_months(d: date, n: int) -> date:
    m = d.month - 1 + n
    return date(d.year + m // 12, m % 12 + 1, 1)


def occ_date(month_first: date, day: int) -> date:
    """Ziua `day` din luna dată, limitată la lungimea lunii (31 → 28/30 unde e cazul)."""
    last = calendar.monthrange(month_first.year, month_first.month)[1]
    return month_first.replace(day=min(day, last))


def ts(d: date, hour: int = 12, minute: int = 0) -> datetime:
    return datetime.combine(d, time(hour, minute), tzinfo=timezone.utc)


class Ctx:
    """Context partajat: cursor, generator aleator determinist, nomenclatoare."""

    def __init__(self, cur, rng):
        self.cur = cur
        self.rng = rng
        cur.execute("SELECT code, id FROM currencies")
        self.currency = dict(cur.fetchall())
        cur.execute("SELECT name, id FROM categories WHERE is_system = TRUE")
        self.syscat = dict(cur.fetchall())
        self.tx_count = 0


# ── Curățare date demo ───────────────────────────────────────────────────────
def cleanup(cur) -> int:
    cur.execute("SELECT id FROM users WHERE email LIKE %s", (f"%{DEMO_DOMAIN}",))
    ids = [r[0] for r in cur.fetchall()]
    if not ids:
        return 0
    # Mesajele de chat nu au FK spre groups → le ștergem explicit înaintea grupurilor.
    # (garda to_regclass: volumele Postgres mai vechi nu au schema de chat)
    cur.execute("SELECT to_regclass('chat_messages')")
    if cur.fetchone()[0]:
        cur.execute("DELETE FROM chat_messages WHERE group_id IN "
                    "(SELECT id FROM groups WHERE owner_user_id = ANY(%s))", (ids,))
    # groups CASCADE: group_members, pending_invitations, group_expenses,
    # expense_splits, payments, payment_allocations.
    cur.execute("DELETE FROM groups WHERE owner_user_id = ANY(%s)", (ids,))
    cur.execute("DELETE FROM group_members WHERE user_id = ANY(%s)", (ids,))
    cur.execute("DELETE FROM transactions WHERE user_id = ANY(%s)", (ids,))
    cur.execute("DELETE FROM recurring_transaction_templates WHERE user_id = ANY(%s)", (ids,))
    cur.execute("DELETE FROM categories WHERE created_by_user_id = ANY(%s)", (ids,))
    # users CASCADE: refresh_tokens, password_reset_tokens.
    cur.execute("DELETE FROM users WHERE id = ANY(%s)", (ids,))
    return len(ids)


# ── Creare entități ──────────────────────────────────────────────────────────
def create_user(ctx, email, username, first, last, currency_code, created: date,
                password_hash: str) -> int:
    ctx.cur.execute(
        """INSERT INTO users (email, username, password_hash, first_name, last_name,
                              status, preferred_currency_id, email_confirmed_at,
                              last_login_at, created_at, updated_at)
           VALUES (%s, %s, %s, %s, %s, 'ACTIVE', %s, %s, %s, %s, %s)
           RETURNING id""",
        (email, username, password_hash, first, last, ctx.currency[currency_code],
         ts(created, 10), ts(TODAY - timedelta(days=1), 20), ts(created, 9), ts(created, 10)))
    return ctx.cur.fetchone()[0]


def create_category(ctx, user_id, name, kind, icon, color, created: date) -> int:
    ctx.cur.execute(
        """INSERT INTO categories (name, kind, icon, color, created_by_user_id,
                                   is_system, created_at, updated_at)
           VALUES (%s, %s, %s, %s, %s, FALSE, %s, %s) RETURNING id""",
        (name, kind, icon, color, user_id, ts(created, 11), ts(created, 11)))
    return ctx.cur.fetchone()[0]


def insert_tx(ctx, user_id, category_id, amount, currency_code, kind, desc, d: date,
              status="POSTED", template_id=None):
    hour = ctx.rng.randint(8, 21)
    ctx.cur.execute(
        """INSERT INTO transactions (user_id, category_id, amount, currency_id, kind,
                                     description, transaction_date, status, template_id,
                                     created_at, updated_at)
           VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)""",
        (user_id, category_id, round(amount, 2), ctx.currency[currency_code], kind,
         desc, d, status, template_id, ts(d, hour), ts(d, hour)))
    ctx.tx_count += 1


def create_recurring(ctx, user_id, category_id, currency_code, kind, desc, amount,
                     day, start_month: date, amount_fn=None, monthly_suffix=False):
    """Template MONTHLY + tranzacțiile istorice generate din el (template_id setat).

    amount_fn(data) permite valori istorice diferite (ex. mărire de salariu);
    template-ul păstrează valoarea curentă, ca după o modificare reală în aplicație.
    """
    start_date = occ_date(start_month, day)
    m = start_month
    occurrences = []
    while True:
        d = occ_date(m, day)
        if d > TODAY:
            next_run = d
            break
        occurrences.append(d)
        m = add_months(m, 1)

    ctx.cur.execute(
        """INSERT INTO recurring_transaction_templates
               (user_id, category_id, amount, currency_id, kind, description, frequency,
                interval_count, start_date, end_date, next_run_date, is_active,
                created_at, updated_at)
           VALUES (%s, %s, %s, %s, %s, %s, 'MONTHLY', 1, %s, NULL, %s, TRUE, %s, %s)
           RETURNING id""",
        (user_id, category_id, round(amount, 2), ctx.currency[currency_code], kind,
         desc, start_date, next_run, ts(start_date, 9), ts(start_date, 9)))
    template_id = ctx.cur.fetchone()[0]

    for d in occurrences:
        amt = amount_fn(d) if amount_fn else amount
        text = f"{desc} — {MONTHS_RO[d.month - 1]} {d.year}" if monthly_suffix else desc
        insert_tx(ctx, user_id, category_id, amt, currency_code, kind, text, d,
                  template_id=template_id)
    return template_id


def add_noise(ctx, user_id, category_id, currency_code, kind, per_month, lo, hi,
              descs, start_month: date, weekend_bias=False, winter_mult=1.0,
              void_prob=0.0):
    """Tranzacții aleatoare lunare (seed fix): cumpărături, transport, ieșiri etc."""
    rng = ctx.rng
    m = start_month
    while m <= TODAY:
        last = calendar.monthrange(m.year, m.month)[1]
        n = max(0, round(rng.gauss(per_month, per_month * 0.25)))
        for _ in range(n):
            day = rng.randint(1, last)
            if weekend_bias and rng.random() < 0.6:
                # împinge ziua spre cel mai apropiat weekend
                d0 = date(m.year, m.month, day)
                shift = (5 - d0.weekday()) % 7
                day = min(last, day + shift)
            d = date(m.year, m.month, day)
            if d > TODAY:
                continue
            mult = winter_mult if m.month in (11, 12, 1, 2) else 1.0
            amount = round(rng.uniform(lo, hi) * mult, 2)
            status = "VOIDED" if rng.random() < void_prob else "POSTED"
            insert_tx(ctx, user_id, category_id, amount, currency_code, kind,
                      rng.choice(descs), d, status=status)
        m = add_months(m, 1)


# ── Split Bill (prin procedurile aplicației) ─────────────────────────────────
def create_group(ctx, name, desc, currency_code, owner_id, member_ids, created: date) -> int:
    ctx.cur.execute(
        """INSERT INTO groups (name, description, currency_id, owner_user_id, status,
                               created_at, updated_at)
           VALUES (%s, %s, %s, %s, 'ACTIVE', %s, %s) RETURNING id""",
        (name, desc, ctx.currency[currency_code], owner_id, ts(created, 18), ts(created, 18)))
    group_id = ctx.cur.fetchone()[0]
    for uid in [owner_id] + member_ids:
        role = "OWNER" if uid == owner_id else "MEMBER"
        joined = ts(created, 18) if uid == owner_id else ts(created + timedelta(days=1), 12)
        ctx.cur.execute(
            """INSERT INTO group_members (group_id, user_id, role, status, invited_at, joined_at)
               VALUES (%s, %s, %s, 'ACTIVE', %s, %s)""",
            (group_id, uid, role, ts(created, 18), joined))
    return group_id


def equal_split(total, user_ids, payer_id):
    """Împărțire egală cu corecție de rotunjire pe cota plătitorului (suma = totalul)."""
    share = round(total / len(user_ids), 2)
    splits = {uid: share for uid in user_ids}
    splits[payer_id] = round(total - share * (len(user_ids) - 1), 2)
    return [{"user_id": uid, "owed_amount": owed} for uid, owed in splits.items()]


def add_expense(ctx, group_id, paid_by, title, amount, currency_code, split_type,
                d: date, splits) -> int:
    ctx.cur.execute("SELECT sp_create_group_expense(%s, %s, %s, %s, %s, %s, %s, %s::jsonb)",
                    (group_id, paid_by, title, round(amount, 2), ctx.currency[currency_code],
                     split_type, d, json.dumps(splits)))
    ctx.tx_count += 1  # tranzacția personală a plătitorului, creată de procedură
    return ctx.cur.fetchone()[0]


def add_payment(ctx, group_id, from_id, to_id, amount, currency_code, d: date, method,
                original_amount=None, original_currency_code=None, rate=1.0):
    """Plată settle-up prin sp_create_payment (alocare FIFO + tranzacția debitorului)."""
    original_amount = round(original_amount if original_amount is not None else amount, 2)
    original_code = original_currency_code or currency_code
    ctx.cur.execute("SELECT sp_create_payment(%s, %s, %s, %s, %s, %s, %s, %s, %s, %s)",
                    (group_id, from_id, to_id, round(amount, 2), ctx.currency[currency_code],
                     original_amount, ctx.currency[original_code], round(rate, 8), d, method))
    ctx.tx_count += 1  # "Decontare grup" — tranzacția personală a debitorului
    return ctx.cur.fetchone()[0]


# ── Cei 5 useri demo ─────────────────────────────────────────────────────────
def seed_ana(ctx, uid):
    """Userul principal de demo: 12 luni istoric bogat, mărire de salariu, sezonalitate."""
    rng = ctx.rng
    start = add_months(month_start(TODAY), -12)
    cat_sanatate = create_category(ctx, uid, "Sănătate", "EXPENSE", "🏥", "#14b8a6", start)
    cat_vacante = create_category(ctx, uid, "Vacanțe", "EXPENSE", "✈️", "#0ea5e9", start)
    cat_cadouri = create_category(ctx, uid, "Cadouri", "EXPENSE", "🎁", "#ec4899", start)
    cat_abon = create_category(ctx, uid, "Abonamente", "EXPENSE", "📱", "#8b5cf6", start)

    raise_date = date(2026, 1, 1)
    create_recurring(ctx, uid, ctx.syscat["Salariu"], "RON", "INCOME", "Salariu Luxoft",
                     9500, 10, start, monthly_suffix=True,
                     amount_fn=lambda d: 8200 if d < raise_date else 9500)
    create_recurring(ctx, uid, cat_abon, "RON", "EXPENSE", "Netflix", 55.99, 3, start)
    create_recurring(ctx, uid, cat_abon, "RON", "EXPENSE", "Spotify Premium", 25.99, 15, start)
    create_recurring(ctx, uid, cat_abon, "RON", "EXPENSE", "Abonament Orange", 45, 20, start)
    create_recurring(ctx, uid, cat_sanatate, "RON", "EXPENSE",
                     "Abonament sală World Class", 199, 1, start)

    add_noise(ctx, uid, ctx.syscat["Mâncare"], "RON", "EXPENSE", 12, 20, 180, FOOD,
              start, void_prob=0.01)
    add_noise(ctx, uid, ctx.syscat["Transport"], "RON", "EXPENSE", 8, 3, 60, TRANSPORT, start)
    add_noise(ctx, uid, ctx.syscat["Divertisment"], "RON", "EXPENSE", 5, 30, 250, FUN,
              start, weekend_bias=True, void_prob=0.01)
    add_noise(ctx, uid, cat_sanatate, "RON", "EXPENSE", 1.5, 25, 200, HEALTH, start)

    # Evenimente: prime, cadouri de sărbători, pregătiri pentru vacanța din Grecia
    insert_tx(ctx, uid, ctx.syscat["Alte venituri"], 2000, "RON", "INCOME",
              "Primă de Crăciun", date(2025, 12, 20))
    insert_tx(ctx, uid, ctx.syscat["Alte venituri"], 1500, "RON", "INCOME",
              "Bonus de performanță", date(2026, 3, 16))
    for desc, amount, d in [("Cadou Crăciun — părinți", 420, date(2025, 12, 12)),
                            ("Cadou Crăciun — Mihai", 180, date(2025, 12, 15)),
                            ("Cadou Secret Santa birou", 120, date(2025, 12, 8)),
                            ("Cadouri colegi + ambalaje", 240, date(2025, 12, 19)),
                            ("Cadou aniversare mama", 350, date(2026, 4, 22))]:
        insert_tx(ctx, uid, cat_cadouri, amount, "RON", "EXPENSE", desc, d)
    for desc, amount, d in [("Bagaj de cală Wizz Air", 139, date(2026, 6, 10)),
                            ("Asigurare de călătorie", 95, date(2026, 6, 12)),
                            ("Suveniruri Santorini", 180, date(2026, 6, 25)),
                            ("Ieșire pe plajă Perissa", 160, date(2026, 6, 24))]:
        insert_tx(ctx, uid, cat_vacante, amount, "RON", "EXPENSE", desc, d)


def seed_mihai(ctx, uid):
    """Student: buget mic, bursă + meditații; owner-ul grupului de apartament."""
    start = add_months(month_start(TODAY), -10)
    cat_abon = create_category(ctx, uid, "Abonamente", "EXPENSE", "📱", "#8b5cf6", start)
    cat_facultate = create_category(ctx, uid, "Facultate", "EXPENSE", "📚", "#f97316", start)

    create_recurring(ctx, uid, ctx.syscat["Alte venituri"], "RON", "INCOME",
                     "Bursă de merit", 1100, 5, start, monthly_suffix=True)
    create_recurring(ctx, uid, ctx.syscat["Transport"], "RON", "EXPENSE",
                     "Abonament STB studenți", 40, 1, start)
    create_recurring(ctx, uid, cat_abon, "RON", "EXPENSE", "Spotify Student", 12.99, 7, start)

    add_noise(ctx, uid, ctx.syscat["Alte venituri"], "RON", "INCOME", 5, 140, 260,
              ["Meditații matematică", "Meditații informatică", "Meditații fizică"], start)
    add_noise(ctx, uid, ctx.syscat["Mâncare"], "RON", "EXPENSE", 10, 10, 70,
              ["Cantina Regie", "Shaormeria Grozăvești", "Mega Image", "Glovo — pizza",
               "Lidl", "Covrigărie"], start, void_prob=0.01)
    add_noise(ctx, uid, ctx.syscat["Divertisment"], "RON", "EXPENSE", 4, 15, 90, FUN,
              start, weekend_bias=True)
    add_noise(ctx, uid, cat_facultate, "RON", "EXPENSE", 3, 10, 90,
              ["Printare cursuri", "Culegere probleme", "Caiet + rechizite", "Licență software student"],
              start)


def seed_elena(ctx, uid):
    """Freelancer plătit în EUR: venituri variabile, unelte de lucru, educație."""
    start = add_months(month_start(TODAY), -11)
    cat_freelance = create_category(ctx, uid, "Freelancing", "INCOME", "💼", "#22c55e", start)
    cat_tools = create_category(ctx, uid, "Software & Tools", "EXPENSE", "🖥️", "#64748b", start)
    cat_edu = create_category(ctx, uid, "Educație", "EXPENSE", "📚", "#f59e0b", start)

    create_recurring(ctx, uid, cat_tools, "EUR", "EXPENSE", "Adobe Creative Cloud", 60.49, 8, start)
    create_recurring(ctx, uid, ctx.syscat["Divertisment"], "EUR", "EXPENSE", "Netflix", 13.99, 12, start)

    add_noise(ctx, uid, cat_freelance, "EUR", "INCOME", 3, 400, 1600,
              ["Factură client — proiect web", "Factură client — identitate vizuală",
               "Factură client — aplicație mobilă", "Factură client — mentenanță site"], start)
    add_noise(ctx, uid, ctx.syscat["Mâncare"], "EUR", "EXPENSE", 10, 8, 60, FOOD, start,
              void_prob=0.01)
    add_noise(ctx, uid, ctx.syscat["Transport"], "EUR", "EXPENSE", 4, 2, 25, TRANSPORT, start)
    add_noise(ctx, uid, ctx.syscat["Divertisment"], "EUR", "EXPENSE", 4, 10, 70, FUN,
              start, weekend_bias=True)
    add_noise(ctx, uid, cat_edu, "EUR", "EXPENSE", 1, 15, 200,
              ["Curs Udemy", "Carte tehnică", "Abonament Figma", "Workshop online"], start)


def seed_andrei(ctx, uid):
    """Corporatist: salariu mare, chirie scumpă, mult divertisment, gadgeturi."""
    start = add_months(month_start(TODAY), -12)
    cat_locuinta = create_category(ctx, uid, "Locuință", "EXPENSE", "🏠", "#78716c", start)
    cat_sport = create_category(ctx, uid, "Sport", "EXPENSE", "🏋️", "#ef4444", start)
    cat_tech = create_category(ctx, uid, "Tech & Gadgets", "EXPENSE", "🎧", "#6366f1", start)

    create_recurring(ctx, uid, ctx.syscat["Salariu"], "RON", "INCOME", "Salariu Deloitte",
                     12500, 25, start, monthly_suffix=True)
    create_recurring(ctx, uid, cat_locuinta, "RON", "EXPENSE", "Chirie apartament Pipera",
                     2800, 1, start)
    create_recurring(ctx, uid, cat_sport, "RON", "EXPENSE",
                     "Abonament sală + antrenor personal", 350, 5, start)
    create_recurring(ctx, uid, ctx.syscat["Divertisment"], "RON", "EXPENSE", "HBO Max", 29.99, 9, start)

    add_noise(ctx, uid, ctx.syscat["Divertisment"], "RON", "EXPENSE", 8, 80, 600,
              ["Cină Trattoria", "Cocktail bar", "Sushi Terra", "Steakhouse", "Club",
               "Degustare de vinuri"], start, weekend_bias=True, void_prob=0.008)
    add_noise(ctx, uid, ctx.syscat["Mâncare"], "RON", "EXPENSE", 12, 30, 250, FOOD, start)
    add_noise(ctx, uid, ctx.syscat["Transport"], "RON", "EXPENSE", 8, 20, 90,
              ["Uber", "Bolt", "Plin benzină OMV", "Parcare centru"], start)
    add_noise(ctx, uid, ctx.syscat["Utilități"], "RON", "EXPENSE", 1, 250, 450,
              ["Factură Enel + Engie", "Întreținere bloc"], start, winter_mult=1.5)
    add_noise(ctx, uid, cat_tech, "RON", "EXPENSE", 1, 100, 900,
              ["Căști wireless", "Accesorii birou", "Tastatură mecanică", "Periferice gaming"],
              start)
    insert_tx(ctx, uid, cat_tech, 6499, "RON", "EXPENSE", "iPhone 16 Pro", date(2025, 11, 14))


def seed_ioana(ctx, uid):
    """Mamă cu 2 copii: rată la casă, utilități și mâncare dominante, foarte disciplinată."""
    start = add_months(month_start(TODAY), -12)
    cat_casa = create_category(ctx, uid, "Casă", "EXPENSE", "🏠", "#78716c", start)
    cat_sanatate = create_category(ctx, uid, "Sănătate", "EXPENSE", "🏥", "#14b8a6", start)
    cat_edu = create_category(ctx, uid, "Educație copii", "EXPENSE", "🎓", "#f59e0b", start)

    create_recurring(ctx, uid, ctx.syscat["Salariu"], "RON", "INCOME",
                     "Salariu — birou contabilitate", 6800, 12, start, monthly_suffix=True)
    create_recurring(ctx, uid, ctx.syscat["Alte venituri"], "RON", "INCOME",
                     "Alocație copii (2)", 600, 20, start)
    create_recurring(ctx, uid, cat_casa, "RON", "EXPENSE", "Rată credit ipotecar", 2650, 28, start)
    create_recurring(ctx, uid, ctx.syscat["Utilități"], "RON", "EXPENSE",
                     "Factură curent + gaz", 480, 16, start)
    create_recurring(ctx, uid, ctx.syscat["Utilități"], "RON", "EXPENSE", "Abonament Digi", 55, 6, start)
    create_recurring(ctx, uid, cat_edu, "RON", "EXPENSE", "After-school copii", 900, 3, start)

    add_noise(ctx, uid, ctx.syscat["Mâncare"], "RON", "EXPENSE", 16, 40, 350, FOOD, start,
              void_prob=0.006)
    add_noise(ctx, uid, cat_sanatate, "RON", "EXPENSE", 2, 20, 250,
              ["Farmacia Catena", "Pediatru", "Vitamine copii", "Analize Synevo"], start)
    add_noise(ctx, uid, ctx.syscat["Transport"], "RON", "EXPENSE", 4, 10, 60, TRANSPORT, start)
    add_noise(ctx, uid, ctx.syscat["Divertisment"], "RON", "EXPENSE", 2, 30, 150,
              ["Loc de joacă", "Cinema cu copiii", "Cofetărie"], start, weekend_bias=True)


# ── Grupuri Split Bill ───────────────────────────────────────────────────────
def seed_group_apartament(ctx, mihai, ana, elena):
    """Chirie + utilități împărțite EQUAL, lunar; lunile vechi decontate integral
    (Elena plătește în EUR cu curs real), ultima lună rămâne cu solduri deschise."""
    rng = ctx.rng
    created = date(2025, 9, 28)
    gid = create_group(ctx, "Apartament Militari",
                       "Chirie și utilități pentru apartamentul din Militari",
                       "RON", mihai, [ana, elena], created)
    members = [mihai, ana, elena]
    utilities = {10: 380, 11: 520, 12: 610, 1: 650, 2: 590, 3: 460, 4: 380, 5: 330, 6: 310}

    m = date(2025, 10, 1)
    while m <= month_start(TODAY):
        add_expense(ctx, gid, mihai, "Chirie apartament", 2400, "RON", "EQUAL",
                    m, equal_split(2400, members, mihai))
        util_date = occ_date(m, 15)
        util_total = utilities.get(m.month)
        if util_total and util_date <= TODAY:
            add_expense(ctx, gid, mihai, "Utilități (curent, gaz, internet)", util_total,
                        "RON", "EQUAL", util_date, equal_split(util_total, members, mihai))
        # Decontare pe ~20 ale lunii: acoperă chiria + utilitățile lunii curente.
        pay_date = occ_date(m, 20)
        if pay_date <= TODAY and m < month_start(TODAY):
            owed = round(2400 / 3 + (util_total or 0) / 3, 2)
            add_payment(ctx, gid, ana, mihai, owed, "RON", pay_date,
                        rng.choice(["Revolut", "Transfer bancar"]))
            rate = round(rng.uniform(0.198, 0.203), 6)  # RON → EUR (curs creditor→plătitor)
            add_payment(ctx, gid, elena, mihai, owed, "RON", pay_date, "Revolut",
                        original_amount=round(owed * rate, 2),
                        original_currency_code="EUR", rate=rate)
        m = add_months(m, 1)
    return gid


def seed_group_grecia(ctx, ana, mihai, elena, andrei, ioana):
    """Vacanță cu split-uri variate (EQUAL/EXACT/SHARES), decontată PARȚIAL —
    balanțe deschise pentru demonstrația live de settle-up."""
    all5 = [ana, mihai, elena, andrei, ioana]
    gid = create_group(ctx, "Vacanță Grecia 2026",
                       "Santorini, 18–27 iunie 2026 — cazare, transport, ieșiri",
                       "RON", ana, [mihai, elena, andrei, ioana], date(2026, 5, 20))

    add_expense(ctx, gid, ana, "Cazare Santorini — Hotel Katerina", 7500, "RON", "EQUAL",
                date(2026, 6, 18), equal_split(7500, all5, ana))
    add_expense(ctx, gid, andrei, "Bilete avion București–Atena", 4250, "RON", "EXACT",
                date(2026, 6, 19),
                [{"user_id": ana, "owed_amount": 900}, {"user_id": andrei, "owed_amount": 900},
                 {"user_id": mihai, "owed_amount": 750}, {"user_id": elena, "owed_amount": 850},
                 {"user_id": ioana, "owed_amount": 850}])
    add_expense(ctx, gid, ana, "Feribot + transfer port", 900, "RON", "EQUAL",
                date(2026, 6, 21), equal_split(900, all5, ana))
    add_expense(ctx, gid, ioana, "Cină tavernă Oia", 640, "RON", "EQUAL",
                date(2026, 6, 24), equal_split(640, all5, ioana))
    add_expense(ctx, gid, mihai, "Închiriere ATV-uri", 800, "RON", "SHARES",
                date(2026, 6, 25),
                [{"user_id": ana, "owed_amount": 200}, {"user_id": andrei, "owed_amount": 200},
                 {"user_id": elena, "owed_amount": 200}, {"user_id": mihai, "owed_amount": 100},
                 {"user_id": ioana, "owed_amount": 100}])

    # Decontări parțiale (28–30 iunie). Rămân datorii deschise: Andrei → Ana (1680),
    # restul lui Mihai → Ana (680) și biletele de avion către Andrei (Mihai, Elena, Ioana).
    add_payment(ctx, gid, ioana, ana, 1680, "RON", date(2026, 6, 28), "Revolut")
    add_payment(ctx, gid, mihai, ana, 1000, "RON", date(2026, 6, 29), "Transfer bancar")
    rate = 0.2015
    add_payment(ctx, gid, elena, ana, 1680, "RON", date(2026, 6, 29), "Revolut",
                original_amount=round(1680 * rate, 2), original_currency_code="EUR", rate=rate)
    add_payment(ctx, gid, ana, andrei, 900, "RON", date(2026, 6, 30), "Revolut")
    add_payment(ctx, gid, ana, ioana, 128, "RON", date(2026, 6, 30), "Cash")
    add_payment(ctx, gid, ana, mihai, 200, "RON", date(2026, 6, 30), "Cash")
    return gid


def backdate_sp_rows(cur):
    """Rândurile create de sp_create_group_expense / sp_create_payment au created_at =
    NOW() (constant în tranzacția curentă). Le aliniem la datele reale ale evenimentelor,
    ca ordonarea cronologică din aplicație să fie corectă."""
    cur.execute("""UPDATE transactions
                   SET created_at = transaction_date::timestamptz + interval '18 hours'
                   WHERE created_at = now()""")
    cur.execute("""UPDATE group_expenses
                   SET created_at = expense_date::timestamptz + interval '19 hours'
                   WHERE created_at = now()""")
    cur.execute("""UPDATE payments
                   SET paid_at    = rate_date::timestamptz + interval '20 hours',
                       created_at = rate_date::timestamptz + interval '20 hours'
                   WHERE created_at = now()""")


# ── Sumar final ──────────────────────────────────────────────────────────────
def print_summary(cur, users, groups):
    print("\n" + "═" * 72)
    print("  DATE DEMO CREATE CU SUCCES")
    print("═" * 72)
    print(f"\n  Login (toți userii au parola: {DEMO_PASSWORD})\n")
    print(f"  {'Email':<34} {'Nume':<22} Monedă")
    print("  " + "─" * 68)
    for email, (uid, name, curr) in users.items():
        print(f"  {email:<34} {name:<22} {curr}")

    print("\n  Volum de date per user:\n")
    for email, (uid, name, _) in users.items():
        cur.execute("""SELECT COUNT(*) FILTER (WHERE kind = 'INCOME'),
                              COUNT(*) FILTER (WHERE kind = 'EXPENSE'),
                              COUNT(*) FILTER (WHERE status = 'VOIDED'),
                              MIN(transaction_date), MAX(transaction_date)
                       FROM transactions WHERE user_id = %s""", (uid,))
        inc, exp, voided, dmin, dmax = cur.fetchone()
        cur.execute("SELECT COUNT(*) FROM recurring_transaction_templates WHERE user_id = %s", (uid,))
        templates = cur.fetchone()[0]
        print(f"  {name:<18} {inc:>4} venituri, {exp:>4} cheltuieli "
              f"({voided} anulate), {templates} recurente, {dmin} → {dmax}")

    print("\n  Grupuri Split Bill (solduri rămase de plată):\n")
    for gid, gname in groups:
        cur.execute("""SELECT COUNT(*), COALESCE(SUM(amount), 0)
                       FROM group_expenses WHERE group_id = %s""", (gid,))
        n_exp, total = cur.fetchone()
        print(f"  {gname} — {n_exp} cheltuieli, total {total:.2f} RON")
        for email, (uid, name, _) in users.items():
            cur.execute("SELECT sp_get_user_unsettled(%s, %s)", (gid, uid))
            owed = cur.fetchone()[0]
            if owed and owed > 0:
                print(f"      {name:<18} mai are de achitat {owed:.2f} RON")
    print("\n" + "═" * 72)
    print("  Următorul pas: deschide PREZENTARE-DEMO.md pentru scenariul de prezentare.")
    print("═" * 72 + "\n")


def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    password = load_db_password()
    try:
        conn = psycopg2.connect(host=DB_HOST, port=DB_PORT, dbname=DB_NAME,
                                user=DB_USER, password=password)
    except psycopg2.OperationalError as e:
        sys.exit(f"Nu mă pot conecta la Postgres ({DB_HOST}:{DB_PORT}).\n"
                 f"Containerele Docker rulează? (cd BE ; docker compose up -d)\n\n{e}")

    rng = random.Random(SEED)
    print(f"Conectat la {DB_NAME}@{DB_HOST}:{DB_PORT}. Generez datele demo (azi = {TODAY})...")

    try:
        with conn:
            with conn.cursor() as cur:
                deleted = cleanup(cur)
                if deleted:
                    print(f"Curățat: {deleted} useri demo existenți (și toate datele lor).")

                ctx = Ctx(cur, rng)
                pw_hash = bcrypt.hashpw(DEMO_PASSWORD.encode(), bcrypt.gensalt(rounds=11)).decode()

                origin = add_months(month_start(TODAY), -12) - timedelta(days=20)
                ana = create_user(ctx, f"ana.popescu{DEMO_DOMAIN}", "ana.popescu",
                                  "Ana", "Popescu", "RON", origin, pw_hash)
                mihai = create_user(ctx, f"mihai.ionescu{DEMO_DOMAIN}", "mihai.ionescu",
                                    "Mihai", "Ionescu", "RON", origin + timedelta(days=55), pw_hash)
                elena = create_user(ctx, f"elena.georgescu{DEMO_DOMAIN}", "elena.georgescu",
                                    "Elena", "Georgescu", "EUR", origin + timedelta(days=25), pw_hash)
                andrei = create_user(ctx, f"andrei.radu{DEMO_DOMAIN}", "andrei.radu",
                                     "Andrei", "Radu", "RON", origin + timedelta(days=3), pw_hash)
                ioana = create_user(ctx, f"ioana.dumitrescu{DEMO_DOMAIN}", "ioana.dumitrescu",
                                    "Ioana", "Dumitrescu", "RON", origin + timedelta(days=8), pw_hash)

                print("Useri creați. Generez tranzacții, recurente și categorii custom...")
                seed_ana(ctx, ana)
                seed_mihai(ctx, mihai)
                seed_elena(ctx, elena)
                seed_andrei(ctx, andrei)
                seed_ioana(ctx, ioana)

                print("Creez grupurile Split Bill (prin procedurile aplicației)...")
                g1 = seed_group_apartament(ctx, mihai, ana, elena)
                g2 = seed_group_grecia(ctx, ana, mihai, elena, andrei, ioana)
                backdate_sp_rows(cur)
                print(f"Total tranzacții inserate: {ctx.tx_count}")

                users = {
                    f"ana.popescu{DEMO_DOMAIN}": (ana, "Ana Popescu", "RON"),
                    f"mihai.ionescu{DEMO_DOMAIN}": (mihai, "Mihai Ionescu", "RON"),
                    f"elena.georgescu{DEMO_DOMAIN}": (elena, "Elena Georgescu", "EUR"),
                    f"andrei.radu{DEMO_DOMAIN}": (andrei, "Andrei Radu", "RON"),
                    f"ioana.dumitrescu{DEMO_DOMAIN}": (ioana, "Ioana Dumitrescu", "RON"),
                }
                groups = [(g1, "Apartament Militari"), (g2, "Vacanță Grecia 2026")]

        # tranzacția e comisă; sumarul citește datele finale
        with conn.cursor() as cur:
            print_summary(cur, users, groups)
    finally:
        conn.close()


if __name__ == "__main__":
    main()
