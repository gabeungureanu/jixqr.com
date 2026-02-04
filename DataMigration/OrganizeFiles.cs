using Dapper;
using Microsoft.Data.SqlClient;

public class OrganizeFiles
{
    static string sqlServerConn = "Data Source=localhost;Initial Catalog=JubileeGPT;Integrated Security=True;TrustServerCertificate=True;";
    static string cdnFolder = @"C:\Websites\jixqr.com\cdn";

    public static async Task OrganizeIntoFolders()
    {
        Console.WriteLine("=== Organizing files into folder structure ===\n");

        using var sqlConn = new SqlConnection(sqlServerConn);
        await sqlConn.OpenAsync();

        // Get all folders (both root and nested)
        var allFolders = (await sqlConn.QueryAsync<FolderInfo>(@"
            SELECT Id, FolderName, RootId, Level
            FROM system_FileStructure
            WHERE IsFolder = 1
            ORDER BY Level")).ToList();

        // Build folder lookup
        var folderById = allFolders.ToDictionary(f => f.Id, f => f);

        // Build full paths for each folder
        foreach (var folder in allFolders)
        {
            folder.FullPath = BuildFolderPath(folder, folderById);
        }

        Console.WriteLine($"Found {allFolders.Count} folders\n");

        // Get all files with their root folder
        var files = await sqlConn.QueryAsync<FileInfo>(@"
            SELECT sr.FileID, sr.FileName, sr.FileExtension, sr.Azurefilename,
                   fs.RootId
            FROM system_ShareRedirector sr
            INNER JOIN system_FileStructure fs ON sr.FileID = fs.FileID
            WHERE sr.FileExtension IN ('.mp3', '.pdf', '.mp4')
            ORDER BY fs.RootId, sr.FileName");

        var fileList = files.ToList();
        Console.WriteLine($"Found {fileList.Count} files to organize\n");

        int moved = 0;
        int notFound = 0;
        var createdFolders = new HashSet<string>();

        foreach (var file in fileList)
        {
            // Get folder path from root
            string folderPath = "Uncategorized";
            if (folderById.TryGetValue(file.RootId, out var folder))
            {
                folderPath = folder.FullPath ?? folder.FolderName ?? "Uncategorized";
            }

            // Clean folder path
            folderPath = string.Join("/", folderPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => string.Join("_", p.Split(Path.GetInvalidFileNameChars()))));

            var targetFolder = Path.Combine(cdnFolder, folderPath);

            // Create folder if needed
            if (!createdFolders.Contains(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
                createdFolders.Add(targetFolder);
            }

            // Find and move the file
            var safeName = string.Join("_", file.FileName.Split(Path.GetInvalidFileNameChars()));
            var sourceFile = Path.Combine(cdnFolder, safeName + file.FileExtension);
            var targetFile = Path.Combine(targetFolder, safeName + file.FileExtension);

            if (File.Exists(sourceFile))
            {
                try
                {
                    if (sourceFile != targetFile)
                    {
                        if (File.Exists(targetFile))
                            File.Delete(targetFile);
                        File.Move(sourceFile, targetFile);
                    }
                    moved++;

                    if (moved % 50 == 0 || moved <= 10)
                        Console.WriteLine($"✓ [{moved}] {file.FileName} -> {folderPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error moving {file.FileName}: {ex.Message.Substring(0, Math.Min(40, ex.Message.Length))}");
                }
            }
            else
            {
                notFound++;
                if (notFound <= 5)
                    Console.WriteLine($"✗ Not found: {safeName}{file.FileExtension}");
            }
        }

        Console.WriteLine($"\n=== Organization Complete ===");
        Console.WriteLine($"Files organized: {moved}");
        Console.WriteLine($"Not found: {notFound}");
        Console.WriteLine($"Folders created: {createdFolders.Count}");

        // List top-level folders
        Console.WriteLine($"\nFolder structure created:");
        foreach (var dir in Directory.GetDirectories(cdnFolder).OrderBy(d => d))
        {
            var dirInfo = new DirectoryInfo(dir);
            var fileCount = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).Length;
            Console.WriteLine($"  📁 {dirInfo.Name}/ ({fileCount} files)");
        }
    }

    static string BuildFolderPath(FolderInfo folder, Dictionary<int, FolderInfo> allFolders)
    {
        var pathParts = new List<string>();
        var current = folder;
        var visited = new HashSet<int>();

        while (current != null && !visited.Contains(current.Id))
        {
            visited.Add(current.Id);
            if (!string.IsNullOrEmpty(current.FolderName))
                pathParts.Insert(0, current.FolderName);

            // Move to parent (RootId points to parent folder)
            if (current.Level != "Level_0" && allFolders.TryGetValue(current.RootId, out var parent))
                current = parent;
            else
                break;
        }

        return string.Join("/", pathParts);
    }

    class FolderInfo
    {
        public int Id { get; set; }
        public string? FolderName { get; set; }
        public int RootId { get; set; }
        public string? Level { get; set; }
        public string? FullPath { get; set; }
    }

    class FileInfo
    {
        public int FileID { get; set; }
        public string FileName { get; set; } = "";
        public string FileExtension { get; set; } = "";
        public string? Azurefilename { get; set; }
        public int RootId { get; set; }
    }
}
