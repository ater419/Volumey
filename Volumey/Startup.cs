﻿using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using log4net;
using log4net.Config;
using Volumey.Helper;

namespace Volumey
{
	static class Startup
	{
		private static Mutex mutex = new Mutex(true, "2A777FD7-2EB6-4D0B-8523-87F23462918B");

		private const int HWND_BROADCAST = 0xffff;
		public static readonly int WM_SHOWME = NativeMethods.RegisterWindowMessage("WM_SHOWME");

		internal static string MinimizedArg { get; } = "-minimized";
		internal static bool StartMinimized { get; private set; }

		[STAThread]
		private static void Main()
		{
			if(mutex.WaitOne(TimeSpan.Zero, true))
			{
				App.InitializeExecutionTimer();
				InitializeLoggerConfig();

				try
				{
					#if(!DEBUG)
					var args = AppInstance.GetActivatedEventArgs();
					if(args != null)
					{
						//when the app was launched on system startup, Kind argument will be "StartupTask"
						//otherwise i.e. when the app was launched normal way it will be "Launch"
						if(args.Kind == ActivationKind.StartupTask)
							StartMinimized = true;
					}
					#endif

					if(!StartMinimized)
					{
						foreach(var arg in Environment.GetCommandLineArgs())
						{
							if(arg == MinimizedArg)
							{
								//log the app version since the app launches with minimized arg only after it was updated
								var ver = Package.Current.Id.Version;
								LogManager.GetLogger(typeof(Startup))
								          .Info($"Started with {MinimizedArg} argument, ver.: [{ver.Major.ToString()}.{ver.Minor.ToString()}.{ver.Build.ToString()}.{ver.Revision.ToString()}]");
								StartMinimized = true;
							}
						}
					}
				}
				catch { }

				App.Main();
				mutex.ReleaseMutex();
			}
			else
			{
				NativeMethods.PostMessage((IntPtr) HWND_BROADCAST, WM_SHOWME, IntPtr.Zero,
				                          IntPtr.Zero);
			}
		}
		
		#if(DEBUG)
		private const string PathToDebugLogConfig = @"";
		#endif

		private static void InitializeLoggerConfig()
		{
			#if(!DEBUG)
			try
			{
				var assembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var repo = LogManager.GetRepository(Assembly.GetExecutingAssembly());
				if(assembly != null)
				{
					var file = new FileInfo(Path.Combine(assembly, "log4net.config"));
					XmlConfigurator.Configure(repo, file);
					try
					{
						PackageVersion ver = Package.Current?.Id?.Version ?? new PackageVersion();
						GlobalContext.Properties["AppVer"] =
							$"[ver.: {ver.Major.ToString()}.{ver.Minor.ToString()}.{ver.Build.ToString()}.{ver.Revision.ToString()}]";
					}
					catch { }
				}
			}
			catch { }
			#endif
			
			#if(DEBUG)
			if(!string.IsNullOrEmpty(PathToDebugLogConfig))
			{
				var file = new FileInfo(PathToDebugLogConfig);
				var repo = LogManager.GetRepository(Assembly.GetExecutingAssembly());
				XmlConfigurator.Configure(repo, file);
			}
			#endif
		}
	}
}