using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using Crime_analysis.Models;
using Crime_analysis.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Crime_analysis
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();
        private DispatcherTimer _clock;

        // ── Chart series bindings ─────────────
        public ObservableCollection<ISeries> BarSeries { get; set; } = new();
        public ObservableCollection<ISeries> PieSeries { get; set; } = new();
        public ObservableCollection<ISeries> LineSeries { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Show logged in user info
            txtUserName.Text = AuthService.CurrentUser?.FullName ?? "User";
            txtUserRole.Text = AuthService.CurrentUser?.Role ?? "User";

            // Start clock
            StartClock();

            // Load dashboard
            LoadDashboard();
        }

        // ═════════════════════════════════════════
        //  LOAD DASHBOARD
        // ═════════════════════════════════════════
        private void LoadDashboard()
        {
            try
            {
                string city = GetCombo(CityFilter, "All");
                string type = GetCombo(TypeFilter, "All");
                string sev = GetCombo(SeverityFilter, "All");
                string from = FromDate.SelectedDate?.ToString("yyyy-MM-dd") ?? "2024-01-01";
                string to = ToDate.SelectedDate?.ToString("yyyy-MM-dd") ?? "2024-12-31";

                // ── Stat Cards ────────────────
                int total = _db.GetTotalCrimes(city, from, to);
                int solved = _db.GetSolvedCrimes(city, from, to);
                int suspects = _db.GetTotalSuspects();
                int officers = _db.GetTotalOfficers();

                txtTotalCrimes.Text = total.ToString();
                txtSolved.Text = solved.ToString();
                txtSuspects.Text = suspects.ToString();
                txtOfficers.Text = officers.ToString();

                // ── Alert Banner ──────────────
                var highRisk = _db.GetHighRiskAreas();
                if (highRisk.Rows.Count > 0)
                {
                    var names = new List<string>();
                    foreach (DataRow row in highRisk.Rows)
                        names.Add(row["AreaName"].ToString()!);
                    AlertText.Text = $"{highRisk.Rows.Count} high-risk area(s): " +
                                     string.Join(", ", names);
                }
                else
                {
                    AlertText.Text = "✓ No high-risk areas detected";
                }

                // ── Charts ────────────────────
                LoadBarChart(city, from, to);
                LoadPieChart(city, from, to);
                LoadLineChart(city, from, to);

                // ── Alerts Table ──────────────
                LoadAlertsTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dashboard:\n" + ex.Message,
                                "Error", MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        // ─────────────────────────────────────────
        //  BAR CHART — Crimes by Area
        // ─────────────────────────────────────────
        private void LoadBarChart(string city, string from, string to)
        {
            var dt = _db.GetCrimesByArea(city, from, to);
            var values = new List<double>();
            var labels = new List<string>();

            foreach (DataRow row in dt.Rows)
            {
                labels.Add(row["AreaName"].ToString()!);
                values.Add(Convert.ToDouble(row["CrimeCount"]));
            }

            BarSeries.Clear();
            BarSeries.Add(new ColumnSeries<double>
            {
                Values = values,
                Fill = new SolidColorPaint(SKColor.Parse("#E74C3C")),
                Stroke = null,
                MaxBarWidth = 40
            });

            BarChart.XAxes = new[]
            {
                new Axis
                {
                    Labels     = labels,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                    TextSize   = 11
                }
            };
            BarChart.YAxes = new[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                    TextSize   = 11
                }
            };
        }

        // ─────────────────────────────────────────
        //  PIE CHART — Crime Types
        // ─────────────────────────────────────────
        private void LoadPieChart(string city, string from, string to)
        {
            var dt = _db.GetCrimesByType(city, from, to);
            var colors = new[]
            {
                "#E74C3C","#3498DB","#2ECC71",
                "#F39C12","#9B59B6","#1ABC9C"
            };

            PieSeries.Clear();
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                PieSeries.Add(new PieSeries<double>
                {
                    Values = new[] { Convert.ToDouble(row["Total"]) },
                    Name = row["TypeName"].ToString(),
                    Fill = new SolidColorPaint(
                                 SKColor.Parse(colors[i % colors.Length]))
                });
                i++;
            }
        }

        // ─────────────────────────────────────────
        //  LINE CHART — Monthly Trend
        // ─────────────────────────────────────────
        private void LoadLineChart(string city, string from, string to)
        {
            var dt = _db.GetMonthlyTrend(city, from, to);
            var values = new List<double>();
            var labels = new List<string>();

            foreach (DataRow row in dt.Rows)
            {
                labels.Add(row["Month"].ToString()!);
                values.Add(Convert.ToDouble(row["Total"]));
            }

            LineSeries.Clear();
            LineSeries.Add(new LineSeries<double>
            {
                Values = values,
                Fill = new SolidColorPaint(SKColor.Parse("#3498DB33")),
                Stroke = new SolidColorPaint(SKColor.Parse("#3498DB"))
                { StrokeThickness = 2 },
                GeometryFill = new SolidColorPaint(SKColor.Parse("#3498DB")),
                GeometrySize = 6
            });

            LineChart.XAxes = new[]
            {
                new Axis
                {
                    Labels      = labels,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                    TextSize    = 10
                }
            };
            LineChart.YAxes = new[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                    TextSize    = 10
                }
            };
        }

        // ─────────────────────────────────────────
        //  ALERTS TABLE
        // ─────────────────────────────────────────
        private void LoadAlertsTable()
        {
            var dt = _db.GetHighRiskAreas();
            var alerts = new List<AreaAlert>();

            foreach (DataRow row in dt.Rows)
            {
                alerts.Add(new AreaAlert
                {
                    AreaName = row["AreaName"].ToString(),
                    City = row["City"].ToString(),
                    CrimeCount = Convert.ToInt32(row["CrimeCount"]),
                    TopCrime = row["TopCrime"].ToString(),
                    Severity = row["Severity"].ToString(),
                    IsHighRisk = true
                });
            }

            AlertsGrid.ItemsSource = alerts;
        }

        // ═════════════════════════════════════════
        //  NAV BUTTON CLICKS
        // ═════════════════════════════════════════
        private void HideAllPanels()
        {
            DashboardPanel.Visibility = Visibility.Collapsed;
            CrimesPanel.Visibility = Visibility.Collapsed;
            SuspectsPanel.Visibility = Visibility.Collapsed;
            OfficersPanel.Visibility = Visibility.Collapsed;
            AreasPanel.Visibility = Visibility.Collapsed;
            ReportsPanel.Visibility = Visibility.Collapsed;
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            DashboardPanel.Visibility = Visibility.Visible;
            txtPageTitle.Text = "Dashboard";
            LoadDashboard();
        }

        private void btnCrimes_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            CrimesPanel.Visibility = Visibility.Visible;
            txtPageTitle.Text = "Crimes";
        }

        private void btnSuspects_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            SuspectsPanel.Visibility = Visibility.Visible;
            txtPageTitle.Text = "Suspects";
        }

        private void btnOfficers_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            OfficersPanel.Visibility = Visibility.Visible;
            txtPageTitle.Text = "Officers";
        }

        private void btnAreas_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            AreasPanel.Visibility = Visibility.Visible;
            txtPageTitle.Text = "Areas";
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            ReportsPanel.Visibility = Visibility.Visible;
            txtPageTitle.Text = "Reports";
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            AuthService.Logout();
            var login = new Views.LoginWindow();
            login.Show();
            this.Close();
        }

        // ═════════════════════════════════════════
        //  FILTER + EXPORT
        // ═════════════════════════════════════════
        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboard();
        }

        private void ExportPdf_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PDF export coming soon!",
                            "Export", MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        // ═════════════════════════════════════════
        //  HELPERS
        // ═════════════════════════════════════════
        private string GetCombo(System.Windows.Controls.ComboBox combo,
                                string defaultVal)
        {
            return (combo.SelectedItem as
                    System.Windows.Controls.ComboBoxItem)
                       ?.Content?.ToString() ?? defaultVal;
        }

        private void StartClock()
        {
            _clock = new DispatcherTimer
            { Interval = TimeSpan.FromSeconds(1) };
            _clock.Tick += (s, e) =>
                txtDateTime.Text =
                    DateTime.Now.ToString("ddd dd MMM yyyy   HH:mm:ss");
            _clock.Start();
        }
    }
}