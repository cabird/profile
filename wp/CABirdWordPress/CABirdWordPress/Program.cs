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
using System.Text.RegularExpressions;

namespace CABirdWordPress
{
    class Program
    {


        static void Main(string[] args)
        {
            Program program = new Program();

            string bibtexPath = @"C:\Users\cbird\Documents\GitHub\cv\bird.bib";

            var entries = program.GetBibtexEntries(bibtexPath);

            program.Connect();
            int a = program.GetOrCreateTagID("Test Tag");

            program.PostToDatabase(entries["barnett2015helping"]);
            return;

            foreach (var entry in entries.Values)
            {
                Debug.WriteLine("Adding/Updating {0}", entry.getField("title"));
                program.PostToDatabase(entry);
            }
           
        }

        MySqlConnection conn;
        string tablePrefix = "wp_2a8dr8";

        void Connect()
        {
            string password = Microsoft.VisualBasic.Interaction.InputBox("Database Password", "Credential Information");
            string connectionString = string.Format("Server=mysql.cabird.com;Database=cabird_com_4;Uid=cabird;Pwd={0}", password); 
            conn = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
            conn.Open();
        }

        void PostToDatabase(BibtexEntry entry)
        {
            PostData post = new PostData(entry);
            

            string tablePrefix = "wp_2a8dr8";

            try
            {
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

                AddCategoryToPost("Publications", post.Key);
                
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        public void AddCategoryToPost(string category, string postKey)
        {

            int categoryId = GetOrCreateCategoryID(category);

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;

            // need to do a left join here...
            cmd.CommandText = string.Format(
                @"select p.ID, tt.term_taxonomy_id
                from {0}_posts p, {0}_terms t, {0}_term_taxonomy tt
                where post_name='{1}' and t.term_id = tt.term_id and tt.taxonomy='category'",
                tablePrefix, postKey);

            int postID = -1;
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    postID = reader.GetInt32(0);
                    if (reader.GetInt32(1) == categoryId)
                    {
                        return;
                    }
                }
            }
            cmd.CommandText = string.Format(
                @"insert into {0}_term_relationships (object_id, term_taxonomy_id) values ({1}, {2})",
                tablePrefix, postID, categoryId);
            cmd.ExecuteNonQuery();

        }

        public string Slugify(string term)
        {
            return Regex.Replace(term, @"\s+", "-").ToLower();
        }

        public int GetOrCreateTagID(string tag)
        {
            string sql = string.Format(
                @"select t.term_id from {0}_terms t, {0}_term_taxonomy tt 
                where t.name = '{1}' and t.term_id = tt.term_id and tt.taxonomy='post_tag' ",
                tablePrefix, tag);
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            object value = cmd.ExecuteScalar();
            if (value != null)
            {
                return Convert.ToInt32(value);
            }

            /* it didn't exist, so add it */
            sql = string.Format("insert into {0}_terms (name, slug, term_group) values ('{1}', '{2}', 0)", tablePrefix, tag, Slugify(tag));
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            cmd.CommandText = "select last_insert_id()";
            int id = Convert.ToInt32(cmd.ExecuteScalar());
            sql = string.Format("insert into {0}_term_taxonomy (term_id, taxonomy) values ({1}, 'post_tag')", tablePrefix, id);
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            return id;

        }

        public int GetOrCreateCategoryID(string category)
        {
            string sql = string.Format(
               @"select t.term_id from {0}_terms t, {0}_term_taxonomy tt 
                where t.name = '{1}' and t.term_id = tt.term_id and tt.taxonomy='category' ",
               tablePrefix, category);
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            object value = cmd.ExecuteScalar();
            if (value != null)
            {
                return Convert.ToInt32(value);
            }

            /* it didn't exist, so add it */
            sql = string.Format("insert into {0}_terms (name, slug, term_group) values ('{1}', '{2}', 0)", tablePrefix, category, Slugify(category));
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            cmd.CommandText = "select last_insert_id()";
            int id = Convert.ToInt32(cmd.ExecuteScalar());
            sql = string.Format("insert into {0}_term_taxonomy (term_id, taxonomy) values ({1}, 'category')", tablePrefix, id);
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            return id;
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
