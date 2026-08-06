#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Viết lại SRS SYS v1.1 — đặc tả thủ công, không dùng luồng khuôn mẫu."""
from __future__ import annotations

from pathlib import Path

OUT = (
    Path(__file__).resolve().parents[1]
    / "01. SYS - Hệ thống nền tảng"
    / "SRS_SYS_v1.1.md"
)

# (mã, tên, ưu tiên, actor, mô tả, tiền[], luồng[], ngoại lệ[], hậu[], br[], ac[])
# Ưu tiên: Must|Should|Could|Later

def uc(ma, ten, prio, actor, mo_ta, tien, luong, ngoai, hau, br, ac):
    return {
        "ma": ma, "ten": ten, "prio": prio, "actor": actor, "mo_ta": mo_ta,
        "tien": tien, "luong": luong, "ngoai": ngoai, "hau": hau, "br": br, "ac": ac,
    }


COMMON_AUTH_PRE = [
    "Tenant đang hoạt động (chưa bị đình chỉ).",
    "User biết định danh đăng nhập (username/email/SĐT) đã được cấp.",
]

GROUPS = []

# ===== SYS-01 =====
g1 = []
g1.append(uc(
    "UC_SYS_001", "Đăng nhập hệ thống", "Must", "End User",
    "Cho phép người dùng xác thực bằng username/email/SĐT + mật khẩu để nhận phiên làm việc.",
    COMMON_AUTH_PRE + ["Tài khoản ở trạng thái Active (không bị khóa/xóa mềm)."],
    [
        "Người dùng mở màn hình đăng nhập của tenant.",
        "Nhập định danh + mật khẩu (có tùy chọn hiện/ẩn mật khẩu).",
        "Hệ thống kiểm tra: tồn tại user, trạng thái, mật khẩu (hash), số lần sai, chính sách hết hạn mật khẩu.",
        "Nếu bật 2FA và thiết bị chưa tin cậy → chuyển bước xác thực 2FA.",
        "Cấp access token / refresh token (hoặc session tương đương); ghi LoginLog thành công.",
        "Điều hướng trang chủ / landing theo role; tải menu theo license + permission.",
    ],
    [
        "Sai mật khẩu → tăng bộ đếm sai; thông báo chung (không tiết lộ user có tồn tại hay không — theo policy).",
        "Vượt ngưỡng sai → khóa tạm tài khoản (UC_SYS_007).",
        "User Locked/Disabled → từ chối kèm lý do phù hợp.",
        "Tenant hết hạn license nền → chặn đăng nhập admin vận hành theo policy (trừ kênh gia hạn).",
    ],
    [
        "Phiên hợp lệ được tạo; người dùng vào được hệ thống đúng quyền.",
        "LoginLog ghi nhận IP, user-agent, thời điểm, kết quả.",
    ],
    ["BR-SYS-AUTH-01", "BR-SYS-AUTH-02", "BR-SYS-SEC-01"],
    [
        "Đăng nhập đúng credential → vào hệ thống < 3 giây trong điều kiện chuẩn.",
        "Sai credential không cấp phiên.",
        "Menu chỉ gồm module đang license và permission được gán.",
    ],
))
g1.append(uc(
    "UC_SYS_002", "Đăng xuất", "Must", "End User",
    "Thu hồi phiên hiện tại (và tùy chọn tất cả phiên) khi người dùng đăng xuất.",
    ["Người dùng đang có phiên hợp lệ."],
    [
        "Người dùng chọn Đăng xuất.",
        "Hệ thống thu hồi access/refresh token (hoặc hủy server session) của phiên hiện tại.",
        "Xóa cookie/local session phía client.",
        "Ghi LoginLog/Audit sự kiện logout.",
        "Chuyển về màn hình đăng nhập.",
    ],
    ["Token đã hết hạn vẫn đưa về màn hình đăng nhập an toàn (idempotent)."],
    ["Phiên hiện tại không còn dùng được để gọi API."],
    ["BR-SYS-AUTH-03"],
    ["Sau đăng xuất, gọi API với token cũ trả 401.", "Không còn truy cập được màn hình nội bộ."],
))
g1.append(uc(
    "UC_SYS_003", "Đổi mật khẩu", "Must", "End User",
    "Người dùng đã đăng nhập đổi mật khẩu bằng cách xác nhận mật khẩu cũ và đặt mật khẩu mới theo policy.",
    ["Đã đăng nhập.", "Tài khoản cho phép login bằng mật khẩu (không purely SSO-only — trừ khi policy cho phép set local password)."],
    [
        "Mở form Đổi mật khẩu.",
        "Nhập mật khẩu cũ, mật khẩu mới, xác nhận mật khẩu mới.",
        "Hệ thống kiểm tra mật khẩu cũ đúng; mật khẩu mới đạt policy và không trùng N mật khẩu gần nhất.",
        "Lưu hash mật khẩu mới; vô hiệu hóa các phiên khác (khuyến nghị); ghi audit.",
        "Thông báo thành công; yêu cầu đăng nhập lại nếu policy bắt buộc.",
    ],
    ["Mật khẩu cũ sai → từ chối.", "Mật khẩu mới không đạt policy → liệt kê rule vi phạm."],
    ["Mật khẩu mới có hiệu lực; mật khẩu cũ không còn dùng được."],
    ["BR-SYS-AUTH-04", "BR-SYS-AUTH-05"],
    ["Đổi thành công với mật khẩu hợp lệ.", "Mật khẩu yếu bị từ chối theo cấu hình tenant."],
))
g1.append(uc(
    "UC_SYS_004", "Quên mật khẩu – gửi OTP/link", "Must", "End User",
    "Khởi tạo quy trình đặt lại mật khẩu khi người dùng quên mật khẩu.",
    ["User nhớ định danh đăng nhập hoặc email/SĐT đã gắn.", "Kênh email/SMS đã cấu hình (ít nhất 1 kênh)."],
    [
        "Người dùng chọn Quên mật khẩu, nhập định danh/email/SĐT.",
        "Hệ thống tra cứu user (phản hồi trung tính nếu không tìm thấy — chống user enumeration theo policy).",
        "Tạo token/OTP có thời hạn; lưu trạng thái Pending reset.",
        "Gửi email link hoặc SMS OTP qua gateway.",
        "Ghi NotificationLog + security log.",
    ],
    ["Gateway lỗi → báo thử lại; không để token dùng được nếu gửi thất bại (theo thiết kế)."],
    ["Có mã/OTP hiệu lực trong thời hạn cấu hình (ví dụ 15 phút)."],
    ["BR-SYS-AUTH-06"],
    ["User hợp lệ nhận được OTP/link.", "OTP hết hạn không dùng được."],
))
g1.append(uc(
    "UC_SYS_005", "Đặt lại mật khẩu sau OTP", "Must", "End User",
    "Hoàn tất đặt lại mật khẩu sau khi xác thực OTP/link hợp lệ.",
    ["Có token/OTP còn hiệu lực từ UC_SYS_004."],
    [
        "Người dùng mở link hoặc nhập OTP.",
        "Hệ thống xác thực token/OTP chưa dùng, chưa hết hạn.",
        "Nhập mật khẩu mới + xác nhận theo policy.",
        "Cập nhật mật khẩu; đánh dấu token đã dùng; thu hồi mọi phiên cũ.",
        "Ghi audit; chuyển đăng nhập.",
    ],
    ["OTP sai/hết hạn/đã dùng → từ chối.", "Mật khẩu mới không đạt policy → từ chối."],
    ["Đăng nhập được bằng mật khẩu mới; phiên cũ hết hiệu lực."],
    ["BR-SYS-AUTH-04", "BR-SYS-AUTH-06"],
    ["Reset thành công với OTP đúng.", "Tái sử dụng OTP cũ thất bại."],
))
g1.append(uc(
    "UC_SYS_006", "Chính sách độ mạnh mật khẩu", "Must", "Security Admin",
    "Cấu hình và áp dụng rule độ mạnh mật khẩu cho tenant (độ dài, phức tạp, lịch sử, tuổi thọ).",
    ["Người dùng có quyền sys.setting.manage hoặc quyền bảo mật tương đương."],
    [
        "Mở cấu hình Password Policy.",
        "Thiết lập: min length, chữ hoa/thường/số/ký tự đặc biệt, số mật khẩu không được tái sử dụng, số ngày hết hạn (optional), thông báo trước hết hạn.",
        "Lưu cấu hình; có hiệu lực với lần đổi/reset mật khẩu tiếp theo (và lần đăng nhập nếu buộc đổi).",
        "Ghi audit thay đổi cấu hình.",
    ],
    ["Giá trị ngoài khoảng cho phép → validate lỗi."],
    ["Policy được áp dụng nhất quán cho UC đổi/reset/tạo mật khẩu."],
    ["BR-SYS-AUTH-04"],
    ["Đổi mật khẩu vi phạm policy bị chặn.", "Admin xem được policy hiện hành."],
))
g1.append(uc(
    "UC_SYS_007", "Khóa tài khoản sau N lần sai", "Must", "Hệ thống / Security Admin",
    "Tự động khóa tạm tài khoản khi vượt số lần đăng nhập sai; cho phép admin mở khóa.",
    ["Password policy có cấu hình ngưỡng N và thời gian khóa."],
    [
        "Mỗi lần đăng nhập sai tăng fail counter.",
        "Khi đạt N: đặt trạng thái LockedTemporarily + thời điểm hết khóa (hoặc chờ admin).",
        "Thông báo phù hợp cho user; ghi security log.",
        "Admin có thể mở khóa thủ công; hoặc hệ thống tự mở sau thời gian cấu hình.",
    ],
    ["Đăng nhập đúng trong lúc bị khóa → vẫn từ chối đến khi hết khóa/admin mở."],
    ["Tài khoản không đăng nhập được trong thời gian khóa."],
    ["BR-SYS-AUTH-02"],
    ["Sai N lần liên tiếp → bị khóa.", "Admin unlock thành công."],
))
g1.append(uc(
    "UC_SYS_008", "Xác thực 2 bước (2FA)", "Should", "End User / Security Admin",
    "Bật và xác thực lớp thứ hai bằng TOTP authenticator và/hoặc OTP SMS.",
    ["User đã vượt lớp mật khẩu.", "Admin đã cho phép 2FA (optional/bắt buộc theo role)."],
    [
        "User bật 2FA: quét QR TOTP hoặc đăng ký SĐT nhận OTP.",
        "Xác nhận mã lần đầu để kích hoạt.",
        "Các lần đăng nhập sau: sau mật khẩu đúng → nhập mã 2FA.",
        "Admin có thể reset 2FA khi mất thiết bị (có audit).",
    ],
    ["Mã 2FA sai quá số lần → tạm khóa bước 2FA/phiên đăng nhập.", "Role bắt buộc 2FA mà chưa bật → buộc setup trước khi vào hệ thống."],
    ["Chỉ cấp phiên đầy đủ khi 2FA thành công (nếu bật/bắt buộc)."],
    ["BR-SYS-AUTH-07"],
    ["Bật TOTP và đăng nhập thành công với mã đúng.", "Mã sai không vào được hệ thống."],
))
g1.append(uc(
    "UC_SYS_009", "Đăng nhập SSO / OAuth", "Could", "End User / System Admin",
    "Cho phép đăng nhập qua nhà cung cấp OIDC/OAuth (Google, Microsoft…).",
    ["Admin đã cấu hình IdP (client id/secret/redirect/issuer).", "User có email map được với tài khoản nội bộ hoặc policy JIT provisioning được bật."],
    [
        "User chọn Đăng nhập bằng IdP.",
        "Redirect đến IdP; xác thực bên ngoài.",
        "Callback về hệ thống với authorization code/token.",
        "Map hoặc tạo user theo policy; cấp phiên nội bộ; ghi audit.",
    ],
    ["Email không map và JIT tắt → từ chối.", "IdP lỗi/timeout → thông báo rõ."],
    ["User vào hệ thống với quyền của tài khoản đã map."],
    ["BR-SYS-AUTH-08"],
    ["SSO thành công với IdP cấu hình đúng.", "User lạ bị từ chối khi JIT tắt."],
))
g1.append(uc(
    "UC_SYS_010", "Quản lý phiên đang hoạt động", "Should", "End User / Security Admin",
    "Xem danh sách phiên/thiết bị đang hoạt động và thu hồi từng phiên.",
    ["Đã đăng nhập."],
    [
        "Mở mục Phiên đăng nhập / Thiết bị.",
        "Hiển thị danh sách: thiết bị, IP, thời điểm tạo/hoạt động gần nhất, phiên hiện tại.",
        "User thu hồi 1 phiên hoặc tất cả phiên khác.",
        "Admin (có quyền) có thể thu hồi phiên của user khác.",
        "Ghi audit.",
    ],
    ["Thu hồi phiên hiện tại = đăng xuất."],
    ["Phiên bị thu hồi không gọi API được nữa."],
    ["BR-SYS-AUTH-03"],
    ["User thấy phiên hiện tại.", "Thu hồi phiên khác làm token đó hết hiệu lực."],
))
g1.append(uc(
    "UC_SYS_011", "Giới hạn số phiên đồng thời", "Should", "Security Admin",
    "Giới hạn số phiên đồng thời trên mỗi user để giảm chia sẻ tài khoản.",
    ["Admin có quyền cấu hình bảo mật."],
    [
        "Cấu hình max concurrent sessions (theo tenant hoặc theo role).",
        "Khi đăng nhập mới vượt ngưỡng: từ chối hoặc đá phiên cũ nhất (theo policy chọn).",
        "Thông báo cho user về phiên bị thay thế (nếu có).",
    ],
    [],
    ["Số phiên active không vượt ngưỡng cấu hình."],
    ["BR-SYS-AUTH-09"],
    ["Đăng nhập vượt max → áp dụng đúng policy (reject hoặc revoke oldest)."],
))
g1.append(uc(
    "UC_SYS_012", "Ghi nhớ thiết bị tin cậy", "Later", "End User",
    "Cho phép bỏ qua 2FA trong thời hạn trên thiết bị đã đánh dấu tin cậy.",
    ["2FA đã bật.", "Policy cho phép trusted device."],
    [
        "Sau 2FA thành công, user chọn Tin cậy thiết bị này.",
        "Hệ thống lưu device token gắn user + hạn dùng.",
        "Lần sau: mật khẩu đúng + device token hợp lệ → bỏ qua 2FA.",
    ],
    ["Đổi mật khẩu / reset 2FA / admin revoke → xóa trusted devices."],
    ["Thiết bị tin cậy còn hạn không bị hỏi 2FA."],
    ["BR-SYS-AUTH-07"],
    ["Trusted device bỏ qua 2FA trong hạn.", "Hết hạn phải 2FA lại."],
))
GROUPS.append(("01", "Xác thực & phiên làm việc", "Nhóm nền tảng an toàn truy cập: đăng nhập, phiên, mật khẩu, 2FA/SSO.", g1))

