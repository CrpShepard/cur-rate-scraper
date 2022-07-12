using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CsvHelper;
using HtmlAgilityPack;
using System.IO;
using System.Globalization;

namespace Currency_Parser
{
    public partial class Form_CurrencyRate : Form
    {
        public Form_CurrencyRate()
        {
            InitializeComponent();
        }

        static HtmlAgilityPack.HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(url);
            return doc;
        }

        void loadText(string url)
        {
            HtmlAgilityPack.HtmlDocument doc = GetDocument(url);
            var currency = new List<CRRow>();
            
            var col = doc.DocumentNode.SelectNodes("//table//tr//td");
            
            for (int i = 0; i < col.Count; i += 3)
            {
                currency.Add(new CRRow { ISO = col[i].InnerText, Currency = col[i + 1].SelectNodes("a")[0].InnerText });
            }

            using (var writer = new StreamWriter("Currencies.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(currency);
            }

            using (var reader = new StreamReader("Currencies.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<CRRow>();
                foreach (var item in records)
                {
                    comboBox1.Items.Add(item.ISO + " - " + item.Currency);
                    comboBox2.Items.Add(item.ISO + " - " + item.Currency);
                }
            }
        }

        void loadRate(string url)
        {
            HtmlAgilityPack.HtmlDocument doc = GetDocument(url);
            string cur1 = comboBox1.Text.Substring(0, 3);
            string cur2 = comboBox2.Text.Substring(0, 3);

            label4.Text = textBox1.Text + " " + cur1 + " = " +
                doc.DocumentNode.SelectNodes("//p[@class='result__BigRate-sc-1bsijpp-1 iGrAod']/text()")[0].InnerText.Trim() +
                " " + cur2;
            label4.Visible = true;
        }

        private void Form_CurrencyRate_Load(object sender, EventArgs e)
        {
            loadText("https://www.worlddata.info/currencies/");

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;

            comboBox1.SelectedIndex = comboBox1.FindString("USD");
            comboBox2.SelectedIndex = comboBox2.FindString("EUR");
        }

        private void button1_Click(object sender, EventArgs e) //Convert
        {
            if (textBox1.Text.Length > 0)
            {
                label3.Visible = false;
                string cur1 = comboBox1.Text.Substring(0, 3);
                string cur2 = comboBox2.Text.Substring(0, 3);
                loadRate("https://www.xe.com/currencyconverter/convert/?Amount=" + textBox1.Text +
                    "&From=" + cur1 + "&To=" + cur2);
            }
            else
            {
                label3.Visible = true;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) //Only numbers and one dot
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
                label3.Visible = false;
            }

            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e) //Swap
        {
            var temp = comboBox1.SelectedItem;
            comboBox1.SelectedItem = comboBox2.SelectedItem;
            comboBox2.SelectedItem = temp;
        }
    }

    public class CRRow
    {
        public string ISO { get; set; }
        public string Currency { get; set; }
    }
}