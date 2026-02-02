using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Sudoku_Projesi
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        SqlConnection cnt = new SqlConnection("Data Source=DESKTOP-4QHAJ0K\\SQLEXPRESS;Initial Catalog=sudokubasarilistesi;Integrated Security=True");
        private void Form2_Load(object sender, EventArgs e)
        {
            //SqlDataAdapter da = new SqlDataAdapter("select *from veriler", cnt);
            //DataSet ds = new DataSet();
            //da.Fill(ds);

            //dataGridView1.DataSource = ds.Tables[0];
            //dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            // Karmaşık sıralama sorgusu: 
            // 1. Önce Seviye (Zor > Orta > Kolay)
            // 2. Sonra Süre (En düşük saniye en üstte)

            string query = @"
                SELECT ad AS [Oyuncu Adı], 
                       seviye AS [Zorluk Seviyesi], 
                       checksayisi AS [Kontrol Sayısı], 
                       süre AS [Bitirme Süresi], 
                       tarih AS [Oynanma Tarihi] 
                FROM veriler 
                ORDER BY 
                    CASE 
                        WHEN seviye = 'Hard' THEN 1 
                        WHEN seviye = 'Medium' THEN 2 
                        WHEN seviye = 'Easy' THEN 3 
                        ELSE 4 
                    END ASC, 
                    CAST(REPLACE(süre, ' Saniye', '') AS INT) ASC";

            SqlDataAdapter da = new SqlDataAdapter(query, cnt);
            DataSet ds = new DataSet();
            da.Fill(ds);

            dataGridView1.DataSource = ds.Tables[0];

            // Görsel Ayarlar
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Tabloyu forma yayar
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Satırı komple seçer
            dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically; // Kullanıcı elle değiştiremez
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Eğer satır indeksi geçersizse veya sütun "Zorluk Seviyesi" değilse işlem yapma
            if (e.RowIndex < 0 || dataGridView1.Columns[e.ColumnIndex].Name != "Zorluk Seviyesi")
                return;

            if (e.Value != null)
            {
                string seviye = e.Value.ToString();
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                if (seviye == "Hard")
                {
                    row.DefaultCellStyle.BackColor = Color.Firebrick; // Koyu Kırmızı
                    row.DefaultCellStyle.ForeColor = Color.White;    // Beyaz Yazı
                }
                else if (seviye == "Medium")
                {
                    row.DefaultCellStyle.BackColor = Color.Goldenrod; // Turuncu/Altın
                    row.DefaultCellStyle.ForeColor = Color.Black;    // Siyah Yazı
                }
                else if (seviye == "Easy")
                {
                    row.DefaultCellStyle.BackColor = Color.ForestGreen; // Yeşil
                    row.DefaultCellStyle.ForeColor = Color.White;      // Beyaz Yazı
                }
            }
        }
    }
}
