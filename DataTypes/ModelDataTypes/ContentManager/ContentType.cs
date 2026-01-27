using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ContentManager
{
    public class ContentType
    {
        public Guid ID { get; set; }
        public string Description { get; set; } = null!;
        public List<BookSeries> bookSeries { get; set; }
    }

    public class BookSeries
    {
        public Guid ID { get; set; }
        public string Description { get; set; } = null!;
        public List<Book> books { get; set; }
    }

    public class Book
    {
        public Guid ID { get; set; }
        public string Description { get; set; } = null!;
        public List<BookLevel1> booksLevel1 { get; set; }
    }
    public class BookLevel1
    {
        public Guid ID { get; set; }
        public string Description { get; set; } = null!;
    }
}
