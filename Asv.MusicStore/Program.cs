using Avalonia;
using System;
using System.IO;
using System.Reflection;
using Asv.Avalonia;
using Asv.Common;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;

namespace Asv.MusicStore;

sealed class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		var builder = AppHost.CreateBuilder(args);
		var dataFolder =
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
		builder
			.UseAvalonia(BuildAvaloniaApp)
			.UseAppPath(opt => opt.WithRelativeFolder(Path.Combine(dataFolder, "data")))
			.UseJsonUserConfig(opt =>
				opt.WithFileName("user_settings.json").WithAutoSave(TimeSpan.FromSeconds(1))
			)
			.UseAppInfo(opt => opt.FillFromAssembly(typeof(App).Assembly))
			.UseSoloRun(opt => opt.WithArgumentForwarding())
			.UseLogging(options =>
			{
				options.WithLogToFile(Path.Combine(dataFolder, "data", "logs"));
				options.WithLogToConsole();
				options.WithLogViewer();
				options.WithLogLevel(LogLevel.Trace);
			});
		
		using var host = builder.Build();
		host.ExitIfNotFirstInstance();
		host.StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder
			.Configure<App>()
			.UsePlatformDetect()
			.With(new Win32PlatformOptions { OverlayPopups = true }) // Windows
			.WithInterFont()
			.LogToTrace()
			.UseR3();
			
}