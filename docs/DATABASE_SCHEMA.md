# AI-ATC Database Schema Design

## Overview

PostgreSQL database schema for the AI-ATC system, designed for scalability, performance, and data integrity.

---

## Entity Relationship Diagram

```
Users
  ├─> Sessions (1:N)
  ├─> Scores (1:N)
  └─> SavedScenarios (1:N)

Scenarios
  ├─> ScenarioAircraft (1:N)
  ├─> SavedScenarios (1:N)
  └─> Scores (1:N)

Airports
  ├─> Runways (1:N)
  ├─> Fixes (1:N)
  └─> Procedures (1:N)

Sessions
  ├─> SessionCommands (1:N)
  └─> SessionEvents (1:N)
```

---

## Tables

### Users
Stores user account information and authentication data.

```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    username VARCHAR(50) UNIQUE NOT NULL,
    oauth_provider VARCHAR(50) NOT NULL,  -- 'google', 'microsoft', 'github'
    oauth_subject_id VARCHAR(255) NOT NULL,
    display_name VARCHAR(100),
    avatar_url VARCHAR(500),
    role VARCHAR(20) NOT NULL DEFAULT 'user',  -- 'user', 'superuser'
    is_guest BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    last_login_at TIMESTAMP,
    settings JSONB,  -- User preferences (theme, audio settings, etc.)
    UNIQUE(oauth_provider, oauth_subject_id)
);

CREATE INDEX idx_users_oauth ON users(oauth_provider, oauth_subject_id);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);
```

---

### Sessions
Tracks active and completed user sessions.

```sql
CREATE TABLE sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    scenario_id UUID REFERENCES scenarios(id) ON DELETE SET NULL,
    started_at TIMESTAMP NOT NULL DEFAULT NOW(),
    ended_at TIMESTAMP,
    duration_seconds INT,
    score INT,
    aircraft_controlled INT DEFAULT 0,
    commands_issued INT DEFAULT 0,
    separation_violations INT DEFAULT 0,
    successful_landings INT DEFAULT 0,
    successful_handoffs INT DEFAULT 0,
    time_acceleration FLOAT DEFAULT 1.0,
    final_score_breakdown JSONB,  -- Detailed score components
    state_snapshot BYTEA,  -- Compressed simulation state for replay
    status VARCHAR(20) DEFAULT 'active',  -- 'active', 'completed', 'abandoned'
    CONSTRAINT chk_time_acceleration CHECK (time_acceleration > 0 AND time_acceleration <= 10)
);

CREATE INDEX idx_sessions_user ON sessions(user_id);
CREATE INDEX idx_sessions_scenario ON sessions(scenario_id);
CREATE INDEX idx_sessions_started ON sessions(started_at DESC);
CREATE INDEX idx_sessions_status ON sessions(status) WHERE status = 'active';
```

---

### SessionCommands
Stores individual commands issued during a session (for replay).

```sql
CREATE TABLE session_commands (
    id BIGSERIAL PRIMARY KEY,
    session_id UUID NOT NULL REFERENCES sessions(id) ON DELETE CASCADE,
    simulation_time FLOAT NOT NULL,  -- Seconds since session start
    aircraft_id VARCHAR(20) NOT NULL,
    command_type VARCHAR(50) NOT NULL,  -- 'heading', 'altitude', 'speed', 'direct', 'approach', 'contact'
    command_text TEXT NOT NULL,
    command_params JSONB,  -- Structured command parameters
    response_time_ms INT,
    issued_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_session_commands_session ON session_commands(session_id, simulation_time);
```

---

### SessionEvents
Stores important events during sessions (for analysis and replay).

```sql
CREATE TABLE session_events (
    id BIGSERIAL PRIMARY KEY,
    session_id UUID NOT NULL REFERENCES sessions(id) ON DELETE CASCADE,
    simulation_time FLOAT NOT NULL,
    event_type VARCHAR(50) NOT NULL,  -- 'landing', 'separation_violation', 'handoff', 'go_around'
    aircraft_ids TEXT[],
    event_data JSONB,
    occurred_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_session_events_session ON session_events(session_id, simulation_time);
CREATE INDEX idx_session_events_type ON session_events(event_type);
```

---

### Scores
Stores high scores and score history.

```sql
CREATE TABLE scores (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    session_id UUID UNIQUE REFERENCES sessions(id) ON DELETE SET NULL,
    scenario_id UUID NOT NULL REFERENCES scenarios(id) ON DELETE CASCADE,
    score INT NOT NULL,
    time_acceleration FLOAT NOT NULL DEFAULT 1.0,
    adjusted_score INT NOT NULL,  -- Score with time multiplier applied
    aircraft_controlled INT NOT NULL,
    commands_issued INT NOT NULL,
    efficiency_rating FLOAT,
    safety_rating FLOAT,
    achieved_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_score_positive CHECK (score >= 0),
    CONSTRAINT chk_adjusted_score_positive CHECK (adjusted_score >= 0)
);

CREATE INDEX idx_scores_user ON scores(user_id);
CREATE INDEX idx_scores_scenario ON scores(scenario_id);
CREATE INDEX idx_scores_leaderboard ON scores(scenario_id, adjusted_score DESC, achieved_at DESC);
CREATE INDEX idx_scores_achieved_at ON scores(achieved_at DESC);
```

