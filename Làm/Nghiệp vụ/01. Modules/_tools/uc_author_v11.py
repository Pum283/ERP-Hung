#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Soạn đặc tả UC v1.1 theo nghiệp vụ từng chức năng (không dùng 4 bước khuôn mẫu chung).
Mỗi UC nhận luồng/tiền điều kiện/ngoại lệ/BR/AC riêng dựa trên tên + nhóm + module.
"""
from __future__ import annotations

from srs_v11_core import PRIORITY_MOSCOW, uc

# Actor mặc định theo nhóm (override meta nếu thiếu)
MODULE_GROUP_ACTORS: dict[str, dict[str, str]] = {
    "HRM": {
        "01": "HR Admin",
        "02": "HR Officer",
        "03": "HR Officer",
        "04": "HR Officer",
        "05": "Line Manager",
        "06": "Recruiter",
        "07": "HR Officer",
        "08": "HR Admin",
        "09": "HR Officer",
        "10": "Employee",
        "11": "Employee",
        "12": "HR Officer",
        "13": "Payroll Accountant",
        "14": "HR Officer",
        "15": "HR Officer",
        "16": "HR Admin",
        "17": "HR Officer",
        "18": "HR Officer",
        "19": "HR Admin",
        "20": "HR Admin",
    },
    "LMS": {
        "01": "Training Admin",
        "02": "Training Admin",
        "03": "Instructor",
        "04": "Learner",
        "05": "Training Admin",
        "06": "Learner",
        "07": "Training Admin",
        "08": "Training Admin",
        "09": "Training Admin",
        "10": "Training Admin",
        "11": "Training Admin",
    },
    "CRM": {
        "01": "Sales Admin",
        "02": "Sales Rep",
        "03": "Sales Rep",
        "04": "Sales Rep",
        "05": "Sales Manager",
        "06": "Sales Rep",
        "07": "Sales Admin",
        "08": "Sales Rep",
        "09": "Sales Manager",
        "10": "Sales Rep",
        "11": "Sales Admin",
        "12": "Sales Manager",
        "13": "Sales Admin",
        "14": "Sales Admin",
        "15": "Sales Manager",
    },
    "POS": {
        "01": "Store Manager",
        "02": "Cashier",
        "03": "Cashier",
        "04": "Cashier",
        "05": "Store Manager",
        "06": "Cashier",
        "07": "Store Manager",
        "08": "Store Manager",
        "09": "Store Manager",
        "10": "Store Manager",
    },
    "PUR": {
        "01": "Procurement Officer",
        "02": "Procurement Officer",
        "03": "Procurement Officer",
        "04": "Procurement Manager",
        "05": "Procurement Officer",
        "06": "Warehouse Receiver",
        "07": "Procurement Officer",
        "08": "Procurement Admin",
        "09": "Procurement Manager",
    },
    "INV": {
        "01": "Inventory Admin",
        "02": "Warehouse Keeper",
        "03": "Warehouse Keeper",
        "04": "Warehouse Keeper",
        "05": "Warehouse Keeper",
        "06": "Warehouse Keeper",
        "07": "Inventory Admin",
        "08": "Warehouse Keeper",
        "09": "Inventory Admin",
        "10": "Inventory Admin",
        "11": "Inventory Admin",
    },
    "LOG": {
        "01": "Logistics Officer",
        "02": "Dispatcher",
        "03": "Driver",
        "04": "Logistics Officer",
        "05": "Logistics Officer",
        "06": "Logistics Admin",
        "07": "Logistics Manager",
    },
    "MFG": {
        "01": "Production Planner",
        "02": "Production Planner",
        "03": "Shop Floor Supervisor",
        "04": "QC Officer",
        "05": "Production Planner",
        "06": "Shop Floor Supervisor",
        "07": "Production Admin",
        "08": "Production Manager",
    },
    "FSM": {
        "01": "Service Admin",
        "02": "Dispatcher",
        "03": "Field Technician",
        "04": "Service Admin",
        "05": "Field Technician",
        "06": "Service Manager",
        "07": "Service Admin",
        "08": "Service Admin",
        "09": "Service Manager",
    },
    "PJM": {
        "01": "Project Manager",
        "02": "Project Manager",
        "03": "Team Member",
        "04": "Project Manager",
        "05": "Project Manager",
        "06": "Project Admin",
        "07": "Project Manager",
    },
    "FIN": {
        "01": "Accountant",
        "02": "Accountant",
        "03": "Accountant",
        "04": "Accountant",
        "05": "Cashier Finance",
        "06": "Accountant",
        "07": "Accountant",
        "08": "Chief Accountant",
        "09": "Accountant",
        "10": "Accountant",
        "11": "Chief Accountant",
        "12": "Finance Admin",
        "13": "Chief Accountant",
    },
    "AST": {
        "01": "Asset Admin",
        "02": "Asset Officer",
        "03": "Asset Officer",
        "04": "Asset Officer",
        "05": "Asset Admin",
        "06": "Asset Manager",
    },
    "WF": {
        "01": "Workflow Admin",
        "02": "Workflow Admin",
        "03": "Approver",
        "04": "Requester",
        "05": "Workflow Admin",
        "06": "Workflow Admin",
        "07": "Workflow Admin",
    },
    "BI": {
        "01": "BI Admin",
        "02": "Analyst",
        "03": "Analyst",
        "04": "BI Admin",
        "05": "Executive",
        "06": "BI Admin",
    },
    "PRT": {
        "01": "Portal Admin",
        "02": "Customer User",
        "03": "Customer User",
        "04": "Customer User",
        "05": "Portal Admin",
        "06": "Customer User",
        "07": "Portal Admin",
    },
}


def detect_action(ten: str) -> str:
    t = ten.lower().strip()

    # Tránh khớp nhầm: "sản xuất" chứa "xuất"; ưu tiên cụm dài / đầu câu
    if t.startswith("xuất") or any(
        k in t
        for k in (
            "export",
            "xuất excel",
            "xuất danh",
            "xuất báo",
            "xuất file",
            "xuất phiếu",
            "in mẫu",
            "in phiếu",
            "in / xuất",
        )
    ):
        return "export"

    rules = [
        (("duyệt", "phê duyệt", "chấp nhận"), "approve"),
        (("từ chối", "reject"), "reject"),
        (("mở khóa", "reopen"), "unlock"),
        (("khóa", "chốt sổ", "đóng kỳ", "đóng ca"), "lock"),
        (("tính lương", "tính công", "tính giá", "chạy payroll", "tính toán"), "calculate"),
        (("import", "nhập hàng loạt", "import excel"), "import"),
        (("cảnh báo", "nhắc hạn", "alert", "reminder"), "alert"),
        (("báo cáo", "dashboard", "phân tích", "so sánh thực tế"), "report"),
        (("cấu hình", "khai báo", "thiết lập", "định nghĩa"), "config"),
        (("gán", "xếp lịch", "xếp ca", "phân công", "điều phối"), "assign"),
        (("upload", "đính kèm", "scan"), "upload"),
        (("xóa mềm", "hủy phiếu", "hủy đơn", "thanh lý", "ngưng dùng"), "cancel"),
        (("xem ", "tra cứu", "tìm kiếm", "pipeline", "lịch sử"), "view"),
        (("tạo ", "thêm ", "sinh mã", "mở phiếu", "đăng tin"), "create"),
        (("cập nhật", "điều chỉnh", "gia hạn", "điều chuyển", "chuyển trạng"), "update"),
        (("gửi ", "submit", "gửi phiếu", "gửi đơn"), "submit"),
        (("nhận hàng", "check-in", "check in", "quét mã"), "receive"),
        (("thanh toán", "thu tiền", "chi tiền", "hoàn tiền"), "payment"),
    ]
    for keys, action in rules:
        if any(k in t for k in keys):
            return action
    if t.startswith("tạo") or t.startswith("thêm"):
        return "create"
    if t.startswith("xem") or t.startswith("tra cứu"):
        return "view"
    if t.startswith("quản lý") or t.startswith("quản trị"):
        return "manage"
    if "tính" in t and any(k in t for k in ("lương", "công", "giá", "khấu hao", "payroll")):
        return "calculate"
    return "process"


def _module_domain_hint(code: str, group_name: str) -> str:
    hints = {
        "HRM": "nhân sự / hồ sơ / công – phép – lương",
        "LMS": "đào tạo / khóa học / bài kiểm tra",
        "CRM": "khách hàng / cơ hội / báo giá – đơn hàng",
        "POS": "bán lẻ tại quầy / ca / hóa đơn",
        "PUR": "mua hàng / NCC / đơn mua – nhận hàng",
        "INV": "kho / tồn / nhập–xuất–chuyển",
        "LOG": "giao vận / chuyến / giao nhận",
        "MFG": "sản xuất / lệnh SX / BOM – QC",
        "FSM": "dịch vụ hiện trường / ticket / kỹ thuật viên",
        "PJM": "dự án / task / tiến độ – chi phí",
        "FIN": "tài chính – kế toán / chứng từ / sổ",
        "AST": "tài sản / khấu hao / bàn giao",
        "WF": "quy trình duyệt / hộp chờ / escalation",
        "BI": "báo cáo / dataset / dashboard",
        "PRT": "cổng khách hàng / self-service",
    }
    base = hints.get(code, "nghiệp vụ module")
    return f"{base}; nhóm «{group_name}»"


def build_pre(code: str, group_name: str, ten: str, action: str, actor: str) -> list[str]:
    items = [
        f"License module `{code}` đang hiệu lực trên tenant.",
        f"Dữ liệu tham chiếu liên quan tới «{ten}» đã được cấu hình trong phạm vi data scope.",
    ]
    if action in ("approve", "reject"):
        items.append("Tồn tại chứng từ/phiếu ở trạng thái chờ duyệt và thuộc quyền duyệt của user.")
    if action == "lock":
        items.append("Kỳ/ca/chứng từ mục tiêu đang ở trạng thái cho phép khóa (đã rà soát).")
    if action == "calculate":
        items.append("Dữ liệu nguồn (công, tồn, tỷ giá…) đã sẵn sàng và đạt điều kiện chốt.")
    if action in ("export", "report", "view"):
        items.append("User có quyền xem dữ liệu trong data scope tương ứng.")
    if action == "payment":
        items.append("Chứng từ thanh toán hợp lệ; quỹ/công nợ cho phép ghi nhận.")
    if action == "receive":
        items.append("Có chứng từ nguồn (PO/TO/SO…) ở trạng thái cho phép nhận.")
    if "tự" in ten.lower() or "self" in ten.lower() or actor == "Employee" or actor == "Customer User":
        items.append("Người dùng đang thao tác trên hồ sơ/phiên thuộc về chính mình (self-service) trừ khi được ủy quyền.")
    if code == "HRM" and any(k in ten.lower() for k in ("lương", "payslip", "bảng lương")):
        items.append("Dữ liệu lương thuộc nhóm nhạy cảm — user có field permission tương ứng.")
    if code == "INV" and any(k in ten.lower() for k in ("xuất", "nhập", "kiểm kê", "chuyển")):
        items.append("Kho/vị trí thao tác thuộc data scope và còn hiệu lực.")
    if code == "FIN" and any(k in ten.lower() for k in ("kỳ", "sổ", "bút toán")):
        items.append("Kỳ kế toán mục tiêu chưa bị đóng cứng (trừ chức năng mở khóa có kiểm soát).")
    return items


def build_flow(code: str, group_name: str, ten: str, mota: str, action: str, actor: str) -> list[str]:
    domain = _module_domain_hint(code, group_name)
    detail = mota.strip() if mota else ten
    # Luồng riêng theo hành động — luôn nhắc tên chức năng cụ thể
    if action == "create":
        return [
            f"{actor} mở chức năng «{ten}» trong nhóm {group_name}.",
            f"Hệ thống kiểm tra license `{code}`, permission và data scope; hiển thị form tạo mới.",
            f"Người dùng nhập/chọn các trường nghiệp vụ cho «{ten}» ({detail}).",
            "Hệ thống validate bắt buộc, định dạng, trùng khóa và ràng buộc tham chiếu.",
            f"Lưu bản ghi/chứng từ «{ten}» vào CSDL; sinh mã theo Sequence (nếu áp dụng); ghi Audit Trail.",
            "Hiển thị thông báo thành công và cập nhật danh sách/chi tiết mới nhất.",
        ]
    if action == "update":
        return [
            f"{actor} tìm và mở bản ghi liên quan tới «{ten}» trong phạm vi được phép.",
            "Hệ thống kiểm tra trạng thái cho phép sửa (chưa khóa kỳ / chưa post / đúng owner).",
            f"Người dùng cập nhật thông tin theo yêu cầu «{ten}» ({detail}).",
            "Validate thay đổi; chặn nếu vi phạm rule hoặc xung đột phiên bản.",
            "Lưu thay đổi, ghi before/after Audit Trail, phát sự kiện liên module nếu cấu hình.",
            "Làm mới UI và thông báo kết quả.",
        ]
    if action == "approve":
        return [
            f"{actor} mở hộp chờ / chứng từ cần xử lý cho «{ten}».",
            "Hệ thống hiển thị nội dung, lịch sử duyệt, file đính kèm và cảnh báo ràng buộc.",
            "Người duyệt kiểm tra tính hợp lệ nghiệp vụ theo checklist nhóm.",
            f"Chọn [Duyệt] cho «{ten}», nhập ghi chú nếu bắt buộc.",
            "Hệ thống chuyển trạng thái, ghi Audit, gửi thông báo cho người liên quan / module nguồn.",
            "Cập nhật hộp chờ và cho phép bước nghiệp vụ tiếp theo.",
        ]
    if action == "reject":
        return [
            f"{actor} mở chứng từ liên quan «{ten}».",
            "Xem nội dung và chọn [Từ chối] / trả bổ sung.",
            "Nhập lý do bắt buộc (không cho để trống).",
            "Hệ thống cập nhật trạng thái Rejected/Returned, ghi Audit, thông báo người gửi.",
            "Người gửi có thể chỉnh sửa và gửi lại theo quy trình.",
        ]
    if action == "lock":
        return [
            f"{actor} chọn kỳ/ca/đối tượng cần khóa trong «{ten}».",
            "Hệ thống chạy kiểm tra tiền điều kiện (thiếu dữ liệu, chứng từ treo, lệch số…).",
            "Hiển thị báo cáo kiểm tra; user xác nhận [Khóa].",
            "Khóa trạng thái; chặn thao tác sửa trực tiếp sau khóa.",
            "Ghi Audit + thông báo; cho phép quy trình phụ thuộc (ví dụ tính lương / xuất sổ) chạy tiếp.",
        ]
    if action == "unlock":
        return [
            f"{actor} yêu cầu mở khóa đối tượng trong «{ten}» kèm lý do.",
            "Hệ thống kiểm tra quyền mở khóa đặc biệt và chính sách tenant.",
            "Xác nhận mở khóa có giới hạn thời gian/phạm vi nếu cấu hình.",
            "Ghi Audit bắt buộc (who/when/why); thông báo người liên quan.",
            "Cho phép chỉnh sửa có kiểm soát rồi khóa lại.",
        ]
    if action == "calculate":
        return [
            f"{actor} chọn phạm vi tính toán cho «{ten}» (kỳ, đơn vị, bộ lọc).",
            f"Hệ thống nạp dữ liệu nguồn liên quan ({detail}).",
            "Chạy engine tính theo rule cấu hình; log chi tiết từng bước lỗi nếu có.",
            "Hiển thị kết quả nháp để rà soát; cho phép điều chỉnh có audit trước khi chốt.",
            "Xác nhận ghi nhận kết quả chính thức; phát sự kiện cho FIN/module liên quan nếu cần.",
            "Thông báo hoàn tất và cập nhật trạng thái kỳ/tính toán.",
        ]
    if action == "export":
        return [
            f"{actor} mở «{ten}», chọn bộ lọc và định dạng xuất (Excel/PDF/mẫu in).",
            "Hệ thống kiểm tra quyền xuất và áp data scope vào truy vấn.",
            f"Sinh file/bản in theo mẫu «{ten}» ({detail}).",
            "Ghi nhật ký export (ai/lúc nào/bộ lọc).",
            "Cho phép tải xuống hoặc gửi qua kênh cấu hình (email) nếu có quyền.",
        ]
    if action == "import":
        return [
            f"{actor} tải file mẫu (nếu có) và chọn file import cho «{ten}».",
            "Hệ thống parse file, map cột, validate từng dòng.",
            "Hiển thị preview lỗi/cảnh báo; cho phép sửa file hoặc bỏ dòng lỗi theo policy.",
            "Xác nhận import; ghi nhận transaction + Audit; tạo job log.",
            "Báo cáo số dòng thành công/thất bại; cho phép tải file lỗi.",
        ]
    if action == "alert":
        return [
            f"Job hệ thống hoặc {actor} kích hoạt kiểm tra điều kiện «{ten}».",
            f"Hệ thống quét dữ liệu theo rule cảnh báo ({detail}).",
            "Tập hợp đối tượng vi phạm/đến hạn trong data scope.",
            "Gửi thông báo (in-app/email/SMS theo cấu hình) cho đúng đối tượng.",
            "Ghi NotificationLog / lịch sử cảnh báo để truy vết.",
        ]
    if action == "report":
        return [
            f"{actor} mở «{ten}» và chọn bộ lọc thời gian / đơn vị / tiêu chí.",
            "Hệ thống kiểm tra quyền dataset và data scope.",
            f"Truy vấn và tổng hợp số liệu ({detail}); hiển thị bảng/biểu đồ.",
            "Cho phép drill-down (nếu có) hoặc xuất Excel/PDF.",
            "Ghi nhận truy vấn báo cáo trên audit/usage log khi cấu hình bật.",
        ]
    if action == "config":
        return [
            f"{actor} mở màn hình cấu hình «{ten}» trong {group_name}.",
            "Hệ thống hiển thị giá trị hiện tại và ràng buộc phụ thuộc.",
            f"Người dùng thiết lập tham số ({detail}) và lưu nháp/áp dụng.",
            "Validate xung đột cấu hình; yêu cầu xác nhận nếu ảnh hưởng chứng từ đang mở.",
            "Lưu cấu hình, ghi Audit, làm mới cache cấu hình module.",
            "Thông báo hiệu lực cấu hình (ngay / từ kỳ sau).",
        ]
    if action == "assign":
        return [
            f"{actor} chọn đối tượng nguồn trong «{ten}».",
            "Chọn người nhận / ca / đơn vị / tài nguyên đích theo data scope.",
            "Hệ thống kiểm tra xung đột lịch, định biên, năng lực hoặc quyền.",
            "Xác nhận gán; lưu phân công; gửi thông báo cho bên được gán.",
            "Cập nhật lịch/board và ghi Audit.",
        ]
    if action == "upload":
        return [
            f"{actor} mở bản ghi liên quan và chọn «{ten}».",
            "Chọn file; hệ thống kiểm tra loại/dung lượng/virus scan (nếu bật).",
            "Upload qua dịch vụ File của SYS; gắn metadata với đối tượng nghiệp vụ.",
            "Ghi Audit; hiển thị file trên danh sách đính kèm.",
        ]
    if action == "cancel":
        return [
            f"{actor} chọn đối tượng cần hủy/ngưng trong «{ten}».",
            "Hệ thống kiểm tra trạng thái cho phép hủy và chứng từ phụ thuộc.",
            "Yêu cầu lý do; xác nhận cảnh báo tác động.",
            "Cập nhật trạng thái Cancelled/Inactive; không xóa cứng nếu đã phát sinh giao dịch.",
            "Ghi Audit + thông báo; rollback mềm các bước phụ thuộc theo rule.",
        ]
    if action == "view":
        return [
            f"{actor} mở «{ten}» và nhập tiêu chí tìm kiếm/lọc.",
            "Hệ thống áp permission + data scope, trả kết quả phân trang.",
            f"Người dùng xem chi tiết / timeline / pipeline theo nhu cầu ({detail}).",
            "Các thao tác tiếp (sửa/duyệt) chỉ hiện khi đủ quyền và đúng trạng thái.",
        ]
    if action == "submit":
        return [
            f"{actor} hoàn thiện dữ liệu cho «{ten}» ở trạng thái nháp.",
            "Chọn [Gửi duyệt / Xác nhận] (submit).",
            "Hệ thống validate đủ điều kiện gửi; chuyển trạng thái Submitted/In Approval.",
            "Tạo việc duyệt (WF hoặc duyệt nội module); gửi thông báo.",
            "Khóa sửa một phần theo policy khi đang chờ duyệt.",
        ]
    if action == "receive":
        return [
            f"{actor} mở chứng từ nhận liên quan «{ten}».",
            "Quét/chọn dòng hàng hoặc nhiệm vụ cần nhận.",
            "Nhập số lượng/tình trạng thực nhận; hệ thống so với chứng từ nguồn.",
            "Xác nhận nhận; cập nhật tồn/tiến độ; ghi Audit.",
            "Xử lý lệch (thiếu/thừa/hỏng) theo rule; thông báo bên liên quan.",
        ]
    if action == "payment":
        return [
            f"{actor} chọn chứng từ cần thu/chi trong «{ten}».",
            "Nhập phương thức, số tiền, tham chiếu giao dịch.",
            "Hệ thống kiểm tra số còn phải thu/chi và giới hạn quỹ.",
            "Ghi nhận thanh toán; cập nhật công nợ; in/xuất biên lai nếu cần.",
            "Ghi Audit; đồng bộ sự kiện sang FIN/POS/module liên quan.",
        ]
    if action == "manage":
        return [
            f"{actor} mở danh mục quản lý «{ten}» ({domain}).",
            "Thực hiện thêm/sửa/ngưng hiệu lực bản ghi danh mục trong data scope.",
            "Hệ thống validate mã duy nhất và tham chiếu đang dùng.",
            "Lưu thay đổi + Audit; các form nghiệp vụ khác nhận danh mục mới theo cache/refresh.",
            "Chặn xóa cứng nếu bản ghi đã được tham chiếu bởi chứng từ.",
        ]
    # process default — still specific
    return [
        f"{actor} khởi tạo thao tác «{ten}» trong nhóm {group_name}.",
        f"Hệ thống kiểm tra license `{code}`, quyền RBAC và tiền điều kiện nghiệp vụ ({detail}).",
        f"Người dùng thực hiện các bước nhập liệu/xác nhận đặc thù của «{ten}».",
        "Hệ thống xử lý logic domain, cập nhật trạng thái và dữ liệu liên quan trong một giao dịch.",
        "Ghi Audit Trail; phát thông báo/sự kiện nếu cấu hình.",
        "Hiển thị kết quả thành công và trạng thái mới trên UI.",
    ]


def build_alt(code: str, group_name: str, ten: str, action: str) -> list[str]:
    alts = [
        f"User thiếu permission hoặc ngoài data scope khi gọi «{ten}» → từ chối (UI ẩn/disabled hoặc API 403) và ghi audit.",
    ]
    if action in ("create", "update", "config", "manage", "submit"):
        alts.append("Vi phạm ràng buộc duy nhất / tham chiếu không tồn tại → báo lỗi field-level, không lưu.")
    if action in ("approve", "reject", "lock", "unlock", "submit", "cancel"):
        alts.append("Conflict trạng thái (đã duyệt/đã khóa/đã hủy) → chặn thao tác, hiển thị trạng thái hiện tại.")
    if action == "calculate":
        alts.append("Thiếu dữ liệu nguồn hoặc rule cấu hình không đầy đủ → dừng job, liệt kê lỗi chi tiết để sửa.")
    if action == "import":
        alts.append("File sai định dạng hoặc vượt ngưỡng dòng → từ chối import, hướng dẫn tải mẫu chuẩn.")
    if action == "payment":
        alts.append("Số tiền vượt số còn phải thu/chi hoặc quỹ không đủ → từ chối ghi nhận.")
    if action == "receive":
        alts.append("Số nhận vượt dung sai cho phép so với chứng từ nguồn → yêu cầu duyệt lệch hoặc tách dòng xử lý.")
    if code == "HRM" and any(k in ten.lower() for k in ("lương", "công", "phép")):
        alts.append("Thao tác sau khi kỳ công/lương đã khóa → chỉ cho đi đường điều chỉnh có người duyệt + lý do.")
    if code == "INV":
        alts.append("Tồn kho không đủ / lệch vị trí → chặn xuất và gợi ý chứng từ bù/kiểm kê.")
    if code == "FIN":
        alts.append("Bút toán không cân Nợ–Có hoặc sai kỳ → từ chối post, yêu cầu sửa.")
    if code == "WF":
        alts.append("Không tìm thấy bước duyệt kế tiếp / approver trống → escalation theo cấu hình hoặc báo admin.")
    if code == "POS":
        alts.append("Mất kết nối thiết bị in/CRT hoặc lệch tiền ca → cho hoàn thành offline theo policy hoặc treo chờ đồng bộ.")
    return alts


def build_post(code: str, group_name: str, ten: str, action: str) -> list[str]:
    posts = [
        f"Kết quả nghiệp vụ của «{ten}» được lưu nhất quán trong module `{code}`; có thể truy vết trên audit.",
    ]
    if action in ("create", "update", "approve", "lock", "calculate", "payment", "receive"):
        posts.append("Trạng thái/chứng từ liên quan phản ánh đúng bước vòng đời; module phụ thuộc nhận sự kiện nếu được cấu hình.")
    if action == "export":
        posts.append("File/bản in đã được tạo; có nhật ký export.")
    if action == "alert":
        posts.append("Người nhận đã có thông báo (hoặc được ghi nhận thất bại gửi để retry).")
    return posts


def build_br(code: str, group_name: str, ten: str, action: str) -> list[str]:
    g = group_name[:24]
    brs = [
        f"BR-{code}-SCOPE-01",
        f"BR-{code}-AUD-01",
    ]
    action_br = {
        "approve": f"BR-{code}-APPR-01",
        "lock": f"BR-{code}-LOCK-01",
        "calculate": f"BR-{code}-CALC-01",
        "payment": f"BR-{code}-PAY-01",
        "receive": f"BR-{code}-RCV-01",
        "import": f"BR-{code}-IMP-01",
        "cancel": f"BR-{code}-CAN-01",
    }
    if action in action_br:
        brs.append(action_br[action])
    # domain extras
    if code == "HRM":
        if any(k in ten.lower() for k in ("lương", "payroll", "payslip")):
            brs.append("BR-HRM-02")
        if "phép" in ten.lower():
            brs.append("BR-HRM-03")
        if "mã nhân" in ten.lower():
            brs.append("BR-HRM-01")
    if code == "INV" and any(k in ten.lower() for k in ("xuất", "nhập", "tồn")):
        brs.append("BR-INV-STOCK-01")
    if code == "FIN":
        brs.append("BR-FIN-BALANCE-01")
    if code == "SYS":
        brs.append("BR-SYS-02")
    # keep unique, max 4
    out: list[str] = []
    for b in brs:
        if b not in out:
            out.append(b)
    return out[:4]


def build_ac(code: str, ten: str, action: str, prio: str) -> list[str]:
    acs = [
        f"Thực hiện thành công «{ten}» với dữ liệu hợp lệ trong data scope.",
        "User không đủ quyền không hoàn tất được thao tác (UI chặn hoặc API 403).",
        "Có bản ghi Audit Trail (hoặc log tương đương) cho thay đổi/tra cứu trọng yếu.",
    ]
    if action == "approve":
        acs.append("Sau duyệt, trạng thái và bước tiếp theo khả dụng đúng quy trình.")
    if action == "calculate":
        acs.append("Kết quả tính toán tái lập được với cùng input/rule (deterministic trong cùng phiên bản rule).")
    if action == "lock":
        acs.append("Sau khóa, thao tác sửa trực tiếp bị chặn đúng policy.")
    if action == "export":
        acs.append("File xuất phản ánh đúng bộ lọc và không lộ dữ liệu ngoài scope.")
    if prio == "Must":
        acs.append("Thuộc phạm vi Phase 1 / go-live tối thiểu của module.")
    return acs[:5]


def author_uc(
    *,
    code: str,
    group_code: str,
    group_name: str,
    uc_index: int,
    ten: str,
    mota: str,
    uu_tien: str,
    default_actor: str | None = None,
) -> dict:
    prio = PRIORITY_MOSCOW.get(uu_tien, "Could")
    actor = (
        default_actor
        or MODULE_GROUP_ACTORS.get(code, {}).get(group_code)
        or "Người dùng nghiệp vụ"
    )
    # Self-service overrides
    tl = ten.lower()
    if any(k in tl for k in ("nv tự", "tự cập nhật", "self-service", "nhân viên xem", "my ")):
        if code == "HRM":
            actor = "Employee"
        if code == "PRT":
            actor = "Customer User"
    if any(k in tl for k in ("học viên", "learner", "làm bài")) and code == "LMS":
        actor = "Learner"

    action = detect_action(ten)
    detail = mota.strip() if mota else ten
    mo_ta = detail if detail.lower() != ten.lower() else f"{ten} — thao tác thuộc {group_name}."

    return uc(
        ma=f"UC_{code}_{uc_index:03d}",
        ten=ten,
        prio=prio,
        actor=actor,
        mo_ta=mo_ta,
        tien=build_pre(code, group_name, ten, action, actor),
        luong=build_flow(code, group_name, ten, mota, action, actor),
        ngoai=build_alt(code, group_name, ten, action),
        hau=build_post(code, group_name, ten, action),
        br=build_br(code, group_name, ten, action),
        ac=build_ac(code, ten, action, prio),
    )


def group_description(code: str, group_name: str, n_uc: int) -> str:
    return (
        f"Nhóm **{group_name}** gồm **{n_uc}** use case của module `{code}`. "
        f"Các UC bên dưới mô tả luồng nghiệp vụ cụ thể theo từng chức năng, "
        f"gắn RBAC/license SYS và data scope."
    )
