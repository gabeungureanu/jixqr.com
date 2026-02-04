-- PostgreSQL Schema for jixqr database
-- Migrated from MS SQL Server JubileeGPT

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =====================================================
-- LOOKUP TABLES
-- =====================================================

-- Denominations
CREATE TABLE IF NOT EXISTS denominations (
    denomination_id SERIAL PRIMARY KEY,
    denomination_name VARCHAR(255) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    display_order INT DEFAULT 0
);

-- Languages
CREATE TABLE IF NOT EXISTS languages (
    language_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    language_name VARCHAR(100) NOT NULL,
    language_flag VARCHAR(100),
    category VARCHAR(100),
    prompt TEXT,
    is_default BOOLEAN DEFAULT FALSE,
    is_first_response_upend BOOLEAN DEFAULT FALSE,
    usage_count INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE
);

-- Timezones
CREATE TABLE IF NOT EXISTS timezones (
    timezone_id BIGSERIAL PRIMARY KEY,
    timezone_name VARCHAR(255) NOT NULL,
    utc_offset VARCHAR(20),
    is_active BOOLEAN DEFAULT TRUE
);

-- Date Formats
CREATE TABLE IF NOT EXISTS date_formats (
    date_format_id SERIAL PRIMARY KEY,
    date_format_description VARCHAR(255) NOT NULL,
    format_pattern VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE
);

-- =====================================================
-- USER & ACCOUNT TABLES
-- =====================================================

-- Users (main user table)
CREATE TABLE IF NOT EXISTS users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_user_id UUID,
    first_name VARCHAR(255),
    last_name VARCHAR(255),
    user_name VARCHAR(255) NOT NULL,
    email_address VARCHAR(255),
    password VARCHAR(500),
    discount_code VARCHAR(100),
    is_discount_code BOOLEAN DEFAULT FALSE,
    is_locked BOOLEAN DEFAULT FALSE,
    is_admin BOOLEAN DEFAULT FALSE,
    is_show_language BOOLEAN DEFAULT FALSE,
    account_status VARCHAR(50),
    is_email_verified BOOLEAN DEFAULT FALSE,
    is_term_condition BOOLEAN DEFAULT FALSE,
    profile_image VARCHAR(255),
    profile_image_path VARCHAR(500),
    profile_image_class VARCHAR(100),
    occupation VARCHAR(255),
    is_profile_image_class BOOLEAN DEFAULT FALSE,
    denomination_id INT REFERENCES denominations(denomination_id),
    language_id UUID REFERENCES languages(language_id),
    timezone_id BIGINT REFERENCES timezones(timezone_id),
    date_format_id INT REFERENCES date_formats(date_format_id),
    is_enable BOOLEAN DEFAULT TRUE,
    session_id VARCHAR(255),
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    modified_date TIMESTAMP
);

-- Login History
CREATE TABLE IF NOT EXISTS login_history (
    history_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    account_user_id UUID,
    ip_address VARCHAR(50),
    session_id VARCHAR(255),
    user_agent TEXT,
    login_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    logout_date TIMESTAMP
);

