# Quyết định G1 / Bundle M1

| Thuộc tính | Giá trị |
|---|---|
| Ngày | 03/08/2026 |
| Người quyết | AI (theo ủy quyền PO: tự gợi ý & quyết, generic) |
| Trạng thái | **Đã chốt hướng M1** |

---

## 1. Câu “Có bán FIN Day-1 trong M1 không?” nghĩa là gì?

**Giải thích đơn giản:**

| Thuật ngữ | Nghĩa |
|---|---|
| **M1** | Bản sản phẩm **đầu tiên bán/triển khai được** (MVP thương mại) — chưa cần đủ 16 module |
| **Day-1** | Những gì **có trong gói ngay từ ngày giao bản M1** (khách dùng được liền) |
| **FIN** | Module **Tài chính – Kế toán** (sổ cái, công nợ, hạch toán…) |
| **Câu hỏi** | Trong gói M1 lúc mới ra, **có bắt buộc kèm FIN** không, hay để sau? |

**Ví dụ:**  
- FIN Day-1 = **Có** → khách mua gói Nhân sự cũng phải có (hoặc được kèm) kế toán sổ sách ngay.  
- FIN Day-1 = **Không** → khách dùng HRM/duyệt phép… trước; khi cần post lương / công nợ mới mua thêm FIN.

**Quyết định:** FIN Day-1 = **Không** (để Phase 2).  
Lý do: HRM chạy được trên SYS+WF mà không cần sổ kế toán; giảm phạm vi M1, nhanh có bản demo/bán được.

---

## 2. Vì sao chọn bundle này? (theo phụ thuộc)

Thứ tự phụ thuộc cứng gợi ý khi xây:

```text
SYS (nền) → WF (duyệt)
         → HRM          ← ít phụ thuộc nhất sau SYS
         → INV          ← hub hàng hóa
         → PUR / POS / CRM / LOG / MFG (cần INV hoặc chỉ SYS)
         → FIN          ← hub tiền (nhận chứng từ từ module khác)
```

| Ứng viên | Hard deps | Độ nặng M1 | Kết luận |
|---|---|---|---|
| B1 Nhân sự số | SYS (+ WF khuyến nghị) | Thấp | **Chọn làm M1** |
| B5 P2P | SYS, PUR, INV, FIN | Trung bình | Phase 2 |
| B2 Bán hàng | SYS, CRM, INV, LOG, FIN | Cao | Phase 2–3 |
| B8 Enterprise | Gần như full | Rất cao | Không làm Day-1 |

**Chốt M1 = B1 — Nhân sự số**  
- Chứng minh được: authz (Department/JobLevel/scope) + duyệt WF + nghiệp vụ người.  
- Không bị kẹt hard-dep LOG/MFG/INV.  
- FIN không chặn go-live HRM.

---

## 3. Phạm vi M1 đã chốt

### Must (Day-1 — phải có)

| Module | Vai trò |
|---|---|
| **SYS** | Auth, RBAC, org, license, audit… |
| **HRM** | Hồ sơ NV, công/phép khung, hợp đồng khung… (UC Must) |
| **WF** | Duyệt phép / đề xuất (tập trung) |

### Must bổ sung M1 (sau Day-1 slice — đã chốt backlog 04/08/2026)

| Hạng mục | Vai trò |
|---|---|
| **SYS-13 Nhắn tin realtime** | Chat nội bộ 1-1 · SignalR `/hubs/msg` · lịch sử · unread (`UC_SYS_095…100`) |

Chi tiết: `Làm/Source/docs/04_MSG_REALTIME.md` · checklist mục **G4.9**.

### Should (Phase 2 — ngay sau M1, cùng “họ Nhân sự”)

| Module | Vai trò |
|---|---|
| LMS | Đào tạo onboarding |
| AST | Thu hồi tài sản khi nghỉ |
| FIN | Post chi phí lương / sổ (khi bắt đầu E2E tiền) |

### Không bao gồm trong M1

POS, CRM, PUR, INV, LOG, MFG, FSM, PJM, BI, PRT — làm ở các wave sau theo PKG.

### Journey INT

- **E2E-05 Hire to Retire** (phần Must: tạo NV → gán org/dept/job level → phép có duyệt WF).  
- Không cam kết post sổ lương (thiếu FIN).

---

## 4. Các quyết định “chung chung” kèm theo

| Hạng mục | Quyết định |
|---|---|
| 2FA Day-1 | **Không** — làm Should sau (SRS vẫn giữ thiết kế) |
| Ngôn ngữ FE Day-1 | **Chỉ tiếng Việt (`vi`)** |
| Branding | **Pum's ERP** (logo để sau) |
| DoD 1 UC | API + permission + UI tối thiểu + chạy được trên SQL Server |
| SRS viết tay ưu tiên | **SYS** (đã có) + **HRM nhóm Must** trước khi code sâu HRM |
| Git | PO đẩy toàn bộ thư mục `Làm` |
| SQL Server | Hosted (connection trong `Source/backend/src/Erp.Api/.env` — **không commit**) |
| Nghiệm thu Gate | **PO** (chưa có khách) |
| Bảng giá | Tạm **chưa cần** |

---

## 5. Wave code sau Gate G1

| Wave | Nội dung |
|---|---|
| W0 / G2 | SYS hoàn thiện + WF khung + FE shell + migration SQL Server |
| W1 / G3 | HRM Must |
| W2 | LMS / AST (Should) |
| W3 | FIN + B5 hoặc B2 (commerce) — Phase 2 thương mại |

---

*Cập nhật checklist & kế hoạch theo quyết định này.*
