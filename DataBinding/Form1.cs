using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataBinding
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        //tworzenie listy
        List<Person> persons = new List<Person>();

        private void Form1_Load(object sender, EventArgs e)
        {
            persons.Add(new Person("Jan", "Kowalski", 55, "Zdun", false));
            persons.Add(new Person("Marek", "Nowak", 35, "Social media ninja", true));
            persons.Add(new Person("Emil", "Zatopek", 66, "Biegacz", true));
            persons.Add(new Person("Zenek", "Martyniuk", 52, "Piosenkarz", false));

            //wyswietlenie danych w listboxie
            //foreach (Person item in persons)
            //{
            //    lbPersons.Items.Add(item.Fname + " " + item.Lname);
            //}

            //databinding zamiast foreach
            lbPersons.DataSource = persons;
            lbPersons.DisplayMember = "FullName";

            //databinding dla textboxów
            tbfName.DataBindings.Add(new Binding("Text", lbPersons.DataSource, "Fname"));
            tbfName.DataBindings.Add(new Binding("Enabled", lbPersons.DataSource, "Active"));

            tbLname.DataBindings.Add(new Binding("Text", lbPersons.DataSource, "Lname"));
            tbAge.DataBindings.Add(new Binding("Text", lbPersons.DataSource, "Age"));
            tbJob.DataBindings.Add(new Binding("Text", lbPersons.DataSource, "Job"));
        }

        private void lbPersons_SelectedIndexChanged(object sender, EventArgs e)
        {
            ////sprawdzam które nazwisko zostało wybrane
            //int index = lbPersons.SelectedIndex;
            //if (index >= 0) //zabezpieczenie
            //{
            //    //przypisanie, co ma sie wyswietalc w textboxach
            //    tbfName.Text = persons[index].Fname;
            //   tbLname.Text = persons[index].Lname;
            //    tbAge.Text = persons[index].Age.ToString();
            //    tbJob.Text = persons[index].Job;
            //}

        }
    }
}
