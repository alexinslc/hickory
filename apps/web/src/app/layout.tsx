import './global.css';
import { QueryProvider } from '@/providers/query-provider';

export const metadata = {
  title: 'Hickory Help Desk',
  description: 'Modern help desk system for customer support',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body>
        <QueryProvider>{children}</QueryProvider>
      </body>
    </html>
  );
}
