using MiDeRP;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Linq;
using System.ComponentModel;

namespace Nobel
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		BackgroundWorker updater = new BackgroundWorker();
		const int TimeoutBeforeStart = 4;
		MySqlConnection connection;
		BeamerWindow bw = new BeamerWindow();
		String comPort = "";
		SerialInterface Serial;
		Stopwatch sA, sB;
		String teamA, teamB;
		Boolean running = false;
		Boolean runningPM = false;
		Dictionary<String, long> points = new Dictionary<string, long>();
		List<Timeslot> timeslots = new List<Timeslot>();
		int prematch = TimeoutBeforeStart;
		List<String> teams = new List<String>();
		FileInfo timeslotsPath = new FileInfo("timeslots.csv");
		FileInfo teamsPath = new FileInfo("teams.csv");
		FileInfo timesPath = new FileInfo("times.csv");
		Timer timer = new Timer(50);
		Timer timerPrematch = new Timer(1000);
		Timer timerDatabase = new Timer(30000);
		Action<String, long, bool, String, long, bool> updateSW;
		Action<int> updatePM;
		Action startAdt;
		Action updateDB;
		DateTime ecoStart = new DateTime(2014, 2, 19, 21, 00, 00);
		TimeSpan timeslotLen = new TimeSpan(0, 20, 0);
		object _updaterLock = new object();
		int totalQueries = 0;
		String getQuery = @"SELECT 
    SUM(bestelling.Bestelling_AantalS) as Totaal_Aantal, debiteur.Debiteur_Naam, prijs.Prijs_Naam
FROM
    bestelling
        LEFT JOIN
    (bon
    LEFT JOIN (debiteur) ON (Bon_Debiteur = Debiteur_ID)) ON (Bestelling_Bon = Bon_ID)
  LEFT JOIN
    (prijs) ON (Bestelling_Wat = Prijs_ID)
