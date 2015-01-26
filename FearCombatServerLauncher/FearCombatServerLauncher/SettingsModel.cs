using CS.MVVM.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace FearCombatServerLauncher
{
    class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


    class SettingsItemModel : ModelBase
    {
        private string _group;

        public string Group
        {
            get { return _group; }
            set { _group = value; RaisePropertyChanged("Group"); }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged("Name"); }
        }

        private string _value;

        public string Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged("Value"); }
        }
    }

    class SettingsModel : ModelBase
    {
        public string FilePath { get; private set; }

        public ObservableCollection<SettingsItemModel> Items { get; set; }

        public ListCollectionView CollectionView { get; set; }

        public SettingsModel()
        {
            FilePath = Properties.Settings.Default.LastOpenedFile;
            this.Parse(File.OpenRead(FilePath));
            InitializeCommands();
        }

        public void Parse(Stream stream)
        {
            var items = new List<SettingsItemModel>(500);
            var streamReader = new StreamReader(stream);
            string actGroup = "No Group";
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue; // this is an empty line
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(";")) continue; // this line is a comment
                if (trimmedLine.StartsWith("[")) // this line is a settings group
                {
                    actGroup = trimmedLine;
                }
                else
                {
                    // this is a normal, setting item line
                    var splittedLine = trimmedLine.Split('=');
                    items.Add(new SettingsItemModel()
                    {
                        Group = actGroup,
                        Name = splittedLine[0],
                        Value = splittedLine.Length > 1 ? splittedLine[1] : null
                    });
                }
            }

            Items = new ObservableCollection<SettingsItemModel>(items);

            CollectionView = new ListCollectionView(Items);
            CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            RaisePropertyChanged("CollectionView");
            streamReader.Close();
            stream.Close();
            Properties.Settings.Default.LastOpenedFile = FilePath;
            Properties.Settings.Default.Save();
        }

        private void InitializeCommands()
        {
            Save = new RelayCommand(o => save());
            SaveAs = new RelayCommand(o => saveAs());
            Load = new RelayCommand(o => load());
            Start = new RelayCommand(o => start());
            SetFearServerExePath = new RelayCommand(o =>
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.FileName = Properties.Settings.Default.FearServerExePath;
                    if (dialog.ShowDialog() == true)
                    {
                        Properties.Settings.Default.FearServerExePath = dialog.FileName;
                        Properties.Settings.Default.Save();
                    }
                });
        }

        private void load()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
                this.Parse(File.OpenRead(FilePath));
            }
        }

        public void save()
        {
            var stream = File.Open(FilePath, FileMode.Create);
            var streamWriter = new StreamWriter(stream);
            SettingsItemModel prevItem = null;
            foreach (var item in Items.OrderBy(o => o.Name).OrderBy(o => o.Group))
            {
                if (prevItem == null || prevItem.Group != item.Group)
                {
                    streamWriter.WriteLine(item.Group);
                }
                else
                {
                    streamWriter.WriteLine(string.Format("{0}={1}", item.Name, item.Value ?? string.Empty));
                }
                prevItem = item;
            }
            streamWriter.Close();
            stream.Close();
        }

        public void saveAs()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
                save();
            }
        }

        public void start()
        {
            save();
            var server = new Process();
            server.StartInfo.FileName = Properties.Settings.Default.FearServerExePath;
            server.StartInfo.Arguments = string.Format("-optionsfile \"{0}\"", FilePath);
            server.StartInfo.WorkingDirectory = Path.GetDirectoryName(Properties.Settings.Default.FearServerExePath);
            server.Start();
        }

        public RelayCommand Save { get; private set; }
        public RelayCommand SaveAs { get; private set; }
        public RelayCommand Load { get; private set; }
        public RelayCommand Start { get; private set; }

        public RelayCommand SetFearServerExePath { get; private set; }
    }
}