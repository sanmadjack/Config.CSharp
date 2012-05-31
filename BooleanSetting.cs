using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Config {
    public class BooleanSetting: ASetting<Boolean> {
        public BooleanSetting(string name, Boolean default_value, params string[] path) :
                base(name, default_value,path) {
        }

        public override Boolean parse(string value) {
            return Boolean.Parse(value);
        }
    }
}
