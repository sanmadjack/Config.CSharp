using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Config {
    public class SettingProperty: Setting {
        public SettingProperty(string name, object default_value, params string[] path):
        base (name, default_value,path) {

        }
    }
}
