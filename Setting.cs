using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Config {
    public class Setting {
        public ASettings settings;

        public string name { get; private set; }

        public List<string> path = new List<string>();
        private List<string> additional_notifications = new List<string>();
        public List<string> all_notifications {
            get {
                List<string> return_me = new List<string>();
                return_me.Add(name);
                return_me.AddRange(additional_notifications);
                return return_me;
            }
        }

        public string DefaultValue {
            get;
            protected set;
        }

        public Setting(string name, object default_value, params string[] path) {
            if (default_value == null)
                this.DefaultValue = null;
            else
                this.DefaultValue = default_value.ToString();
            this.name = name;
            foreach (string value in path) {
                this.path.Add(value);
            }
        }

        public void addAdditionalNotification(string name) {
            additional_notifications.Add(name);
        }

    }
}
