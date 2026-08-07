# Hướng dẫn deploy Pum's ERP (MonsterASP / runasp.net)

Tài liệu này dành cho **AI / người vận hành** cần deploy lại BE/FE lên hosting hiện tại. Đọc hết trước khi chạy lệnh.

## 1. Mục tiêu & URL

| Thành phần | Hosting site | WebDeploy host | URL production |
|---|---|---|---|
| **BE** (ASP.NET Core 8) | `site83800` | `site83800.siteasp.net:8172` | **https://pumerpapi.runasp.net** |
| **FE** (Next.js 15 standalone + Node/HttpPlatformHandler) | `site83804` | `site83804.siteasp.net:8172` | **https://pumerp.runasp.net** |

- FE **phải** gọi API bằng **HTTPS** (`https://pumerpapi.runasp.net`). Nếu build với `http://` thì trình duyệt chặn Mixed Content khi user mở FE bằng HTTPS.
- CORS BE phải cho phép `https://pumerp.runasp.net` và `http://pumerp.runasp.net`.

## 2. File cấu hình trong thư mục này

```
Làm/File deploy/
  BE.publishSettings   # credential WebDeploy BE (KHÔNG commit public nếu repo mở)
  FE.publishSettings   # credential WebDeploy FE
  deploy.ps1           # script deploy All | BE | FE
  DEPLOY.md            # file này
```

Đọc credential từ `*.publishSettings`:

- `publishUrl`, `msdeploySite`, `userName`, `userPWD`
- `destinationAppUrl` = URL app (tham chiếu)

Nếu đổi mật khẩu trên Control Panel → cập nhật lại `userPWD` trong 2 file publish settings.

## 3. Điều kiện máy local

- Windows
- .NET SDK 8+
- Node.js + npm
- **IIS Web Deploy 3**:  
  `C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe`
- File BE `.env` tồn tại:  
  `Làm/Source/backend/src/Erp.Api/.env`  
  (có `CONNECTION_STRING` trỏ DB SiteASP, JWT, Cloudinary…)

## 4. Cách deploy nhanh (khuyến nghị)

PowerShell:

```powershell
cd "d:\HungNDM\ERP Hùng\Làm\File deploy"

# Chỉ backend
.\deploy.ps1 -Target BE

# Chỉ frontend
.\deploy.ps1 -Target FE

# Cả hai
.\deploy.ps1
```

Sau khi deploy FE: bảo user **Ctrl+F5** (hard refresh).

## 5. Quy tắc quan trọng (đã học từ lần deploy lỗi)

### 5.1 BE

1. `dotnet publish` Release, **không** self-contained.
2. Copy `.env` vào thư mục publish.
3. Xóa `Erp.Api.exe` (tránh rủi ro OutOfProcess + EXE trên MonsterASP).
4. `web.config`: AspNetCoreModuleV2, `hostingModel="inprocess"`, `ASPNETCORE_ENVIRONMENT=Production`.
5. Deploy WebDeploy tới **cả hai**:
   - `site83800\wwwroot`
   - `site83800` (skip thư mục `wwwroot` khi sync root)
6. Verify:
   - `https://pumerpapi.runasp.net/api/auth/me` → **401**
   - CORS OPTIONS từ Origin `https://pumerp.runasp.net` có `Access-Control-Allow-Origin`

### 5.2 FE

1. Next config cần `output: "standalone"` (`Làm/Source/frontend/next.config.ts`).
2. **Không** build trực tiếp trong path có Unicode (`ERP Hùng`) nếu gặp lỗi symlink `EPERM` — script copy sang `C:\Temp\erp-fe-build` rồi `npm install` + `npm run build`.
3. Build với env:
   ```text
   NEXT_PUBLIC_API_URL=https://pumerpapi.runasp.net
   ```
   Biến `NEXT_PUBLIC_*` được **inline lúc build** — chỉ sửa `web.config` env **không đủ**.
4. Đóng gói deploy:
   - Copy `.next/standalone/*`
   - Copy `.next/static` → `standalone/.next/static`
   - Copy `public` → `standalone/public`
   - Thêm `web.config` HttpPlatformHandler (`processPath=node`, `arguments=.\server.js`, `PORT=%HTTP_PLATFORM_PORT%`)
5. Deploy WebDeploy tới **`site83804` (site root)**, skip `wwwroot`.  
   (Thực tế site FE chạy Node từ **root**, không phải `wwwroot`.)
6. Verify:
   - `https://pumerp.runasp.net/login` title chứa ERP
   - Bundle JS chứa `https://pumerpapi.runasp.net`, **không** còn `http://pumerpapi...`

### 5.3 CORS (BE `Program.cs`)

Policy origins tối thiểu:

- `http://localhost:2222`
- `http://127.0.0.1:2222`
- `http://pumerp.runasp.net`
- `https://pumerp.runasp.net`

