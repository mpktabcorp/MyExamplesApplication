using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFCoreExamples.AsyncAwait
{
    /// <summary>
    /// Interaction logic for AsyncAwaitWindow.xaml
    /// </summary>
    public partial class AsyncAwaitWindow : Window
    {
        public AsyncAwaitWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Click button to go through numbers and report them back from separate thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ProgressBarButton_Click(object sender, RoutedEventArgs e)
        {
            // create the Progress object to be passed into the Task method 
            Progress<int> progress = new Progress<int>(value =>
            {
                ProgressBar.Value = value;
            });

            await Task.Run(() =>
            {
                LoopThroughNumbers(100, progress);
            });
        }

        /// <summary>
        /// Loop through numbers and report back the percent complete
        /// </summary>
        /// <param name="count"></param>
        /// <param name="progress"></param>
        private void LoopThroughNumbers(int count, IProgress<int> progress)
        {
            for (int x = 0; x < count; x++)
            {
                Thread.Sleep(100);
                int percentComplete = (x * 100) / count;
                progress.Report(percentComplete);
            }
        }
    }
}
