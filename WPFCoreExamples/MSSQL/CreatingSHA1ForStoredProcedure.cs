using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCoreExamples.Cryptography;

namespace WPFCoreExamples.MSSQL
{
    class CreatingSHA1ForStoredProcedure
    {
        #region Properties

        /// <summary>
        /// The name of the SQL server to connect to
        /// </summary>
        private string _serverName = "";
        /// <summary>
        /// The name of the SQL server to connect to
        /// </summary>
        public string ServerName
        {
            get { return _serverName; }
            set
            {
                    _serverName = value;
            }
        }

        /// <summary>
        /// The Result of the SQL Connection
        /// </summary>
        private string _connectionResult = "";
        /// <summary>
        /// The Result of the SQL Connection
        /// </summary>
        public string ConnectionResult
        {
            get { return _connectionResult; }
            set
            {
                    _connectionResult = value;
            }
        }

        private bool _connectionResultAsBool = false;

        public bool ConnectionResultAsBool
        {
            set
            {
                _connectionResultAsBool = value;
            }
        }

        /// <summary>
        /// List of all the databases on current server
        /// </summary>
        private ObservableCollection<string> _databaseList = new ObservableCollection<string>();
        /// <summary>
        /// List of all the databases on current server
        /// </summary>
        public ObservableCollection<string> DatabaseList
        {
            get { return _databaseList; }
            set
            {
                    _databaseList = value;
            }
        }

        /// <summary>
        /// Currently Selected Database
        /// </summary>
        private string _selectedDatabase = "";
        /// <summary>
        /// Currently Selected Database
        /// </summary>
        public string SelectedDatabase
        {
            get { return _selectedDatabase; }
            set
            {
                if (_selectedDatabase == null)
                    _selectedDatabase = "";
                _selectedDatabase = value;

                StoredProcList = loadStoredProcs(value);
            }
        }

        /// <summary>
        /// List of all the Stored Procedures on current selected database
        /// </summary>
        private ObservableCollection<string> _storedProcList = new ObservableCollection<string>();
        /// <summary>
        /// List of all the Stored Procedures on current selected database
        /// </summary>
        public ObservableCollection<string> StoredProcList
        {
            get { return _storedProcList; }
            set
            {
                if (_storedProcList != null)
                {
                    _storedProcList = value;
                }
            }
        }

        /// <summary>
        /// Currently Selected Stored Procedure
        /// </summary>
        private string _selectedStoredProc = "";
        /// <summary>
        /// Currently Selected Stored Procedure
        /// </summary>
        public string SelectedStoredProc
        {
            get { return _selectedStoredProc; }
            set
            {
                if (_selectedStoredProc == null)
                    StoredProcText = "";

                _selectedStoredProc = value;

                StoredProcText = ReadStoredProc(SelectedDatabase, value);
            }
        }

        /// <summary>
        /// The Text Output from the stored Procedure
        /// </summary>
        private string _storedProcText = "";
        /// <summary>
        /// The Text Output from the stored Procedure
        /// </summary>
        public string StoredProcText
        {
            get => _storedProcText;
            set
            {
                if (_storedProcText != null)
                {
                    if (!string.IsNullOrEmpty(value))
                        ShaOutput = SHA1Util.SHA1HashStringForUTF8String(value);
                    _storedProcText = value;
                }
            }
        }

        /// <summary>
        /// The Sha1 value for the Stored Proc Text
        /// </summary>
        private string _shaOutput = "";
        /// <summary>
        /// The Sha1 value for the Stored Proc Text
        /// </summary>
        public string ShaOutput
        {
            get { return _shaOutput; }
            set
            {
                if (_shaOutput != null)
                {
                    _shaOutput = value;
                }
            }
        }


        #endregion


        #region SQL Methods

