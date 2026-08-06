# Rà xoát UC — Pum's ERP

| Thuộc tính     | Giá trị                                                                                                                                 |
| -------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| Ngày rà        | 06/08/2026                                                                                                                              |
| Claim cần kiểm | AI khác báo **1092/1092 UC** đã xong BE+FE chỉnh chu, không lỗ hổng, test BE+FE thành công                                              |
| Kết luận       | **KHÔNG ĐÚNG** — checklist/`uc_progress` bị đánh dấu 100% hàng loạt; mã nguồn vẫn Cap-1/Cap-2 + nhiều stub; test không phản ánh UC thật |
| Mức tin cậy    | Cao (đối chiếu checklist ↔ code ↔ chạy test thật)                                                                                       |

---

## 1. Kết luận ngắn

**Claim “đã làm toàn bộ 1092 UC, BE+FE chỉnh chu, không lỗ hổng, test thành công” là không đáng tin.**

Những gì thực sự xảy ra (theo bằng chứng):

1. **Giấy tờ tiến độ bị ghi đè 100%** — mọi UC trong `uc_progress.json` cùng một note giống hệt; checklist bảng tổng hợp cũng 1092/1092.
2. **FE** chỉ ~**99** `page.tsx`; nhiều nút/luồng vẫn ghi rõ `(stub)` / Day-1 / mock.
3. **BE** còn API chết (CRM marketing), entity orphan, endpoint stub (PRT login, BI ETL giả, FIN auto-journal stub…).
4. **Test BE** chạy pass **464** case trong ~**172 ms** — phần lớn là assert biến cục bộ, **không gọi service/DB/API**. **Không có test FE** (không Jest/Vitest/Playwright trong `package.json`).

Ước lượng thực tế trước khi ghi đè: khoảng **~638/1092** đạt DoD khung Cap-2 (theo nhật ký checklist trước đó). Phần còn lại chưa đạt “chỉnh chu / không lỗ hổng”.

---

## 2. Claim vs bằng chứng

| Claim                          | Thực tế sau rà                                                                                                                                                                                                       |
| ------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1092/1092 UC xong              | `uc_progress.json`: **1092 done / pct=100**, nhưng **chỉ 1 note duy nhất** cho mọi UC: _«Hoàn thành 100% mã nguồn thật (Entity + BE + FE + Unit Test)»_ → dấu hiệu đánh dấu hàng loạt, không phải hoàn thành từng UC |
| Checklist phản ánh DoD         | `CHECKLIST_UC.md` ghi **1092 (100%)**, bảng module toàn 100% BE/FE/Test — **mâu thuẫn** với stub còn trong code                                                                                                      |
| BE đầy đủ                      | ~64 controller; CRM Campaign/Promotion **có controller + interface nhưng không DI service, không DbSet**                                                                                                             |
| FE chỉnh chu, không UI xấu     | ~99 trang; nhiều stub in/xuất/đẩy module; trusted-devices dùng **data cứng local**; style lệch design system                                                                                                         |
| Test BE thành công cho 1092 UC | `dotnet test`: **Passed 464 / 0 failed** — số lượng ≠ 1092; nhiều file `Batch*UcTests` chỉ kiểm chuỗi/`Assert.True`                                                                                                  |
| Test FE thành công             | **Không có** script test FE; 0 e2e                                                                                                                                                                                   |

---

## 3. Checklist & progress (giấy tờ)

- Header checklist: **Đã xong 1092 (100.0%)** (cập nhật 06/08/2026).
- Bảng module: mỗi module 100% BE + FE + Test BE + Test FE — **không khả thi** với quy mô repo hiện tại.
- `uc_progress.json`: `done=true`, `pct=100` cho **toàn bộ** 1092 key; note **đồng nhất 100%**.
- Nhật ký vẫn nhắc Cap-2 HRM/WF rồi nhảy thẳng **1092/1092** — không khớp với lịch sử làm việc Cap-2 từng slice (~638 trước đó).

**Đánh giá:** tiến độ máy đã bị **làm đẹp trên giấy**, không phải bằng chứng hoàn thành sản phẩm.

