---
title: API / Documentation Discrepancies
tags: [analysis, api, authentication]
---

# API / Documentation Discrepancies

Places where the bexio documentation, the vendored OpenAPI spec and the observed API behaviour do
not line up. Each entry records what was checked, when, and what the library does about it.

## 1. `client_credentials` grant — advertised, unverified

**Status:** unverified as of 2026-07-31. Does not block the OIDC implementation.

The bexio realm discovery document
(<https://auth.bexio.com/realms/bexio/.well-known/openid-configuration>) advertises:

```
grant_types_supported: authorization_code, implicit, refresh_token, password,
                       client_credentials, ciba, token-exchange
```

This is a Keycloak realm, so the list describes what the *realm* supports, not what any individual
client registration is permitted to use — `password`, `implicit` and `token-exchange` are plainly
not enabled for partner apps either. Whether `client_credentials` works depends on Keycloak's
per-client `serviceAccountsEnabled` flag, which bexio controls at app-registration time and does
not document.

A second, independent risk: a service-account token has no binding to a bexio **company**. That
binding is what the authorization-code consent establishes. A token can therefore mint successfully
and still return `403` on every API call.

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

### What the library does

Nothing in the design depends on the outcome. `client_credentials` is one
`IBexioTokenProvider` implementation (`ClientCredentialsBexioTokenProvider`) next to
`RefreshTokenBexioTokenProvider` and `StaticBexioTokenProvider`; the pipeline is identical either
way. Record the result here once someone runs the check against a real registration.

## 2. Stale authentication doc anchor

**Status:** fixed 2026-07-31.

XML docs and the README linked to
`https://docs.bexio.com/#section/Authentication/JWT-(JSON-Web-Tokens)`. That anchor no longer
exists — legacy API tokens were removed on 2024-10-22 and replaced by Personal Access Tokens. All
references now point at `https://docs.bexio.com/#section/Authentication`.
