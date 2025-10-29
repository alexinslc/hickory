'use client';

import Link from 'next/link';
import { useAuthStore } from '@/store/auth-store';
import { useMyTickets } from '@/hooks/use-tickets';
import { AuthGuard } from '@/components/auth-guard';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
  TicketIcon,
  Clock,
  CheckCircle2,
  AlertCircle,
  BarChart3,
  MessageSquare,
  BookOpen,
  Loader2,
} from 'lucide-react';
import { useMemo } from 'react';

export default function DashboardPage() {
  const { user } = useAuthStore();
  const { data: tickets, isLoading } = useMyTickets();

  const stats = useMemo(() => {
    if (!tickets) {
      return [
        {
          title: 'Open Tickets',
          value: '0',
          description: 'Loading...',
          icon: TicketIcon,
          trend: 'neutral' as const,
          color: 'text-blue-600',
          bgColor: 'bg-blue-50',
        },
        {
          title: 'In Progress',
          value: '0',
          description: 'Loading...',
          icon: Clock,
          trend: 'neutral' as const,
          color: 'text-yellow-600',
          bgColor: 'bg-yellow-50',
        },
        {
          title: 'Resolved',
          value: '0',
          description: 'Loading...',
          icon: CheckCircle2,
          trend: 'neutral' as const,
          color: 'text-green-600',
          bgColor: 'bg-green-50',
        },
        {
          title: 'High Priority',
          value: '0',
          description: 'Loading...',
          icon: AlertCircle,
          trend: 'neutral' as const,
          color: 'text-red-600',
          bgColor: 'bg-red-50',
        },
      ];
    }

    const openTickets = tickets.filter(t => t.status === 'Open').length;
    const inProgressTickets = tickets.filter(t => t.status === 'InProgress').length;
    const resolvedTickets = tickets.filter(t => t.status === 'Resolved').length;
    const highPriorityTickets = tickets.filter(t => t.priority === 'High' || t.priority === 'Urgent').length;

    return [
      {
        title: 'Open Tickets',
        value: openTickets.toString(),
        description: `${tickets.length} total tickets`,
        icon: TicketIcon,
        trend: 'neutral' as const,
        color: 'text-blue-600',
        bgColor: 'bg-blue-50',
      },
      {
        title: 'In Progress',
        value: inProgressTickets.toString(),
        description: inProgressTickets > 0 ? 'Being worked on' : 'None active',
        icon: Clock,
        trend: 'neutral' as const,
        color: 'text-yellow-600',
        bgColor: 'bg-yellow-50',
      },
      {
        title: 'Resolved',
        value: resolvedTickets.toString(),
        description: `${Math.round((resolvedTickets / Math.max(tickets.length, 1)) * 100)}% of total`,
        icon: CheckCircle2,
        trend: 'neutral' as const,
        color: 'text-green-600',
        bgColor: 'bg-green-50',
      },
      {
        title: 'High Priority',
        value: highPriorityTickets.toString(),
        description: highPriorityTickets > 0 ? 'Requires attention' : 'All good',
        icon: AlertCircle,
        trend: 'neutral' as const,
        color: 'text-red-600',
        bgColor: 'bg-red-50',
      },
    ];
  }, [tickets]);

  const quickActions = [
    {
      title: 'New Ticket',
      description: 'Create a support ticket',
      icon: TicketIcon,
      href: '/tickets/new',
    },
    {
      title: 'View All Tickets',
      description: 'Browse and manage tickets',
      icon: BarChart3,
      href: '/tickets',
    },
    {
      title: 'Knowledge Base',
      description: 'Browse articles',
      icon: BookOpen,
      href: '/knowledge-base',
    },
    {
      title: 'Team Chat',
      description: 'Collaborate with team',
      icon: MessageSquare,
      href: '/chat',
    },
  ];

  return (
    <AuthGuard>
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100">
        {/* Main Content */}
        <main className="max-w-7xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
          {/* Welcome Section */}
          <div className="mb-8">
            <h2 className="text-3xl font-bold text-gray-900 mb-2">
              Welcome back, {user?.firstName}! 👋
            </h2>
            <p className="text-gray-600">
              Here's what's happening with your help desk today
            </p>
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            {stats.map((stat) => (
              <Card key={stat.title} className="hover:shadow-lg transition-shadow">
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium">
                    {stat.title}
                  </CardTitle>
                  <div className={`${stat.bgColor} ${stat.color} p-2 rounded-lg`}>
                    <stat.icon className="h-4 w-4" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{stat.value}</div>
                  <p className="text-xs text-muted-foreground mt-1">
                    {stat.description}
                  </p>
                </CardContent>
              </Card>
            ))}
          </div>

          {/* Quick Actions */}
          <Card className="mb-8">
            <CardHeader>
              <CardTitle>Quick Actions</CardTitle>
              <CardDescription>
                Common tasks to get you started
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                {quickActions.map((action) => (
                  <Link
                    key={action.title}
                    href={action.href}
                    className="flex flex-col items-start p-4 rounded-lg border border-gray-200 hover:border-primary hover:bg-blue-50 transition-all group"
                  >
                    <action.icon className="h-8 w-8 text-primary mb-3 group-hover:scale-110 transition-transform" />
                    <h3 className="font-semibold text-sm mb-1">{action.title}</h3>
                    <p className="text-xs text-muted-foreground">{action.description}</p>
                  </Link>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Recent Tickets */}
          <div className="grid grid-cols-1 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Recent Tickets</CardTitle>
                <CardDescription>
                  Your latest support tickets
                </CardDescription>
              </CardHeader>
              <CardContent>
                {isLoading ? (
                  <div className="flex items-center justify-center py-8">
                    <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
                  </div>
                ) : tickets && tickets.length > 0 ? (
                  <div className="space-y-4">
                    {tickets.slice(0, 5).map((ticket) => (
                      <div key={ticket.id} className="flex items-start gap-4 pb-4 border-b last:border-0">
                        <div className={`p-2 rounded-full ${
                          ticket.status === 'Open' ? 'bg-blue-100' :
                          ticket.status === 'InProgress' ? 'bg-yellow-100' :
                          ticket.status === 'Resolved' ? 'bg-green-100' :
                          'bg-gray-100'
                        }`}>
                          <TicketIcon className={`h-4 w-4 ${
                            ticket.status === 'Open' ? 'text-blue-600' :
                            ticket.status === 'InProgress' ? 'text-yellow-600' :
                            ticket.status === 'Resolved' ? 'text-green-600' :
                            'text-gray-600'
                          }`} />
                        </div>
                        <div className="flex-1 space-y-1">
                          <p className="text-sm font-medium">
                            {ticket.title}
                          </p>
                          <p className="text-xs text-muted-foreground">
                            {ticket.ticketNumber} • {ticket.status} • {ticket.priority} priority
                          </p>
                          <p className="text-xs text-gray-400">
                            Created {new Date(ticket.createdAt).toLocaleDateString()}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="text-center py-8">
                    <TicketIcon className="h-12 w-12 mx-auto text-gray-300 mb-3" />
                    <p className="text-sm text-muted-foreground">No tickets yet</p>
                    <p className="text-xs text-gray-400 mt-1">Create your first ticket to get started</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        </main>
      </div>
    </AuthGuard>
  );
}
