# -*- coding: utf-8 -*-
"""Smoke E2E M1 Day-1: SYS + leave submit → WF approve/reject + FE routes."""
from __future__ import annotations

import json
import sys
import time
import urllib.error
import urllib.request
from datetime import date, timedelta

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

BASE = "http://localhost:5000"
FE = "http://localhost:3000"
results: list[str] = []


def ok(name: str, detail: str = "") -> None:
    msg = f"PASS  {name}" + (f" | {detail}" if detail else "")
    results.append(msg)
    print(msg)


def fail(name: str, detail: str = "") -> None:
    msg = f"FAIL  {name}" + (f" | {detail}" if detail else "")
    results.append(msg)
    print(msg)


def req(method: str, path: str, token: str | None = None, body: dict | None = None, base: str = BASE):
    data = None
    headers = {"Accept": "application/json"}
    if body is not None:
        data = json.dumps(body).encode()
        headers["Content-Type"] = "application/json"
    if token:
        headers["Authorization"] = f"Bearer {token}"
    request = urllib.request.Request(base + path, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(request, timeout=30) as resp:
            raw = resp.read().decode()
            corr = resp.headers.get("X-Correlation-Id")
            payload = json.loads(raw) if raw else None
            return resp.status, payload, corr
    except urllib.error.HTTPError as e:
        raw = e.read().decode(errors="replace")
        try:
            payload = json.loads(raw)
        except Exception:
            payload = raw
        raise RuntimeError(f"HTTP {e.code} {path}: {payload}") from e


def login(user: str) -> str:
    _, p, _ = req("POST", "/api/auth/login", body={"username": user, "password": "!Abc123"})
    return p["data"]["accessToken"]


def fe_get(path: str) -> None:
    try:
        with urllib.request.urlopen(FE + path, timeout=30) as r:
            ok(f"FE {path}", str(r.status))
    except urllib.error.HTTPError as e:
        if e.code < 500:
            ok(f"FE {path}", str(e.code))
        else:
            fail(f"FE {path}", str(e.code))
    except Exception as e:
        fail(f"FE {path}", str(e))


def main() -> int:
    fe_get("/login")

    al_id = None
    req_id = None
    used0 = rem0 = None

    try:
        at = login("admin")
        ok("Login admin")
        _, me, corr = req("GET", "/api/auth/me", token=at)
        if me["data"]["username"] == "admin":
            ok("GET /auth/me")
        else:
            fail("GET /auth/me", json.dumps(me))
        if corr:
            ok("X-Correlation-Id", corr)
        else:
            fail("X-Correlation-Id", "missing")
        _, menu, _ = req("GET", "/api/sys/menu", token=at)
        ok("Menu", f"count={len(menu['data'])}")
        _, users, _ = req("GET", "/api/sys/users", token=at)
        ok("Users", f"count={len(users['data'])}")
        _, emps, _ = req("GET", "/api/hrm/employees", token=at)
        ok("Employees", f"count={len(emps['data'])}")
        _, orgs, _ = req("GET", "/api/sys/org-units", token=at)
        ok("Org units", f"count={len(orgs['data'])}")
        _, roles, _ = req("GET", "/api/sys/roles", token=at)
        ok("Roles", f"count={len(roles['data'])}")
    except Exception as e:
        fail("Admin SYS", str(e))

    try:
        stok = login("hr.spec1")
        ok("Login hr.spec1")
        _, types, _ = req("GET", "/api/hrm/leave-types", token=stok)
        al = next(t for t in types["data"] if t["code"] == "AL")
        al_id = al["id"]
        _, bals, _ = req("GET", "/api/hrm/leave-balances", token=stok)
        bal = next(b for b in bals["data"] if b["leaveTypeId"] == al_id)
        used0 = float(bal["used"])
        rem0 = float(bal["remaining"])
        ok("Balance before", f"used={used0} remaining={rem0}")
        day = (date.today() + timedelta(days=14)).isoformat()
        _, created, _ = req(
            "POST",
            "/api/hrm/leave-requests",
            token=stok,
            body={
                "employeeId": None,
                "leaveTypeId": al_id,
                "fromDate": day,
                "toDate": day,
                "days": 1,
                "reason": "Smoke E2E approve",
                "submit": True,
            },
        )
        req_id = created["data"]["id"]
        ok(
            "Create+submit leave",
            f"id={req_id} status={created['data']['status']} wf={created['data'].get('wfInstanceId')}",
        )
    except Exception as e:
        fail("hr.spec1 leave", str(e))

    try:
        mtok = login("hr.manager")
        ok("Login hr.manager")
        _, tasks, _ = req("GET", "/api/wf/tasks/my", token=mtok)
        task = next((t for t in tasks["data"] if t.get("sourceDocId") == req_id), None)
        if not task and tasks["data"]:
            task = tasks["data"][0]
        if not task:
            raise RuntimeError(f"no tasks count={len(tasks['data'])}")
        ok("Inbox task", f"id={task['id']} doc={task.get('sourceDocId')}")
        _, act, _ = req(
            "POST",
            f"/api/wf/tasks/{task['id']}/act",
            token=mtok,
            body={"action": "Approve", "comment": "smoke"},
        )
        ok("WF Approve", json.dumps(act.get("data")))
        time.sleep(1)
        stok = login("hr.spec1")
        _, reqs, _ = req("GET", "/api/hrm/leave-requests", token=stok)
        lr = next(x for x in reqs["data"] if x["id"] == req_id)
        if lr["status"] == "Approved":
            ok("Leave Approved")
        else:
            fail("Leave Approved", lr["status"])
        _, bals, _ = req("GET", "/api/hrm/leave-balances", token=stok)
        bal = next(b for b in bals["data"] if b["leaveTypeId"] == al_id)
        used1 = float(bal["used"])
        rem1 = float(bal["remaining"])
        if used0 is not None and used1 == used0 + 1 and rem1 == rem0 - 1:
            ok("Balance deducted", f"{used0}->{used1}, {rem0}->{rem1}")
        else:
            fail("Balance deducted", f"{used0}->{used1}, {rem0}->{rem1}")

        # Outbox published (poll up to ~20s)
        at = login("admin")
        published = False
        for _ in range(8):
            _, box, _ = req("GET", "/api/sys/outbox/recent?take=30", token=at)
            rows = box.get("data") or []
            hit = next(
                (
                    r
                    for r in rows
                    if r.get("eventType") == "hrm.leave.approved" and r.get("status") == "Published"
                ),
                None,
            )
            if hit:
                ok("Outbox published hrm.leave.approved", f"id={hit.get('id')}")
                published = True
                break
            time.sleep(2.5)
        if not published:
            fail("Outbox published hrm.leave.approved", "not found in 20s")
    except Exception as e:
        fail("Approve E2E", str(e))

    try:
        stok = login("hr.spec1")
        _, bals, _ = req("GET", "/api/hrm/leave-balances", token=stok)
        bal = next(b for b in bals["data"] if b["leaveTypeId"] == al_id)
        used_b = float(bal["used"])
        day = (date.today() + timedelta(days=21)).isoformat()
        _, created, _ = req(
            "POST",
            "/api/hrm/leave-requests",
            token=stok,
            body={
                "leaveTypeId": al_id,
                "fromDate": day,
                "toDate": day,
                "days": 1,
                "reason": "Smoke reject",
                "submit": True,
            },
        )
        rid = created["data"]["id"]
        mtok = login("hr.manager")
        _, tasks, _ = req("GET", "/api/wf/tasks/my", token=mtok)
        task = next(t for t in tasks["data"] if t.get("sourceDocId") == rid)
        req(
            "POST",
            f"/api/wf/tasks/{task['id']}/act",
            token=mtok,
            body={"action": "Reject", "comment": "smoke"},
        )
        _, reqs, _ = req("GET", "/api/hrm/leave-requests", token=stok)
        lr = next(x for x in reqs["data"] if x["id"] == rid)
        _, bals, _ = req("GET", "/api/hrm/leave-balances", token=stok)
        bal = next(b for b in bals["data"] if b["leaveTypeId"] == al_id)
        used_a = float(bal["used"])
        if lr["status"] == "Rejected" and used_a == used_b:
            ok("Reject path", f"status=Rejected used={used_a}")
        else:
            fail("Reject path", f"status={lr['status']} used {used_b}->{used_a}")
    except Exception as e:
        fail("Reject E2E", str(e))

    # Idempotency-Key replay
    try:
        stok = login("hr.spec1")
        _, types, _ = req("GET", "/api/hrm/leave-types", token=stok)
        al = next(t for t in types["data"] if t["code"] == "AL")
        day = (date.today() + timedelta(days=28)).isoformat()
        key = f"smoke-idem-{day}"
        body = {
            "leaveTypeId": al["id"],
            "fromDate": day,
            "toDate": day,
            "days": 1,
            "reason": "idempotency",
            "submit": True,
        }
        # first call with header via raw request helper
        def post_idem(tok: str, idem: str):
            data = json.dumps(body).encode()
            headers = {
                "Accept": "application/json",
                "Content-Type": "application/json",
                "Authorization": f"Bearer {tok}",
                "Idempotency-Key": idem,
            }
            request = urllib.request.Request(
                BASE + "/api/hrm/leave-requests", data=data, headers=headers, method="POST"
            )
            with urllib.request.urlopen(request, timeout=30) as resp:
                raw = resp.read().decode()
                replayed = resp.headers.get("X-Idempotency-Replayed")
                return json.loads(raw), replayed

        p1, r1 = post_idem(stok, key)
        p2, r2 = post_idem(stok, key)
        if p1["data"]["id"] == p2["data"]["id"] and r2 == "true":
            ok("Idempotency replay", f"id={p1['data']['id']}")
        elif p1["data"]["id"] == p2["data"]["id"]:
            ok("Idempotency same id", f"id={p1['data']['id']} replayed={r2}")
        else:
            fail("Idempotency", f"{p1['data']['id']} vs {p2['data']['id']} replayed={r2}")
    except Exception as e:
        fail("Idempotency", str(e))

    # SYS-MSG realtime (Must)
    try:
        atok = login("admin")
        stok = login("hr.spec1")
        _, dir_a, _ = req("GET", "/api/sys/msg/directory", token=atok)
        peer = next(u for u in dir_a["data"] if u["username"] == "hr.spec1")
        _, conv, _ = req(
            "POST",
            "/api/sys/msg/conversations",
            token=atok,
            body={"peerUserId": peer["id"]},
        )
        cid = conv["data"]["id"]
        _, sent, _ = req(
            "POST",
            f"/api/sys/msg/conversations/{cid}/messages",
            token=atok,
            body={"body": "Smoke MSG hello"},
        )
        _, msgs, _ = req("GET", f"/api/sys/msg/conversations/{cid}/messages", token=stok)
        bodies = [m["body"] for m in msgs["data"]]
        _, unread, _ = req("GET", "/api/sys/msg/unread-count", token=stok)
        req("POST", f"/api/sys/msg/conversations/{cid}/read", token=stok)
        _, unread2, _ = req("GET", "/api/sys/msg/unread-count", token=stok)
        if "Smoke MSG hello" in bodies and unread["data"]["count"] >= 1 and unread2["data"]["count"] == 0:
            ok("MSG Direct+read", f"conv={cid} msg={sent['data']['id']}")
        else:
            fail(
                "MSG Direct+read",
                f"bodies={bodies} unread={unread['data']['count']}->{unread2['data']['count']}",
            )
    except Exception as e:
        fail("MSG E2E", str(e))

    for path in [
        "/app",
        "/app/hrm/employees",
        "/app/hrm/leaves",
        "/app/hrm/contracts",
        "/app/wf/tasks",
        "/app/sys/users",
        "/app/sys/org",
        "/app/sys/roles",
        "/app/sys/messages",
    ]:
        fe_get(path)

    print()
    print("==== SUMMARY ====")
    for x in results:
        print(x)
    fails = sum(1 for x in results if x.startswith("FAIL"))
    passes = sum(1 for x in results if x.startswith("PASS"))
    print(f"PASS={passes} FAIL={fails}")
    return 1 if fails else 0


if __name__ == "__main__":
    raise SystemExit(main())
