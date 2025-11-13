# Identity Service

**Central Authentication and Identity Management Service**

The Identity Service is the central source of truth for authentication across all microservices and external clients. It provides JWT-based authentication, user management, and token validation services with enterprise-grade security and compliance features.

## Overview

### Key Features

- **Central Authentication**: Single source of truth for all authentication across the platform
- **OAuth2/OIDC Support**: Full OAuth2 authorization server with OpenID Connect endpoints
  - Authorization Code Flow with PKCE support
  - Client Credentials Flow
  - Refresh Token Flow
  - OIDC Discovery endpoint
  - Token Introspection (RFC 7662)
  - Token Revocation (RFC 7009)
  - JWKS endpoint
- **OAuth2 Client Management**: Full CRUD operations for OAuth2/OIDC clients
  - Client registration and management
  - Client secret rotation
  - Redirect URI validation
  - Grant type restrictions
  - PKCE enforcement
- **Multi-Factor Authentication (MFA)**: Multiple MFA providers with strategy pattern
  - TOTP (Time-based One-Time Password) with QR code enrollment
  - SMS MFA provider
  - Email MFA provider
  - Backup codes for account recovery
- **WebAuthn/FIDO2**: Passwordless authentication support
  - WebAuthn credential registration
  - WebAuthn authentication challenges
  - FIDO2 standard compliance
- **JWT Token Management**: Advanced token handling
  - Access and refresh token generation and validation
  - JWT key rotation with signing key management
  - Token Provider Factory (JWT and Reference tokens)
  - Token introspection and revocation
- **User Management**: Full CRUD operations for users with pagination and search
- **Role-Based Access Control (RBAC)**: Role and permission management
- **Permission Management**: Granular permissions with resource-action model
- **Session Management**: Active session tracking and revocation
- **Reliability Features**:
  - Outbox Pattern for reliable event publishing
  - Idempotency support for all commands
  - Grant Type Handler Strategy Pattern
- **Security Features**: 
  - Account locking after failed login attempts
  - Password hashing with BCrypt
  - Global and per-endpoint rate limiting
  - Correlation ID tracking
  - CORS support
  - MFA support (TOTP, SMS, Email)
  - WebAuthn/FIDO2 passwordless authentication
- **Kubernetes Ready**: Health checks, structured logging, and observability
- **Prometheus Metrics**: Comprehensive metrics exposure for monitoring
- **SOC2/Compliance Ready**: Audit trails, secure token handling, and comprehensive logging

### Architecture

The service follows **Clean Architecture** principles with clear separation of concerns:

```
IdentityService/
├── Domain/              # Business logic and entities
├── Application/         # Use cases and orchestration
├── Infrastructure/      # External concerns (DB, services)
└── API/                # Presentation layer
```

## API Endpoints

### Authentication

#### Register User
```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "username": "johndoe",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "username": "johndoe",
    "message": "User registered successfully. Please verify your email."
  },
  "message": "User registered successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Login
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "deviceId": "device-123",
  "deviceName": "Chrome Browser",
  "ipAddress": "192.168.1.1"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "expiresAt": "2024-01-15T11:30:00Z",
    "tokenType": "Bearer"
  },
  "message": "Login successful",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Refresh Token
```http
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "refreshToken": "base64-encoded-refresh-token"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "new-base64-encoded-refresh-token",
    "expiresAt": "2024-01-15T11:30:00Z",
    "tokenType": "Bearer"
  },
  "message": "Token refreshed successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Validate Token
```http
POST /api/v1/auth/validate
Content-Type: application/json

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "isValid": true,
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "username": "johndoe",
    "roles": ["User", "Admin"],
    "claims": {
      "sub": "123e4567-e89b-12d3-a456-426614174000",
      "email": "user@example.com",
      "name": "johndoe",
      "role": "User",
      "role": "Admin"
    }
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Get Current User
```http
GET /api/v1/auth/me
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "username": "johndoe",
    "roles": ["User", "Admin"]
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### OAuth2 / OpenID Connect

#### Authorization Endpoint
```http
GET /api/v1/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&state={state}
Authorization: Bearer {accessToken}
```

**Response (302 Redirect):**
```
{redirectUri}?code={authorizationCode}&state={state}
```

#### Token Endpoint
```http
POST /api/v1/oauth2/token
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code&code={code}&redirect_uri={redirectUri}&client_id={clientId}&code_verifier={codeVerifier}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "refreshToken": "base64-encoded-refresh-token",
  "scope": "openid profile email"
}
```

