using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SueldoLiquido
{
    public partial class Form1 : Form
    {
        private int? idSeleccionado = null;

        public Form1()
        {
            InitializeComponent();

            Label lblTitulo = new Label();
            lblTitulo.Text = "SISTEMA CÁLCULO SUELDO LÍQUIDO";
            lblTitulo.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            lblTitulo.Dock = DockStyle.Top;
            lblTitulo.Height = 60;
            lblTitulo.ForeColor = Color.FromArgb(0, 123, 94); // Verde elegante
            lblTitulo.BackColor = Color.WhiteSmoke;
            lblTitulo.Margin = new Padding(0);

            this.Controls.Add(lblTitulo);
            lblTitulo.BringToFront();



            // Estilo general
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);
            this.Text = "Cálculo de Sueldo Líquido";

            Action<Button, Color> estilizarBoton = (btn, color) =>
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = color;
                btn.ForeColor = Color.White;
                btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            };

            estilizarBoton(btnCalcular, Color.FromArgb(0, 123, 94));
            estilizarBoton(btnlimpiar, Color.FromArgb(52, 58, 64));
            estilizarBoton(btnmodificar, Color.FromArgb(52, 58, 64));
            estilizarBoton(btneliminar, Color.FromArgb(192, 57, 43));

            // Panel superior para filtros
            cmbTipoPago.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbModalidad.DropDownStyle = ComboBoxStyle.DropDownList;

            // TextBoxes
            TextBox[] inputs = { txtSalarioBase, txtHoras, txtPagoHora, txtVentas, txtComision };
            foreach (var txt in inputs)
            {
                txt.BackColor = Color.White;
                txt.BorderStyle = BorderStyle.FixedSingle;
            }

            tabla1.BackgroundColor = Color.White;
            tabla1.GridColor = Color.Gainsboro;
            tabla1.BorderStyle = BorderStyle.None;
            tabla1.DefaultCellStyle.BackColor = Color.White;
            tabla1.DefaultCellStyle.ForeColor = Color.Black;
            tabla1.DefaultCellStyle.SelectionBackColor = Color.LightSteelBlue;
            tabla1.DefaultCellStyle.SelectionForeColor = Color.Black;
            tabla1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 123, 94);
            tabla1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            tabla1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            tabla1.EnableHeadersVisualStyles = false;


            cmbTipoPago.Items.AddRange(new[] { "Mensual", "Quincenal" });
            cmbModalidad.Items.AddRange(new[] { "Sueldo Base", "Por Horas", "Por Comisión" });

            cmbModalidad.SelectedIndexChanged += CmbModalidad_SelectedIndexChanged;
            btnCalcular.Click += BtnCalcular_Click;
            btnlimpiar.Click += btnlimpiar_Click;
            btneliminar.Click += btneliminar_Click;
            btnmodificar.Click += btnmodificar_Click;

            OcultarCampos();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tabla1.Columns.Clear();
            tabla1.Columns.Add("ID", "ID");
            tabla1.Columns[0].Visible = false;
            tabla1.Columns.Add("TipoPago", "Tipo de Pago");
            tabla1.Columns.Add("Modalidad", "Modalidad");
            tabla1.Columns.Add("SalarioBruto", "Salario Bruto");
            tabla1.Columns.Add("ISSS", "ISSS");
            tabla1.Columns.Add("AFP", "AFP");
            tabla1.Columns.Add("Renta", "Renta");
            tabla1.Columns.Add("TotalDeducciones", "Total Deducciones");
            tabla1.Columns.Add("SueldoLiquido", "Sueldo Líquido");
            CargarHistorialDesdeBD();
        }

        private void CmbModalidad_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbModalidad.SelectedItem == null)
                return;

            OcultarCampos();

            switch (cmbModalidad.SelectedItem.ToString())
            {
                case "Sueldo Base":
                    lblSalarioBase.Visible = txtSalarioBase.Visible = true;
                    break;
                case "Por Horas":
                    lblHoras.Visible = txtHoras.Visible = true;
                    lblPagoHora.Visible = txtPagoHora.Visible = true;
                    break;
                case "Por Comisión":
                    lblVentas.Visible = txtVentas.Visible = true;
                    lblComision.Visible = txtComision.Visible = true;
                    break;
            }
        }

        private void OcultarCampos()
        {
            lblSalarioBase.Visible = txtSalarioBase.Visible = false;
            lblHoras.Visible = txtHoras.Visible = false;
            lblPagoHora.Visible = txtPagoHora.Visible = false;
            lblVentas.Visible = txtVentas.Visible = false;
            lblComision.Visible = txtComision.Visible = false;
        }

        private void BtnCalcular_Click(object sender, EventArgs e)
        {
            try
            {
                double salarioBruto = ObtenerSalarioBruto();
                double isss = Math.Min(salarioBruto * 0.03, 30);
                double afp = salarioBruto * 0.0725;
                double renta = CalcularRenta(salarioBruto - isss - afp);
                double totalDeducciones = isss + afp + renta;
                double sueldoLiquido = salarioBruto - totalDeducciones;

                lblResultado.Text = $"Sueldo líquido: ${sueldoLiquido:F2}";

                if (idSeleccionado == null)
                {
                    GuardarEnBD(
                        cmbTipoPago.SelectedItem.ToString(),
                        cmbModalidad.SelectedItem.ToString(),
                        salarioBruto, isss, afp, renta,
                        totalDeducciones, sueldoLiquido);
                }
                else
                {
                    ActualizarEnBD(idSeleccionado.Value,
                        cmbTipoPago.SelectedItem.ToString(),
                        cmbModalidad.SelectedItem.ToString(),
                        salarioBruto, isss, afp, renta,
                        totalDeducciones, sueldoLiquido);
                    idSeleccionado = null;
                }

                tabla1.Rows.Clear();
                CargarHistorialDesdeBD();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al calcular: " + ex.Message);
            }
        }

        private void btnlimpiar_Click(object sender, EventArgs e)
        {
            cmbTipoPago.SelectedIndex = -1;
            cmbModalidad.SelectedIndex = -1;
            txtSalarioBase.Clear();
            txtHoras.Clear();
            txtPagoHora.Clear();
            txtVentas.Clear();
            txtComision.Clear();
            lblResultado.Text = "";
            OcultarCampos();
            idSeleccionado = null;
        }

        private void btneliminar_Click(object sender, EventArgs e)
        {
            if (tabla1.SelectedRows.Count > 0)
            {
                int id = int.Parse(tabla1.SelectedRows[0].Cells[0].Value.ToString());

                string conexion = "Server=LapPUENTE\\PUENT;Database=SUELDO;User Id=sa;Password=83Dakota77#;";
                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    conn.Open();
                    string query = "DELETE FROM HistorialSueldo WHERE Id=@Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                tabla1.Rows.RemoveAt(tabla1.SelectedRows[0].Index);
            }
            else
            {
                MessageBox.Show("Seleccione una fila para eliminar.");
            }
        }

        private void btnmodificar_Click(object sender, EventArgs e)
        {
            if (tabla1.SelectedRows.Count > 0)
            {
                DataGridViewRow fila = tabla1.SelectedRows[0];

                idSeleccionado = int.Parse(fila.Cells[0].Value.ToString());

                cmbTipoPago.Text = fila.Cells[1].Value.ToString();
                cmbModalidad.Text = fila.Cells[2].Value.ToString();

                txtSalarioBase.Text = fila.Cells[3].Value.ToString();
                txtHoras.Text = "";
                txtPagoHora.Text = "";
                txtVentas.Text = "";
                txtComision.Text = "";
                lblResultado.Text = "";
            }
            else
            {
                MessageBox.Show("Seleccione una fila para modificar.");
            }
        }

        private double ObtenerSalarioBruto()
        {
            string modalidad = cmbModalidad.SelectedItem?.ToString();
            string tipoPago = cmbTipoPago.SelectedItem?.ToString();

            double bruto = 0;

            if (modalidad == "Sueldo Base")
            {
                bruto = double.Parse(txtSalarioBase.Text);
            }
            else if (modalidad == "Por Horas")
            {
                double horas = double.Parse(txtHoras.Text);
                double pagoHora = double.Parse(txtPagoHora.Text);
                bruto = horas * pagoHora;
            }
            else if (modalidad == "Por Comisión")
            {
                double ventas = double.Parse(txtVentas.Text);
                double porcentaje = double.Parse(txtComision.Text);
                bruto = ventas * (porcentaje / 100);
            }

            if (tipoPago == "Quincenal")
            {
                bruto *= 2;
            }

            return bruto;
        }

        private double CalcularRenta(double salario)
        {
            if (salario <= 472)
                return 0;
            else if (salario <= 895.24)
                return (salario - 472) * 0.1 + 17.67;
            else if (salario <= 2038.10)
                return (salario - 895.24) * 0.2 + 60;
            else
                return (salario - 2038.10) * 0.3 + 288.57;
        }

        private void GuardarEnBD(string tipoPago, string modalidad, double bruto, double isss, double afp, double renta, double totalDeducciones, double liquido)
        {
            string conexion = "Server=LapPUENTE\\PUENT;Database=SUELDO;User Id=sa;Password=83Dakota77#;";

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                string query = "INSERT INTO HistorialSueldo (TipoPago, Modalidad, SalarioBruto, ISSS, AFP, Renta, TotalDeducciones, SueldoLiquido) " +
                               "VALUES (@TipoPago, @Modalidad, @Bruto, @ISSS, @AFP, @Renta, @TotalDeducciones, @Liquido)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TipoPago", tipoPago);
                    cmd.Parameters.AddWithValue("@Modalidad", modalidad);
                    cmd.Parameters.AddWithValue("@Bruto", bruto);
                    cmd.Parameters.AddWithValue("@ISSS", isss);
                    cmd.Parameters.AddWithValue("@AFP", afp);
                    cmd.Parameters.AddWithValue("@Renta", renta);
                    cmd.Parameters.AddWithValue("@TotalDeducciones", totalDeducciones);
                    cmd.Parameters.AddWithValue("@Liquido", liquido);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ActualizarEnBD(int id, string tipoPago, string modalidad, double bruto, double isss, double afp, double renta, double totalDeducciones, double liquido)
        {
            string conexion = "Server=LapPUENTE\\PUENT;Database=SUELDO;User Id=sa;Password=83Dakota77#;";

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                string query = "UPDATE HistorialSueldo SET TipoPago=@TipoPago, Modalidad=@Modalidad, SalarioBruto=@Bruto, ISSS=@ISSS, AFP=@AFP, Renta=@Renta, TotalDeducciones=@TotalDeducciones, SueldoLiquido=@Liquido WHERE Id=@Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@TipoPago", tipoPago);
                    cmd.Parameters.AddWithValue("@Modalidad", modalidad);
                    cmd.Parameters.AddWithValue("@Bruto", bruto);
                    cmd.Parameters.AddWithValue("@ISSS", isss);
                    cmd.Parameters.AddWithValue("@AFP", afp);
                    cmd.Parameters.AddWithValue("@Renta", renta);
                    cmd.Parameters.AddWithValue("@TotalDeducciones", totalDeducciones);
                    cmd.Parameters.AddWithValue("@Liquido", liquido);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void CargarHistorialDesdeBD()
        {
            string conexion = "Server=LapPUENTE\\PUENT;Database=SUELDO;User Id=sa;Password=83Dakota77#;";

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                string query = "SELECT Id, TipoPago, Modalidad, SalarioBruto, ISSS, AFP, Renta, TotalDeducciones, SueldoLiquido FROM HistorialSueldo ORDER BY Id DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tabla1.Rows.Add(
                            reader["Id"].ToString(),
                            reader["TipoPago"].ToString(),
                            reader["Modalidad"].ToString(),
                            Convert.ToDouble(reader["SalarioBruto"]).ToString("F2"),
                            Convert.ToDouble(reader["ISSS"]).ToString("F2"),
                            Convert.ToDouble(reader["AFP"]).ToString("F2"),
                            Convert.ToDouble(reader["Renta"]).ToString("F2"),
                            Convert.ToDouble(reader["TotalDeducciones"]).ToString("F2"),
                            Convert.ToDouble(reader["SueldoLiquido"]).ToString("F2")
                        );
                    }
                }
            }
        }
    }
}
