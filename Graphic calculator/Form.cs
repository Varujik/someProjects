using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
namespace Brushesss
{
    public partial class Form1 : Form
    {
        List<MyPoint> points = new List<MyPoint>(); // Координаты графика. (X & Y)
        List<Label> numberLabels = new List<Label>(); // Координаты.
        MyPoint O = new MyPoint(0, 0); // Координаты центра.
        bool MethodOnPaint = true; // Восстановить ли график после минимизации?
        bool XYwasDrawn = false;
        bool FunctionWasDrawn = false;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Shown(object sender, EventArgs e) // Первый показ формы
        {
            splitContainer1.Size = this.Size;
            O.X = splitContainer1.Panel2.Width / 2;
            O.Y = splitContainer1.Panel2.Height / 2;
            InterpolarityPanel.Enabled = false;
            SimpleFunctionPanel.Enabled = false;
        }
        public void DrawLine(MyPoint p1, MyPoint p2) // Провести линию между двумя точками
        {
            try
            {
                Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 2);
                Graphics graphics;
                graphics = splitContainer1.Panel2.CreateGraphics();
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality; // Качество графика
                graphics.DrawLine(pen, O.X + p1.X, O.Y - p1.Y, O.X + p2.X, O.Y - p2.Y); // Провести линию, учитывая центр
                pen.Dispose();
                graphics.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void DrawXY() // Рисуем оси X & Y
        {
            XYwasDrawn = true;
            // X
            {
                Pen pen = new System.Drawing.Pen(System.Drawing.Color.Green, 4);
                Graphics graphics;
                graphics = splitContainer1.Panel2.CreateGraphics();
                graphics.DrawLine(pen, 0, O.Y, Form1.ActiveForm.Width, O.Y);
                pen.Dispose();
                graphics.Dispose();
            }
            // Y
            {
                Pen pen = new System.Drawing.Pen(System.Drawing.Color.Green, 4);
                Graphics graphics;
                graphics = splitContainer1.Panel2.CreateGraphics();
                graphics.DrawLine(pen, O.X, 0, O.X, Form1.ActiveForm.Height);
                pen.Dispose();
                graphics.Dispose();
            }
            // Координаты осей
            // Координаты X
            {
                for (int i = 0; i < O.X * 2; i += 30)
                {
                    if ((i - O.X) > 40 || (i - O.X) < -40)
                    {
                        Label label = new Label();
                        Point labelLocation = new Point(i, (int)(2 * O.Y - 80));
                        label.Location = labelLocation;
                        label.Text = (i - O.X).ToString();
                        label.Font = new Font("Arial", 7, FontStyle.Bold);
                        label.Size = new Size(30, 20);
                        numberLabels.Add(label);
                        splitContainer1.Panel2.Controls.Add(label);
                    }
                }
            }
            // Координаты Y
            {
                for (int i = 0; i < O.Y * 2; i += 30)
                {
                    if ((i - O.Y) > 40 || (i - O.Y) < -40)
                    {
                        Label label = new Label();
                        Point labelLocation = new Point((int)(40), i);
                        label.Location = labelLocation;
                        label.Text = (O.Y - i).ToString();
                        label.Font = new Font("Arial", 7, FontStyle.Bold);
                        label.Size = new Size(30, 20);
                        numberLabels.Add(label);
                        splitContainer1.Panel2.Controls.Add(label);
                    }
                }
            }
        }
        public void DrawFunction() // Построение графика, учитывая точки
        {
            FunctionWasDrawn = true; // Пометим, что функция построена
            for (int i = 0; i < points.Count - 1; i++) // Выбор точек попарно (по два)
            {
                MyPoint p1 = new MyPoint(points[i].X, points[i].Y); // Создаем экземпляр класса MyPoint
                MyPoint p2 = new MyPoint(points[i + 1].X, points[i + 1].Y); // Создаем второе экземпляр
                DrawLine(p1, p2); // Вызов функции построения линии для двух точек
            }
        }
        private void btnDrawXY_Click(object sender, EventArgs e) // button Event
        {
            DrawXY();
        }
        private void btnDrawFunction_Click(object sender, EventArgs e) // button Event
        {
            try
            {
                points.Clear(); // Очищаем ранее построенный график (его точки)
                // Какую функцию рисуем?
                if (InterpolarityPanel.Enabled)
                {
                    CreatePolynomial();
                }
                else if (SimpleFunctionPanel.Enabled)
                {
                    if (rbSinx.Checked)
                        CreateSinxFunction();
                    else if (rbCosx.Checked)
                        CreateCosxFunction();
                    else if (rbSqrtx.Checked)
                        CreateSqrtFunction();
                    else if (rbPowx.Checked)
                        CreatePowFunction();
                    else if (rbCircle.Checked)
                        CreateCircle();
                }
                DrawFunction();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public double GetDividedDifferences(List<double> x, List<double> y, int i, int p) // Получение разделенных разностей
        {
            double Sum = 0;
            try
            {
                double Prod = 1;
                for (int k = i; k <= i + p; k++)
                {
                    Prod = 1;
                    for (int j = i; j <= i + p; j++)
                    {
                        if (j != k)
                        {
                            Prod *= x[k] - x[j];
                        }
                    }
                    Sum += (double)y[k] / Prod;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Sum;
        } 
        public void CreatePolynomial() // Метод построения полиномиальной функции
        {
            List<double> x = GetAllDataFromTextBox(txtBoxX); // X координаты (входящие данные)
            List<double> y = GetAllDataFromTextBox(txtBoxY); // Y координаты (входящие данные)
            List<double> interval = GetAllDataFromTextBox(txtBoxInterval); // Сегмент(интервал)
            List<double> a = new List<double>(); // Разделенные разности
            if (txtBoxX.Text == "" || txtBoxY.Text == "" || x.Count == 0 || y.Count == 0 || interval.Count == 0 || interval.Count > 2)
                return;
            if (x.Count != y.Count)
                return;
            a.Add(y[0]); // f(x0)
            for (int i = 1; i < x.Count; i++)
            {
                a.Add(GetDividedDifferences(x, y, 0, i));
            }
            for (int xx = (int)interval[0]; xx < (int)interval[1]; xx++)
            { // N(x) = a0 + a1*(x-x0) + a2 * (x-x0)*(x-x1) + ...
              // S = N(x)
                double S = 0;
                for (int i = 0; i < a.Count; i++)
                {
                    double Prod = 1;
                    for (int j = 0; j < i; j++)
                    {
                        Prod *= (xx - x[j]);
                    }
                    if (S + Prod*a[i] < -1000000000 || S + Prod * a[i] > 1000000000) // Границы пикселей
                    {
                        MessageBox.Show("Too big number, change Interval");
                        return;
                    }
                    S += Prod * a[i];
                }
                MyPoint myPoint = new MyPoint(xx, (float)S); // Создаем экземпляр в соответствии с X & f(X)
                points.Add(myPoint); // Добавление полученной точки в структуру данных List
            }
            // Получение формулы функции
            string str = string.Empty;
            for (int i = 0; i < a.Count; i++)
            {
                string Prod = "";
                for (int j = 0; j < i; j++)
                {
                    Prod += "(x - " + x[j] + ")*";
                }
                str += a[i] + "*" + Prod + " + ";
            }
            MessageBox.Show(str, "Function", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public void CreateSinxFunction() // Метод получения точек графика Sin в соответствии с коэффициентами
        {
            List<double> dataList = GetAllDataFromTextBox(txtBoxFunction);
            if (txtBoxFunction.Text == "" || dataList.Count == 0)
                return;
            for (double x = -1000; x < 1000; x += 20)
            {
                points.Add(new MyPoint((float)x, (float)(dataList[0] * Math.Sin(x) + dataList[1]))); // A*Sinx + B
            }
        }
        public void CreateCosxFunction() // Метод получения точек графика Cos в соответствии с коэффициентами
        {
            List<double> dataList = GetAllDataFromTextBox(txtBoxFunction);
            if (txtBoxFunction.Text == "" || dataList.Count == 0)
                return;
            for (double x = -1000; x < 1000; x += 20)
            {
                points.Add(new MyPoint((float)x, (float)(dataList[0] * Math.Cos(x) + dataList[1]))); // A*Cosx + B
            }
        }
        public void CreateSqrtFunction() // Метод получения точек графика Sqrt в соответствии с коэффициентами
        {
            List<double> dataList = GetAllDataFromTextBox(txtBoxFunction);
            if (txtBoxFunction.Text == "" || dataList.Count == 0)
                return;
            for (double x = 0; x < 1000; x += 10)
            {
                points.Add(new MyPoint((float)x, (float)(dataList[0] * Math.Sqrt(x) + dataList[1]))); //A* Sqrt(x) + B
            }
        }
        public void CreatePowFunction() // Метод получения точек графика степенной функции в соответствии с коэффициентами
        {
            List<double> dataList = GetAllDataFromTextBox(txtBoxFunction);
            if (txtBoxFunction.Text == "" || dataList.Count == 0)
                return;
            for (double x = -20; x < 20; x += 5)
            {
                points.Add(new MyPoint((float)x, (float)(dataList[0] * Math.Pow(dataList[1], x) + dataList[2]))); //A * B^x + C
            }
        }
        public void CreateCircle() // Метод получения точек графика окружности в соответствии с коэффициентами
        {
            List<double> dataList = GetAllDataFromTextBox(txtBoxFunction);
            if (txtBoxFunction.Text == "" || dataList.Count == 0)
                return;
            double r = dataList[0];
            for (double x = -r; x <= r; x++)
            {
                float y = (float)Math.Sqrt(r * r - x * x);
                points.Add(new MyPoint((float)x, y)); // x^2 + y^2 = r^2 => y = +sqrt(r^2 - x^2);
            }
            for (double x = r; x >= -r; x--)
            {
                float y = (float)Math.Sqrt(r * r - x * x);
                points.Add(new MyPoint((float)x, -y)); // x^2 + y^2 = r^2 => y = -sqrt(r^2 - x^2);
            }
        }
        public List<double> GetAllDataFromTextBox(TextBox txtBox) // Метод получения входящих данных
        {
            List<double> x = new List<double>();
            string[] arrX;
            if (txtBox.Text != "")
            {
                try
                {
                    arrX = txtBox.Text.Split(' ');
                    foreach (string number in arrX)
                    {
                        x.Add(Convert.ToDouble(number));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    x.Clear();
                    return x;
                }
            }
            if (InterpolarityPanel.Enabled && txtBox == txtBoxX) // Проверим на x[i] = x[j]
            {
                for (int i = 0; i < x.Count; i++)
                {
                    for (int j = 0; j < x.Count; j++)
                    {
                        if (i == j)
                            continue;
                        if (x[i] == x[j])
                        {
                            x.Clear();
                            MessageBox.Show("Input data is not correct. x[i] = x[j]");
                            goto Exit;
                        }
                    }
                }
            }
            Exit:
                return x;
        }
        private void splitContainer1_Panel2_MouseMove(object sender, MouseEventArgs e) // Метод (event) движения мыши
        {
            txtCoordinates.Text = "X: " + (e.X - O.X) + " Y " + (O.Y - e.Y);
        }
        // Какая панель активна?
        private void rbInterpolarity_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton checkedRadioButton = (sender as RadioButton);
            if (checkedRadioButton.Checked)
            {
                InterpolarityPanel.Enabled = true;
                SimpleFunctionPanel.Enabled = false;
            }
        }
        private void rbSimpleFunction_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton checkedRadioButton = (sender as RadioButton);
            if (checkedRadioButton.Checked)
            {
                InterpolarityPanel.Enabled = false;
                SimpleFunctionPanel.Enabled = true;
            }
        }
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            // Если изменились размеры формы, заменим размеры splitContainer
            // И координаты центры
            splitContainer1.Size = this.Size;
            O.X = splitContainer1.Panel2.Width / 2;
            O.Y = splitContainer1.Panel2.Height / 2;
            if (XYwasDrawn)
            {
                foreach (Label label in numberLabels) // Удалим старые координаты осей
                {
                    splitContainer1.Panel2.Controls.Remove(label);
                }
                numberLabels.Clear();
                DrawXY();
            }
            if (FunctionWasDrawn)
                DrawFunction(); // Построим функцию занова
        }
        private void Form1_Paint(object sender, PaintEventArgs e) // Событие вызванное к примеру при минимизации или скрытия части приложения
        {
            if (!MethodOnPaint)
                return;
            if (FunctionWasDrawn)
            {
                DrawFunction();
            }
            if (XYwasDrawn)
            {
                DrawXY();
            }
        }
        private void btnClear_Click(object sender, EventArgs e) // Кнопка Clear
        {
            splitContainer1.Panel2.Invalidate();
            points.Clear();
            Form1_ResizeEnd(sender, e);  
        }
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e) // Движения разделителя splitter
        {
            O.X = splitContainer1.Panel2.Width / 2;
            O.Y = splitContainer1.Panel2.Height / 2;
            if (FunctionWasDrawn)
            {
                DrawFunction();
            }
            if (XYwasDrawn)
            {
                DrawXY();
            }
        }      
    }
    public class MyPoint // Класс описывающий точку на плоскости
    {
        public float X { get; set; }
        public float Y { get; set; }
        public MyPoint(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}