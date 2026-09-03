"""
Client de test SignalR (Python) pentru ChatService.

Echivalentul lui chat-test.mjs. Python (spre deosebire de browser) poate seta
header-ul X-User-Id pe handshake-ul WebSocket, deci nu avem nevoie de Gateway/JWT.

Preconditii:
  - ChatService porneste pe :5003 (dotnet run)
  - Redis pornit (docker compose up -d)
  - userii 1 si 2 sunt membri ACTIVE ai grupului 1 (vezi ghidul: creare grup + invite + accept)

Instalare:
  pip install signalrcore

Rulare (in doua terminale):
  python chat-test.py 2      # asculta
  python chat-test.py 1      # trimite un mesaj dupa 1.5s
"""

import sys
import time
from signalrcore.hub_connection_builder import HubConnectionBuilder

GROUP_ID = 1
user_id = sys.argv[1] if len(sys.argv) > 1 else "1"

# groupId merge in query string; X-User-Id in header (citit de ChatHub.OnConnectedAsync).
server_url = f"http://localhost:5003/api/hubs/chat?groupId={GROUP_ID}"

hub = (
    HubConnectionBuilder()
    .with_url(server_url, options={"headers": {"X-User-Id": user_id}})
    .build()
)

# ── Evenimente primite de la server ──
hub.on_open(lambda: print(f"[{user_id}] conectat la grupul {GROUP_ID}"))
hub.on_close(lambda: print(f"[{user_id}] deconectat"))
hub.on_error(lambda e: print(f"[{user_id}] eroare:", e))

hub.on("MessageReceived",
       lambda m: print(f"[{user_id}] primit:", m[0]["content"], "de la", m[0]["senderUserId"], "(id", m[0]["id"], ")"))
hub.on("MessageEdited",
       lambda m: print(f"[{user_id}] editat:", m[0]["id"], "->", m[0]["content"]))
hub.on("MessageDeleted",
       lambda m: print(f"[{user_id}] sters:", m[0]["id"]))
hub.on("PresenceChanged",
       lambda p: print(f"[{user_id}] prezenta: user", p[0]["userId"], "online" if p[0]["online"] else "offline"))

hub.start()
time.sleep(1.5)

# Userul 1 trimite un mesaj; userul 2 doar asculta.
if user_id == "1":
    hub.send("SendMessage", [{"content": "salut din python", "replyToMessageId": None}])
    # Test edit/delete (decomenteaza, pune un id real returnat la "primit"):
    # time.sleep(1)
    # hub.send("EditMessage", [<msgId>, "modificat din python"])
    # hub.send("DeleteMessage", [<msgId>])

print(f"[{user_id}] Ctrl+C pentru iesire.")
try:
    while True:
        time.sleep(1)
except KeyboardInterrupt:
    hub.stop()
