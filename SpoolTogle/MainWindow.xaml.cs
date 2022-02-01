/****************************************************************
 * title: PrintSpooler service util.
 * file: MainWindow.xaml.cs
 * discription: プリントスプーラのサービスを走らせたり、止めたり、キャッシュを削除したりします。
 * version: 2.0
 * date: 2021.09.24
 * copyright: 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace SpoolTogle
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
	{
		//public string ServiceName = "Spooler";
		string ServiceName = "Spooler";
		
		// スプールディレクトリ 'C:\Windows\System32\spool\PRINTERS' 
		string SPOOL_DIR = @"C:\Windows\System32\spool\PRINTERS";

		/// <summary> 主処理 </summary>
		public MainWindow() {
			InitializeComponent();

			ServiceControllerStatus sc_stat = check_stat();
		}



		/// <summary> [キャッシュ削除] ボタンのテキスト
		/// https://www.kakistamp.com/entry/2018/04/02/221856
		///==========< ボタン内で改行を入れる >==========
		/// //(コード側で設定)
		/// var b = new TextBlock();
		/// b.Text = "TexoBlockのTextWrappingを使うと改行が楽だよ。";
		/// b.TextWrapping = TextWrapping.Wrap;
		/// Button02.Content = b;
		/// 
		/// </summary>
		/// <param name="runing"></param>
		void clear_button_text(bool runing = true)
		{
			List<string> files = new List<string>();
			files.AddRange(Directory.EnumerateFiles(SPOOL_DIR, "*"));
			var blk = new TextBlock();
			blk.Text = "キャッシュ削除";
			if (files.Count > 0) {
				blk.Text += $"\n     ( {files.Count} 件)";
			}
			clear_cache.Content = blk;

			if (!runing && (files.Count > 0)) {
				// 停止中かつキャッシュあり
				clear_cache.IsEnabled = true;
				clear_cache.Background = new LinearGradientBrush(Color.FromRgb(0xfb, 0xf4, 0x5f), Colors.Gray, 90);
			}
			else {
				// 動作中またはキャッシュなし
				clear_cache.IsEnabled = false;
				clear_cache.Background = new LinearGradientBrush(Color.FromRgb(128, 128, 58), Colors.Gray, 90);
			}

		}

		/// <summary> [起動] ボタンのテキスト
		/// 
		/// </summary>
		/// <param name="runing"></param>
		void start_button_text(bool runing)
		{
			Brush bg_color = null;
			if (runing) {
				bg_color =  new LinearGradientBrush(Colors.LightSeaGreen, Colors.LightBlue, 90);
				start_button.IsEnabled = false;
				start_button.Content = "動作中";
				start_button.Background = new LinearGradientBrush(Colors.LightBlue, Colors.Gray, 90);
			}
			else {
				bg_color =  new LinearGradientBrush(Colors.LightPink, Colors.OrangeRed, 90);
				start_button.IsEnabled = true;
				start_button.Content = "起動させる";
				start_button.Background = new LinearGradientBrush(Colors.LightBlue, Colors.SlateBlue, 90);
			}
		}

		/// <summary> [停止] ボタンのテキスト
		/// 
		/// </summary>
		/// <param name="runing"></param>
		void stop_button_text(bool runing)
		{
			if (runing) {
				stop_button.IsEnabled = true;
				stop_button.Content = "停止させる";
				stop_button.Background = new LinearGradientBrush(Colors.OrangeRed, Colors.LightPink, 90);
			}
			else {
				stop_button.IsEnabled = false;
				stop_button.Content = "停止中";
				stop_button.Background = new LinearGradientBrush(Colors.OrangeRed, Colors.Gray, 90);
			}
		}

		/// <summary> サービスの稼働状態を調べる
		/// </summary>
		/// <returns></returns>
		private ServiceControllerStatus check_stat() {
			ServiceControllerStatus stat;
			string text = "";
			Brush bg_color = null;
			//usingステートメントは範囲から抜けた際に自動的にDisposeなどしてくれるので便利です。
			//「任意のWindowsサービス」は動作したいサービス名を指定してください。
			using (ServiceController sc = new ServiceController(ServiceName)) // 任意のWindowsサービス名
			{
				clear_button_text();

				//プロパティ値を更新
				sc.Refresh();
				stat = sc.Status;
				switch (stat) {
				case ServiceControllerStatus.Running:
					text = "稼働中";
					start_button_text(true);
					stop_button_text(true);
					clear_button_text(true);
					break;

				case ServiceControllerStatus.Stopped:
					text = "停止中";
					start_button_text(false);
					stop_button_text(false);
					clear_button_text(false);
					break;

				case ServiceControllerStatus.Paused:
					bg_color = this.Background;
					text = "一時停止中";
					break;
				}

				this.Title = "プリンタスプーラ --- [" + text + "]";
				this.Background = bg_color;
			}
			return stat;
		}

		/// <summary> [起動]ボタンを押した </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void start_button_Checked(object sender, RoutedEventArgs e)
		{
			var btn = (System.Windows.Controls.RadioButton)sender;
			if (!(bool)btn.IsChecked) 
				return;

			using (ServiceController sc = new ServiceController(ServiceName)) {
				sc.Refresh();       //プロパティ値を更新

				if (sc.Status == ServiceControllerStatus.Stopped) { //停止中なら
					sc.Start();     // 開始する
					sc.WaitForStatus(ServiceControllerStatus.Running);
					program_exit();
				}
			}
		}

		/// <summary> [停止]ボタンを押した </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void stop_button_Checked(object sender, RoutedEventArgs e)
		{
			var btn = (System.Windows.Controls.RadioButton)sender;
			if (!(bool)btn.IsChecked) 
				return;

			using (ServiceController sc = new ServiceController(ServiceName)) {
				sc.Refresh();	//プロパティ値を更新

				if (sc.Status == ServiceControllerStatus.Running) { //起動中なら
					sc.Stop();	// 停止する
					sc.WaitForStatus(ServiceControllerStatus.Stopped);
					program_exit();
				}
			}
		}

		/// <summary> 終わり </summary>
		private void program_exit()
		{
			ServiceControllerStatus sc_stat = check_stat();
		}

		/// <summary> [キャッシュ削除]ボタンを押した </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cache_clear_Click(object sender, RoutedEventArgs e)
		{
			string spool_dir = SPOOL_DIR;

			if (!Directory.Exists(spool_dir))
				return;

			spoolFolder_Open(sender, e);

			var res = CustomMessageBox.Show(new WindowWrapper(this)
						, "本当にキャッシュを消してもよいか ?"
						, "SpoolTogle"
						, (System.Windows.Forms.MessageBoxButtons)MessageBoxButton.OKCancel
						, (System.Windows.Forms.MessageBoxIcon)MessageBoxImage.Question);
			if (res != System.Windows.Forms.DialogResult.OK) 
				return;

			// ディレクトリ 'C:\Windows\System32\spool\PRINTERS' 取得
			foreach (string fpath in Directory.EnumerateFiles(spool_dir, "*")) {
				//１ファイルの削除実行。
				System.Diagnostics.Debug.WriteLine("削除 > " + fpath);
				File.Delete(fpath);
			}
			clear_button_text(true);
		}


		/// <summary>
		/// スプールファイルのあるフォルダをエクスプローラで開く
		/// 2021.10.04
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void spoolFolder_Open(object sender, RoutedEventArgs e)
		{
			ProcessStartInfo pInfo = new ProcessStartInfo();
			pInfo.FileName = "explorer";
			pInfo.Arguments = SPOOL_DIR;
 
			Process.Start(pInfo);
		}


	}

	/// <summary>
	/// ウィンドウハンドルを取得
	/// 2022.02.01
	/// using System.Windows.Interop;
	/// 
	/// "Get System.Windows.Forms.IWin32Window from WPF window"
	/// https://stackoverflow.com/questions/10296018/get-system-windows-forms-iwin32window-from-wpf-window/10296513
	/// You would then get your IWin32Window like this:
	///		IWin32Window win32Window = new WindowWrapper(new WindowInteropHelper(this).Handle);
	/// or(in response to KeithS' suggestion):
	///		IWin32Window win32Window = new WindowWrapper(this);
	/// </summary>
	public class WindowWrapper : System.Windows.Forms.IWin32Window
	{
		public WindowWrapper(IntPtr handle) 
		{
			_hwnd = handle;
		}

		public WindowWrapper(Window window)
		{
			_hwnd = new WindowInteropHelper(window).Handle;
		}

		public IntPtr Handle
		{
			get { return _hwnd; }
		}

		private IntPtr _hwnd;
	}
}
