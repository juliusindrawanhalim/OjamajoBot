using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OjamajoBot.Database.Models
{
    public class Models
    {

        public Scenario _scenario;

        public enum Scenario { 
            SCENARIO_INSERT,
            SCENARIO_UPDATE,
            SCENARIO_DELETE
        };

        public string tableName;
        public DataTable dataTable;


        /*string[,] parameter example:
         * new string[,] { 
         * {"id_guild","???"}
         * };
        */

        public string where;//filter for where
        public string[,] insertKeyValue;
        public string[,] whereKeyValue;
        public string[,] updateKeyValue;

        public Models(string tableName)
        {
            this.tableName = tableName;
        }

        public string GetTableName()
        {
            return tableName;
        }

        public void getArrayAttr()
        {
            IDictionary<string, string> arr = new Dictionary<string, string>();
            //var allowedAttributes;

        }

        public string[] getDatabaseField()
        {
            return new string[] { };
        }

        public void insert(string[,] insertKeyValue=null)
        {
            try
            {
                if (insertKeyValue == null) insertKeyValue = this.insertKeyValue;

                string sqlInsertColumn = string.Empty;
                string query = $"INSERT INTO {tableName}(";
                for (int i = 0; i < insertKeyValue.GetLength(0); i++)
                {
                    string parameterName = $"{insertKeyValue[i, 0]}";
                    sqlInsertColumn += string.IsNullOrWhiteSpace(sqlInsertColumn) ? parameterName : ", " + parameterName;
                }

                query += $"{sqlInsertColumn}) VALUES(";
                string sqlInsertValues = string.Empty;
                for (int i = 0; i < insertKeyValue.GetLength(0); i++)
                {
                    string parameterName = $"@{insertKeyValue[i, 0]}{i}";
                    sqlInsertValues += string.IsNullOrWhiteSpace(sqlInsertValues) ? parameterName : ", " + parameterName;
                }
                query += $"{sqlInsertValues})";
                using (MySqlCommand cmd = new MySqlCommand(query, Database.dbConn))
                {
                    for (int i = 0; i < insertKeyValue.GetLength(0); i++)
                    {
                        cmd.Parameters.AddWithValue($"@{insertKeyValue[i, 0]}{i}", insertKeyValue[i, 1]);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
            catch(Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
            
        }

        public DataTable find(string selectedColumn = "*", Scenario scenario = Scenario.SCENARIO_UPDATE)
        {
            try
            {
                var list = new List<dynamic>();
                _scenario = scenario;

                string query = $"SELECT {selectedColumn} FROM {tableName} ";
                if (where == null && whereKeyValue != null)
                {
                    string sqlWhere = string.Empty;
                    for (int i = 0; i < whereKeyValue.GetLength(0); i++)
                    {
                        sqlWhere += $"{whereKeyValue[i, 0]}=@{whereKeyValue[i, 0]}{i}";

                        if (i < whereKeyValue.GetLength(0) - 1)
                            sqlWhere += " AND ";

                    }
                    where = sqlWhere;
                    query += $"WHERE {sqlWhere} ";
                }
                else if (where != null)
                {
                    query += $"WHERE {where} ";
                }
                query += " LIMIT 1";

                using (MySqlCommand cmd = new MySqlCommand(query, Database.dbConn))
                {
                    //int index = 0;
                    string sqlWhere = string.Empty;

                    if (whereKeyValue != null)
                    {
                        for (int i = 0; i < whereKeyValue.GetLength(0); i++)
                        {
                            string parameterName = $"@{whereKeyValue[i, 0]}{i}";
                            sqlWhere += string.IsNullOrWhiteSpace(sqlWhere) ? parameterName : ", " + parameterName;
                            cmd.Parameters.AddWithValue(parameterName, whereKeyValue[i, 1]);
                        }
                    }

                    query = string.Format(query, sqlWhere);
                    cmd.CommandText = query;
                    using (MySqlDataReader row = cmd.ExecuteReader())
                    {
                        while (row.Read())
                        {
                            var obj = new ExpandoObject();
                            var dict = obj as IDictionary<String, object>;
                            for (int index = 0; index < row.FieldCount; index++)
                            {
                                if (!row.IsDBNull(index))
                                {
                                    dict[row.GetName(index)] = row.GetString(index);
                                }
                            }
                            list.Add(obj);

                        }
                    }
                }

                var json = JsonConvert.SerializeObject(list);
                DataTable dataTable = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));
                this.dataTable = dataTable;

                return dataTable;
            }
            catch
            {
                return null;
            }
            
        }

        public DataTable findAll(string selectedColumn = "*",Scenario scenario = Scenario.SCENARIO_UPDATE)
        {
            try
            {
                var list = new List<dynamic>();
                _scenario = scenario;

                string query = $"SELECT {selectedColumn} FROM {tableName} ";
                if (where == null && whereKeyValue != null)
                {
                    string sqlWhere = string.Empty;
                    for (int i = 0; i < whereKeyValue.GetLength(0); i++)
                    {
                        sqlWhere += $"{whereKeyValue[i, 0]}=@{whereKeyValue[i, 0]}{i}";

                        if (i < whereKeyValue.GetLength(0) - 1)
                            sqlWhere += " AND ";

                    }
                    where = sqlWhere;
                    query += $"WHERE {sqlWhere} ";
                }
                else if (where != null)
                {
                    query += $"WHERE {where} ";
                }

                using (MySqlCommand cmd = new MySqlCommand(query, Database.dbConn))
                {
                    //int index = 0;
                    string sqlWhere = string.Empty;

                    if (whereKeyValue != null)
                    {
                        for (int i = 0; i < whereKeyValue.GetLength(0); i++)
                        {
                            string parameterName = $"@{whereKeyValue[i, 0]}{i}";
                            sqlWhere += string.IsNullOrWhiteSpace(sqlWhere) ? parameterName : ", " + parameterName;
                            cmd.Parameters.AddWithValue(parameterName, whereKeyValue[i, 1]);
                        }
                    }

                    query = string.Format(query, sqlWhere);
                    cmd.CommandText = query;
                    using (MySqlDataReader row = cmd.ExecuteReader())
                    {
                        while (row.Read())
                        {
                            var obj = new ExpandoObject();
                            var dict = obj as IDictionary<String, object>;
                            for (int index = 0; index < row.FieldCount; index++)
                            {
                                if (!row.IsDBNull(index))
                                {
                                    dict[row.GetName(index)] = row.GetString(index);
                                }
                            }
                            list.Add(obj);
                        }
                    }
                }

                var json = JsonConvert.SerializeObject(list);
                DataTable dataTable = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));
                this.dataTable = dataTable;

                return dataTable;
            }
            catch
            {
                return null;
            }
            
        }

        public void save()
        {
            try
            {
                string query = string.Empty;
                if(_scenario == Scenario.SCENARIO_INSERT)
                {

                } 
                else if(_scenario == Scenario.SCENARIO_UPDATE)
                {
                    string sqlSet = string.Empty;
                    if (updateKeyValue != null)
                    {
                        for (int i = 0; i < updateKeyValue.GetLength(0); i++)
                        {
                            string parameterName = $"{updateKeyValue[i, 0]}=@update_{updateKeyValue[i, 0]}{i}";
                            sqlSet += string.IsNullOrWhiteSpace(sqlSet) ? parameterName : ", " + parameterName;
                        }
                    }

                    query = $"UPDATE {tableName} " +
                        $"SET {sqlSet} ";

                    if (whereKeyValue != null)
                    {
                        query += $"WHERE {where}";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, Database.dbConn))
                    {
                        string sqlWhere = string.Empty;

                        for (int i = 0; i < updateKeyValue.GetLength(0); i++)
                        {
                            cmd.Parameters.AddWithValue($"@update_{updateKeyValue[i, 0]}{i}", updateKeyValue[i, 1]);
                        }

                        if (whereKeyValue != null)
                        {
                            for (int i = 0; i < whereKeyValue.GetLength(0); i++)
                            {
                                string parameterName = $"@{whereKeyValue[i, 0]}{i}";
                                sqlWhere += string.IsNullOrWhiteSpace(sqlWhere) ? parameterName : ", " + parameterName;
                                cmd.Parameters.AddWithValue(parameterName, whereKeyValue[i, 1]);
                            }
                        }
                        
                        query = string.Format(query, sqlWhere);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
        }

    }
}
