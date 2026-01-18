# Domain Invariants

## Module Structure Overview

The application will be divided into the following modules:
- **Identity Module**: Users, Accounts, Authentication, Guests
- **Quiz Module**: Quizzes, Questions, QuestionItems, Categories
- **Session Module**: QuizSessions, Participants, Answers, Scores
- **Social Module**: Friendships, FriendRequests
- **Notification Module**: Notifications

---

# Identity Module

## User Invariants
- A user has a unique identifier (UserId)
- A user can sign in with credentials
- A user can sign in via external identity provider (Keycloak)
- A user can create an account
- A user can choose to play as a guest (creates temporary guest identity)
- A user can create a quiz
- A user cannot participate in multiple active quiz sessions simultaneously
- A user cannot create a new quiz session if they have an ongoing quiz session that hasn't ended
- A user can generate a quiz invitation link
- A user can participate in a public quiz session
- A user can participate in a private quiz session via invitation link
- A user can send a friend request to another user
- A user can accept or reject a friend request
- A user can remove a friend
- A user can block another user
- A user can list previous quiz summaries they participated in
- A user can list previous quiz summaries they created
- A user has statistics (total quizzes played, wins, average score, etc.)
- A user can update their profile (display name, avatar)
- A user can delete their account (soft delete, anonymize data)

## Guest Invariants
- A guest has a temporary unique identifier (GuestId)
- A guest has a temporary display name (auto-generated or chosen)
- A guest can participate in a public quiz session
- A guest can participate in a private quiz session via invitation link
- A guest cannot create a quiz
- A guest cannot send/receive friend requests
- A guest cannot view quiz history after session ends
- A guest session expires after inactivity timeout (configurable, default: 24 hours)
- A guest can convert to a registered user (preserving current session data)

## Account Invariants
- An account has a unique identifier (AccountId)
- An account belongs to exactly one user
- An account consists of a username and a password
- An account has an email (required for password reset and notifications)
- An account email must be verified before full access is granted
- An account can be in states: Pending, Active, Suspended, Deleted
- An account tracks last login timestamp
- An account tracks failed login attempts (for security lockout)
- An account can be locked after maximum failed attempts (default: 5)
- An account lockout duration is configurable (default: 15 minutes)

