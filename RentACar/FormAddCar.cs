using MySql.Data.MySqlClient;
using RentACar.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RentACar
{
    public partial class FormAddCar : Form
    {
        //pole do przechowywania id_rekordu
        public int RowId = 0;
        public FormAddCar()
        {
            InitializeComponent();
        }

        private void FormAddCar_Load(object sender, EventArgs e)
        {
            LoadDictionaryData();

            //jesli uzupełnimy configapp, to możemy pobrac dane z konfiguracji
            //uwaga - pobrane dane to string, więc trzeba to konwertować
            //uwaga2 - trzeba się zabezpieczyc, przed wpisaniem bzdur w configu
            //numYear.Minimum = Convert.ToInt32(ConfigurationManager.AppSettings["minyear"]);

            try
            {
                if (RowId > 0)
                {
                    //wczytaj dane edytowanego rekordu i pokaż w kontrolkach
                    String sql = @"SELECT    	c.*,
                            	    		m.brand_id  
                                FROM 		cars c, 
                            			    car_models m 
                                WHERE 	    c.id = {0} 
                            	    AND 	c.model_id = m.id";
                    sql = String.Format(sql, RowId);

                    MySqlCommand cmd = new MySqlCommand(sql, GlobalData.connection);
                    //zapytanie zwraca jeden wiersz wieć uzyjemy datareader
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows) //sprawdzamy czy zapytanie zwróciło cokolwiek
                    {
                        reader.Read();
                        //odczyt danych z readera

                        numEngine.Value = Convert.ToInt32(reader["engine"]);
                        numYear.Value = Convert.ToInt32(reader["manufacturer_year"]);
                        tbRegPlate.Text = reader["registration_plate"].ToString();
                        cbFuel.SelectedIndex = cbFuel.Items.IndexOf(reader["fuel"]);

                        //selectedvalue, bo robilismy databinding
                        cbBrands.SelectedValue = reader["brand_id"];
                        cbModels.SelectedValue = reader["model_id"];
                        cbTypes.SelectedValue = reader["type_id"];
                        cbModels.Enabled = true;

                        //czy wartość reader od image nie jest dbnull - czyli czy odczytywana 
                        //z bazy danych wartość nie jest nullowa
                        if (!(reader["image"] is DBNull))
                        {
                            //image - sórowka - zawartośc image
                            byte[] b = (byte[])reader["image"];
                            //wszystkie dane mamy wymagane, ale obrazek jest w bazie opcjonalny
                            //więc trzeba zrobić if, aby apka nie wywaliła się
                            if (b != null && b.Length > 0)
                            {
                                //tworzenie strumienia
                                using (MemoryStream ms = new MemoryStream(b))
                                {   //tworzenie obiektu typu image przypisaywany do naszego obrazka w picture boxie
                                    picCar.Image = Image.FromStream(ms);
                                }
                            }
                        }

                        //readera trzeba zamknąć
                        reader.Close();
                    }
                }
            } catch (Exception exc)
            {
                DialogHelper.Error(exc.Message);
            }
            
            //zmiana napisów na przyciskach i oknie
            if (RowId > 0)
            {
                btnOk.Text = "Zapisz zmiany";
                Text = "Edycja pojazdu";

            }else
            {
                btnOk.Text = "Dodaj";
                Text = "Nowy pojazd";
            }
        }

        BindingSource bsBrands = new BindingSource();
        BindingSource bsModels = new BindingSource();
        BindingSource bsTypes = new BindingSource();

        private void LoadDictionaryData()
        {
            try
            {
                //ładownanie słownika marek
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                String sql = "select id, name from car_brands order by name";
                adapter.SelectCommand = new MySqlCommand(sql, GlobalData.connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                bsBrands.DataSource = dt;
                cbBrands.DataSource = bsBrands;
                cbBrands.DisplayMember = "name"; //to wyswietlamy uzytkownikowi
                cbBrands.ValueMember = "id";//identyfikator numeryczny
                cbBrands.SelectedIndex = -1;//komórka wyboru marki bedzie pusta
                cbBrands.SelectedIndexChanged += CbBrands_SelectedIndexChanged;

                //ładowanie slownika modeli mamy adapter, 
                //wiec nie musimy go tworzyć na nowo
                sql = "SELECT * FROM car_models ORDER BY name";
                adapter = new MySqlDataAdapter();
                adapter.SelectCommand = new MySqlCommand(sql, GlobalData.connection);
                dt = new DataTable();
                adapter.Fill(dt);

                bsModels.DataSource = dt;
                cbModels.DataSource = bsModels;
                cbModels.DisplayMember = "name";
                cbModels.ValueMember = "id";
                cbModels.SelectedIndex = -1;
                cbModels.Enabled = false;

                //ładowanie słownika własności
                sql = "SELECT * FROM car_types ORDER BY name";
                adapter = new MySqlDataAdapter();
                adapter.SelectCommand = new MySqlCommand(sql, GlobalData.connection);
                dt = new DataTable();
                adapter.Fill(dt);

                bsTypes.DataSource = dt;
                cbTypes.DataSource = bsTypes;
                cbTypes.DisplayMember = "name";
                cbTypes.ValueMember = "id";
                cbTypes.SelectedIndex = -1;
              

            }
            catch (Exception exc)
            {

                DialogHelper.Error(exc.Message);
            }
        }


        private void CbBrands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbBrands.SelectedIndex >-1)
            {
                bsModels.Filter = "brand_id=" + cbBrands.SelectedValue;
                cbModels.DataSource = bsModels;
                cbModels.Enabled = true;
                cbModels.SelectedIndex = -1;
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        //wielkie litery w miejscu tablicy rejestracyjnej (mozna też w propertis okienka dodac > przed aaaaaa)
        private void tbRegPlate_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Char.ToUpper(e.KeyChar);
        }

        //odpalamy okienko do wyboru pliku (zdjęcia)
        private String pictureFileName = null;
        private void btnInsertPic_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Pliki graficzne|*.png;*.jpg;*.jpeg;*.bmp|Pliki GIF|*.gif";
            if(dialog.ShowDialog()==DialogResult.OK)
            {
                //ładujemy plik 
                picCar.Image = new Bitmap(dialog.FileName);
                pictureFileName = dialog.FileName;
            }
        }

        //usuwanie obrazka
        private void btnRemovePic_Click(object sender, EventArgs e)
        {
            if (picCar.Image != null)
            {
                picCar.Image.Dispose();
                picCar.Image = null; //goodpractice
                pictureFileName = null; 
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (ValidateDate())
            {
                SaveDate();
            }
        }

        private bool ValidateDate()
        {//sprawdzamy czy wszystkie dane wypełniono i czy można zrobić save do bazy
           if(cbModels.SelectedIndex > -1 &&
                cbTypes.SelectedIndex > -1 &&
                cbFuel.SelectedIndex > -1 &&
                tbRegPlate.Text.Replace(" ","").Length >= 3)
            {
                return true;
            } else
            {
                DialogHelper.Error("Sprawdź pola formularza!");
                return false;
            }
        }

        private void SaveDate()
        {
            try
            {
                String sql = "";
                if (RowId > 0)
                {
                    sql = @"update cars
                            set     model_id = @model_id,
                                    type_id = @type_id,
                                    registration_plate = @reg_plate,
                                    engine = @engine,
                                    manufacturer_year = @year,
                                     image = @image,
                                    fuel = @fuel
                             where id=@row_id
                           ";
                }
                else
                {



                    sql = @"INSERT INTO cars
                                    (model_id, 
                                    type_id, 
                                    registration_plate, 
                                    engine, 
                                    manufacturer_year, 
                                    avail, 
                                    image, 
                                    fuel)
                            VALUES
                                    (@model_id, 
                                    @type_id, 
                                    @reg_plate, 
                                    @engine, 
                                    @year, 
                                    1, 
                                    @image, 
                                    @fuel)";
                }

                MySqlCommand cmd = new MySqlCommand(sql, GlobalData.connection);

                cmd.Parameters.Add("@model_id", MySqlDbType.Int32);
                cmd.Parameters.Add("@type_id", MySqlDbType.Int32);
                cmd.Parameters.Add("@reg_plate", MySqlDbType.VarChar, 50);
                cmd.Parameters.Add("@engine", MySqlDbType.Int32);
                cmd.Parameters.Add("@year", MySqlDbType.Int32);
                cmd.Parameters.Add("@fuel", MySqlDbType.VarChar, 10);
                cmd.Parameters.Add("@image", MySqlDbType.MediumBlob);
                cmd.Parameters.Add("@row_id", MySqlDbType.Int32);

                cmd.Parameters["@model_id"].Value = cbModels.SelectedValue;
                cmd.Parameters["@type_id"].Value = cbTypes.SelectedValue;
                cmd.Parameters["@reg_plate"].Value = tbRegPlate.Text.Trim();
                cmd.Parameters["@year"].Value = numYear.Value;
                cmd.Parameters["@engine"].Value = numEngine.Value;
                cmd.Parameters["@fuel"].Value = cbFuel.SelectedItem; //bo paliwo mamy zdefiniowane jako kolekcje (wpisywaliśmy z lapy listę on, lpg, pb)
                cmd.Parameters["@row_id"].Value = RowId;

                //jesli plik jest
                if (pictureFileName != null && File.Exists(pictureFileName))
                {
                    cmd.Parameters["@image"].Value = File.ReadAllBytes(pictureFileName);

                }
                else //jesli nie ma
                {
                    cmd.Parameters["@image"].Value = null;
                }
                //wywołanie zapytania
                cmd.ExecuteNonQuery();

                DialogResult = DialogResult.OK;
                Close();
            } catch (Exception exc)
            {
                DialogHelper.Error(exc.Message);
            }
        }
    }
}
