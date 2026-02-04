using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;

public class FileMigration
{
    static string sqlServerConn = "Data Source=localhost;Initial Catalog=JubileeGPT;Integrated Security=True;TrustServerCertificate=True;";
    static string pgConn = "Host=172.21.70.165;Port=5432;Database=jixqr;Username=jixqr_admin;Password=JixQR2026Secure;";

    public static async Task MigrateUploadedFiles()
    {
        Console.WriteLine("Migrating uploaded_files with explicit mapping...");

        using var sqlConn = new SqlConnection(sqlServerConn);
        using var pgConn_db = new NpgsqlConnection(pgConn);

        await sqlConn.OpenAsync();
        await pgConn_db.OpenAsync();

        // Truncate target
        await pgConn_db.ExecuteAsync("TRUNCATE TABLE uploaded_files CASCADE");

        // Get data with explicit column selection
        var data = await sqlConn.QueryAsync(@"
            SELECT FileID, NanoId, FileName, ShareDescription, Azurefilename, FileExtension,
                   Duration, FileSize, RedirectURL, IsPublic, IsActive, IsSpotlight,
                   IsMusicAlbum, IsAudioBook, IsVideoFile, IsPDFFiles, CreatedDate
            FROM system_ShareRedirector");

        int count = 0;
        foreach (var row in data)
        {
            var d = (IDictionary<string, object>)row;

            try
            {
                await pgConn_db.ExecuteAsync(@"
                    INSERT INTO uploaded_files (file_id, nano_id, file_name, share_description, azure_filename,
                        file_extension, duration, file_size, redirect_url, is_public, is_active, is_spotlight,
                        is_music_album, is_audio_book, is_video_file, is_pdf_files, created_date)
                    VALUES (@file_id, @nano_id, @file_name, @share_description, @azure_filename,
                        @file_extension, @duration, @file_size, @redirect_url, @is_public, @is_active, @is_spotlight,
                        @is_music_album, @is_audio_book, @is_video_file, @is_pdf_files, @created_date)
                    ON CONFLICT DO NOTHING",
                    new
                    {
                        file_id = GetVal<int>(d, "FileID"),
                        nano_id = GetVal<string>(d, "NanoId"),
                        file_name = GetVal<string>(d, "FileName"),
                        share_description = GetVal<string>(d, "ShareDescription"),
                        azure_filename = GetVal<string>(d, "Azurefilename"),
                        file_extension = GetVal<string>(d, "FileExtension"),
                        duration = GetVal<string>(d, "Duration"),
                        file_size = GetVal<long?>(d, "FileSize"),
                        redirect_url = GetVal<string>(d, "RedirectURL"),
                        is_public = GetVal<bool>(d, "IsPublic"),
                        is_active = GetVal<bool>(d, "IsActive"),
                        is_spotlight = GetVal<bool>(d, "IsSpotlight"),
                        is_music_album = GetVal<bool>(d, "IsMusicAlbum"),
                        is_audio_book = GetVal<bool>(d, "IsAudioBook"),
                        is_video_file = GetVal<bool>(d, "IsVideoFile"),
                        is_pdf_files = GetVal<bool>(d, "IsPDFFiles"),
                        created_date = GetVal<DateTime?>(d, "CreatedDate")
                    });
                count++;
            }
            catch (Exception ex)
            {
                if (count < 3) Console.WriteLine($"  Error: {ex.Message.Substring(0, Math.Min(80, ex.Message.Length))}");
            }
        }

        // Reset sequence
        await pgConn_db.ExecuteAsync(@"SELECT setval(pg_get_serial_sequence('uploaded_files', 'file_id'),
            COALESCE((SELECT MAX(file_id) FROM uploaded_files), 1), true)");

        Console.WriteLine($"✓ Migrated {count} uploaded_files records");
    }

    static T? GetVal<T>(IDictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var val) && val != DBNull.Value && val != null)
            return (T)Convert.ChangeType(val, typeof(T).IsGenericType ? Nullable.GetUnderlyingType(typeof(T))! : typeof(T));
        return default;
    }
}
