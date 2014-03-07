using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Nobel
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		BackgroundWorker updater = new BackgroundWorker();
		MySqlConnection connection;
		BeamerWindow bw = new BeamerWindow();
        Dictionary<string, long> points = new Dictionary<string, long>();
		List<Timeslot> timeslots = new List<Timeslot>();
        Dictionary<string, int> pointSources = new Dictionary<string, int>();
		FileInfo timeslotsPath = new FileInfo("timeslots.csv");
        FileInfo pointsourcesPath = new FileInfo("pointsources.csv");
        
		Timer timerDatabase = new Timer(30000);
		Action updateDB;
		//DateTime ecoStart = new DateTime(2013, 9, 18, 19, 00, 00);
       // DateTime ecoEnd = new DateTime(2013, 9, 19, 8, 00, 00);
        DateTime ecoStart = new DateTime(2014, 2, 26, 17, 00, 00);
        DateTime ecoEnd = new DateTime(2014, 2, 27, 0, 00, 00);
		TimeSpan timeslotLen = new TimeSpan(0, 30, 0);
		object _updaterLock = new object();
		int totalQueries = 0;

        Stopwatch performanceStopwatch = new Stopwatch();

		String getQuery = @"SELECT 
    SUM(bestelling.Bestelling_AantalS) as Totaal_Aantal, debiteur.Debiteur_Naam, prijs.Prijs_Naam
FROM
    bestelling
        LEFT JOIN
    (bon
    LEFT JOIN (debiteur) ON (Bon_Debiteur = Debiteur_ID)) ON (Bestelling_Bon = Bon_ID)
  LEFT JOIN
    (prijs) ON (Bestelling_Wat = Prijs_ID)
