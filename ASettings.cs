using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MVC;
namespace Config {
    public enum WindowState {
        Normal,
        Minimized,
        Maximized,
        Iconified
    }

    public abstract class ASettings: ANotifyingObject {

        private ASettingsSource source;
        public ConfigMode mode {
            get {
                return source.mode;
            }
        }

        private SettingsCollection settings;

        public bool IsReady {
            get {
                return source.config_ready;
            }
        }

        public ASettings(ASettingsSource source) {
            this.source = source;
            this.settings = createSettings(new SettingsCollection());
            processSettings();
        }


        protected abstract void processSettings();

        private Setting keyTest(string name) {
            if (!settings.ContainsKey(name)) {
                throw new KeyNotFoundException(name);
            } else {
                return settings[name];
            }
        }

        public List<string> get(string name) {
            Setting setting = keyTest(name);

            List<string> result = source.read(setting);
            if(result.Count==0) {
                result = new List<string>();
                if (setting.DefaultValue != null) {
                    result.Add(setting.DefaultValue);
                }
            }
            return result;
        }

        public string getLast(string name) {
            List<string> list = get(name);
            if (list.Count > 0) {
                return list[list.Count - 1];
            } else {
                return null;
            }
        }
        public int getLastInteger(string name) {
            string value = getLast(name);
            if (value == null)
                value = "0";
            return Int32.Parse(value);
        }

        public bool getLastBoolean(string name) {
            return Boolean.Parse(getLast(name));
        }

        public List<string> erase(string name) {
            Setting setting = keyTest(name);
            
            List<string> old = source.erase(setting);
            if (old.Count > 0) {
                source.save();
                NotifyPropertyChanged(setting);
            }
            return old;
        }

        public void set(string name, object value) {
            Setting setting = keyTest(name);

            List<string> old = source.overwrite(setting, value.ToString());
            if(old.Count != 1 || old[0] != value.ToString()) {
                source.save();
                NotifyPropertyChanged(setting);
            }
        }

        public void add(string name, object value) {
            Setting setting = keyTest(name);
            source.write(setting, value.ToString());
            source.save();
            NotifyPropertyChanged(setting);
        }

        public bool addUnique(string name, object value) {
            Setting setting = keyTest(name);
            if(!source.read(setting).Contains(value.ToString())) {
                source.write(setting, value.ToString());
                source.save();
                NotifyPropertyChanged(setting);
                return true;
            }
            return false;
        }

        public bool remove(string name, string value) {
            Setting setting = keyTest(name);
            bool result = source.erase(setting, value);
            if (result) {
                source.save();
                NotifyPropertyChanged(setting);
            }
            return result;
        }

        private void NotifyPropertyChanged(Setting setting) {
            foreach (string notify in setting.all_notifications) {
                NotifyPropertyChanged(notify);
            }
        }

        #region Generic, everything could use these kinds of settings
        protected virtual SettingsCollection createSettings(SettingsCollection settings) {
            settings.Add(new Setting("EmailSender", null, "email", "sender"));
            settings.Add(new Setting("EmailRecipient", null, "email", "recipient"));
            settings.Add(new Setting("last_drive", current_drive, "portable_settings", "last_drive"));
            settings.Add(new Setting("WindowX", null, "window", "x"));
            settings.Add(new Setting("WindowY", null, "window", "y"));
            settings.Add(new Setting("WindowW", null, "window", "h"));
            settings.Add(new Setting("WindowH", null, "window", "w"));
            settings.Add(new Setting("WindowState", null, "window", "state"));

            return settings;
        }


        #region Window-related
        public int WindowX {
            get {
                return getLastInteger("WindowX");
            }
            set {
                set("WindowX", value);
            }
        }
        public int WindowY {
            get {
                return getLastInteger("WindowY");
            }
            set {
                set("WindowY", value);
            }
        }
        public int WindowW {
            get {
                return getLastInteger("WindowW");
            }
            set {
                set("WindowW", value);
            }
        }
        public int WindowH {
            get {
                return getLastInteger("WindowH");
            }
            set {
                set("WindowH", value);
            }
        }
        public WindowState WindowState {
            get {
                string value = getLast("WindowState");
                if (value == null)
                    return Config.WindowState.Normal;
                return (WindowState)Enum.Parse(typeof(WindowState), value);
            }
            set {
                set("WindowState", value);
            }
        }

        #endregion

        #region e-mail related
        public string EmailSender {
            get {
                return getLast("EmailSender");
            }
            set {
                if (value != null && value.Contains("@")) {
                    int loc = value.IndexOf('@');
                    if (value.Substring(loc + 1).Contains("."))
                        set("EmailSender", value);
                }
            }
        }
        public string EmailRecipient {
            get {
                return getLast("EmailRecipient");
            }
            set {
                if (value != null && value.Contains("@")) {
                    int loc = value.IndexOf('@');
                    if (value.Substring(loc + 1).Contains("."))
                        set("EmailRecipient", value);
                }
            }
        }
        #endregion

        #region Portable apps-related
        public string current_drive {
            get {
                DirectoryInfo dir = new DirectoryInfo("./");
                return dir.Root.Name;
            }
        }
        public string last_drive {
            get {
                return getLast("last_drive");
            }
        }
        public void updateLastDrive() {
            set("last_drive", current_drive);
        }
        protected string adjustPortablePath(string old_path) {
            if (last_drive != current_drive && old_path.StartsWith(last_drive)) {
                string value = old_path.Replace(last_drive, current_drive);
                return value;
            }
            return old_path;
        }
        #endregion

        #endregion
    }
}
