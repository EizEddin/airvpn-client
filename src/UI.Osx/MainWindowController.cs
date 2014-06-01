using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using AirVPN.Core;

namespace AirVPN.UI.Osx
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		public TableServersController TableServersController;
		public TableAreasController TableAreasController;
		public TableLogsController TableLogsController;
		public TableStatsController TableStatsController;

		public NSStatusItem StatusItem;
		public NSMenuItem StatusMenuItem;

		#region Constructors
		// Called when created from unmanaged code
		public MainWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public MainWindowController () : base ("MainWindow")
		{
			Initialize ();
		}	
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		//strongly typed window accessor
		public new MainWindow Window {
			get {
				return (MainWindow)base.Window;
			}
		}

		
		public UI.Osx.Engine Engine
		{
			get {
				return Core.Engine.Instance as UI.Osx.Engine;
			}
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib ();

			Window.Delegate = new MainWindowDelegate (this);

			CreateMenuBarIcon ();

			ChkRemember.State = Engine.Storage.GetBool("remember") ? NSCellStateValue.On : NSCellStateValue.Off; 
			ChkServersShowAll.State = NSCellStateValue.Off;
			GuiUtils.SetCheck (ChkServersLockCurrent, Engine.Storage.GetBool ("servers.locklast"));
			GuiUtils.SetSelected (CboServersScoringRule, Engine.Storage.Get ("servers.scoretype"));

			CboSpeedResolutions.RemoveAllItems ();
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution1);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution2);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution3);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution4);
			CboSpeedResolutions.AddItem (Messages.WindowsMainSpeedResolution5);
			CboSpeedResolutions.SelectItem (0);

			CmdConnect.Title = Messages.CommandConnect;
			LblConnect.StringValue = Messages.CommandConnectSubtitle;
			CmdDisconnect.Title = Messages.CommandDisconnect;
			CmdCancel.Title = Messages.CommandCancel;
			CboServersScoringRule.ToolTip = Messages.TooltipServersScoreType;
			ChkServersLockCurrent.ToolTip = Messages.TooltipServersLockCurrent;
			ChkServersShowAll.ToolTip = Messages.TooltipServersShowAll;
			CmdServersConnect.ToolTip = Messages.TooltipServersConnect;
			CmdServersUndefined.ToolTip = Messages.TooltipServersUndefined;
			CmdServersBlackList.ToolTip = Messages.TooltipServersBlackList;
			CmdServersWhiteList.ToolTip = Messages.TooltipServersWhiteList;
			CmdAreasUndefined.ToolTip = Messages.TooltipAreasUndefined;
			CmdAreasBlackList.ToolTip = Messages.TooltipAreasBlackList;
			CmdAreasWhiteList.ToolTip = Messages.TooltipAreasWhiteList;
			CmdLogsOpenVpnManagement.ToolTip = Messages.TooltipLogsOpenVpnManagement;
			CmdLogsClean.ToolTip = Messages.TooltipLogsClean;
			CmdLogsCopy.ToolTip = Messages.TooltipLogsCopy;
			CmdLogsSave.ToolTip = Messages.TooltipLogsSave;
			CmdLogsSupport.ToolTip = Messages.TooltipLogsSupport;

			if (Engine.Storage.GetBool ("remember")) {
				ChkRemember.State = NSCellStateValue.On;
				TxtLogin.StringValue = Engine.Storage.Get ("login");
				TxtPassword.StringValue = Engine.Storage.Get ("password");

			}

			ChkLockedMode.State = (Engine.Storage.GetBool ("advanced.locked_security") ? NSCellStateValue.On : NSCellStateValue.Off);



			ChkRemember.Activated += (object sender, EventArgs e) =>
			{
				Engine.Storage.SetBool ("remember", ChkRemember.State == NSCellStateValue.On);
			};

			CmdLogin.Activated += (object sender, EventArgs e) =>
			{
				if(Engine.IsLogged() == false)
					Login();
				else
					Logout();
			};	

			TxtLogin.Activated += (object sender, EventArgs e) => {
				EnabledUI();
			};

			TxtPassword.Activated += (object sender, EventArgs e) => {
				EnabledUI();
			};

			CmdConnect.Activated += (object sender, EventArgs e) =>
			{
				Connect();
			};

			CmdDisconnect.Activated += (object sender, EventArgs e) => {
				Disconnect();
			};

			CmdCancel.Activated += (object sender, EventArgs e) => {
				CmdCancel.Enabled = false;
				Disconnect ();
			};

			ChkLockedMode.Activated += (object sender, EventArgs e) => {
				CheckLockedNetwork();
			};
			TableServers.DoubleClick += (object sender, EventArgs e) => {
				ConnectManual();
			};

			CmdServersConnect.Activated += (object sender, EventArgs e) => {
				ConnectManual();
			};

			CmdServersWhiteList.Activated += (object sender, EventArgs e) => {
				ServersWhiteList();
			};

			CmdServersBlackList.Activated += (object sender, EventArgs e) => {
				ServersBlackList();
			};

			CmdServersUndefined.Activated += (object sender, EventArgs e) => {
				ServersUndefinedList ();
			};

			MnuServersConnect.Activated += (object sender, EventArgs e) => {
				ConnectManual();
			};

			MnuServersWhitelist.Activated += (object sender, EventArgs e) => {
				ServersWhiteList();
			};

			MnuServersBlacklist.Activated += (object sender, EventArgs e) => {
				ServersBlackList();
			};

			MnuServersUndefined.Activated += (object sender, EventArgs e) => {
				ServersUndefinedList ();
			};

			CmdAreasWhiteList.Activated += (object sender, EventArgs e) => {
				AreasWhiteList();
			};

			CmdAreasBlackList.Activated += (object sender, EventArgs e) => {
				AreasBlackList();
			};

			CmdAreasUndefined.Activated += (object sender, EventArgs e) => {
				AreasUndefinedList ();
			};

			MnuAreasWhitelist.Activated += (object sender, EventArgs e) => {
				AreasWhiteList();
			};

			MnuAreasBlacklist.Activated += (object sender, EventArgs e) => {
				AreasBlackList();
			};

			MnuAreasUndefined.Activated += (object sender, EventArgs e) => {
				AreasUndefinedList ();
			};

			ChkServersShowAll.Activated += (object sender, EventArgs e) => {
				TableServersController.ShowAll = (ChkServersShowAll.State == NSCellStateValue.On);
				TableServersController.RefreshUI();
			};

			/*
			 * Xamarin Bug: https://bugzilla.xamarin.com/show_bug.cgi?id=12467
			 * Resolved with delegate
			 * 
			TableServers.SelectionDidChange += (object sender, EventArgs e) => {
				EnabledUI();
			};

			TableAreas.SelectionDidChange += (object sender, EventArgs e) => {
				EnabledUI();
			};
			*/

			ChkServersLockCurrent.Activated += (object sender, EventArgs e) => {
				Engine.Storage.SetBool ("servers.locklast", ChkServersLockCurrent.State == NSCellStateValue.On);
			};

			CboServersScoringRule.Activated += (object sender, EventArgs e) => {
				Engine.Storage.Set ("servers.scoretype", GuiUtils.GetSelected(CboServersScoringRule));

				RefreshUi (AirVPN.Core.Engine.RefreshUiMode.Full);
			};

			CmdLogsClean.Activated += (object sender, EventArgs e) => {
				TableLogsController.Clear();
			};

			CmdLogsSave.Activated += (object sender, EventArgs e) => {
				LogsDoSave(false);
			};

			CmdLogsCopy.Activated += (object sender, EventArgs e) => {
				LogsDoCopy(false);
			};

			CmdLogsSupport.Activated += (object sender, EventArgs e) =>
			{
				LogsDoCopy(false);
			};		

			MnuLogsCopyAll.Activated += (object sender, EventArgs e) =>
			{
				LogsDoCopy(false);
			};		

			MnuLogsSaveAll.Activated += (object sender, EventArgs e) =>
			{
				LogsDoSave(false);
			};			
			MnuLogsCopySelected.Activated += (object sender, EventArgs e) =>
			{
				LogsDoCopy(true);
			};			
			MnuLogsSaveSelected.Activated += (object sender, EventArgs e) =>
			{
				LogsDoSave(true);
			};			

			CmdLogsOpenVpnManagement.Activated += (object sender, EventArgs e) => {
				if(Engine.IsConnected())
				{
					WindowOpenVpnManagementCommandController w = new WindowOpenVpnManagementCommandController();
					NSApplication.SharedApplication.RunModalForWindow(w.Window);
					if(w.Command != "")
						Core.UI.Actions.SendOpenVpnManagementCommand(w.Command);
				}
			};

			CboSpeedResolutions.Activated += (object sender, EventArgs e) => {
				// TODO
			};

			TableServersController = new TableServersController (this.TableServers);
			this.TableServers.Delegate = new TableServersDelegate (this);

			TableAreasController = new TableAreasController (this.TableAreas);
			this.TableAreas.Delegate = new TableAreasDelegate (this);

			TableLogsController = new TableLogsController (this.TableLogs);
			TableStatsController = new TableStatsController (this.TableStats);
		
			TableStats.DoubleClick += (object sender, EventArgs e) => {
				TableStatsController.DoubleClickItem();
			};

			Engine.MainWindow = this;

			Engine.OnRefreshUi ();
		}

		public void RefreshUi (Engine.RefreshUiMode mode)
		{
			if ((mode == Engine.RefreshUiMode.MainMessage) || (mode == Engine.RefreshUiMode.Full)) {

				if (Engine.CurrentServer != null) {
					ImgTopFlag.Image = NSImage.ImageNamed("flag_" + Engine.CurrentServer.CountryCode.ToLowerInvariant() + ".png");
				}
				else {
					ImgTopFlag.Image = NSImage.ImageNamed ("notconnected.png"); // TODO img neutra
				}

				LblWaiting1.StringValue = Engine.WaitMessage;

				if(Engine.IsWaiting())
				{
					ImgProgress.StartAnimation(this);
					ImgTopPanel.Image = NSImage.ImageNamed ("topbar_osx_yellow.png");
					LblTopStatus.StringValue = Engine.WaitMessage;

					TabOverview.SelectAt(1);

					CmdCancel.Hidden = (Engine.IsWaitingCancelAllowed() == false);
					CmdCancel.Enabled = (Engine.IsWaitingCancelPending() == false);
				}
				else if (Engine.IsConnected())
				{
					ImgProgress.StopAnimation (this);
					ImgTopPanel.Image = NSImage.ImageNamed ("topbar_osx_green.png");
					LblTopStatus.StringValue = Messages.Format(Messages.TopBarConnected, Engine.CurrentServer.PublicName);

					TabOverview.SelectAt(2);

					// TODO: varie UI
				}
				else
				{
					ImgProgress.StopAnimation (this);
					ImgTopPanel.Image = NSImage.ImageNamed ("topbar_osx_red.png");
					LblTopStatus.StringValue = Messages.TopBarNotConnectedExposed; // TODO la locked

					TabOverview.SelectAt(0);
				}

				// Icon update
				if(StatusMenuItem != null)
				{
					if(Engine.IsConnected())
					{
						StatusItem.Image = NSImage.ImageNamed("statusbar_black.png");
						//NSApplication.SharedApplication.DockTile. =  DateTime.Now.ToString ();
						NSApplication.SharedApplication.ApplicationIconImage = NSImage.ImageNamed("icon.png");
					}
					else
					{
						StatusItem.Image = NSImage.ImageNamed("statusbar_black_gray.png");
						//NSApplication.SharedApplication.DockTile.Description =  DateTime.Now.ToString ();
						NSApplication.SharedApplication.ApplicationIconImage = NSImage.ImageNamed("icon_gray.png");
					}
				}

				EnabledUI();
			}

			if ((mode == Engine.RefreshUiMode.Log) || (mode == Engine.RefreshUiMode.Full)) {

				lock (Engine.LogsPending) {
					while (Engine.LogsPending.Count > 0) {
						LogEntry l = Engine.LogsPending [0];
						Engine.LogsPending.RemoveAt (0);

						Log (l);
					}
				}
				LblWaiting2.StringValue = Engine.GetLogDetailTitle();
			}

			if ((mode == Engine.RefreshUiMode.Stats) || (mode == Engine.RefreshUiMode.Full)) {
				// TODO
			}

			if ((mode == Engine.RefreshUiMode.Full)) {
				if(TableServersController != null)
					TableServersController.RefreshUI ();
				if(TableAreasController != null)
					TableAreasController.RefreshUI ();
			}
			 



		}

		public void Log(LogEntry l)
		{
			string msg = l.Message;

			TableLogsController.AddLog (l);

			StatusMenuItem.Title = msg;
			StatusItem.ToolTip = msg;

			if ((msg != "") && (l.Type != Core.Engine.LogType.Verbose)) {
				if(Engine.IsConnected() == false)
				{
					Window.Title = Constants.Name + " - " + msg;
					StatusMenuItem.Title = "> " + msg;
				}
			}

			if (l.Type >= Engine.LogType.InfoImportant)
				Notification (msg, "");

			if (l.Type >= Engine.LogType.InfoImportant)
				RequestAttention ();

			if (l.Type == AirVPN.Core.Engine.LogType.Fatal)
				MessageAlert (msg);
		}

		public void EnabledUI()
		{
			bool logged = Engine.IsLogged ();
			bool connected = Engine.IsConnected ();
			bool waiting = Engine.IsWaiting ();

			if (logged == false)
				CmdLogin.Title = Messages.CommandLogin;
			else
				CmdLogin.Title = Messages.CommandLogout;

			CmdLogin.Enabled = ((waiting == false) && (connected == false) && (TxtLogin.StringValue.Trim () != "") && (TxtPassword.StringValue.Trim () != ""));

			TxtLogin.Enabled = (logged == false);
			TxtPassword.Enabled = (logged == false);

			if (logged) {
				CmdConnect.Enabled = true;
			} else {
				CmdConnect.Enabled = false;
			}

			CmdServersConnect.Enabled = ((logged) && (TableServers.SelectedRowCount == 1));
			CmdServersWhiteList.Enabled = (TableServers.SelectedRowCount > 0);
			CmdServersBlackList.Enabled = CmdServersWhiteList.Enabled;
			CmdServersUndefined.Enabled = CmdServersWhiteList.Enabled;
			MnuServersConnect.Enabled = CmdServersConnect.Enabled;
			MnuServersWhitelist.Enabled = CmdServersWhiteList.Enabled;
			MnuServersBlacklist.Enabled = CmdServersBlackList.Enabled;
			MnuServersUndefined.Enabled = CmdServersUndefined.Enabled;

			CmdAreasWhiteList.Enabled = (TableAreas.SelectedRowCount > 0);
			CmdAreasBlackList.Enabled = CmdAreasWhiteList.Enabled;
			CmdAreasUndefined.Enabled = CmdAreasWhiteList.Enabled;
			MnuAreasWhitelist.Enabled = CmdAreasWhiteList.Enabled;
			MnuAreasBlacklist.Enabled = CmdAreasBlackList.Enabled;
			MnuAreasUndefined.Enabled = CmdAreasUndefined.Enabled;

			CmdLogsOpenVpnManagement.Hidden = (Engine.Storage.GetBool("advanced.expert") == false);
			CmdLogsOpenVpnManagement.Enabled = connected;
		}

		public void FrontMessage(string message)
		{
			WindowFrontMessageController w = new WindowFrontMessageController ();
			(Engine.Instance as Engine).WindowsOpen.Add (w);
			w.Message = message;
			w.ShowWindow (this);
		}

		public void RequestAttention()
		{
			NSApplication.SharedApplication.RequestUserAttention (NSRequestUserAttentionType.InformationalRequest);


		}

		public void MessageAlert(string message)
		{
			MessageAlert (message, "");
		}

		public void MessageAlert(string message, string title)
		{
			NSAlert alert = new NSAlert();
			alert.MessageText = title;
			alert.InformativeText = message;
			alert.RunModal();
		}

		public void Notification(string title, string notes)
		{
			RequestAttention ();

			// First we create our notification and customize as needed
			NSUserNotification not = null;

			try {
				not = new NSUserNotification();
			} catch {
				// This API was introduced in OS X Mountain Lion (10.8)
				return;
			}

			not.Title = title;
			not.InformativeText = notes;
			not.DeliveryDate = DateTime.Now;
			not.SoundName = NSUserNotification.NSUserNotificationDefaultSoundName;

			// We get the Default notification Center
			NSUserNotificationCenter center = NSUserNotificationCenter.DefaultUserNotificationCenter;

			// If we return true here, Notification will show up even if your app is TopMost.
			center.ShouldPresentNotification = (c, n) => { return true; };

			center.ScheduleNotification(not);
		}

		public void CreateMenuBarIcon ()
		{
			NSMenu notifyMenu = new NSMenu ();
			StatusMenuItem = new NSMenuItem ("");
			notifyMenu.AddItem (StatusMenuItem);
			NSMenuItem exitMenuItem = new NSMenuItem ("Quit", (a,b) => {
				//System.Environment.Exit (0);
				Engine.RequestStop();
			});
			notifyMenu.AddItem (exitMenuItem);

			StatusItem = NSStatusBar.SystemStatusBar.CreateStatusItem (22);
			StatusItem.Menu = notifyMenu;
			StatusItem.Image = NSImage.ImageNamed ("statusbar_black_gray.png");
			StatusItem.HighlightMode = true;
		}

		void Login()
		{
			Engine.Storage.Set ("login", TxtLogin.StringValue);
			Engine.Storage.Set ("password", TxtPassword.StringValue);

			if (TermsOfServiceCheck (false) == false)
				return;

			Engine.Login ();
		}

		void Logout()
		{
			Engine.Logout ();
		}

		void Connect()
		{
			if((Engine.IsLogged() == true) && (Engine.IsConnected() == false) && (Engine.IsWaiting() == false))
			{
				TabMain.SelectAt (0);
				Engine.Connect ();
			}
		}

		void ConnectManual()
		{
			if (TableServers.SelectedRows.Count == 1) {
				ServerInfo s = TableServersController.GetRelatedItem(TableServers.SelectedRow);
				Engine.NextServer = s;
				Connect ();
			}
		}

		void Disconnect()
		{
			Engine.Disconnect ();
		}

		void CheckLockedNetwork()
		{
			// TODO
		}

		bool TermsOfServiceCheck(bool force)
		{
			bool show = force;
			if(show == false)
				show = (Engine.Storage.GetBool ("gui.tos") == false);

			if (show) {
				WindowTosController tos = new WindowTosController ();
				tos.PazzoTest = "gamma";
				tos.Window.ReleasedWhenClosed = true;
				NSApplication.SharedApplication.RunModalForWindow (tos.Window);
				tos.Window.Close ();

				if (tos.Accepted) {
					Engine.Storage.SetBool ("gui.tos", true);
					return true;
				} else {
					return false;
				}
			} else {
				return true;
			}
		}


		void ServersWhiteList()
		{
			foreach(int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ServerInfo.UserListType.WhiteList;
			}
			TableServersController.RefreshUI ();
		}

		void ServersBlackList()
		{
			foreach(int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ServerInfo.UserListType.BlackList;
			}
			TableServersController.RefreshUI ();
		}

		void ServersUndefinedList()
		{
			foreach(int i in TableServers.SelectedRows)
			{
				TableServersController.GetRelatedItem(i).UserList = ServerInfo.UserListType.None;
			}
			TableServersController.RefreshUI ();
		}

		void AreasWhiteList()
		{
			foreach(int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.WhiteList;
			}
			TableServersController.RefreshUI ();
		}

		void AreasBlackList()
		{
			foreach(int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.BlackList;
			}
			TableAreasController.RefreshUI ();
		}

		void AreasUndefinedList()
		{
			foreach(int i in TableAreas.SelectedRows)
			{
				TableAreasController.GetRelatedItem(i).UserList = AreaInfo.UserListType.None;
			}
			TableAreasController.RefreshUI ();
		}

		void LogsDoCopy(bool selectedOnly)
		{
			string t = TableLogsController.GetBody (selectedOnly);
			if (t != "") {
				string [] pboardTypes = new string[] { "NSStringPboardType" };
				NSPasteboard.GeneralPasteboard.DeclareTypes (pboardTypes, null);
				NSPasteboard.GeneralPasteboard.SetStringForType (t, pboardTypes [0]);
				MessageAlert (Messages.LogsCopyClipboardDone);
			}
		}

		void LogsDoSave(bool selectedOnly)
		{
			string t = TableLogsController.GetBody (selectedOnly);
			if (t.Trim () != "") {
				string filename = "AirVPN_" + DateTime.Now.ToString ("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".txt"; // TOCLEAN
				//string filename = Engine.GetLogSuggestedFileName();

				NSSavePanel panel = new NSSavePanel ();
				panel.NameFieldStringValue = filename;
				panel.CanCreateDirectories = true;
				int result = panel.RunModal ();
				if (result == 1) {
					System.IO.File.WriteAllText (panel.Url.Path, t);
			
					MessageAlert (Messages.LogsSaveToFileDone);
				}
			}
		}
	}
}

