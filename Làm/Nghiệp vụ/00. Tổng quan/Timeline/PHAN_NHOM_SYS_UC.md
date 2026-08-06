# Phân nhóm 78 UC SYS còn lại

|                       |                                                      |
| --------------------- | ---------------------------------------------------- |
| Mã                    | `PHAN-NHOM-SYS-UC-v1`                                |
| Ngày chốt             | 04/08/2026                                           |
| PO                    | Đã chốt — **Nhóm 1 + 2 đã implement** (04/08/2026)   |
| Checklist UC          | [CHECKLIST_UC.md](./CHECKLIST_UC.md) |
| Plan implement Nhóm 1 | [KE_HOACH_SYS_NHOM1.md](./KE_HOACH_SYS_NHOM1.md)     |

> Chia theo **mức cần thiết vận hành M1/G4**, không copy nguyên cột Must/Should/Could của catalog.  
> Baseline lúc phân nhóm: SYS **26/104** xong · **78** còn (`[~]` / `[ ]`).

---

## Nhóm 1 — Cần ngay (~28 UC)

Thiếu thì user/admin khó dùng ổn định trên M1. **Đã ship.**

| Cluster             | UC           | Tiêu đề                               |
| ------------------- | ------------ | ------------------------------------- |
| Auth cơ bản         | `UC_SYS_002` | Đăng xuất                             |
|                     | `UC_SYS_003` | Đổi mật khẩu                          |
|                     | `UC_SYS_006` | Chính sách độ mạnh mật khẩu           |
|                     | `UC_SYS_007` | Khóa tài khoản sau N lần sai          |
|                     | `UC_SYS_083` | Chính sách hết hạn phiên              |
| User admin          | `UC_SYS_016` | Xóa mềm người dùng                    |
|                     | `UC_SYS_017` | Gán người dùng vào chi nhánh          |
|                     | `UC_SYS_018` | Reset mật khẩu bởi Admin              |
| Tenant / locale     | `UC_SYS_034` | Quản lý công ty / tenant              |
|                     | `UC_SYS_041` | Cấu hình múi giờ / ngôn ngữ / tiền tệ |
| License vận hành    | `UC_SYS_044` | Khai báo module trong hệ thống        |
|                     | `UC_SYS_046` | Quản lý gói license                   |
|                     | `UC_SYS_047` | Giới hạn số user / chi nhánh theo gói |
|                     | `UC_SYS_048` | Cảnh báo / gia hạn license            |
| Config tối thiểu    | `UC_SYS_051` | Tham số cấu hình toàn cục             |
|                     | `UC_SYS_053` | Danh mục dùng chung                   |
|                     | `UC_SYS_054` | Mẫu số chứng từ                       |
|                     | `UC_SYS_055` | Sinh mã tự động                       |
| Audit login         | `UC_SYS_079` | Nhật ký đăng nhập / thất bại          |
| Thông báo tối thiểu | `UC_SYS_059` | Thông báo in-app                      |
|                     | `UC_SYS_063` | Cấu hình sự kiện kích hoạt            |
| Import/Export khung | `UC_SYS_072` | Import Excel/CSV theo mẫu             |
|                     | `UC_SYS_073` | Tải file mẫu import                   |
|                     | `UC_SYS_074` | Export Excel                          |
| MSG gần xong        | `UC_SYS_096` | Tạo hội thoại nhóm (FE)               |
|                     | `UC_SYS_101` | Đính kèm file trong tin nhắn          |
|                     | `UC_SYS_102` | Thu hồi tin nhắn                      |

**Ngoài Nhóm 1:** `004`/`005` quên MK+OTP → Nhóm 2 (cùng email gateway).

---

## Nhóm 2 — Cần sớm (~30 UC)

Cần để bán/triển khai tenant thật; chưa chặn smoke Day-1.

| Cluster          | UC                                    |
| ---------------- | ------------------------------------- |
| Auth nâng        | `004` · `005` · `008` · `010` · `011` |
| User mở rộng     | `019` · `020` · `022`                 |
| RBAC sâu         | `024` · `029` · `032` · `033`         |
| Tổ chức          | `037` · `040` · `043` · `035`         |
| Config / lịch    | `052` · `056` · `057` · `042`         |
| Thông báo + kênh | `060` · `061` · `065` · `088` · `089` |
| File             | `068` · `069` · `070`                 |
| Audit / IE       | `075` · `076` · `080` · `081`         |
| Tích hợp         | `084` · `085` · `086` · `090`         |
| i18n cơ bản      | `091` · `092`                         |

---

## Nhóm 3 — Để sau (~20 UC)

Could / Won't / phụ thuộc hạ tầng nặng hoặc ít dùng M1.

| UC                                        | Ghi chú           |
| ----------------------------------------- | ----------------- |
| `009` SSO/OAuth                           | Could · IdP khách |
| `012` Thiết bị tin cậy                    | Won't catalog     |
| `031` Quyền trường nhạy cảm               | Schema phức tạp   |
| `058` Version cấu hình                    | Could             |
| `062` Push mobile                         | Chưa có app       |
| `064` Tùy chọn TB cá nhân                 | Could             |
| `071` Quét virus                          | Could             |
| `077` Export hàng loạt                    | Could             |
| `082` IP allow/deny                       | Won't catalog     |
| `093` Theme/logo · `094` Home theo role   | Could UX          |
| `103` Tìm kiếm tin · `104` Mute hội thoại | Could MSG         |

---

## Tóm tắt

| Nhóm           | Số ~ | Trạng thái                                                      |
| -------------- | ---: | --------------------------------------------------------------- |
| **1 Cần ngay** |   28 | **DONE** — API + FE tối thiểu                                   |
| **2 Cần sớm**  |   30 | **DONE** — API Day-1 / stub email·SMS                           |
| **3 Để sau**   |   20 | Backlog (SSO, push, Won't…)                                     |

### Quy ước DoD khi implement

1. API (+ FE nếu admin/user chạm được) đủ dùng như các UC M1 đã `[x]`.
2. Cập nhật `uc_progress.json` → `python build_uc_checklist.py`.
3. Email/SMS Dev: **stub** (log), không bắt SMTP/SMS thật trong Nhóm 1.
