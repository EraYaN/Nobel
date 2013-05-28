using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Fysiologie
{
    public partial class Form1 : Form
    {
        delegate void SetTextCallback(string text);

        private const char keyStart = ' ';
        private const char keyTeam1 = '1';
        private const char keyTeam2 = '2';
        private const char keyResults = 'r';

        private Color winColor = Color.FromArgb(236, 174, 27);

        private string[] teams;

        private Stopwatch stopwatch = new Stopwatch();

        private int state = 0;  //0 = no teams selected || timer has stopped, winner has been selected
                                //1 = teams are ready to start
                                //2 = time is running

        private Thread timer;

        private int team1Index;
        private int team2Index;

        private bool go1 = true;
        private bool go2 = true;
                                
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader("teams.txt");
            teams = sr.ReadToEnd().Split('\n');
            sr.Close();

            comboBoxTeam1.Items.AddRange(teams);
            comboBoxTeam2.Items.AddRange(teams);

            nameTeam1.Visible = false;
            nameTeam2.Visible = false;
            timerTeam1.Visible = false;
            timerTeam2.Visible = false;
        }

        private void counter() 
        {
            while (go1 || go2)
            {
                string time = (stopwatch.Elapsed.Seconds + stopwatch.Elapsed.Minutes * 60) + "." + stopwatch.Elapsed.Milliseconds;
                if(go1)
                    SetText1(time);
                if(go2)
                    SetText2(time);
            }
        }

        private void SetText1(string text)
        {
            if (timerTeam1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText1);
                try
                {
                    Invoke(d, new object[] { text });
                }
                catch (Exception e) 
                {
                    if (e.ToString() == "fuck jou unused e")
                    { }
                }
            }
            else
                timerTeam1.Text = text;
        }
        private void SetText2(string text)
        {
            if (timerTeam2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText2);
                try
                {
                    Invoke(d, new object[] { text });
                }
                catch (Exception e)
                {
                    if (e.ToString() == "fuck jou unused e")
                    { }
                }
            }
            else
                timerTeam2.Text = text;
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e) 
        {
            switch (e.KeyChar)
            {
                case keyResults:
                    if (state == 0)
                    {
                        comboBoxTeam1.Visible = false;
                        comboBoxTeam2.Visible = false;
                        timesBox.Visible = true;

                        timesBox.Items.Clear();

                        StreamReader sr = new StreamReader("times.txt");
                        string[] times = sr.ReadToEnd().Split('%');
                        sr.Close();

                        for (int i = 0; i < times.Length - 1; i++)
                        {
                            string[] timeitems = times[i].Split('|');
                            ListViewItem item = new ListViewItem(timeitems);

                            if (float.Parse(timeitems[1]) > float.Parse(timeitems[3]))
                            {
                                item.SubItems[2].BackColor = winColor;
                                item.SubItems[3].BackColor = winColor;
                            }
                            else if (float.Parse(timeitems[1]) < float.Parse(timeitems[3]))
                            {
                                item.SubItems[0].BackColor = winColor;
                                item.SubItems[1].BackColor = winColor;
                            }

                            item.UseItemStyleForSubItems = false;
                            timesBox.Items.Add(item);
                        }

                        state = 10;
                    }
                    else if (state == 10)
                    {
                        comboBoxTeam1.Visible = true;
                        comboBoxTeam2.Visible = true;
                        timesBox.Visible = false;

                        state = 0;
                    }
                    break;
                case keyStart:
                    if (state == 0)
                    {
                        if (comboBoxTeam1.SelectedIndex != -1 && comboBoxTeam2.SelectedIndex != -1 && comboBoxTeam1.SelectedIndex != comboBoxTeam2.SelectedIndex)
                        {
                            nameTeam1.Text = teams[comboBoxTeam1.SelectedIndex];
                            nameTeam2.Text = teams[comboBoxTeam2.SelectedIndex];
                            team1Index = comboBoxTeam1.SelectedIndex;
                            team2Index = comboBoxTeam2.SelectedIndex;
                            timerTeam1.Text = "0.00";
                            timerTeam2.Text = "0.00";

                            comboBoxTeam1.Visible = false;
                            comboBoxTeam2.Visible = false;
                            timerTeam1.Visible = true;
                            timerTeam2.Visible = true;
                            nameTeam1.Visible = true;
                            nameTeam2.Visible = true;

                            state = 1;
                        }
                        else
                            return;
                    }
                    else if (state == 1)
                    {
                        stopwatch.Start();
                        timer = new Thread(new ThreadStart(counter));
                        timer.Start();

                        state = 2;
                    }
                    else if (state == 3)
                    {
                        go1 = true;
                        go2 = true;

                        comboBoxTeam1.Visible = true;
                        comboBoxTeam2.Visible = true;
                        timerTeam1.Visible = false;
                        timerTeam2.Visible = false;
                        nameTeam1.Visible = false;
                        nameTeam2.Visible = false;

                        timerTeam1.BackColor = SystemColors.Control;
                        nameTeam1.BackColor = SystemColors.Control;
                        panelTeam1.BackColor = SystemColors.Control;
                        timerTeam2.BackColor = SystemColors.Control;
                        nameTeam2.BackColor = SystemColors.Control;
                        panelTeam2.BackColor = SystemColors.Control;

                        state = 0;
                    }
                    break;
                case keyTeam1:
                    if(state == 2)
                        go1 = false;
                    break;
                case keyTeam2:
                    if(state == 2)
                        go2 = false;
                    break;
            }
            if (!go1 && !go2)
                selectWinner();
            e.Handled = true;
        }

        private void selectWinner()
        {
            stopwatch.Reset();
            float time1 = float.Parse(timerTeam1.Text.Replace('.', ','));
            float time2 = float.Parse(timerTeam2.Text.Replace('.', ','));

            if (time1 > time2)
            {
                //team 2 won
                timerTeam2.BackColor = winColor;
                nameTeam2.BackColor = winColor;
                panelTeam2.BackColor = winColor;
            }
            else if (time1 < time2)
            {
                //team 1 won
                timerTeam1.BackColor = winColor;
                nameTeam1.BackColor = winColor;
                panelTeam1.BackColor = winColor;
            }

            StreamReader sr = new StreamReader("times.txt");
            string times = sr.ReadToEnd();
            sr.Close();

            times += teams[team1Index];                 //[teamnaam1]
            times += "|" + time1.ToString();            //[teamnaam1]|[tijd1]
            times += "|" + teams[team2Index];           //[teamnaam1]|[tijd1]|[teamnaam2]
            times += "|" + time2.ToString() + "%";     //[teamnaam1]|[tijd1]|[teamnaam2]|[tijd2]

            StreamWriter sw = new StreamWriter("times.txt");
            sw.Write(times);
            sw.Close();

            state = 3;
        }
    }
}
