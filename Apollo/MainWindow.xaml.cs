using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

using Apollo.Classes;

namespace Apollo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string poemsPath = "Poems";
        private const string textsPath = "Texts";
        private const string textFiles = "*.txt";

        public MainWindow()
        {
            InitializeComponent();
            spStatus.Visibility = Visibility.Collapsed;
            tboxPhrase.Focus();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            btnCompose.IsEnabled = false;
            tbStatus.Text = "Training model...";
            pBar.IsIndeterminate = false;
            spStatus.Visibility = Visibility.Visible;

            Task[] tasks =
            {
                TrainPoems(),
                TrainTexts()
            };

            await Task.WhenAll(tasks);

            spStatus.Visibility = Visibility.Collapsed;
            btnCompose.IsEnabled = true;
        }

        private async Task TrainPoems()
        {
            DirectoryInfo dir = new DirectoryInfo(poemsPath);

            foreach (FileInfo f in dir.GetFiles(textFiles, SearchOption.AllDirectories))
            {
                string poemText = await GetTextFromFile(f.FullName);
                await PoetryComposer.TrainPoem(poemText);
            }
        }

        private async Task TrainTexts()
        {
            DirectoryInfo dir = new DirectoryInfo(textsPath);

            var files = dir.GetFiles(textFiles, SearchOption.AllDirectories);
            pBar.Maximum = files.Length - 1;
            int i = 1;

            foreach (FileInfo f in files)
            {
                pBar.Value = i++;
                string text = await GetTextFromFile(f.FullName);
                await PoetryComposer.TrainText(text);
            }
        }

        public static async Task<string> GetTextFromFile(string fileName)
        {
            try
            {
                using (StreamReader sr = new StreamReader(fileName, Encoding.Default))
                {
                    return await sr.ReadToEndAsync();
                }
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        private async void btnCompose_Click(object sender, RoutedEventArgs e)
        {
            tbPoem.Text = String.Empty;
            btnCompose.IsEnabled = btnSave.IsEnabled = false;

            tbPoem.Text = await PoetryComposer.GetNewPoem(tboxPhrase.Text);

            btnCompose.IsEnabled = btnSave.IsEnabled = true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Text files (.txt)|*.txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            string title = tbPoem.Text.Substring(0, tbPoem.Text.IndexOf("\r"));

            dlg.FileName = PoetryComposer.RemovePunctuationMarks(title.ToLower()) + ".txt";
            
            if (dlg.ShowDialog().Value)
            {
                using (StreamWriter poemFile = new StreamWriter(dlg.FileName))
                {
                    poemFile.Write(tbPoem.Text);
                }
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "APOLLO\n" +
                "A 'Pie Forzado' Automatic Poetry Composer.\n\n" +
                "'Pie Forzado': Verse that occurs when the author is imposed the word or verse in which the poem must end.\n\n\n" +
                "Created by Rubén Hinojosa Chapel\n" +
                "contact@hinojosachapel.com\n\n" +
                "for the Turing Tests 2018\n" +
                "bregman.dartmouth.edu/turingtests", "About Apollo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
