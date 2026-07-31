---
title: API / Documentation Discrepancies
tags: [analysis, api, authentication]
---

# API / Documentation Discrepancies

Places where the bexio documentation, the vendored OpenAPI spec and the observed API behaviour do
not line up. Each entry records what was checked, when, and what the library does about it.

> **Reading the docs:** `https://docs.bexio.com/` looks like a JS single-page app and per-anchor
> fetching returns nothing useful, but `curl -sL https://docs.bexio.com/` returns ~8.5 MB of
> server-rendered HTML containing the entire documentation. Strip the tags and read it. The other
> authoritative source is `https://auth.bexio.com/realms/bexio/.well-known/openid-configuration`.

## 1. `client_credentials` grant — advertised by the realm, absent from the docs

**Status:** unproven as of 2026-07-31. Implemented, but no consumer should be designed around it.

The realm discovery document advertises `client_credentials` in `grant_types_supported`. Three
independent signals say that does not mean a portal-registered app can use it:

1. **`client_credentials` appears zero times** in the ~542,000 characters of documentation text.
   The documented mechanisms are the Authorization Code Flow, the Refresh Token Flow, and PAT. The
   realm also advertises `password`, `implicit` and `token-exchange`, which are plainly not enabled
   for partner apps either — this is standard Keycloak realm-level advertisement, not a per-client
   capability.
2. The **app registration form** collects only consent-screen fields (name, website, description,
   logo) plus a required redirect URL. No app type, no service-account option, no scope picker.
   Whether the grant works depends on Keycloak's per-client `serviceAccountsEnabled` flag, which
   bexio controls and does not expose.
3. **The permission model has no place for it.** bexio's authorization is two-level:

   > "While connecting via the API there are 2 levels of authorization. One level is the scopes
   > granted to the application... The other level is based on user rights... **The API access
   > happens with the user rights of the user who set up the connection to the application.**"

   A `client_credentials` token has no user behind it, therefore no user rights, therefore no
   access under this model. This is the stronger argument: even with service accounts enabled, a
   token could mint successfully and still return `403` on every call.

### How to verify

Needs a real app registration from <https://developer.bexio.com>. Minting a token is the easy
half — it must be spent on a real API call:

```bash
curl -s -X POST https://auth.bexio.com/realms/bexio/protocol/openid-connect/token \
  -d grant_type=client_credentials \
  -d client_id=$CLIENT_ID -d client_secret=$CLIENT_SECRET \
  -d scope='accounting file'

curl -s -H "Authorization: Bearer $TOKEN" -H 'Accept: application/json' \
  https://api.bexio.com/2.0/accounts
```

- **Both green** → consumers can skip consent and refresh token storage entirely.
- **`unauthorized_client`, or a 401/403/empty company scope on the second call** →
  authorization code + `offline_access` is the only unattended path, and enabling service accounts
  becomes a question for bexio.

Record the result here once someone runs it against a real registration.

### What the library does

`ClientCredentialsBexioTokenProvider` exists next to `RefreshTokenBexioTokenProvider` and
`StaticBexioTokenProvider` — the pipeline is identical either way, so nothing in the design depends
on the outcome. Its XML docs and the README both flag it as unproven and point at
`AddBexioServicesWithRefreshToken` as the supported unattended path.

## 2. Refresh token rotation — not documented as universal

**Status:** handled defensively.

The sentence requiring applications to "replace refresh tokens with the new refresh tokens provided
during the token refresh" lives in the **"Migration from idp.bexio.com to auth.bexio.com"**
subsection, under *"Do users have to re-authorize my application after the switch?"*. It describes
migrating legacy tokens and may not describe steady-state behaviour. The actual **Refresh Token
Flow** section says only: *"The response contains a new access token."* — no mention of a new
refresh token.

Neither assumption is safe. `RefreshTokenBexioTokenProvider` persists a replacement when the
response carries one and keeps the stored token when it does not. Requiring a rotation would break
if bexio returns none; ignoring one would break if it does.

## 3. Client authentication method — unspecified

**Status:** configurable, defaults to `client_secret_post`.

The realm advertises five methods (`client_secret_basic`, `client_secret_post`, `private_key_jwt`,
`client_secret_jwt`, `tls_client_auth`) and the documentation never states which one bexio
configures for portal-registered apps. Hardcoding the wrong one yields a `401` from the token
endpoint with an unhelpful body. `BexioOAuthOptions.ClientAuthenticationMethod` selects between the
two secret-based methods.

## 4. Access token lifetime — undocumented

**Status:** derived from the response.

No fixed TTL is stated anywhere. `CachingBexioTokenProvider` computes expiry from the token
response's `expires_in` and applies a configurable clock skew margin. Never assume a lifetime.

## 5. Scopes — the documented table is not exhaustive

**Status:** modelled as free-form strings.

The API-scope table lists ~35 scopes; the realm's `scopes_supported` advertises 82. Read access is
implicitly granted by the corresponding write scope (`contact_edit` implies `contact_show`).
`BexioOAuthOptions.Scopes` is `IReadOnlyList<string>` — a hand-maintained enum would be wrong.

Scopes are also fixed at consent time: *"the requested scopes do not change when refreshing a
token. Acquiring new scopes is only possible by going through the initial authorization process
again."* `IBexioTokenClient.RefreshTokenAsync` therefore neither accepts nor sends a scope set.

## 6. Stale authentication doc anchor

**Status:** fixed 2026-07-31.

XML docs and the README linked to
`https://docs.bexio.com/#section/Authentication/JWT-(JSON-Web-Tokens)`. That anchor no longer
exists — legacy API tokens were removed on 2024-10-22 and replaced by Personal Access Tokens. All
references now point at `https://docs.bexio.com/#section/Authentication`.