#### UserInfo Endpoint (OIDC)
```http
GET /api/v1/oauth2/userinfo
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "sub": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "emailVerified": "true",
  "name": "John Doe",
  "givenName": "John",
  "familyName": "Doe",
  "preferredUsername": "johndoe"
}
```

#### OIDC Discovery Endpoint
```http
GET /api/v1/oauth2/.well-known/openid-configuration
```

**Response (200 OK):**
```json
{
  "issuer": "IdentityService",
  "authorization_endpoint": "https://identity-service/api/v1/oauth2/authorize",
  "token_endpoint": "https://identity-service/api/v1/oauth2/token",
  "userinfo_endpoint": "https://identity-service/api/v1/oauth2/userinfo",
  "jwks_uri": "https://identity-service/api/v1/oauth2/.well-known/jwks.json",
  "scopes_supported": ["openid", "profile", "email"],
  "response_types_supported": ["code"],
  "grant_types_supported": ["authorization_code", "client_credentials", "refresh_token"],
  "id_token_signing_alg_values_supported": ["HS256"],
  "subject_types_supported": ["public"],
  "claims_supported": ["sub", "email", "email_verified", "name", "given_name", "family_name", "preferred_username"]
}
```

#### JWKS Endpoint
```http
GET /api/v1/oauth2/.well-known/jwks.json
```

**Response (200 OK):**
```json
{
  "keys": [
    {
      "kty": "oct",
      "use": "sig",
      "alg": "HS256"
    }
  ]
}
```

#### Token Introspection Endpoint (RFC 7662)
```http
POST /api/v1/oauth2/introspect
Content-Type: application/x-www-form-urlencoded

token={accessToken}&token_type_hint=access_token
```

**Response (200 OK):**
```json
{
  "active": true,
  "scope": "openid profile email",
  "client_id": "my-client",
  "username": "johndoe",
  "token_type": "Bearer",
  "exp": 1705320000,
  "iat": 1705316400,
  "sub": "123e4567-e89b-12d3-a456-426614174000",
  "aud": "IdentityService",
  "iss": "IdentityService",
  "jti": "token-id"
}
```

#### Token Revocation Endpoint (RFC 7009)
```http
POST /api/v1/oauth2/revoke
Content-Type: application/x-www-form-urlencoded

token={refreshToken}&token_type_hint=refresh_token
```

**Response (200 OK):**
```
(Empty body - always returns 200 OK per RFC 7009)
```

#### End Session Endpoint (OIDC)
```http
GET /api/v1/oauth2/endsession?post_logout_redirect_uri={uri}&state={state}
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "message": "Session ended successfully"
}
```

### OAuth2 Client Management

#### Create Client
```http
POST /api/v1/clients
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "clientId": "my-client",
  "name": "My Application",
  "secret": "client-secret",
  "redirectUris": ["https://app.example.com/callback"],
  "postLogoutRedirectUris": ["https://app.example.com/logout"],
  "allowedGrantTypes": ["authorization_code", "refresh_token"],
  "accessTokenLifetime": 3600,
  "refreshTokenLifetime": 2592000,
  "requireConsent": true,
  "requirePkce": true
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "clientId": "123e4567-e89b-12d3-a456-426614174000",
    "clientIdentifier": "my-client"
  },
  "message": "Client created successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Get Clients (Paginated)
```http
GET /api/v1/clients?pageNumber=1&pageSize=20&searchTerm=my&isActive=true
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "clients": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "clientId": "my-client",
        "name": "My Application",
        "redirectUris": ["https://app.example.com/callback"],
        "postLogoutRedirectUris": ["https://app.example.com/logout"],
        "allowedGrantTypes": ["authorization_code", "refresh_token"],
        "accessTokenLifetime": 3600,
        "refreshTokenLifetime": 2592000,
        "requireConsent": true,
        "requirePkce": true,
        "isActive": true,
        "createdAt": "2024-01-15T10:30:00Z",
        "lastUsedAt": "2024-01-15T11:00:00Z"
      }
    ],
    "totalCount": 10,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1
  }
}
```

#### Rotate Client Secret
```http
POST /api/v1/clients/{clientId}/rotate-secret
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "clientId": "123e4567-e89b-12d3-a456-426614174000",
  "newSecret": "new-client-secret"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "clientId": "123e4567-e89b-12d3-a456-426614174000",
    "clientIdentifier": "my-client"
  },
  "message": "Client secret rotated successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Multi-Factor Authentication (MFA)

