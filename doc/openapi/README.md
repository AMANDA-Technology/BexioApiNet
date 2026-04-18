---
title: Vendored Bexio OpenAPI Spec
tags: [openapi, bexio, spec, ai]
---

# Vendored Bexio OpenAPI Spec

This directory contains a local copy of the Bexio REST API OpenAPI specification for use by AI agents and offline tooling.

## Spec Details

| Field | Value |
|-------|-------|
| **File** | `bexio-v3.json` |
| **API Version** | 3.0.0 |
| **OpenAPI Version** | 3.0.2 |
| **Paths** | 355 |
| **Retrieval Source** | <https://docs.bexio.com/> (spec embedded in Redoc HTML at `__redoc_state.spec.data`) |
| **Retrieval Date** | 2026-04-18 |
| **Human-readable mirror** | <https://docs.bexio.com/> |

## Usage by AI Agents

AI agents working in this repository should consult `doc/openapi/bexio-v3.json` as the **primary reference** for endpoint definitions, request/response shapes, field names, types, and status codes. This enables deterministic, offline model generation without depending on external uptime.

The human-readable documentation at <https://docs.bexio.com/> remains the canonical mirror for browsing.

## Refresh Procedure

### When to refresh

- A Bexio API changelog entry on <https://docs.bexio.com/> describes new endpoints, changed fields, or deprecated operations that affect this library.
- A developer notices a mismatch between the vendored spec and the live API behavior.
- Routine maintenance: at most once per quarter unless a change warrants an earlier update.

### How to refresh

1. Download the latest spec:
   ```bash
   curl -s "https://docs.bexio.com/" | python3 -c "
   import sys, json
   html = sys.stdin.read()
   idx = html.find('__redoc_state = {')
   start = idx + len('__redoc_state = ')
   depth, in_string, escape_next = 0, False, False
   for i, c in enumerate(html[start:], start=start):
       if escape_next: escape_next = False; continue
       if c == '\\\\' and in_string: escape_next = True; continue
       if c == '\"' and not escape_next: in_string = not in_string; continue
       if in_string: continue
       if c == '{': depth += 1
       elif c == '}':
           depth -= 1
           if depth == 0: end = i + 1; break
   spec = json.loads(html[start:end])['spec']['data']
   print(json.dumps(spec, indent=2, ensure_ascii=False))
   " > doc/openapi/bexio-v3.json
   ```

2. Verify the new spec looks correct:
   ```bash
   python3 -c "import json; s=json.load(open('doc/openapi/bexio-v3.json')); print('version:', s['info']['version'], '| paths:', len(s['paths']))"
   ```

3. Diff the new spec against the previous version to understand what changed:
   ```bash
   git diff doc/openapi/bexio-v3.json
   ```

4. Re-verify `ai_instructions.md` and `doc/development/feature-addition-guide.md`:
   - If new endpoints were added, update the "not yet implemented" lists in these files.
   - If existing endpoints changed (new required fields, changed types, deprecated parameters), note them as "watch points" in the relevant ADR or open a new issue.

5. Update the **Retrieval Date** in this README to today's date.

6. Commit with message: `chore(openapi): refresh vendored Bexio v3 spec (YYYY-MM-DD)`
