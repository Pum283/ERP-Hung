# Cấu trúc thư mục Source

> Prompt ngắn cho AI: `@docs/CAU-TRUC-THU-MUC.md` + câu yêu cầu chức năng.

## Cây thư mục

```
Source/
├── backend/
│   ├── ErpModular.sln
│   ├── Directory.Build.props
│   ├── src/
│   │   ├── Erp.Api/
│   │   │   ├── Controllers/{Module}/     # Auth, Sys, Hrm…
│   │   │   ├── Hubs/                     # WfHub · MsgHub (backlog chat)
│   │   │   ├── Filters/                  # AuthorizePermission
│   │   │   ├── Middlewares/
│   │   │   └── Program.cs
│   │   ├── Erp.Application/
│   │   │   ├── DTOs/{Module}/
│   │   │   ├── Interfaces/Services/{Module|Auth}/
│   │   │   ├── Interfaces/Repositories/
│   │   │   └── Common/
│   │   ├── Erp.Domain/
│   │   │   ├── Base/
│   │   │   ├── Entities/{Module}/
│   │   │   └── Enums/{Module}/
│   │   └── Erp.Infrastructure/
│   │       ├── Persistence/              # AppDbContext, Configurations, Seed
│   │       ├── Implementations/Services/
│   │       ├── Implementations/Repositories/
│   │       └── Security/                 # JWT, PasswordHasher
│   └── tests/Erp.UnitTests/
├── frontend/
│   ├── src/app/app/{sys|hrm|wf}/…    # UI đầy đủ theo module
│   ├── src/app/app/[module]/         # Day-1 stub CRM…PRT
│   ├── src/shared/                   # api, auth, hooks, ui, realtime, modules/
│   │   ├── modules/module-meta.ts    # Catalog switcher (cắt SKU = xóa entry)
│   │   ├── realtime/                 # wf-hub.ts · msg-hub.ts
│   │   └── theme/brand-kit.css
│   └── …
├── MODULES.json · MODULES.md         # ★ Manifest bán / cắt source
├── scripts/cut_modules.py
└── docs/                             # 01_API · 04_MSG · living
```

## Brand Kit (FE)

- File: `frontend/src/shared/theme/brand-kit.css`
- Đổi **màu chính / phụ / font / type scale / radius** chỉ tại đây.
- `globals.css` map → Tailwind (`bg-brand`, `text-body`, `font-display`…).
- Không hardcode hex trong component.

## Quy tắc thêm chức năng (vertical slice)

Ví dụ: “quản lý phòng ban” → Module `Sys`, Entity `Department`

1. `Domain/Entities/Sys/Department.cs` (+ enum nếu cần)
2. EF `Configurations/Sys/...`
3. `Application` DTO + `IXxxService`
4. `Infrastructure` service implement
5. `Api/Controllers/Sys/...Controller.cs` + `[AuthorizePermission("sys.department.manage")]`
6. Cập nhật `docs/01_DAC_TA_API.md` (living — bắt buộc cùng lượt)
7. FE: `app/app/{sys|…}/…` + `usePermissions()`
8. Nếu module mới / đổi biên: cập nhật `MODULES.json`

## Bán & cắt module

Xem [`MODULES.md`](../MODULES.md). Soft license ≠ xóa source. Clone bán → `python scripts/cut_modules.py --keep …`.

## Permission code

Dạng `{module}.{resource}.{action}` — đăng ký seed / bảng `sys.permission` (catalog toàn sản phẩm).

## Data scope

Resolve tại service (không chỉ JWT):

1. Role `BypassDataScope` → `All`
2. Else `JobLevel.DefaultScopeType` → Own / Team / Department / All
3. AND thêm `user_data_scope` (chi nhánh/kho) khi có
