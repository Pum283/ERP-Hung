import { redirect } from "next/navigation";

export default function HrmModuleIndex() {
  redirect("/app/hrm/employees");
}
