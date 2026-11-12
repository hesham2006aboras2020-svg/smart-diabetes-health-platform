# Firebase Push Notifications

The backend supports Firebase Cloud Messaging (FCM) for push delivery. Push delivery is disabled by default.

## Enable locally

Set the following configuration:

```json
"Firebase": {
  "Enabled": true
}
```

Provide Firebase Admin credentials through Application Default Credentials. Do not commit a Firebase service-account JSON file or private key to the repository.

For local development, configure the `GOOGLE_APPLICATION_CREDENTIALS` environment variable to point to the Firebase service-account JSON file on the developer machine.

## Runtime behavior

- The API keeps an in-app `Notification` record regardless of push delivery.
- When FCM is enabled, the backend sends the notification to the user's registered device tokens.
- A successful push marks the notification as sent.
- An FCM `Unregistered` response removes the stale device token.
- If FCM is disabled or no device token exists, the in-app notification remains available through the notifications API.

The Firebase Admin SDK currently recommends Firebase Installation IDs for new integrations; this project currently stores client registration values in the existing `DeviceToken.Token` field to preserve the current API contract. The client integration should be reviewed when the Flutter FCM layer is implemented.
