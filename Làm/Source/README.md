# Pum's ERP — Source

Phase 1: **1 FE shell · 1 BE host · 1 DB (Microsoft SQL Server)** (schema theo module), bán theo license module.

Tham chiếu mẫu:
- Clean Architecture / authz Digi: `Mẫu/ERP Digi - 20-5 - BIG SOURCE/erp-corporation-api-v2`
- FE shell / RBAC tập trung Digione: `Mẫu/Digione - ERP`

Nghiệp vụ đã chốt: `Làm/Nghiệp vụ` (SRS · INT · PKG · DDD).

## Cấu trúc

```
Source/
├── backend/                 # .NET 8 — ErpModular.sln
│   ├── src/Erp.Api          # Host HTTP (port 7000)
│   ├── src/Erp.Application
│   ├── src/Erp.Domain
│   ├── src/Erp.Infrastructure
│   └── tests/
├── frontend/                # Next.js shell (port 3000)
└── docs/                    # 00 kiến trúc · 01 API living · 02 M1 scope · 03 events · 04 MSG realtime
```

## Yêu cầu máy dev

| Tool | Gợi ý |
|---|---|
| .NET SDK | 8+ (đang build net8.0) |
| Node + pnpm | Node 20+ · pnpm 9+ |
| SQL Server | 2019+ / LocalDB / Docker — DB `erp_modular` |

## Chạy nhanh

### Backend

```bash
cd backend
# chỉnh ConnectionStrings (SQL Server) trong appsettings.json hoặc copy .env.example → .env
# Mặc định: Server=localhost;Database=erp_modular;Trusted_Connection=True;TrustServerCertificate=True;
dotnet run --project src/Erp.Api
```

- Swagger: http://localhost:1111/swagger  
- Health: `GET /api/sys/health`  
- Login: `POST /api/auth/login` body `{ "username":"admin", "password":"!Abc123" }`  
- Seed tự chạy khi DB trống (tenant `DEMO`) — tạo DB `erp_modular` trên SQL Server trước (hoặc để EF tạo nếu quyền đủ).

### Frontend

```bash
cd frontend
cp .env.example .env.local
pnpm install
pnpm dev
```

Mở http://localhost:2222

## Phân quyền (chốt)

| Trục | Cơ chế |
|---|---|
| Functional | User → Role → Permission (`module.resource.action`) |
| Data scope 4 tầng | JobLevel.`DefaultScopeType` = Own/Team/Department/All |
| Bypass | Role.`BypassDataScope` |
| Org | Department (+ OrgUnit chi nhánh) |

Chi tiết: `docs/CAU-TRUC-THU-MUC.md`, `docs/QUY-UOC-CONG-DEV.md`.

## Nhắn tin realtime (backlog G4.9)

Chat nội bộ SYS-13 — SignalR `/hubs/msg` · REST `/api/sys/msg/*`.  
Spec: [`docs/04_MSG_REALTIME.md`](./docs/04_MSG_REALTIME.md) · UC `UC_SYS_095`…`104`.

## Module packs

Folder entity/controller đã chuẩn bị cho 16 module (`Sys`, `Hrm`, `Crm`…).  
Phase 1 triển khai dần theo gói bán (PKG) — bắt đầu từ **SYS**.