# ===== SYS-02 =====
g2 = []
g2.append(uc("UC_SYS_013", "Tạo người dùng", "Must", "System Admin",
    "Tạo tài khoản người dùng mới trong tenant, gắn thông tin cơ bản và trạng thái ban đầu.",
    ["Có quyền sys.user.manage.", "Chưa vượt quota user của license (nếu áp dụng)."],
    ["Mở form tạo user.", "Nhập username/email/SĐT, họ tên, chi nhánh mặc định, trạng thái.", "Validate trùng định danh.", "Lưu user (Active/InvitePending).", "Tùy chọn gửi invite/reset password.", "Ghi audit."],
    ["Trùng email/username → lỗi.", "Vượt quota → chặn kèm thông báo nâng gói."],
    ["User tồn tại và có thể được gán role."],
    ["BR-SYS-USER-01", "BR-SYS-LIC-02"],
    ["Tạo user thành công với email duy nhất.", "User trùng bị từ chối."]))
g2.append(uc("UC_SYS_014", "Cập nhật thông tin người dùng", "Must", "System Admin / End User",
    "Cập nhật hồ sơ hiển thị của user (họ tên, SĐT, avatar…). End User chỉ sửa trường self-service cho phép.",
    ["User tồn tại.", "Có quyền quản trị hoặc đang sửa chính mình với phạm vi field cho phép."],
    ["Mở hồ sơ user.", "Sửa các trường được phép.", "Validate định dạng.", "Lưu + audit field change."],
    ["Sửa field nhạy cảm không đủ quyền → 403."],
    ["Thông tin mới hiển thị đúng ở UI/API."],
    ["BR-SYS-USER-02"],
    ["Admin cập nhật SĐT thành công.", "User thường không sửa được username hệ thống nếu policy cấm."]))
g2.append(uc("UC_SYS_015", "Khóa / mở khóa người dùng", "Must", "System Admin",
    "Vô hiệu hóa hoặc kích hoạt lại khả năng đăng nhập của user mà không xóa dữ liệu.",
    ["Có quyền sys.user.manage.", "Không phải tự khóa tài khoản admin cuối cùng theo rule an toàn."],
    ["Chọn user → Khóa hoặc Mở khóa.", "Xác nhận.", "Cập nhật trạng thái; thu hồi phiên nếu khóa.", "Audit."],
    ["Không cho khóa hết toàn bộ System Admin của tenant."],
    ["User bị khóa không đăng nhập được; mở khóa thì đăng nhập lại được."],
    ["BR-SYS-USER-03"],
    ["Khóa user → login 403/denied.", "Mở khóa → login được."]))
g2.append(uc("UC_SYS_016", "Xóa mềm người dùng", "Must", "System Admin",
    "Ngưng sử dụng user bằng soft-delete; giữ lịch sử giao dịch và audit.",
    ["User không còn cần truy cập.", "Có quyền sys.user.manage."],
    ["Chọn Xóa/Ngưng dùng.", "Hệ thống chuyển Deleted/Inactive; giải phóng username theo policy (hoặc giữ).", "Thu hồi phiên + API key của user.", "Audit."],
    ["Không xóa cứng nếu đã phát sinh chứng từ — chỉ soft-delete."],
    ["User không còn trong danh sách active; dữ liệu lịch sử vẫn truy vết được."],
    ["BR-SYS-USER-01"],
    ["Soft-delete thành công.", "Không còn đăng nhập được."]))
g2.append(uc("UC_SYS_017", "Gán người dùng vào chi nhánh", "Must", "System Admin / Org Admin",
    "Gán chi nhánh mặc định và danh sách chi nhánh được truy cập (nền tảng cho data scope).",
    ["Đã có master chi nhánh.", "Có quyền gán org cho user."],
    ["Chọn user → tab Tổ chức.", "Chọn chi nhánh mặc định + các chi nhánh bổ sung.", "Lưu; tính lại data scope hiệu lực.", "Audit."],
    ["Chi nhánh ngưng dùng không gán mới được."],
    ["Các module nghiệp vụ lọc dữ liệu theo scope chi nhánh của user."],
    ["BR-SYS-SCOPE-01"],
    ["User chỉ thấy dữ liệu chi nhánh được gán trong kịch bản kiểm thử mẫu."]))
g2.append(uc("UC_SYS_018", "Reset mật khẩu bởi Admin", "Must", "System Admin",
    "Admin đặt lại mật khẩu hoặc gửi link reset cho user khi hỗ trợ.",
    ["Có quyền sys.user.manage.", "User tồn tại."],
    ["Chọn Reset mật khẩu.", "Chọn: đặt mật khẩu tạm hoặc gửi link.", "Nếu mật khẩu tạm: bắt buộc đổi ở lần đăng nhập tiếp theo.", "Audit bắt buộc (ai reset ai)."],
    [],
    ["User đăng nhập được bằng mật khẩu/link mới."],
    ["BR-SYS-AUTH-05", "BR-SYS-AUD-01"],
    ["Có bản ghi audit reset.", "User phải đổi mật khẩu tạm trước khi dùng bình thường."]))
g2.append(uc("UC_SYS_019", "Mời người dùng qua email", "Should", "System Admin",
    "Gửi lời mời kích hoạt tài khoản qua email có link hết hạn.",
    ["Email gateway đã cấu hình.", "User ở trạng thái InvitePending hoặc mới tạo."],
    ["Tạo/chọn user → Gửi lời mời.", "Sinh invite token có hạn.", "Gửi email.", "User bấm link → đặt mật khẩu → Active."],
    ["Email bounce → trạng thái gửi thất bại trên log."],
    ["User kích hoạt thành công và đăng nhập được."],
    ["BR-SYS-USER-04"],
    ["Link invite hết hạn không kích hoạt được.", "Invite hợp lệ kích hoạt được."]))
g2.append(uc("UC_SYS_020", "Import danh sách người dùng Excel", "Should", "System Admin",
    "Tạo hàng loạt user từ file Excel/CSV theo mẫu chuẩn.",
    ["Có quyền import.", "Trong hạn quota."],
    ["Tải mẫu → điền → upload.", "Validate từng dòng; hiện preview lỗi.", "Xác nhận import các dòng hợp lệ (hoặc all-or-nothing theo cấu hình).", "Sinh báo cáo kết quả; audit."],
    ["Dòng lỗi không được ghi đè dữ liệu sai."],
    ["Các user hợp lệ được tạo."],
    ["BR-SYS-USER-01", "BR-SYS-IE-01"],
    ["Import 10 user hợp lệ thành công.", "Dòng trùng email bị báo lỗi rõ số dòng."]))
g2.append(uc("UC_SYS_021", "Tìm kiếm / lọc người dùng", "Must", "System Admin",
    "Tìm user theo tên, email, SĐT, role, chi nhánh, trạng thái.",
    ["Có quyền xem danh sách user."],
    ["Nhập từ khóa/bộ lọc → kết quả phân trang.", "Click mở chi tiết."],
    [],
    ["Kết quả đúng filter và trong data scope quản trị."],
    ["BR-SYS-SCOPE-01"],
    ["Lọc theo chi nhánh A không trả user chỉ thuộc chi nhánh B."]))
g2.append(uc("UC_SYS_022", "Xuất danh sách người dùng", "Should", "System Admin",
    "Xuất danh sách user theo bộ lọc hiện tại ra Excel.",
    ["Có quyền export user."],
    ["Áp dụng filter → Export Excel.", "Ghi audit export (ai xuất, số dòng)."],
    [],
    ["File tải về đủ cột cấu hình, đúng dữ liệu lọc."],
    ["BR-SYS-AUD-01"],
    ["Export thành công file mở được bằng Excel."]))
GROUPS.append(("02", "Người dùng", "Quản trị vòng đời tài khoản người dùng trong tenant.", g2))

# ===== SYS-03 =====
g3 = []
g3.append(uc("UC_SYS_023", "Tạo / sửa / ngưng vai trò (Role)", "Must", "Security Admin",
    "Quản lý danh mục role nghiệp vụ (Admin, Kế toán, Sales…).",
    ["Có quyền sys.role.manage."],
    ["Tạo role với mã, tên, mô tả, trạng thái.", "Sửa thông tin.", "Ngưng role (không xóa nếu đang được gán — hoặc chặn ngưng khi đang gán)."],
    ["Trùng mã role → lỗi.", "Ngưng role đang gán → cảnh báo/chặn theo policy."],
    ["Role sẵn sàng để gán permission và user."],
    ["BR-SYS-RBAC-01"],
    ["Tạo role mới thành công.", "Không tạo trùng mã."]))
g3.append(uc("UC_SYS_024", "Sao chép vai trò", "Should", "Security Admin",
    "Nhân bản role kèm ma trận permission để tạo biến thể nhanh.",
    ["Role nguồn tồn tại."],
    ["Chọn role → Sao chép → nhập mã/tên mới.", "Copy permission (và tùy chọn data scope mặc định).", "Lưu role mới."],
    [],
    ["Role mới có cùng permission như nguồn tại thời điểm copy."],
    ["BR-SYS-RBAC-01"],
    ["Copy role tạo bản ghi mới; sửa bản mới không ảnh hưởng bản cũ."]))
g3.append(uc("UC_SYS_025", "Quản lý danh mục quyền (Permission)", "Must", "Security Admin / Hệ thống",
    "Danh mục permission kỹ thuật do module đăng ký (sys.user.manage, crm.order.view…). Cho phép xem/nhóm hóa; hạn chế sửa mã hệ thống.",
    ["Module đã đăng ký permission catalog khi cài/bật."],
    ["Xem danh sách permission theo module.", "Bật/ẩn nhóm hiển thị trên UI gán quyền.", "Không cho xóa permission hệ thống đang được tham chiếu."],
    [],
    ["Catalog phản ánh đúng module đang license."],
    ["BR-SYS-RBAC-02"],
    ["Permission của module tắt license không hiện để gán mới (hoặc hiện nhưng đánh dấu inactive)."]))
g3.append(uc("UC_SYS_026", "Gán quyền vào vai trò", "Must", "Security Admin",
    "Thiết lập ma trận Role–Permission.",
    ["Role tồn tại.", "Có quyền sys.permission.assign."],
    ["Mở role → tick/untick permission theo nhóm module.", "Lưu; invalidate cache quyền.", "Audit before/after."],
    [],
    ["User mang role nhận đúng permission hiệu lực ngay phiên mới hoặc sau refresh quyền."],
    ["BR-SYS-RBAC-03", "BR-SYS-AUD-01"],
    ["Gán crm.order.view → user có role đó gọi API xem đơn được.", "Bỏ quyền → 403."]))
