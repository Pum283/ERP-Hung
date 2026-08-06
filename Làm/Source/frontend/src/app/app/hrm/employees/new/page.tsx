import { redirect } from "next/navigation";

/** Form đã chuyển sang SideSheet trên danh sách — giữ route để bookmark cũ không 404. */
export default function NewEmployeeRedirect() {
  redirect("/app/hrm/employees");
}
