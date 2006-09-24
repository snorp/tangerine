using System;

namespace Tangerine {

    public class Provider {

        private string name;
        private string plugin;
        
        public string Name {
            get { return name; }
        }

        public string Plugin {
            get { return plugin; }
        }

        public Provider (string name, string plugin) {
            this.name = name;
            this.plugin = plugin;
        }

        public override string ToString () {
            return name;
        }
    }
}