g3.append(uc("UC_SYS_027", "Gán người dùng vào vai trò", "Must", "Security Admin",
    "Gán một user vào một hoặc nhiều role.",
    ["User và role Active."],
    ["Chọn user → thêm/gỡ role.", "Tính quyền hiệu lực = hợp các role (union) theo policy tenant.", "Audit."],
    [],
    ["Quyền hiệu lực của user được cập nhật."],
    ["BR-SYS-RBAC-04"],
    ["User 2 role nhận đủ permission của cả hai."]))
g3.append(uc("UC_SYS_028", "Phân quyền dữ liệu theo chi nhánh", "Must", "Security Admin",
    "Giới hạn dữ liệu nghiệp vụ theo danh sách chi nhánh user được phép.",
    ["Đã có cây chi nhánh.", "User đã gán org."],
    ["Cấu hình scope All / Assigned branches / Single branch.", "Lưu vào hồ sơ phân quyền dữ liệu của user/role.", "Module nghiệp vụ bắt buộc enforce scope này."],
    [],
    ["Truy vấn dữ liệu ngoài chi nhánh bị loại/403."],
    ["BR-SYS-SCOPE-01"],
    ["User chi nhánh A không đọc được chứng từ chi nhánh B."]))
g3.append(uc("UC_SYS_029", "Phân quyền dữ liệu theo kho / điểm", "Must", "Security Admin",
    "Thu hẹp data scope theo kho hoặc điểm bán khi module INV/POS yêu cầu.",
    ["Master kho/điểm đã có (từ module tương ứng hoặc SYS location)."],
    ["Gán danh sách kho/điểm cho user/role.", "Enforce ở API nghiệp vụ liên quan tồn/quầy."],
    [],
    ["User chỉ thao tác kho được gán."],
    ["BR-SYS-SCOPE-02"],
    ["Xuất kho khác scope bị chặn."]))
g3.append(uc("UC_SYS_030", "Phân quyền theo phòng ban", "Should", "Security Admin",
    "Data scope theo phòng ban cho các module dùng chiều tổ chức này (HRM, WF…).",
    ["Có master phòng ban."],
    ["Gán phòng ban được truy cập.", "Module liên quan lọc theo dept scope."],
    [],
    ["Dữ liệu ngoài phòng ban không hiển thị."],
    ["BR-SYS-SCOPE-03"],
    ["Manager phòng X không thấy hồ sơ phòng Y (kịch bản HRM)."]))
g3.append(uc("UC_SYS_031", "Quyền theo trường nhạy cảm", "Should", "Security Admin",
    "Ẩn/mask hoặc cấm sửa các field nhạy cảm (ví dụ lương, CCCD, giá vốn) theo permission field-level.",
    ["Module đăng ký sensitive fields."],
    ["Cấu hình field permission theo role.", "UI mask/ẩn; API không trả plain value nếu không có quyền."],
    [],
    ["User thiếu quyền không đọc được plain sensitive data."],
    ["BR-SYS-RBAC-05"],
    ["Role không có quyền lương không thấy số lương trên API/UI."]))
g3.append(uc("UC_SYS_032", "Xem ma trận phân quyền tổng hợp", "Should", "Security Admin",
    "Báo cáo ma trận Role×Permission hoặc User×Permission hiệu lực để kiểm toán.",
    ["Có quyền xem báo cáo phân quyền."],
    ["Chọn role hoặc user → xem permission hiệu lực + nguồn role.", "Export Excel."],
    [],
    ["Báo cáo khớp cấu hình thực tế."],
    ["BR-SYS-RBAC-03"],
    ["Ma trận phản ánh đúng tick permission."]))
g3.append(uc("UC_SYS_033", "Nhật ký thay đổi phân quyền", "Must", "Security Admin",
    "Lưu và xem lịch sử thay đổi role/permission/scope.",
    ["Có quyền sys.audit.view."],
    ["Mọi thay đổi UC_SYS_023–031 ghi audit.", "Màn hình lọc theo thời gian/user/đối tượng.", "Xem before/after."],
    [],
    ["Truy vết được ai đổi quyền lúc nào."],
    ["BR-SYS-AUD-01"],
    ["Sau khi gán quyền có dòng audit tương ứng."]))
GROUPS.append(("03", "Vai trò & phân quyền", "RBAC + data scope + field-level security — trái tim kiểm soát truy cập ERP.", g3))

# ===== SYS-04 =====
g4 = []
g4.append(uc("UC_SYS_034", "Quản lý công ty / tenant", "Must", "System Admin",
    "Quản lý thông tin tenant/công ty: tên, MST, địa chỉ, logo, trạng thái thuê bao.",
    ["Có quyền sys.org.manage."],
    ["Xem/sửa hồ sơ công ty.", "Upload logo.", "Lưu; các chỗ branding đọc lại thông tin này."],
    [],
    ["Thông tin công ty nhất quán trên chứng từ/header."],
    ["BR-SYS-ORG-01"],
    ["Cập nhật tên công ty phản ánh trên UI."]))
g4.append(uc("UC_SYS_035", "Quản lý pháp nhân / công ty con", "Should", "System Admin",
    "Hỗ trợ multi-company trong một tenant: nhiều pháp nhân hạch toán/vận hành.",
    ["Gói license cho phép multi-company."],
    ["Tạo pháp nhân con với mã, MST, trạng thái.", "Gán chi nhánh thuộc pháp nhân.", "User được chỉ định legal entity scope nếu cần."],
    [],
    ["Chứng từ có thể gắn legal entity (khi module FIN bật)."],
    ["BR-SYS-ORG-02"],
    ["Tạo được ≥2 pháp nhân và gán chi nhánh."]))
g4.append(uc("UC_SYS_036", "Quản lý chi nhánh", "Must", "Org Admin",
    "CRUD cây chi nhánh: mã, tên, địa chỉ, quản lý, trạng thái.",
    ["Có quyền sys.org.manage."],
    ["Thêm/sửa/ngưng chi nhánh.", "Thiết lập quan hệ cha–con nếu có.", "Không xóa cứng khi đã phát sinh dữ liệu."],
    ["Ngưng chi nhánh đang là mặc định của user → cảnh báo."],
    ["Chi nhánh dùng được cho data scope và master module khác."],
    ["BR-SYS-ORG-01"],
    ["Tạo chi nhánh mới thành công.", "Ngưng chi nhánh ẩn khỏi chọn mặc định mới."]))
g4.append(uc("UC_SYS_037", "Quản lý điểm bán / cửa hàng", "Must", "Org Admin",
    "Danh mục điểm bán thuộc chi nhánh (phục vụ POS/CRM/HRM).",
    ["Chi nhánh tồn tại."],
    ["CRUD điểm bán: mã, tên, chi nhánh, địa chỉ, trạng thái.", "Gắn timezone riêng nếu cần."],
    [],
    ["Điểm bán xuất hiện cho các module được license."],
    ["BR-SYS-ORG-01"],
    ["Điểm bán thuộc đúng chi nhánh."]))
g4.append(uc("UC_SYS_038", "Quản lý phòng ban", "Must", "Org Admin",
    "Danh mục phòng ban dùng chung (HRM/WF…).",
    ["Có quyền quản lý org."],
    ["CRUD phòng ban; gắn chi nhánh hoặc cấp công ty.", "Ngưng dùng khi không còn hiệu lực."],
    [],
    ["Master phòng ban sẵn sàng cho module nghiệp vụ."],
    ["BR-SYS-ORG-01"],
    ["Tạo phòng ban và chọn được khi gán user/NV."]))
g4.append(uc("UC_SYS_039", "Quản lý chức danh", "Must", "Org Admin",
    "Danh mục chức danh/job title dùng chung.",
    ["Có quyền quản lý org."],
    ["CRUD chức danh; mã + tên + trạng thái."],
    [],
    ["Chức danh dùng cho HRM và hiển thị user."],
    ["BR-SYS-ORG-01"],
    ["CRUD chức danh thành công."]))
g4.append(uc("UC_SYS_040", "Sơ đồ tổ chức trực quan", "Should", "Org Admin",
    "Hiển thị cây tổ chức (công ty–chi nhánh–phòng ban) dạng sơ đồ.",
    ["Đã có dữ liệu org."],
    ["Mở Org Chart.", "Xem/zoom/expand.", "Click node mở chi tiết."],
    [],
    ["Sơ đồ phản ánh đúng master."],
    [],
    ["Cây hiển thị đúng quan hệ cha–con đã cấu hình."]))
g4.append(uc("UC_SYS_041", "Cấu hình múi giờ / ngôn ngữ / tiền tệ", "Must", "System Admin",
    "Thiết lập locale mặc định của tenant: timezone, ngôn ngữ, tiền tệ gốc.",
    ["Có quyền sys.setting.manage."],
    ["Chọn timezone, default language, base currency.", "Lưu; áp dụng cho hiển thị ngày giờ và chứng từ mặc định."],
    [],
    ["Hệ thống format thời gian/tiền theo cấu hình."],
    ["BR-SYS-CFG-01"],
    ["Đổi timezone phản ánh trên timestamp UI."]))
g4.append(uc("UC_SYS_042", "Cấu hình định dạng ngày số", "Should", "System Admin",
    "Cấu hình format ngày (dd/MM/yyyy…) và dấu phân tách số.",
    ["Có quyền cấu hình."],
    ["Chọn format → lưu → áp dụng UI/export."],
    [],
    ["Ngày/số hiển thị nhất quán."],
    ["BR-SYS-CFG-01"],
    ["Export Excel dùng đúng format cấu hình."]))
g4.append(uc("UC_SYS_043", "Quản lý địa chỉ / tỉnh thành", "Must", "System Admin",
    "Danh mục quốc gia/tỉnh-thành/quận-huyện/phường-xã (hoặc mức tương đương) dùng chung form địa chỉ.",
    ["Có quyền quản lý danh mục dùng chung."],
    ["Import hoặc CRUD địa giới.", "Form địa chỉ dùng cascade select."],
    [],
    ["Các module dùng chung master địa chỉ."],
    ["BR-SYS-CFG-02"],
    ["Chọn tỉnh lọc đúng danh sách quận."]))
GROUPS.append(("04", "Tổ chức & đa chi nhánh", "Master tổ chức dùng chung toàn ERP và nền cho data scope.", g4))

# ===== SYS-05 =====
g5 = []
g5.append(uc("UC_SYS_044", "Khai báo module trong hệ thống", "Must", "System Admin / Hệ thống",
    "Đăng ký catalog module kỹ thuật (SYS, HRM, CRM…) với mã, tên, phiên bản, dependencies.",
    ["Pack module được cài vào môi trường."],
    ["Hệ thống đăng ký module khi deploy hoặc admin đồng bộ catalog.", "Hiển thị dependencies (ví dụ LOG cần INV)."],
    [],
    ["Catalog module làm nguồn cho license và menu."],
    ["BR-SYS-LIC-01"],
    ["Danh sách 16 module sản phẩm hiện đủ trong catalog."]))
g5.append(uc("UC_SYS_045", "Bật / tắt module theo tenant", "Must", "System Admin",
    "Bật hoặc tắt module nghiệp vụ theo hợp đồng thuê bao.",
    ["Module có trong catalog.", "Không tắt SYS."],
    ["Chọn module → Active/Inactive.", "Khi Inactive: ẩn menu, chặn API, giữ dữ liệu.", "Kiểm tra dependency (không bật LOG nếu INV off — cảnh báo/chặn).", "Audit + event LicenseChanged."],
    ["Tắt module đang là dependency của module khác đang bật → cảnh báo."],
    ["User không truy cập UI/API module tắt."],
    ["BR-SYS-LIC-01", "BR-SYS-LIC-03"],
    ["Tắt CRM → menu CRM biến mất; API CRM 403."]))
g5.append(uc("UC_SYS_046", "Quản lý gói license", "Must", "System Admin",
    "Quản lý gói (Starter/Retail/…) gồm tập module, hạn dùng, quota.",
    ["Có quyền sys.license.manage."],
    ["Tạo/sửa gói: tên, danh sách module, effective/expiry, số user tối đa, số chi nhánh tối đa.", "Gán gói cho tenant.", "Audit."],
    [],
    ["Tenant vận hành đúng phạm vi gói."],
    ["BR-SYS-LIC-01"],
    ["Gán gói có CRM+FIN → chỉ bật đúng 2 module (+SYS)."]))