#### Enroll TOTP
```http
POST /api/v1/mfa/totp/enroll
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "secret": "JBSWY3DPEHPK3PXP",
    "qrCodeUri": "otpauth://totp/IdentityService:user@example.com?secret=JBSWY3DPEHPK3PXP&issuer=IdentityService",
    "backupCodes": ["12345678", "87654321", ...]
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Verify TOTP
```http
POST /api/v1/mfa/totp/verify
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "code": "123456"
}
```

#### Enable TOTP
```http
POST /api/v1/mfa/totp/enable
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "code": "123456"
}
```

#### Enroll SMS MFA
```http
POST /api/v1/mfa/sms/enroll
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "phoneNumber": "+1234567890"
}
```

#### Verify SMS MFA
```http
POST /api/v1/mfa/sms/verify
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "code": "123456"
}
```

#### Enroll Email MFA
```http
POST /api/v1/mfa/email/enroll
Authorization: Bearer {accessToken}
```

#### Verify Email MFA
```http
POST /api/v1/mfa/email/verify
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "code": "123456"
}
```

### WebAuthn/FIDO2

#### Register WebAuthn Credential
```http
POST /api/v1/webauthn/register/start
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "challenge": "base64-encoded-challenge",
    "rp": {
      "name": "IdentityService",
      "id": "identity-service.example.com"
    },
    "user": {
      "id": "base64-user-id",
      "name": "user@example.com",
      "displayName": "John Doe"
    },
    "pubKeyCredParams": [
      {
        "type": "public-key",
        "alg": -7
      }
    ],
    "timeout": 60000,
    "attestation": "direct"
  }
}
```

#### Complete WebAuthn Registration
```http
POST /api/v1/webauthn/register/complete
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "credential": {
    "id": "credential-id",
    "rawId": "base64-raw-id",
    "response": {
      "attestationObject": "base64-attestation",
      "clientDataJSON": "base64-client-data"
    },
    "type": "public-key"
  }
}
```

#### Authenticate with WebAuthn
```http
POST /api/v1/webauthn/authenticate/start
Content-Type: application/json

{
  "username": "user@example.com
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "challenge": "base64-encoded-challenge",
    "allowCredentials": [
      {
        "id": "credential-id",
        "type": "public-key"
      }
    ],
    "timeout": 60000
  }
}
```

#### Complete WebAuthn Authentication
```http
POST /api/v1/webauthn/authenticate/complete
Content-Type: application/json

