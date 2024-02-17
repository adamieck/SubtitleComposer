using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace WPF2
{
    public partial class MainWindow : Window
    {

        private TimeSpan TotalTime;
        public ObservableCollection<Subtitles> subs { get; set; }
        public CollectionViewSource ViewSource { get; set; }
        private bool isPaused = false;
        public MainWindow()
        {
            subs = new ObservableCollection<Subtitles>();
            ViewSource = new CollectionViewSource();
            ViewSource.Source = subs;
            ViewSource.SortDescriptions.Add(new SortDescription("ShowTime",
                ListSortDirection.Ascending));

            DataContext = ViewSource;
            InitializeComponent();
            InitializePlugins();
        }


        private void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a Subtitle Composer.", "Subtitle Composer", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void datagrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            Subtitles newSub = e.NewItem as Subtitles;
            if (newSub == null)
                return;

            var maxTimeSpan = subs.Select(s => s.HideTime).Max();
            newSub.HideTime = maxTimeSpan;
            newSub.ShowTime = maxTimeSpan;
            newSub.Text = "";
            newSub.Translation = "";
        }
        private void MenuItem_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video files (*.mp4;*.wmv;*.avi;)|*.mp4;*.wmv;*.avi|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string videoFilePath = openFileDialog.FileName;
                videoPlayer.Source = new Uri(videoFilePath, UriKind.RelativeOrAbsolute);
                videoPlayer.Play();
            }
        }

        private void videoPlayer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (videoPlayer.Volume < 1)
                {
                    videoPlayer.Volume += 0.01;
                }
            }
            else
            {
                if (videoPlayer.Volume > 0)
                {
                    videoPlayer.Volume -= 0.01;
                }
            }
            e.Handled = true;
        }

        private void videoPlayer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isPaused)
            {
                videoPlayer.Play();
                isPaused = false;
            }
            else
            {
                videoPlayer.Pause();
                isPaused = true;
            }
        }

        private void datagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datagrid.SelectedItem == null || !(datagrid.SelectedItem is Subtitles)) return;

            var timestamp = ((Subtitles)datagrid.SelectedItem).ShowTime;
            videoPlayer.Position = timestamp;
            videoPlayer.Pause();
            isPaused = true;
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.Play();
            isPaused = false;
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.Pause();
            isPaused = true;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.Stop();
            isPaused = true;
            videoPlayer.Position = TimeSpan.Zero;
        }

        private void durationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TotalTime.TotalSeconds > 0)
            {
                videoPlayer.Position = TimeSpan.FromSeconds(durationSlider.Value * TotalTime.TotalSeconds);
                timestampLabel.Content = videoPlayer.Position.ToString(@"h\:mm\:ss\.fff");
            }
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            videoPlayer.Volume = volumeSlider.Value;
        }

        private void videoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            TotalTime = videoPlayer.NaturalDuration.TimeSpan;
            volumeSlider.Value = 1.0 / videoPlayer.Volume;

            var timerVideoTime = new DispatcherTimer();
            timerVideoTime.Interval = TimeSpan.FromMilliseconds(10);
            timerVideoTime.Tick += new EventHandler(timer_Tick);
            timerVideoTime.Start();
        }

        //https://stackoverflow.com/questions/10208959/binding-mediaelement-to-slider-position-in-wpf
        private void timer_Tick(object sender, EventArgs e)
        {
            if (videoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds <= 0) return;

            // Sliders
            if (TotalTime.TotalMilliseconds > 0)
            {
                durationSlider.Value = videoPlayer.Position.TotalMilliseconds /
                                    TotalTime.TotalMilliseconds;
                timestampLabel.Content = videoPlayer.Position.ToString("h\\:mm\\:ss\\.fff");
            }

            //Subtitles
            StringBuilder subBuilder = new StringBuilder();
            foreach (Subtitles subtitle in subs)
            {
                
                if (subtitle.ShowTime <= videoPlayer.Position && subtitle.HideTime >= videoPlayer.Position)
                {
                    if (!TranslationEnable.IsChecked)
                        subBuilder.Append(subtitle.Text);
                    else
                        subBuilder.Append(subtitle.Translation);
                    subBuilder.Append("\n");
                }
            }

            if (subBuilder.Length <= 0)
            {
                subtitleBlock.Visibility = Visibility.Collapsed;
                subtitleBlock.Text = "";
            }
            else
            {
                subtitleBlock.Visibility = Visibility.Visible;
                subtitleBlock.Text = subBuilder.ToString();
            }
        }

        private void gridAddMenu_Click(object sender, RoutedEventArgs e)
        {
            var maxTimeSpan = subs.Select(s => s.HideTime).Max();
            Subtitles newSub = new Subtitles(maxTimeSpan, maxTimeSpan, "", "");
            subs.Add(newSub);
        }

        private void gridAddAfterMenu_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan maxTimeSpan = TimeSpan.Zero;
            foreach (var x in datagrid.SelectedItems)
            {
                var sub = (Subtitles)x;
                if (sub.HideTime > maxTimeSpan)
                    maxTimeSpan = sub.HideTime;
            }

            Subtitles newSub = new Subtitles(maxTimeSpan, maxTimeSpan, "", "");
            subs.Add(newSub);
        }

        private void gridDeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            for (int i = datagrid.SelectedItems.Count - 1; i >= 0; i--)
                subs.Remove((Subtitles)datagrid.SelectedItems[i]);
        }

        static Assembly LoadPlugin(string pluginLocation)
        {
            PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
            return loadContext.LoadFromAssemblyName(new AssemblyName(System.IO.Path.GetFileNameWithoutExtension(pluginLocation)));
        }
        private void InitializePlugins()
        {
            string? globalPluginsPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\plugins\";
            string[] pluginPaths = System.IO.Directory.GetFiles(globalPluginsPath, "*.dll");

            IEnumerable<ISubtitlesPlugin?> plugins = pluginPaths.SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = LoadPlugin(pluginPath);
                return CreatePlugins(pluginAssembly);
            }).ToList();

            foreach(var plugin in plugins)
            {
                MenuItem plugOpen = new MenuItem();
                MenuItem plugSave = new MenuItem();
                MenuItem plugTranslation = new MenuItem();
                plugOpen.Header = plugin.Name;

                plugOpen.Click += (sender, args) =>
                {
                    var dialog = new Microsoft.Win32.OpenFileDialog();
                    dialog.DefaultExt = plugin.Extension;
                    dialog.Filter = $"Subtitle Files (*{plugin.Extension})|*{plugin.Extension}";
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        foreach (var elem in plugin.Load(dialog.FileName))
                        {
                            subs.Add(elem);
                        }
                    }
                };
                MenuOpen.Items.Add(plugOpen);

                plugSave.Header = plugin.Name;
                plugSave.Click += (sender, args) =>
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog();
                    dialog.DefaultExt = plugin.Extension;
                    dialog.Filter = $"Subtitle Files (*{plugin.Extension})|*{plugin.Extension}";
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        plugin.Save(dialog.FileName, subs);
                    };
                };
                MenuSave.Items.Add(plugSave);

                plugTranslation.Header = plugin.Name;
                plugTranslation.Click += (sender, args) =>
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog();
                    dialog.DefaultExt = plugin.Extension;
                    dialog.Filter = $"Subtitle Files (*{plugin.Extension})|*{plugin.Extension}";
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        plugin.SaveTranslation(dialog.FileName, subs);
                    };
                };
                MenuSaveTranslation.Items.Add(plugTranslation);

            }
        }
        static IEnumerable<ISubtitlesPlugin?> CreatePlugins(Assembly assembly)
        {
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(ISubtitlesPlugin).IsAssignableFrom(type))
                {
                    if (Activator.CreateInstance(type) is ISubtitlesPlugin result)
                    {
                        count++;
                        yield return result;
                    }
                }
            }

        }
    }
}