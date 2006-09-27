using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Tangerine.src {
    public partial class QuitDialog : Form {

        public bool NoAsk {
            get { return dontAskCheck.Checked; }
        }

        public QuitDialog () {
            InitializeComponent ();
        }
    }
}