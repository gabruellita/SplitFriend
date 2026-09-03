import * as signalR from "@microsoft/signalr";

const groupId = 1;
const userId  = process.argv[2] ?? "1";   // ruleaza cu: node chat-test.mjs 1  (si in alt terminal: node chat-test.mjs 2)

const conn = new signalR.HubConnectionBuilder()
  .withUrl(`http://localhost:5003/api/hubs/chat?groupId=${groupId}`, {
    headers: { "X-User-Id": userId },
    transport: signalR.HttpTransportType.LongPolling   // header-e custom merg pe long-polling fara browser
  })
  .build();

conn.on("MessageReceived", m => console.log(`[${userId}] primit:`, m.content, "de la", m.senderUserId));
conn.on("MessageEdited",   m => console.log(`[${userId}] editat:`, m.id, "→", m.content));
conn.on("MessageDeleted",  m => console.log(`[${userId}] sters:`, m.id));

await conn.start();
console.log(`[${userId}] conectat la grupul ${groupId}`);

if (userId === "1") {
  setTimeout(() => conn.invoke("SendMessage", { content: "salut din test", replyToMessageId: null }), 1500);
}
