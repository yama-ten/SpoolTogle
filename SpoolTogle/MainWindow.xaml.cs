using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpoolTogle
{

	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public string ServiceName = "Spooler";
		public MainWindow() {
			InitializeComponent();

			ServiceControllerStatus sc_stat = check_stat();
		}

		private ServiceControllerStatus check_stat() {
			ServiceControllerStatus stat;
			string text = "";
			Brush bg_color = null;
			//usingステートメントは範囲から抜けた際に自動的にDisposeなどしてくれるので便利です。
			//「任意のWindowsサービス」は動作したいサービス名を指定してください。
			using (ServiceController sc = new ServiceController(ServiceName)) // 任意のWindowsサービス名
			{
				//プロパティ値を更新
				sc.Refresh();
				stat = sc.Status;
				switch (stat) {
				case ServiceControllerStatus.Running:
					text = "稼働中";
					bg_color =  new LinearGradientBrush(Colors.LightSeaGreen, Colors.LightBlue, 90);
					start_button.IsEnabled = false;
					start_button.Content = "動作中";
					start_button.Background = new LinearGradientBrush(Colors.LightBlue, Colors.Gray, 90);
					
					stop_button.IsEnabled = true;
					stop_button.Content = "停止させる";
					stop_button.Background = new LinearGradientBrush(Colors.OrangeRed, Colors.LightPink, 90);
					break;

				case ServiceControllerStatus.Stopped:
					text = "停止中";
					bg_color =  new LinearGradientBrush(Colors.LightPink, Colors.OrangeRed, 90);
					start_button.IsEnabled = true;
					start_button.Content = "起動させる";
					start_button.Background = new LinearGradientBrush(Colors.LightBlue, Colors.SlateBlue, 90);
					
					stop_button.IsEnabled = false;
					stop_button.Content = "停止中";
					stop_button.Background = new LinearGradientBrush(Colors.OrangeRed, Colors.Gray, 90);
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

		private void program_exit()
		{
			ServiceControllerStatus sc_stat = check_stat();
			
		//	Application.Current.Shutdown();
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