import type { Metadata } from "next";
import { Manrope, JetBrains_Mono, Be_Vietnam_Pro } from "next/font/google";
import "./globals.css";

/** Brand Kit fonts — đổi family tại đây; màu/cỡ chữ ở brand-kit.css */
const brandSans = Manrope({
  variable: "--font-brand-sans",
  subsets: ["latin", "vietnamese"],
});

const brandDisplay = Be_Vietnam_Pro({
  variable: "--font-brand-display",
  subsets: ["latin", "vietnamese"],
  weight: ["500", "600", "700"],
});

const brandMono = JetBrains_Mono({
  variable: "--font-brand-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Pum's ERP",
  description: "Pum's ERP — modular shell",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi">
      <body
        className={`${brandSans.variable} ${brandDisplay.variable} ${brandMono.variable} antialiased font-sans`}
      >
        {children}
      </body>
    </html>
  );
}
