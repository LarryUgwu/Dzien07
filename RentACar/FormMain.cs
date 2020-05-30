using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RentACar
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            tsInfo.Text = "Oczekuję na połączenie...";
        }

        private void mnuLogin_Click(object sender, EventArgs e)
        {
            FormLogin form = new FormLogin();
            //jezeli połączenie powiodło się
            if (form.ShowDialog() == DialogResult.OK)
            {
                tsInfo.Text = "Połączono z bazą danych";
                mnuCarRent.Enabled = true;
                //połączenie nie jest wieczne - może sie rozłączyc
                //np z braku netu, lub restartu serwera
            }
        }

        private void mnuCarList_Click(object sender, EventArgs e)
        {
            FormCarList form = new FormCarList();
            form.ShowDialog();
        }
    }
}