## Username Invariants
- A username consists of a display name and a discriminator tag (format: DisplayName#0000)
- A username display name must be 3-20 characters long
- A username display name can contain alphanumeric characters, underscores, and hyphens
- A username display name cannot start or end with special characters
- A username discriminator is a 4-digit number (0001-9999)
- A username (display name + discriminator combination) must be unique
- A username display name cannot contain offensive or prohibited words
- A user can change their display name (discriminator may change to maintain uniqueness)

## Password Invariants
- A password must be 8-64 characters long
- A password must contain at least one uppercase letter
- A password must contain at least one lowercase letter
- A password must contain at least one digit
- A password must contain at least one special character (!@#$%^&*()_+-=[]{}|;:,.<>?)
- A password cannot contain the username
- A password is stored as a secure hash (handled by Keycloak)
- A password can be reset via email verification link
- A password reset link expires after 24 hours

## Email Invariants
- An email must be a valid email format
- An email must be unique across all accounts
- An email verification token expires after 48 hours

## RefreshToken Invariants
- A refresh token belongs to one user/guest session
- A refresh token has an expiration time (configurable, default: 7 days)
- A refresh token can be revoked
- A refresh token is rotated on each use (sliding expiration)
- A user can have multiple active refresh tokens (multiple devices)
- Maximum refresh tokens per user is configurable (default: 5)

---

# Quiz Module

## Quiz Invariants
- A quiz has a unique identifier (QuizId)
- A quiz has a creator (User)
- A quiz has a title (required, 3-100 characters)
- A quiz has an optional description (max 500 characters)
- A quiz consists of questions (1 to maximum)
- A quiz cannot contain more than the maximum number of questions (default: 10)
- A quiz has a category (optional)
- A quiz has a difficulty level (Easy, Medium, Hard)
- A quiz can be in states: Draft, Published, Archived
- A quiz in Draft state can be edited
- A quiz in Published state cannot be edited (must create new version)
- A quiz can be soft-deleted (Archived)
- A quiz has a creation timestamp
- A quiz has a last modified timestamp
- A quiz tracks total times played
- A quiz tracks average score
- A quiz questions are ordered (sequence number)

## Question Invariants
- A question has a unique identifier (QuestionId)
- A question belongs to exactly one quiz
- A question has text content (required, 1-250 characters)
- A question has a sequence/order number within the quiz
- A question has multiple answer options (QuestionItems)
- A question cannot contain more than the maximum number of answer options (default: 5)
- A question must have at least 2 answer options
- A question has exactly one correct answer
- A question has a point value (configurable, default: 100)
- A question has an optional hint/tip
- A question hint has a point penalty when used (percentage of points, default: 50%)
- A question has a time limit in seconds (configurable per question, default: 30 seconds)
- A question can have an optional image attachment
- A question image must be under maximum size (default: 2MB)
- A question image must be in allowed formats (JPEG, PNG, GIF, WebP)

## QuestionItem (Answer Option) Invariants
- A question item has a unique identifier (QuestionItemId)
- A question item belongs to exactly one question
- A question item has text content (required, 1-100 characters)
- A question item has a flag indicating if it's the correct answer
- A question item has a sequence/order number
- A question item can have an optional image attachment

## Category Invariants
- A category has a unique identifier (CategoryId)
- A category has a name (required, unique, 2-50 characters)
- A category has an optional description (max 200 characters)
- A category has an optional icon/image
- A category can be active or inactive
- A category can have a parent category (hierarchical, max 2 levels)
- A category tracks quiz count

---

# Session Module

## QuizSession Invariants
- A quiz session has a unique identifier (SessionId)
- A quiz session is based on exactly one quiz
- A quiz session has a host (the user who created it)
- A quiz session has a unique join code (6 alphanumeric characters, case-insensitive)
- A quiz session join code expires when session ends or after timeout
- A quiz session can be Public or Private (visibility)
- A public quiz session appears in lobby/browse list
- A private quiz session requires join code or invitation link
- A quiz session has a maximum number of participants (default: 10, configurable 2-50)
- A quiz session has a minimum number of participants to start (default: 2)
- A quiz session has states: Lobby, Starting, InProgress, Paused, Completed, Cancelled
- A quiz session in Lobby state accepts new participants
- A quiz session in Lobby state has a countdown timer before auto-cancel (default: 10 minutes)
- A quiz session can only transition to Starting when minimum participants joined
- A quiz session in Starting state has a countdown (default: 5 seconds) before InProgress
- A quiz session in InProgress state does not accept new participants
- A quiz session in InProgress state tracks current question index
- A quiz session in InProgress state has per-question timer running
- A quiz session can be paused only by host (if enabled)
- A quiz session pause has maximum duration (default: 5 minutes)
- A quiz session transitions to Completed when all questions answered or time expires
- A quiz session can be Cancelled by host at any time before Completed
- A quiz session has an overall time limit (default: 5 minutes, calculated from questions)
- A quiz session has creation timestamp
- A quiz session has start timestamp (when InProgress begins)
- A quiz session has end timestamp (when Completed/Cancelled)
- A quiz session generates a QuizSummary upon completion

## Participant Invariants
- A participant has a unique identifier (ParticipantId)
- A participant belongs to exactly one quiz session
- A participant is either a User or a Guest
- A participant has a display name (shown during session)
- A participant has a current score (starts at 0)
- A participant has a current rank/position
- A participant has states: Joined, Ready, Playing, Finished, Disconnected, Left
- A participant in Joined state can mark themselves as Ready
- A participant must be Ready for session to start (or auto-ready after timeout)
- A participant tracks connection status (for reconnection handling)
- A participant can reconnect within grace period (default: 30 seconds)
- A participant who disconnects beyond grace period is marked as Left
- A participant has join timestamp
- A participant has answer history for the session

## Answer Invariants
- An answer has a unique identifier (AnswerId)
- An answer belongs to exactly one participant
- An answer is for exactly one question in the session
- An answer references selected question item (can be null if timed out)
- An answer has submission timestamp
- An answer tracks time taken to answer (milliseconds from question display)
- An answer has a flag indicating if correct
- An answer has points awarded (0 if incorrect or timed out)
- An answer tracks if hint was used
- An answer can only be submitted once per question per participant
- An answer must be submitted before question timer expires
- An answer submitted after timer expires awards 0 points

## Score Invariants
- Score is calculated based on correctness and speed
- Base points come from question point value
- Time bonus is calculated as: (remaining_time / total_time) * bonus_multiplier
- Maximum time bonus percentage is configurable (default: 50%)
- Hint usage reduces points by hint penalty percentage
- Streak bonus: consecutive correct answers multiply points (configurable)
- Streak multiplier caps at maximum (default: 2x after 5 consecutive)
- Streak resets on incorrect answer
- Minimum score per question is 0 (no negative scores)

## QuizSummary Invariants
- A quiz summary has a unique identifier (SummaryId)
- A quiz summary is generated for exactly one completed quiz session
- A quiz summary records final standings (all participants with ranks)
- A quiz summary identifies winner (1st place)
- A quiz summary identifies podium (1st, 2nd, 3rd place)
- A quiz summary records each participant's total score
- A quiz summary records each participant's correct answer count
- A quiz summary records each participant's average response time
- A quiz summary records session duration
- A quiz summary records timestamp of completion
- A quiz summary stores per-question statistics (% correct, avg time)
- A quiz summary is immutable once created
- A quiz summary can be shared via link
- A quiz summary visibility matches original session visibility (Public/Private)

## Leaderboard Invariants
- A leaderboard is real-time during active session
- A leaderboard shows all participants ranked by score
- A leaderboard updates after each answer submission
- A leaderboard shows: rank, display name, score, correct answers
- A leaderboard ties are broken by: fewer total time, then join order
- A global leaderboard exists for all-time statistics
- Global leaderboard can be filtered by: time period, category, friends only

---

# Social Module

## Friendship Invariants
- A friendship has a unique identifier (FriendshipId)
- A friendship is between exactly two users
- A friendship is bidirectional (if A friends B, B friends A)
- A friendship has creation timestamp
- A friendship can be removed by either user
- A friendship removal is immediate and bidirectional
- Users can have maximum number of friends (configurable, default: 500)

## FriendRequest Invariants
- A friend request has a unique identifier (FriendRequestId)
- A friend request has a sender (User)
- A friend request has a receiver (User)
- A friend request has states: Pending, Accepted, Rejected, Cancelled, Expired
- A friend request cannot be sent to self
- A friend request cannot be sent if friendship already exists
- A friend request cannot be sent if pending request already exists (either direction)
- A friend request cannot be sent to a blocked user
- A friend request cannot be sent by a blocked user
- A friend request has creation timestamp
- A friend request expires after duration (configurable, default: 30 days)
- A friend request can be cancelled by sender while Pending
- Accepting a friend request creates a Friendship
- A user can have maximum pending outgoing requests (configurable, default: 100)

## Block Invariants
- A block has a unique identifier (BlockId)
- A block has a blocker (User who initiated)
- A block has a blocked user (User who is blocked)
- A block is unidirectional (A blocks B doesn't mean B blocks A)
- A block removes existing friendship if present
- A block cancels any pending friend requests between users
- A blocked user cannot: send friend requests, see online status, invite to quiz
- A block has creation timestamp
- A block can be removed by blocker at any time

---

# Notification Module

## Notification Invariants
- A notification has a unique identifier (NotificationId)
- A notification has a recipient (User)
- A notification has a type (FriendRequest, QuizInvite, SessionStarting, Achievement, System)
- A notification has a title (required, max 100 characters)
- A notification has a body/message (required, max 500 characters)
- A notification has optional action data (JSON payload for deep linking)
- A notification has states: Unread, Read, Dismissed
- A notification has creation timestamp
- A notification has read timestamp (if Read)
- A notification expires after duration (configurable by type, default: 30 days)
- A notification can be sent as push notification (if user enabled)
- A notification can be sent as email (for important types, if user enabled)
- A user's unread notification count is tracked
- Old notifications are auto-archived after threshold (configurable, default: 100)

## NotificationPreference Invariants
- Notification preferences belong to exactly one user
- Preferences control: push notifications, email notifications, in-app notifications
- Preferences can be set per notification type
- Default preferences are created on account creation
- Preferences include quiet hours setting (no push during specified times)

---

# Cross-Cutting Concerns

## Audit Log Invariants
- Audit logs track important domain events
- Audit log has: timestamp, actor (user/system), action, entity type, entity id, changes
- Audit logs are immutable
- Audit logs are retained for configurable duration (default: 90 days)

## Soft Delete Invariants
- Soft deleted entities have IsDeleted flag
- Soft deleted entities have DeletedAt timestamp
- Soft deleted entities are excluded from normal queries
- Soft deleted entities can be restored within retention period
- Soft deleted entities are permanently purged after retention period

## Timestamp Invariants
- All entities have CreatedAt timestamp (set once on creation)
- Mutable entities have UpdatedAt timestamp (updated on each modification)
- All timestamps are stored in UTC
- All timestamps are returned in ISO 8601 format

## Concurrency Invariants
- Optimistic concurrency is used for entities with high contention
- Version/RowVersion field tracks entity version
- Concurrent modification results in conflict response (HTTP 409)
- Client must retry with fresh data on conflict

---

# Event Definitions (For Module Communication)

## Identity Module Events
- UserRegistered { UserId, Username, Email, Timestamp }
- UserLoggedIn { UserId, Timestamp, DeviceInfo }
- UserLoggedOut { UserId, Timestamp }
- UserProfileUpdated { UserId, Changes, Timestamp }
- UserDeleted { UserId, Timestamp }
- GuestCreated { GuestId, DisplayName, Timestamp }
- GuestConverted { GuestId, UserId, Timestamp }
- GuestExpired { GuestId, Timestamp }

## Quiz Module Events
- QuizCreated { QuizId, CreatorId, Title, Timestamp }
- QuizPublished { QuizId, Timestamp }
- QuizArchived { QuizId, Timestamp }

## Session Module Events
- SessionCreated { SessionId, QuizId, HostId, JoinCode, Visibility, Timestamp }
- SessionStarted { SessionId, Timestamp }
- ParticipantJoined { SessionId, ParticipantId, UserId/GuestId, Timestamp }
- ParticipantLeft { SessionId, ParticipantId, Timestamp }
- ParticipantAnswered { SessionId, ParticipantId, QuestionId, IsCorrect, Points, Timestamp }
- QuestionStarted { SessionId, QuestionIndex, Timestamp }
- QuestionEnded { SessionId, QuestionIndex, Timestamp }
- SessionCompleted { SessionId, WinnerId, Timestamp }
- SessionCancelled { SessionId, Reason, Timestamp }
- SummaryGenerated { SummaryId, SessionId, Timestamp }

## Social Module Events
- FriendRequestSent { RequestId, SenderId, ReceiverId, Timestamp }
- FriendRequestAccepted { RequestId, Timestamp }
- FriendRequestRejected { RequestId, Timestamp }
- FriendshipCreated { FriendshipId, User1Id, User2Id, Timestamp }
- FriendshipRemoved { FriendshipId, Timestamp }
- UserBlocked { BlockId, BlockerId, BlockedId, Timestamp }
- UserUnblocked { BlockId, Timestamp }

## Notification Module Events
- NotificationCreated { NotificationId, RecipientId, Type, Timestamp }
- NotificationRead { NotificationId, Timestamp }
- NotificationDismissed { NotificationId, Timestamp }

---

# Configuration Constants

| Constant | Default Value | Description |
|----------|---------------|-------------|
| MAX_QUESTIONS_PER_QUIZ | 10 | Maximum questions in a quiz |
| MAX_OPTIONS_PER_QUESTION | 5 | Maximum answer options per question |
| MIN_OPTIONS_PER_QUESTION | 2 | Minimum answer options per question |
| MAX_PARTICIPANTS_PER_SESSION | 10 | Maximum players in a session |
| MIN_PARTICIPANTS_TO_START | 2 | Minimum players to start session |
| DEFAULT_QUESTION_TIME_SECONDS | 30 | Default time per question |
| DEFAULT_SESSION_LOBBY_TIMEOUT_MINUTES | 10 | Auto-cancel if not started |
| DEFAULT_QUESTION_POINTS | 100 | Base points per question |
| TIME_BONUS_MAX_PERCENTAGE | 50 | Maximum time bonus % |
| HINT_PENALTY_PERCENTAGE | 50 | Points reduction for hint |
| MAX_STREAK_MULTIPLIER | 2.0 | Maximum streak bonus |
| STREAK_THRESHOLD | 5 | Correct answers for max streak |
| RECONNECT_GRACE_PERIOD_SECONDS | 30 | Time to reconnect |
| USERNAME_MIN_LENGTH | 3 | Minimum display name length |
| USERNAME_MAX_LENGTH | 20 | Maximum display name length |
| PASSWORD_MIN_LENGTH | 8 | Minimum password length |
| PASSWORD_MAX_LENGTH | 64 | Maximum password length |
| FRIEND_REQUEST_EXPIRY_DAYS | 30 | Friend request expiration |
| MAX_FRIENDS | 500 | Maximum friends per user |
| REFRESH_TOKEN_EXPIRY_DAYS | 7 | Refresh token lifetime |
| MAX_REFRESH_TOKENS_PER_USER | 5 | Max devices per user |
| ACCOUNT_LOCKOUT_THRESHOLD | 5 | Failed attempts before lock |
| ACCOUNT_LOCKOUT_DURATION_MINUTES | 15 | Lockout duration |
