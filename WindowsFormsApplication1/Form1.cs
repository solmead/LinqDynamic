using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var list = new List<AClass>();
            for (int a = 0; a < 100; a++)
            {
                list.Add(new AClass() {ID=a.ToString(),Name="Name" + a});
            }

            var dt = list.ToDataTable(true);
            Debug.WriteLine(dt.Rows.Count);
        }
    }


    public class BClass
    {
        public string ID { get; set; }
        [Display(Name = "RefID",AutoGenerateField = false)]
        public string RefID { get; set; }
    }

    public class AClass : BClass
    {
        [Display(Name="A Class Name")]
        public string Name { get; set; }
        [Display(Name = "A Class Description")]
        public string Description { get; set; }
        [Display(Name = "RefID 2", AutoGenerateField = false)]
        public string RefID2 { get; set; }
    }
}
