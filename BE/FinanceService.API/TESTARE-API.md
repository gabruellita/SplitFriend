# Testare Finance Service API

Ghid pentru testarea microserviciului **Finance Service** (`:5002`) — categorii, tranzacții
(venituri/cheltuieli singulare) și template-uri recurente.

> **Două moduri de testare:**
> 1. **Prin Gateway** (`:5010`) — calea reală: cu JWT, Gateway-ul injectează `X-User-Id`. *Recomandat.*
> 2. **Direct pe serviciu** (`:5002`) — pentru debugging: trimiți manual header-ul `X-User-Id`.

---

## 0. Pornire

```powershell
# Infrastructura (din BE/). La PRIMA rulare după adăugarea Finance Service:
cd BE
docker compose down -v        # ⚠️ șterge datele existente — necesar o dată pt. schema nouă
docker compose up -d

# 3 terminale separate:
cd BE\IdentityService\IdentityService ; dotnet run        # :5067
cd BE\GateWay.API\GateWay.API ; dotnet run                # :5010  ← singurul port apelat
cd BE\FinanceService.API\FinanceService.API ; dotnet run  # :5002
```

- Swagger Finance: **http://localhost:5002/swagger**
- MailHog (email confirmare cont): **http://localhost:8025**
- pgAdmin (inspectează tabelele): **http://localhost:5050**

---

## 1. Obține un token (prin Gateway)

Finance Service nu are useri proprii — folosește identitatea din Identity Service.

```powershell
# 1.1 Înregistrare
$reg = @{ email="test@example.com"; username="testuser"; password="Test@1234!";
          firstName="Test"; lastName="User"; preferredCurrencyId=1 } | ConvertTo-Json
Invoke-RestMethod http://localhost:5010/api/identity/auth/register -Method Post -ContentType "application/json" -Body $reg

# 1.2 Confirmă emailul → ia tokenul din MailHog (http://localhost:8025)
$conf = @{ token = "TOKEN_DIN_MAILHOG" } | ConvertTo-Json
Invoke-RestMethod http://localhost:5010/api/identity/auth/confirm-email -Method Post -ContentType "application/json" -Body $conf

# 1.3 Login → salvează accessToken
$login = @{ email="test@example.com"; password="Test@1234!" } | ConvertTo-Json
$resp  = Invoke-RestMethod http://localhost:5010/api/identity/auth/login -Method Post -ContentType "application/json" -Body $login
$TOKEN = $resp.accessToken
$H = @{ Authorization = "Bearer $TOKEN" }
"Token salvat."
```

> `preferredCurrencyId=1` = RON (vezi `GET /api/identity/currencies`). Devine moneda implicită
> a tranzacțiilor dacă nu trimiți `currencyId`.

---

## 2. Categorii

| Metodă | Endpoint | Descriere |
|--------|----------|-----------|
| GET    | `/api/finance/categories` | Listă (system + ale tale) |
| POST   | `/api/finance/categories` | Creează categorie custom |
| PUT    | `/api/finance/categories/{id}` | Modifică o categorie custom proprie |
| DELETE | `/api/finance/categories/{id}` | Dezactivează (soft delete) o categorie custom |

```powershell
# Listă (vezi cele 6 categorii system seedate)
Invoke-RestMethod http://localhost:5010/api/finance/categories -Headers $H

# Creează o categorie custom de cheltuieli
$cat = @{ name="Abonamente"; kind="EXPENSE"; icon="📺"; color="#8b5cf6" } | ConvertTo-Json
$newCat = Invoke-RestMethod http://localhost:5010/api/finance/categories -Method Post -Headers $H -ContentType "application/json" -Body $cat
$catId = $newCat.id

# Modifică
$upd = @{ name="Abonamente lunare"; icon="📺"; color="#8b5cf6" } | ConvertTo-Json
Invoke-RestMethod http://localhost:5010/api/finance/categories/$catId -Method Put -Headers $H -ContentType "application/json" -Body $upd

# Dezactivează
Invoke-RestMethod http://localhost:5010/api/finance/categories/$catId -Method Delete -Headers $H
```

> Categoriile **system** (`isSystem=true`) sunt read-only: PUT/DELETE pe ele → **404**.
> `kind` trebuie să fie `INCOME` sau `EXPENSE`.

---

## 3. Tranzacții (venituri & cheltuieli singulare)

| Metodă | Endpoint | Descriere |
|--------|----------|-----------|
| GET    | `/api/finance/transactions?from=&to=&categoryId=&kind=` | Listă filtrată (doar POSTED) |
| GET    | `/api/finance/transactions/summary?from=&to=` | Total venituri/cheltuieli + pe categorii |
| GET    | `/api/finance/transactions/{id}` | O tranzacție |
| POST   | `/api/finance/transactions` | Adaugă tranzacție |
| PUT    | `/api/finance/transactions/{id}` | Modifică (doar POSTED) |
| DELETE | `/api/finance/transactions/{id}` | Anulează (soft → VOIDED) |

