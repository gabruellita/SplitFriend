# 🚀 Ghid pornire Identity Service

## Cerințe prealabile

| Tool | Versiune minimă | Verificare |
|------|----------------|------------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 | `dotnet --version` |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | orice | `docker --version` |

---

## Pasul 1 — Pornește infrastructura (Docker)

Din folderul `BE/` rulează:

```bash
docker compose up -d
```

Asta pornește 4 containere:

| Container | Ce face | Port |
|-----------|---------|------|
| `identity_postgres` | Baza de date PostgreSQL | `5432` |
| `identity_redis` | Cache pentru sesiuni JWT | `6379` |
| `identity_mailhog` | Server SMTP fake (captează email-urile) | `1025` (SMTP) / `8025` (UI) |
| `identity_pgadmin` | Interfață grafică pentru DB | `5050` |

> **Notă:** La primul `docker compose up`, PostgreSQL rulează automat `IdentityService/DataBase/init/01_schema.sql`
> și creează toate tabelele. Durează ~10 secunde.

Verifică că toate containerele sunt `healthy`:

```bash
docker compose ps
```

---

## Pasul 2 — Pornește aplicația .NET

### Din terminal:

```bash
cd BE/IdentityService/IdentityService
dotnet run
```

### Din Visual Studio / Rider:
- Setează proiectul de startup pe **`IdentityService`** (cel cu `.API`)
- Apasă **F5** (cu debugging) sau **Ctrl+F5** (fără)

Browserul se deschide automat pe **[http://localhost:5067/swagger](http://localhost:5067/swagger)**.

---

## Pasul 3 — Testare prin Swagger

Accesează **[http://localhost:5067/swagger](http://localhost:5067/swagger)**

### Flux complet de testare:

#### 1. Înregistrare cont nou
**`POST /api/auth/register`**
```json
{
  "email": "test@example.com",
  "username": "testuser",
  "password": "Test1234!",
  "preferredCurrencyId": 1
}
```
Răspuns `201` → contul e creat cu status `PENDING`.

---

#### 2. Confirmă email-ul
Deschide **[http://localhost:8025](http://localhost:8025)** (MailHog) și copiază token-ul din emailul primit.

**`POST /api/auth/confirm-email`**
```json
{
  "token": "<token-ul din email>"
}
```
Răspuns `200` → contul devine `ACTIVE`.

---

#### 3. Login
**`POST /api/auth/login`**
```json
{
  "email": "test@example.com",
  "password": "Test1234!"
}
```
Răspuns `200` → primești `accessToken` și `refreshToken`.

---

#### 4. Autorizare în Swagger
- Click pe butonul **🔒 Authorize** (dreapta sus în Swagger)
- În câmpul `Bearer` introdu: `<accessToken-ul primit la login>`
- Click **Authorize** → acum toate endpoint-urile protejate funcționează

---

#### 5. Refresh token
**`POST /api/auth/refresh`**
```json
{
  "refreshToken": "<refreshToken-ul primit la login>"
}
```
Răspuns `200` → primești un nou pereche `accessToken` + `refreshToken`.

---

#### 6. Logout
**`POST /api/auth/logout`**
```json
{
  "refreshToken": "<refreshToken-ul curent>"
}
```
Răspuns `204` → refresh token-ul e revocat.

---

## Adrese utile

| Serviciu | URL | Credențiale |
|---------|-----|-------------|
| **Swagger UI** | http://localhost:5067/swagger | — |
| **MailHog** (email-uri) | http://localhost:8025 | — |
| **pgAdmin** (baza de date) | http://localhost:5050 | `admin@finance.local` / `admin123` |

### Conectare pgAdmin la baza de date:
1. Deschide http://localhost:5050
2. Click dreapta **Servers** → **Register** → **Server**
3. Tab **General**: Name = `IdentityDB`
4. Tab **Connection**:
   - Host: `identity_postgres`
   - Port: `5432`
   - Database: `finance_db`
   - Username: `finance_user`
   - Password: `FinanceP@ss2026!`

---

## Oprire

Din folderul `BE/`:

```bash
# Oprește containerele (păstrează datele)
docker compose stop

# Oprește și șterge containerele + volumele (reset complet)
docker compose down -v
```
