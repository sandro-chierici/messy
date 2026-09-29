CREATE TABLE tenants (
    id                   SERIAL          PRIMARY KEY,
    tenant_id            UUID            NOT NULL UNIQUE,
    name                 VARCHAR(255),
    code                 VARCHAR(100)    UNIQUE,
    legal_name           VARCHAR(255),
    tax_code             VARCHAR(100),
    country              CHAR(2),
    time_zone            VARCHAR(100),
    locale               VARCHAR(10),
    industry_type        VARCHAR(100),
    is_active            BOOLEAN         NOT NULL DEFAULT TRUE,
    license_type         VARCHAR(100),
    license_expires_at   TIMESTAMP,
    max_users            INTEGER,
    max_machines         INTEGER,
    created_utc_date     TIMESTAMP,
    updated_utc_date     TIMESTAMP,
    created_by           VARCHAR(255),
    ext_props            JSONB
);

CREATE INDEX idx_tenants_tenant_id  ON tenants (tenant_id);
CREATE INDEX idx_tenants_code       ON tenants (code);
CREATE INDEX idx_tenants_is_active  ON tenants (is_active);



-- ============================================================================
-- Users (OFBiz Party model, core subset). Rules: see .claude/skills/create-entity
--   PK = id (int); external key = <entity>_id (UUID v7); FK columns = <entity>_pk (int).
-- ============================================================================

-- OFBiz: PartyType (reference table: natural code, no GUID)
CREATE TABLE party_types (
    id           SERIAL          PRIMARY KEY,
    code         VARCHAR(50)     NOT NULL UNIQUE,
    parent_pk    INTEGER         REFERENCES party_types (id),
    description  VARCHAR(255)
);

INSERT INTO party_types (code, description) VALUES
    ('PERSON',      'Person'),
    ('PARTY_GROUP', 'Group of parties');

CREATE INDEX idx_party_types_parent_pk ON party_types (parent_pk);


-- OFBiz: Party
CREATE TABLE parties (
    id                SERIAL          PRIMARY KEY,
    party_id          UUID            NOT NULL UNIQUE,
    tenant_pk         INTEGER         NOT NULL REFERENCES tenants (id),
    party_type_pk     INTEGER         NOT NULL REFERENCES party_types (id),
    external_id       VARCHAR(255),
    description       VARCHAR(500),
    is_active         BOOLEAN         NOT NULL DEFAULT TRUE,
    created_utc_date  TIMESTAMP,
    updated_utc_date  TIMESTAMP,
    created_by        VARCHAR(255),
    ext_props         JSONB,
    CONSTRAINT uq_parties_id_tenant_pk UNIQUE (id, tenant_pk)
);

CREATE INDEX idx_parties_tenant_pk      ON parties (tenant_pk);
CREATE INDEX idx_parties_party_type_pk  ON parties (party_type_pk);


-- OFBiz: Person (1:1 subtype of Party: PK is the party PK, external id is the party GUID)
CREATE TABLE persons (
    party_pk        INTEGER         PRIMARY KEY REFERENCES parties (id),
    salutation      VARCHAR(50),
    first_name      VARCHAR(100),
    middle_name     VARCHAR(100),
    last_name       VARCHAR(100),
    nickname        VARCHAR(100),
    personal_title  VARCHAR(100),
    suffix          VARCHAR(50),
    gender          CHAR(1),
    birth_date      DATE,
    comments        VARCHAR(1000)
);


-- OFBiz: UserLogin (one login per party in this subset)
CREATE TABLE user_logins (
    id                        SERIAL          PRIMARY KEY,
    user_login_id             UUID            NOT NULL UNIQUE,
    party_pk                  INTEGER         NOT NULL UNIQUE,
    tenant_pk                 INTEGER         NOT NULL,
    username                  VARCHAR(100)    NOT NULL,
    password_hash             VARCHAR(500)    NOT NULL,
    enabled                   BOOLEAN         NOT NULL DEFAULT TRUE,
    is_system                 BOOLEAN         NOT NULL DEFAULT FALSE,
    require_password_change   BOOLEAN         NOT NULL DEFAULT FALSE,
    successive_failed_logins  INTEGER         NOT NULL DEFAULT 0,
    disabled_utc_date         TIMESTAMP,
    last_locale               VARCHAR(10),
    last_time_zone            VARCHAR(100),
    external_auth_id          VARCHAR(255),
    created_utc_date          TIMESTAMP,
    updated_utc_date          TIMESTAMP,
    created_by                VARCHAR(255),
    -- a login can only belong to a party of its own tenant
    CONSTRAINT fk_user_logins_party_tenant
        FOREIGN KEY (party_pk, tenant_pk) REFERENCES parties (id, tenant_pk)
);

