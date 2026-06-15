using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace Heracles.Robot.Models
{
    public class PositionPreset
    {
        public string Name { set; get; }

        public double J1 { set; get; }
        public double J2 { set; get; }
        public double J3 { set; get; }
        public double J4 { set; get; }
        public double J5 { set; get; }
        public double J6 { set; get; }
        public override string ToString()
        {
            return $"{Name} {J1:0.00} {J2:0.00} {J3:0.00} {J4:0.00} {J5:0.00} {J6:0.00}";
        }
    }

    public interface IPositionsPresetsMonitor
    {
        ObservableCollection<PositionPreset> PositionPresets { get; }
        event EventHandler<EventArgs> PositionPresetsChanged;
        void Save();
    }

    public class PositionsPresetsXMLMonitor : IPositionsPresetsMonitor
    {
        private object _lock = new object();
        public PositionsPresetsXMLMonitor(string robotPositionsXmlFileName)
        {
            _robotPositionsXmlFileName = robotPositionsXmlFileName;
            _robotPositionsXmlFileNameWatcher = new FileSystemWatcher(".", robotPositionsXmlFileName);
            _robotPositionsXmlFileNameWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _robotPositionsXmlFileNameWatcher.Changed += _onRobotPositionsXmlChanged;
            _robotPositionsXmlFileNameWatcher.EnableRaisingEvents = true;

            _refreshPositionPresets();

            //PositionPresets.CollectionChanged += (sender, e) => {
            //    XmlSerializer serializer = new(typeof(ObservableCollection<PositionPreset>), typeof(ObservableCollection<PositionPreset>).GetNestedTypes());
            //    serializer.Save(_robotPositionsXmlFileName, PositionPresets);
            //};
        }
        FileSystemWatcher _robotPositionsXmlFileNameWatcher;
        string _robotPositionsXmlFileName;
        private void _onRobotPositionsXmlChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            _refreshPositionPresets();
        }
        public ObservableCollection<PositionPreset> PositionPresets { get; private set; } = new();

        public void Save()
        {
            _savePositionPresets();
        }
        private void _refreshPositionPresets()
        {
            lock (_lock)
            {

                XmlSerializer serializer = new(typeof(ObservableCollection<PositionPreset>), typeof(ObservableCollection<PositionPreset>).GetNestedTypes());
                try
                {
                    PositionPresets = (ObservableCollection<PositionPreset>)serializer.Load(_robotPositionsXmlFileName);
                    PositionPresetsChanged?.Invoke(this, new EventArgs());
                }
                catch { }
            }
        }

        private void _savePositionPresets()
        {
            lock (_lock)
            {
                XmlSerializer serializer = new(typeof(ObservableCollection<PositionPreset>), typeof(ObservableCollection<PositionPreset>).GetNestedTypes());
                serializer.Save(_robotPositionsXmlFileName, PositionPresets);

                try
                {
                    // workaround to save xml to project folder also
                    // copy from
                    // Xcc\Heracles\Heracles.Robot\bin\x64\Debug\net8.0-windows7.0\HeraclesRobotPositions.xml 
                    // to
                    // Xcc\Heracles\Heracles.Robot\HeraclesRobotPositions.xml
                    var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
                    var projectDirectory = currentDirectory.Parent.Parent.Parent.Parent.Parent.FullName;
                    // "Heracles.Robot" is needed for starting from Heracles.Indoor project
                    var fileName = System.IO.Path.Combine(
                        System.IO.Path.Combine(projectDirectory, "Heracles.Robot"),
                        _robotPositionsXmlFileName);
                    if (File.Exists(fileName))
                    {
                        serializer.Save(fileName, PositionPresets);
                    }
                }
                catch { }
            }
        }

        public event EventHandler<EventArgs> PositionPresetsChanged;
    }
}
