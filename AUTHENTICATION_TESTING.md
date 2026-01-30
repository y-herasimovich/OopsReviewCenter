# Authentication Testing Guide

## Architecture Overview

OopsReviewCenter now uses a **pure authentication service** design:

- **OopsReviewCenterAA**: Pure application service with no HTTP dependencies
  - Performs DB lookups and password verification only
  - Returns `AuthResult` with user data
  - Provides role permission helpers: `CanView()`, `CanEdit()`, `IsAdminFull()`
  
- **Cookie Authentication**: Standard ASP.NET Core cookie authentication
  - Login page calls `AuthenticateAsync()` and creates claims on success
  - Uses `CookieAuthenticationDefaults.AuthenticationScheme`
  - 8-hour sliding expiration

## Setup

1. Database migrations are applied automatically on first run

2. Seed test data:
   ```bash
   # The database is created in the project root directory
   sqlite3 oopsreviewcenter.db < scripts/seed-test-data.sql
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

## Test Credentials

The following test users are available (from `scripts/seed-test-data.sql`):

| Username | Role              | Permissions                          |
|----------|-------------------|--------------------------------------|
| admin    | Administrator     | Full access (AdminFullAccess)        |
| jsmith   | Incident Manager  | Full access (AdminFullAccess)        |
| bwilson  | Developer         | Edit operations data (CanEditOpsData)|
| viewer   | Viewer            | Read-only access (CanViewOpsData)    |

## Test Scenarios

### 1. Login/Logout Flow
1. Navigate to `http://localhost:5000` (or your configured port)
2. You should be redirected to `/login`
3. Enter test credentials (e.g., `admin` / `Admin123!`)
4. Click "Login"
5. You should be redirected to the dashboard
6. Verify user info and role badge appear in top-right corner
7. Click "Sign Out"
8. You should be redirected back to `/login`

### 2. Administrator Access
**Login as:** `admin`

**Verify:**
- ✅ Can access Dashboard (/)
- ✅ Can access Incidents (/incidents)
- ✅ Can see "New Incident" button
- ✅ Can create new incidents (/incidents/new)
- ✅ Can access Action Items (/actions)
- ✅ Can see "Complete" buttons on action items
- ✅ Can access Admin Panel link in header
- ✅ Can access Templates (/admin/templates)
- ✅ Can access Tags (/admin/tags)

### 3. Developer Access

**Verify:**
- ✅ Can access Dashboard (/)
- ✅ Can access Incidents (/incidents)
- ✅ Can see "New Incident" button
- ✅ Can create new incidents (/incidents/new)
- ✅ Can access Action Items (/actions)
- ✅ Can see "Complete" buttons on action items
- ❌ Admin Panel link should NOT be visible
- ❌ Cannot access /admin/templates (should get Access Denied)
- ❌ Cannot access /admin/tags (should get Access Denied)

### 4. Viewer (Read-Only) Access

**Verify:**
- ✅ Can access Dashboard (/)
- ✅ Can access Incidents (/incidents)
- ❌ "New Incident" button should NOT be visible
- ❌ Cannot access /incidents/new (should get Access Denied)
- ✅ Can access Incident Details (/incidents/{id})
- ✅ Can access Action Items (/actions)
- ❌ "Complete" buttons should NOT be visible on action items
- ❌ Admin Panel link should NOT be visible
- ❌ Cannot access /admin/* (should get Access Denied)

### 5. Inactive User
To test inactive user functionality:

1. Update a user to be inactive:
   ```sql
   UPDATE Users SET IsActive = 0 WHERE Username = 'viewer';
   ```

2. Try to login as `viewer` / `Viewer123!`
3. Should see error: "Your account is not active. Please contact an administrator."

4. Reactivate the user:
   ```sql
   UPDATE Users SET IsActive = 1 WHERE Username = 'viewer';
   ```

### 6. Invalid Credentials
1. Navigate to `/login`
2. Enter invalid username or password
3. Should see error: "Invalid username or password."
4. User should remain on login page

## Security Verification

### Password Hashing
- Passwords are hashed using PBKDF2 with 600,000 iterations
- 128-bit salt (16 bytes, base64-encoded)
- 256-bit hash (32 bytes, base64-encoded)
- Constant-time comparison prevents timing attacks

### Authentication Flow
1. User submits credentials to `/login`
2. `OopsReviewCenterAA.AuthenticateAsync()` validates credentials
   - Loads user from database with EF Core Include(Role)
   - Checks IsActive status
   - Verifies password using PasswordHasher
   - Returns AuthResult with UserId, RoleName, Username, FullName
3. Login page creates claims (NameIdentifier, Name, Role, FullName)
4. ASP.NET Core cookie authentication signs user in
5. Authorization policies enforce role-based access

### Session Management
- Cookie-based authentication via ASP.NET Core
- 8-hour sliding expiration
- HttpOnly cookies (automatic with cookie auth)
- Secure cookies in HTTPS (automatic)
- SameSite=Lax for CSRF protection

### Authorization Enforcement
- **Policies**: AdminFullAccess, CanEditOpsData, CanViewOpsData
- **Client-side**: UI controls hidden based on role using `<AuthorizeView>`
- **Server-side**: Protected pages use `@attribute [Authorize(Policy="...")]`
- **Permission Helpers**: `IsAdminFull()`, `CanEdit()`, `CanView()` available for code-level checks

## Troubleshooting

### Database Issues
If you see "no such table" errors:
```bash
cd OopsReviewCenter
dotnet ef migrations script --output migration.sql
sqlite3 App_Data/oopsreviewcenter.db < migration.sql
sqlite3 App_Data/oopsreviewcenter.db < scripts/seed-test-data.sql
```

### Login Not Working
1. Check that the database has been seeded
2. Verify user exists:
   ```bash
   sqlite3 App_Data/oopsreviewcenter.db "SELECT Username, IsActive FROM Users"
   ```
3. Check application logs for errors

### Authorization Not Working
1. Clear browser cookies
2. Restart the application
3. Check that policies are defined in `Program.cs`
4. Verify role names match exactly (case-sensitive)

## Production Deployment Notes

1. **Change default passwords** - Generate new secure passwords
2. **Use HTTPS** - Ensure SSL/TLS is configured
3. **Secure cookies** - ASP.NET Core automatically uses secure cookies over HTTPS
4. **Rate limiting** - Consider adding login attempt rate limiting
5. **Audit logging** - Add logging for authentication events
6. **Password policy** - Implement password complexity requirements
7. **Multi-factor authentication** - Consider adding MFA for production
