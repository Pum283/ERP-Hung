# -*- coding: utf-8 -*-
"""Chèn SYS-13 vào SRS_SYS_v1.1.md (catalog + mục 7.13 đặc tả UC)."""
from __future__ import annotations

from pathlib import Path

SRS = Path(__file__).resolve().parents[1] / "01. SYS - Hệ thống nền tảng" / "SRS_SYS_v1.1.md"

UCS = [
    ("095", "Tạo hội thoại 1-1", "Must", "End User",
     "Tạo hoặc mở hội thoại Direct với một user khác cùng tenant.",
     "Đã đăng nhập; có quyền sys.msg.send; đối phương thuộc cùng tenant và Active.",
     "Không tạo trùng Direct giữa cùng 2 user; ghi audit tạo hội thoại.",
     "1. Chọn user đích.\n2. Hệ thống tìm Direct hiện có hoặc tạo mới + 2 members.\n3. Mở khung chat."),
    ("096", "Tạo hội thoại nhóm", "Should", "End User",
     "Tạo hội thoại Group với tiêu đề và nhiều thành viên.",
     "Đã đăng nhập; sys.msg.send; ≥2 thành viên hợp lệ.",
     "Lưu title; members ≥2; creator là admin nhóm mặc định.",
     "1. Nhập tên nhóm + chọn members.\n2. Tạo conversation kind=Group.\n3. Thông báo thành viên qua SignalR conversationUpdated."),
    ("097", "Gửi tin nhắn realtime", "Must", "End User",
     "Gửi tin text (và tuỳ chọn đính kèm) vào hội thoại đang tham gia.",
     "Là member của hội thoại; sys.msg.send; body không rỗng (trừ khi có file).",
     "Persist chat_message; đẩy messageReceived tới members online.",
     "1. Nhập nội dung → Gửi.\n2. API lưu DB.\n3. Hub đẩy realtime; UI người gửi hiện tin ngay."),
    ("098", "Nhận tin nhắn realtime (SignalR)", "Must", "End User",
     "Nhận tin mới qua hub /hubs/msg mà không poll API.",
     "Đã kết nối SignalR với JWT; thuộc group user:{id} hoặc conv:{id}.",
     "Cấm setInterval gọi lịch sử; mất kết nối thì reconnect + sync phần thiếu.",
     "1. FE subscribe hub.\n2. Khi có messageReceived → append UI / tăng badge.\n3. Offline: khi online lại gọi REST lịch sử."),
    ("099", "Xem lịch sử hội thoại", "Must", "End User",
     "Xem tin đã lưu với phân trang (before/take).",
     "Là member; sys.msg.read.",
     "Không lộ tin ngoài hội thoại; tin đã thu hồi hiển thị trạng thái Recalled.",
     "1. Mở hội thoại.\n2. GET messages phân trang.\n3. Cuộn lên tải thêm."),
    ("100", "Đánh dấu đã đọc / badge chưa đọc", "Must", "End User",
     "Cập nhật last_read_at và badge unread trên shell.",
     "Là member; sys.msg.read.",
     "Badge tổng = tổng tin sau last_read_at của mọi hội thoại.",
     "1. Mở hội thoại → POST read.\n2. Cập nhật unread-count.\n3. SignalR conversationUpdated cho peers (tuỳ chọn)."),
    ("101", "Đính kèm file trong tin nhắn", "Should", "End User",
     "Gửi tin kèm file đã upload qua SYS file.",
     "sys.msg.send + quyền file; file thuộc tenant.",
     "Lưu attachment_file_id; người nhận tải theo quyền file.",
     "1. Upload file SYS.\n2. Gửi message kèm fileId.\n3. UI hiện preview/tên file."),
    ("102", "Thu hồi tin nhắn", "Should", "End User",
     "Người gửi thu hồi tin trong cửa sổ thời gian cấu hình.",
     "Là sender; trong TTL thu hồi; sys.msg.send.",
     "Set recalled_at; đẩy messageRecalled; body ẩn với mọi member.",
     "1. Chọn Thu hồi.\n2. Validate TTL.\n3. Cập nhật DB + broadcast."),
    ("103", "Tìm kiếm tin nhắn", "Could", "End User",
     "Tìm theo từ khóa trong các hội thoại user tham gia.",
     "sys.msg.read.",
     "Chỉ trả tin thuộc hội thoại của user; phân trang.",
     "1. Nhập từ khóa.\n2. Search full-text/like.\n3. Jump tới tin trong hội thoại."),
    ("104", "Tắt thông báo hội thoại", "Could", "End User",
     "Mute hội thoại: không tăng badge / không toast.",
     "Là member.",
     "muted=true trên conversation_member; vẫn đọc được lịch sử.",
     "1. Bật Mute.\n2. Tin mới không đẩy toast cho user.\n3. Unmute khôi phục."),
]

ALT = (
    "3.1. Người dùng nhấn nút [Hủy / Thoát]:<br>"
    "  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.<br>"
    "4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:<br>"
    "  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.<br>"
    "  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.<br>"
    "5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:<br>"
    "  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log."
)


