# Phân nhóm UC 3 cấp — mọi module còn lại

|                  |                                                                                                        |
| ---------------- | ------------------------------------------------------------------------------------------------------ |
| Mã               | `PHAN-NHOM-UC-ALL-v1`                                                                                  |
| Ngày             | 04/08/2026                                                                                             |
| PO               | Làm **cấp 1 (Cần ngay)** cho mọi module                                                                |
| SYS (riêng)      | [PHAN_NHOM_SYS_UC.md](./PHAN_NHOM_SYS_UC.md) — N1+N2 đã xong                                           |
| Checklist        | [CHECKLIST_UC.md](./CHECKLIST_UC.md) · (cũ: [CHECKLIST_UC_TOAN_BO.md](./CHECKLIST_UC_TOAN_BO.md) stub) |
| Trạng thái cấp 1 | **DONE** 04/08/2026 · Day-1 masters/docs + HRM/WF · ~287/1092 UC                                       |
| Trạng thái cấp 2 | **IN_PROGRESS** · INV FEFO/giữ hàng/HSD DONE · Cap-2 Must tiếp theo                                         |

> **Cấp 1** = masters + chứng từ/vận hành gốc đủ Day-1.  
> **Cấp 2** = vận hành đầy đủ / kênh / báo cáo sâu.  
> **Cấp 3** = Could/Won't / mobile / polish / phụ thuộc hạ tầng.

---

## HRM (187 UC · đã 17)

|   Cấp | UC (gợi ý)                                                                     | Nội dung                                                                                        |
| ----: | ------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------- |
| **1** | `001–004`, `006`, `012`, `026–027`, `029–030`, `032–036`, `039–043`, `045–046` | Org vận hành HR · mã NV · xuất Excel · trạng thái NS · HĐPL phụ lục/gia hạn/thanh lý · lương HĐ |
| **2** | Tuyển dụng `047+`, chấm công, lương kỳ, BHXH, đánh giá…                        | DONE tới `187` + `017` giấy tờ (skip `174`; KPI Could sau)                                      |
| **3** | Could/Won't còn lại                                                            |                                                                                                 |

## WF (40 · đã 6)

|   Cấp | UC                                                                 | Nội dung                                                                                    |
| ----: | ------------------------------------------------------------------ | ------------------------------------------------------------------------------------------- |
| **1** | `001`, `004–007`, `009–010`, `014`, `017`, `023–024`, `033`, `038` | Loại việc · dự án · task giao việc · ticket · quy tắc duyệt · escalate stub · khối lượng mở |
| **2** | `031` mobile · `032` ủy quyền · `040` dashboard đầy đủ             | `032`+`040` DONE · `031` APP sau                                                            |
| **3** | Could còn lại                                                      |                                                                                             |

## LMS (74 · Cap-2 gần đủ Must học viên)

|   Cấp | UC                                      | Nội dung                                                               |
| ----: | --------------------------------------- | ---------------------------------------------------------------------- |
| **1** | `001–006`, `009–010`, `012`, `014` DONE | CTĐT · khóa · chương/bài · NHCH · đề · điểm đạt/lần thi                |
| **2** | Offline + Online + Thi/CC + GV/BC DONE  | Offline · learn · quiz/cert · GV `049–051` · BC `065–066,070` · skip `058` |
| **3** | Could/Won't                             |                                                                        |

## CRM (131 · Cap-1 master DONE · Lead+Opp Cap-2)

|   Cấp | UC                                                                                              | Nội dung                                                                          |
| ----: | ----------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| **1** | `001–006`, `008–011`, `014–015` DONE                                                            | KH CN/DN · trùng · gộp · phân loại · phụ trách · 360° · liên hệ · import · search |
| **2** | Lead+Opp DONE · Quote/Order DONE · **Marketing/promo `016,019,023,026,029,031,032–038` WIRED** · omni tiếp | `/app/crm/campaigns` · `/app/crm/promotions` (+ sync POS · BC voucher) |
| **3** | Could/Won't                                                                                     |                                                                                   |

