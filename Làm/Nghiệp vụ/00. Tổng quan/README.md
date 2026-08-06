# 00. Tổng quan — Pum's ERP

Tài liệu điều phối sản phẩm **Pum's ERP** (tầm nhìn, catalog, kế hoạch đến 100%, checklist tiến độ & UC).

## Timeline (theo dõi liên tục)

| File | Nội dung |
|---|---|
| **[Timeline/CHECKLIST_TIEN_DO_GIAI_DOAN.md](./Timeline/CHECKLIST_TIEN_DO_GIAI_DOAN.md)** | Checklist giai đoạn G0→G7 · % · Gate |
| **[Timeline/KE_HOACH_TONG_THE_DEN_100.md](./Timeline/KE_HOACH_TONG_THE_DEN_100.md)** | Kế hoạch chi tiết từng giai đoạn, DoD, mục **[BẠN] phải cung cấp** |
| **[Timeline/CHECKLIST_UC.md](./Timeline/CHECKLIST_UC.md)** | **1000+ UC** — checklist từng UC (living) |
| [`Timeline/uc_progress.json`](./Timeline/uc_progress.json) | Tiến độ máy (giữ `[x]` khi regenerate) |
| [`Timeline/build_uc_checklist.py`](./Timeline/build_uc_checklist.py) | Sinh lại checklist UC từ catalog |

## Tài liệu khác

| File | Nội dung |
|---|---|
| **[QUYET_DINH_G1_M1.md](./QUYET_DINH_G1_M1.md)** | Quyết định bundle M1 (B1), FIN Day-1, phụ lục Must/Should |
| `cay_chuc_nang_data.py` | Nguồn catalog module / nhóm / chức năng (**nguồn UC**) |
| `build_danh_muc_module.py` | Sinh Excel danh mục module |

## Thứ tự đọc

1. **Checklist tiến độ** (`Timeline/…`) — đang ở giai đoạn nào  
2. **Checklist UC** — UC nào xong / còn Must  
3. Kế hoạch tổng thể — việc cần làm & thông tin từ PO  
4. SRS / INT / PKG / DDD trong `01`…`04`  
5. Source: `Làm/Source/README.md`

## Cập nhật UC liên tục

```bash
# Sau khi code xong UC: sửa Timeline/uc_progress.json rồi:
python Timeline/build_uc_checklist.py
```

## Trạng thái nhanh

- Sản phẩm: **Pum's ERP**  
- DB: **Microsoft SQL Server** (hosted) · Gate **G1/G2 OPEN**  
- M1: **B1** SYS+HRM+WF · **G3 DONE** · **G4 ~55%** · backlog **SYS-13 nhắn tin realtime** (G4.9)  
- M1 ước ~**79%** · UC **1092** · còn: chat realtime · UAT · backup · rồi LMS/AST  
- Spec chat: `Làm/Source/docs/04_MSG_REALTIME.md` · API: `01_DAC_TA_API.md`  
