'use client';

import { useAuthStore } from '@/store/auth-store';
import { useLogout } from '@/hooks/use-auth';
import { useMyTickets } from '@/hooks/use-tickets';
import { AuthGuard } from '@/components/auth-guard';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  TicketIcon,
  Clock,
  CheckCircle2,
  AlertCircle,
  Search,
  Bell,
  Settings,
  LogOut,
  User,
  BarChart3,
  MessageSquare,
  BookOpen,
  Loader2,
} from 'lucide-react';
import { useMemo } from 'react';

export default function DashboardPage() {
  const { user } = useAuthStore();
  const logout = useLogout();
  const { data: tickets, isLoading, isError } = useMyTickets();

  const getInitials = (firstName?: string, lastName?: string) => {
    return `${firstName?.[0] || ''}${lastName?.[0] || ''}`.toUpperCase();
  };

  // Calculate real stats from ticket data
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
        {/* Navigation Header */}
        <nav className="bg-white border-b border-gray-200 shadow-sm">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between items-center h-16">
              {/* Logo and Title */}
              <div className="flex items-center gap-3">
                <div className="bg-primary text-primary-foreground p-2 rounded-lg">
                  <TicketIcon className="h-6 w-6" />
                </div>
                <div>
                  <h1 className="text-xl font-bold text-gray-900">Hickory</h1>
                  <p className="text-xs text-gray-500">Help Desk</p>
                </div>
              </div>

              {/* Search Bar (Desktop) */}
              <div className="hidden md:flex flex-1 max-w-md mx-8">
                <div className="relative w-full">
                  <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                  <input
                    type="text"
                    placeholder="Search tickets, users, articles..."
                    className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
                  />
                </div>
              </div>

              {/* Right Side Actions */}
              <div className="flex items-center gap-4">
                <Button variant="ghost" size="icon" className="relative">
                  <Bell className="h-5 w-5" />
                  <span className="absolute top-1 right-1 h-2 w-2 bg-red-600 rounded-full"></span>
                </Button>

                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" className="relative h-10 w-10 rounded-full">
                      <Avatar>
                        <AvatarFallback className="bg-primary text-primary-foreground">
                          {getInitials(user?.firstName, user?.lastName)}
                        </AvatarFallback>
                      </Avatar>
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent className="w-56" align="end" forceMount>
                    <DropdownMenuLabel className="font-normal">
                      <div className="flex flex-col space-y-1">
                        <p className="text-sm font-medium leading-none">
                          {user?.firstName} {user?.lastName}
                        </p>
                        <p className="text-xs leading-none text-muted-foreground">
                          {user?.email}
                        </p>
                      </div>
                    </DropdownMenuLabel>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem>
                      <User className="mr-2 h-4 w-4" />
                      <span>Profile</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem>
                      <Settings className="mr-2 h-4 w-4" />
                      <span>Settings</span>
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={logout} className="text-red-600">
                      <LogOut className="mr-2 h-4 w-4" />
                      <span>Log out</span>
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </div>
            </div>
          </div>
        </nav>

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
                  <button
                    key={action.title}
                    className="flex flex-col items-start p-4 rounded-lg border border-gray-200 hover:border-primary hover:bg-blue-50 transition-all group"
                  >
                    <action.icon className="h-8 w-8 text-primary mb-3 group-hover:scale-110 transition-transform" />
                    <h3 className="font-semibold text-sm mb-1">{action.title}</h3>
                    <p className="text-xs text-muted-foreground">{action.description}</p>
                  </button>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Recent Activity and User Info Grid */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Recent Activity */}
            <Card className="lg:col-span-2">
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

            {/* User Info */}
            <Card>
              <CardHeader>
                <CardTitle>Your Profile</CardTitle>
                <CardDescription>Account information</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center gap-4">
                  <Avatar className="h-16 w-16">
                    <AvatarFallback className="bg-primary text-primary-foreground text-xl">
                      {getInitials(user?.firstName, user?.lastName)}
                    </AvatarFallback>
                  </Avatar>
                  <div>
                    <p className="font-semibold">
                      {user?.firstName} {user?.lastName}
                    </p>
                    <p className="text-sm text-muted-foreground capitalize">
                      {user?.role}
                    </p>
                  </div>
                </div>
                
                <div className="space-y-2 pt-4 border-t">
                  <div>
                    <p className="text-xs text-muted-foreground">Email</p>
                    <p className="text-sm">{user?.email}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">User ID</p>
                    <p className="text-sm font-mono text-xs">{user?.userId}</p>
                  </div>
                </div>

                <Button className="w-full mt-4" variant="outline">
                  <User className="mr-2 h-4 w-4" />
                  Edit Profile
                </Button>
              </CardContent>
            </Card>
          </div>
        </main>
      </div>
    </AuthGuard>
  );
}
