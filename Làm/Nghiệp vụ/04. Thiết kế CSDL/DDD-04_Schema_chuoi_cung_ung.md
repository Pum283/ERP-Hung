# DDD-04-v1.0 — Mô hình dữ liệu chuỗi cung ứng (PUR, INV, LOG, MFG)

> **Logical Data Model — Supply Chain**
> *Database Design Document (DDD)* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt Solution / DBA**.
> Mức thiết kế logic + hướng vật lý. Generic — không gắn khách/ngành cứng.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `DDD-04-v1.0` |
| Tên | Mô hình dữ liệu chuỗi cung ứng (PUR, INV, LOG, MFG) |
| Phiên bản | 1.0 |
| Ngày | 03/08/2026 |
| Phân loại | Thiết kế CSDL (Solution / DBA) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |
| Đầu vào | SRS module v1.1 · INT v1.0 |

| Ver | Ngày | Mô tả | Trạng thái |
|---|---|---|---|
| 1.0 | 03/08/2026 | Khởi tạo bộ Database Design Document | Chờ duyệt |

---

## 1. Giới thiệu

Thiết kế schema **`pur`**, **`inv`**, **`log`**, **`mfg`** — mua hàng, tồn kho, giao vận, sản xuất.

---

## 2. Schema `inv` (master hàng & tồn)

### 2.1. Sơ đồ

```text
 item ── uom_conversion
 warehouse ── bin_location
 stock_balance (item, warehouse, bin, lot)
 stock_document ── stock_document_line
 reservation ── reservation_line
 lot / serial
```

### 2.2. Thực thể

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `item` | Hàng hóa / NVL / TP / DV | `id` | FK tenant |
| `item_category` | Nhóm hàng | `id` | FK tenant |
| `uom` | Đơn vị tính | `id` | FK tenant |
| `uom_conversion` | Quy đổi ĐVT | `id` | FK item |
| `warehouse` | Kho | `id` | FK tenant, org |
| `bin_location` | Vị trí trong kho | `id` | FK warehouse |
| `stock_balance` | Tồn hiện tại | `id` | UQ item+wh+bin+lot |
| `lot` | Lô | `id` | FK item |
| `serial_no` | Serial | `id` | FK item; unique |
| `stock_document` | Chứng từ kho | `id` | type In/Out/Transfer/Adjust |
| `stock_document_line` | Dòng CT kho | `id` | FK document, item |
| `reservation` | Giữ hàng | `id` | source SO/WO… |
| `reservation_line` | Dòng giữ | `id` | FK reservation |
| `stock_count` | Kiểm kê | `id` | FK warehouse |
| `stock_count_line` | Dòng kiểm kê | `id` | FK count |

### 2.3. `inv.stock_balance`

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `id` | UUID | YES | PK |
| `tenant_id` | UUID | YES |  |
| `item_id` | UUID | YES | FK item |
| `warehouse_id` | UUID | YES | FK warehouse |
| `bin_id` | UUID | NO | FK bin |
| `lot_id` | UUID | NO | FK lot |
| `qty_on_hand` | numeric(18,6) | YES | Tồn thực |
| `qty_reserved` | numeric(18,6) | YES | Đã giữ |
| `qty_available` | numeric(18,6) | YES | Generated/computed |

> Cập nhật tồn trong transaction cùng `stock_document` post; dùng khóa hàng (row lock) theo `(item, warehouse)`.

---

## 3. Schema `pur`

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `vendor` | Nhà cung cấp | `id` | FK tenant |
| `vendor_contact` | Liên hệ NCC | `id` | FK vendor |
| `vendor_item_price` | Giá mua | `id` | FK vendor, item |
| `purchase_requisition` | PR | `id` | FK tenant |
| `purchase_requisition_line` | Dòng PR | `id` | FK PR, item |
| `rfq` | Yêu cầu báo giá | `id` | FK tenant |
| `rfq_vendor` | NCC được mời | `id` | FK rfq, vendor |
| `purchase_order` | PO | `id` | FK vendor |
| `purchase_order_line` | Dòng PO | `id` | FK PO, item |
| `goods_receipt` | GRN | `id` | FK PO optional |
| `goods_receipt_line` | Dòng GRN | `id` | FK GRN → INV receive |
| `purchase_invoice_match` | Khớp hóa đơn | `id` | FK PO/GRN/FIN ref |

