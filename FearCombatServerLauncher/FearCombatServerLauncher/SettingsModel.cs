using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public ObservableCollection<SettingsItemModel> Items { get; set; }

        public SettingsModel()
        {
            this.Parse(File.OpenRead("Content/ExampleServerOptions.txt"));
        }

        public SettingsModel(string path)
        {
            this.Parse(File.OpenRead(path));
        }

        public void Parse(Stream stream)
        {
            var items = new List<SettingsItemModel>(500);
            var streamReader = new StreamReader(stream);
            string actGroup = "No Group";
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(";")) continue; // this line is a comment
                if (trimmedLine.StartsWith("[")) // this line is a settings group
                {
                    actGroup = trimmedLine;
                }
                else
                {
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
        }
    }
}