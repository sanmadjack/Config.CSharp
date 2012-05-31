using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Config {
    public abstract class ASetting<T> : ASetting {
        public T DefaultValue {
            get;
            protected set;
        }

        public ASetting(string name, T default_value, params string[] path) :
            base(name, path) {

            this.DefaultValue = default_value;
        }

        protected override Type getType() {
            return DefaultValue.GetType();
        }

    }

    public abstract class ASetting {
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

        public Type type {
            get {
                return getType();
            }
        }


        protected ASetting(string name, params string[] path) {
            this.name = name;
            foreach (string value in path) {
                this.path.Add(value);
            }
        }

        protected abstract Type getType();

        public void addAdditionalNotification(string name) {
            additional_notifications.Add(name);
        }

        public object get() {
            return settings.
        }

        public object set() {
        }

        public abstract object parse(string value);

    }
}