CREATE UNIQUE INDEX uq_user_logins_tenant_username ON user_logins (tenant_pk, lower(username));
CREATE INDEX idx_user_logins_tenant_pk ON user_logins (tenant_pk);


-- OFBiz: RoleType (reference table)
CREATE TABLE role_types (
    id           SERIAL          PRIMARY KEY,
    code         VARCHAR(50)     NOT NULL UNIQUE,
    parent_pk    INTEGER         REFERENCES role_types (id),
    description  VARCHAR(255)
);

INSERT INTO role_types (code, description) VALUES
    ('EMPLOYEE',   'Employee'),
    ('OPERATOR',   'Machine operator'),
    ('MAINTAINER', 'Maintenance technician'),
    ('SUPERVISOR', 'Supervisor'),
    ('ADMIN',      'Administrator');

CREATE INDEX idx_role_types_parent_pk ON role_types (parent_pk);


-- OFBiz: PartyRole (link table)
CREATE TABLE party_roles (
    party_pk      INTEGER  NOT NULL REFERENCES parties (id),
    role_type_pk  INTEGER  NOT NULL REFERENCES role_types (id),
    PRIMARY KEY (party_pk, role_type_pk)
);

CREATE INDEX idx_party_roles_role_type_pk ON party_roles (role_type_pk);


-- OFBiz: SecurityGroup (tenant_pk NULL = built-in group, visible to every tenant)
CREATE TABLE security_groups (
    id                 SERIAL          PRIMARY KEY,
    security_group_id  UUID            NOT NULL UNIQUE,
    tenant_pk          INTEGER         REFERENCES tenants (id),
    name               VARCHAR(100)    NOT NULL,
    description        VARCHAR(500)
);

CREATE UNIQUE INDEX uq_security_groups_tenant_name ON security_groups (COALESCE(tenant_pk, 0), lower(name));
CREATE INDEX idx_security_groups_tenant_pk ON security_groups (tenant_pk);


-- OFBiz: SecurityPermission (reference table)
CREATE TABLE security_permissions (
    id           SERIAL          PRIMARY KEY,
    code         VARCHAR(100)    NOT NULL UNIQUE,
    description  VARCHAR(255)
);

INSERT INTO security_permissions (code, description) VALUES
    ('TENANT_VIEW',     'View tenant data'),
    ('USER_VIEW',       'View users'),
    ('USER_ADMIN',      'Create, update and disable users'),
    ('SECURITY_VIEW',   'View security groups'),
    ('SECURITY_ADMIN',  'Manage security groups and assignments');


-- OFBiz: SecurityGroupPermission (link table)
CREATE TABLE security_group_permissions (
    security_group_pk       INTEGER  NOT NULL REFERENCES security_groups (id),
    security_permission_pk  INTEGER  NOT NULL REFERENCES security_permissions (id),
    PRIMARY KEY (security_group_pk, security_permission_pk)
);

CREATE INDEX idx_sgp_security_permission_pk ON security_group_permissions (security_permission_pk);


-- OFBiz: UserLoginSecurityGroup (link table with validity dates)
CREATE TABLE user_login_security_groups (
    user_login_pk      INTEGER    NOT NULL REFERENCES user_logins (id),
    security_group_pk  INTEGER    NOT NULL REFERENCES security_groups (id),
    from_date          TIMESTAMP  NOT NULL DEFAULT now(),
    thru_date          TIMESTAMP,
    PRIMARY KEY (user_login_pk, security_group_pk, from_date)
);

CREATE INDEX idx_ulsg_security_group_pk ON user_login_security_groups (security_group_pk);
