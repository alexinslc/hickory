import './global.css';
import { QueryProvider } from '@/providers/query-provider';
import { Navigation } from '@/components/layout/Navigation';

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
        <QueryProvider>
          <Navigation />
          <main>{children}</main>
        </QueryProvider>
      </body>
    </html>
  );
}
