using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FactoryPrototypeShapes
{

    // 1. ПАТТЕРН PROTOTYPE (Интерфейс клонирования)
    public interface IShape
    {
        string Name { get; }
        int Cells { get; set; }
        IShape Clone(); // Метод для создания копии
    }


    // 2. КОНКРЕТНЫЕ ПРОДУКТЫ (Обычные фигуры)
    public class Tetromino : IShape
    {
        public string Name { get; private set; }
        public int Cells { get; set; }
        public string Color { get; set; }

        public Tetromino(string name, int cells, string color)
        {
            Name = name;
            Cells = cells;
            Color = color;
        }

        public IShape Clone()
        {
            // Создаем новый объект и копируем в него все свойства
            return new Tetromino(this.Name, this.Cells, this.Color);
        }
    }


    // 3. КОНКРЕТНЫЕ ПРОДУКТЫ (Супер-фигуры)
    public class SuperShape : IShape
    {
        public string Name { get; private set; }
        public int Cells { get; set; }
        public int BonusPoints { get; set; } // Уникальное свойство для супер-фигуры

        public SuperShape(string name, int cells, int bonus)
        {
            Name = name;
            Cells = cells;
            BonusPoints = bonus;
        }

        public IShape Clone()
        {
            return new SuperShape(this.Name, this.Cells, this.BonusPoints);
        }
    }


    // 4. ПАТТЕРН FACTORY METHOD
    public abstract class ShapeFactory
    {
        // Фабричный метод, который должны реализовать наследники
        public abstract IShape CreateShape();
    }

    public class NormalShapeFactory : ShapeFactory
    {
        private static readonly string[] Names = { "Квадрат", "Линия", "Уголок", "Зигзаг" };
        private static readonly string[] Colors = { "Красный", "Синий", "Зеленый", "Желтый" };
        private Random _rnd = new Random();

        public override IShape CreateShape()
        {
            // Обычная фигура имеет от 1 до 4 клеток
            int cells = _rnd.Next(1, 5);
            string name = Names[_rnd.Next(Names.Length)];
            string color = Colors[_rnd.Next(Colors.Length)];
            return new Tetromino(name, cells, color);
        }
    }

    public class SuperShapeFactory : ShapeFactory
    {
        private Random _rnd = new Random();

        public override IShape CreateShape()
        {
            // Супер-фигура имеет от 5 до 9 клеток и бонусные очки
            int cells = _rnd.Next(5, 10);
            int bonus = _rnd.Next(50, 100);
            return new SuperShape("Супер-Блок", cells, bonus);
        }
    }


    // 5. ГРАФИЧЕСКИЙ ИНТЕРФЕЙС
    public partial class Form1 : Form
    {
        private List<IShape> shapes = new List<IShape>();
        private ListBox listBoxShapes;
        private Random _rnd = new Random();

        public Form1()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Фигуры (Factory Method + Prototype)";
            this.Size = new System.Drawing.Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            var font = new System.Drawing.Font("Segoe UI", 10F);

            // Список фигур
            listBoxShapes = new ListBox
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(540, 300),
                Font = font
            };

            // Кнопка генерации (Factory Method)
            var btnGenerate = new Button
            {
                Text = "Создать случайную фигуру",
                Location = new System.Drawing.Point(20, 340),
                Size = new System.Drawing.Size(260, 40),
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.LightBlue
            };
            btnGenerate.Click += BtnGenerate_Click;

            // Кнопка клонирования (Prototype)
            var btnClone = new Button
            {
                Text = "Клонировать выбранную",
                Location = new System.Drawing.Point(300, 340),
                Size = new System.Drawing.Size(260, 40),
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.LightGreen
            };
            btnClone.Click += BtnClone_Click;

            this.Controls.Add(listBoxShapes);
            this.Controls.Add(btnGenerate);
            this.Controls.Add(btnClone);
        }

        // Событие: Генерация случайной фигуры (Factory Method)
        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            ShapeFactory factory;

            // С вероятностью 50% создаем обычную или супер-фигуру
            if (_rnd.Next(2) == 0)
            {
                factory = new NormalShapeFactory();
            }
            else
            {
                factory = new SuperShapeFactory();
            }

            // Фабрика создает фигуру
            IShape newShape = factory.CreateShape();
            shapes.Add(newShape);
            UpdateList();
        }

        // Событие: Клонирование выбранной фигуры (Prototype)
        private void BtnClone_Click(object sender, EventArgs e)
        {
            if (listBoxShapes.SelectedIndex == -1)
            {
                MessageBox.Show("Сначала выберите фигуру из списка!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Берем оригинальную фигуру по индексу
            IShape original = shapes[listBoxShapes.SelectedIndex];

            // ВЫЗЫВАЕМ ПАТТЕРН PROTOTYPE - создаем копию
            IShape clone = original.Clone();

            // Добавляем копию в общий список
            shapes.Add(clone);
            UpdateList();

            MessageBox.Show($"Создана копия фигуры '{original.Name}'!", "Клонирование", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Обновление списка на экране
        private void UpdateList()
        {
            listBoxShapes.Items.Clear();
            for (int i = 0; i < shapes.Count; i++)
            {
                var s = shapes[i];
                string info = "";

                if (s is SuperShape super)
                {
                    info = $"[{i + 1}] {s.Name} | Клеток: {s.Cells} | Бонус: {super.BonusPoints} (СУПЕР)";
                }
                else if (s is Tetromino tetro)
                {
                    info = $"[{i + 1}] {s.Name} | Клеток: {s.Cells} | Цвет: {tetro.Color}";
                }
                else
                {
                    info = $"[{i + 1}] {s.Name} | Клеток: {s.Cells}";
                }

                listBoxShapes.Items.Add(info);
            }
        }
    }
}