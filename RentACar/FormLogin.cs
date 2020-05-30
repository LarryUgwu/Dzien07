using MySql.Data.MySqlClient;
using RentACar.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RentACar
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnlogin_Click(object sender, EventArgs e)
        {
            //aby sie podpiąc do bazy wpierw nalezy dodać referencje mysql

            // String cs = "Server=127.0.0.1;Port=3306;Database=rent_a_car;Uid={0};Pwd={1}";

            //to powyżej to hardskodowane połączenie do bazy - tak sie nie robi! bo np
            //przeniesiono baze na inny serwer
            //trzeba użyć plik konfiguracyjny (app.config)
            //do refeerencji musimy dodać system.configuration i robimy tak:

            String cs = ConfigurationManager.AppSettings["cs"];

            try
            {
                if(String.IsNullOrWhiteSpace(tbLogin.Text) ||
                    String.IsNullOrWhiteSpace(tbPassword.Text))
                {
                    DialogHelper.Error("Podaj dane logowania");
                    return;
                }

                //zmiana kursora na czas podłączania do bazy
                Cursor.Current = Cursors.WaitCursor;
                //dotyczy bieżacego okna, więc kursor wróci do bazowej wersji przy zamkniecuy okienka

                cs = String.Format(cs, tbLogin.Text.Trim(), tbPassword.Text.Trim());
   
                //MySqlConnection connection = new MySqlConnection(cs);
                //connection.Open();
                //po utworzeniu globaldata, możemy wywalić powyższe linijki i 
                //dodajemy dwie poniższe linijki
                GlobalData.connection = new MySqlConnection(cs);
                GlobalData.connection.Open();

                //przed zamknięciem okna zwrócmy kod wyjścia i przekazać go do formy głównej w pasku na dole formy
                this.DialogResult = DialogResult.OK;
                Close();


                //połączenie mamy tylko z formy login, więc jej zamknięcie
                //spowoduje rozłączenie z bazą. trzeba zrobić "kontener" na
                //połączenie globalne (klasę statyczną) - folder utils a tam 
                //klase pomocniczą - GlobalData

            }
            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message);

                //Po zrobieniu klasy pomocniczej dialog helper możemy wywalic powyższe a dodać poniższy kod

                DialogHelper.Error(exc.Message);
            }
            finally 
            {
                //przywrócenie domyslnego kursora 
                Cursor.Current = Cursors.Default;
            }
        }
    }
}
