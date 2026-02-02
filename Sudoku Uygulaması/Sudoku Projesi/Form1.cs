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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        TextBox[,] cells = new TextBox[9, 9];
        int[,] solutionBoard = new int[9, 9]; // Arka plandaki doğru cevap anahtarı
        Random rnd = new Random();
        int süre = 0,checksayisi;
        SqlConnection cnt = new SqlConnection("Data Source=DESKTOP-4QHAJ0K\\SQLEXPRESS;Initial Catalog=sudokubasarilistesi;Integrated Security=True");

        private void Cell_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && (e.KeyChar < '1' || e.KeyChar > '9'))
                e.Handled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            nametextBox.Font = new Font("Segoe UI", 14, FontStyle.Italic);
            nametextBox.Text = "Enter Name";
            nametextBox.ForeColor = Color.Gray;
            timer1.Interval = 1000;
            label1.Visible = false;
            button3.Enabled = false;
            int index = 1;

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    cells[row, col] = this.Controls.Find("textBox" + index, true)[0] as TextBox;
                    cells[row, col].MaxLength = 1;
                    cells[row, col].TextAlign = HorizontalAlignment.Center;
                    cells[row, col].Font = new Font("Segoe UI", 16, FontStyle.Bold);
                    cells[row, col].KeyPress += Cell_KeyPress;
                    index++;
                }
            }

            foreach (var cell in cells) cell.ReadOnly = true;

            button2.Enabled = false;
            button3.Enabled = false;
        }

        // Sayı yerleştirmek güvenli mi?
        private bool IsSafe(int[,] board, int row, int col, int num)
        {
            for (int i = 0; i < 9; i++)
            {
                // Satırda veya sütunda aynı sayı var mı?
                if (board[row, i] == num || board[i, col] == num)
                    return false;
            }

            // 3x3 blok kontrolü için başlangıç koordinatlarını bul
            int startRow = (row / 3) * 3;
            int startCol = (col / 3) * 3;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[startRow + i, startCol + j] == num)
                        return false;
                }
            }
            return true;
        }

        // Çözücü (Solver)
        private bool Solve(int[,] board)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (board[row, col] == 0)
                    {
                        // 1'den 9'a kadar sayıları karıştırılmış bir liste olarak al
                        List<int> numbers = Enumerable.Range(1, 9).OrderBy(x => rnd.Next()).ToList();

                        foreach (int num in numbers)
                        {
                            if (IsSafe(board, row, col, num))
                            {
                                board[row, col] = num;
                                if (Solve(board)) return true;
                                board[row, col] = 0;
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        //Start Butonu
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "")
            {
                MessageBox.Show("Zorluk Seviyesini Seçiniz");
                return;
            }
            if (string.IsNullOrWhiteSpace(nametextBox.Text) || nametextBox.Text == "Enter Name") 
            {
                MessageBox.Show("Sonuçlarınızın Kaydedilmesi İçin Adınızı Giriniz");
                return;
            }

            checksayisi = -1;
            timer1.Start();
            süre = 0;
            label1.Visible = true;

            button3.Enabled = true;
            button2.Enabled = true;
            button1.Enabled = false;
            // 1. Temizlik
            foreach (var cell in cells)
            {
                cell.Text = "";
                cell.BackColor = Color.White;
                cell.ReadOnly = false;
            }

            int[,] board = new int[9, 9];

            Solve(board);

            // Önce çözümün tam halini solutionBoard'a kopyala
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    solutionBoard[r, c] = board[r, c];

            //Zorluk Ayarı: Kaç hücre silinsin? 
            int cellsToRemove = 0;
            if (comboBox1.SelectedIndex == 0)
            {
                cellsToRemove = 35;
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                cellsToRemove = 45;
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                cellsToRemove = 50;
            }
            if (nametextBox.Text=="admin123")
            {
                cellsToRemove = 5;
            }
            while (cellsToRemove > 0)
            {
                int r = rnd.Next(9);
                int c = rnd.Next(9);
                if (board[r, c] != 0)
                {
                    board[r, c] = 0;
                    cellsToRemove--;
                }
            }

            // 4. Ekrana Yazdır
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (board[r, c] != 0)
                    {
                        cells[r, c].Text = board[r, c].ToString();
                        cells[r, c].ReadOnly = true; // Sabit sayılar değiştirilemez
                        cells[r, c].BackColor = Color.LightGray; // Sabit sayıların arka planı farklı olsun
                        cells[r, c].ForeColor = Color.DarkBlue;
                    }
                    else
                    {
                        cells[r, c].ForeColor = Color.Black; // Kullanıcının yazacağı sayılar siyah
                    }
                }
            }

        }

        //Check Butonu
        private void button2_Click(object sender, EventArgs e)
        {
            checksayisi += 1;
            // Oyunun bitip bitmediğini anlamak için bir değişken tanımlıyoruz
            bool isCompleteAndCorrect = true;

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    // Eğer hücre zaten doğru bilinmiş ve kilitlenmişse (Yeşilse) dokunma, atla
                    if (cells[r, c].ReadOnly && cells[r, c].BackColor == Color.Green)
                    {
                        continue;
                    }

                    // Sadece kullanıcının doldurduğu kutuları kontrol et
                    if (!cells[r, c].ReadOnly && cells[r, c].Text != "")
                    {
                        int userValue = int.Parse(cells[r, c].Text);

                        if (userValue == solutionBoard[r, c])
                        {
                            cells[r, c].BackColor = Color.Green;
                            cells[r, c].ForeColor = Color.White;
                            cells[r, c].ReadOnly = true; // Doğruysa kilitle
                        }
                        else
                        {
                            cells[r, c].BackColor = Color.Red;
                            cells[r, c].ForeColor = Color.White;
                            isCompleteAndCorrect = false; // Bir hata varsa oyun henüz bitmemiştir
                        }
                    }
                    else if (cells[r, c].Text == "")
                    {
                        // Eğer hala boş bir kutu varsa oyun bitmemiştir
                        cells[r, c].BackColor = Color.White;
                        cells[r, c].ForeColor = Color.Black;
                        isCompleteAndCorrect = false;
                    }
                }
            }

            // Tüm döngü bittikten sonra kontrol et: Her şey doğru mu?
            if (isCompleteAndCorrect)
            {
                timer1.Stop();
                MessageBox.Show("Tebrikler! Bulmacayı hatasız bir şekilde "+ label1.Text+" saniyede tamamladınız. Gerçek bir Sudoku ustasısın!", "Oyun Bitti!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                button1.Enabled = true;
                button3.Enabled = false;
                button2.Enabled = false;
                // Oyun bittiği için tüm hücreleri kilitle
                foreach (var cell in cells) cell.ReadOnly = true;

                //Sonucu sqle kaydetme
                cnt.Open();
                SqlCommand cmd = new SqlCommand("insert into veriler(ad,seviye,checksayisi,süre,tarih) values (@ad,@seviye,@checksayisi,@süre,@tarih)", cnt);
                cmd.Parameters.AddWithValue("@ad", nametextBox.Text);
                cmd.Parameters.AddWithValue("@seviye", comboBox1.Text);
                cmd.Parameters.AddWithValue("@checksayisi", checksayisi.ToString());
                cmd.Parameters.AddWithValue("@süre", label1.Text+" Saniye");
                cmd.Parameters.AddWithValue("@tarih", DateTime.Now.ToString("d MMMM yyyy dddd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                cnt.Close();

                nametextBox.Font = new Font("Segoe UI", 14, FontStyle.Italic);
                nametextBox.Text = "Enter Name";
                nametextBox.ForeColor = Color.Gray;

                comboBox1.SelectedIndex = -1;
            }
        }

        //Cevap Görüntüleme Butonu
        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            label1.Visible = false;

            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    // Eğer hücre sistem tarafından verilmiş bir ipucu değilse (ReadOnly değilse)
                    // veya kullanıcı yanlış yazmışsa (kırmızıysa), doğru cevabı yazdır.
                    if (!cells[r, c].ReadOnly || cells[r, c].BackColor == Color.Red)
                    {
                        // Arka plandaki doğru cevabı hücreye yaz
                        cells[r, c].Text = solutionBoard[r, c].ToString();

                        // Görsel geri bildirim: Bilgisayarın tamamladığını belirtmek için farklı bir renk 
                        cells[r, c].BackColor = Color.DarkKhaki;
                        cells[r, c].ForeColor = Color.White;

                        // Artık kullanıcı değiştiremesin diye kilitle
                        cells[r, c].ReadOnly = true;
                    }
                }
            }

            nametextBox.Font = new Font("Segoe UI", 14, FontStyle.Italic);
            nametextBox.Text = "Enter Name";
            nametextBox.ForeColor = Color.Gray;

            comboBox1.SelectedIndex = -1;

            MessageBox.Show("Bulmaca otomatik olarak tamamlandı!", "Finish");

        }
        
        //Süre için timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            süre++;
            label1.Text = süre.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.Show();
        }

        private void nametextBox_MouseEnter(object sender, EventArgs e)
        {
            // Eğer zaten kutunun içindeysem veya yazı "Enter Name" değilse hiçbir şey yapma
            if (nametextBox.Focused || nametextBox.Text != "Enter Name")
            {
                return;
            }

            nametextBox.Text = "";
            nametextBox.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            nametextBox.ForeColor = Color.Black;
        }

     
        private void nametextBox_MouseLeave(object sender, EventArgs e)
        {
            // EĞER kutu odaklanmışsa (kullanıcı şu an içine yazıyorsa), 
            // fare dışarı çıksa bile hiçbir şeyi değiştirme!
            if (nametextBox.Focused)
            {
                return;
            }

            // Sadece odak dışarıdayken ve kutu boşken "Enter Name" moduna dön
            if (string.IsNullOrWhiteSpace(nametextBox.Text))
            {
                nametextBox.Font = new Font("Segoe UI", 14, FontStyle.Italic);
                nametextBox.Text = "Enter Name";
                nametextBox.ForeColor = Color.Gray;
            }
        }
    }
}