---

### Scenarios
Defines scenario configurations and challenges.

```sql
CREATE TABLE scenarios (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    description TEXT,
    difficulty_level INT NOT NULL,  -- 1-5
    airport_code VARCHAR(4) NOT NULL,  -- ICAO code (KSFO, KLAX, etc.)
    scenario_type VARCHAR(50) NOT NULL,  -- 'preset', 'custom', 'live'
    duration_minutes INT,
    max_aircraft INT,
    weather_conditions JSONB,  -- Wind, visibility, ceiling
    initial_aircraft_states JSONB,  -- Starting positions, altitudes, etc.
    active_runways TEXT[],
    active_frequencies JSONB,  -- {sector: frequency}
    created_by UUID REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    is_public BOOLEAN DEFAULT false,
    play_count INT DEFAULT 0,
    average_score FLOAT,
    tags TEXT[],
    CONSTRAINT chk_difficulty CHECK (difficulty_level BETWEEN 1 AND 5)
);

CREATE INDEX idx_scenarios_airport ON scenarios(airport_code);
CREATE INDEX idx_scenarios_difficulty ON scenarios(difficulty_level);
CREATE INDEX idx_scenarios_public ON scenarios(is_public) WHERE is_public = true;
CREATE INDEX idx_scenarios_tags ON scenarios USING GIN(tags);
```

---

### SavedScenarios
Allows users to save progress in scenarios.

```sql
CREATE TABLE saved_scenarios (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    scenario_id UUID NOT NULL REFERENCES scenarios(id) ON DELETE CASCADE,
    save_name VARCHAR(200),
    simulation_state BYTEA NOT NULL,  -- Compressed state
    simulation_time FLOAT NOT NULL,
    current_score INT NOT NULL,
    saved_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, scenario_id, save_name)
);

CREATE INDEX idx_saved_scenarios_user ON saved_scenarios(user_id);
```

---

### Airports
Reference data for airports.

```sql
CREATE TABLE airports (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    icao_code VARCHAR(4) UNIQUE NOT NULL,
    iata_code VARCHAR(3),
    name VARCHAR(200) NOT NULL,
    latitude NUMERIC(10, 7) NOT NULL,
    longitude NUMERIC(10, 7) NOT NULL,
    elevation_ft INT NOT NULL,
    timezone VARCHAR(50),
    country_code VARCHAR(2),
    CONSTRAINT chk_latitude CHECK (latitude BETWEEN -90 AND 90),
    CONSTRAINT chk_longitude CHECK (longitude BETWEEN -180 AND 180)
);

CREATE INDEX idx_airports_icao ON airports(icao_code);
CREATE INDEX idx_airports_location ON airports(latitude, longitude);
```

---

### Runways
Runway configurations for airports.

```sql
CREATE TABLE runways (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    airport_id UUID NOT NULL REFERENCES airports(id) ON DELETE CASCADE,
    runway_identifier VARCHAR(5) NOT NULL,  -- '27L', '09R', etc.
    magnetic_heading INT NOT NULL,
    length_ft INT NOT NULL,
    width_ft INT NOT NULL,
    surface_type VARCHAR(20),
    has_ils BOOLEAN DEFAULT false,
    localizer_frequency NUMERIC(6, 3),  -- MHz
    glideslope_angle NUMERIC(3, 1) DEFAULT 3.0,
    displaced_threshold_ft INT DEFAULT 0,
    elevation_ft INT NOT NULL,
    latitude_threshold NUMERIC(10, 7),
    longitude_threshold NUMERIC(10, 7),
    CONSTRAINT chk_heading CHECK (magnetic_heading BETWEEN 0 AND 359),
    UNIQUE(airport_id, runway_identifier)
);

CREATE INDEX idx_runways_airport ON runways(airport_id);
```

---

### Fixes
Navigation fixes (waypoints) for procedure design.

```sql
CREATE TABLE fixes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    fix_identifier VARCHAR(5) NOT NULL UNIQUE,
    name VARCHAR(100),
    latitude NUMERIC(10, 7) NOT NULL,
    longitude NUMERIC(10, 7) NOT NULL,
    type VARCHAR(20),  -- 'waypoint', 'vor', 'ndb', 'airport'
    associated_airport_id UUID REFERENCES airports(id) ON DELETE SET NULL,
    CONSTRAINT chk_fix_latitude CHECK (latitude BETWEEN -90 AND 90),
    CONSTRAINT chk_fix_longitude CHECK (longitude BETWEEN -180 AND 180)
);

CREATE INDEX idx_fixes_identifier ON fixes(fix_identifier);
CREATE INDEX idx_fixes_location ON fixes(latitude, longitude);
CREATE INDEX idx_fixes_airport ON fixes(associated_airport_id);
```

