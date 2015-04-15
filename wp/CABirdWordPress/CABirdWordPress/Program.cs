using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using net.sf.jabref;
using net.sf.jabref.export;
using net.sf.jabref.imports;
using System.IO;
using System.Diagnostics;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace CABirdWordPress
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();

            program.ConnectToDatabase();
           
        }

        void ConnectToDatabase()
        {
            string connectionString = "Server=mysql.cabird.com;Database=cabird_com_4;Uid=cabird;Pwd=***REMOVED***";

            MySqlConnection conn;

            string tablePrefix = "wp_2a8dr8";

            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
                conn.Open();

                string sql = string.Format("SELECT ID from {0}_users where user_login='cabird'", tablePrefix);
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                int userID = 1;
                while (rdr.Read())
                {
                    userID = rdr.GetInt32(0);
                    Debug.WriteLine("User id for cabird is {0}", userID);
                }
                rdr.Close();

                string postSql = string.Format(
                    @"insert into {0}_posts (post_author, post_date, post_date_gmt, post_content, post_title, post_excerpt, post_status, comment_status, ping_status,
                        post_password, post_name, to_ping, pinged, post_modified, post_modified_gmt, post_content_filtered, post_parent, guid, menu_order, post_type, 
                        post_mime_type, comment_count) 
                        values (@post_author, @post_date, @post_date_gmt, @post_content, @post_title, @post_excerpt, @post_status, @comment_status, @ping_status,
                        @post_password, @post_name, @to_ping, @pinged, @post_modified, @post_modified_gmt, @post_content_filtered, @post_parent, @guid, @menu_order, @post_type, 
                        @post_mime_type, @comment_count)", tablePrefix);

                MySqlCommand postCmd = new MySqlCommand(postSql, conn);
                postCmd.Prepare();


                Dictionary<string, object> paramValues = new Dictionary<string, object>()
                {
                    {"@post_author", userID},
                    {"@post_date", DateTime.Now},
                    {"@post_date_gmt", DateTime.Now},
                    {"@post_content", "Test Post"},
                    {"@post_title", "Test Post Title"},
                    {"@post_excerpt", ""},
                    {"@post_status", "publish"},
                    {"@comment_status", "closed"},
                    {"@ping_status", "open"},
                    {"@post_password", ""},
                    {"@post_name", "test-post"},
                    {"@to_ping", ""},
                    {"@pinged", ""},
                    {"@post_modified", DateTime.Now},
                    {"@post_modified_gmt", DateTime.Now},
                    {"@post_content_filtered", ""},
                    {"@post_parent", 0},
                    {"@guid", Guid.NewGuid().ToString()},
                    {"@menu_order", 0},
                    {"@post_type", "post"},
                    {"@post_mime_type", ""},
                    {"@comment_count", 0}
                };

                foreach (var kvp in paramValues)
                {
                    postCmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                postCmd.ExecuteNonQuery();
                
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        void Foo()
        {
            var parser = new BibtexParser(new StreamReader(@"C:\Users\cbird\Documents\GitHub\cv\bird.bib"));
            var result = parser.Parse();
            var db = result.Database;

            var ff = new LatexFieldFormatter();

            foreach (var entry in db.getEntries())
            {
                entry.clearField("title");

                Debug.WriteLine(entry.ToString());
                var sw = new StringWriter();
                entry.write(sw, ff, true);
                AuthorList authorList = AuthorList.getAuthorList(entry.getField("author"));
                for (int i = 0; i > authorList.size(); i++)
                {
                    sw.WriteLine("  " + authorList.getAuthor(i).getFirstLast(false));
                }

                Debug.WriteLine(sw.ToString());
            }
        }
    }
}
