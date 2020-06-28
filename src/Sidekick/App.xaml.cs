using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sidekick.Core.Natives;
using Sidekick.Localization.Application;
using Sidekick.Localization.Splash;
using Sidekick.Views;
using Sidekick.Views.Initialize;
using Sidekick.Views.TrayIcon;

// Enables debug specific markup in XAML
// See: https://stackoverflow.com/a/19940157
#if DEBUG
[assembly: XmlnsDefinition("debug-mode", "Namespace")]
#endif

namespace Sidekick
{
    /// <summary>
    /// Entry point for the app
    /// </summary>
    public partial class App : Application
    {
        private const string APPLICATION_PROCESS_GUID = "93c46709-7db2-4334-8aa3-28d473e66041";

        private ServiceProvider serviceProvider;
        private ILogger logger;
        private INativeProcess nativeProcess;
        private IViewLocator viewLocator;
        public TaskbarIcon trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AttachErrorHandlers();

            // Tooltip opened indefinitely until mouse is moved.
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

            serviceProvider = Sidekick.Startup.InitializeServices(this);

            logger = serviceProvider.GetRequiredService<ILogger>();
            nativeProcess = serviceProvider.GetRequiredService<INativeProcess>();
            viewLocator = serviceProvider.GetRequiredService<IViewLocator>();

            trayIcon = (TaskbarIcon)FindResource("TrayIcon");
            trayIcon.DataContext = serviceProvider.GetRequiredService<TrayIconViewModel>();

            EnsureSingleInstance();

            viewLocator.Open<InitializeView>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayIcon?.Dispose();
            serviceProvider?.Dispose();
            base.OnExit(e);
        }

        private void EnsureSingleInstance()
        {
            nativeProcess.Mutex = new Mutex(true, APPLICATION_PROCESS_GUID, out var instanceResult);
            if (!instanceResult)
            {
                AdonisUI.Controls.MessageBox.Show(SplashResources.AlreadyRunningText, SplashResources.AlreadyRunningTitle, AdonisUI.Controls.MessageBoxButton.OK, AdonisUI.Controls.MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        private void AttachErrorHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var exception = (Exception)e.ExceptionObject;
                LogUnhandledException(exception);
            };

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception);
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception);
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception ex)
        {
            logger.Fatal(ex, "Unhandled exception in application root");
            Dispatcher.Invoke(() =>
            {
                try
                {
                    AdonisUI.Controls.MessageBox.Show(ApplicationResources.FatalErrorOccured, buttons: AdonisUI.Controls.MessageBoxButton.OK);
                }
                catch (Exception) { }
                Shutdown(1);
            });
        }
    }
}
