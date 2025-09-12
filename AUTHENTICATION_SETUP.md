# Microsoft Entra (Azure AD) Authentication Setup

This document provides step-by-step instructions for setting up Microsoft Entra (Azure AD) authentication in the Andy Agentic application.

## Prerequisites

- Azure AD tenant
- Admin access to Azure portal
- .NET 9 SDK
- Angular 16 CLI
- MySQL database

## Backend Setup (.NET 9)

### 1. Azure AD App Registration

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Fill in the details:
   - **Name**: `Andy Agentic API`
   - **Supported account types**: `Accounts in this organizational directory only`
   - **Redirect URI**: Leave blank for now
5. Click **Register**
6. Note down the **Application (client) ID** and **Directory (tenant) ID**

### 2. Configure API Permissions

1. In your app registration, go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph**
4. Choose **Delegated permissions**
5. Add the following permissions:
   - `User.Read`
   - `openid`
   - `profile`
   - `email`
6. Click **Add permissions**
7. Click **Grant admin consent** (if you have admin rights)

### 3. Create Client Secret (Optional)

1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Add a description and select expiration
4. Click **Add**
5. **Important**: Copy the secret value immediately (it won't be shown again)

### 4. Update Backend Configuration

Update `appsettings.json` with your Azure AD configuration:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourdomain.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "CallbackPath": "/signin-oidc",
    "Scopes": "User.Read"
  }
}
```

### 5. Generate Database Migration

Run the following command to create the database migration:

```bash
cd andy-agentic
dotnet ef migrations add AddUserAuthentication --project src/Andy.Agentic.Infrastructure --startup-project src/Andy.Agentic
```

### 6. Apply Migration

```bash
dotnet ef database update --project src/Andy.Agentic.Infrastructure --startup-project src/Andy.Agentic
```

## Frontend Setup (Angular)

### 1. Azure AD App Registration for Frontend

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Fill in the details:
   - **Name**: `Andy Agentic Web`
   - **Supported account types**: `Accounts in this organizational directory only`
   - **Redirect URI**: `http://localhost:4200` (for development)
5. Click **Register**
6. Note down the **Application (client) ID**

### 2. Configure Frontend Redirect URIs

1. In your frontend app registration, go to **Authentication**
2. Under **Single-page application**, add these redirect URIs:
   - `http://localhost:4200` (development)
   - `https://yourdomain.com` (production)
3. Under **Logout URLs**, add:
   - `http://localhost:4200` (development)
   - `https://yourdomain.com` (production)

### 3. Update Frontend Configuration

Update `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost/api',
  azureAd: {
    clientId: 'your-frontend-client-id',
    tenantId: 'your-tenant-id',
    redirectUri: 'http://localhost:4200'
  }
};
```

Update `src/environments/environment.prod.ts`:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://yourdomain.com/api',
  azureAd: {
    clientId: 'your-production-client-id',
    tenantId: 'your-tenant-id',
    redirectUri: 'https://yourdomain.com'
  }
};
```

## Testing the Implementation

### 1. Start the Backend

```bash
cd andy-agentic
dotnet run --project src/Andy.Agentic
```

### 2. Start the Frontend

```bash
cd andy-agentic-web
ng serve
```

### 3. Test Authentication Flow

1. Navigate to `http://localhost:4200`
2. You should be redirected to the login page
3. Click "Sign in with Microsoft"
4. Complete the Microsoft login process
5. You should be redirected back to the application
6. Verify that user information is displayed in the sidebar

## API Endpoints

The following authentication endpoints are available:

- `GET /api/auth/me` - Get current user information
- `POST /api/auth/sync` - Sync user with backend after login
- `GET /api/auth/status` - Check authentication status

## Security Considerations

1. **HTTPS**: Always use HTTPS in production
2. **CORS**: Configure CORS properly for your domains
3. **Token Validation**: JWT tokens are automatically validated by the backend
4. **User Data**: User data is automatically created/updated on first login
5. **Session Management**: Tokens are stored securely in browser storage

## Troubleshooting

### Common Issues

1. **CORS Errors**: Ensure CORS is configured correctly in `Startup.cs`
2. **Token Validation Errors**: Check Azure AD configuration
3. **Redirect URI Mismatch**: Verify redirect URIs in Azure AD
4. **Database Errors**: Ensure migration was applied successfully

### Debug Steps

1. Check browser console for errors
2. Check backend logs for authentication errors
3. Verify Azure AD app registration configuration
4. Test API endpoints directly using tools like Postman

## Extending the Implementation

### Adding More User Claims

To add more user information from Azure AD:

1. Update the `UserEntity` in the backend
2. Modify the `CreateOrUpdateUserAsync` method in `AuthService`
3. Update the frontend `User` interface
4. Add the new claims to your Azure AD app registration

### Adding Role-Based Authorization

1. Add roles to the `UserEntity`
2. Update the authentication service to handle roles
3. Add role-based authorization attributes to controllers
4. Implement role-based guards in Angular

### Supporting Multiple Identity Providers

The architecture is designed to be extensible. To add support for other providers:

1. Create new authentication services implementing `IAuthService`
2. Add provider-specific configuration
3. Update the frontend to support multiple login methods
4. Add provider selection logic

## Support

For issues or questions:

1. Check the troubleshooting section above
2. Review Azure AD documentation
3. Check the application logs
4. Contact the development team


