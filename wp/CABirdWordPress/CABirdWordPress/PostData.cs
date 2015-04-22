using net.sf.jabref;
using net.sf.jabref.export;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABirdWordPress
{
    public class PostData
    {
        public BibtexEntry Entry;

        public PostData(BibtexEntry entry)
        {
            Entry = entry;
        }

        private bool ContainsField(string field)
        {
            return Entry.getAllFields().ContainsKey(field);
        }

        public DateTime DateTime
        {
            get
            {
                if (ContainsField("postdate"))
                {
                    return DateTime.Parse(Entry.getField("postdate"));
                }
                if (ContainsField("year"))
                {
                    return new DateTime(Int32.Parse(Entry.getField("year")), 1, 1);
                }
                return DateTime.Now;
            }
        }

        public string Title
        {
            get
            {
                string title = Entry.getField("title").Replace("{", "").Replace("}", "");
                if (HasShortVenue())
                {
                    title += " (" + ShortVenue + ")";
                }
                return title;
            }
        }

        public string PostText
        {
            get
            {
                PostTemplate template = new PostTemplate(this);
                string text = template.TransformText();
                return text;
            }
        }

        public string Key { get { return Entry.getCiteKey(); } }

        public bool HasAbstract()
        {
            return ContainsField("abstract");
        }
        public string Abstract { get { return Entry.getField("abstract").Replace("\\%", "%"); } }

        public bool HasShortVenue()
        {
            return ContainsField("short_venue");
        }
        public string ShortVenue { get { return Entry.getField("short_venue"); } }

        public bool HasPublishedIn()
        {
            return ContainsField("booktitle");
        }

        public string PublishedIn { get { return Entry.getField("booktitle"); } }

        public string Authors
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                AuthorList authorList = AuthorList.getAuthorList(Entry.getField("author"));

                bool useCommas = authorList.size() > 2;
                bool needAnd = authorList.size() > 1;

                for (int i = 0; i < authorList.size(); i++)
                {
                    sb.Append(authorList.getAuthor(i).getFirstLast(false));

                    if (i < authorList.size() - 1)
                    {
                        if (useCommas)
                        {
                            sb.Append(",");
                        }
                        if (i == authorList.size() - 2 && needAnd)
                        {
                            sb.Append(" and ");
                        }
                        else
                        {
                            sb.Append(" ");
                        }
                    }
                }

                return sb.ToString();
            }
        }
        public string BibTex { get {
            BibtexEntry filteredEntry = (BibtexEntry) Entry.clone();

            //remove all fields except for those listed
            var desiredFields = new HashSet<string>("author,title,booktitle,year,publisher,journal,location,bibtexkey".Split(','));
            foreach (var field in filteredEntry.getAllFields().Keys)
            {
                if (!desiredFields.Contains(field))
                {
                    filteredEntry.clearField(field);
                }
            }
            var sw = new StringWriter();
            var ff = new LatexFieldFormatter();
            filteredEntry.write(sw, ff, true);
            return sw.ToString();
        } }
    }

}
