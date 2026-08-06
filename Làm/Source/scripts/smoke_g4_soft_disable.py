# -*- coding: utf-8 -*-
"""G4.4 — tắt soft module CRM, hard path SYS vẫn sống, CRM API = 403."""
from __future__ import annotations

import json
import sys
import urllib.error
import urllib.request

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

BASE = "http://localhost:5000"


def req(method: str, path: str, token: str | None = None, body: dict | None = None):
    data = None
    headers = {"Accept": "application/json"}
    if body is not None:
        data = json.dumps(body).encode()
        headers["Content-Type"] = "application/json"
    if token:
        headers["Authorization"] = f"Bearer {token}"
    request = urllib.request.Request(BASE + path, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(request, timeout=30) as resp:
            raw = resp.read().decode()
            return resp.status, json.loads(raw) if raw else None
    except urllib.error.HTTPError as e:
        raw = e.read().decode(errors="replace")
        try:
            payload = json.loads(raw)
        except Exception:
            payload = raw
        return e.code, payload


def login(user: str) -> str:
    st, p = req("POST", "/api/auth/login", body={"username": user, "password": "!Abc123"})
    if st != 200:
        raise RuntimeError(f"login failed {st} {p}")
    return p["data"]["accessToken"]


def main() -> int:
    results: list[str] = []

    def ok(n: str, d: str = "") -> None:
        m = f"PASS  {n}" + (f" | {d}" if d else "")
        results.append(m)
        print(m)

    def fail(n: str, d: str = "") -> None:
        m = f"FAIL  {n}" + (f" | {d}" if d else "")
        results.append(m)
        print(m)

    token = login("admin")
    ok("Login admin")

    st, _ = req("PUT", "/api/sys/license-modules/CRM", token=token, body={"isEnabled": False})
    if st == 200:
        ok("Disable CRM")
    else:
        fail("Disable CRM", f"{st}")
        return 1

    try:
        st, me = req("GET", "/api/auth/me", token=token)
        if st == 200 and me["data"]["username"] == "admin":
            ok("Hard path /auth/me")
        else:
            fail("Hard path /auth/me", f"{st}")

        st, menu = req("GET", "/api/sys/menu", token=token)
        if st == 200:
            ok("Hard path /sys/menu", f"count={len(menu['data'])}")
        else:
            fail("Hard path /sys/menu", str(st))

        st, users = req("GET", "/api/sys/users", token=token)
        if st == 200:
            ok("Hard path /sys/users")
        else:
            fail("Hard path /sys/users", str(st))

        # CRM chưa có controller → middleware vẫn 403 trước khi 404
        st, body = req("GET", "/api/crm/ping", token=token)
        if st == 403:
            ok("Soft CRM API 403", str(body.get("message") if isinstance(body, dict) else body))
        else:
            fail("Soft CRM API 403", f"got {st} {body}")

        st, me2 = req("GET", "/api/auth/me", token=token)
        mods = [m.upper() for m in me2["data"].get("enabledModules", [])]
        if "CRM" not in mods:
            ok("enabledModules omits CRM", ",".join(mods))
        else:
            fail("enabledModules still has CRM", ",".join(mods))
    finally:
        st, _ = req("PUT", "/api/sys/license-modules/CRM", token=token, body={"isEnabled": True})
        if st == 200:
            ok("Re-enable CRM")
        else:
            fail("Re-enable CRM", str(st))

    print()
    print("==== SUMMARY ====")
    for x in results:
        print(x)
    fails = sum(1 for x in results if x.startswith("FAIL"))
    print(f"PASS={sum(1 for x in results if x.startswith('PASS'))} FAIL={fails}")
    return 1 if fails else 0


if __name__ == "__main__":
    raise SystemExit(main())
