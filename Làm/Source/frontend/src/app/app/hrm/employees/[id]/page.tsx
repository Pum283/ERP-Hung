import { redirect } from "next/navigation";

export default function EmployeeDetailRedirect() {
  redirect("/app/hrm/employees");
}
