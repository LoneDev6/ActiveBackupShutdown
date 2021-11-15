using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ActiveBackupShutdown
{
    class Program
    {
        [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        private static Timer timer = new Timer(mainLogic, "OhYeaBoy", TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(15));

        private static void mainLogic(object state)
        {
            string path = @"C:\ProgramData\ActiveBackupforBusinessAgent\system-db.sqlite";
            using (var connection = new SQLiteConnection($"Data Source='{path}';Read Only=True"))
            {
                connection.Open();

                // Check if any backup still going on
                if(!anyBackupStillGoingOn(connection))
                {
                    anyBackupFinishedRecently(connection);
                }
            }
        }

        private static bool anyBackupStillGoingOn(SQLiteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT count(*) as amount FROM  snapshot_table WHERE status is null or status = '' or status = ' '";
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var amount = dr.GetInt16(0);
                        if (amount > 0) // Still backing up
                        {
                            Console.WriteLine($"{amount} backups still processing, waiting...");
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            throw new InvalidOperationException("Failed to execute this program query somehow, wtf");
        }

        private static void anyBackupFinishedRecently(SQLiteConnection connection)
        {
            // Check if all backups finished in a range of 20 minutes
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select count(*) as amount from task_table where strftime('%s', 'now') - last_backup_success_time < 1200";
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var amount = dr.GetInt16(0);
                        if (amount > 0) // One or more backups finished in the last 5 minutes.
                        {
                            Console.WriteLine("Stopping timer..");

                            Console.WriteLine("All backup finished, hibernating in 30 seconds...");
                            Thread.Sleep(10000);

                            Process[] pname = Process.GetProcessesByName("logonui");
                            if (pname.Length == 0)
                            {
                                Console.WriteLine("Cancelled hibernation, user is using the PC.");
                            }
                            else
                            {
                                Console.WriteLine("Hibernating...");
                                // Hibernate
                                SetSuspendState(true, true, true);
                                Thread.Sleep(100);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No backup finished in the last time interval.");
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Console.ReadLine(); // To keep this shit alive
        }
    }
}
