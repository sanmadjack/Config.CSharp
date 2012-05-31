using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Config {
    public class ListSetting: ASetting<List<String>> {
        private char delimiter;
        public ListSetting(string name, char delimiter, params string[] path) :
                base(name, null, path) {
                    this.delimiter = delimiter;
        }

        public override List<String> parse(string value) {
            if(value==null)
                return null;

            List<String> return_me = new List<string>();
            if (value.Contains(delimiter)) {
                return_me.AddRange(value.Split(delimiter));
            } else {
                return_me.Add(value);
            }
            return return_me;
        }
    }
}