g5.append(uc("UC_SYS_047", "Giới hạn số user / chi nhánh theo gói", "Must", "Hệ thống / System Admin",
    "Enforce quota user active và chi nhánh active theo gói.",
    ["Gói có cấu hình quota."],
    ["Khi tạo user/chi nhánh vượt quota → chặn.", "Dashboard license hiển thị usage/quota.", "Cảnh báo khi đạt ngưỡng (ví dụ 90%)."],
    [],
    ["Không vượt quota trừ khi admin nâng gói."],
    ["BR-SYS-LIC-02"],
    ["Quota 10 user: user thứ 11 bị chặn."]))
g5.append(uc("UC_SYS_048", "Cảnh báo quý/gia hạn license", "Must", "System Admin / Hệ thống",
    "Cảnh báo sắp hết hạn và xử lý trạng thái hết hạn (grace period).",
    ["Gói có ngày hết hạn."],
    ["Job kiểm tra hạn hàng ngày.", "Gửi thông báo in-app/email cho admin trước N ngày.", "Khi hết hạn: chuyển ReadOnly hoặc Block theo policy; cho phép vào trang gia hạn."],
    [],
    ["Admin biết trước khi hết hạn; hết hạn áp dụng đúng policy."],
    ["BR-SYS-LIC-04"],
    ["Trước hạn 7 ngày có thông báo.", "Hết hạn không tạo chứng từ mới nếu policy ReadOnly."]))
# Fix typo in title - user facing
g5[-1]["ten"] = "Cảnh báo / gia hạn license"

g5.append(uc("UC_SYS_049", "Menu động theo module + quyền", "Must", "Hệ thống / End User",
    "Render menu chỉ gồm mục thuộc module đang bật và permission user có.",
    ["User đã đăng nhập."],
    ["Client gọi API menu.", "Server tính: licensed modules ∩ permissions ∩ feature flags.", "Trả cây menu; client hiển thị."],
    [],
    ["Không lộ entry module chưa mua."],
    ["BR-SYS-LIC-03", "BR-SYS-RBAC-03"],
    ["User thiếu quyền không thấy menu tương ứng.", "Module off không có entry."]))
g5.append(uc("UC_SYS_050", "Ẩn API module chưa mua", "Must", "Hệ thống",
    "Chặn gọi API của module không nằm trong license dù user đoán URL.",
    ["Request vào API gateway/backend."],
    ["Middleware kiểm tra license module của route.", "Nếu inactive → 403 FEATURE_NOT_LICENSED.", "Ghi security log khi bị gọi lặp bất thường (optional)."],
    [],
    ["API module off không thực thi nghiệp vụ."],
    ["BR-SYS-LIC-03"],
    ["Gọi API CRM khi CRM off → 403."]))
GROUPS.append(("05", "License & module bán hàng", "Cơ chế đóng gói–bán module và enforce runtime.", g5))

# ===== SYS-06 =====
g6 = []
g6.append(uc("UC_SYS_051", "Tham số cấu hình toàn cục", "Must", "System Admin",
    "Quản lý key-value settings toàn tenant (timeout, ngưỡng, feature flags nội bộ…).",
    ["Có quyền sys.setting.manage."],
    ["Danh sách settings có nhóm/mô tả/kiểu dữ liệu.", "Sửa giá trị có validate.", "Audit thay đổi."],
    ["Sửa key hệ thống nguy hiểm cần xác nhận 2 bước (optional)."],
    ["Giá trị mới có hiệu lực theo cơ chế cache invalidate."],
    ["BR-SYS-CFG-01"],
    ["Đổi session timeout áp dụng cho phiên mới."]))
g6.append(uc("UC_SYS_052", "Cấu hình theo chi nhánh", "Should", "Org Admin",
    "Cho phép override một số setting theo chi nhánh.",
    ["Setting được đánh dấu overridable."],
    ["Chọn chi nhánh → override giá trị.", "Clear override để kế thừa global."],
    [],
    ["Chi nhánh dùng giá trị riêng khi có override."],
    ["BR-SYS-CFG-01"],
    ["Chi nhánh A override timezone khác global."]))
g6.append(uc("UC_SYS_053", "Danh mục dùng chung", "Must", "System Admin",
    "Quản lý các danh mục dùng chung: đơn vị tính, loại tiền, trạng thái chung, lý do hủy…",
    ["Có quyền danh mục."],
    ["CRUD item theo từng loại danh mục.", "Mỗi item: mã, tên, thứ tự, trạng thái.", "Module khác chỉ đọc/tham chiếu."],
    ["Không xóa item đã tham chiếu — chỉ ngưng."],
    ["Danh mục nhất quán xuyên module."],
    ["BR-SYS-CFG-02"],
    ["Thêm ĐVT mới và chọn được ở module INV (khi có)."]))
g6.append(uc("UC_SYS_054", "Mẫu số chứng từ", "Must", "System Admin",
    "Định nghĩa rule đánh số chứng từ: tiền tố, năm/tháng, độ dài, reset kỳ.",
    ["Có quyền cấu hình sequence."],
    ["Tạo rule theo loại chứng từ (SO, PO, INV… đăng ký bởi module).", "Cấu hình pattern.", "Xem số hiện tại; không cho sửa lùi tùy tiện."],
    [],
    ["Chứng từ mới nhận số đúng rule."],
    ["BR-SYS-CFG-03"],
    ["Pattern SO-{YYYY}-{00001} sinh đúng số tiếp theo."]))
g6.append(uc("UC_SYS_055", "Sinh mã tự động", "Must", "Hệ thống",
    "Cung cấp service sinh mã atomic, không trùng dưới tải đồng thời.",
    ["Rule sequence đã cấu hình."],
    ["Module gọi Sequence.next(docType, context).", "SYS cấp số trong transaction/lock.", "Trả mã hiển thị."],
    ["Hết dải số → lỗi rõ ràng."],
    ["Không trùng mã trong cùng scope rule."],
    ["BR-SYS-CFG-03"],
    ["100 request song song không sinh mã trùng."]))
g6.append(uc("UC_SYS_056", "Cấu hình mẫu email / SMS", "Must", "System Admin",
    "Quản lý template thông báo có biến động ({{user_name}}, {{reset_link}}…).",
    ["Có quyền cấu hình thông báo."],
    ["CRUD template theo sự kiện.", "Preview với dữ liệu mẫu.", "Chọn kênh mặc định."],
    [],
    ["Sự kiện gửi dùng đúng template đang Active."],
    ["BR-SYS-NOTI-01"],
    ["Sửa template invite → email mời dùng nội dung mới."]))
g6.append(uc("UC_SYS_057", "Cấu hình lịch làm việc", "Should", "System Admin",
    "Thiết lập ngày làm việc trong tuần và lịch nghỉ lễ dùng cho SLA/WF/chấm công (khi module dùng).",
    ["Có quyền cấu hình."],
    ["Chọn ngày làm việc.", "Thêm ngày nghỉ lễ theo năm.", "Export/import lịch lễ."],
    [],
    ["Các module đọc calendar chung từ SYS."],
    ["BR-SYS-CFG-01"],
    ["Đánh dấu 01/01 là holiday thành công."]))
g6.append(uc("UC_SYS_058", "Quản lý phiên bản cấu hình", "Could", "System Admin",
    "Lưu snapshot thay đổi cấu hình quan trọng để xem lại/rollback có kiểm soát.",
    ["Có quyền cấu hình nâng cao."],
    ["Mỗi lần đổi setting quan trọng tạo version.", "Xem diff.", "Rollback (optional) có xác nhận và audit."],
    ["Rollback có thể bị chặn nếu không an toàn."],
    ["Có lịch sử cấu hình truy vết được."],
    ["BR-SYS-AUD-01"],
    ["Xem được phiên bản cấu hình trước đó."]))
GROUPS.append(("06", "Cấu hình hệ thống", "Tham số, danh mục dùng chung, đánh số chứng từ, template thông báo.", g6))

# ===== SYS-07 =====
g7 = []
g7.append(uc("UC_SYS_059", "Thông báo in-app", "Must", "End User / Hệ thống",
    "Chuông thông báo trong ứng dụng: danh sách, đánh dấu đã đọc, deep-link tới chứng từ.",
    ["User đăng nhập."],
    ["Hệ thống tạo notification khi có sự kiện.", "User mở trung tâm thông báo.", "Click → điều hướng đối tượng liên quan nếu còn quyền.", "Đánh dấu đã đọc / đọc tất cả."],
    ["Mất quyền xem đối tượng → thông báo vẫn xem được tiêu đề, deep-link bị chặn."],
    ["User nhận được thông báo gần realtime (websocket/poll)."],
    ["BR-SYS-NOTI-02"],
    ["Có thông báo mới hiển thị badge.", "Đánh dấu đã đọc cập nhật trạng thái."]))
g7.append(uc("UC_SYS_060", "Gửi email hệ thống", "Must", "Hệ thống",
    "Gửi email giao dịch qua SMTP/ESP đã cấu hình.",
    ["Email gateway Active.", "Có template hoặc nội dung."],
    ["Module/SYS tạo Outbox message.", "Worker gửi qua provider.", "Cập nhật trạng thái Sent/Failed; retry có giới hạn."],
    ["Fail → log lý do; không crash nghiệp vụ nguồn."],
    ["Email được gửi hoặc ghi nhận thất bại rõ ràng."],
    ["BR-SYS-NOTI-01"],
    ["Invite email gửi thành công trong môi trường có gateway test."]))
g7.append(uc("UC_SYS_061", "Gửi SMS / messaging", "Should", "Hệ thống",
    "Gửi SMS/Zalo OA (khung) cho OTP và cảnh báo.",
    ["SMS/messaging gateway đã cấu hình."],
    ["Tạo message → worker gửi → log delivery."],
    ["Provider lỗi → retry/fail log."],
    ["OTP SMS nhận được trong môi trường tích hợp thật."],
    ["BR-SYS-NOTI-01"],
    ["Gửi SMS test thành công hoặc mock provider ghi Sent."]))
g7.append(uc("UC_SYS_062", "Push notification mobile", "Should", "Hệ thống",
    "Đẩy thông báo về app mobile qua FCM/APNs.",
    ["Thiết bị user đã đăng ký push token.", "Cấu hình provider."],
    ["Sự kiện → push payload → provider → log."],
    ["Token hết hạn → đánh dấu invalid."],
    ["Thiết bị nhận push (hoặc log success từ provider)."],
    ["BR-SYS-NOTI-02"],
    ["Đăng ký token và gửi push test thành công."]))
g7.append(uc("UC_SYS_063", "Cấu hình sự kiện kích hoạt thông báo", "Must", "System Admin",
    "Map sự kiện hệ thống → kênh + template + đối tượng nhận (role/user/owner).",
    ["Có quyền cấu hình thông báo."],
    ["Chọn event (UserInvited, ApprovalPending…).", "Chọn kênh in-app/email/SMS/push.", "Chọn template + recipients rule.", "Bật/tắt rule."],
    [],
    ["Khi event phát sinh, rule Active được thực thi."],
    ["BR-SYS-NOTI-01"],
    ["Tắt rule → event không gửi kênh đó nữa."]))
g7.append(uc("UC_SYS_064", "Tùy chọn thông báo cá nhân", "Could", "End User",
    "Cho phép user tắt/bật một số loại thông báo không bắt buộc.",
    ["Đã đăng nhập."],
    ["Mở Preference.", "Tắt email marketing/nhắc không critical.", "Không cho tắt cảnh báo bảo mật bắt buộc."],
    [],
    ["Preference được tôn trọng khi gửi."],
    ["BR-SYS-NOTI-02"],
    ["Tắt email nhắc việc → không nhận email loại đó."]))
g7.append(uc("UC_SYS_065", "Nhật ký gửi thông báo", "Should", "System Admin",
    "Tra cứu lịch sử gửi: kênh, trạng thái, thời điểm, lỗi.",
    ["Có quyền xem log thông báo."],
    ["Lọc theo thời gian/user/kênh/trạng thái.", "Xem chi tiết payload đã che thông tin nhạy cảm nếu cần.", "Resend thủ công (optional)."],
    [],
    ["Truy vết được thông báo đã gửi."],
    ["BR-SYS-AUD-01"],
    ["Có bản ghi Sent cho email invite vừa gửi."]))
GROUPS.append(("07", "Thông báo", "Kênh in-app/email/SMS/push và cấu hình sự kiện kích hoạt.", g7))

