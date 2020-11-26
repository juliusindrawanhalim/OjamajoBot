using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Lavalink4NET.Statistics;
using MySql.Data.MySqlClient;

namespace OjamajoBot.Database
{
    class Database
    {
        public static string ipAddress; public static string port;
        public static string username; public static string password;
        public static MySqlConnection dbConn;
        static string connectionString;

        public Database()
        {
            // Prepare the connection
            connectionString = $"datasource={ipAddress};port={port};username={username};password={password};database=ojamajo_bot;";
            dbConn = new MySqlConnection(connectionString);
            dbConn.Open();
        }
        
    }
}
