import type { Metadata } from 'next'
import './globals.css'

export const metadata: Metadata = {
  title: 'Digital Human - NVIDIA + LangChain',
  description: 'Digital Human powered by NVIDIA ACE and integrated with LangChain',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  )
}