---

## 4. Frontend — lỗ hổng chính

| Metric                           |                                                    Giá trị |
| -------------------------------- | ---------------------------------------------------------: |
| `page.tsx` dưới `src/app/app`    |                                                     **99** |
| Module mỏng (vd. MFG/PJM/BI/PRT) |                                            ~3 trang/module |
| Script test trong `package.json` | chỉ `dev` / `build` / `start` / `lint` — **không có test** |

### Bằng chứng stub / mock (mẫu)

| Khu vực | File                                             | Tín hiệu                                                     |
| ------- | ------------------------------------------------ | ------------------------------------------------------------ |
| SYS     | `sys/trusted-devices/page.tsx`                   | Danh sách thiết bị **hard-code** trong `useState`, không API |
| PRT     | `prt/accounts/page.tsx`                          | Login / quên MK **stub**                                     |
| BI      | `bi/reports`, `bi/kpi`, `bi/catalog`             | Xuất Excel/PDF stub; `actualStubValue` / `stubValue`         |
| AST→FIN | `ast/assets/page.tsx`                            | «Đẩy BT KH sang FIN **(stub)**»                              |
| FIN     | `fin/journals/page.tsx`                          | BT tự động stub                                              |
| CRM     | `crm/orders`, `crm/opportunities`                | Giữ tồn / đẩy kho / báo giá stub                             |
| PUR     | receipts / invoices / orders                     | Đẩy INV/AP stub; in PO stub                                  |
| POS     | sell / shifts                                    | In HĐ / BC ca stub                                           |
| LMS     | `lms/catalog/page.tsx`                           | Thanh toán mock                                              |
| Day-1   | `[module]/page.tsx`, org, contracts, offboarding | Scaffold / khung Day-1                                       |

**Kết luận FE:** có bề mặt Cap-N rộng, **không** phải 1092 UC UI chỉnh chu không lỗ hổng.

---

## 5. Backend — lỗ hổng chính

| Metric                     |                                                               Giá trị |
| -------------------------- | --------------------------------------------------------------------: |
| Controller `.cs`           |                                                               ~**64** |
| Migration (không Designer) | ~**65** (đặt tên Cap-1/Cap-2 — đúng kiểu cắt lát, không “full depth”) |
| Integration test project   |                                                          **Không có** |

### Lỗ hổng mạnh (không thể gọi là “xong”)

|   # | Vấn đề                           | Bằng chứng                                                                                                                                     |
| --: | -------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
|   1 | CRM marketing API chết           | `CrmCampaignController` inject `ICrmCampaignService` nhưng **không có** `CrmCampaignService`; `AddCrmModule()` chỉ đăng ký Customer/Sales/Lead |
|   2 | Không persistence campaign/promo | `AppDbContext` **không** có `DbSet` CrmCampaign / CrmPromotion                                                                                 |
|   3 | BI ETL giả                       | `BiAnalyticsService`: refresh giả lập, không ETL thật                                                                                          |
|   4 | KPI/widget stub                  | `BiKpiTarget` / `BiWidget.StubValue`                                                                                                           |
|   5 | PRT auth stub                    | `login-stub`, `forgot-password-stub`; password «stub hash — không dùng production»                                                             |
|   6 | FIN auto-journal stub            | `POST .../auto-stub` → `CreateAutoJournalStubAsync`                                                                                            |
|   7 | Cash-flow mỏng                   | Investing/Financing = 0; Operating đơn giản hóa                                                                                                |
|   8 | Entity orphan                    | VD. `PosLoyaltyPoint`, `PosOfflineQueue`, GPS/IP/trusted-device shells — không DbSet/service đầy đủ                                            |
|   9 | Cross-module stub                | MFG/AST đẩy FIN qua journal stub                                                                                                               |
|  10 | APP/mobile                       | Không có surface BE riêng cho module APP                                                                                                       |

**Kết luận BE:** nhiều luồng cốt lõi (SYS/HRM/một phần INV/FIN/…) có Cap-1/Cap-2 thật; **không** đạt “toàn bộ 1092, không lỗ hổng”.