-- Testimonies
CREATE TABLE IF NOT EXISTS testimonies (
    testimony_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    testimony_text TEXT,
    testimony_name VARCHAR(255),
    testimony_city_state VARCHAR(255),
    status VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- SUBSCRIPTION & PAYMENT TABLES
-- =====================================================

-- Payment Plan Types
CREATE TABLE IF NOT EXISTS payment_plan_types (
    plan_type_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    plan_type_name VARCHAR(100) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE
);

-- Payment Plans
CREATE TABLE IF NOT EXISTS payment_plans (
    payment_plan_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    plan_name VARCHAR(255) NOT NULL,
    description TEXT,
    plan_price NUMERIC(18,2),
    featured_plan BOOLEAN DEFAULT FALSE,
    plan_type_id UUID REFERENCES payment_plan_types(plan_type_id),
    plan_status VARCHAR(50),
    plan_index INT,
    stripe_product_id VARCHAR(255),
    stripe_price_id VARCHAR(255),
    is_active BOOLEAN DEFAULT TRUE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Features
CREATE TABLE IF NOT EXISTS features (
    feature_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    feature_name VARCHAR(255) NOT NULL,
    description TEXT,
    basic_free BOOLEAN DEFAULT FALSE,
    standard BOOLEAN DEFAULT FALSE,
    deluxe BOOLEAN DEFAULT FALSE,
    professional BOOLEAN DEFAULT FALSE,
    enterprise BOOLEAN DEFAULT FALSE,
    display_order INT DEFAULT 0
);

-- Payment Plan Items
CREATE TABLE IF NOT EXISTS payment_plan_items (
    payment_plan_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    feature_id UUID REFERENCES features(feature_id),
    plan_type_id UUID REFERENCES payment_plan_types(plan_type_id),
    item VARCHAR(255),
    title VARCHAR(255),
    description TEXT,
    plan1 VARCHAR(100),
    plan2 VARCHAR(100),
    plan3 VARCHAR(100),
    plan4 VARCHAR(100),
    plan5 VARCHAR(100),
    plan6 VARCHAR(100)
);

-- User Subscriptions
CREATE TABLE IF NOT EXISTS user_subscriptions (
    subscription_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    payment_plan_id UUID REFERENCES payment_plans(payment_plan_id),
    subscription_status VARCHAR(50),
    stripe_subscription_id VARCHAR(255),
    stripe_customer_id VARCHAR(255),
    stripe_session_id VARCHAR(255),
    stripe_product_id VARCHAR(255),
    stripe_price_id VARCHAR(255),
    start_date TIMESTAMP,
    end_date TIMESTAMP,
    cancel_date TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Billing Information
CREATE TABLE IF NOT EXISTS billing_information (
    billing_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    customer_id VARCHAR(255),
    customer_name VARCHAR(255),
    email_address VARCHAR(255),
    phone_number VARCHAR(50),
    address_line1 VARCHAR(255),
    address_line2 VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(100),
    country VARCHAR(100),
    postal_code VARCHAR(20),
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    modified_date TIMESTAMP
);

-- Invoice Details
CREATE TABLE IF NOT EXISTS invoice_details (
    invoice_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    payment_plan_id UUID REFERENCES payment_plans(payment_plan_id),
    payment_method_id VARCHAR(255),
    subscription_id VARCHAR(255),
    customer_id VARCHAR(255),
    session_id VARCHAR(255),
    line_id VARCHAR(255),
    payment_intent_id VARCHAR(255),
    product_id VARCHAR(255),
    plan_created_date TIMESTAMP,
    plan_interval VARCHAR(50),
    price_id VARCHAR(255),
    price_created_date TIMESTAMP,
    price_interval VARCHAR(50),
    charge_id VARCHAR(255),
    amount_paid NUMERIC(18,2),
    amount_due BIGINT,
    amount_remaining BIGINT,
    status VARCHAR(50),
    recharge_type VARCHAR(50),
    invoice_number VARCHAR(100),
    invoice_pdf_link TEXT,
    hosted_invoice_pdf_link TEXT,
    attempt_count BIGINT,
    invoice_date TIMESTAMP,
    effective_date TIMESTAMP,
    period_start_date TIMESTAMP,
    period_end_date TIMESTAMP,
    currency VARCHAR(10),
    customer_name VARCHAR(255),
    customer_email VARCHAR(255),
    customer_phone VARCHAR(50),
    customer_address_line1 VARCHAR(255),
    customer_address_line2 VARCHAR(255),
    customer_address_city VARCHAR(100),
    customer_address_state VARCHAR(100),
    customer_address_country VARCHAR(100),
    customer_address_postal_code VARCHAR(20),
    card_brand VARCHAR(50),
    card_type VARCHAR(50),
    last4_digits VARCHAR(4),
    expiry_month BIGINT,
    expiry_year BIGINT,
    country VARCHAR(100),
    fingerprint VARCHAR(255),
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Payment Methods
CREATE TABLE IF NOT EXISTS payment_methods (
    payment_method_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    stripe_payment_method_id VARCHAR(255),
    type VARCHAR(50),
    card_brand VARCHAR(50),
    card_last4 VARCHAR(4),
    expire_month INT,
    expire_year INT,
    country VARCHAR(100),
    is_default BOOLEAN DEFAULT FALSE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Stripe Options (configuration)
CREATE TABLE IF NOT EXISTS stripe_options (
    stripe_option_id SERIAL PRIMARY KEY,
    website_id UUID,
    secret_key VARCHAR(500),
    publishable_key VARCHAR(500),
    webhook_secret VARCHAR(500),
    domain VARCHAR(255),
    is_active BOOLEAN DEFAULT TRUE
);

-- Token Tracking
CREATE TABLE IF NOT EXISTS token_usage (
    token_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    subscription_id UUID REFERENCES user_subscriptions(subscription_id),
    total_request_token BIGINT DEFAULT 0,
    total_response_token BIGINT DEFAULT 0,
    total_available_token BIGINT DEFAULT 0,
    total_remaining_token BIGINT DEFAULT 0,
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Webhook Logs
CREATE TABLE IF NOT EXISTS webhook_logs (
    log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type VARCHAR(100),
    event_id VARCHAR(255),
    payload TEXT,
    processed BOOLEAN DEFAULT FALSE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- AI/PROMPT TABLES
-- =====================================================

-- System Prompts (prefix prompts)
CREATE TABLE IF NOT EXISTS system_prompts (
    prompt_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    website_id UUID,
    system_prompt TEXT,
    voice_prompt TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- User Prompt Folders (conversation groupings)
CREATE TABLE IF NOT EXISTS user_prompt_folders (
    prompt_folder_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    website_id UUID,
    prompt_text TEXT,
    is_favourite BOOLEAN DEFAULT FALSE,
    is_archived BOOLEAN DEFAULT FALSE,
    share_chat_id UUID,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    modified_date TIMESTAMP
);

-- User Prompts (individual messages)
CREATE TABLE IF NOT EXISTS user_prompts (
    prompt_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    prompt_folder_id UUID REFERENCES user_prompt_folders(prompt_folder_id),
    parent_id UUID,
    user_id UUID REFERENCES users(user_id),
    website_id UUID,
    prompt TEXT,
    response TEXT,
    feedback_type VARCHAR(50),
    request_token INT DEFAULT 0,
    response_token INT DEFAULT 0,
    is_archived BOOLEAN DEFAULT FALSE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- User Memory
CREATE TABLE IF NOT EXISTS user_memory (
    memory_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    account_user_id UUID,
    website_id UUID,
    domain VARCHAR(255),
    memory_data TEXT,
    type VARCHAR(50),
    display_order INT,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Response Feedback
CREATE TABLE IF NOT EXISTS response_feedback (
    feedback_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    message_id UUID,
    prompt_id UUID REFERENCES user_prompts(prompt_id),
    feedback_type VARCHAR(50),
    selected_reason VARCHAR(255),
    optional_comment TEXT,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Feedback Reasons
CREATE TABLE IF NOT EXISTS feedback_reasons (
    reason_id SERIAL PRIMARY KEY,
    reason_text VARCHAR(255),
    is_active BOOLEAN DEFAULT TRUE,
    display_order INT DEFAULT 0
);

-- Initial Prompt Responses
CREATE TABLE IF NOT EXISTS initial_prompt_responses (
    response_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    website_id UUID,
    user_id UUID REFERENCES users(user_id),
    consumed_token INT DEFAULT 0,
    response TEXT,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Sub Prompts (mode prompts)
CREATE TABLE IF NOT EXISTS sub_prompts (
    sub_prompt_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_id UUID,
    website_id UUID,
    category VARCHAR(255),
    sub_prompt TEXT,
    icon VARCHAR(100),
    is_parent BOOLEAN DEFAULT FALSE,
    display_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE
);

-- Text to Speech Voices
CREATE TABLE IF NOT EXISTS tts_voices (
    voice_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    voice_name VARCHAR(100),
    voice_alt_name VARCHAR(100),
    provider VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE
);

-- User Voice Preferences
CREATE TABLE IF NOT EXISTS user_voices (
    user_voice_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    voice_id UUID REFERENCES tts_voices(voice_id),
    is_default BOOLEAN DEFAULT TRUE
);

-- =====================================================
-- SHARE REDIRECTOR / FILE MANAGEMENT TABLES
-- =====================================================

-- Authors
CREATE TABLE IF NOT EXISTS authors (
    author_id SERIAL PRIMARY KEY,
    author_name VARCHAR(255) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE
);

-- Series
CREATE TABLE IF NOT EXISTS series (
    series_id SERIAL PRIMARY KEY,
    author_id INT REFERENCES authors(author_id),
    series_name VARCHAR(255) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE
);

-- Products
CREATE TABLE IF NOT EXISTS products (
    product_id SERIAL PRIMARY KEY,
    series_id INT REFERENCES series(series_id),
    product_name VARCHAR(255) NOT NULL,
    product_image VARCHAR(500),
    thumbnail VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE
);

-- Delivery Methods
CREATE TABLE IF NOT EXISTS delivery_methods (
    delivery_id SERIAL PRIMARY KEY,
    delivery_name VARCHAR(255) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE
);

-- Stores
CREATE TABLE IF NOT EXISTS stores (
    store_id SERIAL PRIMARY KEY,
    store_name VARCHAR(255) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE
);

-- Containers (Azure Blob containers)
CREATE TABLE IF NOT EXISTS containers (
    container_id SERIAL PRIMARY KEY,
    container_name VARCHAR(255) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE
);

-- File Structure (folder hierarchy)
CREATE TABLE IF NOT EXISTS file_structure (
    file_id SERIAL PRIMARY KEY,
    nano_id VARCHAR(50),
    user_id UUID REFERENCES users(user_id),
    parent_id INT,
    root_id INT,
    folder_name VARCHAR(255),
    container_name VARCHAR(255),
    is_folder BOOLEAN DEFAULT FALSE,
    is_file BOOLEAN DEFAULT FALSE,
    folder_level VARCHAR(50),
    display_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Uploaded Files
CREATE TABLE IF NOT EXISTS uploaded_files (
    file_id SERIAL PRIMARY KEY,
    nano_id VARCHAR(50) UNIQUE,
    user_id UUID REFERENCES users(user_id),
    file_structure_id INT REFERENCES file_structure(file_id),
    file_name VARCHAR(255),
    share_description TEXT,
    azure_filename VARCHAR(500),
    free_azure_url TEXT,
    file_extension VARCHAR(20),
    redirect_url VARCHAR(1000),
    redirect_link VARCHAR(1000),
    redirect_url_qrcode VARCHAR(1000),
    url_map_link VARCHAR(1000),
    product_id INT REFERENCES products(product_id),
    delivery_id INT REFERENCES delivery_methods(delivery_id),
    series_id INT REFERENCES series(series_id),
    author_id INT REFERENCES authors(author_id),
    is_public BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    is_spotlight BOOLEAN DEFAULT FALSE,
    is_content_available BOOLEAN DEFAULT TRUE,
    duration VARCHAR(50),
    file_size VARCHAR(50),
    display_order INT DEFAULT 0,
    qr_color VARCHAR(50),
    qr_logo VARCHAR(255),
    marketing_qr_name VARCHAR(255),
    payment_qr_name VARCHAR(255),
    is_music_album BOOLEAN DEFAULT FALSE,
    is_audio_book BOOLEAN DEFAULT FALSE,
    is_video_file BOOLEAN DEFAULT FALSE,
    is_pdf_files BOOLEAN DEFAULT FALSE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    modified_date TIMESTAMP
);

-- Custom QR Settings
CREATE TABLE IF NOT EXISTS custom_qr_settings (
    qr_id SERIAL PRIMARY KEY,
    file_id INT REFERENCES uploaded_files(file_id),
    qr_color VARCHAR(50),
    qr_logo VARCHAR(255),
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Albums (Music Player)
CREATE TABLE IF NOT EXISTS albums (
    album_id SERIAL PRIMARY KEY,
    nano_id VARCHAR(50),
    file_id INT REFERENCES uploaded_files(file_id),
    file_name VARCHAR(255),
    share_description TEXT,
    azure_filename VARCHAR(500),
    file_extension VARCHAR(20),
    redirect_url VARCHAR(1000),
    redirect_link VARCHAR(1000),
    url_map_link VARCHAR(1000),
    duration VARCHAR(50),
    total_duration VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE,
    product VARCHAR(255),
    product_image VARCHAR(500),
    archive BOOLEAN DEFAULT FALSE,
    item_id INT,
    display_order INT DEFAULT 0
);

-- Redirect Hits (analytics)
CREATE TABLE IF NOT EXISTS redirect_hits (
    hit_id SERIAL PRIMARY KEY,
    nano_id VARCHAR(50),
    file_id INT,
    ip_address VARCHAR(50),
    user_agent TEXT,
    referrer TEXT,
    hit_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Stripe Payment Details (for share purchases)
CREATE TABLE IF NOT EXISTS stripe_payment_details (
    payment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    library_id VARCHAR(255),
    album_name VARCHAR(255),
    share_id VARCHAR(255),
    session_id VARCHAR(255),
    payment_intent_id VARCHAR(255),
    amount BIGINT,
    email VARCHAR(255),
    name VARCHAR(255),
    city VARCHAR(100),
    country VARCHAR(100),
    address_line1 VARCHAR(255),
    address_line2 VARCHAR(255),
    postal_code VARCHAR(20),
    payment_status VARCHAR(50),
    charge_id VARCHAR(255),
    card_brand VARCHAR(50),
    card_last4 VARCHAR(4),
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- CONTENT MANAGEMENT TABLES
-- =====================================================

-- Content Types
CREATE TABLE IF NOT EXISTS content_types (
    content_type_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    type_name VARCHAR(255) NOT NULL,
    description TEXT,
    navigation_text VARCHAR(255),
    is_active BOOLEAN DEFAULT TRUE
);

-- Book Content
CREATE TABLE IF NOT EXISTS book_content (
    book_content_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    content_type_id UUID REFERENCES content_types(content_type_id),
    parent_id UUID,
    book_code VARCHAR(100),
    book_title VARCHAR(500),
    publisher VARCHAR(255),
    number_of_chapters INT,
    book_theme TEXT,
    book_idea TEXT,
    book_goals TEXT,
    book_persona TEXT,
    description TEXT,
    display_order INT DEFAULT 0,
    indent INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Book Introduction
CREATE TABLE IF NOT EXISTS book_introduction (
    introduction_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    book_content_id UUID REFERENCES book_content(book_content_id),
    description TEXT,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Blueprint Content
CREATE TABLE IF NOT EXISTS blueprint_content (
    blueprint_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    book_content_id UUID REFERENCES book_content(book_content_id),
    blueprint_type VARCHAR(100),
    content TEXT,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- AI Configuration Settings
CREATE TABLE IF NOT EXISTS ai_configuration_settings (
    config_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    content_type_id UUID REFERENCES content_types(content_type_id),
    textbox_one TEXT,
    textbox_two TEXT,
    textbox_three TEXT,
    textbox_four TEXT,
    textbox_five TEXT,
    textbox_six TEXT,
    is_active BOOLEAN DEFAULT TRUE
);

-- =====================================================
-- WEBSITE & SYSTEM TABLES
-- =====================================================

-- Website Configuration
CREATE TABLE IF NOT EXISTS website_configuration (
    website_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    website_name VARCHAR(255) NOT NULL,
    domain_name VARCHAR(255) NOT NULL,
    website_prefix TEXT,
    website_middle TEXT,
    website_suffix TEXT,
    short_disclaimer TEXT,
    disclaimer TEXT,
    prompt_disclaimer TEXT,
    title_text VARCHAR(500),
    favicon_image_path VARCHAR(500),
    main_image_path VARCHAR(500),
    brand_image_path VARCHAR(500),
    main_image_alt_text VARCHAR(255),
    res_replaced_text TEXT,
    plan_name VARCHAR(255),
    is_active BOOLEAN DEFAULT TRUE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- SMTP Settings
CREATE TABLE IF NOT EXISTS smtp_settings (
    smtp_id SERIAL PRIMARY KEY,
    website_id UUID REFERENCES website_configuration(website_id),
    account_id INT,
    email_id INT,
    smtp_name VARCHAR(255),
    smtp_server VARCHAR(255),
    sender_email VARCHAR(255),
    sender_password VARCHAR(500),
    display_name VARCHAR(255),
    smtp_port INT,
    display_order INT,
    ssl_enable BOOLEAN DEFAULT TRUE,
    is_active BOOLEAN DEFAULT TRUE
);

-- Error Logs
CREATE TABLE IF NOT EXISTS error_logs (
    log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    website_id UUID,
    user_id UUID,
    error_message TEXT,
    stack_trace TEXT,
    source VARCHAR(255),
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Website Visitors
CREATE TABLE IF NOT EXISTS website_visitors (
    visitor_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    website_id UUID,
    ip_address VARCHAR(50),
    user_agent TEXT,
    page_visited VARCHAR(500),
    domain_name VARCHAR(255),
    visit_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Language Visitors
CREATE TABLE IF NOT EXISTS language_visitors (
    visitor_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    language_id UUID REFERENCES languages(language_id),
    website_id UUID,
    visit_count INT DEFAULT 1,
    last_visit TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Account Tokens
CREATE TABLE IF NOT EXISTS account_tokens (
    token_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    token_type VARCHAR(50),
    token_value VARCHAR(500),
    expires_at TIMESTAMP,
    is_used BOOLEAN DEFAULT FALSE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- INDEXES FOR PERFORMANCE
-- =====================================================

CREATE INDEX IF NOT EXISTS idx_users_email ON users(email_address);
CREATE INDEX IF NOT EXISTS idx_users_username ON users(user_name);
CREATE INDEX IF NOT EXISTS idx_user_prompts_user_id ON user_prompts(user_id);
CREATE INDEX IF NOT EXISTS idx_user_prompts_folder_id ON user_prompts(prompt_folder_id);
CREATE INDEX IF NOT EXISTS idx_user_prompt_folders_user_id ON user_prompt_folders(user_id);
CREATE INDEX IF NOT EXISTS idx_uploaded_files_nano_id ON uploaded_files(nano_id);
CREATE INDEX IF NOT EXISTS idx_uploaded_files_user_id ON uploaded_files(user_id);
CREATE INDEX IF NOT EXISTS idx_user_subscriptions_user_id ON user_subscriptions(user_id);
CREATE INDEX IF NOT EXISTS idx_invoice_details_user_id ON invoice_details(user_id);
CREATE INDEX IF NOT EXISTS idx_login_history_user_id ON login_history(user_id);
CREATE INDEX IF NOT EXISTS idx_redirect_hits_nano_id ON redirect_hits(nano_id);
CREATE INDEX IF NOT EXISTS idx_user_memory_user_id ON user_memory(user_id);

-- =====================================================
-- INITIAL DATA
-- =====================================================

-- Insert default denominations
INSERT INTO denominations (denomination_name, display_order) VALUES
('Non-Denominational', 1),
('Baptist', 2),
('Catholic', 3),
('Methodist', 4),
('Lutheran', 5),
('Presbyterian', 6),
('Pentecostal', 7),
('Anglican/Episcopal', 8),
('Other', 99)
ON CONFLICT DO NOTHING;

-- Insert default date formats
INSERT INTO date_formats (date_format_description, format_pattern) VALUES
('MM/DD/YYYY', 'MM/dd/yyyy'),
('DD/MM/YYYY', 'dd/MM/yyyy'),
('YYYY-MM-DD', 'yyyy-MM-dd'),
('DD-MM-YYYY', 'dd-MM-yyyy')
ON CONFLICT DO NOTHING;

-- Insert default language
INSERT INTO languages (language_name, language_flag, is_default) VALUES
('English', 'us', TRUE)
ON CONFLICT DO NOTHING;

COMMIT;
