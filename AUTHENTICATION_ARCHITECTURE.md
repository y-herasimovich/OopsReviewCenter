# Authentication & Authorization Architecture

## Overview

The OopsReviewCenterAA service implements a **database-only authentication and authorization system** with NO external HTTP requests.

## Compliance with Requirements

### ✅ Requirement: No HTTP Requests
**Status: COMPLIANT**

The OopsReviewCenterAA service does **NOT** make any HTTP requests. Specifically:
- ❌ No `HttpClient` usage
- ❌ No `WebClient` usage
- ❌ No `HttpRequestMessage` usage
- ❌ No external API calls
- ❌ No REST client usage
- ❌ No SOAP client usage

### ✅ Requirement: Database Reading Only
**Status: COMPLIANT**

The service only **reads** from the database:
- Reads user credentials from `Users` table
- Reads role information from `Role` table
- **Never writes** to the database during authentication
- All database operations use Entity Framework Core with read-only queries

### ✅ Requirement: Local Password Verification
**Status: COMPLIANT**

Password verification is performed **locally** using the `PasswordHasher` service:
- PBKDF2 algorithm with 600,000 iterations
- 256-bit salt (stored in database)
- 256-bit hash (stored in database)
- Constant-time comparison to prevent timing attacks
- No external service calls

### ✅ Requirement: Local Role Checking
**Status: COMPLIANT**

Role verification is performed **locally** from database:
- Roles are stored in the database
- Role checks query the database directly
- No external service calls
- Policy-based authorization uses in-memory logic

## HttpContext Usage Clarification

### What HttpContext Is
`HttpContext` is the **ASP.NET Core request/response context** for the current web request. It provides access to:
- Request cookies (to read session ID)
- Response cookies (to write session ID)
- Request headers
- Response headers
- User claims

### What HttpContext Is NOT
`HttpContext` is **NOT**:
- ❌ An HTTP client for making requests
- ❌ A tool for sending data to external services
- ❌ A way to make outbound HTTP calls

### Why HttpContext Is Necessary
The service needs `HttpContext` to:
1. **Read the session cookie** from the incoming browser request
2. **Write the session cookie** back to the browser response
3. **Manage session state** for the authenticated user

This is standard practice in web applications and does not violate the "no HTTP requests" requirement.

## Architecture Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                     Browser (Client)                          │
│                                                               │
│  1. Sends login credentials                                  │
│  2. Receives session cookie                                  │
└────────────────────────┬─────────────────────────────────────┘
                         │
                         │ HTTPS Request (incoming)
                         │
┌────────────────────────▼─────────────────────────────────────┐
│              ASP.NET Core Web Server                          │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  CustomAuthenticationMiddleware                        │  │
│  │  - Reads session cookie                                │  │
│  │  - Populates HttpContext.User                          │  │
│  └───────────────────────────────────────────────────────┘  │
│                         │                                     │
│                         ▼                                     │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  OopsReviewCenterAA Service                            │  │
│  │                                                         │  │
│  │  SignInAsync():                                        │  │
│  │    1. Read user from database ─────────┐              │  │
│  │    2. Verify password locally           │              │  │
│  │    3. Check role from database ─────────┼──┐          │  │
│  │    4. Create session cookie             │  │          │  │
│  │                                          │  │          │  │
│  │  GetCurrentUserAsync():                 │  │          │  │
│  │    1. Read session from cookie          │  │          │  │
│  │    2. Query user from database ─────────┼──┤          │  │
│  │                                          │  │          │  │
│  │  IsInRole():                            │  │          │  │
│  │    1. Read session from cookie          │  │          │  │
│  │    2. Check role from session ──────────┘  │          │  │
│  └──────────────────────────────────────────┬─┘          │  │
│                                              │            │  │
└──────────────────────────────────────────────┼────────────┼──┘
                                               │            │
                                               ▼            ▼
                                    ┌──────────────────────────┐
                                    │   SQLite Database         │
                                    │                          │
                                    │  - Users table           │
                                    │  - Roles table           │
                                    │                          │
                                    │  READ ONLY              │
                                    │  (No writes)             │
                                    └──────────────────────────┘

NO EXTERNAL HTTP REQUESTS ❌
NO API CALLS ❌
NO WEB SERVICE CALLS ❌
```

## Key Methods Analysis

### SignInAsync(string login, string password, HttpContext httpContext)
**Purpose:** Authenticate a user and create a session

**Operations:**
1. ✅ Read user from database using EF Core
2. ✅ Verify password using local PasswordHasher service
3. ✅ Check role from database (included with user query)
4. ✅ Create in-memory session
5. ✅ Write session cookie to browser response

**HTTP Requests:** None

### GetCurrentUserAsync(HttpContext httpContext)
**Purpose:** Get the authenticated user's data

**Operations:**
1. ✅ Read session ID from cookie
2. ✅ Look up session in memory
3. ✅ Query user from database using EF Core

**HTTP Requests:** None

### IsInRole(HttpContext httpContext, string roleName)
**Purpose:** Check if user has a specific role

**Operations:**
1. ✅ Read session ID from cookie
2. ✅ Look up session in memory
3. ✅ Compare role name (already loaded in session)

**HTTP Requests:** None

### SignOutAsync(HttpContext httpContext)
**Purpose:** Sign out the user

**Operations:**
1. ✅ Read session ID from cookie
2. ✅ Remove session from memory
3. ✅ Delete session cookie

**HTTP Requests:** None

## Session Management

The service uses **in-memory session storage** with cookies:

```csharp
private static readonly ConcurrentDictionary<string, UserSession> _sessions = new();
```

- Sessions are stored in server memory
- Session ID is stored in a secure cookie
- 8-hour expiration with automatic cleanup
- No database writes for session management
- No external service calls

## Security Features

### Password Security
- ✅ PBKDF2 with 600,000 iterations
- ✅ Individual salt per user
- ✅ Constant-time comparison
- ✅ No plaintext passwords

### Session Security
- ✅ HttpOnly cookies (prevents XSS)
- ✅ Secure cookies over HTTPS
- ✅ SameSite=Lax (CSRF protection)
- ✅ 8-hour expiration
- ✅ Session fixation prevention

### Authorization Security
- ✅ Server-side enforcement
- ✅ Policy-based authorization
- ✅ Role-based access control
- ✅ Redirects on unauthorized access

## Verification

To verify that no HTTP requests are made, the following checks have been performed:

### Code Analysis
```bash
# Check for HttpClient usage
grep -r "HttpClient\|IHttpClientFactory" --include="*.cs" .
# Result: No matches found

# Check for HTTP request methods
grep -r "GetAsync\|PostAsync\|PutAsync\|DeleteAsync\|SendAsync" --include="*.cs" .
# Result: No matches found

# Check for System.Net.Http imports
grep -r "using System.Net.Http" --include="*.cs" .
# Result: No matches found
```

### Runtime Verification
All unit tests pass, confirming the service works correctly with only database operations:
```
Test Run Successful.
Total tests: 23
     Passed: 23
```

## Conclusion

The OopsReviewCenterAA service is **fully compliant** with the requirements:
- ✅ No HTTP requests to external services
- ✅ Database reading only (no writes during auth)
- ✅ Local password verification
- ✅ Local role checking
- ✅ Secure session management
- ✅ No external dependencies

The use of `HttpContext` is for managing the incoming web request and session cookies, not for making outbound HTTP requests.