{
  "username": "user@example.com",
  "credential": {
    "id": "credential-id",
    "rawId": "base64-raw-id",
    "response": {
      "authenticatorData": "base64-auth-data",
      "clientDataJSON": "base64-client-data",
      "signature": "base64-signature"
    },
    "type": "public-key"
  }
}
```

### User Management

#### Get Users (Paginated)
```http
GET /api/v1/users?pageNumber=1&pageSize=20&searchTerm=john&isActive=true
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "users": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "email": "user@example.com",
        "username": "johndoe",
        "firstName": "John",
        "lastName": "Doe",
        "isActive": true,
        "isEmailVerified": true,
        "lastLoginAt": "2024-01-15T10:30:00Z",
        "roles": ["User", "Admin"]
      }
    ],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 5
  }
}
```

#### Update User
```http
PUT /api/v1/users/{userId}
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "isActive": true
}
```

### Role Management

#### Get All Roles
```http
GET /api/v1/roles
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "roles": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "name": "Admin",
        "description": "Administrator role",
        "isActive": true,
        "isSystemRole": false,
        "permissions": ["users.read", "users.write", "roles.manage"]
      }
    ]
  }
}
```

#### Create Role
```http
POST /api/v1/roles
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "name": "Manager",
  "description": "Manager role with limited permissions",
  "isSystemRole": false
}
```

#### Assign Permission to Role
```http
POST /api/v1/roles/{roleName}/permissions/{permissionId}
Authorization: Bearer {accessToken}
```

### Permission Management

#### Get All Permissions
```http
GET /api/v1/permissions?resource=users
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "permissions": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "name": "users.read",
        "description": "Read users",
        "resource": "users",
        "action": "read",
        "isActive": true
      }
    ]
  }
}
```

#### Create Permission
```http
POST /api/v1/permissions
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "name": "users.write",
  "description": "Create and update users",
  "resource": "users",
  "action": "write"
}
```

### Session Management

#### Get User Sessions
```http
GET /api/v1/sessions
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "sessions": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "deviceId": "device-123",
        "deviceName": "Chrome Browser",
        "ipAddress": "192.168.1.1",
        "createdAt": "2024-01-15T10:30:00Z",
        "expiresAt": "2024-02-14T10:30:00Z",
        "isRevoked": false,
        "revokedAt": null
      }
    ]
  }
}
```

#### Revoke Session
```http
DELETE /api/v1/sessions/{sessionId}
Authorization: Bearer {accessToken}
```

#### Revoke All Sessions
```http
DELETE /api/v1/sessions/all
Authorization: Bearer {accessToken}
```

### Health Checks

#### Health Check
```http
GET /health
```

#### Readiness Check
```http
GET /health/ready
```

#### Liveness Check
```http
GET /health/live
```

## Cluster Integration

The identity service is fully integrated with the shared cluster resources:

### Shared Resources

- **PostgreSQL**: Uses `postgresql-ha-pgpool.databases.svc.cluster.local` (PgPool for HA)
- **Redis**: Uses `redis-cluster.middleware.svc.cluster.local` for distributed caching
- **RabbitMQ**: Uses `rabbitmq-cluster.middleware.svc.cluster.local` for domain event publishing
- **Prometheus**: Metrics exposed at `/metrics` endpoint with ServiceMonitor for automatic scraping

### Metrics and Observability

- **Prometheus Metrics**: Exposed via OpenTelemetry at `/metrics`
  - HTTP request metrics (count, duration, errors)
  - Database operation metrics
  - Runtime metrics (CPU, memory, GC)
  - Process metrics
  - Entity Framework Core metrics
- **Health Checks**: `/health`, `/health/ready`, `/health/live` endpoints
  - Database connectivity
  - Redis connectivity
  - RabbitMQ connectivity
- **Structured Logging**: Serilog with correlation IDs
- **Domain Events**: Published to RabbitMQ for event-driven architecture
- **ServiceMonitor**: Automatic Prometheus scraping configured

## Configuration

### Environment Variables

| Variable | Description | Required | Default (Cluster) |
|----------|-------------|----------|-------------------|
| `PostgreSQL__HostName` | PostgreSQL hostname | Yes | `postgresql-ha-pgpool.databases.svc.cluster.local` |
| `PostgreSQL__Port` | PostgreSQL port | No | `5432` |
| `PostgreSQL__Database` | Database name | Yes | `appdb` |
| `PostgreSQL__Username` | Database username | Yes | `postgres` |
| `PostgreSQL__Password` | Database password | Yes | - |
| `Redis__HostName` | Redis hostname | No | `redis-cluster.middleware.svc.cluster.local` |
| `Redis__Port` | Redis port | No | `6379` |
| `Redis__Password` | Redis password | No | - |
| `RabbitMQ__HostName` | RabbitMQ hostname | No | `rabbitmq-cluster.middleware.svc.cluster.local` |
| `RabbitMQ__Port` | RabbitMQ port | No | `5672` |
| `RabbitMQ__UserName` | RabbitMQ username | No | `admin` |
| `RabbitMQ__Password` | RabbitMQ password | Yes | - |
| `RabbitMQ__VirtualHost` | RabbitMQ virtual host | No | `/` |
| `Jwt__SecretKey` | JWT signing key (min 32 chars) | Yes | - |
| `Jwt__Issuer` | JWT issuer | No | IdentityService |
| `Jwt__Audience` | JWT audience | No | IdentityService |
| `Jwt__AccessTokenExpirationMinutes` | Access token expiration | No | 60 |
| `Jwt__RefreshTokenExpirationDays` | Refresh token expiration | No | 30 |
| `Cors__AllowedOrigins` | CORS allowed origins (JSON array) | No | ["*"] |
| `Seq__ServerUrl` | Seq logging server URL | No | `http://loki-stack-gateway.observability.svc.cluster.local:3100` |

### Example Configuration (Cluster)

