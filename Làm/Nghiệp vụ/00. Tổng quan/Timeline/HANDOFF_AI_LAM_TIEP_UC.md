# Handoff AI — Làm tiếp UC dang dở (Pum's ERP)

| Thuộc tính                                     | Giá trị                                                                                                                                                      |
| ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Mục đích                                       | Cho AI / dev khác **hiểu bối cảnh**, **quy tắc DoD**, **lỗ hổng**, và **làm tiếp chỉnh chu** — không đánh dấu giả                                            |
| Cập nhật                                       | 07/08/2026                                                                                                                                                   |
| Tiến độ máy hiện tại                           | **724 / 1092** UC `[x]` DoD khung (~66%) — **không** đồng nghĩa production 100%                                                                              |
| File checklist chuẩn                           | [`CHECKLIST_UC.md`](./CHECKLIST_UC.md)                                                                                                                       |
| Nguồn tiến độ                                  | [`uc_progress.json`](./uc_progress.json)                                                                                                                     |
| Rà soát lịch sử (cảnh báo claim 1092/1092 giả) | [`Rà xoát UC.md`](./Rà%20xoát%20UC.md) — **một phần lỗ hổng trong báo cáo đó đã được vá Cap-2 sau ngày 06/08; luôn đối chiếu code + `uc_progress` mới nhất** |

---

## 0. Đọc trước khi sửa code (bắt buộc)

1. Quy tắc Cursor (always-on):
   - `.cursor/rules/uc-dod-quality.mdc` — DoD chỉnh chu + test 5–20 BE/FE
   - `.cursor/rules/uc-checklist-sync.mdc` — sửa JSON → chạy script sinh checklist
   - `.cursor/rules/module-cut-ready.mdc` — biên module / không EF chéo
2. Biên module: `Làm/Source/MODULES.json` + `MODULES.md`
3. Catalog UC: `Làm/Nghiệp vụ/cay_chuc_nang_data.py` (script checklist đọc từ đây)
4. Code: `Làm/Source/backend` (.NET 8 Clean Architecture), `Làm/Source/frontend` (Next.js App Router)

**Cổng dev hiện tại:** BE `http://localhost:1111` · FE `http://localhost:2222` · seed `admin` / `!Abc123`

---

## 1. Đang làm gì / triết lý làm việc

Dự án ERP đa module (SYS, HRM, CRM, POS, PUR, INV, FIN, BI, PRT, …). Cách làm đúng:

- Cắt lát **Cap-N theo nhóm UC liên quan** (không nhảy lung tung 1092 UC).
- Mỗi slice: **BE thật + FE thật + test thật** → cập nhật `uc_progress.json` trung thực → regen checklist.
- User thường bảo: _「Làm tiếp các UC dang dở」_ → ưu tiên UC đã `[x]` nhưng note còn **stub / mock / Day-1 / % thấp**, hoặc Must còn `[ ]`.

### Đã polish gần đây (Cap-2, pct ↑ ~90–95) — **không làm lại trừ khi còn lỗ hổng thật**

