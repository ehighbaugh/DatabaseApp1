using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("*** SQL SERVER CONSOLE APP DEMO ***");

                // connection string
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "localhost";
                builder.UserID = "sa";
                builder.Password = "password";
                builder.InitialCatalog = "master";

                // connect to SQL
                Console.WriteLine("Connecting to SQL Server ... ");
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    Console.WriteLine("Done.");

                    // create sample database
                    Console.WriteLine("Dropping and creating database 'SampleDB' ... ");
                    String sql = "DROP DATABASE IF EXISTS [SampleDB]; CREATE DATABASE [SampleDB]";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Done.");
                    }

                    // insert 5000 rows into table 'Table_with_500_rows'
                    Console.WriteLine("Inserting 5000 rows into table 'Table_with_500_rows'. Please wait a second ... ");
                    StringBuilder sb = new StringBuilder();
                    sb.Append("USE SampleDB; ");
                    sb.Append("WITH a AS (SELECT * FROM (VALUES(1),(2),(3),(4),(5)) AS a(a))");
                    sb.Append("SELECT TOP(5000)");
                    sb.Append("ROW_NUMBER() OVER (ORDER BY a.a) AS OrderItemId");
                    sb.Append(",a.a + b.a + c.a + d.a + e.a AS OrderId ");
                    sb.Append(",a.a * 10 AS Price ");
                    sb.Append(",CONCAT(a.a, N' ', b.a, N' ', c.a, N' ', d.a N' ', e.a) AS ProductName ");
                    sb.Append("INTO Table_with_5000_rows ");
                    sb.Append("FROM a, a AS b, a AS c, a AS d, a AS d;");
                    sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Done.");
                    }

                    // execute SQL query without columnstore index
                    double elapsedTimeWithoutIndex = SumPrice(connection);
                    Console.WriteLine("Query time WITHOUT columnstore index: " + elapsedTimeWithoutIndex + "ms");

                    // add columnstore index
                    Console.WriteLine("Adding a columnstore to table 'Table_with_5000_rows' ... ");
                    sql = "CREATE CLUSTERED COLUMNSTORE INDEX columnstoreindex ON Table_with_5000_rows';";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Done.");
                    }

                    // execute same SQL query again after columnstore index was added
                    double elapsedTimeWithIndex = SumPrice(connection);
                    Console.WriteLine("Query time WITH columnstore index: " + elapsedTimeWithIndex + "ms");

                    // calculate performance gain from adding columnstore index
                    Console.WriteLine("Performance improvement with columnstore index: "
                        + Math.Round(elapsedTimeWithoutIndex / elapsedTimeWithIndex) + "x!");
                }
                Console.WriteLine("All done. Press any key to finish ... ");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static double SumPrice(SqlConnection connection)
        {
            String sql = "SELECT SUM(Price) FROM Table_with_5000_rows";
            long startTicks = DateTime.Now.Ticks;
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                try
                {
                    var sum = command.ExecuteScalar();
                    TimeSpan elapsed = TimeSpan.FromTicks(DateTime.Now.Ticks) - TimeSpan.FromTicks(startTicks);
                    return elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return 0;

            Console.ReadLine();
        }
    }
}
