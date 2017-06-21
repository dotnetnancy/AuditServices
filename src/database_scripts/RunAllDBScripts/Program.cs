using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Data.SqlClient;
using System.Configuration;


namespace ConcatenateFilesInDirectoryToSingleFile_DBScripts
{
    class Program
    {
       
        static void Main(string[] args)
        {
            string sqlConnectionString = ConfigurationSettings.AppSettings["MasterConnectionString"];
            int count = 0;
            foreach (string filename in Directory.GetFiles(ConfigurationSettings.AppSettings["PathToScripts"], "*.sql"))
            {
                if (count == 1)
                {
                    sqlConnectionString = ConfigurationSettings.AppSettings["DatabaseConnectionString"];
                }
                FileInfo file = new FileInfo(filename);
                string script = file.OpenText().ReadToEnd();
                SqlConnection conn = new SqlConnection(sqlConnectionString);
                Server server = new Server(new ServerConnection(conn));
                try
                {
                    server.ConnectionContext.ExecuteNonQuery(script);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                count++;
            }
            Console.WriteLine("Completed Running of Scripts");
            Console.ReadLine();
        }
    }
}


