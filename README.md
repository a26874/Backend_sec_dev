# Backend_sec_dev
Backend project focused on login security, this project and roadmap where create with chatgpt, so i could learn more about backend development focused on security pourposes.

# ASP.NET Core Backend Security Project

A comprehensive roadmap for building a secure, enterprise-grade ASP.NET Core backend with advanced security features.

---

## Challenges so far

### Phase 3
Until the beggining of phase 3 everything is basic.

In phase 3 JWT itself is basic too, what makes this phase difficult, is the architecture designs of the application. 
Should we story only a AuthSession (so we can refresh tokens for the user), based on the IP and hashed_token or should we create multiple of them?

In the end i chose to use the multiple of them, since it goes more with the roadmap objectives.

### Phase 4
Having a bit of confusion distinguishing what a middleware should do, since i in the day i wrote this, already validate some of the stuff i will do in the middleware.
Probably will have to separate that logic so both can use them.
### 

## Roadmap

### Phase 1 — Project Setup & Data Ingestion
1. Create ASP.NET Core Web API
2. Configure EF Core
3. Import finance dataset into DB
4. Map dataset tables to EF Core entities
5. Add minimal tables: `Users`, `AuthSessions` or `RefreshTokens`, `AuditLogs`
6. Configure Serilog logging

### Phase 2 — User Registration & Login
1. Create `User` entity
2. Implement registration endpoint
3. Hash passwords securely
4. Enforce unique email constraint
5. Implement login endpoint
6. Track failed login attempts and last login time
7. Temporary account lockout after N failures

### Phase 3 — Authentication System
1. Session-based authentication:
   - Session table
   - Cookies
   - Rotation
   - Logout
   - Expiration
2. Token-based authentication:
   - Access + refresh tokens
   - Store hashed refresh tokens
   - Rotate refresh tokens on use
   - Detect token reuse and revoke all sessions

### Phase 4 — Authorization & Secure Data Access
1. Add authentication middleware
2. Create authorization policies
3. Protect endpoints
4. Implement row-level ownership checks
5. Prevent IDOR attacks
6. Add admin-only endpoints

### Phase 5 — Expense & Subscription APIs
1. Transaction listing endpoint
2. Filtering by date range, merchant, category
3. Aggregation: monthly totals and category breakdown
4. Subscription detection:
   - Same merchant
   - Similar amount
   - Recurring intervals

### Phase 6 — Security Hardening
1. Rate limiting (`AspNetCoreRateLimit`)
2. Input validation (`FluentValidation`)
3. Global exception handling
4. Prevent sensitive error leaks
5. Secure password reset flow (time-limited, single-use)
6. CSRF protection (if cookies used)

### Phase 7 — Audit Logging & Monitoring
1. Create audit log service
2. Log:
   - Logins
   - Failures
   - Password changes
   - Permission denials
3. Expose admin audit endpoints
4. Add suspicious activity flags

### Phase 8 — Advanced / Hard Mode
1. Login from new device detection
2. IP / geo anomaly detection
3. Encrypted columns using EF Core value converters
4. Soft deletes with recovery
5. Background hosted services
6. API versioning
7. GDPR-style data export

---

## Attack Scenarios to Test
1. Brute-force login
2. Session fixation
3. Token replay
4. IDOR access attempts
5. SQL injection attempts
6. CSRF attacks
7. Privilege escalation

---

## Why This Is a Strong .NET Project
1. Build enterprise-grade ASP.NET APIs
2. Use Identity correctly
3. Handle real security concerns
4. Work with legacy databases
5. Think like a backend engineer


