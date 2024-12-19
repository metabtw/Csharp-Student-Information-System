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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Xml.Linq;


namespace databaseprc
{
    public partial class Form1 : Form
    {
        // SQL bağlantı nesnesi
        SqlConnection baglanti;
        // SQL komut nesnesi
        SqlCommand komut;
        // SQL veri adaptörü
        SqlDataAdapter da;



        public Form1()
        {
            InitializeComponent();
        }

        // Veritabanından öğrenci bilgilerini getirip tabloya dolduran metot
        void ogrgetir()
        {
            // SQL bağlantısını ayarlar ve açar
            baglanti = new SqlConnection("server=localhost\\SQLEXPRESS;Initial Catalog=student;Integrated Security=SSPI");
            baglanti.Open();

            // SQL sorgusu ile tüm verileri çeker
            da = new SqlDataAdapter("SELECT * FROM dbo.ogrtbl", baglanti);

            // Verileri saklamak için DataTable oluşturur
            DataTable tablo = new DataTable();
            // DataTable'ı doldurur
            da.Fill(tablo);
            // DataGridView’e veriyi atar
            dataGridView1.DataSource = tablo;
            baglanti.Close();
        }

        // Form yüklendiğinde öğrenci verilerini getirir
        private void Form1_Load(object sender, EventArgs e)
        {
            ogrgetir(); // Veriyi getirip yükler
        }

        // DataGridView hücresine tıklayınca öğrenci bilgilerini textboxlara doldurur
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            txtno.Text = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
            txtname.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
            txtsurname.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
            txtphone.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
            bdate.Text = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
        }

        // Yeni öğrenci ekleyen butonun olay metodu
        private void addbtn_Click(object sender, EventArgs e)
        {
            // INSERT SQL komutu ile yeni öğrenci ekleme sorgusu oluşturur
            string sorgu = "INSERT INTO ogrtbl(name,surname,phone,birth_date) VALUES (@name,@surname,@phone,@birth_date)";
            komut = new SqlCommand(sorgu, baglanti);
            // Parametrelerle değerleri bağlar
            komut.Parameters.AddWithValue("@name", txtname.Text);
            komut.Parameters.AddWithValue("@surname", txtsurname.Text);
            komut.Parameters.AddWithValue("@phone", txtphone.Text);
            komut.Parameters.AddWithValue("@birth_date", bdate.Value);
            // Bağlantıyı açar ve sorguyu çalıştırır
            baglanti.Open();
            komut.ExecuteNonQuery();
            baglanti.Close();

            // Yeni öğrenci eklendikten sonra tabloyu günceller
            ogrgetir();

        }

        // Öğrenciyi silen butonun olay metodu
        private void deletebtn_Click(object sender, EventArgs e)
        {
            string sorgu = "DELETE FROM ogrtbl WHERE studentno=@studentno";
            komut = new SqlCommand(sorgu, baglanti);
            komut.Parameters.AddWithValue("@studentno", Convert.ToInt32(txtno.Text));
            baglanti.Open();
            komut.ExecuteNonQuery();
            baglanti.Close();
            ogrgetir();
        }

        // Öğrenci bilgilerini güncelleyen butonun olay metodu
        private void upgradebtn_Click(object sender, EventArgs e)
        {
            string sorgu = "UPDATE ogrtbl SET name=@name,surname=@surname,phone=@phone,birth_date=@birth_date WHERE studentno=@studentno";
            komut = new SqlCommand(sorgu, baglanti);
            komut.Parameters.AddWithValue("@studentno", Convert.ToInt32(txtno.Text));
            komut.Parameters.AddWithValue("@name", txtname.Text);
            komut.Parameters.AddWithValue("@surname", txtsurname.Text);
            komut.Parameters.AddWithValue("@phone", txtphone.Text);
            komut.Parameters.AddWithValue("@birth_date", bdate.Value);
            baglanti.Open();
            komut.ExecuteNonQuery();
            baglanti.Close();
            ogrgetir();
        }

