using System;
using System.Collections.Generic;

using DarkRift;
using DarkRift.ConfigTools;
using DarkRift.Storage;

using MySql.Data.MySqlClient;

namespace MySQLConnector
{
	class Connector : Database
	{
		public override string name {
			get {
				return "MySQLConnector";
			}
		}

		public override string version {
			get {
				return "1.0";
			}
		}

		public override Command[] commands {
			get {
				return new Command[0];
			}
		}

		public override string author {
			get {
				return "Jamie Read";
			}
		}


		public override string supportEmail {
			get {
				return "jamie.read@outlook.com";
			}
		}

		public override string databaseName {
			get {
				return "MySQL";
			}
		}

		string connectionString = "";

        public Connector()
        {
            InstallSubdirectory(
                new Dictionary<string, byte[]>
                {
                    {"settings.cnf",    System.Text.Encoding.ASCII.GetBytes("ConnectionString:\t\tSERVER=localhost;DATABASE=DarkRift;USERNAME=username;PASSWORD=password")}
                }
            );

            try
            {
                ConfigReader reader = new ConfigReader(GetSubdirectory() + @"\settings.cnf");

                if( reader["ConnectionString"] == null )
                {
                    Interface.LogFatal("ConnectionString was not defined in the MySQLConnector's settings.cnf file!");
                    DarkRiftServer.Close(true);
                }
                connectionString = reader["ConnectionString"];
            }
            catch(System.IO.FileNotFoundException)
            {
                Interface.LogFatal("MySQLConnector's settings file is missing!");
                DarkRiftServer.Close(true);
            }
        }

		/// <summary>
		/// 	Execute a query on the server with no return values.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="paramaters">The paramaters to be added to this query.</param>
		public override void ExecuteNonQuery (string query, params QueryParameter[] parameters)
		{
			try
            {
                using (MySqlConnection connection = new MySqlConnection (connectionString))
    			{
    				using (MySqlCommand command = new MySqlCommand (query, connection))
    				{
    					foreach (QueryParameter parameter in parameters)
    						command.Parameters.AddWithValue(parameter.name, parameter.obj);

    					connection.Open();
    					command.ExecuteNonQuery ();
    				}
    			}
            }
            catch(MySqlException e)
            {
                throw new DatabaseException(e.Message, e);
            }
		}

		/// <summary>
		/// 	Executes a query on the database with a scalar return.
		/// </summary>
		/// <returns>The object returned from the database.</returns>
		/// <param name="query">The query.</param>
		/// <param name="paramaters">The paramaters to be added to this query.</param>
		public override object ExecuteScalar(string query, params QueryParameter[] parameters)
		{
			try
            {
                using (MySqlConnection connection = new MySqlConnection (connectionString))
    			{
    				using (MySqlCommand command = new MySqlCommand (query, connection))
    				{
    					foreach (QueryParameter parameter in parameters)
    						command.Parameters.AddWithValue(parameter.name, parameter.obj);

    					connection.Open();
    					return command.ExecuteScalar ();
    				}
    			}
            }
            catch(MySqlException e)
            {
                throw new DatabaseException(e.Message, e);
            }
		}

		/// <summary>
		/// 	Executes a query on the database returning an array of rows.
		/// </summary>
		/// <returns>The rows of the database selected.</returns>
		/// <param name="query">The query.</param>
		/// <param name="paramaters">The paramaters to be added to this query.</param>
		public override DatabaseRow[] ExecuteQuery (string query, params QueryParameter[] parameters)
		{
			try
            {
                using (MySqlConnection connection = new MySqlConnection (connectionString))
    			{
    				using (MySqlCommand command = new MySqlCommand (query, connection))
    				{
    					foreach (QueryParameter parameter in parameters)
    						command.Parameters.AddWithValue(parameter.name, parameter.obj);

    					connection.Open();
    					using (MySqlDataReader reader = command.ExecuteReader())
    					{
    						int fieldCount = reader.FieldCount;
    						List<DatabaseRow> rows = new List<DatabaseRow> ();

    						while (reader.Read())
    						{
    							//For each row create a DatabaseRow
    							DatabaseRow row = new DatabaseRow();

    							//And add each field to it
    							for (int i = 0; i < fieldCount; i++)
    							{
    								row.Add(
    									reader.GetName(i),
    									reader.GetValue(i)
    								);
    							}

    							//Add it to the rows
    							rows.Add(row);
    						}

    						return rows.ToArray();
    					}
    				}
			    }
            }
            catch(MySqlException e)
            {
                throw new DatabaseException(e.Message, e);
            }
		}

		/// <summary>
		/// 	Removes any characters that could allow SQL injection.
		/// </summary>
		/// <param name="c">The string to escape</param>
		/// <param name="query">Query.</param>
		public override string EscapeString(string s)
		{
			return MySql.Data.MySqlClient.MySqlHelper.EscapeString (s);
		}

		/// <summary>
		/// 	Releases all resource used by the <see cref="MySQLConnector.Connector"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="MySQLConnector.Connector"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="MySQLConnector.Connector"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="MySQLConnector.Connector"/> so the
		/// garbage collector can reclaim the memory that the <see cref="MySQLConnector.Connector"/> was occupying.</remarks>
		public override void Dispose()
		{

		}
	}
}
