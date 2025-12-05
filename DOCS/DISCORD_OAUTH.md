# Discord OAuth setup for MeetLines

This document explains how to create a Discord Developer App, configure redirect URIs, and how to use the server-side exchange endpoint we added (`POST /api/auth/oauth/discord`). It also includes a small frontend example.

IMPORTANT: keep `DISCORD_CLIENT_SECRET` private and do not commit it to public repos.

## 1) Create a Discord Application

1. Go to https://discord.com/developers/applications and sign in with your Discord account.
2. Click **New Application** and give it a name (e.g. "MeetLines OAuth").
3. In the application page, go to **OAuth2 > General** and note the `Client ID` and `Client Secret`.
4. Under **OAuth2 > Redirects**, add the redirect URI you will use. Examples:
   - For server-side flow (recommended): `https://meet-lines.com/signin-discord`
   - For local testing: `http://localhost:3000/signin-discord`

   If you use tenant subdomains, register the main wildcard redirect or each specific redirect your app will use. Discord requires exact matches.

5. Under **OAuth2 > Scopes**, your frontend should request `identify` and `email` (if you want email).

## 2) Set env vars on your server / `.env`

Add these environment variables (we added placeholders already in `.env`):

```
DISCORD_CLIENT_ID=<your-client-id>
DISCORD_CLIENT_SECRET=<your-client-secret>
DISCORD_REDIRECT_URI=https://meet-lines.com/signin-discord
```

Put them in your host environment (systemd, Docker Compose `environment:` section, or your hosting panel). Avoid committing secrets.

## 3) Server-side flow (what we implemented)

Flow summary:
- Frontend sends user to Discord authorization URL:
  `https://discord.com/api/oauth2/authorize?client_id=<CLIENT_ID>&redirect_uri=<REDIRECT_URI>&response_type=code&scope=identify%20email`
- Discord redirects back to `<REDIRECT_URI>?code=<CODE>`.
- Frontend sends `POST /api/auth/oauth/discord` to backend with JSON `{ "code": "<CODE>", "redirectUri": "<REDIRECT_URI>" }`.
- Backend exchanges code for access token, fetches user info from Discord, and calls the internal `OAuthLoginUseCase` to create / return app tokens (JWT `accessToken` + `refreshToken`).

Example frontend redirect (open link):

```
https://discord.com/api/oauth2/authorize?client_id=<CLIENT_ID>&redirect_uri=<ENCODED_REDIRECT>&response_type=code&scope=identify%20email
```

Example `POST` the frontend can call after receiving the `code` param:

```bash
curl -X POST https://meet-lines.com/api/auth/oauth/discord \
  -H "Content-Type: application/json" \
  -d '{ "code":"<CODE>", "redirectUri":"https://meet-lines.com/signin-discord" }'
```

Successful response: the existing `ApiResponse<LoginResponse>` JSON (same shape as other OAuth logins).

## 4) Frontend example (React minimal)

1. Redirect user to Discord auth URL (example React handler):

```js
function startDiscordLogin() {
  const clientId = process.env.REACT_APP_DISCORD_CLIENT_ID;
  const redirect = encodeURIComponent(process.env.REACT_APP_DISCORD_REDIRECT);
  const url = `https://discord.com/api/oauth2/authorize?client_id=${clientId}&redirect_uri=${redirect}&response_type=code&scope=identify%20email`;
  window.location.href = url;
}
```

2. After redirect back to your frontend at `/signin-discord`, read `code` from query string and POST to your backend endpoint:

```js
// in SigninDiscord component
useEffect(() => {
  const params = new URLSearchParams(window.location.search);
  const code = params.get('code');
  if (!code) return;

  fetch('/api/auth/oauth/discord', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ code, redirectUri: process.env.REACT_APP_DISCORD_REDIRECT })
  })
  .then(r => r.json())
  .then(data => {
    // store tokens, redirect to app
  });
}, []);
```

## 5) Tenant-aware Redirects

If you support per-tenant subdomains (e.g. `acme.meet-lines.com`), you have two options:

- Register each tenant redirect in Discord Developer App (tedious and not scalable).
- Use a single central redirect (e.g. `https://meet-lines.com/signin-discord`) and include the target tenant in the `state` param when requesting authorization, so the central redirect can forward the user to the tenant after login. Example:

```
/auth?state=tenant=acme
```

Then the frontend or backend reads `state` and redirects to tenant after issuing tokens.

## 6) Security notes

- Keep `DISCORD_CLIENT_SECRET` secret.
- Use `state` parameter to protect against CSRF and to carry post-login target tenant.
- Always validate scopes returned by Discord if you rely on `email`.

## 7) Troubleshooting

- If token exchange fails, inspect Discord Developer > OAuth2 logs and the backend logs.
- For local testing, ensure your redirect URI registered in Discord exactly matches the value the browser uses (including port).


---

If you want, I can also:
- Add a small React example project file under `examples/` and a `.env.example` for the frontend.
- Wire tenant-aware `state` handling in `AuthController` to automatically redirect to the tenant subdomain after successful OAuth exchange.
- Add automated tests for the Discord exchange endpoint (mocking Discord API).

Tell me which of those (if any) you want next.