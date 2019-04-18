using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace ER_W
{
    public class Entity : Shape
    {
        public const int entityWidth = 200;
        public const int entityHeight = 100;
        public const int labelWidth = entityWidth;
        public const int labelHeight = (entityHeight / 2);
        public static Entity SelectedEntity { get; set; }
        public static Entity ChangeableEntity { get; set; }

        public static Entity FirstEntity { get; set; }
        public static Entity SecondEntity { get; set; }

        
        private Rectangle Rectangle;
        private Label NameLabel { get; set; }
        private Brush Color { get; set; }

        private int EntityID { get; set; }
        public string Name { get; set; }
        
        
        public bool isMoveEntity = false;

        public List<Relation> Relations { get; set; }

        public Entity(double X, double Y, int ID, Brush color)
        {
            Relations = new List<Relation>();

            this.PositionX = X - entityWidth / 2;
            this.PositionY = Y - entityHeight / 2;
            this.Color = color;
            this.EntityID = ID;
            this.Name = "Unknown" + this.EntityID;

            this.DrawEntity();
        }

        private void DrawEntity()
        {
            Rectangle = new Rectangle
            {
                Stroke = this.Color,
                StrokeThickness = 2,
                Width = entityWidth,
                Height = entityHeight,
                Fill = Brushes.Transparent
            };
            NameLabel = new Label
            {
                Content = this.Name,
                Width = labelWidth,
                Height = labelHeight,
                //Background = new SolidColorBrush(Colors.Green),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(Rectangle, PositionX);
            Canvas.SetTop(Rectangle, PositionY);
            Canvas.SetLeft(NameLabel, PositionX);
            Canvas.SetTop(NameLabel, PositionY + (entityHeight / 4));
            Canvas.SetZIndex(Rectangle, (int)2);
            Canvas.SetZIndex(NameLabel, (int)1);

            Canvas.Children.Add(Rectangle);
            Canvas.Children.Add(NameLabel);
            Rectangle.MouseDown += Rectangle_MouseDown;
            Rectangle.MouseUp += Rectangle_MouseUp;
        }

        private void Rectangle_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (Window.isConnectionEnabled && Entity.FirstEntity != null)
                {
                    Entity.SecondEntity = this;
                    Window.isConnectionEnabled = false;

                    Relation Relation = new Relation(Entity.FirstEntity, Entity.SecondEntity, Window.relationsIDCounter++);
                    Entity.FirstEntity.Relations.Add(Relation);
                    Entity.SecondEntity.Relations.Add(Relation);
                }
                else
                {
                    this.isMoveEntity = false;
                    Entity.SelectedEntity = null;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            } 
        }

        private void Rectangle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Entity.ChangeableEntity = this;

                if (Window.isConnectionEnabled)
                {
                    Entity.FirstEntity = this;
                }
                else if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                {
                    Entity.SelectedEntity = this;
                    this.isMoveEntity = true;

                    Window.EntityPropertiesVisibility(Visibility.Visible);
                    Window.RelationPropertiesVisibility(Visibility.Hidden);
                    Window.AttributePropertiesVisibility(Visibility.Hidden);
                    Window.entityNameTxtbox.Text = this.Name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        public void Move(double X, double Y)
        {
            this.PositionX = X - entityWidth / 2;
            this.PositionY = Y - entityHeight / 2;

            Canvas.SetLeft(this.Rectangle, PositionX);
            Canvas.SetTop(this.Rectangle, PositionY);
            Canvas.SetLeft(this.NameLabel, PositionX);
            Canvas.SetTop(NameLabel, PositionY + (entityHeight / 4));

            foreach (Relation relation in Relations)
                relation.UpdateConnection();
        }
        public void ChangeEntityName(string newName)
        {
            this.Name = newName;
            this.NameLabel.Content = newName;
        }
        public override string ToString()
        {
            return this.Name;
        }
    }
}