WHERE Prijs_Naam LIKE ?product AND Bestelling_Time>=?timestart AND Bestelling_Time<=?timeend GROUP BY Debiteur_ID";
		MySqlCommand getCmd;
		public MainWindow()
		{
			InitializeComponent();
					
			if (timeslotsPath.Exists)
			{
				using (StreamReader r = timeslotsPath.OpenText())
				{
					timeslots.Clear();
					timeSlotsListBox.Items.Clear();
					while (!r.EndOfStream)
					{
						String[] strings = r.ReadLine().Split(';');
						if (strings.Length % 2 != 0)
						{
							MessageBox.Show("Error in timeslots file. Format is 1 per line points per unit;unit;points per unit;unit.");
						}
						else
						{
							Dictionary<String, int> Items = new Dictionary<string, int>();
							StringBuilder sb = new StringBuilder();
							for (int i = 0; i < strings.Length; i += 2)
							{
								if (strings[i] == "" || strings[i + 1] == "")
								{
									MessageBox.Show("Error in timeslots file. Something is emtpy.");
								}
								else
								{
									int pointsperunit = Int32.Parse(strings[i]);
									String unit = strings[i + 1].Trim('"');
									Items.Add(unit, pointsperunit);
									totalQueries++;
									sb.AppendFormat("{1} ({0}), ", pointsperunit, unit);
								}
							}

							timeslots.Add(new Timeslot() { Items = Items });

							timeSlotsListBox.Items.Add(new ListBoxItem() { Content = sb.ToString() });
							sb = null;
						}
					}
				}
			}
			else
			{
				MessageBox.Show("No timeslots.csv found.");
			}
            if (pointsourcesPath.Exists)
            {
                using (StreamReader r = pointsourcesPath.OpenText())
                {
                    pointSources.Clear();
                    pointSourcesListBox.Items.Clear();
                    while (!r.EndOfStream)
                    {
                        String[] strings = r.ReadLine().Split(';');
                        if (strings.Length != 2)
                        {
                            MessageBox.Show("Error in pointsources file. Format is 1 per line points per unit;unit.");
                        }
                        else
                        {
                            if (strings[0] == "" || strings[1] == "")
                            {
                                MessageBox.Show("Error in timeslots file. Something is emtpy.");
                            }
                            else
                            {
                                int pointsperunit = Int32.Parse(strings[0]);
                                String unit = strings[1].Trim('"');
                                if (!pointSources.ContainsKey(unit))
                                {
                                    pointSources.Add(unit, pointsperunit);
                                }
                                totalQueries++;
                                pointSourcesListBox.Items.Add(new ListBoxItem() { Content = String.Format("{1} ({0})", pointsperunit, unit) });
                            }

                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No pointsources.csv found.");
            }
			updateDB = updateDatabase;
			updater.DoWork += updater_DoWork;
			updater.ProgressChanged += updater_ProgressChanged;
			updater.RunWorkerCompleted += updater_RunWorkerCompleted;
			updater.WorkerReportsProgress = true;
            bw.Show();
            timerDatabase.Elapsed += timerDatabase_Elapsed;
		}

		void updater_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
            updaterProgress.Value = 0;
            performanceStopwatch.Stop();
            writeOutputText(String.Format("Database update completed in {0:F2} seconds.", performanceStopwatch.Elapsed.TotalSeconds));
            lock (_updaterLock)
			{
				bw.updateScroller(ref points);
			}					
		}

		void updater_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			updaterProgress.Value = e.ProgressPercentage;
		}

		void updater_DoWork(object sender, DoWorkEventArgs e)
		{
			lock (_updaterLock)
			{
				int current = 0;
				int tsn = 0;
                points.Clear();
                //stuff that gets you points the whole evening.
                getCmd.Parameters["timestart"].Value = ecoStart.ToUnixTimestamp() * 1000;
                getCmd.Parameters["timeend"].Value = ecoEnd.ToUnixTimestamp() * 1000;
                foreach (KeyValuePair<String, int> kvp in pointSources)
                {
                    updater.ReportProgress((int)Math.Round(current / (double)totalQueries * 100.0));
                    try
                    {
                        getCmd.Parameters["product"].Value = kvp.Key;                        
                        using (MySqlDataReader dataReader = getCmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                String deb = dataReader["Debiteur_Naam"].ToString();
                                String item = dataReader["Prijs_Naam"].ToString();
                                int aantal = Int32.Parse(dataReader["Totaal_Aantal"].ToString());
                                if (item.Contains("fles", StringComparison.OrdinalIgnoreCase))
                                {
                                    aantal *= 20;
                                }
                                if (points.ContainsKey(deb))
                                {
                                    points[deb] += kvp.Value * aantal;
                                }
                                else
                                {
                                    points.Add(deb, kvp.Value * aantal);
                                }
                            }
                        }
                    }
                    catch (MySql.Data.MySqlClient.MySqlException ex)
                    {
                        MessageBox.Show("Query error " + ex.Number + " has occurred: " + ex.Message,
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    current++;
                }
                //stuff that gets you points per timeslot.              
				foreach (Timeslot ts in timeslots)
				{
					DateTime start = ecoStart.Add(timeslotLen.Multiply(tsn));
					DateTime end = ecoStart.Add(timeslotLen.Multiply(tsn + 1));
                    if (start > DateTime.Now || start > ecoEnd)
					{
                        continue;
					}
                    getCmd.Parameters["timestart"].Value = start.ToUnixTimestamp() * 1000;
                    getCmd.Parameters["timeend"].Value = end.ToUnixTimestamp() * 1000;
                    foreach (KeyValuePair<String, int> kvp in ts.Items)
                    {
                        updater.ReportProgress((int)Math.Round(current / (double)totalQueries * 100.0));
                        try
                        {
                            getCmd.Parameters["product"].Value = kvp.Key;
                            using (MySqlDataReader dataReader = getCmd.ExecuteReader())
                            {
                                while (dataReader.Read())
                                {
                                    String deb = dataReader["Debiteur_Naam"].ToString();
                                    String item = dataReader["Prijs_Naam"].ToString();
                                    int aantal = Int32.Parse(dataReader["Totaal_Aantal"].ToString());
                                    if (item.Contains("fles", StringComparison.OrdinalIgnoreCase))
                                    {
                                        aantal *= 20;
                                    }
                                    if (points.ContainsKey(deb))
                                    {
                                        points[deb] += kvp.Value * aantal;
                                    }
                                    else
                                    {
                                        points.Add(deb, kvp.Value * aantal);
                                    }
                                }
                            }
                        }
                        catch (MySql.Data.MySqlClient.MySqlException ex)
                        {
                            MessageBox.Show("Query error " + ex.Number + " has occurred: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        current++;
                    }
					tsn++;
				}
                var pointsSorted = from entry in points orderby entry.Value descending select entry;
                points = pointsSorted.ToDictionary(pair => pair.Key, pair => pair.Value);

			}
		}

		void timerDatabase_Elapsed(object sender, ElapsedEventArgs e)
		{
			g.Dispatcher.BeginInvoke(DispatcherPriority.Send, updateDB);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			bw._allowclosing = true;
			bw.Close();
			if (connection != null)
			{
				connection.Close();
			}
		}

        private void mysqlConnectButton_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnectionStringBuilder csb = new MySqlConnectionStringBuilder();

            if (String.IsNullOrEmpty(serverTextBox.Text))
            {
                MessageBox.Show("Enter server.");
                return;
            }
            if (String.IsNullOrEmpty(userTextBox.Text))
            {
                MessageBox.Show("Enter username.");
                return;
            }
            if (String.IsNullOrEmpty(passwordBox.Password))
            {
                MessageBox.Show("Enter password.");
                return;
            }
            if (String.IsNullOrEmpty(databaseTextBox.Text))
            {
                MessageBox.Show("Enter password.");
                return;
            }
            csb.Server = serverTextBox.Text;
            csb.Database = databaseTextBox.Text;
            csb.UserID = userTextBox.Text;
            csb.Password = passwordBox.Password;
            try
            {
                connection = new MySqlConnection(csb.ToString());
                connection.StateChange += connection_StateChange;
                connection.Open();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    MessageBox.Show("Not connected");
                }
                else
                {
                    mysqlConnectButton.IsEnabled = false;
                    timerDatabase.Start();
                    getCmd = new MySqlCommand(getQuery, connection);
                    getCmd.Prepare();
                    getCmd.Parameters.AddWithValue("product", String.Empty);
                    getCmd.Parameters.AddWithValue("timestart", 0L);
                    getCmd.Parameters.AddWithValue("timeend", 0L);
                    updateDatabase();
                    //getCmd.Parameters.Add(new MySqlParameter(
                }

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show("Connection/Preparation error " + ex.Number + " has occurred: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void connection_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            writeOutputText(String.Format("Database connection state change: {1} -> {0}.", e.CurrentState, e.OriginalState));
        }
        void writeOutputText(String text)
        {
            outputTextBox.AppendText(String.Format("{0:HH:mm:ss} -> {1}.", DateTime.Now, text));
            outputTextBox.AppendText(Environment.NewLine);
            outputTextBox.ScrollToEnd();
        }
		void updateDatabase()
		{
			if (!updater.IsBusy && connection.State == System.Data.ConnectionState.Open)
			{
                performanceStopwatch.Reset();
                performanceStopwatch.Start();
                writeOutputText("Database update started.");
                updater.RunWorkerAsync();
			}
            bw.updateTimeslots(ref timeslots, ref pointSources, ecoStart, timeslotLen);
		}

        private void forceUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            updateDatabase();
        }
	}
	public class Timeslot
	{
		public Dictionary<String,int> Items;
	}
	public static class DateTimeExtensions
	{
		public static long ToUnixTimestamp(this DateTime d)
		{
			var duration = d.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0); //Make GMT timestamps.

			return (long)duration.TotalSeconds;
		}
	}
	public static class TimeSpanExtension
	{
		/// <summary>
		/// Multiplies a timespan by an integer value
		/// </summary>
		public static TimeSpan Multiply(this TimeSpan multiplicand, int multiplier)
		{
			return TimeSpan.FromTicks(multiplicand.Ticks * multiplier);
		}

		/// <summary>
		/// Multiplies a timespan by a double value
		/// </summary>
		public static TimeSpan Multiply(this TimeSpan multiplicand, double multiplier)
		{
			return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
		}
	}
	public static class StringExtension
	{
		public static string UppercaseFirst(this string s)
		{
			// Check for empty string.
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			// Return char and concat substring.
			return char.ToUpper(s[0]) + s.Substring(1);
		}
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
	}
}
