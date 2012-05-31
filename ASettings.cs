using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MVC;
namespace Config {
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
                return ready;
            }
        }

        public ASettings(ASettingsSource source) {
            this.source = source;
            this.settings = createSettings(new SettingsCollection());
            processSettings();
        }


        protected abstract void processSettings();

        public string get(string name) {
            if (settings.ContainsKey(name)) {
                return source.read(settings[name]);
            } else {
                throw new KeyNotFoundException(name);
            }
        }

        public string erase(string name) {
            if (settings.ContainsKey(name)) {
                ASetting setting = this.settings[name];
                string old = source.read(setting);
                if (old != null) {
                    foreach (string notify in setting.all_notifications) {
                        NotifyPropertyChanged(notify);
                    }
                    return source.erase(setting);
                }
                return old;
            } else {
                throw new KeyNotFoundException(name);
            }
        }

        public string set(string name, string value) {
            if (settings.ContainsKey(name)) {
                ASetting setting = this.settings[name];
                string old = source.read(setting);
                if (old != value) {
                    old = source.write(settings[name], value);
                    foreach (string notify in setting.all_notifications) {
                        NotifyPropertyChanged(notify);
                    }
                    return old;
                } else {
                    return old;
                }
            } else {
                throw new KeyNotFoundException(name);
            }
        }


        #region Generic, everything could use these kinds of settings
        protected virtual SettingsCollection createSettings(SettingsCollection settings) {
            settings.Add(new StringSetting("email", null, "email"));

            settings.Add(new StringSetting("last_drive", current_drive, "portable_settings", "last_drive"));

            return settings;
        }

        #region e-mail related
        public string email {
            get {
                return get("email");
            }
            set {
                if (value != null && value.Contains("@")) {
                    int loc = value.IndexOf('@');
                    if (value.Substring(loc + 1).Contains("."))
                        set("email", value);
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
                return get("last_drive");
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