```json
{
  "PostgreSQL": {
    "HostName": "postgresql-ha-pgpool.databases.svc.cluster.local",
    "Port": "5432",
    "Database": "appdb",
    "Username": "postgres",
    "Password": "secret"
  },
  "Redis": {
    "HostName": "redis-cluster.middleware.svc.cluster.local",
    "Port": "6379",
    "Password": "secret"
  },
  "RabbitMQ": {
    "HostName": "rabbitmq-cluster.middleware.svc.cluster.local",
    "Port": "5672",
    "UserName": "admin",
    "Password": "secret",
    "VirtualHost": "/"
  },
  "Jwt": {
    "SecretKey": "YourSuperSecretKey-MustBeAtLeast32CharactersLong!",
    "Issuer": "IdentityService",
    "Audience": "IdentityService",
    "AccessTokenExpirationMinutes": "60",
    "RefreshTokenExpirationDays": "30"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://app.example.com",
      "https://api.example.com"
    ]
  },
  "Seq": {
    "ServerUrl": "http://loki-stack-gateway.observability.svc.cluster.local:3100"
  }
}
```

## Security Features

### Account Protection

- **Account Locking**: Accounts are locked after 5 failed login attempts for 30 minutes
- **Password Requirements**:
  - Minimum 8 characters
  - Must contain uppercase, lowercase, number, and special character
- **Password Hashing**: BCrypt with work factor of 12

### Token Security

- **JWT Signing**: HMAC-SHA256 with configurable secret key
- **JWT Key Rotation**: Signing keys can be rotated without service downtime
  - Multiple active keys supported for gradual rotation
  - Key expiration and automatic deactivation
  - JWKS endpoint returns all active keys for validation
- **Token Provider Factory**: Strategy pattern for token generation
  - JWT Token Provider (current implementation)
  - Reference Token Provider (placeholder for future implementation)
  - Easy to switch between token types
- **Token Expiration**: Configurable access token (default 60 minutes) and refresh token (default 30 days)
- **Refresh Token Rotation**: Old refresh tokens are revoked when new ones are issued
- **Token Validation**: Comprehensive token validation with issuer, audience, and signature verification
- **Token Introspection**: RFC 7662 compliant token introspection endpoint
- **Token Revocation**: RFC 7009 compliant token revocation endpoint

### Rate Limiting

- **Global Rate Limit**: 100 requests per minute per IP/user
- **Per-Endpoint Rate Limiting**: Configurable rate limits per endpoint using Redis
- **Configurable**: Rate limits can be adjusted per endpoint

### Reliability and Consistency

- **Outbox Pattern**: Reliable event publishing with transactional outbox
  - Domain events stored in database before publishing
  - Background job processes outbox messages
  - Ensures at-least-once delivery to RabbitMQ
- **Idempotency**: All commands support idempotency keys
  - Prevents duplicate processing of requests
  - Uses distributed cache (Redis) for idempotency tracking
  - Returns cached response for duplicate requests
- **Grant Type Handler Strategy**: Pluggable grant type handlers
  - Authorization Code Grant Handler
  - Client Credentials Grant Handler
  - Refresh Token Grant Handler
  - Easy to extend with new grant types

### Audit and Compliance

- **Correlation IDs**: Every request gets a unique correlation ID for tracing
- **Structured Logging**: All operations logged with Serilog
- **Audit Trail**: User actions tracked with CreatedBy/ModifiedBy fields
- **Domain Events**: User creation, password changes, account status changes, client creation, and key rotation emit domain events
- **JWT Key Rotation**: Signing keys can be rotated for enhanced security
  - Multiple active keys supported
  - Automatic key expiration
  - JWKS endpoint returns all active keys

## Integration

### For Microservices

Other microservices can validate tokens by calling the `/api/v1/auth/validate` endpoint or by validating JWT tokens directly using the shared secret key.

**Example:**
```csharp
// In your microservice
var httpClient = new HttpClient();
var response = await httpClient.PostAsJsonAsync(
    "https://identity-service/api/v1/auth/validate",
    new { token = jwtToken });

var validationResult = await response.Content.ReadFromJsonAsync<ValidateTokenResponse>();
if (validationResult.IsValid)
{
    // Token is valid, extract user information
    var userId = validationResult.UserId;
    var roles = validationResult.Roles;
}
```

### For External Clients

External clients can use the standard OAuth2/OIDC flow:

1. **Register** user account via `/api/v1/auth/register`
2. **Login** to receive access and refresh tokens via `/api/v1/auth/login`
3. **Use access token** in `Authorization: Bearer {token}` header
4. **Refresh token** when access token expires via `/api/v1/auth/refresh`

## Development

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL 14+
- Redis (optional, for distributed caching)
- Seq (optional, for log aggregation)

### Running Locally

```bash
# Set environment variables
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=identitydb;Username=postgres;Password=postgres"
export Jwt__SecretKey="YourSuperSecretKeyForDevelopmentOnly-MustBeAtLeast32CharactersLong!"

# Run the service
cd src/IdentityService.API
dotnet run
```

