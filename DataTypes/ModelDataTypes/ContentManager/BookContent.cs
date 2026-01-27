using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ContentManager
{
    public class BookContent
    {
        public string? BookCode { get; set; }
        public string? BookTitle { get; set; }
        public string? Publisher { get; set; }
        public int NumberOfChapters { get; set; }
        public string? BookTheme { get; set; }
    }

    public class BookContentCode
    {
        public string? ContentCode { get; set; }
        public string? BookIdea { get; set; }
        public string? BookTitle { get; set; }
        public string? BookGoals { get; set; }
        public string? BookTheme { get; set; }
        public string? BookPersona { get; set; }
    }
}