## POS (72 · Cap-1 config DONE)

|   Cấp | UC                                                        | Nội dung                                                                          |
| ----: | --------------------------------------------------------- | --------------------------------------------------------------------------------- |
| **1** | `001–003`, `007`, `009–010`, `012`, `014–016`, `019` DONE | Điểm bán · quầy · máy in · quyền · nhóm/SP · BOM · ngưng · sync · bảng giá · thuế |
| **2** | Ca+bán+promo DONE · **BOM→INV `054` + alerts `055` + đóng ca→FIN `059` + BC `065–067` + chuỗi `069,072` WIRED** | `/app/pos/reports` 8 tab (live) · `/app/pos/stores` target |
| **3** | Could/Won't                                               |                                                                                   |

## PUR (52 · Cap-1 DONE)

|   Cấp | UC                                                        | Nội dung                                                |
| ----: | --------------------------------------------------------- | ------------------------------------------------------- |
| **1** | `001`, `003`, `009–010`, `014`, `017–019`, `026–028` DONE | NCC · liên hệ · SP–NCC · giá · PR · duyệt · PO · gửi PO |
| **2** | PO `030–033` + GRN `034–035,037` + HĐ `040–041,043` DONE · BC `048,051,052` DONE · trả NCC tiếp | `/app/pur/reports` · CSV |
| **3** | Could/Won't                                               |                                                         |

## INV (70 · Cap-1 DONE ~80–85%)

|   Cấp | UC                                          | Nội dung                                                                                 |
| ----: | ------------------------------------------- | ---------------------------------------------------------------------------------------- |
| **1** | `001–005`, `007–008`, `010–012`, `014` DONE | SKU · nhóm · ĐVT · lô/serial · giá vốn · ngưng · import · min/max · kho · loại · thủ kho |
| **2** | Stock + BC + FEFO/giữ/HSD `029,037–038,042–045,048` DONE | FEFO · reservation · ATP · lô/HSD · `/app/inv/stock` · `/app/inv/reports` |
| **3** | Could/Won't                                 |                                                                                          |

## LOG (39 · Cap-1 DONE ~85%)

|   Cấp | UC                                  | Nội dung                                                                       |
| ----: | ----------------------------------- | ------------------------------------------------------------------------------ |
| **1** | `001`, `006`, `008–014`, `017` DONE | ĐVVC · lệnh giao · tách đợt · pick · xuất · vận đơn · hủy · TX · TT · thất bại |
| **2** | COD + hoàn + `034` DONE | Phiếu hoàn · dashboard ops · **% giao đúng hạn** (`PromisedAt`) · COD |
| **3** | Could/Won't                         |                                                                                |

## MFG (46 · Cap-1 DONE ~85%)

|   Cấp | UC                                                 | Nội dung                                                                         |
| ----: | -------------------------------------------------- | -------------------------------------------------------------------------------- |
| **1** | `001–003`, `006–008`, `013`, `017–020`, `022` DONE | TP/BTP/NVL · xưởng · BOM · KH · lệnh SX · duyệt · phát hành · xuất NVL · nhập TP |
| **2** | WIP `023–025` + costing `027,029,031` + BC `041–043,045–046` DONE | Phế · pause/close · NVL · TP · `/app/mfg/reports` · CSV · skip OEE `044` |
| **3** | Could/Won't                                        |                                                                                  |

## FSM (50 · Cap-1 DONE ~85%)

|   Cấp | UC                                                 | Nội dung                                                                               |
| ----: | -------------------------------------------------- | -------------------------------------------------------------------------------------- |
| **1** | `001–003`, `005`, `008–010`, `013–015`, `017` DONE | Loại DV · mã lỗi · linh kiện · SLA · TB · serial/BH · ticket · ưu tiên · KT · escalate |
| **2** | Close + BC + kho LK `024,037–039,047` DONE | Lịch · đóng SLA · `/app/fsm/parts` · `/app/fsm/reports` · skip APP `019/041–042` |
| **3** | Could/Won't                                        |                                                                                        |

