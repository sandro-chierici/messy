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
    created_by           VARCHAR(255)
);

CREATE INDEX idx_tenants_tenant_id  ON tenants (tenant_id);
CREATE INDEX idx_tenants_code       ON tenants (code);
CREATE INDEX idx_tenants_is_active  ON tenants (is_active);


CREATE TABLE tenants_ext (
    id          BIGSERIAL       PRIMARY KEY,
    tenant_id   UUID            NOT NULL REFERENCES tenants (tenant_id),
    name        VARCHAR(255)    NOT NULL,
    type        VARCHAR(50)     NOT NULL DEFAULT 'String',
    value       TEXT,
    is_deleted  BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_tenant_ext_tenant_id            ON tenants_ext (tenant_id);
CREATE INDEX idx_tenant_ext_tenant_id_name       ON tenants_ext (tenant_id, name);
CREATE INDEX idx_tenant_ext_is_deleted           ON tenants_ext (is_deleted);