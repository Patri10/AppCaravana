using System;
using System.Linq;
using System.Windows;
using AppCaravana.Data;
using AppCaravana.Models;

namespace AppCaravana.Views
{
    public partial class CaravanaFormWindow : Window
    {
        private int? _caravanaId;

        public CaravanaFormWindow(int? caravanaId = null)
        {
            InitializeComponent();
            _caravanaId = caravanaId;

            if (_caravanaId.HasValue)
                CargarCaravana();
        }

        private void CargarCaravana()
        {
            using (var db = new AppDbContext())
            {
                var caravana = db.Caravanas.FirstOrDefault(c => c.Id == _caravanaId.Value);
                if (caravana == null) return;

                txtSerie.Text = caravana.Serie;
                txtMarca.Text = caravana.Marca;
                txtModelo.Text = caravana.Modelo;
                txtAño.Text = caravana.Año.ToString();
                txtMatricula.Text = caravana.Matricula;
                txtSenasa.Text = caravana.NumeroSenasa;
                txtTipo.Text = caravana.Tipo;
                txtPrecio.Text = caravana.Precio.ToString();
                chkDisponible.IsChecked = caravana.Disponible;
                txtCaracteristicas.Text = caravana.Caracteristicas;
                txtDescripcion.Text = caravana.Descripcion;
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(txtSerie.Text) ||
                    string.IsNullOrWhiteSpace(txtMarca.Text) ||
                    string.IsNullOrWhiteSpace(txtModelo.Text) ||
                    string.IsNullOrWhiteSpace(txtAño.Text) ||
                    string.IsNullOrWhiteSpace(txtMatricula.Text) ||
                    string.IsNullOrWhiteSpace(txtSenasa.Text) ||
                    string.IsNullOrWhiteSpace(txtTipo.Text) ||
                    string.IsNullOrWhiteSpace(txtPrecio.Text))
                {
                    MessageBox.Show("Todos los campos son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new AppDbContext())
                {
                    Caravana caravana;

                    if (_caravanaId.HasValue)
                        caravana = db.Caravanas.First(c => c.Id == _caravanaId.Value);
                    else
                        caravana = new Caravana();

                    // Asignación con conversiones seguras
                    caravana.Serie = txtSerie.Text.Trim();
                    caravana.Marca = txtMarca.Text.Trim();
                    caravana.Modelo = txtModelo.Text.Trim();
                    caravana.Año = int.Parse(txtAño.Text.Trim());
                    caravana.Matricula = txtMatricula.Text.Trim();
                    caravana.NumeroSenasa = txtSenasa.Text.Trim();
                    caravana.Tipo = txtTipo.Text.Trim();
                    caravana.Precio = decimal.Parse(txtPrecio.Text.Trim());
                    caravana.Disponible = chkDisponible.IsChecked ?? false;
                    caravana.Caracteristicas = txtCaracteristicas.Text?.Trim() ?? string.Empty;
                    caravana.Descripcion = txtDescripcion.Text?.Trim() ?? string.Empty;

                    // Guardar
                    if (_caravanaId.HasValue == false)
                        db.Caravanas.Add(caravana);

                    db.SaveChanges();
                }

                MessageBox.Show("Caravana guardada correctamente.");
                Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Por favor, verifica que Año y Precio sean números válidos.", "Error de formato", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar caravana: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
