﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;

namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private int numPhils;
        private Fork[] forks;
        private Philosopher[] philosophers;

        private void btn_Start_Click(object sender, EventArgs e)
        {
            switch (btn_Start.Text)
            {
                case "Start":
                    lv_info.Items.Clear();
                    btn_Start.Text = "Stop";

                    numPhils = (int)nud_numPhils.Value;
                    forks = new Fork[numPhils];
                    philosophers = new Philosopher[numPhils];

                    //create forks
                    for (int i = 0; i < numPhils; i++)
                        forks[i] = new Fork(i);


                    //create philosophers and start dining
                    for (int i = 0; i < numPhils; i++)
                    {
                        int rightforkID = i - 1;
                        if (i == 0)
                            rightforkID = numPhils - 1;

                        philosophers[i] = new Philosopher(i, forks[i], forks[rightforkID], ref lv_info);

                        ListViewItem lvi = new ListViewItem(i.ToString());
                        lvi.SubItems.Add("Waiting");
                        lvi.SubItems.Add("");
                        lvi.SubItems.Add("0");
                        lv_info.Items.Add(lvi);

                        philosophers[i].start((double)nud_Delay.Value);

                    }

                    break;

                case "Stop":

                    btn_Start.Text = "Start";

                    for (int i = 0; i < numPhils; i++)
                        philosophers[i].stop(false);

                    break;

            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < numPhils; i++)
                philosophers[i].stop(true);
        }

    }

    class Fork
    {
        public int forkID;
        public Philosopher holder = null;

        public Fork(int ID)
        {
            forkID = ID;
        }

        public void RequestFork(Philosopher phil)
        {
            if (holder == null || (holder.philID > phil.philID && holder.eating == false))
            {
                holder = phil;
            }
            else
            {
                while (holder != null)
                    Thread.Sleep(0);
            }

            holder = phil;
        }

        public void CleanFork()
        {
            holder = null;
        }
    }

    delegate void UpdateLV_D(String s, int Row, int SubItem, ref ListView LV);

    class Philosopher
    {
        //for UI purposes
        private ListView LV;
        UpdateLV_D updateLV = UpdateListView;

        private int pMeals = 0;
        int numMeals
        {
            get { return pMeals; }
            set
            {
                pMeals = value;
                UpdateListView(value.ToString(), philID, 3, ref LV);
            }
        }

        private String pStatus = "";
        String Status
        {
            get { return pStatus; }
            set
            {
                pStatus = value;
                UpdateListView(value.ToString(), philID, 1, ref LV);
            }
        }

        private String pForkStatus = "";
        String ForkStatus
        {
            get { return pForkStatus; }
            set
            {
                pForkStatus = value;
                UpdateListView(value.ToString(), philID, 2, ref LV);
            }
        }

        static void UpdateListView(String s, int Row, int SubItem, ref ListView LV)
        {
            if (LV.InvokeRequired)
            {
                UpdateLV_D d = new UpdateLV_D(UpdateListView);
                LV.Invoke(d, new object[] { s, Row, SubItem, LV });
            }
            else
            {
                LV.Items[Row].SubItems[SubItem].Text = s;
            }
        }

        //
        public bool eating;
        public Thread pThread;
        public int philID;
        private Fork leftFork;
        private Fork rightFork;
        private double pDelay = 1.0;
        private bool dining = false;

        public Philosopher(int ID, Fork lFork, Fork rFork, ref ListView infoLV)
        {
            philID = ID;
            leftFork = lFork;
            rightFork = rFork;

            pThread = new Thread(new ThreadStart(Dine));

            LV = infoLV;
        }

        public void start(double delay)
        {
            pDelay = delay;
            dining = true;
            pThread.Start();
        }

        public void stop(bool force)
        {
            if (force)
            {
                pThread.Interrupt();
            }
            else
                dining = false;
        }

        private void Dine()
        {
            try
            {
                while (dining)
                {

                    Status = "Waiting";
                    {
                        Thread.Sleep(new Random(DateTime.Now.Millisecond).Next(0, (int)(500 * pDelay)));
                        if (leftFork.holder != this)
                        {
                            ForkStatus = "R: " + leftFork.forkID;
                            leftFork.RequestFork(this);
                        }
                        ForkStatus = "H: " + leftFork.forkID;

                        Thread.Sleep(new Random(DateTime.Now.Millisecond).Next(0, (int)(500 * pDelay)));
                        if (rightFork.holder != this)
                        {
                            ForkStatus = "R: " + rightFork.forkID + " " + ForkStatus;
                            rightFork.RequestFork(this);
                        }
                        ForkStatus = "H: " + leftFork.forkID + ", " + rightFork.forkID;
                    }

                    Status = "Eating";
                    {
                        eating = true;
                        Thread.Sleep(new Random(DateTime.Now.Millisecond).Next(0, (int)(500 * pDelay)));
                        numMeals += 1;
                        eating = false;
                    }

                    Status = "Cleaning";
                    {
                        leftFork.CleanFork();
                        Thread.Sleep(new Random(DateTime.Now.Millisecond).Next(0, (int)(500 * pDelay)));
                        rightFork.CleanFork();
                        Thread.Sleep(new Random(DateTime.Now.Millisecond).Next(0, (int)(500 * pDelay)));
                        ForkStatus = "";
                    }

                    Status = "Thinking";
                    {
                        Thread.Sleep(new Random(DateTime.Now.Millisecond).Next(0, (int)(500 * (pDelay + 1))));
                    }
                }
            }
            catch { }
        }

    }
}
