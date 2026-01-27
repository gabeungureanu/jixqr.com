using System.IO;
using Microsoft.Extensions.Configuration;

namespace DataAccess
{
    public class DBConnection
    {
        public static string conString { get; set; }
        static public IConfigurationRoot Configuration { get; set; }
        public static string Main(string[] args = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            conString = Configuration.GetSection("ConnectionStrings")["DefaultConnection"];
            return conString;
        }
    }
}
