using Azure.Storage.Blobs;
using Dapper;
using Npgsql;

public class DownloadFiles
{
    static string azureConnStr = "DefaultEndpointsProtocol=https;AccountName=stgabeungure563580095495;AccountKey=6VkUpYpQ/Paer5TRvI8bWj+UrmGSlVdzXCIGiKzEd6DXjapXjQSenOZVgeRivQL9Vr/gwxdRBC9t+AStRYiAFg==;EndpointSuffix=core.windows.net";
    static string pgConn = "Host=172.21.70.165;Port=5432;Database=jixqr;Username=jixqr_admin;Password=JixQR2026Secure;";
    static string cdnFolder = @"C:\Websites\jixqr.com\cdn";

    public static async Task DownloadFromAzure()
    {
        Console.WriteLine("=== Downloading files from Azure Blob Storage ===\n");

        var blobServiceClient = new BlobServiceClient(azureConnStr);

        // Get file list from database
        using var pgConn_db = new NpgsqlConnection(pgConn);
        await pgConn_db.OpenAsync();

        var files = await pgConn_db.QueryAsync<(int file_id, string file_name, string azure_filename, string file_extension)>(
            "SELECT file_id, file_name, azure_filename, file_extension FROM uploaded_files WHERE azure_filename IS NOT NULL AND file_extension IN ('.mp3', '.pdf', '.mp4')");

        var fileList = files.ToList();
        Console.WriteLine($"Found {fileList.Count} media files to download\n");

        // Build a lookup of all blobs across all containers
        var blobLookup = new Dictionary<string, (string container, string blobName)>(StringComparer.OrdinalIgnoreCase);

        Console.WriteLine("Scanning Azure containers...");
        await foreach (var container in blobServiceClient.GetBlobContainersAsync())
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(container.Name);
            await foreach (var blob in containerClient.GetBlobsAsync())
            {
                // Extract GUID from blob name (without extension)
                var blobGuid = Path.GetFileNameWithoutExtension(blob.Name);
                if (!blobLookup.ContainsKey(blobGuid))
                {
                    blobLookup[blobGuid] = (container.Name, blob.Name);
                }
            }
        }
        Console.WriteLine($"Found {blobLookup.Count} blobs across all containers\n");

        // Download files
        Directory.CreateDirectory(cdnFolder);
        int downloaded = 0;
        int notFound = 0;

        foreach (var file in fileList)
        {
            try
            {
                if (blobLookup.TryGetValue(file.azure_filename, out var blobInfo))
                {
                    var containerClient = blobServiceClient.GetBlobContainerClient(blobInfo.container);
                    var blobClient = containerClient.GetBlobClient(blobInfo.blobName);

                    // Create safe filename
                    var safeName = string.Join("_", file.file_name.Split(Path.GetInvalidFileNameChars()));
                    var localPath = Path.Combine(cdnFolder, safeName + file.file_extension);

                    // Skip if already exists
                    if (File.Exists(localPath))
                    {
                        downloaded++;
                        continue;
                    }

                    await blobClient.DownloadToAsync(localPath);
                    downloaded++;

                    if (downloaded % 50 == 0 || downloaded <= 10)
                        Console.WriteLine($"✓ [{downloaded}/{fileList.Count}] {file.file_name}{file.file_extension}");
                }
                else
                {
                    notFound++;
                    if (notFound <= 5)
                        Console.WriteLine($"✗ Not found: {file.azure_filename}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error downloading {file.file_name}: {ex.Message.Substring(0, Math.Min(50, ex.Message.Length))}");
            }
        }

        Console.WriteLine($"\n=== Download Complete ===");
        Console.WriteLine($"Downloaded: {downloaded}");
        Console.WriteLine($"Not found: {notFound}");
        Console.WriteLine($"Location: {cdnFolder}");
    }
}
