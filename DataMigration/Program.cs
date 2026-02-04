using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;

class Program
{
    static string sqlServerConn = "Data Source=localhost;Initial Catalog=JubileeGPT;Integrated Security=True;TrustServerCertificate=True;";
    static string pgConn = "Host=172.21.70.165;Port=5432;Database=jixqr;Username=jixqr_admin;Password=JixQR2026Secure;";

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Data Migration: SQL Server -> PostgreSQL ===\n");

        // Simple table-to-table migrations with SELECT *
        var simpleMigrations = new Dictionary<string, string>
        {
            {"website_Language", "languages"},
            {"system_Denomination", "denominations"},
            {"system_Timezones", "timezones"},
            {"system_DateFormat", "date_formats"},
            {"system_Users", "users"},
            {"system_ShareRedirector", "uploaded_files"},
            {"system_FileStructure", "file_structure"},
            {"System_Viral_MusicPlayer", "albums"},
            {"system_PaymentPlans", "payment_plans"},
            {"system_PaymentPlanItems", "payment_plan_items"},
            {"system_BillingInformation", "billing_information"},
            {"subscription", "user_subscriptions"},
            {"system_Invoice", "invoice_details"},
            {"system_StripeOptions", "stripe_options"},
            {"subscription_Share_StripePayments", "stripe_payment_details"},
            {"system_Prompts", "system_prompts"},
            {"system_SubPrompt", "sub_prompts"},
            {"user_PromptsFolder", "user_prompt_folders"},
            {"user_Prompts", "user_prompts"},
            {"system_InitialPrompt", "initial_prompt_responses"},
            {"system_response_feedback", "response_feedback"},
            {"users_Memory", "user_memory"},
            {"system_Users_Voice", "user_voices"},
            {"module_BookContent", "book_content"},
            {"module_BookIntroduction", "book_introduction"},
            {"BlueprintContent", "blueprint_content"},
            {"website_Testimonies", "testimonies"},
            {"system_Feature", "features"},
            {"system_Websites", "website_configuration"},
            {"email_Server_Settings", "smtp_settings"},
            {"forgot_Password", "account_tokens"},
            {"system_ErrorsLog", "error_logs"},
            {"system_LoginHistory", "login_history"},
            {"system_redirect_hits", "redirect_hits"},
            {"system_WebsiteVisitors", "website_visitors"},
            {"website_LanguageVisitor", "language_visitors"},
            {"webhookLogs", "webhook_logs"},
        };

        int totalMigrated = 0;
        int successTables = 0;

        foreach (var (sqlTable, pgTable) in simpleMigrations)
        {
            try
            {
                int count = await MigrateTableDynamic(sqlTable, pgTable);
                totalMigrated += count;
                successTables++;
                Console.WriteLine($"✓ {sqlTable} -> {pgTable}: {count} rows");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ {sqlTable}: {ex.Message.Substring(0, Math.Min(60, ex.Message.Length))}...");
            }
        }

        Console.WriteLine($"\n=== Migration Complete ===");
        Console.WriteLine($"Tables: {successTables}/{simpleMigrations.Count}");
        Console.WriteLine($"Total rows: {totalMigrated}");

        // Run explicit file migration
        Console.WriteLine("\n--- Running explicit uploaded_files migration ---");
        await FileMigration.MigrateUploadedFiles();

        // Download files from Azure to cdn folder
        Console.WriteLine("\n--- Downloading media files from Azure ---");
        await DownloadFiles.DownloadFromAzure();

        // Organize files into folder structure
        Console.WriteLine("\n--- Organizing files into folder structure ---");
        await OrganizeFiles.OrganizeIntoFolders();
    }

    static async Task<int> MigrateTableDynamic(string sqlTable, string pgTable)
    {
        using var sqlConn = new SqlConnection(sqlServerConn);
        using var pgConn_db = new NpgsqlConnection(pgConn);

        await sqlConn.OpenAsync();
        await pgConn_db.OpenAsync();

        // Get PostgreSQL table columns
        var pgColumnsResult = await pgConn_db.QueryAsync<string>(
            $"SELECT column_name FROM information_schema.columns WHERE table_name = @table ORDER BY ordinal_position",
            new { table = pgTable });
        var pgColumnSet = new HashSet<string>(pgColumnsResult, StringComparer.OrdinalIgnoreCase);

        // Get SQL Server table columns
        var sqlColumnsResult = await sqlConn.QueryAsync<string>(
            $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table ORDER BY ORDINAL_POSITION",
            new { table = sqlTable });
        var sqlColumns = sqlColumnsResult.ToList();

        // Get all data from SQL Server
        var data = (await sqlConn.QueryAsync($"SELECT * FROM [{sqlTable}]")).ToList();
        if (data.Count == 0) return 0;

        // Clear target table
        try { await pgConn_db.ExecuteAsync($"TRUNCATE TABLE {pgTable} CASCADE"); } catch { }

        int inserted = 0;
        foreach (var row in data)
        {
            var rowDict = (IDictionary<string, object>)row;
            var insertCols = new List<string>();
            var insertParams = new List<string>();
            var parameters = new DynamicParameters();

            int i = 0;
            foreach (var sqlCol in sqlColumns)
            {
                // Convert SQL Server column name to PostgreSQL snake_case
                var pgCol = ToSnakeCase(sqlCol);

                // Check if column exists in PostgreSQL table
                if (pgColumnSet.Contains(pgCol) && rowDict.ContainsKey(sqlCol))
                {
                    insertCols.Add(pgCol);
                    insertParams.Add($"@p{i}");

                    var value = rowDict[sqlCol];
                    parameters.Add($"p{i}", value == DBNull.Value ? null : value);
                    i++;
                }
            }

            if (insertCols.Count == 0) continue;

            var sql = $"INSERT INTO {pgTable} ({string.Join(", ", insertCols)}) VALUES ({string.Join(", ", insertParams)}) ON CONFLICT DO NOTHING";

            try
            {
                await pgConn_db.ExecuteAsync(sql, parameters);
                inserted++;
            }
            catch { }
        }

        // Reset sequence
        try
        {
            var firstCol = pgColumnSet.First();
            await pgConn_db.ExecuteAsync($@"
                SELECT setval(pg_get_serial_sequence('{pgTable}', '{firstCol}'),
                    COALESCE((SELECT MAX({firstCol}) FROM {pgTable}), 1), true)");
        }
        catch { }

        return inserted;
    }

    static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && !char.IsUpper(input[i - 1]))
                    result.Append('_');
                result.Append(char.ToLower(c));
            }
            else
            {
                result.Append(c);
            }
        }
        return result.ToString().Replace("__", "_").TrimStart('_');
    }
}
