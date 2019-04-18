using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
/*
 * Contact:
 * Email: varujik10@gmail.com
 */
namespace ER_W
{
    public partial class MainWindow : Window
    {
        // Options
        public static bool isDrawEntity = false;
        public bool isConnectionEnabled = false;
        //
        public int entitiesIDCounter = 0;
        public int relationsIDCounter = 0;
        public List<Entity> entities = new List<Entity>();
        //
        public MainWindow()
        {
            InitializeComponent();
            this.RelationPropertiesVisibility(Visibility.Hidden);
            this.EntityPropertiesVisibility(Visibility.Hidden);
            this.AttributePropertiesVisibility(Visibility.Hidden);
            Shape.Canvas = canvas;
            Shape.Window = mainWindow;
        }

        private void createEntityBtn_Click(object sender, RoutedEventArgs e)
        {
            isDrawEntity = true;
        }

        private void connectEntityBtn_Click(object sender, RoutedEventArgs e)
        {
            isConnectionEnabled = true;
        }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDrawEntity)
            {
                Entity entity = new Entity(e.GetPosition(canvas).X, e.GetPosition(canvas).Y, entitiesIDCounter++, Brushes.Aqua);
                entities.Add(entity);
                isDrawEntity = false;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //canvas.MinWidth = mainWindow.Width;
            //canvas.MinHeight = mainWindow.Height;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            label.Content = e.GetPosition(canvas).X + " : " + e.GetPosition(canvas).Y;
            if (Entity.SelectedEntity != null && Entity.SelectedEntity.isMoveEntity)
            {
                Entity.SelectedEntity.Move(e.GetPosition(canvas).X, e.GetPosition(canvas).Y);
            }
            if (Relation.SelectedRelation != null && Relation.SelectedRelation.isMoveConnection)
            {
                Relation.SelectedRelation.Move(e.GetPosition(canvas).X, e.GetPosition(canvas).Y);
            }
            if (RelationAttribute.SelectedAttribute != null && RelationAttribute.SelectedAttribute.isMoveAttribute)
            {
                RelationAttribute.SelectedAttribute.Move(e.GetPosition(canvas).X, e.GetPosition(canvas).Y, null);
            }
        }

        private void comboBoxFirstEntity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Entity newEntity = comboBoxFirstEntity.SelectedItem as Entity;
            if (Relation.ChangeableRelation != null && newEntity != null)
            {
                Entity oldEntity = Relation.ChangeableRelation.FirstEntity;
                
                oldEntity.Relations.Remove(Relation.ChangeableRelation);
                newEntity.Relations.Add(Relation.ChangeableRelation);
                //
                Relation.ChangeableRelation.FirstEntity = newEntity;
                Relation.ChangeableRelation.UpdateConnection();
            }
        }

        private void comboBoxSecondEntity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Entity newEntity = comboBoxSecondEntity.SelectedItem as Entity;
            if (Relation.ChangeableRelation != null && newEntity != null)
            {
                Entity oldEntity = Relation.ChangeableRelation.SecondEntity;

                oldEntity.Relations.Remove(Relation.ChangeableRelation);
                newEntity.Relations.Add(Relation.ChangeableRelation);
                //
                Relation.ChangeableRelation.SecondEntity = newEntity;
                Relation.ChangeableRelation.UpdateConnection();
            }
        }

        public void RelationPropertiesVisibility(Visibility type)
        {
            comboBoxFirstEntity.Visibility = type;
            comboBoxSecondEntity.Visibility = type;

            cardinalNumberEntityOneLabel.Visibility = type;
            cardinalNumberEntityTwoLabel.Visibility = type;

            cardinalNumberEntityOneCombobox.Visibility = type;
            cardinalNumberEntityTwoCombobox.Visibility = type;

            relationTypeLabel.Visibility = type;
            relationTypeCombobox.Visibility = type;

            firstEntityLabel.Visibility = type;
            secondEntityLabel.Visibility = type;

            addAttributeBtn.Visibility = type;
        }

        public void EntityPropertiesVisibility(Visibility type)
        {
            entityNameLabel.Visibility = type;
            entityNameTxtbox.Visibility = type;
        }

        public void AttributePropertiesVisibility(Visibility type)
        {
            attributeNameLabel.Visibility = type;
            attributeNameTxtbox.Visibility = type;

        }
        private void entityNameTxtbox_KeyUp(object sender, KeyEventArgs e)
        {
            Entity.ChangeableEntity.ChangeEntityName(entityNameTxtbox.Text);
        }

        private void cardinalNumberEntityOneCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateCardinalNumber(sender as ComboBox, ref Relation.ChangeableRelation.EntityOneCardinalOne, ref Relation.ChangeableRelation.EntityOneCardinalZero);
        }

        private void cardinalNumberEntityTwoCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateCardinalNumber(sender as ComboBox, ref Relation.ChangeableRelation.EntityTwoCardinalOne, ref Relation.ChangeableRelation.EntityTwoCardinalZero);
        }
        public void UpdateCardinalNumber(ComboBox combobox, ref Line line, ref Ellipse circle)
        {
            string cardinalNumber;
                cardinalNumber = (combobox.SelectedItem as Label).Content.ToString();
            if (line != null)
                canvas.Children.Remove(line);
            if (circle != null)
                canvas.Children.Remove(circle);
            switch (cardinalNumber)
            {
                case "One":
                    line = new Line();
                    circle = null;
                    canvas.Children.Add(line);
                    break;
                case "Zero":
                    line = null;
                    circle = new Ellipse();
                    canvas.Children.Add(circle);
                    break;
                case "None":
                    line = null;
                    circle = null;
                    break;
            }
            Relation.ChangeableRelation.UpdateConnection();
        }

        private void relationTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combobox = sender as ComboBox;
            Relation.ChangeableRelation.ChangeRelationType((combobox.SelectedItem as Label).Content.ToString());
        }

        private void addAttributeBtn_Click(object sender, RoutedEventArgs e)
        {
            Relation.ChangeableRelation.AddAttribute();
        }

        private void attributeNameTxtbox_KeyUp(object sender, KeyEventArgs e)
        {
            RelationAttribute.ChangeableAttribute.ChangeAttributeName(attributeNameTxtbox.Text);
        }
    }
}
