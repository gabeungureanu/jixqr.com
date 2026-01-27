using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ContentManager
{
    public class BookPart
    {
        public Guid BookContentID { get; set; }
        public Guid ContentTypeID { get; set; }
        public Guid? ParentID { get; set; }
        public string Description { get; set; } = null!;
        public string ParentName { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public int Indent { get; set; }
        public int ChildCount { get; set; }
        public List<Guid> bookIDList { get; set; }
        public List<BookPart> bookParts { get; set; }
    }














    public class BookPartChapter
    {
        public Guid BookContentID { get; set; }
        public Guid ContentTypeID { get; set; }
        public Guid ParentID { get; set; }
        public string Description { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }

    public class BookPartChapterVerse
    {
        public Guid BookContentID { get; set; }
        public Guid ContentTypeID { get; set; }
        public Guid ParentID { get; set; }
        public string Description { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
