using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppCaravana.Data;
using AppCaravana.Models;

namespace AppCaravana.Views
{
    public partial class CaravanasView : UserControl
    {
        public CaravanasView()
        {
            InitializeComponent();
            CargarCaravanas();
        }

        private void CargarCaravanas()
        {
            using (var db = new AppDbContext())
            {
                dgCaravanas.ItemsSource = db.Caravanas
                    .OrderBy(c => c.Marca)
                    .ToList();
            }
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            CargarCaravanas();
        }

        private void BtnNuevaCaravana_Click(object sender, RoutedEventArgs e)
        {
            CaravanaFormWindow form = new CaravanaFormWindow();
            form.ShowDialog();
            CargarCaravanas();
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            Caravana caravana = (sender as Button).DataContext as Caravana;
            if (caravana == null) return;

            CaravanaFormWindow form = new CaravanaFormWindow(caravana.Id);
            form.ShowDialog();
            CargarCaravanas();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            Caravana caravana = (sender as Button).DataContext as Caravana;
            if (caravana == null) return;

            if (MessageBox.Show("¿Seguro que desea eliminar esta caravana?",
                "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var db = new AppDbContext())
                {
                    db.Caravanas.Remove(caravana);
                    db.SaveChanges();
                }

                CargarCaravanas();
            }
        }
    }
}
