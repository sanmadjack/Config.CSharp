using System.Collections.Generic;

namespace Config {
    public class SettingsCollection : Dictionary<string, Setting> {

        public SettingsCollection() {
        }

        public void Add(Setting setting) {
            this.Add(setting.name, setting);
        }
    }
}