| Nhóm    | UC                                                                                                                                                   | Nội dung chính                                                                                                                                                                                                                                                                                                                                                                                                                           |
| ------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| INV     | 014, 017                                                                                                             | Phân quyền thủ kho & chặn SKU Inactive, Post nhập kho tự động từ GRN mua hàng (idempotent), auto SKU & ĐVT |
| PUR     | 014, 030, 033, 037, 043                                                                                              | Tạo PR chuẩn hóa đơn vị + lines, Quản lý hợp đồng PO phiên bản (v1, v2...), khóa revise khi đã nhận hàng, GRN→INV, HĐ→FIN AP, xuất PO CSV |
| FSM     | 014, 017                                                                                                             | Phân loại Priority Low/Normal/High/Critical + auto SLA DueDates, Chuyển KTV + Escalate status, khóa ticket Closed |
| LOG     | 011, 014                                                                                                             | In vận đơn sinh mã VD-{Code}-{Date}, Cập nhật trạng thái InTransit/Delivered, khóa đổi trạng thái khi Delivered |
| PJM     | 013, 014                                                                                                             | Cập nhật % HT WBS 0-100%, tự động chuyển Status Done/InProgress, Milestone IsOverdue tự động khi quá DueDate |
| MFG/FIN | MFG_031, MFG_042, MFG_022, FIN_015, FIN_019/025/030/039                                                            | JE WIP→TP, FgReceipt kiểm tra overproduction 120% + Auto Status Completed, sản lượng ngày/ca (Ca 1/2/3 tự động), Auto JE, thu/NH/AR/AP tạo JE |
| HRM     | 118                                                                                                                                                  | Sync máy chấm công chi tiết                                                                                                                                                                                                                                                                                                                                                                                                              |
| BI      | 002, 008, 014, 016, 018, 019, 021                                                                                                                    | Refresh nguồn module, widget DT/LN live FIN, KPI actual metrics live FIN/POS/CRM, BC + CSV/text                                                                                                                                                                                                                                                                                                                                          |
| PRT     | 002, 014                                                                                                                                             | Portal Auth (Login/Forgot/Reset password token), AR summary tổng hợp FIN AR                                                                                                                                                                                                                                                                                                                                                              |
| LMS     | 031                                                                                                                                                  | Ghi danh khóa học & thanh toán qua Cổng thanh toán (tự động tạo IntegrationCallLog PAYMENT_GATEWAY)                                                                                                                                                                                                                                                                                                                                      |
| SYS     | 004, 008, 010, 011, 012, 019, 029, 060, 061, 075                                                                                                     | 2FA TOTP, Session limit/revocation, Trusted devices, DataScope SalesPoint, Email/SMS log, PDF (%PDF-1.4 header + ContentType application/pdf) & CSV report export                                                                                                                                                                                                                                                                        |

Test slice mới: InMemory service tests (`*PolishTests.cs`) + FE `*.node-test.mts` (helpers). Suite Batch\* cũ vẫn còn nhiều assert giả — **không đếm vào DoD**.

---

## 2. Quy tắc hoàn thiện UC (DoD) — áp dụng mọi lần

### Được gọi là “xong chỉnh chu” khi

| Hạng mục      | Yêu cầu                                                                                                                           |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| SRS / catalog | Bám đúng UC trong cây chức năng; không bịa flow                                                                                   |
| BE            | Controller → Application → Infrastructure; DI `Add{Module}Module`; DbSet + migration nếu cần schema; permission seed `ModuleCode` |
| FE            | Gọi API thật; UI dùng `field` / `btn` / `panel` / `tableWrap` / shell hiện có — **không** trang one-off lệch design               |
| Wiring        | Có DI, FE gọi đúng endpoint — thiếu = **chưa xong**                                                                               |
| Stub          | Chỉ tạm; ghi `[~]` hoặc `pct` thấp + note rõ. **Cấm** `[x]` pct 100 khi còn stub/mock/hard-code                                   |
| Test BE       | **5–20** case gọi service / API / DB InMemory (hoặc WebApplicationFactory)                                                        |
| Test FE       | **5–20** case helpers/`node:test` (hoặc component/E2E) cho luồng đó                                                               |
| Progress      | Sửa trực tiếp file Markdown (`CHECKLIST_UC.md`, `uc_progress.json`, `HANDOFF_AI_LAM_TIEP_UC.md`) — không dùng script Python.      |

### Cấm

- Đánh dấu hàng loạt 100% catalog / note copy-paste giống nhau.
- Ship “Day-1 khung”, `alert('(stub)')`, mock data FE rồi ghi xong.
- Import EF entity module A vào service module B (trừ SYS; cross-module qua interface/outbox).
- Chạy script Python để sinh lại `CHECKLIST_UC.md` — **tất cả file MD bắt buộc sửa trực tiếp**.
- Tính `Batch*UcTests` assert chuỗi/`Assert.True` local là “đã test UC”.

### Ý nghĩa cột tiến độ

