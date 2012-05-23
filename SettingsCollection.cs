using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Config {
    public class SettingsCollection: Dictionary<string,Setting> {

        public void Add(Setting setting) {
            this.Add(setting.name, setting);
        }
    }
}
