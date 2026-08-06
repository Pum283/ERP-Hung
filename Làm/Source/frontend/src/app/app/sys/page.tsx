import { redirect } from "next/navigation";

export default function SysModuleIndex() {
  redirect("/app/sys/users");
}
