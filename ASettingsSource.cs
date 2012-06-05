using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MVC;
using Exceptions;
namespace Config {
    public abstract class ASettingsSource : ANotifyingObject {
        private FileStream config_stream;
        private System.Threading.Mutex mutex = null;

        private string file_path = null;
        private string file_name = "config";
        protected string file_extension = null;
        protected string file_full_path {
            get {
                return Path.Combine(file_path, file_name + "." + file_extension);
            }
        }

        protected List<string> shared_settings = new List<string>();

        public ConfigMode mode { get; protected set; }

        private string app_name;

        public bool config_ready { get; protected set; }

        private bool enable_writing = true;

        public ASettingsSource(string app_name, ConfigMode mode) {

            this.mutex =  new System.Threading.Mutex(false, app_name);

            this.mode = mode;
            switch (mode) {
                case ConfigMode.PortableApps:
                    DirectoryInfo dir = new DirectoryInfo(Path.Combine("..", "..", "Data"));
                    if (!dir.Exists) {
                        dir.Create();
                        dir.Refresh();
                    }
                    file_path = dir.FullName;
                    break;
                case ConfigMode.AllUsers:
                    file_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), app_name);
                    break;
                case ConfigMode.SingleUser:
                    file_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), app_name);
                    break;
            }

            if (file_path != null) {
                // Load the settings from the file
                loadSettings();
            } else {
                throw new Exception("Could Not Determine Config File Location");
            }
        }


        public abstract List<string> read(Setting setting);

        // This erases all the existing settings with the new setting
        public List<string> overwrite(Setting setting, string value) {
            List<string> return_me = read(setting);
            erase(setting);
            write(setting, value);
            return return_me;
        }
        public bool overwrite(Setting setting, string old_value, string new_value) {
            bool result = erase(setting, old_value);
            write(setting, new_value);
            return result;
        }

        // This ADDS a setting to the stack
        public abstract void write(Setting setting, string value);

        // Erasing existing settings
        public abstract bool erase(Setting setting, string value);

        public abstract List<string> erase(Setting setting);

        #region config file stuff
        protected virtual void loadSettings() {
            lockFile();
            config_ready = false;
            if (config_watcher != null) {
                config_watcher.EnableRaisingEvents = false;
                config_watcher.Dispose();
                config_watcher = null;
            }
            for (int i = 0; i <= 5; i++) {
                // An optimistic initializer
                config_ready = true;
                try {
                    if (!Directory.Exists(file_path))
                        Directory.CreateDirectory(file_path);

                    if (!File.Exists(file_full_path)) {
                        createConfig(file_full_path);
                    }

                    config_stream = new FileStream(file_full_path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    break;
                } catch (Exception e) {
                    config_ready = false;
                    if (i < 5)
                        System.Threading.Thread.Sleep(100);
                    else
                        throw new WriteDeniedException(new FileInfo(file_full_path), e);
                } finally {
                    if (config_stream != null)
                        config_stream.Close();
                }
            }

            lock (config_stream) {
                config_stream = new FileStream(file_full_path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
                try {
                    config_ready = loadConfig(config_stream);
                } finally {
                    config_stream.Close();
                }
            }


            foreach (string setting in shared_settings) {
                NotifyPropertyChanged(setting);
            }
            setupWatcher();

            releaseFile();

        }
        protected bool writeConfig() {
            lockFile();
            lock (config_stream) {
                config_watcher.EnableRaisingEvents = false;
                try {
                    config_stream = new FileStream(file_full_path, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);

                    writeConfig(config_stream);
                } finally {
                    config_stream.Close();
                    config_watcher.EnableRaisingEvents = true;
                    releaseFile();
                }
            }
            return true;
        }

        protected abstract bool createConfig(string file_name);
        protected abstract bool loadConfig(FileStream stream);
        protected abstract bool writeConfig(FileStream stream);
        #endregion
        #region lock stuff
        protected void lockFile() {
            enable_writing = false;
            if (mutex != null)
                mutex.WaitOne();
        }

        protected void releaseFile() {
            if (mutex != null)
                mutex.ReleaseMutex();
            enable_writing = true;
        }
        #endregion
        #region config watcher stuff
        private FileSystemWatcher _config_watcher;
        protected FileSystemWatcher config_watcher {
            get {
                return _config_watcher;
            }
            set {
                _config_watcher = value;
            }
        }

        protected void setupWatcher() {
            if (config_watcher == null) {
                config_watcher = new FileSystemWatcher(file_path);
                config_watcher.Changed += new FileSystemEventHandler(config_watcher_Changed);
            }
            if (!config_watcher.EnableRaisingEvents)
                config_watcher.EnableRaisingEvents = true;
        }

        protected virtual void config_watcher_Changed(object sender, FileSystemEventArgs e) {
            if (e.ChangeType == WatcherChangeTypes.Changed && e.Name == file_name) {
                try {
                    loadSettings();
                } catch (Exception ex) {
                    throw ex;
                }
            }
        }
        #endregion

        protected bool ready {
            get {
                return config_ready;
            }
        }

    }
}
