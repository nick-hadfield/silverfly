using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Silverfly.Example.Domain;
using System.Threading;

namespace Silverfly.Example
{
    /// <summary>
    /// The main window.
    /// </summary>
    public partial class MainWindow : Window
    {
        private IBus _bus;

        private Action<object, Domain.Animal> _animalHandler;

        private Action<object, Domain.Shape> _shapeHandler;

        private Action<object, object> _objectHandler;

        public MainWindow(IBus bus)
        {
            InitializeComponent();
            _bus = bus;

            _animalHandler = AnimalNotifier;
            _shapeHandler = ShapeNotifier;
            _objectHandler = ObjectNotifier;

            _shapeHandler = _shapeHandler.Marshall();
            _objectHandler = _objectHandler.Marshall();

            _bus.Subscribe(_animalHandler);
            _bus.Subscribe(_shapeHandler);
            _bus.Subscribe(_objectHandler);
        }

        private void AnimalNotifier(object sender, Domain.Animal animal)
        {
            _animalOutput.Text += animal.Description + "\n";
            System.Diagnostics.Debug.Print("Animal - " + animal.Description);
        }
        private void ShapeNotifier(object sender, Domain.Shape shape)
        {
            _shapeOutput.Text += shape.Description + "\n";
            System.Diagnostics.Debug.Print("Shape - " + shape.Description);
        }
        private void ObjectNotifier(object sender, object o)
        {
            _objectOutput.Text += o.GetType().ToString() + "\n";
            System.Diagnostics.Debug.Print("Object - " + o.GetType().ToString());
        }

        private void _dogButton_Click(object sender, RoutedEventArgs e)
        {
            _bus.Publish(this, new Dog());
        }

        private void _catButton_Click(object sender, RoutedEventArgs e)
        {
            _bus.Publish(this, new Cat());
        }

        private void _fishButton_Click(object sender, RoutedEventArgs e)
        {
            _bus.Publish(this, new Fish());
        }

        private void _squareButton_Click(object sender, RoutedEventArgs e)
        {
            _bus.Publish(this, new Square());
        }

        private void _triangleButton_Click(object sender, RoutedEventArgs e)
        {
            _bus.Publish(this, new Triangle());
        }

        private void _circleButton_Click(object sender, RoutedEventArgs e)
        {
            _bus.Publish(this, new Circle());
        }

        private void _fishOnOtherThread_Click(object sender, RoutedEventArgs e)
        {
            new Thread(
                new ThreadStart(() =>
                {
                    _bus.Publish(this, new Fish());
                }
                )
            ).Start();
        }

        private void _circleOnOtherThread_Click(object sender, RoutedEventArgs e)
        {

            new Thread(
                new ThreadStart(() =>
                {
                    _bus.Publish(this, new Circle());
                }
                )
            ).Start();
        }
    }
}
