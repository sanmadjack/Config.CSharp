using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVC;
namespace Config {
    public class Setting {
        public string name;
        private Setting parent;
        private Dictionary<string, string> properties;
        private SettingsCollection children;

        public string DefaultValue {
            get;
            protected set;
        }

        public Setting(string name): this(name,null) { }
        public Setting(string name, string default_value) {
            this.name = name;
            this.DefaultValue = default_value;
        }
    }
}
