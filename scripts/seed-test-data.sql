-- ============================================================================
-- OopsReviewCenter Test Data Seeding Script
-- ============================================================================
-- This script populates the database with realistic test data for development
-- and testing purposes.
--
-- IMPORTANT: This script should ONLY be run manually, not automatically on 
-- application startup.
--
-- HOW TO RUN:
-- 1. Using sqlite3 CLI:
--    sqlite3 OopsReviewCenter/App_Data/oopsreviewcenter.db < scripts/seed-test-data.sql
--
-- 2. Or using dotnet tool:
--    First install: dotnet tool install --global dotnet-script
--    Then you can execute SQL via a C# script if needed
--
-- 3. Or from the SQLite command line:
--    sqlite3 OopsReviewCenter/App_Data/oopsreviewcenter.db
--    .read scripts/seed-test-data.sql
-- ============================================================================

-- Clear existing data (in case re-running)
DELETE FROM IncidentTags;
DELETE FROM ActionItems;
DELETE FROM TimelineEvents;
DELETE FROM Incidents;
DELETE FROM Users;
DELETE FROM Roles;
DELETE FROM Tags;
DELETE FROM Templates;

-- Reset autoincrement counters
DELETE FROM sqlite_sequence;

-- ============================================================================
-- Roles
-- ============================================================================
INSERT INTO Roles (RoleId, Name, Description) VALUES
(1, 'Administrator', 'Full system access with all permissions'),
(2, 'Incident Manager', 'Can create and manage incidents, assign tasks'),
(3, 'Developer', 'Can view incidents and update action items'),
(4, 'Viewer', 'Read-only access to incidents and reports');

-- ============================================================================
-- Users
-- ============================================================================
-- IMPORTANT: These password hashes are generated using PasswordHasher service for test data.
-- WARNING: Do NOT use these passwords or this script in production environments.
--          These are publicly documented test credentials only.
-- 
-- Admin user password: TestAdminPassword!@#$
-- Viewer user password: PasswordTestUSER!!!
-- 
-- NOTE: Only admin and viewer users have functional passwords. Other users 
--       (jsmith, mjones, bwilson, slee, dchen, aparker) still have dummy 
--       hashes and cannot authenticate.
-- 
-- These hashes were generated using:
--   var hasher = new PasswordHasher();
--   var salt = hasher.GenerateSalt();
--   var hash = hasher.HashPassword(password, salt);
-- 
-- For testing/demo purposes, use the passwords above to login.
INSERT INTO Users (UserId, RoleId, Username, Email, FullName, PasswordHash, Salt, IsActive) VALUES
(1, 1, 'admin', 'admin@oopsreview.com', 'Admin User', 'iJ6mjfNn/pLbc5ixkUW8a0/OQHHLtZSEUjJdX6+ZGnA=', 'OUnH/j/xvVW/2UY2lr1ghw==', 1),
--admin password: TestAdminPassword!@#$
(2, 2, 'jsmith', 'john.smith@oopsreview.com', 'John Smith', 'dummyhash2', 'dummysalt2', 1),
(3, 2, 'mjones', 'mary.jones@oopsreview.com', 'Mary Jones', 'dummyhash3', 'dummysalt3', 1),
(4, 3, 'bwilson', 'bob.wilson@oopsreview.com', 'Bob Wilson', 'dummyhash4', 'dummysalt4', 1),
(5, 3, 'slee', 'sarah.lee@oopsreview.com', 'Sarah Lee', 'dummyhash5', 'dummysalt5', 1),
(6, 3, 'dchen', 'david.chen@oopsreview.com', 'David Chen', 'dummyhash6', 'dummysalt6', 1),
--viewer password: PasswordTestUSER!!!
(7, 4, 'viewer', 'viewer@oopsreview.com', 'Guest Viewer', 'deZySnHC+eSCqzx1XUc+HdOFCzOvpc0UhqYXXtj2U5Y=', 'V349IS4Ym9KaKHehuXNutg==', 1),
(8, 3, 'aparker', 'alice.parker@oopsreview.com', 'Alice Parker', 'dummyhash8', 'dummysalt8', 1);

