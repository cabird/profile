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

            string bibtexPath = @"C:\Users\cbird\Documents\GitHub\cv\bird.bib";

            var entries = program.GetBibtexEntries(bibtexPath);

            program.PostToDatabase(entries["zanjani2015developer"]);
           
        }

        void PostToDatabase(BibtexEntry entry)
        {
            PostData post = new PostData(entry);
            string connectionString = "Server=mysql.cabird.com;Database=cabird_com_4;Uid=cabird;Pwd=***REMOVED***";

            MySqlConnection conn;

            string tablePrefix = "wp_2a8dr8";

            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
                conn.Open();

                string sql = string.Format("SELECT ID from {0}_users where user_login='cabird'", tablePrefix);
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                int userId = Convert.ToInt32(cmd.ExecuteScalar());

                /* check for a post with this citekey */
                cmd.CommandText = string.Format("select count(*) from {0}_posts where post_name='{1}'", tablePrefix, post.Key);
                int number = Convert.ToInt32(cmd.ExecuteScalar());

                MySqlCommand postCmd = new MySqlCommand();
                postCmd.Connection = conn;
                if (number == 0)
                {
                    postCmd.CommandText = string.Format(
                    @"insert into {0}_posts (post_author, post_date, post_date_gmt, post_content, post_title, post_excerpt, post_status, comment_status, ping_status,
                        post_password, post_name, to_ping, pinged, post_modified, post_modified_gmt, post_content_filtered, post_parent, guid, menu_order, post_type, 
                        post_mime_type, comment_count) 
                        values (@post_author, @post_date, @post_date_gmt, @post_content, @post_title, @post_excerpt, @post_status, @comment_status, @ping_status,
                        @post_password, @post_name, @to_ping, @pinged, @post_modified, @post_modified_gmt, @post_content_filtered, @post_parent, @guid, @menu_order, @post_type, 
                        @post_mime_type, @comment_count)", tablePrefix);
                    postCmd.Prepare();

                    Dictionary<string, object> paramValues = new Dictionary<string, object>()
                    {
                        {"@post_author", userId},
                        {"@post_date", post.DateTime},
                        {"@post_date_gmt", post.DateTime},
                        {"@post_content", post.PostText},
                        {"@post_title", post.Title},
                        {"@post_excerpt", ""},
                        {"@post_status", "publish"},
                        {"@comment_status", "closed"},
                        {"@ping_status", "open"},
                        {"@post_password", ""},
                        {"@post_name", post.Key},
                        {"@to_ping", ""},
                        {"@pinged", ""},
                        {"@post_modified", DateTime.Now},
                        {"@post_modified_gmt", DateTime.Now},
                        {"@post_content_filtered", ""},
                        {"@post_parent", 0},
                        {"@guid", entry.getCiteKey()},
                        {"@menu_order", 0},
                        {"@post_type", "post"},
                        {"@post_mime_type", ""},
                        {"@comment_count", 0}
                    };

                    foreach (var kvp in paramValues)
                    {
                        postCmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                } else
                {
                    postCmd.CommandText = string.Format(
                        @"update {0}_posts set post_content=@post_content, post_title=@post_title, post_date=@post_date,
                            post_date_gmt=@post_date_gmt
                            where post_name = '{1}'", tablePrefix, post.Key);
                    postCmd.Prepare();
                    Dictionary<string, object> paramValues = new Dictionary<string, object>()
                    {
                        {"@post_date", post.DateTime},
                        {"@post_date_gmt", post.DateTime},
                        {"@post_content", post.PostText},
                        {"@post_title", post.Title}
                    };
                    foreach (var kvp in paramValues)
                    {
                        postCmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }

                postCmd.ExecuteNonQuery();
                
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }





        Dictionary<string, BibtexEntry> GetBibtexEntries(string path)
        {
            var parser = new BibtexParser(new StreamReader(path));
            var result = parser.Parse();
            var db = result.Database;

            var entries = new Dictionary<string, BibtexEntry>();

            foreach (var entry in db.getEntries())
            {
                entries[entry.getCiteKey()] = entry;
            }

            return entries;
        }
    }
}
