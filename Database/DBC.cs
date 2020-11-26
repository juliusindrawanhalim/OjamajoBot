using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace OjamajoBot.Database
{
    class DBC
    {
        private JObject jobjectconfig;
        private MySqlConnection connection;
        private string server; private string database;
        private string port; private string username;
        private string password;

        private IDictionary<string, string> columns = new Dictionary<string, string>();

        //Constructor
        public DBC()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            jobjectconfig = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.configFileName}"));

            try
            {
                string _parent = "database";
                database = "ojamajo_bot";
                server = jobjectconfig.GetValue(_parent)["host"].ToString();
                port = jobjectconfig.GetValue(_parent)["port"].ToString();
                username = jobjectconfig.GetValue(_parent)["username"].ToString();
                password = jobjectconfig.GetValue(_parent)["password"].ToString();
            }
            catch { Console.WriteLine("Error: Database configuration array is not properly formatted"); Console.ReadLine(); }

            string connectionString = $"datasource={server};port={port};username={username};password={password};database={database};";
            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public DataTable selectAll(string query,
            IDictionary<string,object> filterWhere = null)
        {
            DataTable dt = new DataTable();

            //MySqlDataReader ret = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                //string query = "";
                if (filterWhere != null)
                {
                    for (int i = 0; i < filterWhere.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@{filterWhere.ElementAt(i).Key}", filterWhere[filterWhere.ElementAt(i).Key]);
                    }
                }
                
                using (connection)
                {
                    connection.Open();
                    using (cmd)
                    {
                        cmd.CommandText = query;
                        cmd.Connection = connection;
                        dt.Load(cmd.ExecuteReader());
                        
                        //ret = cmd.ExecuteReader();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return dt;
        }

        public void insert(string tableName,
            IDictionary<string, object> tableParameters)
        {
            try
            {
                string parameterTable = "";
                string parameterValues = "";
                MySqlCommand cmd = new MySqlCommand();
                for (int i = 0; i < tableParameters.Count; i++)
                {
                    parameterTable += $"{tableParameters.ElementAt(i).Key},";
                    parameterValues += $"@{tableParameters.ElementAt(i).Key},";
                    cmd.Parameters.AddWithValue($"@{tableParameters.ElementAt(i).Key}", 
                        tableParameters[tableParameters.ElementAt(i).Key]);

                }

                parameterTable = parameterTable.TrimEnd(',');
                parameterValues = parameterValues.TrimEnd(',');

                string query = $"INSERT INTO {tableName}({parameterTable}) VALUES({parameterValues})";
                
                using (connection)
                {
                    connection.Open();

                    using (cmd)
                    {
                        cmd.CommandText = query;
                        cmd.Connection = connection;
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void update(string query,
            IDictionary<string, object> tableParameters)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                for (int i = 0; i < tableParameters.Count; i++)
                {
                    if (tableParameters[tableParameters.ElementAt(i).Key].ToString()=="")
                    {
                        cmd.Parameters.AddWithValue($"@{tableParameters.ElementAt(i).Key}", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue($"@{tableParameters.ElementAt(i).Key}", tableParameters[tableParameters.ElementAt(i).Key]);
                    }

                }

                using (connection)
                {
                    connection.Open();

                    using (cmd)
                    {
                        cmd.CommandText = query;
                        cmd.Connection = connection;
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void delete(string query,
            IDictionary<string, object> tableParameters)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                for (int i = 0; i < tableParameters.Count; i++)
                {
                    if (tableParameters[tableParameters.ElementAt(i).Key].ToString() == "")
                    {
                        cmd.Parameters.AddWithValue($"@{tableParameters.ElementAt(i).Key}", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue($"@{tableParameters.ElementAt(i).Key}", tableParameters[tableParameters.ElementAt(i).Key]);
                    }

                }

                using (connection)
                {
                    connection.Open();

                    using (cmd)
                    {
                        cmd.CommandText = query;
                        cmd.Connection = connection;
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }

}
