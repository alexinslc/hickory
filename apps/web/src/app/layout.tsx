import './global.css';
import { QueryProvider } from '@/providers/query-provider';
import { ThemeProvider } from '@/providers/theme-provider';
import { Navigation } from '@/components/layout/Navigation';
import { NotificationProvider } from '@/components/notifications/NotificationProvider';
import { NotificationToast } from '@/components/notifications/NotificationToast';
import { ToastProvider } from '@/components/ui/toast';

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
    <html lang="en" suppressHydrationWarning>
      <head>
        <script
          dangerouslySetInnerHTML={{
            __html: `
              (function() {
                const theme = localStorage.getItem('hickory-theme');
                const validThemes = ['light', 'dark', 'system'];
                const initialTheme = theme && validThemes.includes(theme) ? theme : 'system';
                const isDark = initialTheme === 'dark' || (initialTheme === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches);
                if (isDark) document.documentElement.classList.add('dark');
              })();
            `,
          }}
        />
      </head>
      <body>
        <ThemeProvider>
          <ToastProvider>
            <a 
              href="#main-content" 
              className="sr-only focus:not-sr-only focus:absolute focus:top-4 focus:left-4 focus:z-50 focus:px-4 focus:py-2 focus:bg-primary focus:text-primary-foreground focus:rounded-md focus:shadow-lg"
            >
              Skip to main content
            </a>
            <QueryProvider>
              <NotificationProvider>
                <Navigation />
                <main id="main-content">{children}</main>
                <NotificationToast />
              </NotificationProvider>
            </QueryProvider>
          </ToastProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