def uc_table(n: int, code: str, title: str, prio: str, actor: str, desc: str, pre: str, req: str, main: str) -> str:
    main_html = "<br>".join(main.split("\n"))
    return f"""
**Bảng {n}. Đặc tả Use Case "{title}"**

| Trường Thông Tin | Nội Dung Đặc Tả |
| :--- | :--- |
| **Use Case ID** | UC_SYS_{code} |
| **Tên Use Case** | {title} |
| **Tác nhân** | {actor} |
| **Mô tả chức năng** | Cho phép {actor} thực hiện chức năng "{title}" thuộc nhóm Nhắn tin realtime trong module SYS — Hệ thống nền tảng. Mô tả chi tiết: {desc} |
| **Điều kiện tiên quyết** | • Hệ thống ERP đang hoạt động bình thường.<br>• Người dùng đã đăng nhập tài khoản thuộc vai trò [{actor}] và được cấp quyền RBAC tương ứng.<br>• {pre} |
| **Yêu cầu** | • Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.<br>• Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).<br>• Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.<br>• Ưu tiên triển khai (MoSCoW): **{prio}**.<br>• Quy tắc nghiệp vụ liên quan: `BR-SYS-MSG-01`.<br>• Hậu điều kiện: {req}<br>• Tiêu chí chấp nhận AC1: Thực hiện thành công «{title}» với dữ liệu hợp lệ.<br>• Tiêu chí chấp nhận AC2: User không thuộc hội thoại không đọc/gửi được (403). |
| **Kịch bản chính** | {main_html} |
| **Kịch bản phụ** | {ALT} |
"""


def main() -> None:
    text = SRS.read_text(encoding="utf-8")

    catalog_rows = "\n".join(
        f"| `UC_SYS_{c}` | Nhắn tin realtime | {t} | {p} |"
        for c, t, p, *_ in UCS
    )
    if "UC_SYS_095" not in text.split("</details>")[0]:
        text = text.replace(
            "| `UC_SYS_094` | Đa ngôn ngữ & giao diện | Trang chủ theo vai trò | Could |\n\n</details>",
            f"| `UC_SYS_094` | Đa ngôn ngữ & giao diện | Trang chủ theo vai trò | Could |\n{catalog_rows}\n\n</details>",
        )

    # Section 7.13
    if "### 7.13. Nhắn tin realtime" not in text:
        body = [
            "",
            "### 7.13. Nhắn tin realtime (`SYS-13`)",
            "",
            "> Chat nội bộ user↔user (SignalR). Khác **SYS-07 Thông báo** (hệ thống→user). Spec kỹ thuật: `Source/docs/04_MSG_REALTIME.md`.",
            "",
            "| Chỉ số | Giá trị |",
            "|---|---|",
            "| Số UC | 10 |",
            "| Must | 5 |",
            "| Should | 3 |",
            "| Could | 2 |",
            "",
            "**BR-SYS-MSG-01:** Mọi tin/hội thoại thuộc tenant; chỉ member được đọc/gửi; realtime qua `/hubs/msg` — cấm poll.",
            "",
        ]
        for i, (code, title, prio, actor, desc, pre, req, main_s) in enumerate(UCS):
            body.append(uc_table(95 + i, code, title, prio, actor, desc, pre, req, main_s).rstrip())
            body.append("")
        section = "\n".join(body) + "\n---\n"
        text = text.replace(
            "\n---\n\n## 8. Workflow end-to-end",
            "\n" + section + "\n## 8. Workflow end-to-end",
        )

    text = text.replace(
        "| UC IDs | `UC_SYS_001` … `UC_SYS_094` |",
        "| UC IDs | `UC_SYS_001` … `UC_SYS_104` |",
    )
    text = text.replace(
        "| End User mặc định | Người dùng nghiệp vụ | self-service password/notify/file theo module |",
        "| End User mặc định | Người dùng nghiệp vụ | self-service password/notify/file/`sys.msg.*` |",
    )

    # Phase note
    if "Nhắn tin realtime Must" not in text:
        text = text.replace(
            "| Phase 1 — Go-live nền | Toàn bộ **Must** |",
            "| Phase 1 — Go-live nền | Toàn bộ **Must** (gồm SYS-13 nhắn tin realtime Must) |",
        )

    # Acceptance
    if "Nhắn tin realtime A→B" not in text:
        text = text.replace(
            "8. Không còn đặc tả UC dùng luồng khuôn mẫu sai (đăng xuất ≠ dashboard…).",
            "8. Không còn đặc tả UC dùng luồng khuôn mẫu sai (đăng xuất ≠ dashboard…).\n"
            "9. Nhắn tin realtime A→B nhận không F5; unread badge đúng; user ngoài hội thoại 403.",
        )

    # Compact phụ lục B pointer
    text = text.replace(
        "## Phụ lục B — SYS-13 Nhắn tin realtime (bổ sung 04/08/2026)",
        "## Phụ lục B — SYS-13 Nhắn tin realtime (xem mục 7.13)",
    )

    SRS.write_text(text, encoding="utf-8")
    print(f"Patched {SRS}")


if __name__ == "__main__":
    main()
