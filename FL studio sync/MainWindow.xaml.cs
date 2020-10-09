namespace FL_studio_sync
{
    using Newtonsoft.Json;
    using Ookii.Dialogs.Wpf;
    using Standart.Hash.xxHash;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Defines the application_config.
        /// </summary>
        private Application application_config = new Application();

        /// <summary>
        /// Defines the application_config_file_path.
        /// </summary>
        internal string application_config_file_path = "save.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The get_valid_file_name.
        /// </summary>
        /// <param name="filename">The filename<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        internal string get_valid_file_name(string filename)
        {
            string invalid = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                filename = filename.Replace(c.ToString(), "");
            }

            return filename;
        }

        /// <summary>
        /// The CreateBackupFile.
        /// </summary>
        /// <param name="file">The file<see cref="FLP_file"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task CreateBackupFile(FLP_file file)
        {
            file.history.Add(new History(file.filename));
            string file_destination = System.IO.Path.Combine(application_config.destination, file.filename);
            Console.WriteLine(file_destination);
            if (!Directory.Exists(file_destination))
            {
                Directory.CreateDirectory(file_destination);
            }
            try
            {
                Console.WriteLine(file.filename);
                File.Copy(file.filepath, System.IO.Path.Combine(file_destination, get_valid_file_name(DateTime.Now + file.filename)), true);
            }
            catch (IOException iox)
            {
                Console.WriteLine(iox.Message);
            }
        }

        /// <summary>
        /// The CheckForNewFile.
        /// </summary>
        /// <param name="directory">The directory<see cref="string"/>.</param>
        private void CheckForNewFile(string directory)
        {
            DirectoryInfo d = new DirectoryInfo(directory);
            FileInfo[] Files = d.GetFiles("*.flp");
            foreach (FileInfo file in Files)
            {
                FLP_file new_file = new FLP_file();
                new_file.filepath = directory + "\\" + file.Name;
                Console.WriteLine(new_file.filepath);
                new_file.filename = file.Name;
                new_file.hash = HashFile(new_file.filepath);
                if (!ContainsItemFLPFile(new_file))
                {
                    application_config.files.Add(new_file);
                    CreateBackupFile(new_file);
                }
            }
        }

        /// <summary>
        /// The BackgroundCheck.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task BackgroundCheck()
        {
            while (true)
            {
                if (application_config.directories.Count != 0)
                {
                    foreach (string directory in application_config.directories)
                    {
                        CheckForNewFile(directory);
                    }
                }
                await Task.Delay(10 * 60);
            }
        }

        /// <summary>
        /// The ContainsItemFLPFile.
        /// </summary>
        /// <param name="item">The item<see cref="FLP_file"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ContainsItemFLPFile(FLP_file item)
        {
            foreach (FLP_file file in application_config.files)
            {
                if (file.hash == item.hash)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// The Button_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var obd = new VistaFolderBrowserDialog();
            if (obd.ShowDialog() == true)
            {
                application_config.directories.Add(obd.SelectedPath);
            }
        }

        /// <summary>
        /// The HashFile.
        /// </summary>
        /// <param name="file">The file<see cref="string"/>.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        private ulong HashFile(string file)
        {
            using (FileStream fs = File.OpenRead(file))
            {
                return xxHash64.ComputeHash(fs);
            }
        }

        /// <summary>
        /// The SelectBackupFolder.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void SelectBackupFolder(object sender, RoutedEventArgs e)
        {
            var obd = new VistaFolderBrowserDialog();
            if (obd.ShowDialog() == true)
            {
                application_config.destination = obd.SelectedPath;
 
            }

        }

        /// <summary>
        /// The Window_Closing.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.ComponentModel.CancelEventArgs"/>.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string jsonString;
            jsonString = JsonConvert.SerializeObject(application_config);
            Console.WriteLine(jsonString);
            System.IO.File.WriteAllText(application_config_file_path, jsonString);
        }

        /// <summary>
        /// The Window_Loaded.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("ohyeah");
            if (File.Exists(application_config_file_path))
            {
                string file = File.ReadAllText(application_config_file_path);
                application_config = JsonConvert.DeserializeObject<Application>(file);
            }
            else
            {
                System.IO.File.WriteAllText("save.json", "{\"dir\":null,\"directories\":[],\"files\":[],\"destination\":\"null\"}");
            }
            if (application_config.destination != null)
            {
                add_dir.IsEnabled = true;
            }
            Task.Run(BackgroundCheck);
        }
    }
}
