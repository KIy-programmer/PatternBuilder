using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SingletonElevator
{

    // 1. ПАТТЕРН SINGLETON (Лифт)
    public class Elevator
    {
        // Статическая переменная, хранящая единственный экземпляр
        private static Elevator _instance;

        // Объект для блокировки потоков (чтобы Singleton был потокобезопасным)
        private static readonly object _lock = new object();

        // Текущий этаж лифта
        public int CurrentFloor { get; private set; } = 1;

        // Приватный конструктор! Никто извне не может сделать new Elevator()
        private Elevator() { }

        // Свойство для получения экземпляра
        public static Elevator Instance
        {
            get
            {
                lock (_lock) // Блокируем, чтобы два потока не создали два лифта одновременно
                {
                    if (_instance == null)
                    {
                        _instance = new Elevator();
                    }
                    return _instance;
                }
            }
        }

        public void MoveTo(int floor)
        {
            CurrentFloor = floor;
        }
    }


    // 2. ПОМЕЩЕНИЕ И ЭТАЖ
    public class Room
    {
        public string Name { get; set; }
        public Room(string name) { Name = name; }
    }

    public class Floor
    {
        public int Number { get; private set; }
        public List<Room> Rooms { get; private set; } = new List<Room>();

        public Floor(int number)
        {
            Number = number;
        }

        public void AddRoom(string roomName)
        {
            Rooms.Add(new Room(roomName));
        }
    }


    // 3. ПАТТЕРН SINGLETON (Здание)
    public class Building
    {
        private static Building _instance;
        private static readonly object _lock = new object();

        // Здание содержит этажи
        public List<Floor> Floors { get; private set; } = new List<Floor>();

        // И содержит лифт (через свойство Instance)
        public Elevator Elevator => Elevator.Instance;

        private Building()
        {
            // При создании здания сразу создаем этажи и комнаты
            InitializeBuilding();
        }

        public static Building Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Building();
                    }
                    return _instance;
                }
            }
        }

        private void InitializeBuilding()
        {
            // Создаем 3 этажа
            for (int i = 1; i <= 3; i++)
            {
                var floor = new Floor(i);
                floor.AddRoom($"Офис {i}-1");
                floor.AddRoom($"Офис {i}-2");
                Floors.Add(floor);
            }
        }
    }


    // 4. ГРАФИЧЕСКИЙ ИНТЕРФЕЙС
    public partial class Form1 : Form
    {
        private Building _building;
        private ComboBox cmbFloors;
        private TextBox txtLog;
        private Label lblCurrentFloor;

        public Form1()
        {
            InitializeUI();

            // Получаем ЕДИНСТВЕННОЕ здание в системе
            _building = Building.Instance;

            // Выводим информацию о том, что здание создано
            Log("Система запущена. Здание успешно инициализировано.");
            Log($"Количество этажей в здании: {_building.Floors.Count}");
            Log($"Лифт находится на {_building.Elevator.CurrentFloor} этаже.");
            UpdateFloorLabel();
        }

        private void InitializeUI()
        {
            this.Text = "Здание и Лифт (Паттерн Singleton)";
            this.Size = new System.Drawing.Size(600, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            var font = new System.Drawing.Font("Segoe UI", 10F);

            // Текущий этаж
            lblCurrentFloor = new Label
            {
                Text = "Лифт на этаже: 1",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkBlue
            };

            // Выбор этажа
            var lblSelect = new Label { Text = "Выберите этаж и нажмите 'Вызвать':", Location = new System.Drawing.Point(20, 70), AutoSize = true, Font = font };
            cmbFloors = new ComboBox
            {
                Location = new System.Drawing.Point(20, 100),
                Size = new System.Drawing.Size(200, 30),
                Font = font,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFloors.Items.AddRange(new object[] { "1 этаж", "2 этаж", "3 этаж" });
            cmbFloors.SelectedIndex = 0;

            // Кнопка вызова
            var btnMove = new Button
            {
                Text = "Вызвать лифт",
                Location = new System.Drawing.Point(240, 98),
                Size = new System.Drawing.Size(150, 32),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.LightGreen
            };
            btnMove.Click += BtnMove_Click;

            // Кнопка-провокатор (доказываем, что Singleton работает)
            var btnTestSingleton = new Button
            {
                Text = "Создать 'второе' здание (тест)",
                Location = new System.Drawing.Point(400, 98),
                Size = new System.Drawing.Size(170, 32),
                Font = new System.Drawing.Font("Segoe UI", 9F),
                BackColor = System.Drawing.Color.LightCoral
            };
            btnTestSingleton.Click += BtnTestSingleton_Click;

            // Лог событий
            txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new System.Drawing.Point(20, 150),
                Size = new System.Drawing.Size(550, 250),
                Font = new System.Drawing.Font("Consolas", 10F),
                BackColor = System.Drawing.Color.White
            };

            this.Controls.Add(lblCurrentFloor);
            this.Controls.Add(lblSelect);
            this.Controls.Add(cmbFloors);
            this.Controls.Add(btnMove);
            this.Controls.Add(btnTestSingleton);
            this.Controls.Add(txtLog);
        }

        // Логирование
        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }

        private void UpdateFloorLabel()
        {
            lblCurrentFloor.Text = $"Лифт на этаже: {_building.Elevator.CurrentFloor}";
        }

        // Событие: Вызов лифта
        private void BtnMove_Click(object sender, EventArgs e)
        {
            // Получаем номер этажа из выпадающего списка
            int targetFloor = cmbFloors.SelectedIndex + 1;

            if (targetFloor == _building.Elevator.CurrentFloor)
            {
                Log("Лифт уже находится на этом этаже.");
                return;
            }

            // Двигаем единственный экземпляр лифта
            Log($"Вызов лифта на {targetFloor} этаж...");
            _building.Elevator.MoveTo(targetFloor);
            Log($"Лифт прибыл. Текущий этаж: {_building.Elevator.CurrentFloor}");

            // Показываем, какие помещения есть на этом этаже (доказываем, что здание едино)
            var floor = _building.Floors[targetFloor - 1];
            Log($"Помещения на {targetFloor} этаже: {string.Join(", ", floor.Rooms.ConvertAll(r => r.Name))}");

            UpdateFloorLabel();
        }

        // Событие: Доказательство паттерна Singleton
        private void BtnTestSingleton_Click(object sender, EventArgs e)
        {
            // Пытаемся получить здание и лифт еще раз
            var fakeBuilding = Building.Instance;
            var fakeElevator = Elevator.Instance;

            // Сравниваем ссылки (ReferenceEquals проверяет, один ли это объект в памяти)
            bool isSameBuilding = ReferenceEquals(_building, fakeBuilding);
            bool isSameElevator = ReferenceEquals(_building.Elevator, fakeElevator);

            Log("--- ПРОВЕРКА SINGLETON ---");
            if (isSameBuilding && isSameElevator)
            {
                Log("УСПЕХ: Здание и Лифт - это те же самые объекты в памяти!");
                Log("Паттерн Singleton работает корректно. Второго здания/лифта не существует.");
            }
            else
            {
                Log("ОШИБКА: Созданы разные объекты! Паттерн сломан.");
            }
            Log("--------------------------");
        }
    }
}