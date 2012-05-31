using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Config {
    public class StringSetting: ASetting<String> {
        public StringSetting(string name, String default_value, params string[] path) :
                base(name, default_value,path) {
        }

        public override String parse(string value) {
            return value;
        }
    }
}
