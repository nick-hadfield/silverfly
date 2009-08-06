using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silverfly.Example
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            

            Window1 w = new Window1();
            w.Show();
            var application = new System.Windows.Application().Run(w);
            
            
        }
    }
}