WHERE Prijs_Naam LIKE ?product AND Debiteur_Naam IS NOT NULL AND Bestelling_Time>=?timestart AND Bestelling_Time<=?timeend GROUP BY Debiteur_Naam ORDER BY Totaal_Aantal DESC";
		MySqlCommand getCmd;
		public MainWindow()
		{
			InitializeComponent();
			bw.Show();
			sA = new Stopwatch();
			sB = new Stopwatch();
			using (StreamReader r = teamsPath.OpenText())
			{
				teams.Clear();
				while (!r.EndOfStream)
				{
					teams.Add(r.ReadLine());
				}
			}
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
			foreach (String s in teams)
			{
				teamAComboBox.Items.Add(s);
				teamBComboBox.Items.Add(s);
			}
			updateSW = bw.updateStopwatch;
			updatePM = bw.updatePrematch;
			startAdt = startTimers;
			updateDB = updateDatabase;
			timer.Elapsed += timer_Elapsed;
			timerPrematch.Elapsed += timerPrematch_Elapsed;
			timerDatabase.Elapsed += timerDatabase_Elapsed;
			updater.DoWork += updater_DoWork;
			updater.ProgressChanged += updater_ProgressChanged;
			updater.RunWorkerCompleted += updater_RunWorkerCompleted;
			updater.WorkerReportsProgress = true;

		}

		void updater_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			lock (_updaterLock)
			{
				bw.updateScroller(ref points);
			}
			bw.updateTimeslots(ref timeslots, ecoStart, timeslotLen);
			
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
				int tsn = 1;
                points.Clear();			
				foreach (Timeslot ts in timeslots)
				{
					DateTime start = ecoStart.Add(timeslotLen.Multiply(tsn));
					DateTime end = ecoStart.Add(timeslotLen.Multiply(tsn + 1));
					if (start > DateTime.Now)
					{
						continue;
					}
					foreach (KeyValuePair<String, int> kvp in ts.Items)
					{
						updater.ReportProgress((int)Math.Round(current / (double)totalQueries * 100.0));
						getCmd.Parameters.Clear();
						getCmd.Parameters.AddWithValue("product", kvp.Key);
						getCmd.Parameters.AddWithValue("timestart", start.ToUnixTimestamp() * 1000);
						getCmd.Parameters.AddWithValue("timeend", end.ToUnixTimestamp() * 1000);
						MySqlDataReader dataReader = getCmd.ExecuteReader();
						while (dataReader.Read())
						{
							String deb = dataReader["Debiteur_Naam"].ToString();
							String item = dataReader["Prijs_Naam"].ToString();
							int aantal = Int32.Parse(dataReader["Totaal_Aantal"].ToString());
							if (points.ContainsKey(deb))
							{
								points[deb] += kvp.Value * aantal;
							}
							else
							{
								points.Add(deb, kvp.Value * aantal);
							}
						}
						dataReader.Close();
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

		void timerPrematch_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (runningPM)
			{
				prematch--;
				g.Dispatcher.BeginInvoke(DispatcherPriority.Send, updatePM, prematch);
				if (prematch == 0)
				{
					g.Dispatcher.BeginInvoke(DispatcherPriority.Send, startAdt);
				}
			}
		}

		void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (running)
			{
				g.Dispatcher.BeginInvoke(DispatcherPriority.Send, updateSW, teamA, sA.ElapsedMilliseconds, sA.IsRunning, teamB, sB.ElapsedMilliseconds, sB.IsRunning);
			}

		}

		private void connectButton_Click(object sender, RoutedEventArgs e)
		{

			if (comPortComboBox.SelectedItem != null)
			{
				comPort = ((ComboBoxItem)comPortComboBox.SelectedItem).Content.ToString();
				if (comPort == String.Empty)
				{
					MessageBox.Show("Geen comport geselecteerd.");
					return;
				}
				Serial = new SerialInterface(comPort, 115200);
				Serial.OpenPort();
				if (Serial.IsOpen)
				{
					Serial.SerialDataEvent += Serial_SerialDataEvent;
					controlsFysiologieStackPanel.IsEnabled = true;
					connectButton.IsEnabled = false;
					comPortComboBox.IsEnabled = false;
				}
				else
				{
					controlsFysiologieStackPanel.IsEnabled = false;
					connectButton.IsEnabled = true;
					comPortComboBox.IsEnabled = true;
				}
			}
		}

		void Serial_SerialDataEvent(object sender, SerialDataEventArgs e)
		{
			handleSerial(e.DataByte);
		}

		private void comPortComboBox_DropDownOpened(object sender, EventArgs e)
		{
			string[] ports = SerialPort.GetPortNames();
			comPortComboBox.Items.Clear();
			foreach (string s in ports)
			{
				ComboBoxItem extra = new ComboBoxItem();
				extra.Content = s;
				if (comPort == s)
				{
					extra.IsSelected = true;
				}
				comPortComboBox.Items.Add(extra);
			}
		}

		private void startTimeButton_Click(object sender, RoutedEventArgs e)
		{
			teamA = teamAComboBox.SelectedItem.ToString();
			teamB = teamBComboBox.SelectedItem.ToString();
			if (teamA != String.Empty && teamB != String.Empty && teamA != teamB)
			{
				runningPM = true;
				prematch = TimeoutBeforeStart;
				timerPrematch.Start();
				startTimeButton.IsEnabled = false;
				cancelButton.IsEnabled = false;
			}
			else
			{
				MessageBox.Show("Check selected teams.");
			}
		}
		public void handleSerial(Byte b)
		{
			if (g.Dispatcher.CheckAccess())
			{
				if (running)
				{
					if ((char)b == 'A')
					{
						sA.Stop();
					}
					else if ((char)b == 'B')
					{
						sB.Stop();
					}
					if (!sA.IsRunning && !sB.IsRunning)
					{
						running = false;
						timer.Stop();
						bw.showTimes(teamA, sA.ElapsedMilliseconds, teamB, sB.ElapsedMilliseconds);
						writeTimes(teamA, sA.ElapsedMilliseconds, teamB, sB.ElapsedMilliseconds);
						startTimeButton.IsEnabled = true;
						cancelButton.IsEnabled = false;
					}
				}
			}
			else
			{
				Action<Byte> func = handleSerial;
				g.Dispatcher.Invoke(DispatcherPriority.Normal, func, b);
			}
		}
		void writeTimes(String teamA, long msA, String teamB, long msB)
		{
			using (StreamWriter w = timesPath.AppendText())
			{
				w.WriteLine("\"{0}\";{1};\"{2}\";{3}", teamA, msA, teamB, msB);
			}
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

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			running = false;
			timer.Stop();
			Serial.SendByte((Byte)'r');
			startTimeButton.IsEnabled = true;
			cancelButton.IsEnabled = false;
			bw.clearTimes();
		}
		private void startTimers()
		{
			runningPM = false;
			sA.Reset();
			sB.Reset();
			sA.Start();
			sB.Start();
			timer.Start();
			Serial.SendByte((Byte)'r');
			Serial.SendByte((Byte)'s');
			runningPM = false;
			running = true;
			cancelButton.IsEnabled = true;
			timerPrematch.Stop();
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
					updateDatabase();
					//getCmd.Parameters.Add(new MySqlParameter(
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		void updateDatabase()
		{
			/*int tsn = 1;
			foreach (Timeslot ts in timeslots)
			{
				DateTime start = ecoStart.Add(timeslotLen.Multiply(tsn));
				DateTime end = ecoStart.Add(timeslotLen.Multiply(tsn + 1));
				if (start > DateTime.Now)
				{
					continue;
				}
				foreach (KeyValuePair<String, int> kvp in ts.Items)
				{
					getCmd.Parameters.Clear();
					getCmd.Parameters.AddWithValue("product", kvp.Key);
					getCmd.Parameters.AddWithValue("timestart", start.ToUnixTimestamp() * 1000);
					getCmd.Parameters.AddWithValue("timeend", end.ToUnixTimestamp() * 1000);
					MySqlDataReader dataReader = getCmd.ExecuteReader();
					while (dataReader.Read())
					{
						String deb = dataReader["Debiteur_Naam"].ToString();
						String item = dataReader["Prijs_Naam"].ToString();
						int aantal = Int32.Parse(dataReader["Totaal_Aantal"].ToString());
						if (points.ContainsKey(deb))
						{
							points[deb] += kvp.Value * aantal;
						}
						else
						{
							points.Add(deb, kvp.Value * aantal);
						}
					}
					dataReader.Close();
				}
				tsn++;
			}
			var pointsSorted = from entry in points orderby entry.Value descending select entry;
			points = pointsSorted.ToDictionary(pair => pair.Key, pair => pair.Value);
			bw.updateScroller(ref points);
			bw.updateTimeslots(ref timeslots, ecoStart, timeslotLen);*/
			if (!updater.IsBusy)
			{
				updater.RunWorkerAsync();
			}
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
			var duration = d - new DateTime(1970, 1, 1, 0, 0, 0);

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
	}
}
