'use client';

import { useEffect, useState } from 'react';
import { useNotificationPreferences, useUpdateNotificationPreferences } from '@/lib/queries/notification-preferences';

export default function NotificationSettingsPage() {
  const { data: preferences, isLoading } = useNotificationPreferences();
  const updatePreferences = useUpdateNotificationPreferences();

  const [formData, setFormData] = useState({
    emailEnabled: true,
    inAppEnabled: true,
    webhookEnabled: false,
    notifyOnTicketCreated: true,
    notifyOnTicketUpdated: true,
    notifyOnTicketAssigned: true,
    notifyOnCommentAdded: true,
    webhookUrl: '',
    webhookSecret: '',
  });

  // Update form data when preferences load
  useEffect(() => {
    if (preferences) {
      setFormData({
        emailEnabled: preferences.emailEnabled,
        inAppEnabled: preferences.inAppEnabled,
        webhookEnabled: preferences.webhookEnabled,
        notifyOnTicketCreated: preferences.notifyOnTicketCreated,
        notifyOnTicketUpdated: preferences.notifyOnTicketUpdated,
        notifyOnTicketAssigned: preferences.notifyOnTicketAssigned,
        notifyOnCommentAdded: preferences.notifyOnCommentAdded,
        webhookUrl: preferences.webhookUrl ?? '',
        webhookSecret: preferences.webhookSecret ?? '',
      });
    }
  }, [preferences]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await updatePreferences.mutateAsync(formData);
  };

  if (isLoading) {
    return (
      <div className="max-w-4xl mx-auto p-6">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded w-1/4 mb-4"></div>
          <div className="h-4 bg-gray-200 rounded w-1/2 mb-8"></div>
          <div className="space-y-4">
            <div className="h-16 bg-gray-200 rounded"></div>
            <div className="h-16 bg-gray-200 rounded"></div>
            <div className="h-16 bg-gray-200 rounded"></div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-2xl font-bold text-gray-900 mb-2">Notification Preferences</h1>
      <p className="text-gray-600 mb-8">
        Customize how and when you receive notifications about ticket activity.
      </p>

      <form onSubmit={handleSubmit} className="space-y-8">
        {/* Notification Channels */}
        <section className="bg-white rounded-lg border border-gray-200 p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Notification Channels</h2>
          <p className="text-sm text-gray-600 mb-6">
            Choose how you want to receive notifications.
          </p>

          <div className="space-y-4">
            {/* Email */}
            <div className="flex items-center justify-between">
              <div>
                <label htmlFor="emailEnabled" className="font-medium text-gray-900">
                  Email Notifications
                </label>
                <p className="text-sm text-gray-500">
                  Receive notifications via email
                </p>
              </div>
              <button
                type="button"
                role="switch"
                aria-checked={formData.emailEnabled}
                onClick={() => setFormData({ ...formData, emailEnabled: !formData.emailEnabled })}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  formData.emailEnabled ? 'bg-blue-600' : 'bg-gray-200'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    formData.emailEnabled ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </div>

            {/* In-App */}
            <div className="flex items-center justify-between">
              <div>
                <label htmlFor="inAppEnabled" className="font-medium text-gray-900">
                  In-App Notifications
                </label>
                <p className="text-sm text-gray-500">
                  Show real-time notifications in the app
                </p>
              </div>
              <button
                type="button"
                role="switch"
                aria-checked={formData.inAppEnabled}
                onClick={() => setFormData({ ...formData, inAppEnabled: !formData.inAppEnabled })}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  formData.inAppEnabled ? 'bg-blue-600' : 'bg-gray-200'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    formData.inAppEnabled ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </div>

            {/* Webhook */}
            <div className="flex items-center justify-between">
              <div>
                <label htmlFor="webhookEnabled" className="font-medium text-gray-900">
                  Webhook Notifications
                </label>
                <p className="text-sm text-gray-500">
                  Send notifications to external systems
                </p>
              </div>
              <button
                type="button"
                role="switch"
                aria-checked={formData.webhookEnabled}
                onClick={() => setFormData({ ...formData, webhookEnabled: !formData.webhookEnabled })}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  formData.webhookEnabled ? 'bg-blue-600' : 'bg-gray-200'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    formData.webhookEnabled ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </div>
          </div>

          {/* Webhook Configuration */}
          {formData.webhookEnabled && (
            <div className="mt-6 pt-6 border-t border-gray-200 space-y-4">
              <div>
                <label htmlFor="webhookUrl" className="block text-sm font-medium text-gray-700 mb-1">
                  Webhook URL
                </label>
                <input
                  type="url"
                  id="webhookUrl"
                  value={formData.webhookUrl}
                  onChange={(e) => setFormData({ ...formData, webhookUrl: e.target.value })}
                  placeholder="https://example.com/webhooks/notifications"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label htmlFor="webhookSecret" className="block text-sm font-medium text-gray-700 mb-1">
                  Webhook Secret (Optional)
                </label>
                <input
                  type="password"
                  id="webhookSecret"
                  value={formData.webhookSecret}
                  onChange={(e) => setFormData({ ...formData, webhookSecret: e.target.value })}
                  placeholder="Enter secret for request validation"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Used to sign webhook payloads for verification
                </p>
              </div>
            </div>
          )}
        </section>

        {/* Event Types */}
        <section className="bg-white rounded-lg border border-gray-200 p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Event Types</h2>
          <p className="text-sm text-gray-600 mb-6">
            Choose which events trigger notifications.
          </p>

          <div className="space-y-4">
            {/* Ticket Created */}
            <div className="flex items-center justify-between">
              <div>
                <label htmlFor="notifyOnTicketCreated" className="font-medium text-gray-900">
                  Ticket Created
                </label>
                <p className="text-sm text-gray-500">
                  When a new ticket is created
                </p>
              </div>
              <button
                type="button"
                role="switch"
                aria-checked={formData.notifyOnTicketCreated}
                onClick={() => setFormData({ ...formData, notifyOnTicketCreated: !formData.notifyOnTicketCreated })}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  formData.notifyOnTicketCreated ? 'bg-blue-600' : 'bg-gray-200'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    formData.notifyOnTicketCreated ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </div>

            {/* Ticket Updated */}
            <div className="flex items-center justify-between">
              <div>
                <label htmlFor="notifyOnTicketUpdated" className="font-medium text-gray-900">
                  Ticket Updated
                </label>
                <p className="text-sm text-gray-500">
                  When ticket details are modified
                </p>
              </div>
              <button
                type="button"
                role="switch"
                aria-checked={formData.notifyOnTicketUpdated}
                onClick={() => setFormData({ ...formData, notifyOnTicketUpdated: !formData.notifyOnTicketUpdated })}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  formData.notifyOnTicketUpdated ? 'bg-blue-600' : 'bg-gray-200'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    formData.notifyOnTicketUpdated ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </div>

            {/* Ticket Assigned */}
            <div className="flex items-center justify-between">
              <div>
                <label htmlFor="notifyOnTicketAssigned" className="font-medium text-gray-900">
                  Ticket Assigned
                </label>
                <p className="text-sm text-gray-500">
                  When you are assigned to a ticket
                </p>
              </div>
              <button
                type="button"
                role="switch"
                aria-checked={formData.notifyOnTicketAssigned}
                onClick={() => setFormData({ ...formData, notifyOnTicketAssigned: !formData.notifyOnTicketAssigned })}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  formData.notifyOnTicketAssigned ? 'bg-blue-600' : 'bg-gray-200'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    formData.notifyOnTicketAssigned ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </div>

            {/* Comment Added */}
            <div className="flex items-center justify-between">
              <div>
                <label htmlFor="notifyOnCommentAdded" className="font-medium text-gray-900">
                  Comment Added
                </label>
                <p className="text-sm text-gray-500">
                  When someone comments on your tickets
                </p>
              </div>
              <button
                type="button"
                role="switch"
                aria-checked={formData.notifyOnCommentAdded}
                onClick={() => setFormData({ ...formData, notifyOnCommentAdded: !formData.notifyOnCommentAdded })}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  formData.notifyOnCommentAdded ? 'bg-blue-600' : 'bg-gray-200'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    formData.notifyOnCommentAdded ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </div>
          </div>
        </section>

        {/* Save Button */}
        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={() => window.history.back()}
            className="px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={updatePreferences.isPending}
            className="px-4 py-2 text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {updatePreferences.isPending ? 'Saving...' : 'Save Preferences'}
          </button>
        </div>

        {/* Success/Error Messages */}
        {updatePreferences.isSuccess && (
          <div className="p-4 bg-green-50 border border-green-200 rounded-md">
            <p className="text-sm text-green-800">
              Notification preferences saved successfully!
            </p>
          </div>
        )}
        {updatePreferences.isError && (
          <div className="p-4 bg-red-50 border border-red-200 rounded-md">
            <p className="text-sm text-red-800">
              Failed to save preferences. Please try again.
            </p>
          </div>
        )}
      </form>
    </div>
  );
}
