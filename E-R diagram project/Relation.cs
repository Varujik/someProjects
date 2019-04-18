using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ER_W
{
    public class Relation : Shape
    {
        public const int rhombusWidth = 75;
        public const int rhombusHeight = 75;
        public const int cardinalZeroWidth = 30;
        public const int cardinalZeroHeight = 30;
        private const int labelWidth = rhombusWidth / 2;
        private const int labelHeight = rhombusHeight / 2;
        private const int fontSize = 15;
        private Label TypeLabel;
        private RelationType Type { get; set; }
        private Line LineOne { get; set; }
        private Line LineTwo { get; set; }

        public static Relation SelectedRelation { get; set; }
        public static Relation ChangeableRelation { get; set; }

        public Entity FirstEntity { get; set; }
        public Entity SecondEntity { get; set; }
        public int RelationID { get; set; }
        public List<RelationAttribute> Attributes { get; set; }
        public Line EntityOneCardinalOne;
        public Line EntityTwoCardinalOne;
        public Ellipse EntityOneCardinalZero; 
        public Ellipse EntityTwoCardinalZero;
        


        public Image RhombusImage { get; set; }

        public bool isMoveConnection = false;

        public Relation(Entity FE, Entity SE, int ID)
        {
            try
            {
                FirstEntity = FE;
                SecondEntity = SE;
                this.RelationID = ID;
                //this.Name = "1:N";//"Relation" + this.RelationID;
                Attributes = new List<RelationAttribute>();
                ConnectEntities(FE.PositionX, FE.PositionY, SE.PositionX, SE.PositionY);

                RhombusImage.MouseDown += RhombusImage_MouseDown;
                RhombusImage.MouseUp += RhombusImage_MouseUp;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RhombusImage_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Relation.SelectedRelation = null;
            this.isMoveConnection = false;
        }

        private void RhombusImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Relation.ChangeableRelation = this;
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Relation.SelectedRelation = this;
                this.isMoveConnection = true;

                Window.comboBoxFirstEntity.Items.Clear();
                Window.comboBoxSecondEntity.Items.Clear();
                foreach (Entity entity in Window.entities)
                {
                    Window.comboBoxFirstEntity.Items.Add(entity);
                    Window.comboBoxSecondEntity.Items.Add(entity);
                }
                Window.comboBoxFirstEntity.SelectedItem = this.FirstEntity;
                Window.comboBoxSecondEntity.SelectedItem = this.SecondEntity;
                Window.relationTypeCombobox.SelectedIndex = 4;
                if (this.EntityOneCardinalOne != null)
                    Window.cardinalNumberEntityOneCombobox.SelectedIndex = 0;
                else if (this.EntityOneCardinalZero != null)
                    Window.cardinalNumberEntityOneCombobox.SelectedIndex = 1;
                else
                    Window.cardinalNumberEntityOneCombobox.SelectedIndex = 2;

                if (this.EntityTwoCardinalOne != null)
                    Window.cardinalNumberEntityTwoCombobox.SelectedIndex = 0;
                else if (this.EntityTwoCardinalZero != null)
                    Window.cardinalNumberEntityTwoCombobox.SelectedIndex = 1;
                else
                    Window.cardinalNumberEntityTwoCombobox.SelectedIndex = 2;

                Window.EntityPropertiesVisibility(Visibility.Hidden);
                Window.AttributePropertiesVisibility(Visibility.Hidden);
                Window.RelationPropertiesVisibility(Visibility.Visible);
            }
        }

        public void ConnectEntities(double X1, double Y1, double X2, double Y2)
        {
            RhombusImage = new Image();
            RhombusImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/rhombus.png"));
            RhombusImage.Width = rhombusWidth;
            RhombusImage.Height = rhombusHeight;
            RhombusImage.Stretch = Stretch.Fill;
            PositionX = (FirstEntity.PositionX + SecondEntity.PositionX) / 2;
            PositionY = (FirstEntity.PositionY + SecondEntity.PositionY) / 2;

            LineOne = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.Transparent,
                X1 = FirstEntity.PositionX,
                Y1 = FirstEntity.PositionY,
                X2 = PositionX,
                Y2 = PositionY + rhombusHeight / 2
            };
            LineTwo = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.Transparent,
                X1 = SecondEntity.PositionX,
                Y1 = SecondEntity.PositionY,
                X2 = PositionX + rhombusWidth,
                Y2 = PositionY + rhombusHeight / 2
            };
            TypeLabel = new Label
            {
                Content = "",
                Width = labelWidth,
                Height = labelHeight,
                Background = new SolidColorBrush(Colors.Transparent),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                FontSize = fontSize
            };
            Canvas.SetLeft(RhombusImage, PositionX);
            Canvas.SetTop(RhombusImage, PositionY);
            Canvas.SetLeft(TypeLabel, PositionX + labelWidth / 2);
            Canvas.SetTop(TypeLabel, PositionY + labelHeight / 2);
            Canvas.SetZIndex(RhombusImage, (int)2);
            Canvas.SetZIndex(TypeLabel, (int)1);
            Canvas.Children.Add(RhombusImage);
            Canvas.Children.Add(LineOne);
            Canvas.Children.Add(LineTwo);
            Canvas.Children.Add(TypeLabel);
            this.UpdateConnection();
        }

        public void UpdateConnection()
        {
            Point pointOne = GetClosestCoordinates(FirstEntity.PositionX, FirstEntity.PositionY, PositionX, PositionY + rhombusHeight / 2);
            Point pointTwo = GetClosestCoordinates(SecondEntity.PositionX, SecondEntity.PositionY, PositionX + rhombusWidth, PositionY + rhombusHeight / 2);

            LineOne.X1 = pointOne.X;
            LineOne.Y1 = pointOne.Y;


            LineOne.X2 = PositionX;
            LineOne.Y2 = PositionY + rhombusHeight / 2;

            LineTwo.X1 = pointTwo.X;
            LineTwo.Y1 = pointTwo.Y;

            LineTwo.X2 = PositionX + rhombusWidth;
            LineTwo.Y2 = PositionY + rhombusHeight / 2;

            DrawCardinalOne(new Point(LineTwo.X1, LineTwo.Y1), new Point(LineTwo.X2, LineTwo.Y2), ref EntityOneCardinalOne);
            DrawCardinalOne(new Point(LineOne.X1, LineOne.Y1), new Point(LineOne.X2, LineOne.Y2), ref EntityTwoCardinalOne);

            DrawCardinalZero(new Point(LineTwo.X1, LineTwo.Y1), new Point(LineTwo.X2, LineTwo.Y2), ref EntityOneCardinalZero);
            DrawCardinalZero(new Point(LineOne.X1, LineOne.Y1), new Point(LineOne.X2, LineOne.Y2), ref EntityTwoCardinalZero);
        }

        public void Move(double X, double Y)
        {
            var differenceX = this.PositionX;
            var differenceY = this.PositionY;
            this.PositionX = X - rhombusWidth / 2;
            this.PositionY = Y - rhombusHeight / 2;

            differenceX = this.PositionX - differenceX;
            differenceY = this.PositionY - differenceY;

            Canvas.SetLeft(Relation.SelectedRelation.RhombusImage, PositionX);
            Canvas.SetTop(Relation.SelectedRelation.RhombusImage, PositionY);
            Canvas.SetLeft(this.TypeLabel, PositionX + labelWidth / 2);
            Canvas.SetTop(this.TypeLabel, PositionY + labelHeight / 2);

            this.UpdateConnection();

            foreach(var attribute in Attributes)
            {
                attribute.Move(attribute.PositionX + differenceX, attribute.PositionY + differenceY, true);
            }
        }

        public Point GetClosestCoordinates(double entityX, double entityY, double rhombusX, double rhombusY)
        {
            Point point = new Point();
            if (entityX >= rhombusX && entityY >= rhombusY)
            {
                point.X = entityX;
                point.Y = entityY;
            }
            if (entityX <= rhombusX && entityY >= rhombusY)
            {
                point.X = entityX + Entity.entityWidth;
                point.Y = entityY;
            }
            if (entityX <= rhombusX && entityY <= rhombusY)
            {
                point.X = entityX + Entity.entityWidth;
                point.Y = entityY + Entity.entityHeight;
            }
            if (entityX >= rhombusX && entityY <= rhombusY)
            {
                point.X = entityX;
                point.Y = entityY + Entity.entityHeight;
            }
            return point;
        }
        public void DrawCardinalOne(Point p1, Point p2, ref Line line)
        {
            if (line == null)
                return;

            Point midpoint = new Point();

            // Vipovot sawyisi wrfe. (y2 - y1) / (x2 - x1) = k
            double k = (double)(p2.Y - p1.Y) / (double)(p2.X - p1.X);

            // Perpendikularuli wrfis k
            if (k != 0)
                k = -1 / k;

            midpoint.X = (p1.X + p2.X) / 2;
            midpoint.Y = (p1.Y + p2.Y) / 2;

            // b == martobulis b
            double b = -k * midpoint.X + midpoint.Y;
            
            List<Point> points = this.GetTwoCoordinatesOnDistance(midpoint.X, k, b, 10);

            line.Stroke = Brushes.Black;
            line.StrokeThickness = 2;
            line.Fill = Brushes.Transparent;
            line.X1 = points[0].X;
            line.Y1 = points[0].Y;
            line.X2 = points[1].X;
            line.Y2 = points[1].Y;
        }
        public List<Point> GetTwoCoordinatesOnDistance(double x1, double k, double b, double distance)
        {
            double D = 4 * x1 * x1 - 4 * (x1 * x1 - (distance * distance / (1 + k * k)));

            double X1 = (2 * x1 + Math.Sqrt(D)) / 2;
            double Y1 = k * X1 + b;

            double X2 = (2 * x1 - Math.Sqrt(D)) / 2;
            double Y2 = k * X2 + b;

            var res = new List<Point>
            {
                new Point(X1,Y1),
                new Point(X2,Y2)
            };
            return res;
        }
        public void DrawCardinalZero(Point p1, Point p2, ref Ellipse circle)
        {
            if (circle == null)
                return;
            Point midpoint = new Point();

            midpoint.X = (p1.X + p2.X) / 2 - cardinalZeroWidth / 2;
            midpoint.Y = (p1.Y + p2.Y) / 2 - cardinalZeroHeight / 2;

            circle.Stroke = Brushes.Black;
            circle.StrokeThickness = 2;
            circle.Width = cardinalZeroWidth;
            circle.Height = cardinalZeroHeight;
            Canvas.SetLeft(circle, midpoint.X);
            Canvas.SetTop(circle, midpoint.Y);
        }
        //public void ChangeRelationName(string newName)
        //{
        //    this.Name = newName;
        //    this.NameLabel.Content = newName;
        //}
        public void ChangeRelationType(string newType)
        {
            switch (newType)
            {
                case "1:1":
                    this.Type = RelationType.OneOne;
                    break;
                case "1:N":
                    this.Type = RelationType.OneN;
                    break;
                case "N:1":
                    this.Type = RelationType.NOne;
                    break;
                case "N:M":
                    this.Type = RelationType.NM;
                    break;
            }
            if (newType != "")
                this.TypeLabel.Content = this.GetRelationTypeString(this.Type);
        }

        public string GetRelationTypeString(RelationType type)
        {
            switch (type)
            {
                case RelationType.OneOne:
                    return "1:1";
                case RelationType.OneN:
                    return "1:N";
                case RelationType.NOne:
                    return "N:1";
                case RelationType.NM:
                    return "N:M";
            }
            return "";
        }

        public void AddAttribute()
        {
            RelationAttribute newAttribute = new RelationAttribute(this);
            this.Attributes.Add(newAttribute);
        }
    }
}
