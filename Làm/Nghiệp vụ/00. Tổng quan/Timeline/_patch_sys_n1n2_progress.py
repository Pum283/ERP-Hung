# -*- coding: utf-8 -*-
import json
from pathlib import Path

p = Path(__file__).with_name("uc_progress.json")
data = json.loads(p.read_text(encoding="utf-8"))

n1 = {
    "UC_SYS_002": (100, "N1 · logout API + FE"),
    "UC_SYS_003": (100, "N1 · change-password"),
    "UC_SYS_006": (100, "N1 · password.policy setting"),
    "UC_SYS_007": (100, "N1 · FailedLoginCount lock"),
    "UC_SYS_083": (90, "N1 · SessionMinutes / JWT exp"),
    "UC_SYS_016": (100, "N1 · soft delete user"),
    "UC_SYS_017": (90, "N1 · validate primaryOrgUnitId"),
    "UC_SYS_018": (100, "N1 · admin reset-password"),
    "UC_SYS_034": (100, "N1 · GET/PUT tenant"),
    "UC_SYS_041": (100, "N1 · TZ/locale/currency"),
    "UC_SYS_044": (100, "N1 · GET modules catalog"),
    "UC_SYS_046": (100, "N1 · license CRUD"),
    "UC_SYS_047": (100, "N1 · MaxUsers/MaxOrgUnits enforce"),
    "UC_SYS_048": (100, "N1 · license/status"),
    "UC_SYS_051": (100, "N1 · system_setting"),
    "UC_SYS_053": (100, "N1 · lookup category/item"),
    "UC_SYS_054": (100, "N1 · number_sequence"),
    "UC_SYS_055": (100, "N1 · NextNumberAsync"),
    "UC_SYS_079": (100, "N1 · login_audit"),
    "UC_SYS_059": (90, "N1 · in-app notifications"),
    "UC_SYS_063": (90, "N1 · notification_rule"),
    "UC_SYS_072": (100, "N1 · import users CSV"),
    "UC_SYS_073": (100, "N1 · import template"),
    "UC_SYS_074": (90, "N1 · export users CSV"),
    "UC_SYS_096": (100, "N1 · FE group chat"),
    "UC_SYS_101": (80, "N1 · attachmentFileId on send"),
    "UC_SYS_102": (100, "N1 · recall message"),
}
n2 = {
    "UC_SYS_004": (90, "N2 · forgot OTP stub log"),
    "UC_SYS_005": (90, "N2 · reset with OTP"),
    "UC_SYS_008": (80, "N2 · 2FA begin/confirm Dev"),
    "UC_SYS_010": (90, "N2 · user_session list"),
    "UC_SYS_011": (90, "N2 · max 5 sessions"),
    "UC_SYS_019": (70, "N2 · invite via forgot stub"),
    "UC_SYS_020": (100, "N2 · import users = 072"),
    "UC_SYS_022": (100, "N2 · export users = 074"),
    "UC_SYS_024": (100, "N2 · copy role"),
    "UC_SYS_029": (60, "N2 · sales_point scope stub entity"),
    "UC_SYS_032": (100, "N2 · permission-matrix"),
    "UC_SYS_033": (100, "N2 · permission_change_log"),
    "UC_SYS_037": (100, "N2 · sales_point CRUD"),
    "UC_SYS_040": (90, "N2 · org-chart API"),
    "UC_SYS_043": (100, "N2 · province CRUD"),
    "UC_SYS_035": (100, "N2 · legal_entity"),
    "UC_SYS_052": (70, "N2 · settings key scoped"),
    "UC_SYS_056": (100, "N2 · message_template"),
    "UC_SYS_057": (100, "N2 · work_calendar"),
    "UC_SYS_042": (70, "N2 · locale/date via tenant settings"),
    "UC_SYS_060": (60, "N2 · email stub via message_template"),
    "UC_SYS_061": (60, "N2 · SMS stub channel"),
    "UC_SYS_065": (80, "N2 · integration_call_log"),
    "UC_SYS_088": (70, "N2 · external_integration Email"),
    "UC_SYS_089": (70, "N2 · external_integration SMS"),
    "UC_SYS_068": (100, "N2 · file_folder"),
    "UC_SYS_069": (70, "N2 · file linkedEntity ACL light"),
    "UC_SYS_070": (100, "N2 · soft delete/restore file"),
    "UC_SYS_075": (50, "N2 · CSV export as PDF substitute Day-1"),
    "UC_SYS_076": (60, "N2 · import result counts as job"),
    "UC_SYS_080": (70, "N2 · PermissionChangeLog detail"),
    "UC_SYS_081": (70, "N2 · login-audits API"),
    "UC_SYS_084": (100, "N2 · api_key"),
    "UC_SYS_085": (100, "N2 · webhook"),
    "UC_SYS_086": (100, "N2 · integration logs"),
    "UC_SYS_090": (100, "N2 · external_integration"),
    "UC_SYS_091": (100, "N2 · locale_pack"),
    "UC_SYS_092": (100, "N2 · PUT me/locale"),
}

for uc, (pct, note) in {**n1, **n2}.items():
    data[uc] = {"done": True, "pct": pct, "note": note}

ordered = dict(sorted(data.items()))
p.write_text(json.dumps(ordered, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(f"patched {len(n1) + len(n2)} UCs; total={len(ordered)}")
