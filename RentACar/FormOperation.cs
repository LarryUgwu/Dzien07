using MySql.Data.MySqlClient;
using RentACar.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xceed.Words.NET;

namespace RentACar
{
    public partial class FormOperation : Form
    {
        //dodajemy property
        //id rekordu z bazy, czyli na ktorym samochódzie operujemy
        public int CarId { get; set; } = 0; //zero = domyslna wartość
        //nr rej
        public String RegPlate { get; set; } = "";
        //rodzaj operacji
        public bool OperBack { get; set; } = false; 
        public FormOperation()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        //pole
        int lastRecordId;
        private void FormOperation_Load(object sender, EventArgs e)
        {
            if (OperBack)
            {
                Text = "Zwrot pojazdu " + RegPlate;
                String sql = @"select id, description from operations
                    where car_id = {0} order by id desc limit 0,1";

                MySqlDataAdapter adapter = new MySqlDataAdapter();
                sql = String.Format(sql, CarId);
                adapter.SelectCommand = new MySqlCommand(sql, GlobalData.connection);

                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if(dt.Rows.Count > 0)
                {
                    tbDescr.Text = dt.Rows[0]["description"].ToString();
                    lastRecordId = Convert.ToInt32(dt.Rows[0]["id"]);
                }


            } else
            {
                //wydanie pojazdu
                Text = "Wydanie pojazdu " + RegPlate;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {

            // document
            String filename = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "rent-a-car.docx");
            DocX document = DocX.Load(filename);
            document.ReplaceText("{REG_PLATE}", RegPlate);
            document.ReplaceText("{MILAGE}", numMileage.Value.ToString());
            document.ReplaceText("{TS}", dtDate.Value.ToString("yyyy-MM-dd HH:mm"));

            if(OperBack)
            {
                document.ReplaceText("DOC_TYPE", "Zwrot pojazdu");
            } else
            {
                document.ReplaceText("DOC_TYPE", "Wydanie pojazdu");
            }
            filename = Path.Combine(@"c:\tmp", RegPlate + " -" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".docx");
            document.SaveAs(filename);

            MySqlTransaction tr = null;
            try
            {   //transakcja
                tr = GlobalData.connection.BeginTransaction();
                
                    
                    
                String sql;
                int avail;
                if (OperBack == false)
                {
                    //wydaj auto i dodaj rekord
                    sql = @"insert into operations 
                        (car_id, ts_out, mileage_out, description)
                        values
                        (@car_id, @ts, @mileage, @descr)";

                    avail = 0;
                }
                else
                {
                    //przyjmij auto - zaktualizuj ostatni rekord operacji dla auta
                    sql = @"update operations set
                        ts_in = @ts,
                        mileage_in = @mileage,
                        description = @descr
                        where id=@id";
                    avail = 1;
                }
                MySqlCommand cmd = new MySqlCommand(sql, GlobalData.connection);
                cmd.Transaction = tr;
                cmd.Parameters.Add("@car_id", MySqlDbType.Int32).Value = CarId; //.value pozwala przypisać wartość odrazu przy tworzeniu parametru
                cmd.Parameters.Add("@ts", MySqlDbType.DateTime).Value = dtDate.Value;
                cmd.Parameters.Add("@mileage", MySqlDbType.Int32).Value = numMileage.Value;
                cmd.Parameters.Add("@descr", MySqlDbType.Text).Value = tbDescr.Text;
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = lastRecordId;

                cmd.ExecuteNonQuery();

                sql = "update cars set avail=@avail where id=@id";

                cmd = new MySqlCommand(sql, GlobalData.connection);
                cmd.Transaction = tr; //podpinamy transakcje pod komende
                cmd.Parameters.Add("@avail", MySqlDbType.Int32).Value = avail;
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = CarId;
                cmd.ExecuteNonQuery();

                tr.Commit();

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exc)
            {
                if(tr != null)
                {
                    tr.Rollback();
                }
                DialogHelper.Error(exc.Message);
            }
        }
    }
}
