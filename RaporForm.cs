using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ModbusLibrary
{
    public class RaporForm : Form
    {
        private DataGridView dgvAlarmlar;
        private DatabaseManager db;
        private string mevcutOturum;
        private TextBox txtArama;
        private Panel pnlUst;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblDurum;

        public RaporForm(string oturumId)
        {
            db = new DatabaseManager();
            mevcutOturum = oturumId;

            this.Text = "📊 Detaylı Olay ve Arıza Takip Paneli";
            this.Size = new Size(950, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);

            // --- 1. ÜST PANEL (Arama, Yenile, Excel Butonları) ---
            pnlUst = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.WhiteSmoke };

            Label lblAra = new Label { Text = "Loglarda Ara:", Left = 15, Top = 18, AutoSize = true };
            txtArama = new TextBox { Left = 100, Top = 15, Width = 200 };
            txtArama.TextChanged += (s, e) => Filtrele();

            Button btnYenile = new Button { Text = "🔄 Yenile", Left = 320, Top = 14, Width = 90, BackColor = Color.White, Cursor = Cursors.Hand };
            btnYenile.Click += (s, e) => VerileriYukle();

            Button btnExcel = new Button { Text = "📗 Excel'e Aktar", Left = 420, Top = 14, Width = 110, BackColor = Color.LightGreen, Cursor = Cursors.Hand };
            btnExcel.Click += BtnExcel_Click;

            pnlUst.Controls.AddRange(new Control[] { lblAra, txtArama, btnYenile, btnExcel });

            // --- 2. ALT BİLGİ ÇUBUĞU (Status Strip) ---
            statusStrip = new StatusStrip { BackColor = Color.WhiteSmoke };
            lblDurum = new ToolStripStatusLabel { Text = "Veriler yükleniyor...", ForeColor = Color.DarkBlue };
            statusStrip.Items.Add(lblDurum);

            // --- 3. TABLO (DataGridView) ---
            dgvAlarmlar = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(245, 245, 245) }
            };

            dgvAlarmlar.DataBindingComplete += DgvAlarmlar_DataBindingComplete;

            // Kontrolleri Forma Ekle (Sıralama önemlidir: Önce alt, sonra üst, en son dolgu)
            this.Controls.Add(dgvAlarmlar);
            this.Controls.Add(pnlUst);
            this.Controls.Add(statusStrip);

            this.Load += RaporForm_Load;
        }

        private void RaporForm_Load(object sender, EventArgs e)
        {
            VerileriYukle();
        }

        // Merkezi Veri Yükleme Metodu 
        private void VerileriYukle()
        {
            DataTable dt = db.GetirOlayLoglari(mevcutOturum);
            dgvAlarmlar.DataSource = dt;

            if (dgvAlarmlar.Columns.Count > 0)
            {
                dgvAlarmlar.Columns["Tarih"].HeaderText = "Oluşma Zamanı";
                dgvAlarmlar.Columns["Mesaj"].HeaderText = "Olay Açıklaması";
                dgvAlarmlar.Columns["Tip"].HeaderText = "Kategori";
                dgvAlarmlar.Columns["CalismaOturumu"].HeaderText = "Oturum ID";

                dgvAlarmlar.Columns["Tarih"].Width = 140;
                dgvAlarmlar.Columns["Tip"].Width = 80;
            }

            Filtrele(); // arama kutusu doluysa yenileyince filtreyi koru
        }

        private void Filtrele()
        {
            if (dgvAlarmlar.DataSource is DataTable dt)
            {
                dt.DefaultView.RowFilter = string.Format("Mesaj LIKE '%{0}%' OR CalismaOturumu LIKE '%{0}%'", txtArama.Text);
                lblDurum.Text = $"Toplam Kayıt: {dt.DefaultView.Count}";
            }
        }

        // Satır Renklendirme Mantığı
        private void DgvAlarmlar_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvAlarmlar.Rows)
            {
                string mesaj = row.Cells["Mesaj"].Value.ToString();

                if (mesaj.Contains("BAŞLADI"))
                {
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    row.DefaultCellStyle.SelectionBackColor = Color.Red;
                }
                else if (mesaj.Contains("GİDERİLDİ"))
                {
                    row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                    row.DefaultCellStyle.SelectionBackColor = Color.Green;
                }
                else if (mesaj.Contains("BAĞLANTI") || mesaj.Contains("KOPTU"))
                {
                    row.DefaultCellStyle.BackColor = Color.LemonChiffon;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    row.DefaultCellStyle.SelectionBackColor = Color.Orange;
                }
            }
        }

        // Excel Aktarma İşlemi
        private void BtnExcel_Click(object sender, EventArgs e)
        {
            if (dgvAlarmlar.Rows.Count == 0)
            {
                MessageBox.Show("Dışa aktarılacak kayıt bulunamadı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel CSV Dosyası (*.csv)|*.csv",
                FileName = $"Arıza_Raporu_{DateTime.Now:yyyyMMdd_HHmm}.csv",
                Title = "Raporu Kaydet"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    // 1. Sütun Başlıklarını Yazdır (Türkçe Excel için noktalı virgül ';' kullanıyoruz)
                    string[] columnNames = new string[dgvAlarmlar.Columns.Count];
                    for (int i = 0; i < dgvAlarmlar.Columns.Count; i++)
                    {
                        columnNames[i] = dgvAlarmlar.Columns[i].HeaderText;
                    }
                    sb.AppendLine(string.Join(";", columnNames));

                    // 2. Veri Satırlarını Yazdır
                    foreach (DataGridViewRow row in dgvAlarmlar.Rows)
                    {
                        string[] cells = new string[dgvAlarmlar.Columns.Count];
                        for (int i = 0; i < dgvAlarmlar.Columns.Count; i++)
                        {
                            cells[i] = row.Cells[i].Value?.ToString().Replace(";", ",") ?? ""; // İçindeki olası noktalı virgülleri virgüle çevir
                        }
                        sb.AppendLine(string.Join(";", cells));
                    }

                    // 3. Dosyayı Türkçe karakter desteğiyle (UTF8) kaydet
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Rapor başarıyla Excel (CSV) formatında kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dosya kaydedilirken bir hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // RaporForm
            // 
            this.ClientSize = new System.Drawing.Size(316, 293);
            this.Name = "RaporForm";
            this.ResumeLayout(false);

        }
    }
}