| Ký hiệu                           | Nghĩa thật                                                           |
| --------------------------------- | -------------------------------------------------------------------- |
| `[x]` + pct 70–90                 | DoD **khung** Cap-1/2 đủ dùng — thường **chưa** production chỉnh chu |
| `[x]` + pct 90–95                 | Cap-2 đã wire thật + test slice — còn nợ nhỏ ghi trong note          |
| `[x]` + pct 100                   | Chỉ khi thật sự sâu (hiếm); nhiều Batch4 “Verified …” cần soi lại    |
| `[ ]`                             | Chưa làm / chưa ghi progress                                         |
| Note có `stub` / `mock` / `Day-1` | **Ưu tiên polish** dù đã `[x]`                                       |

---

## 3. Lỗ hổng & ưu tiên làm tiếp

### 3.1 Hàng đợi ưu tiên (stub / mỏng rõ — làm trước)

| Ưu tiên | UC / khu vực                  | Vì sao                                         | Hướng chỉnh chu gợi ý                   |
| ------: | ----------------------------- | ---------------------------------------------- | --------------------------------------- |
|       1 | Must còn `[ ]` trên checklist | Đặc biệt module % thấp: BI, FSM, LOG, PJM, PRT | Làm Must trước Should/Could             |
|       2 | Module user đang dùng         | CRM/POS/FIN/HRM                                | Polish sâu các luồng nợ Cap-2 pct < 85% |

### 3.2 Nợ chất lượng rộng (đã `[x]` nhưng pct ~70–85)

Hàng trăm UC ghi `L2` / `N1` / `M1` / `Day-1` — **đủ khung**, chưa “chỉnh chu 100%”. Không cần làm hết một lúc; chọn theo:

1. Must còn stub hoặc cross-module.
2. Module user đang dùng (CRM/POS/FIN/HRM).
3. FE mỏng (~3 trang): **BI, PRT, MFG, PJM, FSM, LOG**.

Thống kê `uc_progress` (done): ~136 ở 100% · ~217 ở 90–99 · ~367 ở 70–89 · vài UC &lt;70%.

### 3.3 Cập nhật so với `Rà xoát UC.md` (06/08)

Báo cáo rà soát **vẫn đúng tinh thần** (đừng tin claim 1092/1092), nhưng **một số lỗ hổng đã vá** — đừng làm lại mù quáng:

| Lỗ hổng cũ trong Rà xoát        | Trạng thái sau Cap-2 (07/08)                                                            |
| ------------------------------- | --------------------------------------------------------------------------------------- |
| CRM Campaign/Promotion API chết | Đã wire (marketing Cap-2) — kiểm DI/DbSet trước khi sửa                                 |
| BI ETL / export stub            | 002/008/014/016 đã live một phần; **KPI actual** còn stub                               |
| PUR→INV/AP, CRM→INV/LOG stub    | Đã polish                                                                               |
| AST/MFG→FIN stub                | Đã JE thật                                                                              |
| FIN auto / thu NH AR AP stub    | Đã CreateAutoJournal / resolve COA                                                      |
| SYS OTP chỉ log Warning         | Đã IntegrationCallLog + FE forgot/invite                                                |
| Không có test FE                | Đã có `npm test` (node:test helpers) — **chưa** Playwright E2E                          |
| Test BE 464 toàn giả            | Tổng pass cao hơn; **Batch\*** vẫn giả — chỉ tin `*PolishTests` / module tests InMemory |

---

## 4. Bản đồ kỹ thuật nhanh

```
Làm/Source/backend/
  src/Erp.Api/Controllers/{Module}/
  src/Erp.Application/DTOs|Interfaces/
  src/Erp.Domain/Entities/{Module}/
  src/Erp.Infrastructure/Implementations/Services/{Module}/
  src/Erp.Infrastructure/Persistence/ (AppDbContext, Migrations, Configurations/{Module})
  tests/Erp.UnitTests/

Làm/Source/frontend/
  src/app/app/{module}/…/page.tsx
  src/shared/api/*-api.ts
  src/shared/api/*-helpers.ts + *.node-test.mts
  src/shared/ui/ (field, btn, panel, …)
```

