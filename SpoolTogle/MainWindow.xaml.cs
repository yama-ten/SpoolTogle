/****************************************************************
 * title: PrintSpooler service util.
 * file: MainWindow.xaml.cs
 * discription: プリントスプーラのサービスを走らせたり、止めたり、キャッシュを削除したりします。
 * version: 2.0
 * date: 2021.09.24
 * copyright: 
 */

using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

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

		/// <summary>
		/// 
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
			RadioButton btn = (RadioButton)sender;
			if ((bool)btn.IsChecked) {
				using (ServiceController sc = new ServiceController(ServiceName)) {
					sc.Refresh();       //プロパティ値を更新

					if (sc.Status == ServiceControllerStatus.Stopped) { //停止中
						sc.Start();     // 開始
						sc.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);
						program_exit();
					}
				}
			}
			// check_stat();
		}

		/// <summary> [停止]ボタンを押した </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void stop_button_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton btn = (RadioButton)sender;

			if ((bool)btn.IsChecked) {
				using (ServiceController sc = new ServiceController(ServiceName)) {
					sc.Refresh();       //プロパティ値を更新

					if (sc.Status == ServiceControllerStatus.Running) { //起動中
						sc.Stop();   // 停止
						sc.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped);
						program_exit();
					}
				}
			}
			// check_stat();
		}

		/// <summary> 終わり </summary>
		private void program_exit()
		{
			ServiceControllerStatus sc_stat = check_stat();
		//	Application.Current.Shutdown();
		}

		/// <summary> メッセージボックスの体裁を変えたいときはここをいじる
		/// https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/windows/how-to-open-message-box?view=netdesktop-5.0
		/// </summary>
		/// <param name="text"></param>
		/// <param name="cap"></param>
		/// <returns></returns>
		MessageBoxResult mbox(string text, string cap)
		{
			string messageBoxText = text; //"Do you want to save changes?";
			string caption = cap; //"Word Processor";
			MessageBoxButton button = MessageBoxButton.YesNoCancel;
			MessageBoxImage icon = MessageBoxImage.Warning;
			MessageBoxResult result;

			result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

			return result;
		}

		/// <summary> [キャッシュ削除]ボタンを押した </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cache_clear_Click(object sender, RoutedEventArgs e)
		{
			string spool_dir = SPOOL_DIR;

			if (!Directory.Exists(spool_dir))
				return;

			if (MessageBox.Show("本当にキャッシュを消してもよいか ?", "SpoolTogle"
						, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) 
				return;

			// ディレクトリ 'C:\Windows\System32\spool\PRINTERS' 取得
			foreach (string fpath in System.IO.Directory.EnumerateFiles(spool_dir, "*")) {
				//１ファイルの削除実行。
				System.Diagnostics.Debug.WriteLine("削除 > " + fpath);
				System.IO.File.Delete(fpath);
				//	if (File.Exists(fpath))
				//		System.Diagnostics.Debug.WriteLine("失敗 " + fpath);
				//	else 
				//		System.Diagnostics.Debug.WriteLine("seikou 成功");
			}
			clear_button_text(true);
		}


		/// <summary>
		/// 2021.10.04
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void spoolFolder_Click(object sender, RoutedEventArgs e)
		{
            ProcessStartInfo pInfo = new ProcessStartInfo();
            pInfo.FileName = "explorer";
            pInfo.Arguments = SPOOL_DIR;
 
            Process.Start(pInfo);
 		}
	}
}

/**


	
static void Test2(string[] args)
{
	try
	{
		string name;
		string machine_name;
		name = "Spooler";
		machine_name = ".";
		System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController(name, machine_name);
		System.ServiceProcess.ServiceControllerStatus stat = sc.Status;
		Console.WriteLine(string.Format("Service=\"{0}\" Current Stat={1}", name, stat.ToString()));
		switch (stat)
		{
			case System.ServiceProcess.ServiceControllerStatus.Stopped:
				// サービスが停止している場合
				sc.Start();
				// 指定した状態になるのを待つ
				sc.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);
				Console.WriteLine(string.Format("Service=\"{0}\" Changed Stat={1}", name, sc.Status.ToString()));
				break;
			case System.ServiceProcess.ServiceControllerStatus.Running:
				if(sc.CanStop)
				{
					sc.Stop();
					// 指定した状態になるのを待つ
					sc.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped);
					Console.WriteLine(string.Format("Service=\"{0}\" Changed Stat={1}", name, sc.Status.ToString()));
				}
				else if (sc.CanPauseAndContinue)
				{
					// 一時中断、再開できる場合
					sc.Pause();
					// 指定した状態になるのを待つ
					sc.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Paused);
					Console.WriteLine(string.Format("Service=\"{0}\" Changed Stat={1}", name, sc.Status.ToString()));
				}
				break;
			case System.ServiceProcess.ServiceControllerStatus.Paused:
				// 一時中断状態の場合
				if (sc.CanPauseAndContinue)
				{
					// 一時中断、再開できる場合
					sc.Continue();
					// 指定した状態になるのを待つ
					sc.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);
					Console.WriteLine(string.Format("Service=\"{0}\" Changed Stat={1}", name, sc.Status.ToString()));
				}
				break;
			default:
				break;
		}
	}
	catch (InvalidOperationException ex)
	{
		Console.WriteLine(ex.Message);
	}
	catch (Exception ex)
	{
		Console.WriteLine(ex.Message);
	}
}

***

namespace ServiceControllerSample
{
	class Program
	{
		static void Main(string[] args)
		{
			//usingステートメントは範囲から抜けた際に自動的にDisposeなどしてくれるので便利です。
			//「任意のWindowsサービス」は動作したいサービス名を指定してください。
			using (ServiceController sc = new ServiceController("Spooler")) // 任意のWindowsサービス名
			{
				//プロパティ値を更新
				sc.Refresh();
				 
				//起動中
				if (sc.Status == ServiceControllerStatus.Running)
				{
					// 停止
					sc.Stop();
					// 一時停止
					//sc.Pause();
				}
				//停止中
				else if (sc.Status == ServiceControllerStatus.Stopped)
				{
					// 開始
					sc.Start();
				}
				//一時停止中
				else if (sc.Status == ServiceControllerStatus.Paused)
				{
					// 再開
					sc.Continue();
					// 停止
					//sc.Stop();
				}
			}
		}
	}
}
 **/