-- ============================================================================
-- Tags
-- ============================================================================
INSERT INTO Tags (Id, Name, Color, CreatedAt) VALUES
(1, 'Database', '#3498db', datetime('now', '-30 days')),
(2, 'API', '#e74c3c', datetime('now', '-30 days')),
(3, 'Network', '#f39c12', datetime('now', '-30 days')),
(4, 'Security', '#9b59b6', datetime('now', '-30 days')),
(5, 'Performance', '#1abc9c', datetime('now', '-30 days')),
(6, 'Frontend', '#34495e', datetime('now', '-30 days')),
(7, 'Backend', '#16a085', datetime('now', '-30 days')),
(8, 'Infrastructure', '#c0392b', datetime('now', '-30 days')),
(9, 'Deployment', '#8e44ad', datetime('now', '-30 days')),
(10, 'Monitoring', '#2c3e50', datetime('now', '-30 days'));

-- ============================================================================
-- Templates
-- ============================================================================
INSERT INTO Templates (Id, Name, Type, Content, CreatedAt) VALUES
(1, 'Standard Incident Report', 'Incident', '## Incident Summary

## Timeline

## Impact

## Root Cause

## Action Items', datetime('now', '-30 days')),
(2, 'Quick Action Item', 'ActionItem', '### Task

### Expected Outcome

### Dependencies', datetime('now', '-30 days')),
(3, 'Security Incident Template', 'Incident', '## Security Incident

### Description
[What happened]

### Detection
[How was it detected]

### Impact Assessment
[Who/what was affected]

### Containment Actions
[Steps taken to contain]

### Root Cause
[Why it happened]

### Remediation Plan
[How to prevent]', datetime('now', '-30 days'));

-- ============================================================================
-- Incidents (30+ realistic incidents)
-- ============================================================================
INSERT INTO Incidents (Id, Title, Description, OccurredAt, CreatedAt, ResolvedAt, Severity, Status, RootCause, Impact, ResolvedByUserId) VALUES
(1, 'Database Connection Pool Exhaustion', 'Production database connection pool was exhausted causing application timeouts', 
 datetime('now', '-25 days'), datetime('now', '-25 days'), datetime('now', '-24 days'), 
 'Critical', 'Resolved', 'Connection pool size was too small for peak traffic. Connections were not being properly released.', 
 'Service was unavailable for 45 minutes. Approximately 1,200 users affected.', 2),

(2, 'API Rate Limit Exceeded', 'Third-party API rate limits were exceeded during batch processing', 
 datetime('now', '-21 days'), datetime('now', '-21 days'), datetime('now', '-20 days'), 
 'High', 'Resolved', 'Batch processing job was not implementing proper rate limiting or backoff strategy.', 
 'Batch job delayed by 2 hours. No direct user impact.', 4),

(3, 'Slow Page Load Performance', 'Users reported significantly slower page load times on the dashboard', 
 datetime('now', '-19 days'), datetime('now', '-19 days'), NULL, 
 'Medium', 'Investigating', NULL, 'Dashboard load times increased from 1s to 5s average', NULL),

(4, 'Authentication Service Outage', 'Complete outage of authentication service preventing all logins', 
 datetime('now', '-18 days'), datetime('now', '-18 days'), datetime('now', '-18 days', '+3 hours'), 
 'Critical', 'Resolved', 'Certificate expired on authentication service causing SSL handshake failures.', 
 'All users unable to login for 3 hours. Critical business impact.', 1),

(5, 'Memory Leak in Background Worker', 'Background worker process consuming increasing amounts of memory', 
 datetime('now', '-17 days'), datetime('now', '-17 days'), datetime('now', '-16 days'), 
 'High', 'Resolved', 'Event handlers were not being properly disposed causing memory accumulation.', 
 'Required service restart every 6 hours. Potential data processing delays.', 4),

(6, 'Incorrect Tax Calculation', 'Tax calculations showing incorrect values for certain states', 
 datetime('now', '-16 days'), datetime('now', '-16 days'), datetime('now', '-15 days'), 
 'High', 'Resolved', 'Tax table update was partially applied, missing state-specific rules.', 
 'Affected approximately 150 transactions. Financial reconciliation required.', 2),

(7, 'Email Notification Delays', 'Email notifications being delivered with 2-3 hour delays', 
 datetime('now', '-15 days'), datetime('now', '-15 days'), NULL, 
 'Medium', 'Open', NULL, 'Users not receiving timely notifications. SLA breach for notification service.', NULL),

(8, 'File Upload Failures', 'Users unable to upload files larger than 5MB', 
 datetime('now', '-14 days'), datetime('now', '-14 days'), datetime('now', '-14 days', '+4 hours'), 
 'Medium', 'Resolved', 'New CDN configuration had incorrect file size limits.', 
 'Approximately 45 users affected. Workaround: compression before upload.', 5),

(9, 'Data Export Timeout', 'Large data export operations timing out before completion', 
 datetime('now', '-13 days'), datetime('now', '-13 days'), datetime('now', '-12 days'), 
 'Medium', 'Resolved', 'Export query was not optimized and took too long for large datasets.', 
 '5 enterprise customers unable to export monthly reports.', 6),

(10, 'Login Page 404 Error', 'Login page returning 404 error for specific subdomain', 
 datetime('now', '-12 days'), datetime('now', '-12 days'), datetime('now', '-12 days', '+1 hour'), 
 'Critical', 'Resolved', 'DNS configuration error after recent infrastructure change.', 
 'All users on subdomain unable to access application for 1 hour.', 1),

(11, 'Payment Processing Failures', 'Payment gateway returning errors for all transactions', 
 datetime('now', '-11 days'), datetime('now', '-11 days'), datetime('now', '-11 days', '+2 hours'), 
 'Critical', 'Resolved', 'API key expired and automated renewal process failed.', 
 'All payment processing halted for 2 hours. Revenue impact estimated at $50K.', 2),

(12, 'Search Results Inaccurate', 'Search functionality returning irrelevant or missing results', 
 datetime('now', '-10 days'), datetime('now', '-10 days'), datetime('now', '-9 days'), 
 'High', 'Resolved', 'Search index rebuild failed overnight, leaving index in corrupted state.', 
 'Poor user experience. Increased support tickets by 200%.', 5),

(13, 'Mobile App Crashes on Startup', 'iOS mobile app crashing immediately on launch for version 2.3.1', 
 datetime('now', '-9 days'), datetime('now', '-9 days'), datetime('now', '-8 days'), 
 'Critical', 'Resolved', 'Third-party SDK update introduced breaking change with iOS 17.', 
 'All iOS users on latest version unable to use mobile app. Emergency patch required.', 4),

(14, 'Report Generation Broken', 'Monthly financial reports generating with incorrect totals', 
 datetime('now', '-8 days'), datetime('now', '-8 days'), datetime('now', '-7 days'), 
 'High', 'Resolved', 'Database migration script contained error in aggregate calculation logic.', 
 'Finance team unable to close month. Manual recalculation required for accuracy.', 3),

(15, 'API Response Time Degradation', 'API endpoints showing 300% increase in response times', 
 datetime('now', '-7 days'), datetime('now', '-7 days'), NULL, 
 'High', 'Investigating', NULL, 'User experience degraded. Some operations timing out.', NULL),

(16, 'Backup Job Failures', 'Nightly database backup jobs failing silently for past week', 
 datetime('now', '-6 days'), datetime('now', '-6 days'), datetime('now', '-5 days'), 
 'High', 'Resolved', 'Backup storage quota exceeded and no alerts configured for failure.', 
 'No data loss but severe business continuity risk. 7 days without backups.', 2),

(17, 'Session Timeout Issues', 'Users being logged out unexpectedly after 5 minutes', 
 datetime('now', '-5 days'), datetime('now', '-5 days'), datetime('now', '-5 days', '+6 hours'), 
 'Medium', 'Resolved', 'Session configuration change inadvertently reduced timeout from 30min to 5min.', 
 'Poor user experience. Multiple support complaints.', 3),

(18, 'Incorrect Permissions', 'Users able to access features outside their permission level', 
 datetime('now', '-4 days'), datetime('now', '-4 days'), datetime('now', '-4 days', '+1 hour'), 
 'Critical', 'Resolved', 'Permission check middleware was bypassed in recent refactoring.', 
 'Potential security breach. Immediate hotfix deployed. Security audit initiated.', 1),

(19, 'Chart Rendering Issues', 'Dashboard charts not rendering correctly in Firefox browser', 
 datetime('now', '-3 days'), datetime('now', '-3 days'), datetime('now', '-2 days'), 
 'Low', 'Resolved', 'Chart library update incompatible with older Firefox versions.', 
 'Approximately 8% of users (Firefox) experiencing visualization issues.', 6),

(20, 'Webhook Delivery Failures', 'Webhooks not being delivered to third-party integrations', 
 datetime('now', '-2 days'), datetime('now', '-2 days'), NULL, 
 'High', 'Open', NULL, 'Partner integrations not receiving real-time updates. Manual sync required.', NULL),

(21, 'SSL Certificate Warning', 'Users seeing SSL certificate warnings on checkout page', 
 datetime('now', '-1 day'), datetime('now', '-1 day'), datetime('now', '-1 day', '+2 hours'), 
 'Critical', 'Resolved', 'Certificate auto-renewal failed due to DNS verification issues.', 
 'Checkout page inaccessible. Revenue impact during resolution window.', 1),

(22, 'Duplicate Notifications', 'Users receiving duplicate email and SMS notifications', 
 datetime('now', '-1 day'), datetime('now', '-1 day'), NULL, 
 'Low', 'Investigating', NULL, 'User annoyance. Increased unsubscribe rate noticed.', NULL),

(23, 'Cache Invalidation Bug', 'Stale data being served from cache after updates', 
 datetime('now', '0 day'), datetime('now', '0 day'), NULL, 
 'Medium', 'Open', NULL, 'Users seeing outdated information. Data consistency issues.', NULL),

(24, 'Job Queue Backlog', 'Background job queue showing significant backlog and delays', 
 datetime('now', '0 day'), datetime('now', '0 day'), NULL, 
 'High', 'Investigating', NULL, 'Delayed processing of user requests. Queue length: 50,000+ jobs.', NULL),

(25, 'Image Upload Corruption', 'Uploaded images appearing corrupted or with wrong orientation', 
 datetime('now', '-26 days'), datetime('now', '-26 days'), datetime('now', '-25 days'), 
 'Medium', 'Resolved', 'Image processing pipeline not preserving EXIF metadata correctly.', 
 'Poor user experience. Required re-upload for affected images.', 8),

(26, 'Cross-Site Scripting Vulnerability', 'XSS vulnerability discovered in user comment section', 
 datetime('now', '-22 days'), datetime('now', '-22 days'), datetime('now', '-22 days', '+4 hours'), 
 'Critical', 'Resolved', 'Input sanitization missing in comment rendering logic.', 
 'Potential security risk. Immediate patch deployed. No exploitation detected.', 1),

(27, 'PDF Generation Failure', 'PDF reports failing to generate with cryptic error messages', 
 datetime('now', '-20 days'), datetime('now', '-20 days'), datetime('now', '-19 days'), 
 'Medium', 'Resolved', 'PDF library dependency conflict after package update.', 
 'Users unable to download reports. Temporary workaround: CSV exports.', 5),

(28, 'Timezone Display Bug', 'Dates and times showing in wrong timezone for international users', 
 datetime('now', '-23 days'), datetime('now', '-23 days'), datetime('now', '-22 days'), 
 'Medium', 'Resolved', 'Timezone conversion logic not accounting for daylight saving transitions.', 
 'Confusion for international users. Meeting scheduling errors reported.', 3),

(29, 'Load Balancer Health Check Fails', 'Load balancer incorrectly marking healthy servers as down', 
 datetime('now', '-24 days'), datetime('now', '-24 days'), datetime('now', '-24 days', '+3 hours'), 
 'Critical', 'Resolved', 'Health check endpoint timeout too aggressive for application startup time.', 
 'Intermittent service unavailability. Traffic not distributed properly.', 2),

(30, 'Search Indexing Lag', 'New content not appearing in search results for 24+ hours', 
 datetime('now', '-27 days'), datetime('now', '-27 days'), datetime('now', '-26 days'), 
 'Medium', 'Resolved', 'Search indexing worker was stuck in error loop, not processing queue.', 
 'Recently added content invisible to users. Support tickets increased.', 6),

(31, 'Two-Factor Auth Bypass', 'Edge case allowing 2FA bypass in password reset flow', 
 datetime('now', '-28 days'), datetime('now', '-28 days'), datetime('now', '-28 days', '+2 hours'), 
 'Critical', 'Resolved', 'State validation missing in password reset flow allowing session reuse.', 
 'Security vulnerability. No known exploitation. Immediate hotfix applied.', 1),

(32, 'Billing Cycle Error', 'Some customers billed twice in same billing period', 
 datetime('now', '-29 days'), datetime('now', '-29 days'), datetime('now', '-28 days'), 
 'Critical', 'Resolved', 'Race condition in billing job when running multiple instances concurrently.', 
 'Financial impact. Required refunds for 23 customers. Immediate fix and safeguards added.', 2);

-- ============================================================================
-- Timeline Events
-- ============================================================================
INSERT INTO TimelineEvents (IncidentId, OccurredAt, Description, Author) VALUES
-- Incident 1: Database Connection Pool Exhaustion
(1, datetime('now', '-25 days'), 'First reports of timeout errors in application logs', 'Monitoring System'),
(1, datetime('now', '-25 days', '+10 minutes'), 'Database connection pool exhaustion confirmed', 'DevOps Team'),
(1, datetime('now', '-25 days', '+30 minutes'), 'Emergency fix deployed - increased connection pool size', 'DevOps Team'),
(1, datetime('now', '-24 days'), 'Monitoring confirms stability. Issue resolved.', 'John Smith'),

-- Incident 2: API Rate Limit Exceeded
(2, datetime('now', '-21 days'), 'Batch job failed with rate limit error', 'System'),
(2, datetime('now', '-21 days', '+1 hour'), 'Investigation started. Reviewing API usage patterns.', 'Mary Jones'),
(2, datetime('now', '-21 days', '+3 hours'), 'Implemented exponential backoff. Restarted job.', 'Bob Wilson'),
(2, datetime('now', '-20 days'), 'Batch job completed successfully with new rate limiting.', 'System'),

-- Incident 4: Authentication Service Outage
(4, datetime('now', '-18 days'), 'Alert: Authentication service health check failing', 'Monitoring System'),
(4, datetime('now', '-18 days', '+15 minutes'), 'Confirmed: SSL certificate expired', 'Admin User'),
(4, datetime('now', '-18 days', '+1 hour'), 'New certificate generated and deployed', 'Admin User'),
(4, datetime('now', '-18 days', '+2 hours'), 'Service restored. Monitoring for stability.', 'Admin User'),

-- Incident 5: Memory Leak in Background Worker
(5, datetime('now', '-17 days'), 'Memory usage alerts for background worker service', 'Monitoring System'),
(5, datetime('now', '-17 days', '+2 hours'), 'Memory profiling shows continuous growth pattern', 'Bob Wilson'),
(5, datetime('now', '-16 days'), 'Root cause identified: event handler disposal issue', 'Bob Wilson'),
(5, datetime('now', '-16 days', '+4 hours'), 'Fix deployed. Memory usage normalized.', 'Bob Wilson'),

-- Incident 10: Login Page 404 Error
(10, datetime('now', '-12 days'), 'User reports: login page not loading', 'Support Team'),
(10, datetime('now', '-12 days', '+15 minutes'), 'DNS misconfiguration identified', 'Admin User'),
(10, datetime('now', '-12 days', '+30 minutes'), 'DNS corrected. Propagation in progress.', 'Admin User'),
(10, datetime('now', '-12 days', '+1 hour'), 'All users can now access login page', 'Admin User'),

-- Incident 11: Payment Processing Failures
(11, datetime('now', '-11 days'), 'Payment gateway returning API authentication errors', 'System'),
(11, datetime('now', '-11 days', '+30 minutes'), 'Confirmed: API key expired', 'John Smith'),
(11, datetime('now', '-11 days', '+1 hour'), 'New API key generated and configured', 'John Smith'),
(11, datetime('now', '-11 days', '+2 hours'), 'Payment processing restored and verified', 'John Smith'),

-- Incident 13: Mobile App Crashes
(13, datetime('now', '-9 days'), 'Spike in iOS app crash reports for v2.3.1', 'Crash Reporting System'),
(13, datetime('now', '-9 days', '+1 hour'), 'Crash logs analyzed. SDK compatibility issue identified.', 'Bob Wilson'),
(13, datetime('now', '-9 days', '+4 hours'), 'Emergency patch v2.3.2 submitted to App Store', 'Bob Wilson'),
(13, datetime('now', '-8 days'), 'Patch approved and released. Monitoring crash metrics.', 'Bob Wilson'),

-- Incident 18: Incorrect Permissions
(18, datetime('now', '-4 days'), 'Security team notified of permission bypass', 'Security Team'),
(18, datetime('now', '-4 days', '+15 minutes'), 'Issue confirmed. Code review initiated.', 'Admin User'),
(18, datetime('now', '-4 days', '+30 minutes'), 'Hotfix developed and tested', 'Admin User'),
(18, datetime('now', '-4 days', '+1 hour'), 'Hotfix deployed. Security audit in progress.', 'Admin User'),

-- Incident 21: SSL Certificate Warning
(21, datetime('now', '-1 day'), 'Users reporting SSL warnings on checkout page', 'Support Team'),
(21, datetime('now', '-1 day', '+30 minutes'), 'Certificate expiry confirmed. Investigating renewal failure.', 'Admin User'),
(21, datetime('now', '-1 day', '+1 hour'), 'DNS verification issue resolved. Certificate renewed.', 'Admin User'),
(21, datetime('now', '-1 day', '+2 hours'), 'New certificate deployed. Checkout page accessible.', 'Admin User');

-- ============================================================================
-- Action Items
-- ============================================================================
INSERT INTO ActionItems (IncidentId, Title, Description, Status, Priority, AssignedTo, DueDate, CreatedAt, CompletedAt) VALUES
-- Incident 1 Action Items
(1, 'Review and optimize connection pooling configuration', 'Perform comprehensive review of database connection pooling settings', 
 'Completed', 'High', 'Bob Wilson', datetime('now', '-23 days'), datetime('now', '-24 days'), datetime('now', '-23 days')),
(1, 'Add connection pool monitoring and alerts', 'Implement monitoring for connection pool metrics', 
 'Completed', 'High', 'Sarah Lee', datetime('now', '-20 days'), datetime('now', '-24 days'), datetime('now', '-21 days')),
(1, 'Code review for connection disposal', 'Audit code to ensure proper connection disposal in all paths', 
 'Completed', 'High', 'David Chen', datetime('now', '-22 days'), datetime('now', '-24 days'), datetime('now', '-22 days')),

-- Incident 2 Action Items
(2, 'Implement rate limiting strategy for API calls', 'Add exponential backoff and queue system for batch processing', 
 'Completed', 'High', 'Bob Wilson', datetime('now', '-18 days'), datetime('now', '-21 days'), datetime('now', '-20 days')),
(2, 'Document API rate limits', 'Create documentation for all third-party API rate limits', 
 'In Progress', 'Medium', 'Sarah Lee', datetime('now', '-10 days'), datetime('now', '-21 days'), NULL),

-- Incident 3 Action Items
(3, 'Profile dashboard queries', 'Identify slow database queries on dashboard page', 
 'In Progress', 'Medium', 'David Chen', datetime('now', '-5 days'), datetime('now', '-19 days'), NULL),
(3, 'Optimize dashboard data loading', 'Implement caching and lazy loading strategies', 
 'Open', 'Medium', 'Bob Wilson', datetime('now', '0 day'), datetime('now', '-19 days'), NULL),

-- Incident 4 Action Items
(4, 'Implement automated certificate renewal', 'Set up Let''s Encrypt automation for certificate renewal', 
 'Completed', 'Critical', 'Admin User', datetime('now', '-17 days'), datetime('now', '-18 days'), datetime('now', '-17 days')),
(4, 'Certificate expiry monitoring', 'Add alerts for certificates expiring within 30 days', 
 'Completed', 'High', 'Sarah Lee', datetime('now', '-16 days'), datetime('now', '-18 days'), datetime('now', '-16 days')),

-- Incident 5 Action Items
(5, 'Fix event handler disposal', 'Ensure proper disposal pattern for all event handlers', 
 'Completed', 'High', 'Bob Wilson', datetime('now', '-15 days'), datetime('now', '-17 days'), datetime('now', '-16 days')),
(5, 'Add memory usage monitoring', 'Implement memory profiling and alerting for all services', 
 'In Progress', 'High', 'Sarah Lee', datetime('now', '-5 days'), datetime('now', '-17 days'), NULL),

-- Incident 10 Action Items
(10, 'Review DNS change process', 'Document and improve DNS configuration change procedures', 
 'Completed', 'Medium', 'Admin User', datetime('now', '-10 days'), datetime('now', '-12 days'), datetime('now', '-11 days')),

-- Incident 11 Action Items
(11, 'Automate API key rotation', 'Implement automated API key rotation before expiration', 
 'In Progress', 'High', 'John Smith', datetime('now', '-3 days'), datetime('now', '-11 days'), NULL),
(11, 'API key expiry alerts', 'Add monitoring for API key expiration dates', 
 'Completed', 'High', 'Sarah Lee', datetime('now', '-9 days'), datetime('now', '-11 days'), datetime('now', '-10 days')),

-- Incident 13 Action Items
(13, 'Update SDK compatibility matrix', 'Document all third-party SDK versions and iOS compatibility', 
 'In Progress', 'Medium', 'Bob Wilson', datetime('now', '0 day'), datetime('now', '-9 days'), NULL),
(13, 'Implement crash monitoring dashboard', 'Create real-time dashboard for mobile app crash metrics', 
 'Open', 'Medium', 'Alice Parker', datetime('now', '+5 days'), datetime('now', '-9 days'), NULL),

-- Incident 18 Action Items
(18, 'Security code review', 'Comprehensive security audit of authentication and authorization code', 
 'In Progress', 'Critical', 'Admin User', datetime('now', '+2 days'), datetime('now', '-4 days'), NULL),
(18, 'Add permission tests', 'Create comprehensive unit and integration tests for permissions', 
 'Open', 'High', 'David Chen', datetime('now', '+5 days'), datetime('now', '-4 days'), NULL),

-- Incident 21 Action Items
(21, 'Improve certificate renewal process', 'Add redundancy and better error handling for certificate renewal', 
 'In Progress', 'High', 'Admin User', datetime('now', '+3 days'), datetime('now', '-1 day'), NULL),

-- General Operational Tasks
(15, 'Investigate database query performance', 'Analyze slow query logs and optimize problematic queries', 
 'In Progress', 'High', 'David Chen', datetime('now', '+2 days'), datetime('now', '-7 days'), NULL),
(24, 'Scale background job workers', 'Add additional worker instances to handle queue backlog', 
 'Open', 'High', 'Sarah Lee', datetime('now', '+1 day'), datetime('now', '0 day'), NULL),
(24, 'Implement job queue monitoring', 'Add dashboards and alerts for job queue depth and processing rates', 
 'Open', 'High', 'Alice Parker', datetime('now', '+3 days'), datetime('now', '0 day'));

-- ============================================================================
-- Incident Tags (many-to-many relationships)
-- ============================================================================
INSERT INTO IncidentTags (IncidentId, TagId) VALUES
-- Incident 1: Database + Performance
(1, 1), (1, 5),
-- Incident 2: API + Backend
(2, 2), (2, 7),
-- Incident 3: Performance + Frontend
(3, 5), (3, 6),
-- Incident 4: Security + Infrastructure
(4, 4), (4, 8),
-- Incident 5: Performance + Backend
(5, 5), (5, 7),
-- Incident 6: Backend + Database
(6, 7), (6, 1),
-- Incident 7: Backend + Infrastructure
(7, 7), (7, 8),
-- Incident 8: Frontend + Infrastructure
(8, 6), (8, 8),
-- Incident 9: Database + Performance
(9, 1), (9, 5),
-- Incident 10: Infrastructure + Network
(10, 8), (10, 3),
-- Incident 11: API + Backend
(11, 2), (11, 7),
-- Incident 12: Database + Backend
(12, 1), (12, 7),
-- Incident 13: Frontend + Deployment
(13, 6), (13, 9),
-- Incident 14: Database + Backend
(14, 1), (14, 7),
-- Incident 15: API + Performance
(15, 2), (15, 5),
-- Incident 16: Database + Infrastructure
(16, 1), (16, 8),
-- Incident 17: Backend + Security
(17, 7), (17, 4),
-- Incident 18: Security + Backend
(18, 4), (18, 7),
-- Incident 19: Frontend
(19, 6),
-- Incident 20: API + Backend
(20, 2), (20, 7),
-- Incident 21: Security + Infrastructure
(21, 4), (21, 8),
-- Incident 22: Backend
(22, 7),
-- Incident 23: Backend + Performance
(23, 7), (23, 5),
-- Incident 24: Backend + Infrastructure
(24, 7), (24, 8),
-- Incident 25: Frontend + Backend
(25, 6), (25, 7),
-- Incident 26: Security + Frontend
(26, 4), (26, 6),
-- Incident 27: Backend
(27, 7),
-- Incident 28: Frontend + Backend
(28, 6), (28, 7),
-- Incident 29: Infrastructure + Network
(29, 8), (29, 3),
-- Incident 30: Database + Backend
(30, 1), (30, 7),
-- Incident 31: Security + Backend
(31, 4), (31, 7),
-- Incident 32: Backend + Database
(32, 7), (32, 1);

-- ============================================================================
-- Verification Queries (uncomment to run after seeding)
-- ============================================================================
-- SELECT 'Roles Count: ' || COUNT(*) FROM Roles;
-- SELECT 'Users Count: ' || COUNT(*) FROM Users;
-- SELECT 'Tags Count: ' || COUNT(*) FROM Tags;
-- SELECT 'Templates Count: ' || COUNT(*) FROM Templates;
-- SELECT 'Incidents Count: ' || COUNT(*) FROM Incidents;
-- SELECT 'Timeline Events Count: ' || COUNT(*) FROM TimelineEvents;
-- SELECT 'Action Items Count: ' || COUNT(*) FROM ActionItems;
-- SELECT 'Incident Tags Count: ' || COUNT(*) FROM IncidentTags;
-- 
-- SELECT 'Resolved Incidents: ' || COUNT(*) FROM Incidents WHERE Status = 'Resolved';
-- SELECT 'Open Incidents: ' || COUNT(*) FROM Incidents WHERE Status = 'Open';
-- SELECT 'Investigating Incidents: ' || COUNT(*) FROM Incidents WHERE Status = 'Investigating';
