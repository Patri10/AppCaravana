using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppCaravana.Data;
using AppCaravana.Models;
using Microsoft.EntityFrameworkCore;

namespace AppCaravana.Views
{
    public partial class VentasView : UserControl
    {
        public VentasView()
        {
            InitializeComponent();
            CargarVentas();
        }

        private void CargarVentas()
        {
            using (var db = new AppDbContext())
            {
                dgVentas.ItemsSource = db.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.VentaCaravanas)
                        .ThenInclude(vc => vc.Caravana)
                    .OrderByDescending(v => v.Fecha)
                    .ToList();
            }
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            CargarVentas();
        }

        private void BtnNuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            VentaFormWindow form = new VentaFormWindow();
            form.ShowDialog();
            CargarVentas();
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            Venta venta = (sender as Button).DataContext as Venta;
            if (venta == null) return;

            VentaFormWindow form = new VentaFormWindow(venta.Id);
            form.ShowDialog();
            CargarVentas();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            Venta venta = (sender as Button).DataContext as Venta;
            if (venta == null) return;

            if (MessageBox.Show("¿Desea eliminar esta venta?",
                "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var db = new AppDbContext())
                {
                    var ventaDb = db.Ventas
                        .Include(v => v.VentaCaravanas)
                        .First(v => v.Id == venta.Id);

                    // Liberar caravanas si no están en otras ventas
                    foreach (var item in ventaDb.VentaCaravanas)
                    {
                        bool usadaEnOtraVenta = db.VentaCaravanas.Any(vc => vc.CaravanaId == item.CaravanaId && vc.VentaId != ventaDb.Id);
                        if (!usadaEnOtraVenta)
                        {
                            var car = db.Caravanas.First(c => c.Id == item.CaravanaId);
                            car.Disponible = true;
                        }
                    }

                    db.Ventas.Remove(ventaDb);
                    db.SaveChanges();
                }

                CargarVentas();
            }
        }
    }
}