Pattern polish đã dùng:

1. Thay stub bằng gọi DB/module đích (hoặc channel log có kiểm chứng).
2. Endpoint download/export trả file thật (CSV/text) khi chưa cần binary nặng.
3. FE helpers thuần + `node:test`; đăng ký thêm file trong `frontend/package.json` → `scripts.test`.
4. BE: class `*PolishTests` / `*Tests` với `UseInMemoryDatabase`.

---

## 5. Cách AI nên làm một slice (checklist thao tác)

```text
[ ] 1. Đọc UC trong catalog + note hiện tại trong uc_progress.json
[ ] 2. Grep code: stub / TODO / endpoint chết / FE hard-code
[ ] 3. Implement BE (DI, DbSet, migration nếu cần schema)
[ ] 4. Implement FE (API client + UI đồng nhất; bỏ chữ stub trên UI)
[ ] 5. Viết ≥5 test BE thật + ≥5 test FE helpers (hoặc tương đương)
[ ] 6. Chạy test slice đến xanh
[ ] 7. Cập nhật uc_progress.json (done/pct/note trung thực — không bịa 100%)
[ ] 8. python "Làm/Nghiệp vụ/00. Tổng quan/Timeline/build_uc_checklist.py"
[ ] 9. Xác nhận CHECKLIST_UC.md khớp; cập nhật MODULE_RISK trong build script nếu rủi ro module đổi
```

**Pct gợi ý khi polish stub→wired:** thường **90–95**, note nêu API + FE + tên file test. Chỉ 100 khi thật sự đủ sâu và không còn nợ trong note.

---

## 6. Lệnh thường dùng

```bash
# Regen checklist (bắt buộc sau sửa progress)
python "Làm/Nghiệp vụ/00. Tổng quan/Timeline/build_uc_checklist.py"

# BE — một suite polish
dotnet test "Làm/Source/backend/tests/Erp.UnitTests/Erp.UnitTests.csproj" --filter "FullyQualifiedName~BiAnalyticsPolishTests"

# FE helpers
cd "Làm/Source/frontend" && npm test

# Dev
cd "Làm/Source/backend" && dotnet run --project src/Erp.Api --urls http://localhost:1111
cd "Làm/Source/frontend" && npm run dev
# → FE :2222 · API :1111 (xem .env.local NEXT_PUBLIC_API_URL)
```

---

## 7. Tiêu chí “hiểu đúng yêu cầu user”

Khi user nói làm tiếp UC dang dở / chỉnh chu:

| Đúng                                                  | Sai                                                            |
| ----------------------------------------------------- | -------------------------------------------------------------- |
| Tìm stub / pct thấp / Must `[ ]`, làm thật BE+FE+test | Đánh 100 hàng loạt trên JSON                                   |
| Giữ biên module + UI shell                            | Tạo trang design lệch / mock local                             |
| Note progress mô tả việc đã làm                       | Note chung chung “hoàn thành 100%”                             |
| Regen checklist bằng script                           | Sửa tay CHECKLIST_UC.md                                        |
| Hạ hoặc giữ pct nếu còn stub có chủ đích              | Giấu stub bằng cách xóa chữ “stub” trên UI nhưng logic vẫn giả |

---

## 8. Gợi ý slice tiếp theo (copy cho AI)

**Slice đề xuất:** `PRT_002` + `PRT_014` (login/forgot/reset + AR summary từ FIN), hoặc **BI KPI actual** (`actualStubValue` → metrics live), mỗi slice đủ 5–20 test BE/FE rồi mới nâng pct.

Sau mỗi slice: cập nhật file handoff này (mục §1 “đã polish”, §3 hàng đợi) nếu thay đổi lớn — hoặc chỉ cập nhật `uc_progress` + checklist nếu slice nhỏ.

---

_File này là bản đồ làm việc, không thay thế SRS từng UC. Nguồn sự thật tiến độ luôn là `uc_progress.json` sau khi regen checklist._
