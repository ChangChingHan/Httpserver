using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Httpserver
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            MicroHttpServer mhs = new MicroHttpServer(1688,
            (req) =>
            {
                byte[] bytes1 = new byte[1];
                Random rnd1 = new Random();
                rnd1.NextBytes(bytes1);
                ClientResponse cr = new ClientResponse()
                {
                      StringData = "Hi I am Lynn",
                      StatusText = ClientResponse.HttpStatus.Http200
                };

                cr.StringData = String.Format("Hi I am  Lynn, ID = {0}", bytes1[0]);
                
                return cr;
                
            });
            //mhs.Stop();
        }
    }
}