# ===== SYS-08 =====
g8 = []
g8.append(uc("UC_SYS_066", "Upload file", "Must", "End User",
    "Upload file đính kèm với kiểm soát loại/dung lượng.",
    ["Đã đăng nhập.", "Có quyền upload trên đối tượng đích."],
    ["Chọn file → validate extension/MIME/size.", "Lưu storage (local/S3 tương đương).", "Tạo FileObject gắn entity.", "Quét virus nếu bật.", "Trả id file."],
    ["File không hợp lệ → từ chối.", "Vượt quota storage tenant → từ chối."],
    ["File sẵn sàng để tải/xem theo quyền."],
    ["BR-SYS-FILE-01"],
    ["Upload PDF hợp lệ thành công.", "Upload .exe bị chặn nếu không nằm whitelist."]))
g8.append(uc("UC_SYS_067", "Tải xuống / xem trước file", "Must", "End User",
    "Tải hoặc preview file nếu user có quyền trên đối tượng/file.",
    ["File tồn tại và chưa bị xóa cứng.", "User có quyền đọc."],
    ["Yêu cầu download/preview.", "Authorize.", "Stream file; ghi access log với file nhạy cảm."],
    ["Hết quyền → 403."],
    ["User nhận đúng nội dung file."],
    ["BR-SYS-FILE-02"],
    ["User có quyền tải được; user khác 403."]))
g8.append(uc("UC_SYS_068", "Quản lý thư mục tài liệu", "Should", "System Admin",
    "Tổ chức cây thư mục tài liệu dùng chung (không thay DMS doanh nghiệp lớn).",
    ["Có quyền quản lý tài liệu."],
    ["Tạo/đổi tên/di chuyển thư mục.", "Phân quyền thư mục cơ bản."],
    [],
    ["File sắp xếp theo thư mục."],
    ["BR-SYS-FILE-02"],
    ["Tạo thư mục con và upload file vào đó."]))
g8.append(uc("UC_SYS_069", "Phân quyền file theo đối tượng", "Should", "System Admin",
    "Quyền file kế thừa từ chứng từ/entity hoặc set riêng.",
    ["File gắn entity."],
    ["Mặc định: ai đọc được entity thì đọc được file.", "Cho phép override share cụ thể (optional)."],
    [],
    ["Không lộ file ngoài quyền."],
    ["BR-SYS-FILE-02"],
    ["User ngoài scope entity không tải được file."]))
g8.append(uc("UC_SYS_070", "Xóa mềm / khôi phục file", "Should", "End User / System Admin",
    "Đưa file vào thùng rác và khôi phục trong thời hạn giữ.",
    ["Có quyền xóa file trên đối tượng."],
    ["Xóa mềm → ẩn khỏi UI chính.", "Khôi phục trong retention window.", "Job purge sau hạn."],
    [],
    ["File xóa mềm không còn gắn hiển thị bình thường."],
    ["BR-SYS-FILE-01"],
    ["Xóa mềm rồi khôi phục thành công trong hạn."]))
g8.append(uc("UC_SYS_071", "Quét virus / bảo mật file", "Could", "Hệ thống",
    "Quét malware trước khi file ở trạng thái Available.",
    ["Engine quét được cấu hình."],
    ["Sau upload → trạng thái Scanning.", "Clean → Available; Infected → Blocked + thông báo."],
    ["Engine lỗi → giữ Pending/Blocked theo policy an toàn."],
    ["File nhiễm không cho tải."],
    ["BR-SYS-FILE-01"],
    ["File EICAR test bị Blocked trong môi trường có scanner."]))
GROUPS.append(("08", "File & tài liệu", "Lưu trữ đính kèm an toàn, có phân quyền và vòng đời file.", g8))

# ===== SYS-09 =====
g9 = []
g9.append(uc("UC_SYS_072", "Import Excel/CSV theo mẫu", "Must", "System Admin",
    "Khung import dùng chung: upload, validate, preview, commit.",
    ["Có mẫu import của đúng entity.", "Có quyền import entity đó."],
    ["Upload file đúng mẫu.", "Validate schema + business rules từng dòng.", "Preview: OK/Error.", "Commit dòng hợp lệ; sinh job result."],
    ["File sai schema → reject toàn bộ."],
    ["Dữ liệu hợp lệ được ghi; báo cáo lỗi tải được."],
    ["BR-SYS-IE-01"],
    ["Import file đúng mẫu thành công; file sai cột bị từ chối."]))
g9.append(uc("UC_SYS_073", "Tải file mẫu import", "Must", "End User",
    "Tải template Excel/CSV chuẩn cho từng loại import.",
    ["Loại import tồn tại."],
    ["Chọn loại → Download template (kèm sheet hướng dẫn nếu có)."],
    [],
    ["User có file mẫu đúng cấu trúc."],
    ["BR-SYS-IE-01"],
    ["Template mở được và có header đúng."]))
g9.append(uc("UC_SYS_074", "Export Excel", "Must", "End User",
    "Xuất dữ liệu danh sách theo filter/quyền hiện tại ra Excel.",
    ["Có quyền export trên màn hình."],
    ["Apply filter → Export.", "Giới hạn số dòng/job async nếu lớn.", "Audit export."],
    ["Vượt ngưỡng → chuyển job nền + thông báo khi xong."],
    ["File phản ánh đúng data scope."],
    ["BR-SYS-IE-02", "BR-SYS-SCOPE-01"],
    ["Export đúng số bản ghi đang lọc."]))
g9.append(uc("UC_SYS_075", "Export PDF", "Must", "End User",
    "Xuất chứng từ/báo cáo dạng PDF theo mẫu in.",
    ["Có mẫu in Active.", "Có quyền in/export."],
    ["Chọn bản ghi → In/PDF.", "Render template + dữ liệu.", "Tải file hoặc mở preview."],
    [],
    ["PDF chứa đúng dữ liệu chứng từ."],
    ["BR-SYS-IE-02"],
    ["PDF tạo thành công cho chứng từ mẫu."]))
g9.append(uc("UC_SYS_076", "Lịch sử job import/export", "Should", "System Admin",
    "Theo dõi các job import/export: trạng thái, tiến độ, file kết quả.",
    ["Có quyền xem job."],
    ["Danh sách job lọc theo loại/ngày/user.", "Xem log lỗi dòng.", "Tải file result."],
    [],
    ["Truy vết được các lần import/export."],
    ["BR-SYS-AUD-01"],
    ["Job vừa chạy xuất hiện trong lịch sử."]))
g9.append(uc("UC_SYS_077", "Xuất dữ liệu hàng loạt", "Could", "System Admin",
    "Xuất lớn phục vụ migration/backup cấu hình (không thay thế DB backup).",
    ["Có quyền vận hành đặc biệt."],
    ["Chọn tập entity → tạo export job.", "Chạy nền; thông báo khi xong; file có hạn tải."],
    [],
    ["Gói dữ liệu được tạo trong giới hạn kỹ thuật."],
    ["BR-SYS-IE-02"],
    ["Tạo được job bulk export và tải file khi hoàn tất."]))
GROUPS.append(("09", "Import / Export", "Khung nhập xuất dữ liệu dùng chung, có job và audit.", g9))

# ===== SYS-10 =====
g10 = []
g10.append(uc("UC_SYS_078", "Nhật ký thao tác người dùng", "Must", "Security Admin",
    "Ghi nhận thao tác CRUD/quan trọng: ai, khi nào, trên entity nào, hành động gì.",
    ["Hệ thống đang chạy."],
    ["Middleware/application tự ghi AuditLog.", "Màn hình tra cứu có lọc.", "Xem chi tiết."],
    ["Không ghi password/PII plaintext không cần thiết."],
    ["Truy vết được thao tác critical."],
    ["BR-SYS-AUD-01"],
    ["Tạo user sinh audit Create."]))
g10.append(uc("UC_SYS_079", "Nhật ký đăng nhập / thất bại", "Must", "Security Admin",
    "Lưu mọi attempt đăng nhập thành công/thất bại phục vụ an ninh.",
    [],
    ["Mỗi attempt ghi LoginLog.", "Dashboard cảnh báo spiking thất bại (optional).", "Export được."],
    [],
    ["Đủ dữ liệu điều tra brute-force."],
    ["BR-SYS-AUD-01", "BR-SYS-AUTH-02"],
    ["Login sai sinh bản ghi Failed."]))
g10.append(uc("UC_SYS_080", "Xem chi tiết thay đổi field", "Should", "Security Admin",
    "Hiển thị before/after ở cấp field với các entity bật field audit.",
    ["Entity hỗ trợ field audit.", "Có quyền xem audit."],
    ["Mở lịch sử bản ghi → xem từng field đổi.", "Lọc theo field."],
    [],
    ["Biết giá trị cũ/mới."],
    ["BR-SYS-AUD-01"],
    ["Đổi email user → thấy before/after email."]))
g10.append(uc("UC_SYS_081", "Xuất audit log", "Should", "Security Admin",
    "Xuất audit/login log ra Excel/CSV cho kiểm toán.",
    ["Có quyền export audit."],
    ["Chọn khoảng thời gian/filter → export job.", "Audit chính việc export."],
    [],
    ["File phục vụ đối soát ngoài hệ thống."],
    ["BR-SYS-AUD-01"],
    ["Export được log 7 ngày gần nhất."]))
g10.append(uc("UC_SYS_082", "Quản lý IP allow/deny", "Later", "Security Admin",
    "Giới hạn đăng nhập theo allowlist/denylist IP (tùy gói).",
    ["Feature được bật."],
    ["Cấu hình danh sách IP/CIDR.", "Enforce ở bước đăng nhập/API."],
    ["IP không thuộc allowlist → từ chối."],
    ["Chỉ IP hợp lệ truy cập được."],
    ["BR-SYS-SEC-01"],
    ["IP ngoài list bị chặn khi policy allowlist bật."]))
g10.append(uc("UC_SYS_083", "Chính sách hết hạn phiên", "Must", "Security Admin",
    "Cấu hình idle timeout và absolute session lifetime.",
    ["Có quyền cấu hình bảo mật."],
    ["Đặt idle timeout / max lifetime.", "Client/server enforce; hết hạn → 401 + yêu cầu đăng nhập lại."],
    [],
    ["Phiên idle quá hạn không dùng tiếp được."],
    ["BR-SYS-AUTH-03"],
    ["Idle timeout 15 phút: sau 15 phút không hoạt động bị đăng xuất."]))
GROUPS.append(("10", "Audit & bảo mật", "Nhật ký, truy vết và chính sách phiên/IP.", g10))

# ===== SYS-11 =====
g11 = []
g11.append(uc("UC_SYS_084", "Quản lý API Key", "Should", "System Admin",
    "Cấp/thu hồi API Key cho tích hợp máy-máy với quyền tối thiểu.",
    ["Có quyền sys.integration.manage."],
    ["Tạo key: tên, scopes/permissions, hạn dùng.", "Hiển thị secret một lần.", "Thu hồi/rotate.", "Audit."],
    [],
    ["Client dùng key còn hạn gọi được API trong scope."],
    ["BR-SYS-INT-01"],
    ["Key bị thu hồi → 401.", "Key thiếu scope → 403."]))
g11.append(uc("UC_SYS_085", "Quản lý Webhook outbound", "Should", "System Admin",
    "Đăng ký URL nhận sự kiện; ký payload; bật/tắt subscription.",
    ["Có quyền integration."],
    ["Tạo subscription: event types, URL, secret.", "Test ping.", "Xem trạng thái giao hàng."],
    ["URL không HTTPS (production) → cảnh báo/chặn theo policy."],
    ["Sự kiện được POST tới URL khi phát sinh."],
    ["BR-SYS-INT-02"],
    ["Test ping trả 2xx được đánh dấu Healthy."]))
g11.append(uc("UC_SYS_086", "Nhật ký gọi API / webhook", "Should", "System Admin",
    "Log request/response tóm tắt của API key và webhook delivery (ẩn secret).",
    ["Có quyền xem log tích hợp."],
    ["Lọc theo thời gian/status/key.", "Xem retry history.", "Replay delivery (optional)."],
    [],
    ["Debug tích hợp được."],
    ["BR-SYS-AUD-01"],
    ["Delivery thất bại có log status/code."]))
g11.append(uc("UC_SYS_087", "Hàng đợi sự kiện liên module", "Must", "Hệ thống",
    "Event bus nội bộ để các module giao tiếp bất đồng bộ (UserDisabled, LicenseChanged…).",
    ["Hệ thống chạy worker/queue."],
    ["Module publish domain event.", "Bus lưu và phân phối tới subscriber.", "Retry/poison queue khi lỗi.", "Idempotent consumer."],
    ["Consumer lỗi không làm mất event nguồn quá giới hạn retention."],
    ["Module đích nhận và xử lý event eventually consistent."],
    ["BR-SYS-INT-03"],
    ["Publish UserDisabled → subscriber thu hồi phiên/chạy cleanup."]))
