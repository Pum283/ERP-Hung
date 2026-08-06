# Chuẩn viết tài liệu Tích hợp liên module

## Mục tiêu
Bộ tài liệu mô tả **cách các module ERP nói chuyện với nhau** sau khi từng SRS module đã chốt — làm cầu nối giữa nghiệp vụ (BA) và thiết kế source.

## Định dạng bàn giao
- **Chính thức:** Microsoft Word (`.docx`) — bìa, mục lục, số trang, trang ký duyệt.
- Markdown (`.md`) là bản nguồn nội bộ.

## Bộ tài liệu bắt buộc

| Mã | Tên | Nội dung |
|---|---|---|
| `INT-01` | Tổng quan kiến trúc tích hợp | Nguyên tắc, mô hình Event Bus, ranh giới SYS |
| `INT-02` | Ma trận phụ thuộc module | Hard/soft dependency, gói bán E2E |
| `INT-03` | Catalog sự kiện & hợp đồng dữ liệu | Event, payload logic, chủ sở hữu master |
| `INT-04` | Luồng E2E liên module | Các journey xuyên module (PR→PO→GRN→AP…) |
| `INT-05` | Đồng bộ, lỗi & vận hành | Idempotency, retry, license tắt module |

## Nguyên tắc nội dung
1. **Generic** — không gắn 1 khách / 1 ngành cứng.
2. **SYS là xương sống** — Auth, RBAC, License, Org, File, Notify, Event Bus.
3. **Không gọi chéo cứng** module↔module; ưu tiên sự kiện / hợp đồng qua SYS.
4. **Master có chủ** — mỗi thực thể có đúng 1 module sở hữu; module khác chỉ tham chiếu.
5. **License-aware** — tắt module ⇒ chặn API/UI; dữ liệu giữ; subscriber phải degrade gracefully.
6. Truy vết về SRS module (`SRS-XXX-v1.1`) và mã UC khi cần.

## Phiên bản
- Bộ tài liệu tích hợp bắt đầu từ **v1.0**.
- Khi SRS module đổi phiên bản ảnh hưởng hợp đồng → cập nhật INT và ghi lịch sử.
