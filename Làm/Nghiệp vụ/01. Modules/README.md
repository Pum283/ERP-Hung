# Danh mục SRS theo Module

**Định dạng bàn giao: Word (`.docx`) v1.1.** File `.md` là bản nguồn nội bộ.

Ngày cập nhật: 04/08/2026 (SYS + nhóm **Nhắn tin realtime**)

| STT | Module | Tài liệu Word | Nhóm | UC | Thư mục |
|---:|---|---|---:|---:|---|
| 1 | `SYS` | [SRS_SYS_v1.1_PRO.docx](./01.%20SYS%20-%20Hệ%20thống%20nền%20tảng/SRS_SYS_v1.1_PRO.docx) | 13 | 104 | `01. SYS - Hệ thống nền tảng` |
| 2 | `HRM` | [SRS_HRM_v1.1.docx](./02.%20HRM%20-%20Quản%20trị%20nhân%20sự/SRS_HRM_v1.1.docx) | 20 | 187 | `02. HRM - Quản trị nhân sự` |
| 3 | `LMS` | [SRS_LMS_v1.1.docx](./03.%20LMS%20-%20Đào%20tạo/SRS_LMS_v1.1.docx) | 11 | 74 | `03. LMS - Đào tạo` |
| 4 | `CRM` | [SRS_CRM_v1.1.docx](./04.%20CRM%20-%20CRM%20và%20Bán%20hàng/SRS_CRM_v1.1.docx) | 15 | 131 | `04. CRM - CRM và Bán hàng` |
| 5 | `POS` | [SRS_POS_v1.1.docx](./05.%20POS%20-%20POS%20bán%20lẻ/SRS_POS_v1.1.docx) | 10 | 72 | `05. POS - POS bán lẻ` |
| 6 | `PUR` | [SRS_PUR_v1.1.docx](./06.%20PUR%20-%20Mua%20hàng/SRS_PUR_v1.1.docx) | 9 | 52 | `06. PUR - Mua hàng` |
| 7 | `INV` | [SRS_INV_v1.1.docx](./07.%20INV%20-%20Kho%20và%20Tồn%20kho/SRS_INV_v1.1.docx) | 11 | 70 | `07. INV - Kho và Tồn kho` |
| 8 | `LOG` | [SRS_LOG_v1.1.docx](./08.%20LOG%20-%20Giao%20vận/SRS_LOG_v1.1.docx) | 7 | 39 | `08. LOG - Giao vận` |
| 9 | `MFG` | [SRS_MFG_v1.1.docx](./09.%20MFG%20-%20Sản%20xuất/SRS_MFG_v1.1.docx) | 8 | 46 | `09. MFG - Sản xuất` |
| 10 | `FSM` | [SRS_FSM_v1.1.docx](./10.%20FSM%20-%20Dịch%20vụ%20kỹ%20thuật/SRS_FSM_v1.1.docx) | 9 | 50 | `10. FSM - Dịch vụ kỹ thuật` |
| 11 | `PJM` | [SRS_PJM_v1.1.docx](./11.%20PJM%20-%20Quản%20lý%20dự%20án/SRS_PJM_v1.1.docx) | 7 | 42 | `11. PJM - Quản lý dự án` |
| 12 | `FIN` | [SRS_FIN_v1.1.docx](./12.%20FIN%20-%20Tài%20chính%20Kế%20toán/SRS_FIN_v1.1.docx) | 13 | 83 | `12. FIN - Tài chính Kế toán` |
| 13 | `AST` | [SRS_AST_v1.1.docx](./13.%20AST%20-%20Quản%20lý%20tài%20sản/SRS_AST_v1.1.docx) | 6 | 34 | `13. AST - Quản lý tài sản` |
| 14 | `WF` | [SRS_WF_v1.1.docx](./14.%20WF%20-%20Công%20việc%20và%20Phê%20duyệt/SRS_WF_v1.1.docx) | 7 | 40 | `14. WF - Công việc và Phê duyệt` |
| 15 | `BI` | [SRS_BI_v1.1.docx](./15.%20BI%20-%20Báo%20cáo%20và%20BI/SRS_BI_v1.1.docx) | 6 | 30 | `15. BI - Báo cáo và BI` |
| 16 | `PRT` | [SRS_PRT_v1.1.docx](./16.%20PRT%20-%20Cổng%20khách%20hàng/SRS_PRT_v1.1.docx) | 7 | 38 | `16. PRT - Cổng khách hàng` |

## Chuẩn v1.1 (như SYS)

- Trang bìa, thông tin kiểm soát, mục lục, đánh số trang, trang ký duyệt.
- Đặc tả UC dạng **bảng 8 trường** (Use Case ID → Kịch bản phụ).
- Mục 0–16 đầy đủ: phạm vi, actor, workflow E2E, entity, BR, NFR, phân quyền, KPI, nghiệm thu.
- Phụ thuộc **SYS**; generic (không gắn 1 khách/ngành cứng).

## Công cụ

| Script | Mục đích |
|---|---|
| `_tools/write_all_srs_v11.py` | Sinh lại MD + DOCX v1.1 cho 15 module (bỏ qua SYS) |
| `_tools/srs_v11_core.py` | Khung shell + render bảng UC |
| `_tools/uc_author_v11.py` | Soạn luồng UC theo hành động / domain |
| `_tools/build_srs_docx_pro.py` | Xuất Word chuyên nghiệp |
| `_tools/write_srs_sys_v1_1.py` | Bản tay riêng module SYS |

## Ghi chú

- Chuẩn viết: [00_CHUAN_VIET_SRS.md](./00_CHUAN_VIET_SRS.md)
- Catalog: `../00. Tổng quan/cay_chuc_nang_data.py` + Excel v3
- Bản `SRS_*_v1.0.*` giữ làm tham chiếu; **bản chính thức là v1.1 `.docx`**
- Sau khi duyệt BA → mới chuyển `Làm/Source`
