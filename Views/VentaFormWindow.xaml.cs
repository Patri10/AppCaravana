using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using AppCaravana.Data;
using AppCaravana.Models;

namespace AppCaravana.Views
{
    public partial class VentaFormWindow : Window
    {
        private int? _ventaId;
        private List<Caravana> _caravanasDisponibles = new();
        private List<Caravana> _caravanasSeleccionadas = new();

        public VentaFormWindow(int? ventaId = null)
        {
            InitializeComponent();
            _ventaId = ventaId;

            CargarClientes();
            CargarCaravanas();

            if (_ventaId.HasValue)
                CargarVenta();
            else
                dpFecha.SelectedDate = DateTime.Now;
        }

        private void CargarClientes()
        {
            using (var db = new AppDbContext())
            {
                cmbCliente.ItemsSource = db.Clientes
                    .OrderBy(c => c.Apellido)
                    .ThenBy(c => c.Nombre)
                    .ToList();
            }
        }

        private void CargarCaravanas()
        {
            using (var db = new AppDbContext())
            {
                // Caravanas disponibles
                _caravanasDisponibles = db.Caravanas
                    .Where(c => c.Disponible == true)
                    .OrderBy(c => c.Serie)
                    .ToList();

                // Si estamos editando, incluir las caravanas ya asociadas a la venta aunque no estén disponibles
                if (_ventaId.HasValue)
                {
                    var caravanasVenta = db.VentaCaravanas
                        .Include(vc => vc.Caravana)
                        .Where(vc => vc.VentaId == _ventaId.Value)
                        .Select(vc => vc.Caravana)
                        .ToList();

                    _caravanasSeleccionadas = caravanasVenta;

                    foreach (var car in caravanasVenta)
                    {
                        if (_caravanasDisponibles.All(c => c.Id != car.Id))
                        {
                            _caravanasDisponibles.Add(car);
                        }
                    }
                }

                RefrescarListas();
            }
        }

        private void CargarVenta()
        {
            using (var db = new AppDbContext())
            {
                var venta = db.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.VentaCaravanas)
                        .ThenInclude(vc => vc.Caravana)
                    .FirstOrDefault(v => v.Id == _ventaId);

                if (venta == null) return;

                cmbCliente.SelectedValue = venta.ClienteId;
                dpFecha.SelectedDate = venta.Fecha;

                // Marcar selección de caravanas de esta venta
                _caravanasSeleccionadas = venta.VentaCaravanas
                    .Select(vc => vc.Caravana)
                    .ToList();

                RefrescarListas();
                RecalcularImporte();
            }
        }

        private void RefrescarListas()
        {
            lstCaravanasDisponibles.ItemsSource = null;
            lstCaravanasDisponibles.ItemsSource = _caravanasDisponibles
                .OrderBy(c => c.Serie)
                .ToList();

            lstCaravanasSeleccionadas.ItemsSource = null;
            lstCaravanasSeleccionadas.ItemsSource = _caravanasSeleccionadas
                .OrderBy(c => c.Serie)
                .ToList();
        }

        private void RecalcularImporte()
        {
            var total = _caravanasSeleccionadas.Sum(c => c.Precio);
            txtImporte.Text = total.ToString("F2");
        }

        private void BtnAgregarCaravanas_Click(object sender, RoutedEventArgs e)
        {
            var seleccion = lstCaravanasDisponibles.SelectedItems.Cast<Caravana>().ToList();
            foreach (var car in seleccion)
            {
                if (_caravanasSeleccionadas.All(c => c.Id != car.Id))
                {
                    _caravanasSeleccionadas.Add(car);
                    _caravanasDisponibles.RemoveAll(c => c.Id == car.Id);
                }
            }

            RefrescarListas();
            RecalcularImporte();
        }

        private void BtnQuitarCaravanas_Click(object sender, RoutedEventArgs e)
        {
            var seleccion = lstCaravanasSeleccionadas.SelectedItems.Cast<Caravana>().ToList();
            foreach (var car in seleccion)
            {
                _caravanasSeleccionadas.RemoveAll(c => c.Id == car.Id);
                if (_caravanasDisponibles.All(c => c.Id != car.Id) && car.Disponible)
                {
                    _caravanasDisponibles.Add(car);
                }
            }

            RefrescarListas();
            RecalcularImporte();
        }

        private void lstCaravanasDisponibles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // No-op; selección usada por botón Agregar
        }

        private void lstCaravanasSeleccionadas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // No-op; selección usada por botón Quitar
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCliente.SelectedValue == null ||
                !_caravanasSeleccionadas.Any())
            {
                MessageBox.Show("Debe seleccionar un cliente y al menos una caravana.");
                return;
            }

            using (var db = new AppDbContext())
            {
                Venta venta;

                if (_ventaId.HasValue)
                {
                    venta = db.Ventas
                        .Include(v => v.VentaCaravanas)
                        .First(v => v.Id == _ventaId.Value);
                }
                else
                {
                    venta = new Venta();
                }

                venta.ClienteId = (int)cmbCliente.SelectedValue;
                venta.Fecha = dpFecha.SelectedDate ?? DateTime.Now;
                venta.Importe = _caravanasSeleccionadas.Sum(c => c.Precio);

                if (!_ventaId.HasValue)
                {
                    db.Ventas.Add(venta);
                }

                db.SaveChanges();

                // Sincronizar ítems de caravanas
                var seleccionIds = _caravanasSeleccionadas.Select(c => c.Id).ToList();

                // Eliminar ítems removidos
                var itemsEliminar = db.VentaCaravanas
                    .Where(vc => vc.VentaId == venta.Id && !seleccionIds.Contains(vc.CaravanaId))
                    .ToList();

                foreach (var item in itemsEliminar)
                {
                    var car = db.Caravanas.First(c => c.Id == item.CaravanaId);

                    // Solo liberar si no está en otras ventas
                    bool usadoEnOtraVenta = db.VentaCaravanas.Any(vc => vc.CaravanaId == car.Id && vc.VentaId != venta.Id);
                    if (!usadoEnOtraVenta)
                    {
                        car.Disponible = true;
                    }
                }

                db.VentaCaravanas.RemoveRange(itemsEliminar);

                // Agregar ítems nuevos
                var itemsExistentesIds = db.VentaCaravanas
                    .Where(vc => vc.VentaId == venta.Id)
                    .Select(vc => vc.CaravanaId)
                    .ToList();

                var idsNuevos = seleccionIds
                    .Where(id => !itemsExistentesIds.Contains(id))
                    .ToList();

                foreach (var id in idsNuevos)
                {
                    var car = db.Caravanas.First(c => c.Id == id);
                    db.VentaCaravanas.Add(new VentaCaravana
                    {
                        VentaId = venta.Id,
                        CaravanaId = car.Id,
                        Importe = car.Precio
                    });

                    car.Disponible = false;
                }

                // Actualizar importe total por si cambió
                venta.Importe = _caravanasSeleccionadas.Sum(c => c.Precio);

                db.SaveChanges();
            }

            MessageBox.Show("Venta guardada correctamente.");
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
