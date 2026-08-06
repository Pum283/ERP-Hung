# Chuẩn viết SRS theo Module (ERP bán module)

## Định dạng bàn giao
- **File chính để duyệt / gửi khách:** `SRS_{MODULE}_vX.Y.docx` (Microsoft Word)
- Markdown (`.md`) chỉ là bản nguồn làm việc nội bộ (nếu có), **không** dùng làm bản giao chính thức.

## Mục tiêu
Mỗi module có **một SRS độc lập**, đủ để:
1. Chốt nghiệp vụ với khách / nội bộ
2. Ước lượng & thiết kế source sau này
3. Bán / triển khai riêng module (kèm SYS)

## Cấu trúc bắt buộc (16 phần)

| Phần | Nội dung |
|------|----------|
| 0 | Thông tin tài liệu & lịch sử thay đổi |
| 1 | Giới thiệu, mục tiêu, phạm vi giá trị |
| 2 | Phạm vi In / Out of Scope |
| 3 | Tác nhân & stakeholder |
| 4 | Thuật ngữ & viết tắt |
| 5 | Ngữ cảnh module, phụ thuộc license/module khác |
| 6 | Catalog chức năng (Module → Nhóm → UC) |
| 7 | Đặc tả chức năng theo nhóm — **bảng UC chuẩn 8 trường** (xem mục dưới) |
| 8 | Workflow end-to-end chính |
| 9 | Mô hình dữ liệu domain (thực thể chính) |
| 10 | Quy tắc nghiệp vụ tổng hợp |
| 11 | Yêu cầu phi chức năng (NFR) |
| 12 | Tích hợp & sự kiện liên module |
| 13 | Phân quyền & bảo mật |
| 14 | Báo cáo & KPI |
| 15 | Giả định, rủi ro, câu hỏi mở |
| 16 | Tiêu chí nghiệm thu & truy vết |

## Format bảng đặc tả Use Case (bắt buộc)

Mỗi UC là **một bảng 2 cột**, có caption `Bảng N. Đặc tả Use Case "…"`, các trường theo đúng thứ tự:

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | `UC_{MODULE}_{NNN}` |
| **Tên Use Case** | Tên chức năng |
| **Tác nhân** | Vai trò thực hiện |
| **Mô tả chức năng** | Cho phép [tác nhân] thực hiện chức năng "…" thuộc nhóm … Mô tả chi tiết: … |
| **Điều kiện tiên quyết** | Gạch đầu dòng (`•`), ngăn bằng `<br>` trong Word |
| **Yêu cầu** | UX/UI tiếng Việt, Validation, Audit Trail; kèm MoSCoW / BR / AC |
| **Kịch bản chính** | Các bước đánh số `1.` `2.` `3.`… |
| **Kịch bản phụ** | Phân cấp theo bước chính: `3.1` / `3.1.1`, `4.1`… (Hủy, validate lỗi, Rollback/Exception Log) + ngoại lệ nghiệp vụ |

Quy ước trình bày Word: cột trái tô nền xanh nhạt; xuống dòng trong ô bằng paragraph (không gộp 1 dòng).

## Nguyên tắc nội dung
- **Generic**: không gắn 1 khách / 1 ngành cứng; ngành hóa bằng cấu hình/template sau.
- **Bán được**: nêu rõ phụ thuộc SYS và module khuyến nghị kèm.
- **Truy vết**: mỗi chức năng có mã `UC_{MODULE}_{NNN}`.
- **MoSCoW**: map từ ưu tiên danh mục (Bắt buộc→Must, Cao→Should, Trung bình→Could, Thấp→Won't/Later).
- Ngôn ngữ: tiếng Việt chuyên nghiệp; thuật ngữ kỹ thuật giữ English khi cần (RBAC, OTP, BOM…).

## Nguồn sự thật
- Cây chức năng: `../00. Tổng quan/cay_chuc_nang_data.py`
- Excel catalog: `../00. Tổng quan/Danh_muc_Module_Chuc_nang_ERP_v3.xlsx`

## Quy trình hoàn thiện
1. Sinh SRS từ generator + meta module
2. Rà soát nghiệp vụ từng module
3. Chốt → đóng băng phiên bản SRS v1.0
4. Mới sang cấu trúc source / coding
