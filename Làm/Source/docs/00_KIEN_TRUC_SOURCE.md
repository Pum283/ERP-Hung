# Kiến trúc Source — Phase 1

## Quyết định

| Hạng mục              | Chọn                                                         | Lý do                                             |
| --------------------- | ------------------------------------------------------------ | ------------------------------------------------- |
| Kiểu triển khai       | Modular monolith                                             | 1 host API, folder theo module — tách service sau |
| BE stack              | .NET 8 · Clean Architecture                                  | Bám Digi API v2                                   |
| FE stack              | Next.js (App Router) · 1 shell                               | Bám Digione FE, không tách nhiều FE app Day-1     |
| DB                    | **Microsoft SQL Server** · 1 database · schema `sys`, `hrm`… | Đã chốt PO (03/08/2026); khớp DDD                 |
| AuthN                 | JWT Bearer                                                   | Digi                                              |
| AuthZ                 | RBAC + ScopeType 4 tầng + Department/JobLevel                | Digi + DDD-MASTER                                 |
| License               | `sys.license` / `license_module`                             | PKG-04                                            |
| Giao tiếp liên module | Outbox/Inbox (thêm dần)                                      | INT-01/05                                         |
| Realtime              | SignalR — `/hubs/wf` (duyệt) + `/hubs/msg` (chat, backlog)   | SYS-13 · [04_MSG_REALTIME.md](./04_MSG_REALTIME.md) |

## Luồng phụ thuộc BE

```
Erp.Api → Erp.Application, Erp.Infrastructure
Erp.Application → Erp.Domain
Erp.Infrastructure → Erp.Application, Erp.Domain
Erp.Domain → (không phụ thuộc layer khác)
```

Luồng UC: `Controller → IService → DbContext/Repository → DB`

## Khác Digione multi-solution

Digione chạy nhiều solution/FE/BE/port theo phân hệ.  
ERP Modular Phase 1 **gom 1 BE + 1 FE**; vẫn giữ **module folder** để sau này tách nếu cần.

## Bán SKU / cắt source

| Lớp | Cơ chế | Khi nào |
|---|---|---|
| Soft | `LicenseModule.IsEnabled` | SaaS multi-tenant |
| Hard | Clone + `scripts/cut_modules.py` | On-prem / không giao IP module chưa mua |

Chi tiết: [`MODULES.md`](../MODULES.md) · manifest [`MODULES.json`](../MODULES.json).  
DI đã tách: `AddSysModule` · `AddModKit` · `AddHrmModule` · `AddWfModule`.

## Khác Digi single-product

Digi tập trung HR/corp. ERP Modular giữ **license gate + 16 schema** theo PKG/DDD.