### Database Migrations

```bash
# Create migration
dotnet ef migrations add InitialCreate --project src/IdentityService.Infrastructure --startup-project src/IdentityService.API

# Apply migration
dotnet ef database update --project src/IdentityService.Infrastructure --startup-project src/IdentityService.API
```

## Deployment

### Kubernetes

The service is deployed to the cluster via ArgoCD with:

- **Health Checks**: `/health`, `/health/ready`, `/health/live` endpoints
- **Stateless Design**: No in-memory state, all state in database
- **Horizontal Scaling**: HPA configured (2-10 replicas based on CPU/memory)
- **Resource Limits**: CPU 500m-2000m, Memory 512Mi-2Gi
- **ServiceMonitor**: Automatic Prometheus scraping configured
- **Secrets**: Uses Kubernetes secrets for sensitive configuration

### ArgoCD Application

The service is managed via ArgoCD Application:
- **Location**: `argocd/applications/microservices/identity-service.yaml`
- **Namespace**: `microservices`
- **Sync Wave**: 3 (after infrastructure and middleware)
- **Manifests**: `microservices/identity-service/k8s/deployment.yaml`

### Building and Deploying

```bash
# Build Docker image
docker build -t your-registry/identity-service:latest .

# Push to registry
docker push your-registry/identity-service:latest

# ArgoCD will automatically sync the deployment
# Or manually sync via ArgoCD UI/CLI
```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/IdentityService.API/IdentityService.API.csproj", "src/IdentityService.API/"]
RUN dotnet restore "src/IdentityService.API/IdentityService.API.csproj"
COPY . .
WORKDIR "/src/src/IdentityService.API"
RUN dotnet build "IdentityService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.API.dll"]
```

## Monitoring and Observability

### Logging

- **Structured Logging**: Serilog with JSON output
- **Log Levels**: Configurable per component
- **Correlation IDs**: Included in all log entries
- **Seq Integration**: Centralized log aggregation

### Metrics

- Health check endpoints for monitoring
- Request/response logging with timing
- Error tracking and alerting

### Tracing

- Correlation IDs propagated across requests
- Domain events for audit trails
- User action audit trails

## Error Handling

All errors follow a standardized format:

```json
{
  "success": false,
  "message": "Error message",
  "errors": [
    "Detailed error 1",
    "Detailed error 2"
  ],
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Common Error Codes

- **400 Bad Request**: Validation errors, invalid input
- **401 Unauthorized**: Invalid or missing authentication token
- **403 Forbidden**: Insufficient permissions
- **500 Internal Server Error**: Unexpected server errors

## Best Practices

### For Consumers

1. **Store tokens securely**: Never expose refresh tokens in client-side code
2. **Handle token expiration**: Implement automatic token refresh
3. **Use correlation IDs**: Include correlation IDs in requests for tracing
4. **Respect rate limits**: Implement exponential backoff for rate limit errors
5. **Validate tokens**: Always validate tokens before trusting user identity
6. **Use PKCE**: Always use PKCE for public clients (mobile apps, SPAs)
7. **Idempotency Keys**: Include `X-Idempotency-Key` header for POST/PUT operations to prevent duplicate processing
8. **OIDC Discovery**: Use the OIDC Discovery endpoint to dynamically discover endpoints and capabilities
9. **Token Introspection**: Use token introspection to validate tokens instead of parsing JWT directly
10. **WebAuthn**: Prefer WebAuthn/FIDO2 for passwordless authentication when possible

### For Developers

1. **Follow Clean Architecture**: Maintain layer separation
2. **Use MediatR**: All business logic through commands/queries
3. **Validate inputs**: Use FluentValidation for all commands
4. **Handle errors gracefully**: Use Result pattern for error handling
5. **Log appropriately**: Log at appropriate levels with context
6. **Use Outbox Pattern**: Always use outbox for domain event publishing to ensure reliability
7. **Support Idempotency**: Implement `ICommandWithIdempotencyKey` for commands that should be idempotent
8. **Strategy Pattern**: Use strategy pattern for extensible features (MFA providers, token providers, grant types)
9. **Key Rotation**: Implement key rotation for signing keys to enhance security
10. **Test Grant Types**: Test all OAuth2 grant types thoroughly, especially PKCE validation

## License

[Your License Here]

## Support

For issues and questions, please contact the development team or create an issue in the repository.

