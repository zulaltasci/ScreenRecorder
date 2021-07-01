using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenRecorder
{
    public partial class Form1 : Form
    {
        bool klasorKontrol = false; //klasörü seçip seçmediğini kontrol eden değişken.
        string outputPath = ""; //videonun kaydedileceği yeri tutacağımız değişken.

        ScreenRecorder kaydedici = new ScreenRecorder(new Rectangle(), ""); //kayıt işlemini gerçekleştirmek için ScreenRecorder sınıfını çağırıyoruz.
        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog(); //klasör seçme işlemi için  FolderBrowserDialog sınıfını çağırıyoruz.
            folderBrowser.Description = "Bir dosya secin"; //Açılacak olan pencerenin açıklama kısmını yazıyoruz.

            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) //eğer ki klasör seçim işlemi yapıldıysa.
            {
                outputPath = folderBrowser.SelectedPath; //videonun kaydedileceği yerini değişkene aktarıyoruz.
                klasorKontrol = true; //klasörün seçilip seçilmediğini kontrol eden değişkene true değerini yani seçildi bilgisini veriyoruz.

                Rectangle bounds = Screen.FromControl(this).Bounds; //ekranın ne kadarını yani sınırlarını kaydedeceğimizi belirtiyoruz.
                kaydedici = new ScreenRecorder(bounds, outputPath); //sınırları ve videonun kaydedileceği yeri parametre olarak ScreenRecorder'a gönderiyoruz.
            }
            else
            {
                MessageBox.Show("Lütfen Bir Klasör Seçin  ", "Hata"); //eğer klasör seçilmediyse mesaj gösteriyoruz.
            }
        }

        private void tmrRecord_Tick(object sender, EventArgs e)
        {
            kaydedici.RecordVideo(); //sürekli olarak ekran görüntüsü alması için timer'ın RecordVideo methodunu çalıştırmasını sağlıyoruz.
            kaydedici.RecordAudio(); ////sürekli olarak ses kaydı alması için timer'ın RecordAudio methodunu çalıştırmasını sağlıyoruz.

            lblTime.Text = kaydedici.GetElapset(); //label a kayıt başladıktn sonra geçen saniyeyi yazdırıyoruz.
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (klasorKontrol) //eğer klasör seçildiyse
            {
                tmrRecord.Start(); //kayıt işlemini sürekli olarak yapması için timer'ı çalıştırıyoruz.
            }
            else
            {
                MessageBox.Show("Kaydetmeden önce bir çıktı klasörü seçmelisiniz ", "Hata"); //eğer klasör seçilmediyse mesaj gösteriyoruz.
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tmrRecord.Stop(); //kayıt duracağı için timer'ı durduruyoruz.
            kaydedici.Stop(); //ScreenRecorder sınıfında bulunan, video kayıt işlemi bittikten sonra yapılacak şeyleri yapan Stop methodunu çalıştırıyoruz.
            Application.Restart(); //Her şeyi sıfırlamak için Application.Restart methodunu çalıştırıyoruz. Bu method programı yeniden başlatır.

        }
    }
}