```powershell
# Cheltuială (categoryId 3 = Mâncare; currencyId lipsă → moneda preferată din JWT)
$tx1 = @{ amount=50.5; kind="EXPENSE"; transactionDate="2026-06-03"; categoryId=3; description="Pranz" } | ConvertTo-Json
$t1 = Invoke-RestMethod http://localhost:5010/api/finance/transactions -Method Post -Headers $H -ContentType "application/json" -Body $tx1

# Venit (categoryId 1 = Salariu)
$tx2 = @{ amount=4200; kind="INCOME"; transactionDate="2026-06-01"; categoryId=1; description="Salariu" } | ConvertTo-Json
Invoke-RestMethod http://localhost:5010/api/finance/transactions -Method Post -Headers $H -ContentType "application/json" -Body $tx2

# Listă toate
Invoke-RestMethod http://localhost:5010/api/finance/transactions -Headers $H

# Filtrare: doar cheltuieli din iunie
Invoke-RestMethod "http://localhost:5010/api/finance/transactions?from=2026-06-01&to=2026-06-30&kind=EXPENSE" -Headers $H

# Sumar (totaluri + breakdown pe categorii)
Invoke-RestMethod "http://localhost:5010/api/finance/transactions/summary" -Headers $H

# Modifică tranzacția 1
$upd = @{ amount=60; kind="EXPENSE"; transactionDate="2026-06-03"; categoryId=3; description="Pranz + cafea" } | ConvertTo-Json
Invoke-RestMethod http://localhost:5010/api/finance/transactions/$($t1.id) -Method Put -Headers $H -ContentType "application/json" -Body $upd

# Anulează (VOIDED) — dispare din listă/sumar
Invoke-RestMethod http://localhost:5010/api/finance/transactions/$($t1.id) -Method Delete -Headers $H
```

### Câmpuri request (POST/PUT tranzacție)
| Câmp | Tip | Obligatoriu | Note |
|------|-----|:-----------:|------|
| `amount` | decimal | ✅ | > 0 |
| `kind` | string | ✅ | `INCOME` / `EXPENSE` |
| `transactionDate` | date `YYYY-MM-DD` | ✅ | |
| `categoryId` | long | ❌ | trebuie să fie a ta/system și cu același `kind` |
| `currencyId` | long | ❌ | fallback pe moneda preferată din JWT |
| `description` | string | ❌ | max 500 |

---

## 4. Template-uri recurente

| Metodă | Endpoint | Descriere |
|--------|----------|-----------|
| GET    | `/api/finance/recurring-templates` | Listă |
| GET    | `/api/finance/recurring-templates/{id}` | Un template |
| POST   | `/api/finance/recurring-templates` | Creează template |
| PUT    | `/api/finance/recurring-templates/{id}` | Modifică |
| DELETE | `/api/finance/recurring-templates/{id}` | Dezactivează |
| POST   | `/api/finance/recurring-templates/run-due` | **Generează tranzacțiile scadente** |

```powershell
# Template: chirie lunară, scadentă AZI (startDate = azi → next_run_date = azi)
$tmpl = @{ amount=1500; kind="EXPENSE"; frequency="MONTHLY"; intervalCount=1;
           startDate="2026-06-03"; categoryId=5; description="Chirie" } | ConvertTo-Json
Invoke-RestMethod http://localhost:5010/api/finance/recurring-templates -Method Post -Headers $H -ContentType "application/json" -Body $tmpl

# Listă template-uri (vezi nextRunDate, isActive)
Invoke-RestMethod http://localhost:5010/api/finance/recurring-templates -Headers $H

# RULEAZĂ generarea → creează tranzacțiile scadente, avansează nextRunDate
Invoke-RestMethod http://localhost:5010/api/finance/recurring-templates/run-due -Method Post -Headers $H
#   → { "generatedCount": 1 }

# Verifică: tranzacția generată are templateId setat
Invoke-RestMethod http://localhost:5010/api/finance/transactions -Headers $H
```

### Câmpuri request (POST template)
| Câmp | Tip | Obligatoriu | Note |
|------|-----|:-----------:|------|
| `amount` | decimal | ✅ | > 0 |
| `kind` | string | ✅ | `INCOME` / `EXPENSE` |
| `frequency` | string | ✅ | `DAILY` / `WEEKLY` / `MONTHLY` / `YEARLY` |
| `intervalCount` | int | ✅ | ≥ 1 (ex. 2 + WEEKLY = la 2 săptămâni) |
| `startDate` | date | ✅ | `next_run_date` inițial = `startDate` |
| `endDate` | date | ❌ | ≥ `startDate`; după depășire → template inactiv |
| `categoryId` / `currencyId` / `description` | — | ❌ | ca la tranzacții |

### Cum funcționează `run-due`
- Ia template-urile **active** ale userului cu `next_run_date <= azi`.
- Pentru fiecare, în buclă cât timp e scadent (și ≤ `endDate`): creează o tranzacție cu `templateId` setat,
  apoi avansează `next_run_date` cu `frequency × intervalCount`.
