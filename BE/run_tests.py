#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Ruleaza toate testele unitare ale FinanceApp (dotnet test) si afiseaza
un rezumat colorat in consola.

Utilizare:
    python run_tests.py              # ruleaza tot
    python run_tests.py --verbose    # afiseaza fiecare test in parte
    python run_tests.py --no-build    # sare peste recompilare (mai rapid)

Nu necesita Docker sau servicii pornite — testele unitare ruleaza izolat.
"""

import os
import re
import sys
import time
import subprocess

# ─── Proiectele de teste (cale relativa la acest script, aflat in BE/) ──────────
PROJECTS = [
    ("Finance",      "FinanceService.API/FinanceService.Tests"),
    ("Currency",     "CurrencyService.API/CurrencyService.Tests"),
    ("Identity",     "IdentityService/IdentityService.Tests"),
    ("Notification", "NotificationService.API/NotificationService.Tests"),
    ("Gateway",      "GateWay.API/GateWay.Tests"),
]

BE_DIR = os.path.dirname(os.path.abspath(__file__))

# ─── Culori ANSI (cu activare pe Windows) ───────────────────────────────────────
class C:
    RESET = "\033[0m"; BOLD = "\033[1m"; DIM = "\033[2m"
    GREEN = "\033[92m"; RED = "\033[91m"; YELLOW = "\033[93m"
    CYAN = "\033[96m"; BLUE = "\033[94m"; GREY = "\033[90m"

def enable_ansi():
    # consola Windows e adesea cp1252 -> fortam UTF-8 ca sa mearga diacriticele si box-drawing
    for stream in (sys.stdout, sys.stderr):
        try:
            stream.reconfigure(encoding="utf-8")
        except Exception:
            pass
    if os.name == "nt":
        try:
            import ctypes
            k = ctypes.windll.kernel32
            k.SetConsoleMode(k.GetStdHandle(-11), 7)  # ENABLE_VIRTUAL_TERMINAL_PROCESSING
        except Exception:
            pass

# Linia de rezumat a lui `dotnet test`:
#   Passed!  - Failed: 0, Passed: 28, Skipped: 0, Total: 28, Duration: 50 ms - ...
SUMMARY_RE = re.compile(
    r"(Passed|Failed)!\s*-\s*Failed:\s*(\d+),\s*Passed:\s*(\d+),"
    r"\s*Skipped:\s*(\d+),\s*Total:\s*(\d+),\s*Duration:\s*([\d.,]+\s*\w+)"
)

def run_project(name, rel_path, verbose, no_build):
    path = os.path.join(BE_DIR, rel_path.replace("/", os.sep))
    cmd = ["dotnet", "test", path, "--nologo"]
    if no_build:
        cmd.append("--no-build")
    if verbose:
        cmd += ["--logger", "console;verbosity=detailed"]

    start = time.time()
    proc = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace")
    elapsed = time.time() - start
    out = (proc.stdout or "") + "\n" + (proc.stderr or "")

    m = SUMMARY_RE.search(out)
    if m:
        result = {
            "ok": m.group(1) == "Passed" and proc.returncode == 0,
            "failed": int(m.group(2)), "passed": int(m.group(3)),
            "skipped": int(m.group(4)), "total": int(m.group(5)),
            "duration": m.group(6), "elapsed": elapsed, "raw": out,
        }
    else:
        # build esuat / nicio linie de rezumat
        result = {
            "ok": False, "failed": 0, "passed": 0, "skipped": 0, "total": 0,
            "duration": "-", "elapsed": elapsed, "raw": out, "error": True,
        }
    return result

def hr(char="─", width=72):
    return char * width

def main():
    enable_ansi()
    verbose  = "--verbose" in sys.argv or "-v" in sys.argv
    no_build = "--no-build" in sys.argv

    print()
    print(f"{C.BOLD}{C.CYAN}╔{hr('═', 70)}╗{C.RESET}")
    title = "FinanceApp — Suita de teste unitare (dotnet test)"
    print(f"{C.BOLD}{C.CYAN}║{C.RESET} {C.BOLD}{title}{C.RESET}{' ' * (69 - len(title))}{C.BOLD}{C.CYAN}║{C.RESET}")
    print(f"{C.BOLD}{C.CYAN}╚{hr('═', 70)}╝{C.RESET}")
    print()

    results = []
    for name, rel in PROJECTS:
        print(f"  {C.DIM}▶ rulez {name:<13}{C.RESET}", end="", flush=True)
        r = run_project(name, rel, verbose, no_build)
        results.append((name, r))
        if verbose:
            print()  # output detaliat deja afisat de subprocess? nu — e capturat; il afisam jos
        if r.get("error"):
            print(f"\r  {C.RED}✗ {name:<13} EROARE DE BUILD{C.RESET}{' ' * 30}")
        elif r["ok"]:
            print(f"\r  {C.GREEN}✓ {name:<13} {r['passed']} trecute{C.RESET}{' ' * 30}")
        else:
            print(f"\r  {C.RED}✗ {name:<13} {r['failed']} picate / {r['total']}{C.RESET}{' ' * 30}")

    # ─── Tabel rezumat ───────────────────────────────────────────────────────
    print()
    print(f"  {C.BOLD}{'Serviciu':<14}{'Trecute':>9}{'Picate':>8}{'Sărite':>8}{'Total':>7}{'Durată':>12}{C.RESET}")
    print(f"  {C.GREY}{hr('─', 58)}{C.RESET}")

    tot_p = tot_f = tot_s = tot_t = 0
    for name, r in results:
        tot_p += r["passed"]; tot_f += r["failed"]; tot_s += r["skipped"]; tot_t += r["total"]
        if r.get("error"):
            status = f"{C.RED}BUILD FAIL{C.RESET}"
            print(f"  {name:<14}{status:>9}")
            continue
        color = C.GREEN if r["ok"] else C.RED
        print(f"  {color}{name:<14}{r['passed']:>9}{r['failed']:>8}{r['skipped']:>8}"
              f"{r['total']:>7}{r['duration']:>12}{C.RESET}")

    print(f"  {C.GREY}{hr('─', 58)}{C.RESET}")
    print(f"  {C.BOLD}{'TOTAL':<14}{tot_p:>9}{tot_f:>8}{tot_s:>8}{tot_t:>7}{C.RESET}")
    print()

    all_ok = all(r["ok"] for _, r in results)
    if all_ok:
        print(f"  {C.BOLD}{C.GREEN}✓ TOATE TESTELE TREC — {tot_p}/{tot_t}{C.RESET}")
    else:
        print(f"  {C.BOLD}{C.RED}✗ EXISTĂ EȘECURI — {tot_f} picate, {tot_p} trecute din {tot_t}{C.RESET}")
        # afiseaza output-ul brut al proiectelor cu probleme
        for name, r in results:
            if not r["ok"]:
                print(f"\n{C.YELLOW}── Output {name} ──{C.RESET}")
                tail = "\n".join(l for l in r["raw"].splitlines()
                                 if ("error" in l.lower() or "fail" in l.lower()))[:4000]
                print(tail or r["raw"][-2000:])
    print()

    # daca s-a cerut verbose, listeaza testele individuale (din output capturat)
    if verbose:
        for name, r in results:
            print(f"\n{C.BLUE}── Teste {name} ──{C.RESET}")
            for line in r["raw"].splitlines():
                ls = line.strip()
                if ls.startswith("Passed ") or "[PASS]" in ls:
                    print(f"  {C.GREEN}✓{C.RESET} {ls}")
                elif ls.startswith("Failed ") or "[FAIL]" in ls:
                    print(f"  {C.RED}✗{C.RESET} {ls}")

    sys.exit(0 if all_ok else 1)

if __name__ == "__main__":
    main()
