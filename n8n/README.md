# Balochistan Academy Portal — n8n Automation Workflows

Importable n8n workflows that automate student onboarding and messaging for the
Balochistan Academy Portal. Built with the same webhook pattern as the existing
landing-page lead form.

| File | Workflow | Webhook (production URL) |
|------|----------|--------------------------|
| `landing2-leads-workflow.json` | Lead capture (landing pages) | `POST /webhook/bap-lead` |
| `student-registration-workflow.json` | Student registration & onboarding | `POST /webhook/bap-register` |
| `broadcast-messaging-workflow.json` | Announcement broadcast (groups · chats · email) | `POST /webhook/bap-broadcast` |

> ⚠️ n8n exposes **two** URLs per webhook:
> - `…/webhook-test/<path>` — only live for ~120s after you click **"Listen for test event"**.
> - `…/webhook/<path>` — only live while the workflow toggle is **Active**.
> The app always calls the production (`/webhook/`) URL, so the workflow **must be Active**.

---

## 1. Import & activate

1. Open n8n → http://localhost:5678
2. **Workflows → Import from File** → pick a `*.json` from this folder.
3. Open it, assign credentials on each colored node (see below), then flip **Active** ON (top-right).
4. Repeat for each workflow.

## 2. Credentials each workflow needs

Create these once under **n8n → Credentials**, then select them on the matching nodes.

| Node | Credential type | Where to get it |
|------|-----------------|-----------------|
| **Save to Google Sheets** | Google Sheets OAuth2 | Google Cloud console → enable Sheets API |
| **Welcome Email / Email Blast** | Gmail OAuth2 (or swap node for SMTP) | Google Cloud console → enable Gmail API |
| **Welcome WhatsApp / WhatsApp Broadcast** | WhatsApp Business Cloud (Meta) | Meta for Developers → WhatsApp → API setup |
| **Telegram Group Notify / Telegram Group · Channel** | Telegram API | Talk to **@BotFather** → `/newbot` → copy token |

## 3. Placeholders to replace

Open each workflow and replace these literal placeholders:

- `YOUR_GOOGLE_SHEET_ID` — the spreadsheet ID (from its URL). Add a tab named **Students** with header row: `name, phone, email, grade, medium, city, source, registeredAt`.
- `YOUR_WHATSAPP_PHONE_NUMBER_ID` — the **Phone Number ID** from your Meta WhatsApp app (not the phone number itself).
- `YOUR_TELEGRAM_GROUP_CHAT_ID` — the target group/channel ID. Add your bot to the group, then ask **@get_id_bot** or call `getUpdates`. Group IDs are negative (e.g. `-1001234567890`).

> WhatsApp note: outside the 24-hour customer-service window, Meta only allows **pre-approved template messages**. For the welcome message, switch the WhatsApp node's operation to **Send Template** and reference an approved template, or trigger onboarding right after the student first messages your number.

---

## 4. Workflow shapes

### Student Registration & Onboarding
```
Register Webhook → Normalize Student ─┬─→ Respond OK (instant 200)
                                       └─→ Save to Google Sheets ─┬─→ Welcome Email (Gmail)
                                                                   ├─→ Welcome WhatsApp
                                                                   └─→ Telegram Group Notify
```
The student gets an immediate success response while the row write + three channel
messages run asynchronously.

**Expected POST body:**
```json
{
  "name": "Ali Khan",
  "phone": "03001234567",
  "email": "ali@example.com",
  "grade": "9",
  "medium": "English",
  "city": "Quetta",
  "source": "app"
}
```

### Announcement Broadcast
```
Broadcast Webhook → Normalize Message ─┬─→ Respond OK
                                        ├─→ Telegram Group / Channel
                                        ├─→ WhatsApp Broadcast
                                        └─→ Email Blast
```
Sends one announcement to a Telegram group/channel, a WhatsApp recipient, and an
email in a single call. Delete any branch you don't need.

**Expected POST body:**
```json
{
  "title": "Mock Test Tomorrow",
  "body": "Grade 9 Physics mock test starts at 5 PM. Be ready!",
  "telegramChatId": "-1001234567890",
  "whatsappTo": "03001234567",
  "emailTo": "students@example.com"
}
```

---

## 5. Calling from the app (optional)

The landing forms already POST to n8n. To wire registration the same way, add to `.env`:

```
VITE_N8N_REGISTER_URL=http://localhost:5678/webhook/bap-register
VITE_N8N_BROADCAST_URL=http://localhost:5678/webhook/bap-broadcast
```

Then `fetch(import.meta.env.VITE_N8N_REGISTER_URL, { method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(payload) })`
— identical to `submitLead()` in `src/views/Landing3Page.vue`.

## 6. Quick test (no app needed)

```bash
# registration
curl -X POST http://localhost:5678/webhook/bap-register \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Student","phone":"03001234567","email":"test@example.com","grade":"9","medium":"English","city":"Quetta"}'

# broadcast
curl -X POST http://localhost:5678/webhook/bap-broadcast \
  -H "Content-Type: application/json" \
  -d '{"title":"Hello","body":"First broadcast from n8n","whatsappTo":"03001234567","emailTo":"test@example.com"}'
```
A `200` with `{"ok":true,...}` means the pipeline ran. A `404` means the workflow isn't Active.