- Dacă următoarea dată depășește `endDate` → template-ul devine `isActive=false`.
- Apelează de mai multe ori: a doua oară nu mai generează nimic (nimic scadent) → `generatedCount: 0`.
- Pentru a vedea recurența „prinzând din urmă", creează un template cu `startDate` în trecut (ex. acum 3 luni,
  `MONTHLY`) și rulează `run-due` → se generează câte o tranzacție per lună scadentă.

### 4.1 Testarea job-ului de fundal (`RecurringGenerationJob`)

Pe lângă `run-due` (manual, per-user), un **hosted `BackgroundService`** generează automat tranzacțiile
scadente ale **tuturor** userilor, cu aceeași logică (`RecurringGenerationEngine`). Cadența vine din secțiunea
`RecurringJob` (`RecurringJobOptions`):

| Cheie | Default (`appsettings.json`) | Dev (`appsettings.Development.json`) |
|-------|------------------------------|--------------------------------------|
| `StartupDelaySeconds` | 30 | 5 |
| `IntervalSeconds` | 86400 (24h) | 30 |

În dev jobul rulează la ~5s după pornire, apoi din 30 în 30s — deci e observabil fără să aștepți 24h.

```powershell
# 1) Creează un template scadent AZI, dar NU apela run-due:
$t = @{ amount=99; kind="EXPENSE"; frequency="MONTHLY"; intervalCount=1;
        startDate="2026-06-05"; categoryId=3; description="TEST JOB background" } | ConvertTo-Json
Invoke-RestMethod http://localhost:5010/api/finance/recurring-templates -Method Post -Headers $H -ContentType "application/json" -Body $t

# 2) Confirmă că tranzacția NU există încă (jobul n-a rulat / template proaspăt):
(Invoke-RestMethod http://localhost:5010/api/finance/transactions -Headers $H |
  Where-Object description -eq "TEST JOB background").Count   # → 0
```

Apoi **repornește Finance** (în terminalul lui: `Ctrl+C` → `dotnet run`). În consolă vei vedea, la pornire,
cadența activă, apoi la fiecare ciclu rezultatul:

```
info: ...RecurringGenerationJob[0]  Job recurenta pornit: prima rulare in 5s, apoi la fiecare 30s.
info: ...RecurringGenerationJob[0]  Job recurenta: N tranzactii generate la 2026-06-05
```

Verifică efectul — tranzacția apare *fără* să fi apelat `run-due`:

```powershell
Invoke-RestMethod http://localhost:5010/api/finance/transactions -Headers $H |
  Where-Object description -eq "TEST JOB background"
```

> `N` numără template-urile scadente ale **tuturor** userilor (jobul e global). Pentru un `N` curat, asigură-te
> că doar template-ul tău e scadent. Ca să revii la cadența de producție în dev, șterge secțiunea `RecurringJob`
> din `appsettings.Development.json` (cade pe default-urile 30s / 24h).

---

## 5. Verificări de securitate (prin Gateway)

```powershell
# Fără token → Gateway răspunde 401 (nu ajunge la serviciu)
Invoke-RestMethod http://localhost:5010/api/finance/transactions

# X-User-Id falsificat fără token → Gateway îl șterge → 401
Invoke-RestMethod http://localhost:5010/api/finance/transactions -Headers @{ "X-User-Id" = "1" }
```

> În PowerShell, `Invoke-RestMethod` aruncă excepție la 4xx. Ca să vezi codul:
> `try { Invoke-RestMethod ... } catch { $_.Exception.Response.StatusCode }`

---

## 6. Testare directă pe serviciu (`:5002`) — doar debugging

Ocolește Gateway-ul; trebuie să trimiți **manual** `X-User-Id` (în producție vine doar de la Gateway).

- **Swagger** (http://localhost:5002/swagger): fiecare endpoint are un câmp **`X-User-Id`** — completează-l (ex. `1`) și apelează.
- **PowerShell:**
  ```powershell
  $Hd = @{ "X-User-Id" = "1"; "X-User-Currency" = "1" }
  Invoke-RestMethod http://localhost:5002/api/categories -Headers $Hd
  ```
- Fără `X-User-Id` pe o rută `/api/...` → **401** din `CurrentUserMiddleware`.

---

## 7. Coduri de răspuns

| Cod | Când |
|-----|------|
| 200 | GET / run-due OK |
| 201 | Creare (POST) — întoarce `{ id }` |
| 204 | Update / Delete OK |
| 400 | Validare eșuată (amount ≤ 0, kind invalid, monedă lipsă etc.) |
| 401 | Lipsă identitate (fără JWT prin Gateway / fără `X-User-Id` direct) |
| 404 | Resursa nu există / nu e a ta / categorie system la modificare |
| 409 | Conflict (constraint UNIQUE) |
| 500 | Eroare internă |

Corpul erorilor: `{ "error": "mesaj" }` (din `GlobalExceptionMiddleware`).
Erorile de validare: format `ValidationProblemDetails` (cu câmpurile invalide).