---

### Procedures
Standard instrument departures and arrivals.

```sql
CREATE TABLE procedures (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    airport_id UUID NOT NULL REFERENCES airports(id) ON DELETE CASCADE,
    runway_id UUID REFERENCES runways(id) ON DELETE SET NULL,
    procedure_type VARCHAR(10) NOT NULL,  -- 'SID', 'STAR', 'IAP'
    procedure_name VARCHAR(100) NOT NULL,
    procedure_identifier VARCHAR(20),
    waypoints JSONB NOT NULL,  -- Array of {fix_id, altitude_constraint, speed_constraint}
    minimum_altitude_ft INT,
    weather_minimums JSONB,  -- {visibility_sm, ceiling_ft}
    notes TEXT,
    CONSTRAINT chk_procedure_type CHECK (procedure_type IN ('SID', 'STAR', 'IAP')),
    UNIQUE(airport_id, procedure_type, procedure_name)
);

CREATE INDEX idx_procedures_airport ON procedures(airport_id);
CREATE INDEX idx_procedures_runway ON procedures(runway_id);
CREATE INDEX idx_procedures_type ON procedures(procedure_type);
```

---

### Weather
Historical and forecasted weather data.

```sql
CREATE TABLE weather (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    airport_id UUID NOT NULL REFERENCES airports(id) ON DELETE CASCADE,
    valid_from TIMESTAMP NOT NULL,
    valid_to TIMESTAMP NOT NULL,
    wind_direction_deg INT,
    wind_speed_kts INT,
    wind_gust_kts INT,
    visibility_sm NUMERIC(4, 2),
    ceiling_ft INT,
    temperature_c INT,
    dewpoint_c INT,
    altimeter_inhg NUMERIC(5, 2),
    weather_phenomena TEXT[],  -- ['rain', 'fog', 'snow']
    metar_raw TEXT,
    taf_raw TEXT,
    source VARCHAR(20),  -- 'live', 'historical', 'synthetic'
    fetched_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_wind_direction CHECK (wind_direction_deg BETWEEN 0 AND 359 OR wind_direction_deg IS NULL)
);

CREATE INDEX idx_weather_airport ON weather(airport_id);
CREATE INDEX idx_weather_valid_from ON weather(valid_from DESC);
CREATE INDEX idx_weather_airport_time ON weather(airport_id, valid_from DESC);
```

---

### Achievements
Gamification badges and achievements.

```sql
CREATE TABLE achievements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    icon_url VARCHAR(500),
    tier VARCHAR(20),  -- 'bronze', 'silver', 'gold', 'platinum'
    criteria JSONB NOT NULL,  -- {type: 'landings', threshold: 100}
    points INT DEFAULT 0
);

CREATE TABLE user_achievements (
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    achievement_id UUID NOT NULL REFERENCES achievements(id) ON DELETE CASCADE,
    earned_at TIMESTAMP NOT NULL DEFAULT NOW(),
    progress JSONB,
    PRIMARY KEY (user_id, achievement_id)
);

CREATE INDEX idx_user_achievements_user ON user_achievements(user_id);
CREATE INDEX idx_user_achievements_earned_at ON user_achievements(earned_at DESC);
```

---

## Indexes Summary

**Performance optimizations:**
- Primary keys on all tables (automatic B-tree index)
- Foreign key indexes for join performance
- Composite indexes for leaderboard queries
- GIN indexes for JSONB and array columns
- Partial indexes for active sessions

**Query patterns supported:**
- User lookup by OAuth credentials
- Leaderboard queries by scenario and time frame
- Session replay data retrieval
- Scenario search by difficulty, airport, tags
- Weather lookup by airport and time range

---

## Partitioning Strategy

For large datasets, consider partitioning:

**session_commands** - Partition by month (range on issued_at)
**session_events** - Partition by month (range on occurred_at)
**scores** - Partition by quarter (range on achieved_at)

---

## Data Retention Policy

- **Active sessions**: Keep indefinitely
- **Completed sessions**: Full detail for 90 days, summary only after
- **Session commands/events**: 90 days for non-high-score sessions
- **High score sessions**: Keep replay data indefinitely
- **Guest user data**: Purge after 30 days of inactivity

---

## Backup Strategy

- **Full backup**: Daily at 02:00 UTC
- **Incremental backup**: Every 6 hours
- **WAL archiving**: Continuous
- **Retention**: 30 days for backups
- **Testing**: Monthly restore test

---

## Migration Strategy

1. Initial migration creates all tables and indexes
2. Seed reference data (airports, runways, fixes, procedures)
3. Seed sample scenarios for each difficulty level
4. Seed achievements
5. Create stored procedures for common queries (leaderboards, statistics)

---

## Performance Targets

- User authentication: < 50ms
- Scenario load: < 100ms
- Score save: < 50ms
- Leaderboard query: < 200ms
- Session replay load: < 500ms