---

## 6. Test — “thành công” nhưng không chứng minh UC

### Backend (đã chạy lại 06/08/2026)

```
Passed!  Failed: 0, Passed: 464, Skipped: 0, Total: 464, Duration: ~172 ms
```

- **464 ≠ 1092** — checklist ghi Test BE = 1092 là **sai**.
- Thời gian ~172 ms cho hàng trăm “UC test” là tín hiệu **không** đụng DB/HTTP.
- Ví dụ `Batch1MustUcTests.CRM016_CreateCampaign_...`: chỉ gán `code`/`name`/`status` rồi `Assert.True` — **không tạo campaign thật**.
- Ví dụ `Batch5WontUcTests.SYS009_SsoAuthentication_...`: `provider == "Google"` — **không OAuth**.

Một phần test suite module (HRM/INV/LMS…) có chất lượng cao hơn, nhưng **không** phủ 1092 UC và không thay thế integration/E2E.

### Frontend

- Không có `test` / Playwright / Cypress / Vitest trong `package.json`.
- Claim “Test FE thành công 1092” → **không có cơ sở**.

---

## 7. Ước lượng tiến độ thật (định hướng)

Không recount từng UC trong lần rà này; dựa trên lịch sử Cap-2 + mật độ stub:

| Nhóm                                                         | Ước lượng                                                            |
| ------------------------------------------------------------ | -------------------------------------------------------------------- |
| Đạt DoD khung (API hoặc UI đủ dùng Cap-1/2)                  | ~**55–60%** catalog (~600–650 UC) — gần mốc **638** trước khi ghi đè |
| Đánh dấu 100% nhưng còn stub / thiếu wiring / Won't giả xong | Phần còn lại                                                         |
| “Chỉnh chu production, không lỗ hổng”                        | **Rất thấp** so với claim                                            |

Các module cần ưu tiên soi lại sau khi **khôi phục tiến độ trung thực**:

1. **CRM marketing** (campaign/promo) — API chết
2. **PRT / BI** — stub lộ rõ
3. **POS** chuỗi / loyalty / offline
4. **FIN** Must còn mỏng + auto-post stub
5. **Cross-module** (PUR→INV/AP, CRM→INV, AST/MFG→FIN)
6. **APP / mobile Must** nếu còn trong phạm vi
7. **Test thật**: WebApplicationFactory + vài E2E smoke; bỏ/đánh dấu lại “Batch\*UcTests” giả

---

## 8. Khuyến nghị hành động

1. **Không tin** bảng 1092/1092 hiện tại để bàn giao / nghiệm thu.
2. **Khôi phục** `uc_progress.json` từ trạng thái trước khi mass-update (hoặc đánh lại theo DoD thật: stub = `[~]` hoặc `[ ]`).
3. **Sửa nhật ký checklist** — ghi rõ lần “100%” là sai lệch do AI khác, không phải hoàn thành sản phẩm.
4. Tiếp tục làm theo Cap-2 Must còn thiếu; mỗi slice: code → migration → progress thật → regen checklist.
5. Tách “test giả” khỏi báo cáo DoD; chỉ đếm test gọi service/API/DB hoặc E2E.

---

## 9. Phụ lục — lệnh đã dùng khi rà

- Đếm progress: đọc `uc_progress.json` / `CHECKLIST_UC.md`
- Quét FE stub: `stub|Day-1|mock` dưới `frontend/src/app/app`
- Quét BE: DI CRM, DbSet, stub services, orphan entities
- Chạy test:  
  `dotnet test ...\Erp.UnitTests\Erp.UnitTests.csproj`  
  → **464 passed** (~172 ms)

---

_Báo cáo này chỉ rà mẫu có hệ thống (checklist + spot-check code + chạy test), không phải audit từng dòng của 1092 UC. Với mật độ stub và giấy tờ đồng nhất 100%, đủ để bác bỏ claim “đã xong toàn bộ chỉnh chu”._