## PJM (42 · Cap-1+2 Must gần đủ)

|   Cấp | UC                                   | Nội dung                                                            |
| ----: | ------------------------------------ | ------------------------------------------------------------------- |
| **1** | `001–002`, `004–009`, `011–012` DONE | Loại DA · WBS mẫu · TT · tạo DA · KH/HĐ · PM · NS · WBS · gán người |
| **2** | Progress + cost/close + BC DONE | `%` · milestone · CP/NVL · NT · DT soft · đóng · P&L `/app/pjm/reports` |
| **3** | Could/Won't                          |                                                                     |

## FIN (83 · Cap-1 DONE · Cap-2 cash/bank/AP/AR/VAT/revenue)

|   Cấp | UC                                          | Nội dung                                                                                   |
| ----: | ------------------------------------------- | ------------------------------------------------------------------------------------------ |
| **1** | `001–004`, `006`, `008–010`, `012–015` DONE | COA · nhóm TK · kỳ · khóa sổ · TTCP · HTTT · thuế · BT · đảo · sổ cái/CT · BT tự động stub |
| **2** | Quỹ+NH+AP+AR+VAT DONE · DT/COGS `057–058,060` DONE | POS · đơn · AR · GVHB · soft-wire · `/app/fin/revenue` |
| **3** | Could/Won't                                 |                                                                                            |

## AST (34 · Cap-1+2 DONE)

|   Cấp | UC                               | Nội dung                                                                                   |
| ----: | -------------------------------- | ------------------------------------------------------------------------------------------ |
| **1** | `001–004`, `008–012`, `014` DONE | Nhóm TS · thẻ · nguyên giá · vị trí · PP/tỷ lệ KH · tính KH · sổ · đẩy FIN stub · ghi tăng |
| **2** | `016–018`, `021–022`, `030–032`, `034` DONE | Điều chuyển · bàn giao · thanh lý · kiểm kê · sổ/KH/vị trí · CSV · FE |
| **3** | Could/Won't                      |                                                                                            |

## BI (30 · Cap-1 DONE · Cap-2 KPI DONE)

|   Cấp | UC                                          | Nội dung                                                                    |
| ----: | ------------------------------------------- | --------------------------------------------------------------------------- |
| **1** | `001–003`, `006–008`, `013–014`, `016` DONE | Dataset · refresh · quyền · DB stub · widget · danh mục BC · chạy BC · xuất |
| **2** | `018`, `019`, `021` DONE                    | Mục tiêu · ngưỡng · Target vs Actual · so sánh kỳ · `/app/bi/kpi`         |
| **3** | Could/Won't · schedule/email Should         |                                                                             |

## PRT (38 · Cap-1 DONE · Cap-2 package DONE)

|   Cấp | UC                                              | Nội dung                                                       |
| ----: | ----------------------------------------------- | -------------------------------------------------------------- |
| **1** | `001–003`, `007–008`, `014–016`, `019–020` DONE | Đăng ký/login stub · liên kết KH · đơn · công nợ stub · ticket |
| **2** | `037` DONE                                      | Cấu hình module portal theo gói · `/app/prt/package`           |
| **3** | Could/Won't · vendor portal                     |                                                                |

## SYS còn (Nhóm 3)

|   Cấp | UC                                                                                  |
| ----: | ----------------------------------------------------------------------------------- |
| **3** | `009`, `012`, `031`, `058`, `062`, `064`, `071`, `077`, `082`, `093–094`, `103–104` |

---

## DoD cấp 1 (Day-1)

1. API list/upsert (hoặc transition) trong schema module.
2. Menu + license module (soft) + FE list tối thiểu `/app/{mod}`.
3. Ghi `uc_progress.json` · regenerate checklist.
