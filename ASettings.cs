using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVC;
namespace Config {
    public abstract class ASettings: ANotifyingObject {
        private ASettingsSource source;

        private SettingsCollection settings;

        public ASettings(ASettingsSource source) {
            this.source = source;
            this.settings = createSettings(new SettingsCollection());
        }

        protected abstract SettingsCollection createSettings(SettingsCollection settings);

        public string get(string name) {
            if (settings.ContainsKey(name)) {
                return source.read(settings[name]);
            } else {
                throw new KeyNotFoundException(name);
            }
        }

        public string set(string name, string value) {
            if (settings.ContainsKey(name)) {
                string old = source.read(settings[name]);
                if (old != value) {
                    old = source.write(settings[name], value);
                    NotifyPropertyChanged(name);
                    return old;
                } else {
                    return old;
                }
            } else {
                throw new KeyNotFoundException(name);
            }
        }


    }
}
