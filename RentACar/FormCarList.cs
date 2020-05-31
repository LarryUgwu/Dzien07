using MySql.Data.MySqlClient;
using RentACar.Utils;
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
    public partial class FormCarList : Form
    {
        public FormCarList()
        {
            InitializeComponent();
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        //definiujemy pole (uzywamy binding source aby móc filtrowac dane
        BindingSource bSource = new BindingSource();


        private void FormCarList_Load(object sender, EventArgs e)
        {
            RefreshData();

            //customizacja grida, np zmiana nazw kolumn

            grid.Columns["id"].HeaderText = "ID";
            grid.Columns["brand"].HeaderText = "Marka";
            grid.Columns["model"].HeaderText = "Model";
            grid.Columns["car_type"].HeaderText = "Własność";
            grid.Columns["registration_plate"].HeaderText = "Nr rejestracyjny";
            grid.Columns["engine"].HeaderText = "Pojemność [cm3]";
            grid.Columns["engine"].DefaultCellStyle.Alignment = 
                DataGridViewContentAlignment.MiddleRight;
            grid.Columns["manufacturer_year"].HeaderText = "Rok produkcji";
            grid.Columns["manufacturer_year"].DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

            grid.Columns["avail"].HeaderText = "Dostępny";
            grid.Columns["avail"].DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

         

        }

        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //chcemy modyfikowac te kolumne, ale mamy włączoną możliwośc przestawiania
            //kolumn
            if (e.ColumnIndex == grid.Columns["avail"].Index)
            { 
                //ternary operator zamiast kolejnego if`a
                e.Value = (Convert.ToInt32(e.Value) == 1) ? "TAK" : "NIE";
            }
        }

        //usuwanie wiersza
        private void menuDelCar_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return;
            DialogResult res =  MessageBox.Show("Czy na pewno usunąć rekord?", "Pytanie",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (res != DialogResult.Yes) return;

            String sql = "DELETE FROM cars Where id= @rowId";
            //informujemy, że to co ma byc parametrem jest intem, aby zapobiec np
            //wstrzyknięciu stringa "drop database"
            using (MySqlCommand deleteCommand = new MySqlCommand(sql, GlobalData.connection))
            {   //jesli dodamy co innego niż int - będzie wyjątek
                deleteCommand.Parameters.Add("@rowId", MySqlDbType.Int32);


                int selectedIndex = grid.SelectedRows[0].Index;
                deleteCommand.Parameters["@rowId"].Value = grid["id", selectedIndex].Value;

                //odpalamy zapytanie, które nie zwraca danych (np select zwraca)
                //dlatego "executenonquery"
                deleteCommand.ExecuteNonQuery();

                //usunelismy rekord z bazy danych, to usuńmy też rekord z grida, aby nie
                //było konieczne odświeżanie zapytania select
                grid.Rows.RemoveAt(selectedIndex);

            }
            //good practice to stosowanie context manager
            //zwalnianie zasobów!
            //czyli w 80 dodajemy using i klamry w 81 i 96

            

        }

        //odświeżenie danych w gridviev
        private void RefreshData()
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            String sql = @"SELECT 	c.id,
                        			    b.name AS brand,
                        			    m.name AS model,
                        			    t.name AS car_type,
                        			    c.registration_plate,
                        			    c.engine,
                        			    c.manufacturer_year,
                        			    c.avail,
                        			    c.fuel
                            FROM		cars c, car_models m, car_types t, car_brands b
                            WHERE		c.model_id = m.id
                        	    AND	c.type_id = t.id
                        	    AND	m.brand_id = b.id";
            adapter.SelectCommand = new MySqlCommand(sql, GlobalData.connection);

            DataTable dt = new DataTable();
            adapter.Fill(dt);
            //można tu skończyć, ale nie bedzie można wyszukiwac 
            //trzeba dodać linię 28 i to co poniżej

            bSource.DataSource = dt;
            grid.DataSource = bSource;

        }

        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void mnuRefresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void tsbInsert_Click(object sender, EventArgs e)
        {
            AddNewCar(); //robimy na to metode, bo auto bedziemy dodawać w dwóch miejscach
        }

        private void AddNewCar()
        {
            FormAddCar form = new FormAddCar();
            if (form.ShowDialog() == DialogResult.OK)
            {
                RefreshData();
            }
            
        }

        private void mnuEditCar_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return; //jesli kolekcja jest pusta to nic nie moge edytowac

            int selectedIndex = grid.SelectedRows[0].Index;
            int rowId = Convert.ToInt32(grid["id", selectedIndex].Value);

            FormAddCar form = new FormAddCar();
            form.RowId = rowId;
            if (form.ShowDialog() == DialogResult.OK) 
            {
                RefreshData();
            }
        }

        private void mnuCarOper_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return;

            int selectedIndex = grid.SelectedRows[0].Index;
            int rowId = Convert.ToInt32(grid["id", selectedIndex].Value);
            String regplate = grid["registration_plate", selectedIndex].Value.ToString();
            //czy auto jest dost czy nie
            int avail = Convert.ToInt32(grid["avail", selectedIndex].Value);

            FormOperation form = new FormOperation();
            form.CarId = rowId;
            form.RegPlate = regplate;
            form.OperBack = (avail == 0);

            if(form.ShowDialog() == DialogResult.OK)
            {
                RefreshData();
            }
        }
    }
}
