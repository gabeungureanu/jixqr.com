-- PostgreSQL Functions for jixqr database
-- Migrated from MS SQL Server stored procedures

-- =====================================================
-- ACCOUNT / USER FUNCTIONS
-- =====================================================

-- Get Denominations
CREATE OR REPLACE FUNCTION get_denominations()
RETURNS TABLE (
    denomination INT,
    denominationname VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT d.denomination_id, d.denomination_name
    FROM denominations d
    WHERE d.is_active = TRUE
    ORDER BY d.display_order, d.denomination_id;
END;
$$ LANGUAGE plpgsql;

-- Get Languages
CREATE OR REPLACE FUNCTION website_language_get(p_website_id UUID DEFAULT NULL)
RETURNS TABLE (
    languageid UUID,
    languagename VARCHAR(100),
    languageflag VARCHAR(100),
    category VARCHAR(100),
    prompt TEXT,
    isdefault BOOLEAN,
    isfirstresponseupend BOOLEAN,
    usagecount INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        l.language_id,
        l.language_name,
        l.language_flag,
        l.category,
        l.prompt,
        l.is_default,
        l.is_first_response_upend,
        l.usage_count
    FROM languages l
    WHERE l.is_active = TRUE
    ORDER BY l.is_default DESC, l.language_name;
END;
$$ LANGUAGE plpgsql;

-- Get Timezones
CREATE OR REPLACE FUNCTION system_timezones_get()
RETURNS TABLE (
    timezone BIGINT,
    timezonename VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT t.timezone_id, t.timezone_name
    FROM timezones t
    WHERE t.is_active = TRUE
    ORDER BY t.timezone_id;
END;
$$ LANGUAGE plpgsql;

-- Get Date Formats
CREATE OR REPLACE FUNCTION system_dateformat_get()
RETURNS TABLE (
    dateformat INT,
    dateformatdescription VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT d.date_format_id, d.date_format_description
    FROM date_formats d
    WHERE d.is_active = TRUE
    ORDER BY d.date_format_id;
END;
$$ LANGUAGE plpgsql;

-- Get User by Email
CREATE OR REPLACE FUNCTION system_users_get(
    p_email VARCHAR(255) DEFAULT NULL,
    p_user_id UUID DEFAULT NULL
)
RETURNS TABLE (
    userid UUID,
    accountuserid UUID,
    firstname VARCHAR(255),
    lastname VARCHAR(255),
    username VARCHAR(255),
    emailaddress VARCHAR(255),
    password VARCHAR(500),
    islocked BOOLEAN,
    isadmin BOOLEAN,
    isshowlanguage BOOLEAN,
    accountstatus VARCHAR(50),
    isemailverified BOOLEAN,
    istermcondition BOOLEAN,
    profileimage VARCHAR(255),
    profileimagepath VARCHAR(500),
    denomination INT,
    denominationname VARCHAR(255),
    languageid UUID,
    languagename VARCHAR(100),
    timezone BIGINT,
    timezonename VARCHAR(255),
    dateformat INT,
    dateformatdescription VARCHAR(255),
    isenable BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        u.user_id,
        u.account_user_id,
        u.first_name,
        u.last_name,
        u.user_name,
        u.email_address,
        u.password,
        u.is_locked,
        u.is_admin,
        u.is_show_language,
        u.account_status,
        u.is_email_verified,
        u.is_term_condition,
        u.profile_image,
        u.profile_image_path,
        u.denomination_id,
        d.denomination_name,
        u.language_id,
        l.language_name,
        u.timezone_id,
        t.timezone_name,
        u.date_format_id,
        df.date_format_description,
        u.is_enable
    FROM users u
    LEFT JOIN denominations d ON u.denomination_id = d.denomination_id
    LEFT JOIN languages l ON u.language_id = l.language_id
    LEFT JOIN timezones t ON u.timezone_id = t.timezone_id
    LEFT JOIN date_formats df ON u.date_format_id = df.date_format_id
    WHERE (p_email IS NULL OR u.email_address = p_email)
      AND (p_user_id IS NULL OR u.user_id = p_user_id)
      AND u.is_enable = TRUE;
END;
$$ LANGUAGE plpgsql;

-- Create User
CREATE OR REPLACE FUNCTION system_users_create(
    p_first_name VARCHAR(255),
    p_last_name VARCHAR(255),
    p_email VARCHAR(255),
    p_password VARCHAR(500),
    p_user_name VARCHAR(255) DEFAULT NULL
)
RETURNS TABLE (
    userid UUID,
    status VARCHAR(50),
    message VARCHAR(255)
) AS $$
DECLARE
    v_user_id UUID;
    v_exists INT;
BEGIN
    -- Check if email already exists
    SELECT COUNT(*) INTO v_exists FROM users WHERE email_address = p_email;

    IF v_exists > 0 THEN
        RETURN QUERY SELECT NULL::UUID, 'Error'::VARCHAR(50), 'Email already exists'::VARCHAR(255);
        RETURN;
    END IF;

    -- Generate new UUID
    v_user_id := gen_random_uuid();

    -- Insert new user
    INSERT INTO users (
        user_id, first_name, last_name, email_address, password, user_name,
        is_email_verified, is_enable, created_date
    ) VALUES (
        v_user_id, p_first_name, p_last_name, p_email, p_password,
        COALESCE(p_user_name, p_first_name),
        FALSE, TRUE, CURRENT_TIMESTAMP
    );

    RETURN QUERY SELECT v_user_id, 'Success'::VARCHAR(50), 'User created successfully'::VARCHAR(255);
END;
$$ LANGUAGE plpgsql;

-- Add Login History
CREATE OR REPLACE FUNCTION system_loginhistory_add(
    p_user_id UUID,
    p_ip_address VARCHAR(50),
    p_session_id VARCHAR(255),
    p_user_agent TEXT DEFAULT NULL
)
RETURNS VOID AS $$
BEGIN
    INSERT INTO login_history (user_id, ip_address, session_id, user_agent, login_date)
    VALUES (p_user_id, p_ip_address, p_session_id, p_user_agent, CURRENT_TIMESTAMP);
END;
$$ LANGUAGE plpgsql;

-- Update Login History (logout)
CREATE OR REPLACE FUNCTION system_loginhistory_update(
    p_session_id VARCHAR(255)
)
RETURNS VOID AS $$
BEGIN
    UPDATE login_history
    SET logout_date = CURRENT_TIMESTAMP
    WHERE session_id = p_session_id AND logout_date IS NULL;
END;
$$ LANGUAGE plpgsql;

-- Set Session ID for User
CREATE OR REPLACE FUNCTION system_user_setsessionidbyuserid(
    p_user_id UUID,
    p_session_id VARCHAR(255)
)
RETURNS VOID AS $$
BEGIN
    UPDATE users SET session_id = p_session_id, modified_date = CURRENT_TIMESTAMP
    WHERE user_id = p_user_id;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- WEBSITE CONFIGURATION FUNCTIONS
-- =====================================================

-- Get Website Configuration
CREATE OR REPLACE FUNCTION system_websites_get(p_domain VARCHAR(255) DEFAULT NULL)
RETURNS TABLE (
    system_websiteid UUID,
    system_websitename VARCHAR(255),
    system_domainname VARCHAR(255),
    system_websiteprefix TEXT,
    system_websitemiddle TEXT,
    system_websitesuffix TEXT,
    shortdisclaimer TEXT,
    disclaimer TEXT,
    promptdisclaimer TEXT,
    titletext VARCHAR(500),
    faviconimagepath VARCHAR(500),
    mainimagepath VARCHAR(500),
    brandimagepath VARCHAR(500),
    mainimagealttext VARCHAR(255),
    resreplacedtext TEXT,
    planname VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        w.website_id,
        w.website_name,
        w.domain_name,
        w.website_prefix,
        w.website_middle,
        w.website_suffix,
        w.short_disclaimer,
        w.disclaimer,
        w.prompt_disclaimer,
        w.title_text,
        w.favicon_image_path,
        w.main_image_path,
        w.brand_image_path,
        w.main_image_alt_text,
        w.res_replaced_text,
        w.plan_name
    FROM website_configuration w
    WHERE (p_domain IS NULL OR w.domain_name ILIKE '%' || p_domain || '%')
      AND w.is_active = TRUE
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

-- Add Error Log
CREATE OR REPLACE FUNCTION system_errorlog_add(
    p_error_message TEXT,
    p_stack_trace TEXT DEFAULT NULL,
    p_user_id UUID DEFAULT NULL,
    p_source VARCHAR(255) DEFAULT NULL
)
RETURNS VOID AS $$
BEGIN
    INSERT INTO error_logs (error_message, stack_trace, user_id, source, created_date)
    VALUES (p_error_message, p_stack_trace, p_user_id, p_source, CURRENT_TIMESTAMP);
END;
$$ LANGUAGE plpgsql;

-- Add Website Visitor
CREATE OR REPLACE FUNCTION system_websitevisitors_add(
    p_ip_address VARCHAR(50),
    p_user_agent TEXT,
    p_page_visited VARCHAR(500),
    p_domain_name VARCHAR(255)
)
RETURNS VOID AS $$
BEGIN
    INSERT INTO website_visitors (ip_address, user_agent, page_visited, domain_name, visit_date)
    VALUES (p_ip_address, p_user_agent, p_page_visited, p_domain_name, CURRENT_TIMESTAMP);
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- SUBSCRIPTION FUNCTIONS
-- =====================================================

-- Get Payment Plans
CREATE OR REPLACE FUNCTION system_paymentplans_get(p_plan_type_id UUID DEFAULT NULL)
RETURNS TABLE (
    paymentplanid UUID,
    planname VARCHAR(255),
    description TEXT,
    planprice NUMERIC(18,2),
    featuredplan BOOLEAN,
    plantypeid UUID,
    planstatus VARCHAR(50),
    planindex INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        p.payment_plan_id,
        p.plan_name,
        p.description,
        p.plan_price,
        p.featured_plan,
        p.plan_type_id,
        p.plan_status,
        p.plan_index
    FROM payment_plans p
    WHERE (p_plan_type_id IS NULL OR p.plan_type_id = p_plan_type_id)
      AND p.is_active = TRUE
    ORDER BY p.plan_index;
END;
$$ LANGUAGE plpgsql;

-- Get Payment Plan by ID
CREATE OR REPLACE FUNCTION system_paymentplans_id_get(p_plan_id UUID)
RETURNS TABLE (
    paymentplanid UUID,
    planname VARCHAR(255),
    description TEXT,
    planprice NUMERIC(18,2),
    featuredplan BOOLEAN,
    plantypeid UUID,
    plantype VARCHAR(100),
    planstatus VARCHAR(50),
    planindex INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        p.payment_plan_id,
        p.plan_name,
        p.description,
        p.plan_price,
        p.featured_plan,
        p.plan_type_id,
        pt.plan_type_name,
        p.plan_status,
        p.plan_index
    FROM payment_plans p
    LEFT JOIN payment_plan_types pt ON p.plan_type_id = pt.plan_type_id
    WHERE p.payment_plan_id = p_plan_id;
END;
$$ LANGUAGE plpgsql;

-- Get Features
CREATE OR REPLACE FUNCTION system_features_get()
RETURNS TABLE (
    featureid UUID,
    featurename VARCHAR(255),
    basicfree BOOLEAN,
    standard BOOLEAN,
    deluxe BOOLEAN,
    professional BOOLEAN,
    enterprise BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        f.feature_id,
        f.feature_name,
        f.basic_free,
        f.standard,
        f.deluxe,
        f.professional,
        f.enterprise
    FROM features f
    ORDER BY f.display_order;
END;
$$ LANGUAGE plpgsql;

-- Get Stripe Options
CREATE OR REPLACE FUNCTION system_stripeoptions_get(p_website_id UUID DEFAULT NULL)
RETURNS TABLE (
    publishablekey VARCHAR(500),
    secretkey VARCHAR(500),
    webhooksecret VARCHAR(500),
    domain VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        s.publishable_key,
        s.secret_key,
        s.webhook_secret,
        s.domain
    FROM stripe_options s
    WHERE (p_website_id IS NULL OR s.website_id = p_website_id)
      AND s.is_active = TRUE
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

-- Get Billing Information
CREATE OR REPLACE FUNCTION system_billinginformation_get(p_user_id UUID)
RETURNS TABLE (
    billingdetailid UUID,
    customername VARCHAR(255),
    customerid VARCHAR(255),
    emailaddress VARCHAR(255),
    phonenumber VARCHAR(50),
    city VARCHAR(100),
    state VARCHAR(100),
    country VARCHAR(100),
    postalcode VARCHAR(20)
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        b.billing_id,
        b.customer_name,
        b.customer_id,
        b.email_address,
        b.phone_number,
        b.city,
        b.state,
        b.country,
        b.postal_code
    FROM billing_information b
    WHERE b.user_id = p_user_id
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

-- Get Invoice History
CREATE OR REPLACE FUNCTION system_invoice_gethistory(p_user_id UUID)
RETURNS TABLE (
    invoiceid INT,
    invoicedate VARCHAR(50),
    amount VARCHAR(50),
    status VARCHAR(50),
    planname VARCHAR(255),
    invoicepdflink TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        ROW_NUMBER() OVER (ORDER BY i.invoice_date DESC)::INT,
        TO_CHAR(i.invoice_date, 'YYYY-MM-DD'),
        COALESCE(i.amount_paid::VARCHAR, '0'),
        i.status,
        pp.plan_name,
        i.invoice_pdf_link
    FROM invoice_details i
    LEFT JOIN payment_plans pp ON i.payment_plan_id = pp.payment_plan_id
    WHERE i.user_id = p_user_id
    ORDER BY i.invoice_date DESC;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- AI/PROMPT FUNCTIONS
-- =====================================================

-- Get System Prompts
CREATE OR REPLACE FUNCTION system_prompts_get(p_website_id UUID DEFAULT NULL)
RETURNS TABLE (
    systemprompt TEXT,
    voiceprompt TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT sp.system_prompt, sp.voice_prompt
    FROM system_prompts sp
    WHERE (p_website_id IS NULL OR sp.website_id = p_website_id)
      AND sp.is_active = TRUE
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

-- Insert Prompt Folder
CREATE OR REPLACE FUNCTION user_promptsfolder_insert(
    p_user_id UUID,
    p_website_id UUID,
    p_prompt_text TEXT
)
RETURNS TABLE (
    promptfolderid UUID,
    status VARCHAR(50),
    message VARCHAR(255)
) AS $$
DECLARE
    v_folder_id UUID;
BEGIN
    v_folder_id := gen_random_uuid();

    INSERT INTO user_prompt_folders (
        prompt_folder_id, user_id, website_id, prompt_text, created_date
    ) VALUES (
        v_folder_id, p_user_id, p_website_id, p_prompt_text, CURRENT_TIMESTAMP
    );

    RETURN QUERY SELECT v_folder_id, 'Success'::VARCHAR(50), 'Folder created'::VARCHAR(255);
END;
$$ LANGUAGE plpgsql;

-- Get Today's Prompt Folders
CREATE OR REPLACE FUNCTION user_promptsfolder_gettoday(p_user_id UUID)
RETURNS TABLE (
    promptfolderid UUID,
    prompttext TEXT,
    isfavourite BOOLEAN,
    isarchived BOOLEAN,
    createddate TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        pf.prompt_folder_id,
        pf.prompt_text,
        pf.is_favourite,
        pf.is_archived,
        pf.created_date
    FROM user_prompt_folders pf
    WHERE pf.user_id = p_user_id
      AND DATE(pf.created_date) = CURRENT_DATE
      AND pf.is_archived = FALSE
    ORDER BY pf.created_date DESC;
END;
$$ LANGUAGE plpgsql;

-- Insert User Prompt
CREATE OR REPLACE FUNCTION user_prompts_insert(
    p_folder_id UUID,
    p_user_id UUID,
    p_website_id UUID,
    p_prompt TEXT,
    p_response TEXT DEFAULT NULL
)
RETURNS TABLE (
    promptid UUID,
    status VARCHAR(50)
) AS $$
DECLARE
    v_prompt_id UUID;
BEGIN
    v_prompt_id := gen_random_uuid();

    INSERT INTO user_prompts (
        prompt_id, prompt_folder_id, user_id, website_id, prompt, response, created_date
    ) VALUES (
        v_prompt_id, p_folder_id, p_user_id, p_website_id, p_prompt, p_response, CURRENT_TIMESTAMP
    );

    RETURN QUERY SELECT v_prompt_id, 'Success'::VARCHAR(50);
END;
$$ LANGUAGE plpgsql;

-- Get Parent Prompts (folders with prompts)
CREATE OR REPLACE FUNCTION user_parentprompts_get(p_user_id UUID)
RETURNS TABLE (
    promptfolderid UUID,
    prompttext TEXT,
    isfavourite BOOLEAN,
    isarchived BOOLEAN,
    createddate TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        pf.prompt_folder_id,
        pf.prompt_text,
        pf.is_favourite,
        pf.is_archived,
        pf.created_date
    FROM user_prompt_folders pf
    WHERE pf.user_id = p_user_id
      AND pf.is_archived = FALSE
    ORDER BY pf.created_date DESC;
END;
$$ LANGUAGE plpgsql;

-- Get User Prompts
CREATE OR REPLACE FUNCTION user_prompts_get(p_folder_id UUID)
RETURNS TABLE (
    promptid UUID,
    prompt TEXT,
    response TEXT,
    createddate TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        up.prompt_id,
        up.prompt,
        up.response,
        up.created_date
    FROM user_prompts up
    WHERE up.prompt_folder_id = p_folder_id
    ORDER BY up.created_date ASC;
END;
$$ LANGUAGE plpgsql;

-- Archive Prompt
CREATE OR REPLACE FUNCTION user_prompt_archive(p_folder_id UUID)
RETURNS TABLE (status VARCHAR(50), message VARCHAR(255)) AS $$
BEGIN
    UPDATE user_prompt_folders
    SET is_archived = TRUE, modified_date = CURRENT_TIMESTAMP
    WHERE prompt_folder_id = p_folder_id;

    RETURN QUERY SELECT 'Success'::VARCHAR(50), 'Archived'::VARCHAR(255);
END;
$$ LANGUAGE plpgsql;

-- Favourite Prompt
CREATE OR REPLACE FUNCTION user_prompt_favourite(p_folder_id UUID)
RETURNS TABLE (status VARCHAR(50), message VARCHAR(255)) AS $$
BEGIN
    UPDATE user_prompt_folders
    SET is_favourite = TRUE, modified_date = CURRENT_TIMESTAMP
    WHERE prompt_folder_id = p_folder_id;

    RETURN QUERY SELECT 'Success'::VARCHAR(50), 'Marked as favourite'::VARCHAR(255);
END;
$$ LANGUAGE plpgsql;

-- Unfavourite Prompt
CREATE OR REPLACE FUNCTION user_prompt_unfavourite(p_folder_id UUID)
RETURNS TABLE (status VARCHAR(50), message VARCHAR(255)) AS $$
BEGIN
    UPDATE user_prompt_folders
    SET is_favourite = FALSE, modified_date = CURRENT_TIMESTAMP
    WHERE prompt_folder_id = p_folder_id;

    RETURN QUERY SELECT 'Success'::VARCHAR(50), 'Removed from favourites'::VARCHAR(255);
END;
$$ LANGUAGE plpgsql;

-- Insert/Update Response
CREATE OR REPLACE FUNCTION website_responses_insert(
    p_prompt_id UUID,
    p_response TEXT
)
RETURNS VOID AS $$
BEGIN
    UPDATE user_prompts
    SET response = p_response
    WHERE prompt_id = p_prompt_id;
END;
$$ LANGUAGE plpgsql;

-- Get User Memory
CREATE OR REPLACE FUNCTION user_memory_get(p_user_id UUID, p_domain VARCHAR(255) DEFAULT NULL)
RETURNS TABLE (
    memoryid UUID,
    domain VARCHAR(255),
    memorydata TEXT,
    type VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        um.memory_id,
        um.domain,
        um.memory_data,
        um.type
    FROM user_memory um
    WHERE um.user_id = p_user_id
      AND (p_domain IS NULL OR um.domain = p_domain)
    ORDER BY um.created_date DESC;
END;
$$ LANGUAGE plpgsql;

-- Insert User Memory
CREATE OR REPLACE FUNCTION user_memory_insert(
    p_user_id UUID,
    p_domain VARCHAR(255),
    p_memory_data TEXT,
    p_type VARCHAR(50) DEFAULT 'general'
)
RETURNS TABLE (memoryid UUID, status VARCHAR(50)) AS $$
DECLARE
    v_memory_id UUID;
BEGIN
    v_memory_id := gen_random_uuid();

    INSERT INTO user_memory (memory_id, user_id, domain, memory_data, type, created_date)
    VALUES (v_memory_id, p_user_id, p_domain, p_memory_data, p_type, CURRENT_TIMESTAMP);

    RETURN QUERY SELECT v_memory_id, 'Success'::VARCHAR(50);
END;
$$ LANGUAGE plpgsql;

-- Delete User Memory
CREATE OR REPLACE FUNCTION user_memory_delete(p_memory_id UUID)
RETURNS TABLE (status VARCHAR(50), message VARCHAR(255)) AS $$
BEGIN
    DELETE FROM user_memory WHERE memory_id = p_memory_id;
    RETURN QUERY SELECT 'Success'::VARCHAR(50), 'Memory deleted'::VARCHAR(255);
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- SHARE REDIRECTOR FUNCTIONS
-- =====================================================

-- Get Authors
CREATE OR REPLACE FUNCTION system_getauthors()
RETURNS TABLE (
    authorid INT,
    authorname VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT a.author_id, a.author_name
    FROM authors a
    WHERE a.is_active = TRUE
    ORDER BY a.author_name;
END;
$$ LANGUAGE plpgsql;

-- Get Series
CREATE OR REPLACE FUNCTION system_getseries(p_author_id INT DEFAULT NULL)
RETURNS TABLE (
    seriesid INT,
    seriesname VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT s.series_id, s.series_name
    FROM series s
    WHERE s.is_active = TRUE
      AND (p_author_id IS NULL OR s.author_id = p_author_id)
    ORDER BY s.series_name;
END;
$$ LANGUAGE plpgsql;

-- Get Products
CREATE OR REPLACE FUNCTION system_getproduct(p_series_id INT DEFAULT NULL)
RETURNS TABLE (
    productid INT,
    productname VARCHAR(255),
    productimage VARCHAR(500)
) AS $$
BEGIN
    RETURN QUERY
    SELECT p.product_id, p.product_name, p.product_image
    FROM products p
    WHERE p.is_active = TRUE
      AND (p_series_id IS NULL OR p.series_id = p_series_id)
    ORDER BY p.product_name;
END;
$$ LANGUAGE plpgsql;

-- Get Delivery Methods
CREATE OR REPLACE FUNCTION system_getdelivery()
RETURNS TABLE (
    deliveryid INT,
    deliveryname VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT d.delivery_id, d.delivery_name
    FROM delivery_methods d
    WHERE d.is_active = TRUE
    ORDER BY d.delivery_name;
END;
$$ LANGUAGE plpgsql;

-- Insert Uploaded File
CREATE OR REPLACE FUNCTION system_insertuploadedfile(
    p_user_id UUID,
    p_azure_filename VARCHAR(500),
    p_file_extension VARCHAR(20),
    p_duration VARCHAR(50) DEFAULT NULL,
    p_file_size VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE (
    fileid INT,
    nanoid VARCHAR(50),
    message VARCHAR(255)
) AS $$
DECLARE
    v_file_id INT;
    v_nano_id VARCHAR(50);
BEGIN
    -- Generate nano ID
    v_nano_id := substr(md5(random()::text || clock_timestamp()::text), 1, 12);

    INSERT INTO uploaded_files (
        nano_id, user_id, azure_filename, file_extension, duration, file_size, created_date
    ) VALUES (
        v_nano_id, p_user_id, p_azure_filename, p_file_extension, p_duration, p_file_size, CURRENT_TIMESTAMP
    )
    RETURNING file_id INTO v_file_id;

    RETURN QUERY SELECT v_file_id, v_nano_id, 'Success'::VARCHAR(255);
END;
$$ LANGUAGE plpgsql;

-- Get File Details
CREATE OR REPLACE FUNCTION system_fetch_filedetails(p_user_id UUID DEFAULT NULL)
RETURNS TABLE (
    fileid INT,
    nanoid VARCHAR(50),
    filename VARCHAR(255),
    sharedescription TEXT,
    azurefilename VARCHAR(500),
    fileextension VARCHAR(20),
    redirecturl VARCHAR(1000),
    redirectlink VARCHAR(1000),
    isactive BOOLEAN,
    isspotlight BOOLEAN,
    authorid INT,
    seriesid INT,
    productid INT,
    deliveryid INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        uf.file_id,
        uf.nano_id,
        uf.file_name,
        uf.share_description,
        uf.azure_filename,
        uf.file_extension,
        uf.redirect_url,
        uf.redirect_link,
        uf.is_active,
        uf.is_spotlight,
        uf.author_id,
        uf.series_id,
        uf.product_id,
        uf.delivery_id
    FROM uploaded_files uf
    WHERE (p_user_id IS NULL OR uf.user_id = p_user_id)
      AND uf.is_active = TRUE
    ORDER BY uf.created_date DESC;
END;
$$ LANGUAGE plpgsql;

-- Get File Details by ID
CREATE OR REPLACE FUNCTION system_filedetails_get_byid(p_file_id INT)
RETURNS TABLE (
    fileid INT,
    nanoid VARCHAR(50),
    filename VARCHAR(255),
    sharedescription TEXT,
    azurefilename VARCHAR(500),
    fileextension VARCHAR(20),
    redirecturl VARCHAR(1000),
    redirectlink VARCHAR(1000),
    isactive BOOLEAN,
    isspotlight BOOLEAN,
    authorid INT,
    author VARCHAR(255),
    seriesid INT,
    series VARCHAR(255),
    productid INT,
    product VARCHAR(255),
    productimage VARCHAR(500),
    deliveryid INT,
    delivery VARCHAR(255),
    qrcolor VARCHAR(50),
    qrlogo VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        uf.file_id,
        uf.nano_id,
        uf.file_name,
        uf.share_description,
        uf.azure_filename,
        uf.file_extension,
        uf.redirect_url,
        uf.redirect_link,
        uf.is_active,
        uf.is_spotlight,
        uf.author_id,
        a.author_name,
        uf.series_id,
        s.series_name,
        uf.product_id,
        p.product_name,
        p.product_image,
        uf.delivery_id,
        d.delivery_name,
        uf.qr_color,
        uf.qr_logo
    FROM uploaded_files uf
    LEFT JOIN authors a ON uf.author_id = a.author_id
    LEFT JOIN series s ON uf.series_id = s.series_id
    LEFT JOIN products p ON uf.product_id = p.product_id
    LEFT JOIN delivery_methods d ON uf.delivery_id = d.delivery_id
    WHERE uf.file_id = p_file_id;
END;
$$ LANGUAGE plpgsql;

-- Save File Detail
CREATE OR REPLACE FUNCTION system_savefiledetail(
    p_file_id INT,
    p_file_name VARCHAR(255),
    p_share_description TEXT,
    p_author_id INT,
    p_series_id INT,
    p_product_id INT,
    p_delivery_id INT,
    p_is_public BOOLEAN DEFAULT FALSE,
    p_is_spotlight BOOLEAN DEFAULT FALSE
)
RETURNS TABLE (status VARCHAR(50), message VARCHAR(255)) AS $$
BEGIN
    UPDATE uploaded_files
    SET
        file_name = p_file_name,
        share_description = p_share_description,
        author_id = p_author_id,
        series_id = p_series_id,
        product_id = p_product_id,
        delivery_id = p_delivery_id,
        is_public = p_is_public,
        is_spotlight = p_is_spotlight,
        modified_date = CURRENT_TIMESTAMP
    WHERE file_id = p_file_id;

    RETURN QUERY SELECT 'Success'::VARCHAR(50), 'File details saved'::VARCHAR(255);
END;
$$ LANGUAGE plpgsql;

-- Delete Record
CREATE OR REPLACE FUNCTION system_deleterecord(p_file_id INT)
RETURNS TABLE (status VARCHAR(50), message VARCHAR(255)) AS $$
BEGIN
    UPDATE uploaded_files
    SET is_active = FALSE, modified_date = CURRENT_TIMESTAMP
    WHERE file_id = p_file_id;

    RETURN QUERY SELECT 'Success'::VARCHAR(50), 'Record deleted'::VARCHAR(255);
END;
$$ LANGUAGE plpgsql;

-- Insert Redirect Hit
CREATE OR REPLACE FUNCTION system_redirecthits_insert(
    p_nano_id VARCHAR(50),
    p_ip_address VARCHAR(50) DEFAULT NULL,
    p_user_agent TEXT DEFAULT NULL
)
RETURNS VOID AS $$
DECLARE
    v_file_id INT;
BEGIN
    SELECT file_id INTO v_file_id FROM uploaded_files WHERE nano_id = p_nano_id;

    INSERT INTO redirect_hits (nano_id, file_id, ip_address, user_agent, hit_date)
    VALUES (p_nano_id, v_file_id, p_ip_address, p_user_agent, CURRENT_TIMESTAMP);
END;
$$ LANGUAGE plpgsql;

-- Fetch Redirect URL
CREATE OR REPLACE FUNCTION system_fetchredirecturl(p_nano_id VARCHAR(50))
RETURNS TABLE (
    redirecturl VARCHAR(1000),
    azurefilename VARCHAR(500),
    fileextension VARCHAR(20)
) AS $$
BEGIN
    RETURN QUERY
    SELECT uf.redirect_url, uf.azure_filename, uf.file_extension
    FROM uploaded_files uf
    WHERE uf.nano_id = p_nano_id AND uf.is_active = TRUE;
END;
$$ LANGUAGE plpgsql;

-- Get Custom QR
CREATE OR REPLACE FUNCTION system_fetch_customqr(p_file_id INT)
RETURNS TABLE (
    color VARCHAR(50),
    logo VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT uf.qr_color, uf.qr_logo
    FROM uploaded_files uf
    WHERE uf.file_id = p_file_id;
END;
$$ LANGUAGE plpgsql;

-- Insert QR Logo
CREATE OR REPLACE FUNCTION system_qrlogo_insert(
    p_file_id INT,
    p_qr_color VARCHAR(50),
    p_qr_logo VARCHAR(255)
)
RETURNS TABLE (status VARCHAR(50), message VARCHAR(255)) AS $$
BEGIN
    UPDATE uploaded_files
    SET qr_color = p_qr_color, qr_logo = p_qr_logo, modified_date = CURRENT_TIMESTAMP
    WHERE file_id = p_file_id;

    RETURN QUERY SELECT 'Success'::VARCHAR(50), 'QR settings saved'::VARCHAR(255);
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- TEXT TO SPEECH FUNCTIONS
-- =====================================================

-- Get Voices
CREATE OR REPLACE FUNCTION system_voices_get()
RETURNS TABLE (
    voiceid UUID,
    voicename VARCHAR(100),
    voicealtname VARCHAR(100)
) AS $$
BEGIN
    RETURN QUERY
    SELECT v.voice_id, v.voice_name, v.voice_alt_name
    FROM tts_voices v
    WHERE v.is_active = TRUE
    ORDER BY v.voice_name;
END;
$$ LANGUAGE plpgsql;

-- Get User Voice
CREATE OR REPLACE FUNCTION system_users_voice_get(p_user_id UUID)
RETURNS TABLE (
    voiceid UUID,
    voicename VARCHAR(100),
    voicealtname VARCHAR(100)
) AS $$
BEGIN
    RETURN QUERY
    SELECT v.voice_id, v.voice_name, v.voice_alt_name
    FROM user_voices uv
    JOIN tts_voices v ON uv.voice_id = v.voice_id
    WHERE uv.user_id = p_user_id AND uv.is_default = TRUE
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

-- Grant execute permissions to jixqr_admin
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO jixqr_admin;

COMMIT;