        private async void LoadDataAsync()
        {
            ConnectionResult = "Testing";

            Task<string> connection = TestConnection();
            await connection.ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        ConnectionResult = t.Result;
                        DatabaseList = GetDatabaseList();
                        SelectedDatabase = DatabaseList.FirstOrDefault();
                        ConnectionResultAsBool = true;
                        break;
                    case TaskStatus.Faulted:
                        StoredProcList = new ObservableCollection<string>();
                        DatabaseList = new ObservableCollection<string>();
                        SelectedDatabase = "";
                        SelectedStoredProc = "";
                        if (t.Exception != null)
                        {
                            foreach (Exception ex in t.Exception.Flatten().InnerExceptions)
                            {
                                ConnectionResultAsBool = false;
                                ConnectionResult = ex.Message;
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Gets the connection status
        /// </summary>
        /// <returns></returns>
        private async Task<string> TestConnection()
        {
            string output = "Connected";

            await Task.Run(() =>
            {
                using (SqlConnection con = new SqlConnection(SQLConn.SQLConnString("Master")))
                {
                    try
                    {
                        con.Open();
                    }
                    finally
                    {
                        con.Close();
                    }

                }
            });

            return output;
        }


        private ObservableCollection<string> GetDatabaseList()
        {
            ObservableCollection<string> list = new ObservableCollection<string>();

            using (SqlConnection con = new SqlConnection(SQLConn.SQLConnString("Master")))
            {
                con.Open();

                // Set up a command with the given query and associate
                // this with the current connection.
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }

                con.Close();
            }
            return list;
        }

        //Enumerate and load all stored procedures from the database 
        private ObservableCollection<string> loadStoredProcs(string database)
        {
            ObservableCollection<string> list = new ObservableCollection<string>();

            if (String.IsNullOrEmpty(database))
                return list;

            using (SqlConnection con = new SqlConnection(SQLConn.SQLConnString(database)))
            {
                con.Open();

                // Set up a command with the given query and associate
                // this with the current connection.
                using (SqlCommand cmd = new SqlCommand("Select [NAME] from sysobjects where type = 'P' order by [NAME]", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }
                con.Close();
            }
            return list;
        }

        private string ReadStoredProc(string database, string storedProc)
        {
            StringBuilder sb = new StringBuilder();
            string output = sb.ToString();

            if (String.IsNullOrEmpty(storedProc))
                return output;

            using (SqlConnection con = new SqlConnection(SQLConn.SQLConnString(database)))
            {
                con.Open();

                // Set up a command with the given query and associate
                // this with the current connection.
                using (SqlCommand cmd = new SqlCommand($"EXEC sp_helptext '{storedProc}'", con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sb.Append(dr[0]);
                        }
                    }
                }

                con.Close();
            }
            output = sb.ToString();
            return output;
        }

        #endregion
    }

    public static class SQLConn
    {
        /// <summary>
        /// Server connection name
        /// </summary>
        public static string Server { get; set; } = ".";

        /// <summary>
        /// Authentication Type
        /// </summary>
        //public static SqlAuthenticationMethod AuthMethod { get; set; } =
        //SqlAuthenticationMethod.ActiveDirectoryIntegrated;

        /// <summary>
        /// Trust Servers Certificate
        /// </summary>
        public static bool TrustCert { get; set; } = true;

        /// <summary>
        /// Run process Asynchronous
        /// </summary>
        public static bool AsyncProcess { get; set; } = true;

        /// <summary>
        /// SQL Connection string
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static string SQLConnString(string database)
        {
            // Create a new SqlConnectionStringBuilder and
            // initialize it with a few name/value pairs.
            SqlConnectionStringBuilder builder =
                new SqlConnectionStringBuilder()
                {
                    DataSource = Server,
                    IntegratedSecurity = true,
                    //Authentication = AuthMethod,
                    TrustServerCertificate = TrustCert,
                    InitialCatalog = database,
                    //AsynchronousProcessing = AsyncProcess
                };
            return builder.ConnectionString;
        }
    }
}
