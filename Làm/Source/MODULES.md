# MODULES — bán SKU & cắt source

| | |
|---|---|
| Manifest máy | [`MODULES.json`](./MODULES.json) |
| Soft-disable (SaaS) | `LicenseModule.IsEnabled` + `LicenseModuleMiddleware` |
| Hard-cut (on-prem / clone bán) | Script [`scripts/cut_modules.py`](./scripts/cut_modules.py) |

Hai lớp **không thay nhau**:

1. **SaaS / multi-tenant** — giữ full source, tắt module bằng license (runtime).
2. **Clone bán cho khách** — copy repo → giữ module đã mua → **xóa code** module không mua → CI/CD riêng.

## Quy tắc biên (bắt buộc khi code mới)

| Được | Không được |
|---|---|
| `{MODULE} → SYS` (org, user, RBAC, file, audit) | `{MODULE_A}` import entity/service EF của `{MODULE_B}` (trừ SYS) |
| Gọi WF qua **interface / outbox** (`IApprovalPort`) | `Hrm*Service` cầm `DbSet` của module khác; `WfRuntime` đọc bảng HRM trực tiếp *(nợ hiện tại — trả dần)* |
| Folder theo code: `Controllers/{Code}`, `Entities/{Code}`, `app/app/{code}` | Nhét entity HRM/WF vào `Configurations/Mod` |
| Permission seed gắn `ModuleCode` | Tạo bảng quyền riêng trong module nghiệp vụ |

**SYS và kit MOD không cắt.** Cắt WF chỉ khi không còn HRM (HRM `depends_on` WF).

## Workflow bán (clone)

```bash
# 1. Clone sang repo/CI riêng của khách
git clone … customer-erp && cd customer-erp/Làm/Source

# 2. Dry-run — xem sẽ xóa gì
python scripts/cut_modules.py --keep SYS,HRM,WF --dry-run

# 3. Áp dụng (stub auto; full module in checklist)
python scripts/cut_modules.py --keep SYS,HRM,WF --apply

# 4. Build
dotnet build backend/ErpModular.sln
cd frontend && pnpm build
```

Script sẽ:

- Xóa folder stub trống (`Controllers/Crm`, `Entities/Crm`, …) không nằm trong `--keep`
- Gỡ code khỏi `MODULES.json` / gợi ý chỉnh `module-meta.ts` + `DEFAULT_TYPES`
- In **checklist thủ công** cho module `maturity=full` (HRM/WF): DI · DbContext · seed · migration

## Đăng ký DI (đã tách theo module)

Trong `Erp.Infrastructure/DependencyInjection.cs`:

```csharp
services.AddSysModule();   // luôn
services.AddModKit();      // luôn (Day-1 stubs)
services.AddHrmModule();   // bỏ dòng này khi cắt HRM
services.AddWfModule();    // bỏ khi cắt WF
```

Khi bán: **xóa folder + xóa 1 dòng DI** trước; DbContext/seed làm theo checklist script.

## FE

| Path thật | Ghi chú |
|---|---|
| `frontend/src/app/app/{sys\|hrm\|wf}/…` | UI đầy đủ |
| `frontend/src/app/app/[module]/page.tsx` | Day-1 stub cho CRM…PRT |
| `frontend/src/shared/modules/module-meta.ts` | Catalog switcher — cắt thì xóa entry |

Doc cũ ghi `src/modules/{module}` — **không dùng**; route sống dưới App Router.

## Migration / DB khi cắt

- Schema đã tách (`hrm`, `wf`, `erp_sys`) — tốt cho cắt.
- Lịch sử EF **chung một chuỗi**: sau cut source, DB khách có thể giữ schema thừa (an toàn) hoặc drop schema module không mua bằng script SQL riêng (ngoài EF).
- **Không** rewrite tay `AppDbContextModelSnapshot` cho từng SKU nếu chưa tách project — ưu tiên bỏ `DbSet` + configs khỏi compile, migration mới chỉ cho tenant greenfield.

## Nợ kỹ thuật chặn cắt sạch HRM/WF

1. `WfRuntimeService` đọc `LeaveRequests` / `Employees`
2. `HrmLeaveService` / `HrmRecruitService` inject `IWfRuntimeService`
3. `DbSeeder` / `AppDbContext` còn monolithic

Hướng trả nợ: port `IDocumentApprovalBridge` + outbox; seed `IModuleSeed` theo folder `Seed/{Code}`.

## Soft vs hard

| | Soft license | Hard cut |
|---|---|---|
| Mục tiêu | Multi-tenant SaaS | On-prem / IP không giao module chưa mua |
| Code trên disk | Full | Chỉ module đã mua |
| Runtime | 403 nếu tắt | Compile không còn module |