g11.append(uc("UC_SYS_088", "Kết nối email gateway", "Must", "System Admin",
    "Cấu hình SMTP/ESP (host, port, credential, from-name) và gửi email thử.",
    ["Có quyền integration/setting."],
    ["Nhập cấu hình → Test connection/send.", "Lưu trạng thái Active.", "Che credential khi xem lại."],
    ["Test fail → không Active."],
    ["UC gửi email dùng được gateway."],
    ["BR-SYS-INT-01"],
    ["Test email thành công với SMTP giả lập/dev."]))
g11.append(uc("UC_SYS_089", "Kết nối SMS gateway", "Should", "System Admin",
    "Cấu hình nhà cung cấp SMS và gửi tin thử.",
    ["Có quyền integration."],
    ["Nhập API key/provider → test → Active."],
    [],
    ["Gửi SMS OTP dùng được khi Active."],
    ["BR-SYS-INT-01"],
    ["Cấu hình lưu và test status hiển thị đúng."]))
g11.append(uc("UC_SYS_090", "Cấu hình tích hợp bên ngoài", "Should", "System Admin",
    "Registry connector (HĐĐT, ngân hàng, chat…) ở mức khung: bật connector, lưu config, health.",
    ["Connector package có sẵn."],
    ["Danh sách connector → cấu hình → enable.", "Health check định kỳ.", "Phân quyền sử dụng connector."],
    [],
    ["Module nghiệp vụ gọi connector qua interface chuẩn."],
    ["BR-SYS-INT-01"],
    ["Enable connector hiển thị Healthy/Unhealthy."]))
GROUPS.append(("11", "Tích hợp nền tảng", "API Key, webhook, event bus và gateway kết nối ngoài.", g11))

# ===== SYS-12 =====
g12 = []
g12.append(uc("UC_SYS_091", "Quản lý gói ngôn ngữ", "Should", "System Admin",
    "Quản lý resource chuỗi UI theo ngôn ngữ (VI mặc định, EN…).",
    ["Có quyền cấu hình giao diện."],
    ["Import/export language pack.", "Sửa chuỗi.", "Đặt ngôn ngữ mặc định tenant."],
    [],
    ["UI lấy chuỗi theo ngôn ngữ đang chọn."],
    ["BR-SYS-UX-01"],
    ["Chuyển pack EN → một số nhãn key có bản dịch hiển thị EN."]))
g12.append(uc("UC_SYS_092", "Đổi ngôn ngữ giao diện", "Should", "End User",
    "User tự chọn ngôn ngữ UI riêng.",
    ["Đã đăng nhập.", "Language pack Active."],
    ["Chọn ngôn ngữ ở profile/header.", "Lưu preference.", "Reload UI."],
    [],
    ["UI theo preference user; nếu thiếu key → fallback VI."],
    ["BR-SYS-UX-01"],
    ["Đổi sang EN cập nhật giao diện ngay."]))
g12.append(uc("UC_SYS_093", "Tùy chỉnh theme / logo", "Could", "System Admin",
    "Branding cơ bản: logo, màu chủ đạo, favicon theo tenant.",
    ["Có quyền branding."],
    ["Upload logo/favicon; chọn màu.", "Preview.", "Publish."],
    [],
    ["Màn hình đăng nhập và header dùng branding mới."],
    ["BR-SYS-UX-01"],
    ["Đổi logo phản ánh trên trang đăng nhập."]))
g12.append(uc("UC_SYS_094", "Trang chủ theo vai trò", "Could", "System Admin",
    "Cấu hình landing/widget mặc định theo role sau đăng nhập.",
    ["Có quyền cấu hình UI."],
    ["Gán landing page/dashboard mặc định theo role.", "User thuộc nhiều role → chọn theo độ ưu tiên cấu hình."],
    [],
    ["Đăng nhập vào đúng landing."],
    ["BR-SYS-UX-01"],
    ["Role Sales vào landing CRM; role Accountant vào FIN (khi module bật)."]))
GROUPS.append(("12", "Đa ngôn ngữ & giao diện", "Ngôn ngữ, branding và trải nghiệm landing theo role.", g12))


def _br_bullets(items: list[str]) -> str:
    return "<br>".join(f"• {x}" for x in items if x)


def _br_numbered(items: list[str]) -> str:
    return "<br>".join(f"{i}. {x}" for i, x in enumerate(items, 1) if x)


def _format_preconditions(u: dict) -> str:
    items = ["Hệ thống ERP đang hoạt động bình thường."]
    actor = u["actor"]
    # UC công khai (chưa đăng nhập): không yêu cầu RBAC phiên hiện tại
    public_keys = ("đăng nhập", "quên mật khẩu", "đặt lại mật khẩu", "sso", "otp")
    ten_l = u["ten"].lower()
    if any(k in ten_l for k in public_keys):
        items.append(
            f"Người dùng có định danh hợp lệ thuộc nhóm đối tượng [{actor}] "
            "(hoặc được cấp tài khoản tương ứng) để thực hiện chức năng."
        )
    else:
        items.append(
            f"Người dùng đã đăng nhập tài khoản thuộc vai trò [{actor}] "
            "và được cấp quyền RBAC tương ứng."
        )
    for x in u["tien"]:
        if x not in items:
            items.append(x)
    return _br_bullets(items)


def _format_requirements(u: dict) -> str:
    items = [
        "Giao diện hiển thị đúng chuẩn UX/UI và ngôn ngữ tiếng Việt cấu hình.",
        "Dữ liệu đầu vào bắt buộc được kiểm tra Validation (Định dạng, Độ dài, Bắt buộc).",
        "Hệ thống kết nối CSDL ghi nhận transaction và lưu vĩnh viễn Audit Trail log.",
        f"Ưu tiên triển khai (MoSCoW): **{u['prio']}**.",
    ]
    if u["br"]:
        items.append("Quy tắc nghiệp vụ liên quan: " + ", ".join(f"`{b}`" for b in u["br"]) + ".")
    if u["hau"]:
        items.append("Hậu điều kiện: " + " ".join(u["hau"]))
    for i, ac in enumerate(u["ac"], 1):
        items.append(f"Tiêu chí chấp nhận AC{i}: {ac}")
    return _br_bullets(items)


def _format_alt_flows(u: dict) -> str:
    """Kịch bản phụ theo format mẫu: 3.1 / 4.1 / 5.1 + ngoại lệ nghiệp vụ riêng."""
    parts = [
        "3.1. Người dùng nhấn nút [Hủy / Thoát]:",
        "  3.1.1. Hệ thống hủy bỏ thao tác nhập liệu dở dang và quay về màn hình trước đó.",
        "4.1. Dữ liệu nhập vào không hợp lệ hoặc thiếu trường bắt buộc:",
        "  4.1.1. Hệ thống hiển thị thông báo lỗi màu đỏ tại đúng ô dữ liệu vi phạm.",
        "  4.1.2. Người dùng chỉnh sửa lại thông tin và nhấn nút xác nhận lại.",
        "5.1. Gián đoạn kết nối mạng hoặc lỗi cơ sở dữ liệu:",
        "  5.1.1. Hệ thống tự động Rollback giao dịch, báo lỗi cho người dùng và lưu Exception Log.",
    ]
    n = 6
    for x in u["ngoai"]:
        parts.append(f"{n}.1. {x}")
        n += 1
    return "<br>".join(parts)


def render_uc(u: dict, table_no: int, group_name: str) -> list[str]:
    """Đặc tả UC theo format bảng 8 trường (mẫu chuyên nghiệp)."""
    mo_ta = (
        f"Cho phép {u['actor']} thực hiện chức năng \"{u['ten']}\" "
        f"thuộc nhóm {group_name} trong module SYS — Hệ thống nền tảng. "
        f"Mô tả chi tiết: {u['mo_ta']}"
    )
    rows = [
        ("**Use Case ID**", u["ma"]),
        ("**Tên Use Case**", u["ten"]),
        ("**Tác nhân**", u["actor"]),
        ("**Mô tả chức năng**", mo_ta),
        ("**Điều kiện tiên quyết**", _format_preconditions(u)),
        ("**Yêu cầu**", _format_requirements(u)),
        ("**Kịch bản chính**", _br_numbered(u["luong"])),
        ("**Kịch bản phụ**", _format_alt_flows(u)),
    ]
    lines = [
        f"**Bảng {table_no}. Đặc tả Use Case \"{u['ten']}\"**",
        "",
        "| Trường Thông Tin | Nội Dung Đặc Tả |",
        "| :--- | :--- |",
    ]
    for label, value in rows:
        # Escape pipe trong nội dung bảng markdown
        safe = value.replace("|", "\\|")
        lines.append(f"| {label} | {safe} |")
    lines.append("")
    return lines


