using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace IGoEnchi
{
	public class GamesListView: FormView
	{
		private DataGrid dataGrid;
		private DataTable dataTable;
		private DataView dataView;
		private IGSServerInfo serverInfo;
		
		private ToolBarButton refreshButton;
		private ToolBarButton observeButton;
		
		public event Action<int> ObserveRequested;
		
		public GamesListView(Size size,
		                     IGSServerInfo serverInfo,
		                     IGSObserver gameObserver): base(size)
		{
			if ((serverInfo == null) || (gameObserver == null))
			{
				throw new Exception("Argument canot be null");
			}
			
			dataTable = new DataTable();
			dataTable.BeginInit();
			dataTable.Columns.AddRange(new DataColumn[]
			                           {
			                           	new DataColumn("#", Type.GetType("System.Int32")),
			                           	new DataColumn("White"),
			                           	new DataColumn("WR", Type.GetType("IGoEnchi.IGSRank")),
			                           	new DataColumn("Black"),
			                           	new DataColumn("BR", Type.GetType("IGoEnchi.IGSRank")),
			                           	new DataColumn("Moves", Type.GetType("System.Int32")),
			                           	new DataColumn("Size", Type.GetType("System.Int32")),
			                           	new DataColumn("Handi", Type.GetType("System.Int32")),
			                           	new DataColumn("Komi", Type.GetType("System.Single")),
			                           	new DataColumn("Byo", Type.GetType("System.Int32")),
			                           	new DataColumn("Type"),
			                           	new DataColumn("Obs", Type.GetType("System.Int32"))
			                           });
			dataTable.EndInit();
			
			var sort = ConfigManager.Settings.SortModes.Games;
			dataView = new DataView(dataTable)
			{
				AllowEdit = false,
				AllowDelete = false,
				AllowNew = false,
				Sort =
					dataTable.Columns.Contains(SortModes.ActualName(sort)) ?
					sort : ""
			};
			
			dataGrid = new DataGrid()
			{
				Dock = DockStyle.Fill,
				RowHeadersVisible = false,
				DataSource = dataView,
			};
			dataGrid.MouseDown += new MouseEventHandler(DataGridClick);
			
			this.serverInfo = serverInfo;
			
			serverInfo.GameListUpdated += new EventHandler(GamesListUpdated);
			
			Controls = new List<Control>() {dataGrid};
			RefreshGamesList();
		}
		
		protected override void OnInit()
		{
			refreshButton = new ToolBarButton()
			{
				ToolTipText = "Refresh list"
			};
			observeButton = new ToolBarButton()
			{
				ToolTipText = "Observe game",
				Enabled = false
			};
			
			Tools = new Tool[]
			{
				new Tool(refreshButton,
				         ConfigManager.GetIcon("refresh"),
				         OnRefreshButtonClick),
				new Tool(observeButton,
				         ConfigManager.GetIcon("observe"),
				         OnObserveButtonClick)
			};
		}
		
		private void DataGridClick(object sender, MouseEventArgs args)
		{
			var hitTestInfo = dataGrid.HitTest(args.X, args.Y);
			if (hitTestInfo.Type == DataGrid.HitTestType.ColumnHeader)
			{
				if (dataView.Sort != dataTable.Columns[hitTestInfo.Column].ColumnName)
				{
					dataView.Sort = dataTable.Columns[hitTestInfo.Column].ColumnName;
				}
				else
				{
					dataView.Sort = dataTable.Columns[hitTestInfo.Column].ColumnName + " DESC";
				}
				ConfigManager.Settings.SortModes.Games = dataView.Sort;
			}
			
			observeButton.Enabled = SelectedGameIndex != 0;
		}
		
		private int SelectedGameIndex
		{
			get
			{
				try
				{
					var cell = dataGrid.CurrentCell;
					var rowView = dataView[cell.RowNumber];
					var gameIndex = Convert.ToString(rowView.Row[dataTable.Columns.IndexOf("#")]);
					return Convert.ToInt32(gameIndex);
				}
				catch (Exception)
				{
					return 0;
				}
			}
		}
		
		private void OnObserveButtonClick(object sender, EventArgs args)
		{
			if (SelectedGameIndex != 0)
			{
				OnObserveRequested(SelectedGameIndex);
			}
		}
		
		private void OnRefreshButtonClick(object sender, EventArgs args)
		{
			RefreshGamesList();
		}
		
		private void GamesListUpdated(object sender, EventArgs args)
		{
			dataGrid.Invoke(new EventHandler(LoadGames));
		}

		private void LoadGames(object sender, EventArgs args)
		{
			dataGrid.Hide();
			dataTable.BeginLoadData();
			dataTable.Rows.Clear();
			DataRow row;
			if (serverInfo.Games != null)
			{
				for (var i = 0; i < serverInfo.Games.Length; i++)
				{
					row = dataTable.NewRow();
					row["#"] = serverInfo.Games[i].GameNumber;
					row["White"] = serverInfo.Games[i].WhitePlayer;
					row["WR"] = serverInfo.Games[i].WhiteRank;
					row["Black"] = serverInfo.Games[i].BlackPlayer;
					row["BR"] = serverInfo.Games[i].BlackRank;
					row["Moves"] = serverInfo.Games[i].MovesMade;
					row["Size"] = serverInfo.Games[i].BoardSize;
					row["Handi"] = serverInfo.Games[i].Handicap;
					row["Komi"] = serverInfo.Games[i].Komi;
					row["Byo"] = serverInfo.Games[i].Byouyomi;
					row["Type"] = serverInfo.Games[i].GameType;
					row["Obs"] = serverInfo.Games[i].ObserversCount;
					dataTable.Rows.Add(row);
				}
			}
			dataTable.EndLoadData();
			dataGrid.Show();
			refreshButton.Enabled = true;
			observeButton.Enabled = SelectedGameIndex != 0;
		}
		
		public void RefreshGamesList()
		{
			if (refreshButton.Enabled)
			{
				if (serverInfo.RequestGamesList())
				{
					refreshButton.Enabled = false;
				}
			}
		}
		
		protected void OnObserveRequested(int gameNumber)
		{
			if (ObserveRequested != null)
			{
				ObserveRequested(gameNumber);
			}
		}
	}
	
	public class PlayersListView: FormView
	{
		private DataGrid dataGrid;
		private DataTable dataTable;
		private DataView dataView;
		private IGSServerInfo serverInfo;
		
		private ToolBarButton refreshButton;
		private ToolBarButton chatButton;
		private ToolBarButton matchButton;
		
		public event Action<string> ChatRequested;
		public event Action<string> MatchRequested;
		
		public PlayersListView(Size size,
		                       IGSServerInfo serverInfo): base(size)
		{
			if ((serverInfo == null))
			{
				throw new Exception("Argument canot be null");
			}
			
			dataTable = new DataTable();
			dataTable.BeginInit();
			dataTable.Columns.AddRange(new DataColumn[]
			                           {
			                           	new DataColumn("Flags"),
			                           	new DataColumn("Name"),
			                           	new DataColumn("Rank", Type.GetType("IGoEnchi.IGSRank")),
			                           	new DataColumn("Won", Type.GetType("System.Int32")),
			                           	new DataColumn("Lost", Type.GetType("System.Int32")),
			                           	new DataColumn("Playing", Type.GetType("System.Int32")),
			                           	new DataColumn("Observing", Type.GetType("System.Int32")),
			                           	new DataColumn("Idle"),
			                           	new DataColumn("Country"),
			                           	new DataColumn("Info")
			                           });
			dataTable.EndInit();
			
			var sort = ConfigManager.Settings.SortModes.Players;
			dataView = new DataView(dataTable)
			{
				AllowEdit = false,
				AllowDelete = false,
				AllowNew = false,
				Sort =
					dataTable.Columns.Contains(SortModes.ActualName(sort)) ?
					sort : ""
			};
			
			dataGrid = new DataGrid()
			{
				Dock = DockStyle.Fill,
				RowHeadersVisible = false,
				DataSource = dataView
			};
			
			dataGrid.MouseDown += new MouseEventHandler(DataGridClick);
			
			this.serverInfo = serverInfo;
			serverInfo.PlayersListUpdated += new EventHandler(PlayersListUpdated);
			
			Controls = new List<Control>() {dataGrid};
			RefreshPlayersList();
		}
		
		protected override void OnInit()
		{
			refreshButton = new ToolBarButton()
			{
				ToolTipText = "Refresh list"
			};
			chatButton = new ToolBarButton()
			{
				ToolTipText = "Chat",
				Enabled = false
			};
			matchButton = new ToolBarButton()
			{
				ToolTipText = "Match",
				Enabled = false
			};
			
			Tools = new Tool[]
			{
				new Tool(refreshButton,
				         ConfigManager.GetIcon("refresh"),
				         OnRefreshButtonClick),
				new Tool(chatButton,
				         ConfigManager.GetIcon("talk"),
				         OnChatButtonClick),
				new Tool(matchButton,
				         ConfigManager.GetIcon("match"),
				         OnMatchButtonClick)
			};
		}
		
		private string SelectedPlayer
		{
			get
			{
				try
				{
					var cell = dataGrid.CurrentCell;
					var rowView = dataView[cell.RowNumber];
					return Convert.ToString(rowView.Row[dataTable.Columns.IndexOf("Name")]);				
				}
				catch (IndexOutOfRangeException)
				{
					return String.Empty;
				}
			}
		}
						
		private void DataGridClick(object sender, MouseEventArgs args)
		{
			DataGrid.HitTestInfo hitTestInfo = dataGrid.HitTest(args.X, args.Y);
			if (hitTestInfo.Type == DataGrid.HitTestType.ColumnHeader)
			{
				if (dataView.Sort != dataTable.Columns[hitTestInfo.Column].ColumnName)
				{
					dataView.Sort =  dataTable.Columns[hitTestInfo.Column].ColumnName;
				}
				else
				{
					dataView.Sort =  dataTable.Columns[hitTestInfo.Column].ColumnName + " DESC";
				}
				ConfigManager.Settings.SortModes.Players = dataView.Sort;
			}
			
			chatButton.Enabled = SelectedPlayer != String.Empty;
			matchButton.Enabled = SelectedPlayer != String.Empty;
		}
		
		private void OnRefreshButtonClick(object sender, EventArgs args)
		{
			RefreshPlayersList();
		}
		
		private void OnChatButtonClick(object sender, EventArgs args)
		{
			if (SelectedPlayer != String.Empty)
			{
				OnChatRequested(SelectedPlayer);
			}
		}
		
		private void OnMatchButtonClick(object sender, EventArgs args)
		{
			if (SelectedPlayer != String.Empty)
			{
				OnMatchRequested(SelectedPlayer);
			}
		}
		
		protected void OnChatRequested(string player)
		{
			if (ChatRequested != null)
			{
				ChatRequested(player);
			}
		}
		
		protected void OnMatchRequested(string player)
		{
			if (MatchRequested != null)
			{
				MatchRequested(player);
			}
		}
		
		private void PlayersListUpdated(object sender, EventArgs args)
		{
			dataGrid.Invoke(new EventHandler(LoadPlayers));
		}

		private void LoadPlayers(object sender, EventArgs args)
		{
			dataGrid.Hide();
			dataTable.BeginLoadData();
			dataTable.Rows.Clear();
			DataRow row;
			if (serverInfo.Players != null)
			{
				for (int i = 0; i < serverInfo.Players.Length; i++)
				{
					row = dataTable.NewRow();
					row["Name"] = serverInfo.Players[i].Name;
					row["Rank"] = serverInfo.Players[i].Rank;
					row["Info"] = serverInfo.Players[i].Info;
					row["Won"] = serverInfo.Players[i].GamesWon;
					row["Lost"] = serverInfo.Players[i].GamesLost;
					row["Playing"] = serverInfo.Players[i].GamePlaying;
					row["Observing"] = serverInfo.Players[i].GameObserving;
					row["Idle"] = serverInfo.Players[i].TimeIdle;
					row["Country"] = serverInfo.Players[i].Country;
					row["Flags"] = serverInfo.Players[i].Flags;
					dataTable.Rows.Add(row);
				}
				Console.WriteLine(serverInfo.Players.Length);
			}
			dataTable.EndLoadData();
			dataGrid.Show();
			
			refreshButton.Enabled = true;
			chatButton.Enabled = SelectedPlayer != String.Empty;
			matchButton.Enabled = SelectedPlayer != String.Empty;
		}
		
		public void RefreshPlayersList()
		{
			if (refreshButton.Enabled)
			{
				if (serverInfo.RequestPlayersList())
				{
					refreshButton.Enabled = false;
				}
			}
		}
	}
}
