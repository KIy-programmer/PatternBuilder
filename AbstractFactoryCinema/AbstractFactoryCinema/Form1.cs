using System;
using System.Windows.Forms;

namespace AbstractFactoryCinema
{
    // 1. АБСТРАКТНЫЕ ПРОДУКТЫ (Интерфейсы)

    public interface IAudioTrack
    {
        string GetDescription();
    }

    public interface ISubtitles
    {
        string GetDescription();
    }


    // 2. КОНКРЕТНЫЕ ПРОДУКТЫ (Русские)
    public class RussianAudio : IAudioTrack
    {
        public string GetDescription() => "Звуковая дорожка: Русский язык (Дубляж)";
    }

    public class RussianSubtitles : ISubtitles
    {
        public string GetDescription() => "Субтитры: Русский язык (.srt)";
    }


    // 3. КОНКРЕТНЫЕ ПРОДУКТЫ (Английские)
    public class EnglishAudio : IAudioTrack
    {
        public string GetDescription() => "Звуковая дорожка: Английский язык (Оригинал)";
    }

    public class EnglishSubtitles : ISubtitles
    {
        public string GetDescription() => "Субтитры: Английский язык (.srt)";
    }


    // 4. АБСТРАКТНАЯ ФАБРИКА
    public interface IMovieFactory
    {
        IAudioTrack CreateAudioTrack();
        ISubtitles CreateSubtitles();
    }


    // 5. КОНКРЕТНЫЕ ФАБРИКИ
    public class RussianMovieFactory : IMovieFactory
    {
        public IAudioTrack CreateAudioTrack() => new RussianAudio();
        public ISubtitles CreateSubtitles() => new RussianSubtitles();
    }

    public class EnglishMovieFactory : IMovieFactory
    {
        public IAudioTrack CreateAudioTrack() => new EnglishAudio();
        public ISubtitles CreateSubtitles() => new EnglishSubtitles();
    }


    // 6. ГРАФИЧЕСКИЙ ИНТЕРФЕЙС
    public partial class Form1 : Form
    {
        private ComboBox cmbMovies;
        private RadioButton rbRussian;
        private RadioButton rbEnglish;
        private Button btnOrder;
        private TextBox txtResult;

        public Form1()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Кинопрокат (Паттерн Abstract Factory)";
            this.Size = new System.Drawing.Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            var font = new System.Drawing.Font("Segoe UI", 11F);

            // Выбор фильма
            var lblMovie = new Label { Text = "Выберите киноленту:", Location = new System.Drawing.Point(20, 20), AutoSize = true, Font = font };
            cmbMovies = new ComboBox
            {
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(540, 30),
                Font = font,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbMovies.Items.AddRange(new object[] { "Терминатор 2", "Матрица", "Интерстеллар" });
            cmbMovies.SelectedIndex = 0; // Выбираем первый по умолчанию

            // Выбор языка
            var lblLang = new Label { Text = "Язык звуковой дорожки (и субтитров):", Location = new System.Drawing.Point(20, 100), AutoSize = true, Font = font };

            rbRussian = new RadioButton { Text = "Русский (Дубляж)", Location = new System.Drawing.Point(20, 130), AutoSize = true, Font = font, Checked = true };
            rbEnglish = new RadioButton { Text = "Английский (Оригинал)", Location = new System.Drawing.Point(220, 130), AutoSize = true, Font = font };

            // Кнопка заказа
            btnOrder = new Button
            {
                Text = "Оформить заказ",
                Location = new System.Drawing.Point(20, 180),
                Size = new System.Drawing.Size(540, 45),
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.LightGreen
            };
            btnOrder.Click += BtnOrder_Click;

            // Окно вывода результата
            txtResult = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Location = new System.Drawing.Point(20, 240),
                Size = new System.Drawing.Size(540, 200),
                Font = new System.Drawing.Font("Consolas", 11F),
                BackColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            this.Controls.Add(lblMovie);
            this.Controls.Add(cmbMovies);
            this.Controls.Add(lblLang);
            this.Controls.Add(rbRussian);
            this.Controls.Add(rbEnglish);
            this.Controls.Add(btnOrder);
            this.Controls.Add(txtResult);
        }

        private void BtnOrder_Click(object sender, EventArgs e)
        {
            // 1. Получаем выбранный фильм
            string movieName = cmbMovies.SelectedItem.ToString();

            // 2. Выбираем нужную фабрику в зависимости от языка
            IMovieFactory factory;
            if (rbRussian.Checked)
            {
                factory = new RussianMovieFactory();
            }
            else
            {
                factory = new EnglishMovieFactory();
            }

            // 3. Фабрика сама создает ПАРУ (аудио + субтитры) на нужном языке
            IAudioTrack audio = factory.CreateAudioTrack();
            ISubtitles subtitles = factory.CreateSubtitles();

            // 4. Выводим результат
            txtResult.Text = $"--- Заказ успешно оформлен ---\r\n\r\n" +
                             $"Фильм: {movieName}\r\n\r\n" +
                             $"Характеристики:\r\n" +
                             $"• {audio.GetDescription()}\r\n" +
                             $"• {subtitles.GetDescription()}\r\n\r\n" +
                             $"* Система гарантирует, что язык аудио и субтитров совпадает.";
        }
    }
}