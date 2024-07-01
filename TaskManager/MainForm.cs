using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace TaskManager
{
	public partial class MainForm : Form
	{
		readonly int ramFactor = 1024;
		readonly string suffix = "kB";
		Dictionary<int, Process> d_processes;
		CommandLine cmd;
		public MainForm()
		{
			InitializeComponent();
			cmd = new CommandLine();

			SetColumns();
			statusStrip1.Items.Add("");
			LoadProcesses();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			//LoadProcesses();
			AddNewProcesses();
			RemoveOldProcesses();
			UpdateExistingProcesses();
			statusStrip1.Items[0].Text = ($"количество  процессов: {listViewProcesses.Items.Count}");
		}
		void SetColumns()
		{
			listViewProcesses.Columns.Add("PID");
			listViewProcesses.Columns.Add("Name");
			listViewProcesses.Columns.Add("Working set");
			listViewProcesses.Columns.Add("Peak working set");
		}
		void LoadProcesses()
		{
			//listViewProcesses.Items.Clear();
			//Process[] processes = Process.GetProcesses();
			//for (int i = 0; i < processes.Length; i++) 
			//{
			//    ListViewItem item = new ListViewItem();
			//    item.Text = processes[i].Id.ToString();
			//    item.SubItems.Add(processes[i].ProcessName);
			//    listViewProcesses.Items.Add(item);
			//}
			d_processes = Process.GetProcesses().ToDictionary(item => item.Id, item => item);
			foreach (KeyValuePair<int, Process> i in d_processes)
			{
				AddProcessToListView(i.Value);
			}
			//statusStrip1.Items[0].Text = ($"Количество процессов: {listViewProcesses.Items.Count}");
		}
		void AddNewProcesses()
		{
			Dictionary<int, Process> d_processes = Process.GetProcesses().ToDictionary(item => item.Id, item => item);
			foreach (KeyValuePair<int, Process> i in d_processes)
			{
				if (!this.d_processes.ContainsKey(i.Key))
				{
					//this.d_processes.Add(i.Key, i.Value);
					AddProcessToListView(i.Value);
				}
			}
			//statusStrip1.Items[0].Text = ($"Количество процессов: {listViewProcesses.Items.Count}");
		}
		void RemoveOldProcesses()
		{
			this.d_processes = Process.GetProcesses().ToDictionary(item => item.Id, item => item);
			for (int i = 0; i < listViewProcesses.Items.Count; i++)
			{
				//string item_name = listViewProcesses.Items[i].Name;
				if (!d_processes.ContainsKey(Convert.ToInt32(listViewProcesses.Items[i].Text)))
					listViewProcesses.Items.RemoveAt(i);
			}
		}
		void UpdateExistingProcesses()
		{
			for(int i=0; i<listViewProcesses.Items.Count; i++)
			{
				int id = Convert.ToInt32(listViewProcesses.Items[i].Text);
				//Process process = d_processes[id];
				listViewProcesses.Items[i].SubItems[2].Text = $"{d_processes[id].WorkingSet64 / ramFactor} {suffix}";
				listViewProcesses.Items[i].SubItems[3].Text = $"{d_processes[id].PeakWorkingSet64 / ramFactor} {suffix}";
			}
		}
		void AddProcessToListView(Process process)
		{
			
			ListViewItem item = new ListViewItem();
			item.Text = process.Id.ToString();
			item.SubItems.Add(process.ProcessName);
			item.SubItems.Add($"{process.WorkingSet64/ramFactor} {suffix}");
			item.SubItems.Add($"{process.PeakWorkingSet64/ramFactor} {suffix}");
			listViewProcesses.Items.Add(item);
		}
		void RemoveProcessFromListView(int pid)
		{
			listViewProcesses.Items.RemoveByKey(pid.ToString());
		}

		private void runToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//CommandLine cmd = new CommandLine();
			cmd.ShowDialog();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			StreamWriter sw = new StreamWriter("ProgramList.txt");

			for (int i = 0; i < cmd.ComboBoxFileName.Items.Count; i++)
			{
				sw.WriteLine(cmd.ComboBoxFileName.Items[i]);
			}

			sw.Close();
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//MessageBox.Show(this, listViewProcesses.SelectedItems[0].Text, "Selected PID", MessageBoxButtons.OK);
			d_processes[Convert.ToInt32(listViewProcesses.SelectedItems[0].Text)].Kill();
		}

		private void listViewProcesses_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			//listViewProcesses.ListViewItemSorter = new Comparer(e.Column);
			listViewProcesses.ListViewItemSorter = GetListViewSorter(e.Column);
			listViewProcesses.Sort();
			//https://stackoverflow.com/questions/1548312/sorting-a-listview-by-column
		}
		Comparer GetListViewSorter(int index)
		{
			Comparer comparer = (Comparer)listViewProcesses.ListViewItemSorter;
			if (comparer == null) comparer = new Comparer();

			comparer.Index = index;
			string columnName = listViewProcesses.Columns[index].Text;
			switch (columnName)
			{
				case "PID":
					comparer.Type = Comparer.ValueType.Integer;
					break;
				case "Name":
					comparer.Type = Comparer.ValueType.String;
					break;
				case "Working set":
				case "Peak working set":
					comparer.Type = Comparer.ValueType.Memory;
					break;
			}

			comparer.Direction = comparer.Direction == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
			return comparer;
		}
	}
}