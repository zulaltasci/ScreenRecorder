using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

using Accord.Video.FFMPEG;

namespace ScreenRecorder
{
    // ZÜLAL TAŞÇI
    
    class ScreenRecorder
    {
        //Video Değişkenleri
        private Rectangle bounds; //Ekran videosu alabilmek için çerçevemizin sınırlarını tutan ve System.Drawing kütüphanesindeki Rectangle yapısından kullandığımız bounds değişkeni.
        private string outputPath = ""; //Kullanıcının seçtiği video durdurma yerini tutabilmek için kullandığımız string türündeki outputPath değişkeni.
        private string tempPath = ""; //Ekran görüntülerimizin kaydolduğu geçiçi dosya yolu için oluşturduğumuz string türündeki değişken.
        private int fileCount = 1;
        private List<string> inputİmageSequence = new List<string>(); //Alınan ekren görüntülerini sırası aktaracağımız string türünde bir dizi.

        // Dosya Değişkenleri

        private string audioName = "mic.wav"; // Ses dosyamızın ismini ve dosya türünü belirtiyoruz.

        private string videoName = "video.mp4"; //Videomuzun dosya değişkenini tanımlıyoruz.
        private string finalName = "finalVideo.mp4"; //Final Videomuzun dosya değişkenini tanımlıyoruz.

        // Zaman Değişkeni
        Stopwatch watch = new Stopwatch(); // Videodaki geçen süreyi tutabilmek için System.Diagnostics kütüphanesindeki Stopwatch sınıfından watch isimli bir nesne örneği oluşturuyoruz.

        // Ses Değişkeni

        /// <summary>
        /// Ses kaydını yapmak için oluşturduğumuz public sınıf.
        /// </summary>
        public static class NativeMethods
        {
            [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)] // Ses kaydı için winmm.dll dosyasını import ediyoruz.
            public static extern int record(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback); // Ses dosyasının kaydını başlatan metot.
        }


        /// <summary>
        /// Kurucu metodumuzu public  oluşturuyoruz ve çerçeve sınırları için bir de video durdurma yerini tutabilmek için iki değişken tanımlıyoruz.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="outPath"></param>
        public ScreenRecorder(Rectangle b, string outPath)
        {
            CreateTempFolder("tempScreenshots"); //Geçici ekran görüntülerimizi burada arıyoruz.

            bounds = b; //bounds değişkenimi kurucu metotda tanımladığımız b değişkenine eşitliyoruz.
            outputPath = outPath; //outputPath değişkenimi kurucu metotda tanımladığımız outPath değişkenine eşitliyoruz.
        }


        /// <summary>
        /// Geçici klosör oluşturmak için private void CreateTempFolder metodumu oluşturup name isimli bir değişken tanımlıyoruz.
        /// </summary>
        /// <param name="name"></param>
        private void CreateTempFolder(string name)
        {
            if (Directory.Exists("D://")) //System.IO kütüphanesindeki Directory.Exists i kullanarak dosya yolu seçim işlemlerini kontrol ediyoruz if ile. D dizini var ise 53 ile 57. satırlar arası çalışacaktır.
            {
                string pathName = $"D://{name}"; //pathName değişkenimi dosyamızı kaydetmek için seçtiğimiz klasör ismine eşitliyoruz. D diski varsa eğer.
                Directory.CreateDirectory(pathName); //System.IO kütüphanesinden yararlanarak geçici yol seçme işlemini yapıyoruz.
                tempPath = pathName; //tempPath değişkenini pathName e eşitlşiyoruz.
            }
            else //Eğer if teki şart sağlanmaz is 59 ile 63. satırlar arası çalışacaktır.
            {
                string pathName = $"C://{name}"; //pathName değişkenimi dosyamızı kaydetmek için seçtiğimiz klasör ismine eşitliyoruz. C diski varsa eğer.
                Directory.CreateDirectory(pathName); //System.IO kütüphanesinden yararlanarak geçici yol seçme işlemini yapıyoruz.
                tempPath = pathName; //tempPath değişkenini pathName e eşitlşiyoruz.
            }
        }

