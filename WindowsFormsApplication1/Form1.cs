﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            var dt = list.ToDataTable();
            Debug.WriteLine(dt.Rows.Count);
        }
    }


    public class BClass
    {
        public string ID { get; set; }
    }

    public class AClass : BClass
    {
        public string Name { get; set; }
    }
}