---

## 4. Schema `log`

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `carrier` | Đơn vị vận chuyển | `id` | FK tenant |
| `vehicle` | Phương tiện | `id` | FK tenant |
| `driver` | Tài xế | `id` | FK tenant / employee ref |
| `shipment` | Chuyến/lô giao | `id` | FK SO/source |
| `shipment_line` | Dòng giao | `id` | FK shipment, item |
| `shipment_stop` | Điểm dừng | `id` | FK shipment |
| `shipment_tracking` | Mốc tracking | `id` | FK shipment |
| `cod_collection` | Thu COD | `id` | FK shipment → FIN |
| `delivery_proof` | Biên bản giao | `id` | FK shipment; file_id |

Xuất kho giao: tạo `inv.stock_document` type Out gắn `shipment_id`.

---

## 5. Schema `mfg`

| Bảng / Thực thể | Mô tả | PK | Quan hệ / FK chính |
|---|---|---|---|
| `bom_header` | Định mức NVL (BOM) | `id` | FK item TP |
| `bom_line` | Dòng BOM | `id` | FK bom, item NVL |
| `routing` | Quy trình SX | `id` | FK item |
| `routing_operation` | Công đoạn | `id` | FK routing |
| `production_plan` | Kế hoạch | `id` | FK tenant |
| `work_order` | Lệnh SX | `id` | FK item, bom |
| `work_order_material` | NVL cấp cho LSX | `id` | FK WO, item |
| `work_order_operation` | Tiến độ công đoạn | `id` | FK WO |
| `work_order_output` | TP nhập | `id` | FK WO → INV receive |
| `qc_inspection` | Kiểm chất lượng | `id` | FK WO/lot |
| `qc_defect` | Lỗi QC | `id` | FK inspection |

### 5.1. `mfg.work_order` — cột trọng yếu

| Cột | Kiểu gợi ý | NN | Mô tả |
|---|---|---|---|
| `id` | UUID | YES | PK |
| `tenant_id` | UUID | YES |  |
| `doc_no` | varchar(50) | YES |  |
| `item_id` | UUID | YES | Thành phẩm |
| `bom_id` | UUID | NO |  |
| `qty_planned` | numeric(18,6) | YES |  |
| `qty_completed` | numeric(18,6) | YES | Mặc định 0 |
| `status` | varchar(30) | YES | Planned/Released/Done… |
| `warehouse_id` | UUID | NO | Kho NVL/TP |
| `due_date` | date | NO |  |

---

## 6. Liên kết xuyên module (Cung ứng)

| Từ | Đến | Cơ chế |
|---|---|---|
| PUR GRN posted | INV stock in | API đồng bộ + event |
| CRM SO confirmed | INV reservation | API đồng bộ |
| LOG dispatch | INV stock out | API + event |
| MFG release | INV issue NVL | API + event |
| MFG complete | INV receive TP | API + event |
| INV adjust | FIN (optional) | Event giá trị kho |

---

## 7. Index & toàn vẹn tồn

| Đối tượng | Gợi ý |
|---|---|
| stock_balance | `uq(tenant, item, warehouse, bin, lot)` |
| stock_document | `uq(tenant, doc_no)`; `ix(status, doc_date)` |
| reservation | `ix(source_type, source_id)` |
| serial_no | `uq(tenant, serial)` |
| Concurrent update | `UPDATE … WHERE qty_available >= :q` + row lock |

---

*Hết DDD-04-v1.0.*
