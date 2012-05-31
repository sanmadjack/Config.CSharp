using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Config {
    public class SettingsCollection: Dictionary<string,ASetting> {
        private ASettings settings;

        public SettingsCollection(ASettings settings) {
            this.settings = settings;
        }

        public void Add(ASetting setting) {
            setting.settings = this.settings;
            this.Add(setting.name, setting);
        }
    }
}
