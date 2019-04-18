using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;

namespace ER_W
{
    public class RelationAttribute : Shape
    {
        private static int attributesIDCounter = 0;
        private const int attributeWidth = 120;
        private const int attributeHeight = 60;
        private const int distanceFromRelation = 200;

        private Ellipse ellipse;
        public Line line;
        private Label label;
        private Border border;

        public static RelationAttribute SelectedAttribute { get; set; }
        public static RelationAttribute ChangeableAttribute { get; set; }

        public int Id { get; }
        public string Name { get; set; }
        public Relation Relation { get; }
        public bool isMoveAttribute { get; set; }

        public RelationAttribute(Relation relation)
        {
            Id = RelationAttribute.attributesIDCounter++;
            Relation = relation;
            Name = "Attribute: " + Id;

            this.DrawAttribute();
        }

        private void DrawAttribute()
        {
            PositionX = this.Relation.PositionX + distanceFromRelation;
            PositionY = this.Relation.PositionY;
            ellipse = new Ellipse()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Width = attributeWidth,
                Height = attributeHeight,
                Fill = Brushes.Transparent
            };
            line = new Line()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.Transparent,
                X1 = this.Relation.PositionX + Relation.rhombusWidth / 2,
                Y1 = this.Relation.PositionY + Relation.rhombusHeight / 2,
                X2 = this.PositionX + attributeWidth / 2 - 5,
                Y2 = this.PositionY + attributeHeight / 2 - 5
            };
            label = new Label()
            {
                Content = this.Name,
                Width = attributeWidth - 20,
                Height = attributeHeight - 20,
                Background = new SolidColorBrush(Colors.White),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            border = new Border()
            {
                BorderThickness = new Thickness(10, 10, 10, 10),
                BorderBrush = Brushes.White,
                CornerRadius = new CornerRadius(200,200,200,200),
                Width = attributeWidth,
                Height = attributeHeight
            };
            border.Child = label;
           
            Canvas.SetLeft(ellipse, PositionX);
            Canvas.SetTop(ellipse, PositionY);
            Canvas.SetLeft(border, PositionX);
            Canvas.SetTop(border, PositionY);
            Canvas.SetZIndex(ellipse, (int)2);
            Canvas.SetZIndex(border, (int)1);         
            Canvas.SetZIndex(line, (int)0);
            Canvas.Children.Add(ellipse);
            Canvas.Children.Add(line);
            Canvas.Children.Add(border);
            ellipse.MouseDown += Attribute_MouseDown;
            ellipse.MouseUp += Attribute_MouseUp;
        }
        private void Attribute_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                RelationAttribute.ChangeableAttribute = this;
                RelationAttribute.SelectedAttribute = this;
                this.isMoveAttribute = true;

                Window.EntityPropertiesVisibility(Visibility.Hidden);
                Window.RelationPropertiesVisibility(Visibility.Hidden);
                Window.AttributePropertiesVisibility(Visibility.Visible);
                Window.attributeNameTxtbox.Text = this.Name;
            }
        }

        private void Attribute_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.isMoveAttribute = false;
            RelationAttribute.SelectedAttribute = null;
        }

        public void Move(double X, double Y, bool? isMovingRelation)
        {
            if (!isMovingRelation.HasValue || !isMovingRelation.Value)
            {
                this.PositionX = X - attributeWidth / 2;
                this.PositionY = Y - attributeHeight / 2 - 5;
            }
            else
            {
                this.PositionX = X;
                this.PositionY = Y;
            }

            Canvas.SetLeft(this.ellipse, PositionX);
            Canvas.SetTop(this.ellipse, PositionY);
            Canvas.SetLeft(border, PositionX);
            Canvas.SetTop(border, PositionY);
            ConnectAttributeToRelation();
        }

        public void ConnectAttributeToRelation()
        {
            line.X1 = this.Relation.PositionX + Relation.rhombusWidth / 2;
            line.Y1 = this.Relation.PositionY + Relation.rhombusHeight / 2;
            line.X2 = this.PositionX + attributeWidth / 2 - 5;
            line.Y2 = this.PositionY + attributeHeight / 2 - 5;
        }

        public void ChangeAttributeName(string newName)
        {
            this.Name = newName;
            this.label.Content = newName;
        }
    }
}