def main():
    total_uc = sum(len(g[3]) for g in GROUPS)
    lines: list[str] = []
    a = lines.append

    a("# SRS-SYS-v1.1 — Hệ thống nền tảng (System Platform)")
    a("")
    a("> **Software Requirements Specification — Module SYS**")
    a("> Phiên bản chỉnh chu sau rà soát; thay thế bản sinh tự động v1.0 cho module này.")
    a("> Trạng thái: **Chờ duyệt nghiệp vụ**. Nội dung generic, phục vụ bán ERP theo module.")
    a("")
    a("---")
    a("")
    a("## 0. Thông tin tài liệu & lịch sử")
    a("")
    a("| Thuộc tính | Giá trị |")
    a("|---|---|")
    a("| Mã tài liệu | `SRS-SYS-v1.1` |")
    a("| Module | `SYS` — Hệ thống nền tảng |")
    a("| Phiên bản | 1.1 |")
    a("| Ngày | 03/08/2026 |")
    a("| Phân loại | SRS nghiệp vụ (BA) |")
    a("| Đóng gói | Không bán riêng — luôn kèm mọi gói sản phẩm |")
    a(f"| Số nhóm / UC | {len(GROUPS)} nhóm / {total_uc} UC |")
    a("| Nguồn catalog | `cay_chuc_nang_data.py` (generic v3) |")
    a("")
    a("| Ver | Ngày | Mô tả | Trạng thái |")
    a("|---|---|---|---|")
    a("| 1.0 | 03/08/2026 | Sinh hàng loạt từ generator | Thay thế |")
    a("| 1.1 | 03/08/2026 | Viết lại đặc tả UC + BR + workflow chuyên sâu | Chờ duyệt |")
    a("")
    a("---")
    a("")
    a("## 1. Giới thiệu")
    a("")
    a("### 1.1. Mục đích")
    a("Đặc tả đầy đủ yêu cầu module **SYS** — lớp nền bắt buộc của sản phẩm ERP — để thống nhất nghiệp vụ trước khi thiết kế kiến trúc source và lập trình.")
    a("")
    a("### 1.2. Vai trò sản phẩm")
    a("SYS không phải module nghiệp vụ đầu cuối. SYS trả lời các câu hỏi:")
    a("")
    a("1. **Ai** được vào hệ thống? (xác thực, phiên, 2FA/SSO)")
    a("2. **Được làm gì**? (RBAC permission)")
    a("3. **Được thấy dữ liệu nào**? (data scope chi nhánh/kho/phòng ban)")
    a("4. **Được dùng module nào**? (license / gói bán)")
    a("5. **Hệ thống vận hành chung ra sao**? (cấu hình, file, thông báo, audit, tích hợp)")
    a("")
    a("Không có SYS ổn định thì không thể bán tách HRM/CRM/FIN… một cách an toàn.")
    a("")
    a("### 1.3. Mục tiêu đo được")
    a("| Mục tiêu | Chỉ dẫn đo |")
    a("|---|---|")
    a("| Onboard tenant mới trong ngày | Workflow WF-SYS-01 hoàn tất |")
    a("| Không lộ module chưa mua | UC_SYS_049/050 pass kiểm thử xâm nhập cơ bản |")
    a("| Truy vết thay đổi quyền | 100% thay đổi role/permission có audit |")
    a("| Chống brute-force | Khóa sau N lần sai hoạt động đúng cấu hình |")
    a("")
    a("### 1.4. Đối tượng đọc")
    a("Product Owner, BA, Architect, Tech Lead, QA, Presales/Implementation.")
    a("")
    a("---")
    a("")
    a("## 2. Phạm vi")
    a("")
    a("### 2.1. In Scope")
    a("- Xác thực & phiên (password, OTP reset, 2FA, SSO khung)")
    a("- User lifecycle (CRUD mềm, invite, import, khóa)")
    a("- RBAC + data scope + field-level security khung")
    a("- Tổ chức: tenant, pháp nhân, chi nhánh, điểm bán, phòng ban, chức danh")
    a("- License module, quota, menu động, chặn API")
    a("- Settings, danh mục dùng chung, sequence chứng từ")
    a("- Notification multi-channel, file, import/export framework")
    a("- Audit/security log, event bus, API key, webhook, gateway email/SMS")
    a("- Đa ngôn ngữ & branding cơ bản")
    a("")
    a("### 2.2. Out of Scope")
    a("- Nghiệp vụ chuyên môn HRM/CRM/FIN/POS/… (chỉ cung cấp nền tảng)")
    a("- Portal khách hàng (PRT) và nội dung marketing CMS")
    a("- BI self-service / kho dữ liệu phân tích (BI)")
    a("- IAM doanh nghiệp thay thế Okta/Azure AD toàn phần (SYS chỉ tích hợp OIDC)")
    a("- Sao lưu CSDL hạ tầng (thuộc vận hành DevOps)")
    a("")
    a("### 2.3. Đóng gói bán")
    a("| Tiêu chí | Quy định |")
    a("|---|---|")
    a("| Bán riêng | Không |")
    a("| Đi kèm | Mọi gói Starter → Full |")
    a("| Tắt được? | Không tắt SYS |")
    a("| Upsell liên quan | Quota user/chi nhánh; SSO; SMS; scanner file |")
    a("")
    a("---")
    a("")
    a("## 3. Tác nhân")
    a("")
    a("| Tác nhân | Mô tả |")
    a("|---|---|")
    a("| System Admin | Quản trị tenant: user, org, license, cấu hình, tích hợp |")
    a("| Security Admin | Password policy, 2FA, RBAC, audit, IP policy |")
    a("| Org Admin | Chi nhánh / điểm bán / phòng ban trong phạm vi được ủy quyền |")
    a("| End User | Đăng nhập, hồ sơ cá nhân, thông báo, đổi mật khẩu |")
    a("| Integration Account | Máy-máy qua API Key |")
    a("| Hệ thống | Job, worker gửi mail/SMS, event bus, enforce license |")
    a("")
    a("### 3.1. Phân tách trách nhiệm gợi ý")
    a("- Tenant nhỏ: một Super Admin giữ mọi quyền SYS.")
    a("- Tenant vừa/lớn: tách Security Admin khỏi admin vận hành thường ngày.")
    a("- Không để một user vừa là admin duy nhất vừa không bật 2FA trên môi trường production.")
    a("")
    a("---")
    a("")
    a("## 4. Thuật ngữ")
    a("")
    a("| Thuật ngữ | Định nghĩa |")
    a("|---|---|")
    a("| Tenant | Không gian dữ liệu cách ly logic của một khách hàng thuê bao |")
    a("| Session / Token | Phiên đăng nhập hợp lệ để gọi API/UI |")
    a("| RBAC | Phân quyền dựa trên vai trò |")
    a("| Permission | Quyền nguyên tử dạng `domain.resource.action` |")
    a("| Data scope | Bộ lọc dữ liệu bắt buộc theo org/kho/điểm/phòng ban |")
    a("| License | Cam kết thương mại về module + hạn + quota |")
    a("| Sequence | Bộ sinh số chứng từ atomic |")
    a("| Event bus | Hàng đợi sự kiện nội bộ giữa các module |")
    a("| Soft-delete | Ngưng dùng nhưng giữ dữ liệu để truy vết |")
    a("| JIT provisioning | Tự tạo user khi đăng nhập SSO lần đầu (nếu bật) |")
    a("")
    a("---")
    a("")
    a("## 5. Ngữ cảnh kiến trúc nghiệp vụ")
    a("")
    a("```text")
    a("Clients (Web/App)")
    a("       |  AuthN")
    a("       v")
    a("+---------------------------+")
    a("|            SYS            |")
    a("| Auth · RBAC · Data Scope  |")
    a("| License · Menu · Audit    |")
    a("| File · Notify · Event Bus |")
    a("+-------------+-------------+")
    a("              |")
    a("   +----------+----------+---------+")
    a("   v          v          v         v")
    a(" HRM/LMS   CRM/POS   PUR/INV/LOG  FIN/AST/...")
    a("```")
    a("")
    a("### 5.1. Nguyên tắc phụ thuộc")
    a("1. Mọi module nghiệp vụ **bắt buộc** gọi SYS cho identity & authorization.")
    a("2. Module nghiệp vụ **đăng ký** permission + sequence + menu khi được bật.")
    a("3. Event xuyên module đi qua bus SYS (tránh gọi chéo chặt cứng không kiểm soát).")
    a("4. Enforce license ở **hai lớp**: UI menu và API middleware.")
    a("")
    a("### 5.2. Tích hợp bên ngoài (khung)")
    a("| Loại | Ví dụ | UC liên quan |")
    a("|---|---|---|")
    a("| Email | SMTP / ESP | UC_SYS_088, 060 |")
    a("| SMS | SMS provider | UC_SYS_089, 061 |")
    a("| SSO | Google/Microsoft OIDC | UC_SYS_009 |")
    a("| Webhook | URL khách hàng | UC_SYS_085 |")
    a("| Push | FCM/APNs | UC_SYS_062 |")
    a("")
    a("---")
    a("")
    a("## 6. Catalog chức năng")
    a("")
    a(f"**Tổng:** {len(GROUPS)} nhóm · {total_uc} use case.")
    a("")
    a("| STT | Mã nhóm | Nhóm | Số UC | Must | Should | Could/Later |")
    a("|---:|---|---|---:|---:|---:|---:|")
    for i, (code, name, _desc, ucs) in enumerate(GROUPS, 1):
        must = sum(1 for u in ucs if u["prio"] == "Must")
        should = sum(1 for u in ucs if u["prio"] == "Should")
        other = len(ucs) - must - should
        a(f"| {i} | `SYS-{code}` | {name} | {len(ucs)} | {must} | {should} | {other} |")
    a("")
    a("<details>")
    a("<summary>Bảng mã UC đầy đủ</summary>")
    a("")
    a("| Mã UC | Nhóm | Tên | Ưu tiên |")
    a("|---|---|---|---|")
    for code, name, _d, ucs in GROUPS:
        for u in ucs:
            a(f"| `{u['ma']}` | {name} | {u['ten']} | {u['prio']} |")
    a("")
    a("</details>")
    a("")
    a("### 6.1. Đề xuất Phase")
    a("| Phase | Phạm vi gợi ý |")
    a("|---|---|")
    a("| Phase 1 — Go-live nền | Toàn bộ **Must** |")
    a("| Phase 2 — An toàn & vận hành | Các **Should** (2FA, session manager, API key, webhook, SMS, field security…) |")
    a("| Phase 3 — Nâng cao | **Could/Later** (trusted device, IP allowlist, virus scan, bulk export, versioning cấu hình…) |")
    a("")
    a("---")
    a("")
    a("## 7. Đặc tả Use Case theo nhóm")
    a("")
    a(
        "Mỗi use case được đặc tả bằng **một bảng thống nhất** gồm 8 trường: "
        "Use Case ID, Tên Use Case, Tác nhân, Mô tả chức năng, Điều kiện tiên quyết, "
        "Yêu cầu, Kịch bản chính, Kịch bản phụ."
    )
    a("")

    table_no = 0
    for code, name, desc, ucs in GROUPS:
        a(f"### 7.{int(code)}. {name} (`SYS-{code}`)")
        a("")
        a(desc)
        a("")
        a("| Chỉ số | Giá trị |")
        a("|---|---|")
        a(f"| Số UC | {len(ucs)} |")
        a(f"| Must | {sum(1 for u in ucs if u['prio']=='Must')} |")
        a("")
        for u in ucs:
            table_no += 1
            lines.extend(render_uc(u, table_no, name))

    a("---")
    a("")
    a("## 8. Workflow end-to-end")
    a("")
    a("### WF-SYS-01 — Onboard tenant mới")
    a("")
    a("**Mục tiêu:** Tenant sẵn sàng để admin đầu tiên làm việc và bật module đã mua.")
    a("")
    a("| Bước | Thực hiện bởi | Hành động | UC |")
    a("|---:|---|---|---|")
    a("| 1 | Ops / Hệ thống | Tạo tenant + hồ sơ công ty | UC_SYS_034 |")
    a("| 2 | Ops | Gán gói license (module, hạn, quota) | UC_SYS_046, 045 |")
    a("| 3 | Ops | Tạo System Admin đầu tiên + gửi invite | UC_SYS_013, 019 |")
    a("| 4 | Admin | Kích hoạt, đặt mật khẩu, (khuyến nghị) bật 2FA | UC_SYS_005, 008 |")
    a("| 5 | Admin | Tạo chi nhánh/điểm bán gốc | UC_SYS_036, 037 |")
    a("| 6 | Admin | Cấu hình timezone/tiền tệ + email gateway | UC_SYS_041, 088 |")
    a("| 7 | Admin | Tạo role chuẩn + gán permission | UC_SYS_023, 026 |")
    a("| 8 | Hệ thống | Menu động phản ánh đúng module | UC_SYS_049 |")
    a("")
    a("**Hoàn tất khi:** Admin đăng nhập được, thấy đúng menu module đã mua, gửi email hệ thống test OK.")
    a("")
    a("### WF-SYS-02 — Cấp quyền nhân sự mới vào hệ thống")
    a("")
    a("| Bước | Hành động | UC |")
    a("|---:|---|---|")
    a("| 1 | Tạo user / gửi invite | UC_SYS_013, 019 |")
    a("| 2 | Gán chi nhánh + scope kho (nếu cần) | UC_SYS_017, 028, 029 |")
    a("| 3 | Gán role | UC_SYS_027 |")
    a("| 4 | User kích hoạt & đăng nhập | UC_SYS_001 |")
    a("| 5 | Kiểm tra menu/data scope đúng thiết kế | UC_SYS_049, 028 |")
    a("")
    a("**Hoàn tất khi:** User vào đúng màn hình được phép; không thấy dữ liệu ngoài scope.")
    a("")
    a("### WF-SYS-03 — Upsell / đổi gói module")
    a("")
    a("| Bước | Hành động | UC |")
    a("|---:|---|---|")
    a("| 1 | Cập nhật gói license (thêm/bớt module, hạn, quota) | UC_SYS_046 |")
    a("| 2 | Bật/tắt module runtime | UC_SYS_045 |")
    a("| 3 | Menu + API enforce tức thì | UC_SYS_049, 050 |")
    a("| 4 | Thông báo admin + audit | UC_SYS_059, 078 |")
    a("")
    a("**Lưu ý:** Tắt module **không xóa dữ liệu**; chỉ ẩn/chặn truy cập theo chính sách lưu trữ.")
    a("")
    a("### WF-SYS-04 — Quên mật khẩu")
    a("")
    a("| Bước | UC |")
    a("|---:|---|")
    a("| 1. Request OTP/link | UC_SYS_004 |")
    a("| 2. Đặt mật khẩu mới | UC_SYS_005 |")
    a("| 3. Đăng nhập | UC_SYS_001 |")
    a("| 4. Thu hồi phiên cũ | UC_SYS_005 / 010 |")
    a("")
    a("---")
    a("")
    a("## 9. Mô hình dữ liệu domain (conceptual)")
    a("")
    a("| Thực thể | Mô tả | Quan hệ chính |")
    a("|---|---|---|")
    a("| `Tenant` | Khách hàng thuê bao | 1–n Company/Branch |")
    a("| `Company` / `LegalEntity` | Pháp nhân | thuộc Tenant |")
    a("| `Branch` / `Outlet` | Chi nhánh / điểm bán | cây tổ chức |")
    a("| `Department` / `JobTitle` | Phòng ban / chức danh | master dùng chung |")
    a("| `User` | Tài khoản đăng nhập | n–n Role; n–n Branch scope |")
    a("| `Role` / `Permission` | RBAC | Role n–n Permission |")
    a("| `UserRole` / `DataScope` | Gán quyền & phạm vi | theo User/Role |")
    a("| `License` / `LicenseModule` | Gói & module | theo Tenant |")
    a("| `Setting` / `Sequence` | Cấu hình & sinh mã | theo Tenant/(Branch) |")
    a("| `Notification` / `Template` / `Outbox` | Thông báo | theo User/Event |")
    a("| `FileObject` | File đính kèm | gắn entity nghiệp vụ |")
    a("| `AuditLog` / `LoginLog` | Nhật ký | theo User/Entity |")
    a("| `ApiKey` / `WebhookSubscription` | Tích hợp | theo Tenant |")
    a("| `DomainEvent` | Sự kiện bus | publish/subscribe |")
    a("")
    a("### 9.1. Trạng thái User (gợi ý)")
    a("`InvitePending` → `Active` → `LockedTemporarily` / `Disabled` → `SoftDeleted`")
    a("")
    a("---")
    a("")
    a("## 10. Quy tắc nghiệp vụ tổng hợp")
    a("")
    a("### 10.1. Xác thực & phiên")
    a("- `BR-SYS-AUTH-01`: Không lưu mật khẩu plaintext; chỉ lưu hash mạnh.")
    a("- `BR-SYS-AUTH-02`: Vượt N lần đăng nhập sai → khóa tạm theo cấu hình.")
    a("- `BR-SYS-AUTH-03`: Đăng xuất / thu hồi phiên làm token hết hiệu lực ngay.")
    a("- `BR-SYS-AUTH-04`: Mật khẩu mới phải đạt Password Policy tenant.")
    a("- `BR-SYS-AUTH-05`: Reset/đổi mật khẩu phải ghi audit; khuyến nghị thu hồi phiên khác.")
    a("- `BR-SYS-AUTH-06`: OTP/link reset có hạn và chỉ dùng một lần.")
    a("- `BR-SYS-AUTH-07`: Role bắt buộc 2FA thì chưa setup 2FA không vào được hệ thống.")
    a("- `BR-SYS-AUTH-08`: SSO chỉ tạo user mới khi JIT provisioning được bật.")
    a("- `BR-SYS-AUTH-09`: Số phiên đồng thời không vượt ngưỡng cấu hình.")
    a("")
    a("### 10.2. User / RBAC / Scope")
    a("- `BR-SYS-USER-01`: Không xóa cứng user đã phát sinh dữ liệu; chỉ soft-delete/disable.")
    a("- `BR-SYS-USER-02`: Self-service chỉ sửa được field được phép.")
    a("- `BR-SYS-USER-03`: Không khóa/xóa hết System Admin cuối cùng của tenant.")
    a("- `BR-SYS-USER-04`: Invite token có thời hạn.")
    a("- `BR-SYS-RBAC-01`: Mã role duy nhất trong tenant.")
    a("- `BR-SYS-RBAC-02`: Permission hệ thống do module đăng ký; không xóa tùy tiện.")
    a("- `BR-SYS-RBAC-03`: Quyền hiệu lực enforce ở API, không chỉ ẩn UI.")
    a("- `BR-SYS-RBAC-04`: Multi-role → hợp permission (union), trừ khi tenant cấu hình khác.")
    a("- `BR-SYS-RBAC-05`: Field nhạy cảm không trả plain value nếu thiếu quyền.")
    a("- `BR-SYS-SCOPE-01/02/03`: Mọi truy vấn nghiệp vụ phải áp data scope chi nhánh/kho/phòng ban.")
    a("")
    a("### 10.3. License & cấu hình")
    a("- `BR-SYS-LIC-01`: Catalog module là nguồn sự thật cho bán/bật-tắt.")
    a("- `BR-SYS-LIC-02`: Không vượt quota user/chi nhánh của gói.")
    a("- `BR-SYS-LIC-03`: Module off → ẩn menu + chặn API; không xóa dữ liệu.")
    a("- `BR-SYS-LIC-04`: Hết hạn license áp dụng ReadOnly/Block theo policy + grace period.")
    a("- `BR-SYS-CFG-01`: Đổi setting quan trọng phải audit.")
    a("- `BR-SYS-CFG-02`: Danh mục dùng chung đã tham chiếu chỉ được ngưng, không xóa cứng.")
    a("- `BR-SYS-CFG-03`: Sequence sinh mã phải atomic, không trùng.")
    a("")
    a("### 10.4. Thông báo / File / Tích hợp / Audit")
    a("- `BR-SYS-NOTI-01`: Gửi theo template Active và rule sự kiện.")
    a("- `BR-SYS-NOTI-02`: Không cho user tắt cảnh báo bảo mật bắt buộc.")
    a("- `BR-SYS-FILE-01`: Validate loại/dung lượng; soft-delete mặc định.")
    a("- `BR-SYS-FILE-02`: Authorize theo quyền entity/file trước khi download.")
    a("- `BR-SYS-IE-01/02`: Import có preview lỗi; export tôn trọng data scope + audit.")
    a("- `BR-SYS-INT-01`: API Key least privilege; che secret.")
    a("- `BR-SYS-INT-02`: Webhook có chữ ký/secret; retry có giới hạn.")
    a("- `BR-SYS-INT-03`: Consumer event phải idempotent.")
    a("- `BR-SYS-AUD-01`: Thay đổi critical (user/role/license/password/integration) bắt buộc có audit.")
    a("- `BR-SYS-SEC-01`: Chính sách phiên/IP áp dụng nhất quán.")
    a("- `BR-SYS-UX-01`: Thiếu bản dịch → fallback ngôn ngữ mặc định (VI).")
    a("")
    a("---")
    a("")
    a("## 11. Yêu cầu phi chức năng (NFR)")
    a("")
    a("| Nhóm | Yêu cầu |")
    a("|---|---|")
    a("| Bảo mật | TLS; hash mật khẩu; hỗ trợ 2FA; chống brute-force; secret không log plaintext |")
    a("| Hiệu năng | Đăng nhập p95 < 2s; kiểm tra permission cache được; menu build < 500ms sau cache ấm |")
    a("| Độ tin cậy | Event bus có retry/poison; gửi mail không làm fail giao dịch nguồn |")
    a("| Audit | Giữ audit/login log tối thiểu 12 tháng (cấu hình được) |")
    a("| Đa thuê bao | Cách ly dữ liệu theo TenantId trên mọi bảng SYS |")
    a("| Khả dụng | Dịch vụ auth là critical path — cần HA khi production |")
    a("| Usability | Form lỗi rõ field; tiếng Việt mặc định |")
    a("| Quan sát | Metric đăng nhập thất bại, quota license, webhook fail rate |")
    a("")
    a("---")
    a("")
    a("## 12. Tích hợp & sự kiện")
    a("")
    a("### 12.1. Sự kiện domain (logical)")
    a("| Event | Khi nào | Subscriber ví dụ |")
    a("|---|---|---|")
    a("| `UserCreated` / `UserInvited` | Tạo/mời user | Notification |")
    a("| `UserDisabled` / `UserDeleted` | Khóa/xóa mềm | Thu hồi session, API key |")
    a("| `RolePermissionsChanged` | Đổi ma trận quyền | Invalidate cache quyền |")
    a("| `LicenseChanged` | Đổi gói/module | Menu, API enforce, BI dataset |")
    a("| `NotificationRequested` | Module yêu cầu gửi thông báo | Email/SMS/Push workers |")
    a("| `FileScanned` | Xong quét virus | Đổi trạng thái FileObject |")
    a("")
    a("### 12.2. Hợp đồng với module nghiệp vụ")
    a("Mỗi module khi bật phải đăng ký tối thiểu:")
    a("1. Danh mục `Permission`")
    a("2. Menu entries")
    a("3. Sequence/doc types (nếu có chứng từ)")
    a("4. (Optional) sensitive fields + event handlers")
    a("")
    a("---")
    a("")
    a("## 13. Phân quyền & bảo mật")
    a("")
    a("### 13.1. Permission catalog đề xuất")
    a("```")
    a("sys.user.manage | sys.user.view")
    a("sys.role.manage | sys.permission.assign")
    a("sys.org.manage")
    a("sys.license.manage")
    a("sys.setting.manage")
    a("sys.file.manage")
    a("sys.audit.view")
    a("sys.integration.manage")
    a("sys.notify.manage")
    a("sys.security.manage")
    a("```")
    a("")
    a("### 13.2. Nguyên tắc")
    a("- Deny by default.")
    a("- Enforce tại API gateway/middleware + kiểm tra trong use-case.")
    a("- Tách quyền xem audit khỏi quyền sửa phân quyền.")
    a("- Môi trường production: bắt buộc HTTPS; khuyến nghị bắt buộc 2FA cho admin.")
    a("")
    a("---")
    a("")
    a("## 14. Báo cáo & KPI vận hành SYS")
    a("")
    a("| KPI | Mục đích |")
    a("|---|---|")
    a("| User active / quota | Kiểm soát gói bán |")
    a("| Chi nhánh active / quota | Kiểm soát gói bán |")
    a("| Tỷ lệ đăng nhập thất bại | An ninh |")
    a("| Số user bị khóa tạm | Brute-force / hỗ trợ |")
    a("| License days-to-expire | Gia hạn |")
    a("| Webhook failure rate | Sức khỏe tích hợp |")
    a("| Email/SMS fail rate | Sức khỏe thông báo |")
    a("| Số thay đổi phân quyền / tuần | Kiểm toán |")
    a("")
    a("---")
    a("")
    a("## 15. Giả định, rủi ro, câu hỏi mở")
    a("")
    a("### 15.1. Giả định")
    a("- Một khách hàng thương mại = một Tenant (multi-company nằm trong tenant nếu cần).")
    a("- Module nghiệp vụ tuân thủ hợp đồng đăng ký permission/menu/sequence.")
    a("- Email gateway có sẵn trước khi dùng invite/reset password trên production.")
    a("")
    a("### 15.2. Rủi ro")
    a("| Rủi ro | Mức | Hướng xử lý |")
    a("|---|---|---|")
    a("| Admin quên bật 2FA | Cao | Checklist go-live + cảnh báo |")
    a("| Cấu hình scope sai → lộ dữ liệu chi nhánh | Cao | Bộ role template + test case scope |")
    a("| Tắt module làm gãy tích hợp E2E | Trung bình | Dependency check khi bật/tắt |")
    a("| Spam OTP quên mật khẩu | Trung bình | Rate limit theo IP/định danh |")
    a("")
    a("### 15.3. Câu hỏi cần chốt")
    a("1. Phase 1 có bắt buộc tách DB theo tenant hay chỉ lọc `TenantId`?")
    a("2. Khi hết hạn license: **ReadOnly** hay **Block login** (trừ admin gia hạn)?")
    a("3. SSO/JIT có nằm Phase 1 không, hay để Phase 2?")
    a("4. Giữ dữ liệu module đã tắt trong bao lâu trước khi cho phép purge?")
    a("5. Có cho phép một user thuộc nhiều tenant (chuyển context) không?")
    a("")
    a("---")
    a("")
    a("## 16. Tiêu chí nghiệm thu & truy vết")
    a("")
    a("### 16.1. Điều kiện chấp nhận module SYS")
    a("1. 100% UC **Must** pass UAT.")
    a("2. WF-SYS-01..04 chạy thành công trên môi trường demo.")
    a("3. Kiểm thử phủ: login/logout/reset password/lock after N fails.")
    a("4. Kiểm thử license: tắt module → menu mất + API 403; dữ liệu vẫn còn.")
    a("5. Kiểm thử RBAC + data scope với ≥ 3 role và ≥ 2 chi nhánh.")
    a("6. Audit có before/after cho đổi quyền và reset mật khẩu.")
    a("7. Email gateway test gửi thành công.")
    a("8. Không còn đặc tả UC dùng luồng khuôn mẫu sai (đăng xuất ≠ dashboard…).")
    a("")
    a("### 16.2. Truy vết")
    a("| Artifact | Vị trí |")
    a("|---|---|")
    a("| Catalog chức năng | `../../00. Tổng quan/cay_chuc_nang_data.py` |")
    a("| Excel tổng hợp | `../../00. Tổng quan/Danh_muc_Module_Chuc_nang_ERP_v3.xlsx` |")
    a("| Chuẩn SRS | `../00_CHUAN_VIET_SRS.md` |")
    a("| Bản SRS này | `SRS_SYS_v1.1.md` |")
    a("| UC IDs | `UC_SYS_001` … `UC_SYS_094` |")
    a("")
    a("---")
    a("")
    a("## Phụ lục A — Role template khởi tạo (gợi ý)")
    a("")
    a("| Role | Mục đích | Nhóm quyền gợi ý |")
    a("|---|---|---|")
    a("| Super Admin | Toàn quyền tenant | tất cả `sys.*` |")
    a("| Security Admin | Bảo mật & phân quyền | `sys.security.*`, `sys.role.*`, `sys.audit.view` |")
    a("| Org Admin | Tổ chức | `sys.org.manage`, `sys.user.view` |")
    a("| Support Agent | Hỗ trợ user | `sys.user.manage` (hạn chế), không `sys.license.manage` |")
    a("| End User mặc định | Người dùng nghiệp vụ | self-service password/notify/file theo module |")
    a("")
    a("---")
    a("")
    a("*Hết tài liệu SRS-SYS-v1.1. Sau khi duyệt, đóng băng phiên bản và chuyển sang module tiếp theo.*")
    a("")

    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text("\n".join(lines), encoding="utf-8")
    print(f"Wrote {OUT}")
    print(f"Groups={len(GROUPS)} UCs={total_uc} lines={len(lines)}")


if __name__ == "__main__":
    main()