        /// <summary>
        /// Silmek için bir private void türünde  metot oluşturuyoruz ve bir değişken tanımlıyoruz.
        /// </summary>
        /// <param name="targetDir"></param>
        private void DeletePath(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir); // files isimli string türünde bir dizi oluşturulup Directory sınıfının GetFiles metonunu kullanarak dosya yolumuzdaki tüm seçim işlemlerini yapıp bu oluşturduğumuz diziye aktarıyoruz.
            string[] dirs = Directory.GetDirectories(targetDir); // dirs isimli string türünde bir dizi oluşturulup Directory sınıfının GetDirectories metonunu kullanarak dosya yolumuzdaki tüm seçim işlemlerini yapıp bu oluşturduğumuz diziye aktarıyoruz.

            foreach (string file in files) // Ekran görüntülerini silmek için oluşturduğumuz döngü.
            {
                File.SetAttributes(file, FileAttributes.Normal); // Silinecek dosyanın yolunu düzenliyoruz.
                File.Delete(file); // Dosyayı siliyoruz.
            }

            foreach (string dir in dirs)
            {
                DeletePath(dir); // Silinecek dosyayı DeletePath e parametre olarak gönderiyoruz.
            }

            Directory.Delete(targetDir, false); // Klosörü silme komutu.
        }

        /// <summary>
        /// Temizleme metodu olurşturuyoruz public void türünde.
        /// </summary>
        public void CleanUp()
        {
            if (Directory.Exists(tempPath)) //System.IO kütüphanesindeki Directory.Exists i kullanarak tempPath parametresini kullanıyoruz.
            {
                DeletePath(tempPath); // Geçici yol olarak seçtiğimiz tempPath i DeletePathte çalıştırıyoruz.
            }
        }


        /// <summary>
        /// Videodaki geçen süreyi döndürecek public metodu oluşturuyoruz.
        /// </summary>
        public string GetElapset()
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds); //Format sınıfından yararlanarak  Saat , dakika ve saniyeyi döndürüyoruz.
        }


        /// <summary>
        /// Video Kaydını oluşturmak için public metot oluşturuyoruz.
        /// </summary>
        public void RecordVideo()
        {
            watch.Start(); // Geçen süreyi tutmak için örneğin aldıdığım watch ı Start ile başlatıryorum.

            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height)) //System.Drawing kütüphanesindeki Bitmap classı ile bir örnek oluşturup ekranın genişliği ve uzunluğunu alıyoruz.
            {
                using (Graphics g = Graphics.FromImage(bitmap)) //Ekran görüntüsü almak için Graphics sınıfını çağırıp bitmap türünde olduğunu belirtiyoruz.
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size); // Ekran görüntüsünün soldan ve yukarıdan sınırını ve boyutunu parametre olarak göndererek alıyoruz.
                }
                string name = tempPath + "//screenshot-" + fileCount + ".png"; // Seçtiğimiz dosya yoluna aldığımız ekran görüntülerini .png olarak isimlendiriyoruz.
                bitmap.Save(name, ImageFormat.Png); // bitmap ile bu aldığımız ekran görüntülerini .png cinsinde kaydediyoruz.
                inputİmageSequence.Add(name); // Ekran görüntülerinin girdi sırasına göre izini süreriz.
                fileCount++; //Her resimde sayıyı bir artırarak kaydetmek için sayıyı artırıyoruz.

                bitmap.Dispose(); // Bitmap yardımı ile kaydettiğimiz tüm resimleri siler.
            }
        }

        /// <summary>
        /// Ses kaydı için bir public metot oluşturduk.
        /// </summary>
        public void RecordAudio()
        {
            NativeMethods.record("Wav ses kaydedici", "", 0, 0); // NativeMethods içersindeki recorda parametreleri göndererek kayıt işlemini gerçekleştiriyoruz.
            NativeMethods.record("Ses kaydı ", "", 0, 0); // NativeMethods içersindeki recorda parametreleri göndererek kayıt işlemini gerçekleştiriyoruz.
        }

        /// <summary>
        /// Vidoyu kaydetmek için private void metot yazıyoruz ve  genişlik , yükseklik , kare hızı için parametlereler tanımlıyoruz.
        /// </summary>
        /// <param name="witdh"></param>
        /// <param name="height"></param>
        /// <param name="frameRate"></param>
        private void SaveVideo(int witdh, int height, int frameRate)
        {
            using (VideoFileWriter vFWriter = new VideoFileWriter()) //Accord.Video.FFMPEG kütüphanesin örnek oluşturuyoruz.
            {
                vFWriter.Open(outputPath + "//" + videoName, witdh, height, frameRate, VideoCodec.MPEG4);  // Paremetre değerlerimi alıp Open sınıfı yardımı ile ffmpeg içer aktarmalarını kullanarak bir video dosyası oluşturuyoruz. Video dosya uzantımız mp4 olduğu için  VideoCodec.MPEG4 kullanıyoruz.

                foreach (string imageLoc in inputİmageSequence) // 
                {
                    Bitmap imageFrame = System.Drawing.Image.FromFile(imageLoc) as Bitmap; // Ekran görüntüsü aldığımız resimleri bitmap türüne çeviriyoruz. 
                    vFWriter.WriteVideoFrame(imageFrame);// Bitmap türüne çevirdiğimiz resimleri video karesi olarak ekliyoruz.
                    imageFrame.Dispose(); // bitmap bellekte ayrılan alandan siliniyor.
                }
                vFWriter.Close(); // Video dosyasını kapatıyoruz.
            }
        }

        /// <summary>
        /// Ses dosyamızı kaydedicek private void metot.
        /// </summary>
        private void SaveAudio()
        {
            string audioPath = "sesi kaydet " + outputPath + "//" + audioName; // Sesin kaydedileceği yolu belirtiyoruz.
            NativeMethods.record(audioPath, "", 0, 0);  // NativeMethods içersindeki recorda parametreleri göndererek kayıt işlemini gerçekleştiriyoruz.
            NativeMethods.record("ses kaydını kapat", "", 0, 0);  // NativeMethods içersindeki recorda parametreleri göndererek kayıt işlemini gerçekleştiriyoruz.
        }

        /// <summary>
        /// Aldığımız ses kaydını ve oluşturduğumuz video dosyasını birleştirenprivate void metot.
        /// </summary>
        /// <param name="video"></param>
        /// <param name="audio"></param>
        private void CombineVideoAndAudio(string video, string audio)
        {
            string command = $"/c ffmpeg -i \"{video}\" -i \"{audio}\" -shortest{finalName}"; // Video ve sesi birleştirme işlemi için gerekli komutu yazıyoruz.
            ProcessStartInfo startInfo = new ProcessStartInfo // Yeni bir process başlatacağımız için ProcessStartInfo clasını çağırıyoruz ve gerekli bilgileri gönderiyoruz.
            {
                CreateNoWindow = false,
                FileName = "cmd.exe",
                WorkingDirectory = outputPath,
                Arguments = command
            };

            using (Process exeProcess = Process.Start(startInfo)) // Processi başlatıyoruz.
            {
                exeProcess.WaitForExit(); // Processten  çıkılıncaya kadar process parçacığını engelliyoruz.
            }
        }


        /// <summary>
        /// Durdurma metodu public void türünde.
        /// </summary>
        public void Stop()
        {
            watch.Stop(); //İzlemeyi durduruyoruz.

            int witdh = bounds.Width; // Sınır genişliğini değişkene aktarıyoruz.
            int height = bounds.Height; // Sınır yüksekliğini değişkene aktarıyoruz.
            int frameRate = 10; // Saniyede 10 resim alacağımızı belirtiyoruz.

            SaveAudio(); // Ses kaydetme metodunu çalıştırıyoruz.

            SaveVideo(witdh, height, frameRate); // Video kaydetme metodunu çalıştırıyoruz.

            DeletePath(tempPath); // Ekran görüntülerini silmek için çalıştırıyoruz. 
        }

    }
}