        // Öğrenciyi numarasına göre arayan bul butonunun metodu
        private void button1_Click(object sender, EventArgs e)
        {
            string studentNo = txtno.Text; // Aranacak öğrenci numarasını alır
            baglanti = new SqlConnection("server=localhost\\SQLEXPRESS;Initial Catalog=student;Integrated Security=SSPI");

            try
            {
                baglanti.Open(); // SQL bağlantısını açar

                // Öğrenci numarasına göre arama yapmak için SQL sorgusu
                string sorgu = "SELECT * FROM dbo.ogrtbl WHERE studentno = @studentno";
                komut = new SqlCommand(sorgu, baglanti);
                komut.Parameters.AddWithValue("@studentno", studentNo); // Sorguya parametre ekler

                DataTable tablo = new DataTable();
                da = new SqlDataAdapter(komut);
                da.Fill(tablo); // DataTable'ı sorgu sonucuyla doldurur

                // Eğer sonuç varsa DataGridView'e ve TextBox'lara doldurur
                if (tablo.Rows.Count > 0)
                {
                    dataGridView1.DataSource = tablo; // Sonucu DataGridView'de gösterir

                    // TextBox'ları ilk kayıt bilgisiyle doldurur
                    txtno.Text = tablo.Rows[0]["studentno"].ToString();
                    txtname.Text = tablo.Rows[0]["name"].ToString();
                    txtsurname.Text = tablo.Rows[0]["surname"].ToString();
                    txtphone.Text = tablo.Rows[0]["phone"].ToString();
                    bdate.Text = tablo.Rows[0]["birth_date"].ToString();
                }
                else
                {
                    MessageBox.Show("Öğrenci bulunamadı."); // Sonuç yoksa bilgi verir
                    dataGridView1.DataSource = null; // DataGridView'i temizler
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya bilgi verir
                MessageBox.Show("Hata:Bu şekilde kayıtlı bir öğrenci yok " + ex.Message);
            }
            finally
            {
                baglanti.Close(); // Bağlantıyı kapatır
            }
        }

        private void print_Click(object sender, EventArgs e)
        {
            // PDF dosyasının kaydedileceği yol
            string filePath = @"C:\Users\muham\OneDrive\Desktop\OgrenciIzinBelgesi.pdf";

            try
            {
                // PDF dokümanını oluştur
                Document pdfDoc = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(pdfDoc, new FileStream(filePath, FileMode.Create));
                pdfDoc.Open();

                // Türkçe karakter desteği için font ayarları
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font titleFont = new iTextSharp.text.Font(bf, 18, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font bodyFont = new iTextSharp.text.Font(bf, 12, iTextSharp.text.Font.NORMAL);

                // Başlık
                Paragraph title = new Paragraph("Öğrenci İzin Belgesi\n\n", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                pdfDoc.Add(title);

                // TextBox'lardan alınan veriler
                string studentName = txtname?.Text.Trim() ?? "Adı Belirtilmemiş";
                string studentSurname = txtsurname?.Text.Trim() ?? "Soyadı Belirtilmemiş";
                string studentNo = txtno?.Text.Trim() ?? "Numarası Belirtilmemiş";

                // İçerik
                string izinMetni = $"Okulumuz öğrencilerinden {studentName} {studentSurname}, " +
                                   $"öğrenci numarası {studentNo}, " +
                                   $"bu gün ({DateTime.Now.ToShortDateString()}) izinlidir.\n\n" +
                                   $"İlgililere duyurulur.";

                Paragraph izinParagraph = new Paragraph(izinMetni, bodyFont)
                {
                    Alignment = Element.ALIGN_LEFT
                };
                pdfDoc.Add(izinParagraph);

                // PDF dokümanını kapat
                pdfDoc.Close();

                MessageBox.Show("PDF başarıyla oluşturuldu ve kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
