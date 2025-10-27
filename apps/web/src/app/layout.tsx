import './global.css';
import { QueryProvider } from '@/providers/query-provider';
import { Navigation } from '@/components/layout/Navigation';
import { NotificationProvider } from '@/components/notifications/NotificationProvider';
import { NotificationToast } from '@/components/notifications/NotificationToast';

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
          <NotificationProvider>
            <Navigation />
            <main>{children}</main>
            <NotificationToast />
          </NotificationProvider>
        </QueryProvider>
      </body>
    </html>
  );
}
