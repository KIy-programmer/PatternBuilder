using System;
using System.Collections.Generic;
using System.IO; // Добавлено для работы с файлами
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace PatternBuilder
{
    // --- 1. ПРОДУКТ ---
    public class Article
    {
        public string Title { get; set; } = "";
        public List<string> Authors { get; set; } = new List<string>();
        public string Text { get; set; } = "";
        public string HashCode { get; set; } = "";
        public bool IsHashValid { get; set; } = false;

        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            return JsonSerializer.Serialize(this, options);
        }
    }

    // --- 2. ПАТТЕРН BUILDER ---
    public class ArticleBuilder
    {
        private Article _article = new Article();

        public ArticleBuilder(string txtContent)
        {
            try
            {
                ParseTxt(txtContent);
            }
            catch
            {
                throw new FormatException("Текст не соответствует формату!\nОжидается:\nTitle: ...\nAuthors: ...\nТекст...\nHash: ...");
            }
        }

        private void ParseTxt(string content)
        {
            var lines = content.Trim().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (lines.Length < 4) throw new Exception();

            _article.Title = lines[0].Replace("Title: ", "").Trim();

            var authorsString = lines[1].Replace("Authors: ", "").Trim();
            foreach (var author in authorsString.Split(','))
            {
                _article.Authors.Add(author.Trim());
            }

            var textLines = new List<string>();
            for (int i = 2; i < lines.Length - 1; i++)
            {
                textLines.Add(lines[i].Trim());
            }
            _article.Text = string.Join(" ", textLines).Trim();

            // Исправленная строка без оператора ^ (чтобы не было ошибок)
            var hashLine = lines[lines.Length - 1].Trim();
            if (hashLine.StartsWith("Hash: "))
            {
                _article.HashCode = hashLine.Replace("Hash: ", "").Trim();
            }
        }

        public ArticleBuilder ValidateHash()
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(_article.Text);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                MessageBox.Show("Программа вычислила этот хеш:\n\n" + sb.ToString(), "Отладка хеша");
                _article.IsHashValid = string.Equals(sb.ToString(), _article.HashCode, StringComparison.OrdinalIgnoreCase);
            }
            return this;
        }

        public Article Build()
        {
            return _article;
        }
    }

    // --- 3. ГРАФИЧЕСКИЙ ИНТЕРФЕЙС (САМА ФОРМА) ---
    public partial class Form1 : Form
    {
        private TextBox txtInput;
        private TextBox txtOutput;
        private Button btnLoadTxt;    // Кнопка загрузки файла
        private Button btnSaveJson;   // Кнопка сохранения файла
        private Label lblStatus;
        private Label lblFileName;    // Показывает имя загруженного файла

        public Form1()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Конвертер TXT в JSON (Паттерн Builder)";
            this.Size = new System.Drawing.Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            var font = new System.Drawing.Font("Segoe UI", 10F);

            // --- Блок загрузки ---
            btnLoadTxt = new Button
            {
                Text = "1. Выбрать TXT файл",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(200, 35),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };
            btnLoadTxt.Click += BtnLoadTxt_Click;

            lblFileName = new Label
            {
                Text = "Файл не выбран",
                Location = new System.Drawing.Point(220, 18),
                AutoSize = true,
                Font = font,
                ForeColor = System.Drawing.Color.Gray
            };

            // --- Поля ввода/вывода ---
            var lblInput = new Label { Text = "Содержимое TXT:", Location = new System.Drawing.Point(10, 55), AutoSize = true, Font = font };
            txtInput = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new System.Drawing.Point(10, 80),
                Size = new System.Drawing.Size(450, 450),
                Font = new System.Drawing.Font("Consolas", 11F),
                ReadOnly = true // Теперь мы только читаем файл, не редактируем вручную
            };

            var lblOutput = new Label { Text = "Результат (JSON):", Location = new System.Drawing.Point(480, 55), AutoSize = true, Font = font };
            txtOutput = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Location = new System.Drawing.Point(480, 80),
                Size = new System.Drawing.Size(440, 450),
                Font = new System.Drawing.Font("Consolas", 11F),
                WordWrap = false
            };

            // --- Кнопка сохранения ---
            btnSaveJson = new Button
            {
                Text = "2. Конвертировать и сохранить как JSON",
                Location = new System.Drawing.Point(10, 540),
                Size = new System.Drawing.Size(910, 40),
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold),
                Enabled = false // Неактивна, пока не загружен TXT
            };
            btnSaveJson.Click += BtnSaveJson_Click;

            // --- Статус (Хеш валиден или нет) ---
            lblStatus = new Label
            {
                Text = "Ожидание файла...",
                Location = new System.Drawing.Point(10, 590),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold)
            };

            this.Controls.Add(btnLoadTxt);
            this.Controls.Add(lblFileName);
            this.Controls.Add(lblInput);
            this.Controls.Add(txtInput);
            this.Controls.Add(lblOutput);
            this.Controls.Add(txtOutput);
            this.Controls.Add(btnSaveJson);
            this.Controls.Add(lblStatus);
        }

        // 1. Событие: Загрузка TXT файла
        private void BtnLoadTxt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                openFileDialog.Title = "Выберите TXT файл статьи";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Читаем файл
                        txtInput.Text = File.ReadAllText(openFileDialog.FileName, Encoding.UTF8);
                        lblFileName.Text = Path.GetFileName(openFileDialog.FileName);
                        lblFileName.ForeColor = System.Drawing.Color.Black;

                        btnSaveJson.Enabled = true; // Активируем кнопку конвертации
                        lblStatus.Text = "Файл загружен. Нажмите 'Конвертировать'.";
                        lblStatus.ForeColor = System.Drawing.Color.Black;
                        txtOutput.Clear();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка чтения файла: " + ex.Message);
                    }
                }
            }
        }

        // 2. Событие: Конвертация и сохранение в JSON
        private void BtnSaveJson_Click(object sender, EventArgs e)
        {
            try
            {
                string inputText = txtInput.Text;

                // Прогоняем через Builder
                Article article = new ArticleBuilder(inputText)
                                    .ValidateHash()
                                    .Build();

                // Выводим на экран
                txtOutput.Text = article.ToJson();

                // Обновляем статус
                if (article.IsHashValid)
                {
                    lblStatus.Text = "✔ Хеш-код совпадает! Можно сохранять.";
                    lblStatus.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblStatus.Text = "✖ ВНИМАНИЕ: Хеш-код не совпадает с текстом статьи!";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                }

                // Предлагаем сохранить файл
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JSON файлы (*.json)|*.json";
                    saveFileDialog.Title = "Сохранить документ как JSON";
                    saveFileDialog.FileName = "article.json"; // Имя по умолчанию

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Записываем JSON в файл
                        File.WriteAllText(saveFileDialog.FileName, txtOutput.Text, Encoding.UTF8);
                        MessageBox.Show("Файл успешно сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка конвертации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}