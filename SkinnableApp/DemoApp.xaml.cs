using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SkinnableApp
{
	public partial class DemoApp : Application
	{
		public void ApplySkin(Uri skinDictionaryUri)
		{
			// Load the ResourceDictionary into memory.
			ResourceDictionary skinDict = Application.LoadComponent(skinDictionaryUri) as ResourceDictionary;

			Collection<ResourceDictionary> mergedDicts = base.Resources.MergedDictionaries;

			// Remove the existing skin dictionary, if one exists.
			// NOTE: In a real application, this logic might need
			// to be more complex, because there might be dictionaries
			// which should not be removed.
			if (mergedDicts.Count > 0)
				mergedDicts.Clear();

			// Apply the selected skin so that all elements in the
			// application will honor the new look and feel.
			mergedDicts.Add(skinDict);

		}

        #region SingleInstance

        private readonly string _notifierName =
            AppDomain.CurrentDomain.BaseDirectory.Where(c => char.IsLetterOrDigit(c)).Aggregate("",
                                                                                                (current, c) =>
                                                                                                current + c);

        private Semaphore _notifier;
        private bool _releaseByClose;

        private bool ReleaseByClose
        {
            get { return _releaseByClose; }
            set
            {
                lock (this)
                {
                    _releaseByClose = value;
                }
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            bool isNotExists;
            _notifier = new Semaphore(0, 1, _notifierName, out isNotExists);
            if (!isNotExists)
            {
                _notifier.Release();
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Shutdown();
                return;
            }
            var notifyThread = new Thread(NotifierThread);
            notifyThread.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            var currentNotifier = new Semaphore(0, 1, _notifierName);
            ReleaseByClose = true;
            currentNotifier.Release();
        }

        private void NotifierThread()
        {
            while (!ReleaseByClose)
            {
                _notifier.WaitOne();
                if (!ReleaseByClose)
                {
                    // Повторный запуск приложения
                    Dispatcher.Invoke(DispatcherPriority.Normal,
                                      new Action(delegate
                                      {
                                          if ((MainWindow != null) &&
                                              (((MainWindow)MainWindow).IsInit))
                                          {
                                              ((MainWindow)MainWindow).ShowWindow();
                                          }
                                      }));
                }
            }
        }

        #endregion

	}
}