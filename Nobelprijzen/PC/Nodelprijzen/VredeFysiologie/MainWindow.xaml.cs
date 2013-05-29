using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MiDeRP;
using System.IO.Ports;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using System.Timers;

namespace VredeFysiologie
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		BeamerWindow bw = new BeamerWindow();
		String comPort = "";
		SerialInterface Serial;
		Stopwatch sA, sB;
		String teamA, teamB;
		Boolean fighting = false;
		Boolean running = false;
		List<String> teams = new List<String>();
		FileInfo teamsPath = new FileInfo("teams.csv");
		FileInfo timesPath = new FileInfo("times.csv");
		FileInfo resultsPath = new FileInfo("results.csv");
		Timer timer = new Timer(50);
		Action<String, long, bool, String, long, bool> updateSW;
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
			foreach (String s in teams)
			{
				teamAComboBox.Items.Add(s);
				teamBComboBox.Items.Add(s);
			}
			updateSW = bw.updateStopwatch;
			timer.Elapsed += timer_Elapsed;
			
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
				sA.Reset();
				sB.Reset();
				sA.Start();
				sB.Start();
				timer.Start();
				Serial.SendByte((Byte)'r');
				Serial.SendByte((Byte)'s');
				running = true;
				startTimeButton.IsEnabled = false;
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
					if ((char)b== 'A')
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
				w.WriteLine("\"{0}\";{1};\"{2}\";{3}",teamA,msA,teamB,msB);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			bw._allowclosing = true;
			bw.Close();
		}

		private void fightButton_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}		
	}
}