Sau khi sửa CORS → phải deploy lại BE.

## 6. Checklist khi nào deploy cái gì

| Thay đổi | Deploy |
|---|---|
| Controller / Service / Entity / Migration BE | `BE` |
| `.env` (DB, JWT, Cloudinary) | `BE` |
| CORS / JWT config trong `Program.cs` | `BE` |
| Page / component / API client FE | `FE` |
| Đổi `NEXT_PUBLIC_API_URL` | `FE` (rebuild bắt buộc) |
| Lần đầu / nghi ngờ lệch phiên bản | `All` |

## 7. Troubleshooting nhanh

| Triệu chứng | Nguyên nhân thường gặp | Cách xử |
|---|---|---|
| FE HTTPS, request `login` đỏ, “Provisional headers” | Mixed Content (FE HTTPS gọi API HTTP) | Rebuild FE với `https://pumerpapi.runasp.net`, Ctrl+F5 |
| `site83800.siteasp.net` ra trang MonsterASP, API 404 | Sai hostname kiểm tra | Dùng **pumerpapi.runasp.net**, không dùng `*.siteasp.net` làm URL app |
| FE vẫn “Your site is ready…” / NODE test cũ | Node chạy ở site root, deploy nhầm/thiếu | Sync full package vào `site83804` root + `web.config` HttpPlatform |
| WebDeploy 401 | Sai `userPWD` | Đối chiếu Control Panel / `*.publishSettings` |
| App pool disabled sau OutOfProcess+EXE | MonsterASP suspend | Support ticket host; dùng InProcess + DLL |
| Login “Sai tên đăng nhập…” | API sống, sai user/pass | Đúng credential seed DB; không phải lỗi deploy |
| CORS thiếu `Access-Control-Allow-Origin` | BE cũ chưa có origin FE | Deploy lại BE sau khi cập nhật CORS |

## 8. Lệnh msdeploy mẫu (nếu không dùng script)

```powershell
$msdeploy = "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe"

# BE -> wwwroot
& $msdeploy -verb:sync `
  "-source:contentPath=C:\Temp\erp-api-publish" `
  "-dest:contentPath=site83800\wwwroot,ComputerName=https://site83800.siteasp.net:8172/msdeploy.axd?site=site83800,UserName=site83800,Password=*** ,AuthType=Basic" `
  -allowUntrusted -enableRule:AppOffline

# FE -> site root
& $msdeploy -verb:sync `
  "-source:contentPath=C:\Temp\erp-fe-deploy" `
  "-dest:contentPath=site83804,ComputerName=https://site83804.siteasp.net:8172/msdeploy.axd?site=site83804,UserName=site83804,Password=*** ,AuthType=Basic" `
  -allowUntrusted -enableRule:AppOffline `
  -skip:objectName=dirPath,absolutePath=wwwroot
```

Password lấy từ `BE.publishSettings` / `FE.publishSettings` — **không hardcode vào chat/log công khai**.

## 9. Verify sau deploy (bắt buộc)

```powershell
# BE
curl.exe -s -D - -o NUL "https://pumerpapi.runasp.net/api/auth/me"
# expect: HTTP 401

curl.exe -s -D - -o NUL -X OPTIONS "https://pumerpapi.runasp.net/api/auth/login" `
  -H "Origin: https://pumerp.runasp.net" `
  -H "Access-Control-Request-Method: POST" `
  -H "Access-Control-Request-Headers: content-type,authorization"
# expect: Access-Control-Allow-Origin: https://pumerp.runasp.net

# FE
curl.exe -s "https://pumerp.runasp.net/login" | findstr /i "Pum _next"
```

## 10. Hướng dẫn ngắn cho AI agent

1. Đọc `BE.publishSettings` + `FE.publishSettings` + `deploy.ps1`.
2. Xác định scope: BE / FE / All.
3. Chạy `.\deploy.ps1 -Target ...` từ thư mục `Làm/File deploy`.
4. Chạy mục **Verify** ở §9.
5. Nếu FE: nhắc user Ctrl+F5; đảm bảo API URL trong bundle là **https**.
6. Không deploy lên hostname `*.siteasp.net` như URL người dùng — chỉ dùng WebDeploy; URL công khai là `*.runasp.net`.
7. Không đánh dấu xong khi mới upload file mà chưa verify HTTP 401 (BE) / HTML Next (FE).

## 11. Liên kết nội bộ source

- BE project: `Làm/Source/backend/src/Erp.Api/`
- FE project: `Làm/Source/frontend/`
- BE env mẫu: `Làm/Source/backend/src/Erp.Api/.env.example`
- CORS: `Làm/Source/backend/src/Erp.Api/Program.cs` (policy `DevFrontend`)
